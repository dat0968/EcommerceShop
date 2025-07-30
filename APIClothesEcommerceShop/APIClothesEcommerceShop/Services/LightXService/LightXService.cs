
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace APIClothesEcommerceShop.Services.LightXService
{
    public class LightXService(HttpClient httpClient, IConfiguration configuration, ILogger<LightXService> logger) : ILightXService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<LightXService> _logger = logger;
        private readonly string _apiKey = configuration["LightX:ApiKey"] ?? string.Empty;

        public async Task<string> ProcessTryOnAsync(string modelImageUrl, List<string> productImagesUrl)
        {
            try
            {
                if (productImagesUrl == null || productImagesUrl.Count == 0)
                {
                    throw new ArgumentException("No product images provided for LightX processing.");
                }

                // Get model image bytes and content type
                var (modelImageBytes, modelContentType) = await GetImageBytesAndContentType(modelImageUrl);

                // Get product image bytes and content type (using the first one for simplicity)
                var (productImageBytes, productContentType) = await GetImageBytesAndContentType(productImagesUrl[0]);

                // Step 1 & 2: Upload model image
                var modelUploadData = await GetLightXUploadUrl(modelImageBytes.Length, modelContentType);
                await UploadToLightX(modelUploadData.uploadImage, modelImageBytes, modelContentType);
                var uploadedModelImageUrl = modelUploadData.imageUrl;

                // Step 1 & 2: Upload product image
                var productUploadData = await GetLightXUploadUrl(productImageBytes.Length, productContentType);
                await UploadToLightX(productUploadData.uploadImage, productImageBytes, productContentType);
                var uploadedStyleImageUrl = productUploadData.imageUrl;

                // Step 3: Start the job
                var orderId = await StartLightXJob(uploadedModelImageUrl, uploadedStyleImageUrl);

                // Step 4: Poll for the result
                var resultUrl = await PollLightXJob(orderId);
                return resultUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing with LightX API.");
                throw new Exception($"LightX API processing failed: {ex.Message}", ex);
            }
        }

        private async Task<(byte[] Bytes, string ContentType)> GetImageBytesAndContentType(string imageUrlOrDataUrl)
        {
            if (imageUrlOrDataUrl.StartsWith("data:"))
            {
                var parts = imageUrlOrDataUrl.Split(',');
                var contentType = parts[0].Split(';')[0].Split(':')[1];
                var bytes = Convert.FromBase64String(parts[1]);
                return (bytes, contentType);
            }
            else
            {
                var response = await _httpClient.GetAsync(imageUrlOrDataUrl);
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg"; // Default to jpeg if not found
                return (bytes, contentType);
            }
        }

        private async Task<dynamic> GetLightXUploadUrl(int size, string contentType)
        {
            var response = await _httpClient.PostAsync("https://api.lightxeditor.com/external/api/v2/uploadImageUrl", new StringContent(
                JsonSerializer.Serialize(new
                {
                    uploadType = "imageUrl",
                    size = size,
                    contentType = contentType
                }), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (data.GetProperty("statusCode").GetInt32() != 2000)
            {
                _logger.LogError("LightX getUploadUrl failed. Full response: {Response}", data.ToString());
                throw new Exception($"Failed to get LightX upload URL: {data.GetProperty("message").GetString()}");
            }
            return data.GetProperty("body");
        }

        private async Task UploadToLightX(string uploadUrl, byte[] imageBytes, string contentType)
        {
            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var response = await _httpClient.PutAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<string> StartLightXJob(string imageUrl, string styleImageUrl)
        {
            var response = await _httpClient.PostAsync("https://api.lightxeditor.com/external/api/v2/aivirtualtryon", new StringContent(
                JsonSerializer.Serialize(new { imageUrl, styleImageUrl }), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (data.GetProperty("statusCode").GetInt32() != 2000)
            {
                _logger.LogError("LightX startJob failed. Full response: {Response}", data.ToString());
                throw new Exception($"Failed to start LightX job: {data.GetProperty("message").GetString()}");
            }
            return data.GetProperty("body").GetProperty("orderId").GetString();
        }

        private async Task<string> PollLightXJob(string orderId)
        {
            const int maxRetries = 10; // Increased retries for robustness
            const int delay = 5000; // 5 seconds

            for (int i = 0; i < maxRetries; i++)
            {
                await Task.Delay(delay);
                var response = await _httpClient.PostAsync("https://api.lightxeditor.com/external/api/v2/order-status", new StringContent(
                    JsonSerializer.Serialize(new { orderId }), Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadFromJsonAsync<JsonElement>();
                var status = data.GetProperty("body").GetProperty("status").GetString();

                if (status == "active")
                {
                    return data.GetProperty("body").GetProperty("output").GetString();
                }
                if (status == "failed")
                {
                    _logger.LogError("LightX job failed. Full response: {Response}", data.ToString());
                    throw new Exception("LightX job failed.");
                }
            }
            throw new TimeoutException("LightX job timed out.");
        }
    }
}
