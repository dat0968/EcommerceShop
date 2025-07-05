using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;

namespace APIClothesEcommerceShop.Services
{
    public interface IGeminiAIService
    {
        Task<ResponseAPI<object>> AnalyzeReviewContent(string reviewContent);
    }
}
