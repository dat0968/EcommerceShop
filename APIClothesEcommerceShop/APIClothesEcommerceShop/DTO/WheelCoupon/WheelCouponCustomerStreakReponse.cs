using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.WheelCoupon
{
    public class WheelCouponCustomerStreakResponse
    {
        public string HoTen { get; set; } = string.Empty;
        public int Streak { get; set; } = 0;
        public DateTime TruyCapLanCuoi { get; set; } = DateTime.Now;
    }
    public static class WheelCouponCustomerStreakResponseExtensions
    {
        public static WheelCouponCustomerStreakResponse ToWheelCouponCustomerStreakResponse(this Khachhang khachhang)
        {
            return new WheelCouponCustomerStreakResponse
            {
                HoTen = khachhang.HoTen,
                Streak = khachhang.Streak,
                TruyCapLanCuoi = khachhang.TruyCapLanCuoi
            };
        }
    }
}