using System.Threading.Tasks;
using System.Collections.Generic;
using APIClothesEcommerceShop.DTO.TryOn;

namespace APIClothesEcommerceShop.Services.LightXService
{
    public interface ILightXApiService
    {
        Task<string> ProcessWithLightX(string apiKey, string modelImageUrl, List<ProductData> products);
    }
}
