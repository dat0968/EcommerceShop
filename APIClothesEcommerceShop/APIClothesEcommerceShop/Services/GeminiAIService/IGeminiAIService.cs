using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;

namespace APIClothesEcommerceShop.Services
{
    public interface IGeminiAIService
    {
        Task<ResponseAPI<object>> AnalyzeReviewContent(string reviewContent);
        Task<ResponseAPI<object>> AnalyzeTryOnImageAsync(string imageBase64, List<object> productsData);
    }
}
