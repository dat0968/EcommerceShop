using APIClothesEcommerceShop.DTO.Categories.CategoryChild;
using APIClothesEcommerceShop.DTO.Categories.CategoryDetail;
using APIClothesEcommerceShop.DTO.Categories.CategoryParent;
using APIClothesEcommerceShop.Models;
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
                var listCategory = await _unit.Category.GetAllCategoriesAsync();

                return Ok(listCategory);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        [HttpGet("GetAllParentCategories")]
        public async Task<IActionResult> GetAllParentCategories()
        {
            var result = await _unit.Category.GetCategoryParentAsync();
            return Ok(result);
        }
        [HttpGet("GetCategoryById/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _unit.Category.GetCategoryByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("UpsertCategory")]
        public async Task<IActionResult> UpsertCategory(int maDanhMucCon, [FromBody] CategoryParentRequestDTO categoryDto)
        {
            var result = await _unit.Category.UpsertCategoryAsync(maDanhMucCon, categoryDto);
            return Ok(result);
        }
        [HttpDelete("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _unit.Category.DeleteCategoryAsync(id);
            return Ok(result);
        }

        // Danh mục con
        [HttpGet("GetAllSubCategories")]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var result = await _unit.Category.GetAllSubCategoriesAsync();
            return Ok(result);
        }

        [HttpGet("GetSubCategoryById/{id}")]
        public async Task<IActionResult> GetSubCategoryById(int id)
        {
            var result = await _unit.Category.GetSubCategoryByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("UpsertSubCategory")]
        public async Task<IActionResult> UpsertSubCategory(int maDanhMucCon, [FromBody] CategoryChildRequestDTO subCategoryDto)
        {
            var result = await _unit.Category.UpsertSubCategoryAsync(maDanhMucCon, subCategoryDto);
            return Ok(result);
        }

        [HttpDelete("DeleteSubCategory/{id}")]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var result = await _unit.Category.DeleteSubCategoryAsync(id);
            return Ok(result);
        }

        // Chi tiết danh mục
        [HttpGet("GetAllCategoryDetails")]
        public async Task<IActionResult> GetAllCategoryDetails()
        {
            var result = await _unit.Category.GetAllCategoryDetailsAsync();
            return Ok(result);
        }

        [HttpGet("GetCategoryDetailById")]
        public async Task<IActionResult> GetCategoryDetailById(int maDanhMucCha, int maDanhMucCon, int maSp)
        {
            var result = await _unit.Category.GetCategoryDetailByIdAsync(maDanhMucCha, maDanhMucCon, maSp);
            return Ok(result);
        }

        [HttpPost("UpsertCategoryDetail")]
        public async Task<IActionResult> UpsertCategoryDetail([FromBody] CategoryDetailRequestDTO detailDto)
        {
            var result = await _unit.Category.UpsertCategoryDetailAsync(detailDto);
            return Ok(result);
        }

        [HttpDelete("DeleteCategoryDetail")]
        public async Task<IActionResult> DeleteCategoryDetail([FromQuery] int maDanhMucCha, [FromQuery] int maDanhMucCon, [FromQuery] int maSp)
        {
            var result = await _unit.Category.DeleteCategoryDetailAsync(maDanhMucCha, maDanhMucCon, maSp);
            return Ok(result);
        }
    }
}
