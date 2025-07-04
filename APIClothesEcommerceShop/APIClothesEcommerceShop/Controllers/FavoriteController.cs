
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.FavoriteProduct;
using APIClothesEcommerceShop.Repositories.Combo;
using APIClothesEcommerceShop.Repositories.FavoriteProduct;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteProduct _favoriteProduct;
        public FavoriteController(IFavoriteProduct favoriteProduct)
        {
            _favoriteProduct = favoriteProduct;
        }
        [HttpPost("CheckFavoriteProduct")]
        public async Task<IActionResult> CheckFavoriteProduct(FavoriteProductDTO fv)
        {
            if (fv == null )
            {
                return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ." });
            }
            var result = await _favoriteProduct.CheckFavoriteProduct(fv);


            return Ok(new
            {
                isFavorited = result,
                
            });
        }

        [HttpPost("AddFavoriteProduct")]
        public async Task<IActionResult> AddFavoriteProduct(FavoriteProductDTO fv)
        {
            if (fv == null || fv.MaKh <= 0 || fv.MaSp <= 0)
            {
                return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ." });
            }

            var result = await _favoriteProduct.AddFavoriteProduct(fv);
            

            return Ok(new
            {
                data = result,
            });
        }
        [HttpGet("GetFavoriteProducts")] // Sửa tên route để khớp với interface
        public async Task<IActionResult> GetFavoriteProducts(int idKhachHang)
        {
            var result = await _favoriteProduct.GetFavoriteProducts(idKhachHang);
            if (result == null )
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm yêu thích nào." });
            }
            return Ok(new
            {
                data = result,
            });
        }
        [HttpDelete("DeleteFavoriteProducts")]
        public async Task<IActionResult> DeleteFavoriteProducts(int idKhachHang, int idSp)
        {
            try
            {
                await _favoriteProduct.DeleteFavoriteProduct(idKhachHang, idSp);
                return Ok(new
                {
                    Message = "Xóa sản phẩm yêu thích thành công"
                });
            }
            catch (Exception ex) {
                throw new Exception("Có lỗi xảy ra: ", ex);
            }
           
           
        }
    }
}
