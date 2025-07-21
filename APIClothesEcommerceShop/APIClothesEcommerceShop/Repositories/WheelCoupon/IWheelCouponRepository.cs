using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.WheelCoupon
{
    public interface IWheelCouponRepository : IRepository<Models.Macoupon>
    {
        public Task<ResponseAPI<dynamic>> TimeCanSpinWheelCoupon(int? userId);
        public Task<ResponseAPI<dynamic>> HavePrivateCoupon(int? userId);
        public Task<ResponseAPI<dynamic>> Over2MillionUse(int? userId);
        public Task<ResponseAPI<dynamic>> IsInWeekSteak(int? userId);
        Task<ResponseAPI<dynamic>> CreatePrivateCoupon(int? userId);
        /// <summary>
        /// Cập nhật lần đăng nhập cuối và streak cho khách hàng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>ResponseAPI với thông tin khách hàng đã cập nhật</returns>
        Task<ResponseAPI<Khachhang>> UpdateLastLoginAndStreak(int? userId);
        Task<ResponseAPI<dynamic>> CreateBlankCoupon(int? userId);
    }
}