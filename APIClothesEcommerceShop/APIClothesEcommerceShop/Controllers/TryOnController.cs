using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.TryOn;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Services.CloudinaryService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TryOnController : ControllerBase
    {
        private readonly IGeminiAIService _geminiAIService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TryOnController(IGeminiAIService geminiAIService, ICloudinaryService cloudinaryService, IWebHostEnvironment webHostEnvironment)
        {
            _geminiAIService = geminiAIService;
            _cloudinaryService = cloudinaryService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Tải lên một hình ảnh người mẫu (do người dùng tự chọn) lên Cloudinary.
        /// </summary>
        /// <param name="file">File hình ảnh cần tải lên (dạng IFormFile).</param>
        /// <returns>URL công khai của hình ảnh trên Cloudinary.</returns>
        /// <remarks>
        /// Gửi yêu cầu với dạng multipart/form-data.
        /// </remarks>
        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            ResponseAPI<UploadImageResponse> res = new();
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File không được rỗng." });
            }

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(file, "user-models");
                res.SetSuccessResponse(data: new UploadImageResponse { ImageUrl = imageUrl });
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi tải ảnh lên: {ex.Message}" });
            }
        }

        /// <summary>
        /// Phân tích hình ảnh đã được xử lý bởi LightX bằng Google Gemini AI.
        /// </summary>
        /// <param name="request">Đối tượng chứa URL của hình ảnh kết quả và dữ liệu sản phẩm liên quan.</param>
        /// <returns>Kết quả phân tích từ Gemini AI.</returns>
        [HttpPost("AnalyzeImage")]
        public async Task<IActionResult> AnalyzeImage([FromBody] AnalyzeRequest request)
        {
            ResponseAPI<object> response = new();
            if (string.IsNullOrEmpty(request.ResultImageUrl))
            {
                return BadRequest(new { message = "URL hình ảnh kết quả không được rỗng." });
            }

            try
            {
                // Tải ảnh từ URL về dạng Base64 để gửi cho Gemini
                string imageBase64;
                using (var httpClient = new HttpClient())
                {
                    byte[] imageBytes = await httpClient.GetByteArrayAsync(request.ResultImageUrl);
                    imageBase64 = Convert.ToBase64String(imageBytes);
                }

                var analysisResult = await _geminiAIService.AnalyzeTryOnImageAsync(imageBase64, request.ProductsData);

                if (!analysisResult.Success)
                {
                    return StatusCode(500, new { message = analysisResult.Message });
                }
                response.SetSuccessResponse(data: analysisResult.Data);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi phân tích ảnh: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tải lên một hình ảnh từ URL (ví dụ: localhost) lên Cloudinary để có URL công khai.
        /// </summary>
        /// <param name="request">Đối tượng chứa URL của hình ảnh cần tải lên.</param>
        /// <returns>URL công khai mới của hình ảnh trên Cloudinary.</returns>
        [HttpPost("UploadFromUrl")]
        public async Task<IActionResult> UploadFromUrl([FromBody] UploadFromUrlRequest request)
        {
            ResponseAPI<UploadImageResponse> res = new();
            if (string.IsNullOrEmpty(request.ImageUrl))
            {
                return BadRequest(new { message = "URL hình ảnh không được rỗng." });
            }

            try
            {
                // Convert localhost URL to physical path
                var uri = new Uri(request.ImageUrl);
                var relativePath = uri.AbsolutePath.TrimStart('/');
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath); //? .Replace('/', '\\')

                if (!System.IO.File.Exists(physicalPath))
                {
                    return NotFound(new { message = $"Không tìm thấy file tại: {physicalPath}" });
                }

                // Read file and upload as Base64
                var imageBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                var base64Image = Convert.ToBase64String(imageBytes);

                var imageUrl = await _cloudinaryService.UploadImageFromBase64Async(base64Image, "product-images");
                res.SetSuccessResponse(data: new UploadImageResponse { ImageUrl = imageUrl });
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi tải ảnh từ URL nội bộ: {ex.Message}" });
            }
        }
    }
}
