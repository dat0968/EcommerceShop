using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using APIClothesEcommerceShop.DTO.TryOn;

namespace APIClothesEcommerceShop.Services.LightXService
{
    public class LightXApiService : ILightXApiService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public LightXApiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        private string GetClothingCategory(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return "unknown";
            var name = categoryName.ToLower();
            if (name.Contains("áo") || name.Contains("top"))
            {
                return "top";
            }
            if (name.Contains("quần") || name.Contains("bottom") || name.Contains("pants") || name.Contains("jeans"))
            {
                return "bottom";
            }
            return "unknown";
        }

        private async Task<LightXUploadUrlResponse> GetLightXUploadUrl(string apiKey, long size)
        {
            var client = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                uploadType = "imageUrl",
                size = size,
                contentType = "image/jpeg"
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await client.PostAsync("https://api.lightxeditor.com/external/api/v2/uploadImageUrl", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<LightXApiResponse<LightXUploadUrlResponse>>(responseString);

            if (data.StatusCode != 2000) {
                throw new Exception($"Failed to get LightX upload URL: {data.Message}");
            }
            return data.Body;
        }

        private async Task UploadToLightX(string uploadUrl, byte[] imageBytes)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var response = await client.PutAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<string> StartLightXJob(string apiKey, string imageUrl, string topImageUrl, string bottomImageUrl)
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new Dictionary<string, object>
            {
                { "imageUrl", imageUrl },
                { "category", "fashion" }
            };

            if (!string.IsNullOrEmpty(topImageUrl))
            {
                payload.Add("styleImageUrl", topImageUrl);
                payload.Add("clothCategory", "top");
            }

            if (!string.IsNullOrEmpty(bottomImageUrl))
            {
                if (!string.IsNullOrEmpty(topImageUrl))
                {
                    payload.Add("stickerImageUrl", bottomImageUrl);
                    payload.Add("stickerCategory", "bottom");
                }
                else
                {
                    payload.Add("styleImageUrl", bottomImageUrl);
                    payload.Add("clothCategory", "bottom");
                }
            }

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var response = await client.PostAsync("https://api.lightxeditor.com/external/api/v2/aivirtualtryon", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<LightXApiResponse<LightXJobResponse>>(responseString);

            if (data.StatusCode != 2000) {
                throw new Exception($"Failed to start LightX job: {data.Message}");
            }
            return data.Body.OrderId;
        }

        private async Task<string> PollLightXJob(string apiKey, string orderId)
        {
            var client = _httpClientFactory.CreateClient();
            var maxRetries = 10;
            var delay = 5000; // 5 seconds

            for (int i = 0; i < maxRetries; i++)
            {
                await Task.Delay(delay);
                var requestBody = new { orderId };
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var response = await client.PostAsync("https://api.lightxeditor.com/external/api/v2/order-status", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<LightXApiResponse<LightXOrderStatusResponse>>(responseString);

                if (data.StatusCode != 2000 || data.Body == null) {
                    throw new Exception($"LightX job status check failed: {data.Message ?? "No response body"}");
                }

                if (data.Body.Status == "active")
                {
                    return data.Body.Output;
                }
                if (data.Body.Status == "failed")
                {
                    throw new Exception("LightX job failed.");
                }
            }
            throw new Exception("LightX job timed out after several retries.");
        }

        public async Task<string> ProcessWithLightX(string apiKey, string modelImageUrl, List<ProductData> products)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Step 1: Download model image bytes
                byte[] modelImageBytes = await client.GetByteArrayAsync(modelImageUrl);

                // Step 2: Upload model image to LightX
                var modelUploadData = await GetLightXUploadUrl(apiKey, modelImageBytes.Length);
                await UploadToLightX(modelUploadData.UploadImage, modelImageBytes);
                var finalModelImageUrl = modelUploadData.ImageUrl;

                // Step 3: Download and upload all product images and categorize them
                var productUrls = new List<(string url, string category)>();
                foreach (var product in products)
                {
                    string imgUrl = product.Image ?? (product.Products != null && product.Products.Count > 0 ? product.Products[0]?.Image : null);
                    if (!string.IsNullOrEmpty(imgUrl))
                    {
                        byte[] productImageBytes = await client.GetByteArrayAsync(imgUrl);
                        var productUploadData = await GetLightXUploadUrl(apiKey, productImageBytes.Length);
                        await UploadToLightX(productUploadData.UploadImage, productImageBytes);
                        productUrls.Add((productUploadData.ImageUrl, GetClothingCategory(product.Name)));
                    }
                }

                var topImageUrl = productUrls.Find(p => p.category == "top").url;
                var bottomImageUrl = productUrls.Find(p => p.category == "bottom").url;

                if (string.IsNullOrEmpty(topImageUrl) && string.IsNullOrEmpty(bottomImageUrl))
                {
                    throw new Exception("Không có sản phẩm nào phù hợp (áo hoặc quần) để thử đồ.");
                }

                // Step 4: Start the job with appropriate parameters
                var orderId = await StartLightXJob(apiKey, finalModelImageUrl, topImageUrl, bottomImageUrl);

                // Step 5: Poll for the result
                var resultUrl = await PollLightXJob(apiKey, orderId);
                return resultUrl;

            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error processing with LightX API: {ex.Message}");
                throw;
            }
        }
    }

    // DTOs for LightX API responses
    public class LightXApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Body { get; set; }
    }

    public class LightXUploadUrlResponse
    {
        public string UploadImage { get; set; }
        public string ImageUrl { get; set; }
    }

    public class LightXJobResponse
    {
        public string OrderId { get; set; }
    }

    public class LightXOrderStatusResponse
    {
        public string Status { get; set; }
        public string Output { get; set; }
    }
}
