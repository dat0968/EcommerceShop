using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.TryOn;

namespace APIClothesEcommerceShop.Services
{
    public interface IGeminiAIService
    {
        Task<ResponseAPI<object>> AnalyzeReviewContent(string reviewContent);
        Task<ResponseAPI<object>> AnalyzeTryOnImageAsync(string imageBase64, List<ProductData> productsData);
    }
}
