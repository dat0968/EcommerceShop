using APIClothesEcommerceShop.DTO.Order;
using APIClothesEcommerceShop.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly CheckoutService checkoutService;
        public CheckoutController(CheckoutService checkoutServic)
        {
            this.checkoutService = checkoutServic;
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderRequestDTO model)
        {
            try
            {
                var NewOder = await checkoutService.Checkout(model);
                if(NewOder == null)
                {
                    throw new Exception("Đặt hàng không thành công");
                }
                return Ok(new
                {
                    Success = true,
                    Message = "Đặt hàng thành công"
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
