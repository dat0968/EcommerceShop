using APIClothesEcommerceShop.DTO.TryOn;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Services.CloudinaryService;
using APIClothesEcommerceShop.Services.LightXService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TryOnController : ControllerBase
    {
        private readonly IGeminiAIService _geminiAIService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILightXService _lightXService;

        public TryOnController(IGeminiAIService geminiAIService, ICloudinaryService cloudinaryService, ILightXService lightXService)
        {
            _geminiAIService = geminiAIService;
            _cloudinaryService = cloudinaryService;
            _lightXService = lightXService;
        }

        [HttpPost("TryOn")]
        public async Task<IActionResult> TryOn([FromBody] TryOnRequest request)
        {
            try
            {
                // 1. Process and Upload image to Cloudinary
                var tryOnImageResultUrl = await ProcessAndUploadImage(request.ModelImage, request.ProductImages);

                // 2. Analyze with Gemini
                var productsDataAsObjectList = request.ProductsData.ConvertAll(p => (object)p);
                var analysisResult = await _geminiAIService.AnalyzeTryOnImageAsync(tryOnImageResultUrl, productsDataAsObjectList);

                if (!analysisResult.Success)
                {
                    return StatusCode(500, new { message = analysisResult.Message });
                }

                dynamic resultData = analysisResult.Data;

                var response = new TryOnResponse
                {
                    Image = tryOnImageResultUrl,
                    Score = resultData.aesthetic_score,
                    Style = resultData.style,
                    GenderSuitability = resultData.gender_suitability
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý hình ảnh người mẫu và sản phẩm bằng LightX, sau đó tải kết quả lên Cloudinary.
        /// </summary>
        /// <param name="request">Đối tượng chứa chuỗi Base64 của hình ảnh người mẫu và danh sách chuỗi Base64 của hình ảnh sản phẩm.</param>
        /// <returns>URL của hình ảnh đã được xử lý và tải lên Cloudinary.</returns>
        /// <remarks>
        /// Gửi yêu cầu với định dạng JSON:
        ///
        ///     POST /api/TryOn/ProcessAndUpload
        ///     {
        ///        "modelImageBase64": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD...",
        ///        "productImagesBase64": [
        ///           "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD..."
        ///        ]
        ///     }
        ///
        /// Lưu ý: `modelImageBase64` và `productImagesBase64` phải là chuỗi Base64 đầy đủ có tiền tố `data:image/jpeg;base64,`.
        /// LightX API hiện tại chỉ xử lý ảnh sản phẩm đầu tiên trong danh sách `productImagesBase64`.
        /// </remarks>
        [HttpPost("ProcessAndUpload")]
        public async Task<IActionResult> ProcessAndUpload([FromBody] ProcessImageRequest request)
        {
            try
            {
                var imageUrl = await ProcessAndUploadImage(request.ModelImageBase64, request.ProductImagesBase64);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



        private async Task<string> ProcessAndUploadImage(string modelDataUrl, List<string> productDataUrls)
        {
            // 1. Process image with LightX
            var lightXResultUrl = await _lightXService.ProcessTryOnAsync(modelDataUrl, productDataUrls);

            // 2. Upload the resulting image from LightX to Cloudinary
            var cloudinaryUrl = await _cloudinaryService.UploadImageFromUrlAsync(lightXResultUrl, "try-on-result");
            return cloudinaryUrl;
        }

        /// <summary>
        /// Tải lên một hình ảnh trực tiếp lên Cloudinary và trả về URL an toàn của hình ảnh đó.
        /// </summary>
        /// <param name="file">File hình ảnh cần tải lên (dạng IFormFile?).</param>
        /// <returns>URL an toàn của hình ảnh trên Cloudinary.</returns>
        /// <remarks>
        /// Gửi yêu cầu với dạng multipart/form-data:
        ///
        ///     POST /api/TryOn/UploadImageToCloudinary
        ///     Content-Type: multipart/form-data
        ///
        ///     --boundary
        ///     Content-Disposition: form-data; name="file"; filename="your_image.jpg"
        ///     Content-Type: image/jpeg
        ///
        ///     [Binary content of your image file]
        ///     --boundary--
        ///
        /// Sử dụng công cụ như Postman hoặc Swagger UI để dễ dàng kiểm thử.
        /// </remarks>
        [HttpPost("UploadImageToCloudinary")]
        public async Task<IActionResult> UploadImageToCloudinary(IFormFile? file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không được rỗng." });
                }

                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "direct-upload");
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi tải ảnh lên Cloudinary: {ex.Message}" });
            }
        }

        /// <summary>
        /// Phân tích một hình ảnh bằng Google Gemini AI và trả về các đặc điểm thẩm mỹ, phong cách, v.v.
        /// Hình ảnh có thể được cung cấp dưới dạng file tải lên hoặc URL.
        /// </summary>
        /// <param name="file">File hình ảnh cần phân tích (dạng IFormFile?).</param>
        /// <param name="imageUrl">URL của hình ảnh cần phân tích (dạng string?).</param>
        /// <returns>Kết quả phân tích hình ảnh từ Gemini AI.</returns>
        /// <remarks>
        /// Gửi yêu cầu với dạng multipart/form-data (cho file) hoặc application/json (cho URL):
        ///
        /// Cho file:
        ///     POST /api/TryOn/AnalyzeImageWithGemini
        ///     Content-Type: multipart/form-data
        ///
        ///     --boundary
        ///     Content-Disposition: form-data; name="file"; filename="image_to_analyze.jpg"
        ///     Content-Type: image/jpeg
        ///
        ///     [Binary content of your image file]
        ///     --boundary--
        ///
        /// Cho URL:
        ///     POST /api/TryOn/AnalyzeImageWithGemini?imageUrl=https://example.com/your_image.jpg
        ///
        /// Hoặc trong body JSON (nếu muốn gửi kèm các tham số khác, nhưng ở đây chỉ có imageUrl):
        ///     POST /api/TryOn/AnalyzeImageWithGemini
        ///     Content-Type: application/json
        ///     {
        ///         "imageUrl": "https://example.com/your_image.jpg"
        ///     }
        ///
        /// Nếu cung cấp file, ảnh sẽ được tải lên Cloudinary trước khi gửi đến Gemini để phân tích.
        /// Nếu cung cấp URL, ảnh sẽ được tải về từ URL đó và chuyển đổi sang Base64 để gửi đến Gemini.
        /// Chỉ được cung cấp một trong hai: file hoặc URL.
        /// </remarks>
        [HttpPost("AnalyzeImageWithGemini")]
        public async Task<IActionResult> AnalyzeImageWithGemini(IFormFile? file, [FromQuery] string? imageUrl)
        {
            try
            {
                string? imageBase64 = null;
                string? finalImageUrl = null;

                if (file != null && imageUrl != null)
                {
                    return BadRequest(new { message = "Chỉ được cung cấp một trong hai: file hình ảnh hoặc URL hình ảnh." });
                }
                else if (file != null)
                {
                    // Upload file to Cloudinary to get a public URL
                    finalImageUrl = await _cloudinaryService.UploadImageAsync(file, "gemini-analysis");

                    // Convert IFormFile to Base64 string
                    using (var ms = new MemoryStream())
                    {
                        await file.CopyToAsync(ms);
                        byte[] fileBytes = ms.ToArray();
                        imageBase64 = Convert.ToBase64String(fileBytes);
                    }
                }
                else if (imageUrl != null)
                {
                    finalImageUrl = imageUrl;
                    // Fetch image from URL and convert to Base64 string
                    using (var httpClient = new HttpClient())
                    {
                        byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        imageBase64 = Convert.ToBase64String(imageBytes);
                    }
                }
                else
                {
                    return BadRequest(new { message = "Vui lòng cung cấp file hình ảnh hoặc URL hình ảnh." });
                }

                // Call GeminiAIService to analyze the image
                var analysisResult = await _geminiAIService.AnalyzeTryOnImageAsync(imageBase64, new List<object>());

                if (!analysisResult.Success)
                {
                    return StatusCode(500, new { message = analysisResult.Message });
                }

                return Ok(analysisResult.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi phân tích ảnh với Gemini AI: {ex.Message}" });
            }
        }
    }
}
