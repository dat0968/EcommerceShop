using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.WheelCoupon;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.WheelCoupon
{
    public interface IWheelCouponRepository : IRepository<Models.Macoupon>
    {
        Task<ResponseAPI<PrivateCouponInfoDTO>> HavePrivateCoupon(int? userId);
        Task<ResponseAPI<dynamic>> TimeCanSpinWheelCoupon(int? userId);
        Task<ResponseAPI<WheelCouponCustomerStreakResponse>> UpdateLastLoginAndStreak(int? userId);
        Task<ResponseAPI<dynamic>> SpinWheelAndGenerateCoupon(int? userId, WheelCouponCreateRequest? request);
        Task<ResponseAPI<CouponPresetDTO>> GenerateCouponPreset();
        Task<ResponseAPI<dynamic>> ClaimPresetCoupon(int? userId, ClaimPresetCouponRequest request);
    }
}