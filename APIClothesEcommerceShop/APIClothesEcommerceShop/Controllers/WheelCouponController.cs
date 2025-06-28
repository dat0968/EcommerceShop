using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WheelCouponController : ControllerBase
    {
        private readonly IUnitOfWork _unit;

        public WheelCouponController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        /// <summary>
        /// HavePrivateCoupon (GET): Kiểm tra số lần người dùng có thể quay vòng quay coupon
        /// </summary>
        [HttpGet("time-spin-wheel-coupon")]
        public async Task<IActionResult> TimeCanSpinWheelCoupon()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.TimeCanSpinWheelCoupon(userId);
            return Ok(response);
        }
        /// <summary>
        /// HavePrivateCoupon (GET): Kiểm tra người dùng có coupon riêng không. Lấy userId từ token.
        /// </summary>
        [HttpGet("private-coupon")]
        public async Task<IActionResult> HavePrivateCoupon()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.HavePrivateCoupon(userId);
            return Ok(response);
        }

        /// <summary>
        /// Over2MillionUse (GET): Kiểm tra người dùng đã sử dụng trên 2 triệu cho coupon riêng. Lấy userId từ token.
        /// </summary>
        [HttpGet("over-2-million-use")]
        public async Task<IActionResult> Over2MillionUse()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.Over2MillionUse(userId);
            return Ok(response);
        }

        /// <summary>
        /// IsInWeekSteak (GET): Kiểm tra người dùng có đang ở chuỗi tuần thưởng không. Lấy userId từ token.
        /// </summary>
        [HttpGet("week-streak")]
        public async Task<IActionResult> IsInWeekSteak()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.IsInWeekSteak(userId);
            return Ok(response);
        }

        /// <summary>
        /// CreatePrivateCoupon (POST): Tạo coupon riêng cho người dùng. Lấy userId từ token.
        /// </summary>
        [HttpPost("private-coupon")]
        public async Task<IActionResult> CreatePrivateCoupon()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.CreatePrivateCoupon(userId);
            return Ok(response);
        }
        /// <summary>
        /// UpdateLastLoginAndStreak (PATCH): Cập nhật lần đăng nhập cuối và streak cho người dùng. Lấy userId từ token.
        /// </summary>
        [HttpPatch("update-last-login-streak")]
        public async Task<IActionResult> UpdateLastLoginAndStreak()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var response = await _unit.WheelCoupon.UpdateLastLoginAndStreak(userId);
            return Ok(response);
        }
    }
}