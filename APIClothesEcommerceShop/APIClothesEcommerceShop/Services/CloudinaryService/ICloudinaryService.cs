using Microsoft.AspNetCore.Http;

namespace APIClothesEcommerceShop.Services.CloudinaryService
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string tag = "general");
        Task<string> UploadImageFromUrlAsync(string imageUrl, string tag = "general");
        Task<string> UploadImageFromBase64Async(string base64Image, string tag = "general");
        Task<bool> DeleteImageAsync(string publicId);
    }
}