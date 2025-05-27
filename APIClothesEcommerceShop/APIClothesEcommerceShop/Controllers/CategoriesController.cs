using APIClothesEcommerceShop.Repositories.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Linq.Expressions;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllBigCategories()
        {
            try
            {
                var ListBigCategories = await categoryRepository.GetAllBigCategories();

                var ListSmallCategories = await categoryRepository.GetAllSmallCategories();
                return Ok(new
                {
                    ListBigCategories = ListBigCategories,
                    ListSmallCategories = ListSmallCategories
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
    }
}
