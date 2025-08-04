using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Category;
using APIClothesEcommerceShop.DTO.Category.CategoryChild;
using APIClothesEcommerceShop.DTO.Category.CategoryDetail;
using APIClothesEcommerceShop.DTO.Category.CategoryParent;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("GetCategoriesforShop")]
        public async Task<IActionResult> GetCategoriesforShop()
        {
            try
            {
                var listBigCategory = await _unit.Category.GetAllBigCategories();
                var listSmallCategory = await _unit.Category.GetAllSmallCategories();
                return Ok(new
                {
                    listBigCategory = listBigCategory,
                    listSmallCategory = listSmallCategory
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Lấy toàn bộ danh sách chi tiết danh mục sản phẩm (bao gồm cha, con, sản phẩm).
        /// </summary>
        [ProducesResponseType(typeof(ResponseAPI<List<CategoryResponseDTO>>), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Nhân viên")]
        [HttpGet("GetAllCategories")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]
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
        [Authorize(Roles = "Admin,Nhân viên")]

        [HttpDelete("detail")]
        public async Task<IActionResult> DeleteCategoryDetail([FromQuery] int maDanhMucCha, [FromQuery] int maDanhMucCon, [FromQuery] int maSp)
        {
            var result = await _unit.Category.DeleteCategoryDetailAsync(maDanhMucCha, maDanhMucCon, maSp);
            return Ok(result);
        }

        /// <summary>
        /// Thay đổi trạng thái hoạt động của danh mục cha.
        /// </summary>
        /// <param name="id">Mã danh mục cha</param>
        [HttpPatch("parent/{id}/change-status")]
        [Authorize(Roles = "Admin,Nhân viên")]
        [ProducesResponseType(typeof(ResponseAPI<dynamic>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeStatusCategory(int id)
        {
            var result = await _unit.Category.ChangeStatusCategoryAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Thay đổi trạng thái hoạt động của danh mục con.
        /// </summary>
        /// <param name="id">Mã danh mục con</param>
        [HttpPatch("child/{id}/change-status")]
        [Authorize(Roles = "Admin,Nhân viên")]
        [ProducesResponseType(typeof(ResponseAPI<dynamic>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeStatusSubCategory(int id)
        {
            var result = await _unit.Category.ChangeStatusSubCategoryAsync(id);
            return Ok(result);
        }
    }
}