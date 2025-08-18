
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Services.LightXService
{
    public interface ILightXService
    {
        Task<string> ProcessTryOnAsync(string modelImageUrl, List<string> productImagesUrl);
    }
}
