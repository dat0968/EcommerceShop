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
        public Task<ResponseAPI<dynamic>> HavePrivateCoupon(int? userId);
        public Task<ResponseAPI<dynamic>> Over2MillionUse(int? userId);
        public Task<ResponseAPI<dynamic>> IsInWeekSteak(int? userId);
    }
}