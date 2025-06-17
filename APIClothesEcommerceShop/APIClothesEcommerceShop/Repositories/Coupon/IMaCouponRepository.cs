using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Coupon;
using APIClothesEcommerceShop.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Repositories.Macoupon
{
    public interface IMaCouponRepository
    {
        Task<List<CouponDTO>> GetAll(string? keywords, string? status, string? sort);
        Task<CouponDTO?> GetById(string macoupon);
        Task<CouponDTO> Create(CouponDTO maCoupon);
        Task Update(CouponDTO maCoupon);
        Task Cancel(string id);
        Task<bool> CheckUser_CouponCode(int maUser, string couponcode);
    }
}