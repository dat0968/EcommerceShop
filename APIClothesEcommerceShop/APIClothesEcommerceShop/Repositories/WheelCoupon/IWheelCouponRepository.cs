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
        public Task<ResponseAPI<dynamic>> TimeCanSpinWheelCoupon(int? userId);
        public Task<ResponseAPI<dynamic>> HavePrivateCoupon(int? userId);
        /// <summary>
        /// Cập nhật lần đăng nhập cuối và streak cho khách hàng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>ResponseAPI với thông tin khách hàng đã cập nhật</returns>
        Task<ResponseAPI<WheelCouponCustomerStreakResponse>> UpdateLastLoginAndStreak(int? userId);
        Task<ResponseAPI<dynamic>> SpinWheelAndGenerateCoupon(int? userId);
    }
}