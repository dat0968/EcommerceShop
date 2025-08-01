using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.TryOn;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Services.CloudinaryService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TryOnController : ControllerBase
    {
        private readonly IGeminiAIService _geminiAIService;
        private readonly ICloudinaryService _cloudinaryService;

        public TryOnController(IGeminiAIService geminiAIService, ICloudinaryService cloudinaryService)
        {
            _geminiAIService = geminiAIService;
            _cloudinaryService = cloudinaryService;
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
                res.Data = new UploadImageResponse { ImageUrl = imageUrl };
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

                return Ok(new AnalysisResponse { GeminiAnalysis = analysisResult.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi khi phân tích ảnh: {ex.Message}" });
            }
        }
    }
}
