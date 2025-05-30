using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Categories;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using System.Linq.Expressions;

namespace APIClothesEcommerceShop.Controllers
{
    /// <summary>
    /// Quản lý danh mục sản phẩm (bao gồm danh mục cha, con và chi tiết).
    /// Danh mục cha là danh mục lớn nhất, danh mục con là danh mục nhỏ hơn thuộc về danh mục cha. (fake)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unit;
        public CategoriesController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách chi tiết danh mục sản phẩm (bao gồm cha, con, sản phẩm).
        /// </summary>
        [ProducesResponseType(typeof(ResponseAPI<List<CategoryResponseDTO>>), StatusCodes.Status200OK)]
        [HttpGet("")]
        public async Task<IActionResult> GetCategories()
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

        /// <summary>
        /// Lấy danh sách tất cả danh mục cha.
        /// </summary>
        [ProducesResponseType(typeof(ResponseAPI<List<CategoryParentResponseDTO>>), StatusCodes.Status200OK)]
        [HttpGet("parents")]
        public async Task<IActionResult> GetAllParentCategories()
        {
            var result = await _unit.Category.GetCategoryParentAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin danh mục cha theo mã.
        /// </summary>
        /// <param name="id">Mã danh mục cha</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryParentResponseDTO>), StatusCodes.Status200OK)]
        [HttpGet("parent/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _unit.Category.GetCategoryByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật danh mục cha.
        /// </summary>
        /// <param name="maDanhMucCha">Mã danh mục cha (0 nếu thêm mới)</param>
        /// <param name="categoryDto">Thông tin danh mục cha</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryParentResponseDTO>), StatusCodes.Status200OK)]
        [HttpPost("parent/{maDanhMucCha}")]
        public async Task<IActionResult> UpsertCategory(int maDanhMucCha, [FromBody] CategoryParentRequestDTO categoryDto)
        {
            var result = await _unit.Category.UpsertCategoryAsync(maDanhMucCha, categoryDto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa danh mục cha theo mã.
        /// </summary>
        /// <param name="id">Mã danh mục cha</param>
        [ProducesResponseType(typeof(ResponseAPI<dynamic>), StatusCodes.Status200OK)]
        [HttpDelete("parent/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _unit.Category.DeleteCategoryAsync(id);
            return Ok(result);
        }

        // Danh mục con

        /// <summary>
        /// Lấy danh sách tất cả danh mục con.
        /// </summary>
        [ProducesResponseType(typeof(ResponseAPI<List<CategoryChildResponseDTO>>), StatusCodes.Status200OK)]
        [HttpGet("childs")]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var result = await _unit.Category.GetAllSubCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin danh mục con theo mã.
        /// </summary>
        /// <param name="id">Mã danh mục con</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryChildResponseDTO>), StatusCodes.Status200OK)]
        [HttpGet("child/{id}")]
        public async Task<IActionResult> GetSubCategoryById(int id)
        {
            var result = await _unit.Category.GetSubCategoryByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật danh mục con.
        /// </summary>
        /// <param name="maDanhMucCon">Mã danh mục con (0 nếu thêm mới)</param>
        /// <param name="subCategoryDto">Thông tin danh mục con</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryChildResponseDTO>), StatusCodes.Status200OK)]
        [HttpPost("child/{maDanhMucCon}")]
        public async Task<IActionResult> UpsertSubCategory(int maDanhMucCon, [FromBody] CategoryChildRequestDTO subCategoryDto)
        {
            var result = await _unit.Category.UpsertSubCategoryAsync(maDanhMucCon, subCategoryDto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa danh mục con theo mã.
        /// </summary>
        /// <param name="id">Mã danh mục con</param>
        [ProducesResponseType(typeof(ResponseAPI<dynamic>), StatusCodes.Status200OK)]
        [HttpDelete("child/{id}")]
        public async Task<IActionResult> DeleteSubCategory(int id)
        {
            var result = await _unit.Category.DeleteSubCategoryAsync(id);
            return Ok(result);
        }

        // Chi tiết danh mục

        /// <summary>
        /// Lấy danh sách tất cả chi tiết danh mục (liên kết cha, con, sản phẩm).
        /// </summary>
        [ProducesResponseType(typeof(ResponseAPI<List<CategoryResponseDTO>>), StatusCodes.Status200OK)]
        [HttpGet("details")]
        public async Task<IActionResult> GetAllCategoryDetails()
        {
            var result = await _unit.Category.GetAllCategoryDetailsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết danh mục theo mã cha, mã con, mã sản phẩm.
        /// </summary>
        /// <param name="maDanhMucCha">Mã danh mục cha</param>
        /// <param name="maDanhMucCon">Mã danh mục con</param>
        /// <param name="maSp">Mã sản phẩm</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryDetailResponseDTO>), StatusCodes.Status200OK)]
        [HttpGet("detail")]
        public async Task<IActionResult> GetCategoryDetailById(int maDanhMucCha, int maDanhMucCon, int maSp)
        {
            var result = await _unit.Category.GetCategoryDetailByIdAsync(maDanhMucCha, maDanhMucCon, maSp);
            return Ok(result);
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật chi tiết danh mục.
        /// </summary>
        /// <param name="detailDto">Thông tin chi tiết danh mục</param>
        [ProducesResponseType(typeof(ResponseAPI<CategoryDetailResponseDTO>), StatusCodes.Status200OK)]
        [HttpPost("detail")]
        public async Task<IActionResult> UpsertCategoryDetail([FromBody] CategoryDetailRequestDTO detailDto)
        {
            var result = await _unit.Category.UpsertCategoryDetailAsync(detailDto);
            return Ok(result);
        }

        /// <summary>
        /// Xóa chi tiết danh mục theo mã cha, mã con, mã sản phẩm.
        /// </summary>
        /// <param name="maDanhMucCha">Mã danh mục cha</param>
        /// <param name="maDanhMucCon">Mã danh mục con</param>
        /// <param name="maSp">Mã sản phẩm</param>
        [ProducesResponseType(typeof(ResponseAPI<dynamic>), StatusCodes.Status200OK)]
        [HttpDelete("detail")]
        public async Task<IActionResult> DeleteCategoryDetail([FromQuery] int maDanhMucCha, [FromQuery] int maDanhMucCon, [FromQuery] int maSp)
        {
            var result = await _unit.Category.DeleteCategoryDetailAsync(maDanhMucCha, maDanhMucCon, maSp);
            return Ok(result);
        }
    }
}