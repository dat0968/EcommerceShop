using APIClothesEcommerceShop.Repositories.Home;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeRepository homeRepository;
        private readonly MLRecommendationSystem mLRecommendationSystem;
        public HomeController(IHomeRepository homeRepository, MLRecommendationSystem mLRecommendationSystem)
        {
            this.homeRepository = homeRepository;
            this.mLRecommendationSystem = mLRecommendationSystem;
        }
        [HttpGet("GetNewProduct")]
        public async Task<IActionResult> GetNewProduct()
        {
            try
            {
                var products = await homeRepository.GetNewProducts();
                return Ok(products);
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpGet("GetBestSellerProduct")]
        public async Task<IActionResult> GetBestSellerProduct()
        {
            try
            {
                var products = await homeRepository.GetBestsellerProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpGet("GetHotProduct")]
        public async Task<IActionResult> GetBestHotProduct()
        {
            try
            {
                var products = await homeRepository.GetHotProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpGet("RecommendationProduct")]
        public async Task<IActionResult> RecommendationProduct(int UserId, int? maSp = null, int numberOfRecommendations = 6)
        {
            try
            {
                var products = await mLRecommendationSystem.Recommend(UserId, maSp, numberOfRecommendations);
                return Ok(products);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
