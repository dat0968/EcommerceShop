using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
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
        private readonly IUnitOfWork _unit;
        public CategoriesController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllBigCategories()
        {
            try
            {
                var listCategory = await _unit.Category.GetAllCategories();

                return Ok(listCategory);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
