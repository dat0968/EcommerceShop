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
        //[HttpGet("GetDiscountCoupon")]
        //public async Task<IActionResult> GetDiscountCoupon(int maUser, string couponcode, decimal originalPrice)
        //{
        //    try
        //    {
        //        var findCouponCode = await MaCouponRepository.GetById(couponcode);
        //        if (findCouponCode == null)
        //        {
        //            return Ok( new { Success = false, Message = "Mã Coupon này không tồn tại" });
        //        }
        //        if(findCouponCode.SoLuongDaDung + 1 > findCouponCode.SoLuong)
        //        {
        //            return Ok(new { Success = false, Message = "Mã này đã hết hàng" });
        //        }
        //        if(findCouponCode.NgayBatDau > DateTime.Now || findCouponCode.NgayKetThuc < DateTime.Now || findCouponCode.TrangThai == false)
        //        {
        //            return Ok( new { Success = false, Message = "Mã không khả dụng" });
        //        }
        //        if(findCouponCode.DonHangToiThieu > originalPrice)
        //        {
        //            return Ok(new { Success = false, Message = $"Mã chỉ áp dụng với đơn hàng có giá trị tối thiểu {findCouponCode.DonHangToiThieu} (không tính phí ship)" });
        //        }
        //        var check = await DetailMaCoupon.CheckUser_CouponCode(maUser, couponcode);
        //        if(check == false)
        //        {
        //            return Ok( new { Success = false, Message = "Bạn đã sử dụng mã này rồi" });
        //        }
        //        var afterApplyCouponPrice = (decimal)(findCouponCode.PhanTramGiam != null ? (originalPrice * findCouponCode.PhanTramGiam / 100) : (findCouponCode.SoTienGiam != null ? findCouponCode.SoTienGiam : 0));
        //        return Ok(new
        //        {
        //            Success = true,
        //            OriginalPrice = originalPrice,
        //            Discount = afterApplyCouponPrice,
        //            Message = "Áp dụng mã coupon thành công"
        //        });
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.InnerException });
        //    }
        //}
    }
}
