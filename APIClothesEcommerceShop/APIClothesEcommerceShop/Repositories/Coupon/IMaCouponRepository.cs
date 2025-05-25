using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Coupon;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Macoupon
{
    public interface IMaCouponRepository
    {
        public List<CouponDTO> GetAll(string? keywords, string? status, string? sort);
        public Task<CouponDTO?> GetById(string macoupon);
        public CouponDTO Create(CouponDTO maCoupon);
        public void Update(CouponDTO maCoupon);
        public void Cancel(string id);
    }
}
