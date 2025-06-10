using APIClothesEcommerceShop.DTO.Cart;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService cartService;
        private readonly ICartRepository cartRepository;
        public CartController(CartService cartService, ICartRepository cartRepository)
        {
            this.cartService = cartService;
            this.cartRepository = cartRepository;
        }
        [HttpGet("{MaKh}")]
        public async Task<IActionResult> Index([FromRoute]int MaKh)
        {
            try
            {
                var getAll = await cartRepository.GetAll(MaKh);
                return Ok(getAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(CartRequestDTO model)
        {
            try
            {
                await cartService.AddToCart(model);
                return Ok(new
                {
                    Success = true,
                    Message = "Thêm giỏ hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await cartRepository.DeleteCart(id);
                return Ok(new
                {
                    Success = true,
                    Message = "Đã xóa sản phẩm ra khỏi giỏ hàng"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
