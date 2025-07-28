using APIClothesEcommerceShop.DTO.TryOn;
using APIClothesEcommerceShop.Services;
using APIClothesEcommerceShop.Services.CloudinaryService;
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

        public TryOnController(IGeminiAIService geminiAIService, ICloudinaryService cloudinaryService)
        {
            _geminiAIService = geminiAIService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost]
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

        private async Task<string> ProcessAndUploadImage(string modelDataUrl, List<string> productDataUrls)
        {
            // In a real application, you would first process the image with a service like LightX,
            // and then upload the resulting image to Cloudinary.
            // For now, we'll just upload the provided model image directly.
            if (string.IsNullOrEmpty(modelDataUrl))
            {
                // Return a default placeholder or handle the error as appropriate
                return "https://via.placeholder.com/300x400.png?text=No+Image+Provided";
            }

            // Upload the base64 image using the Cloudinary service
            var imageUrl = await _cloudinaryService.UploadImageFromBase64Async(modelDataUrl, "try-on");
            return imageUrl;
        }
    }
}
