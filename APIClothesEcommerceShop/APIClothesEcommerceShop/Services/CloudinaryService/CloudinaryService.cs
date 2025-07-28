using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace APIClothesEcommerceShop.Services.CloudinaryService
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        private readonly string _uploadPreset;
        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudinaryConfig = configuration.GetSection("Cloudinary");
            var account = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );
            _uploadPreset = cloudinaryConfig["UploadPreset"];

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string tag = "general")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is null or empty");

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Tags = tag,
                    UploadPreset = _uploadPreset,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Cloudinary upload error: {uploadResult.Error.Message}");
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                throw;
            }
        }

        public async Task<string> UploadImageFromUrlAsync(string imageUrl, string tag = "general")
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    throw new ArgumentException("Image URL is null or empty");

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageUrl),
                    Tags = tag,
                    UploadPreset = _uploadPreset,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Cloudinary upload error: {uploadResult.Error.Message}");
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image from URL to Cloudinary");
                throw;
            }
        }

        public async Task<string> UploadImageFromBase64Async(string base64Image, string tag = "general")
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                    throw new ArgumentException("Base64 image is null or empty");

                // Remove data URL prefix if present
                if (base64Image.StartsWith("data:"))
                {
                    var commaIndex = base64Image.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        base64Image = base64Image.Substring(commaIndex + 1);
                    }
                }

                var imageBytes = Convert.FromBase64String(base64Image);
                using var stream = new MemoryStream(imageBytes);

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription($"{tag}_{DateTime.Now:yyyyMMddHHmmss}.jpg", stream),
                    Tags = tag,
                    UploadPreset = _uploadPreset,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Cloudinary upload error: {uploadResult.Error.Message}");
                    throw new Exception($"Upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading base64 image to Cloudinary");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting image with publicId: {publicId}");
                return false;
            }
        }
    }
}