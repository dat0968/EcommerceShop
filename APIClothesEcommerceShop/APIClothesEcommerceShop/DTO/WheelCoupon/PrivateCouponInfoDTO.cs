
namespace APIClothesEcommerceShop.DTO.WheelCoupon
{
    public class PrivateCouponInfoDTO
    {
        public int Streak { get; set; }
        public decimal TotalOrderValue { get; set; }
        public int WonSpins { get; set; }
        public int BlankSpins { get; set; }
        public List<CouponInfoDTO> PrivateCoupons { get; set; }
    }

    public class CouponInfoDTO
    {
        public string MaCode { get; set; }
        public string MoTa { get; set; }
        public decimal? SoTienGiam { get; set; }
        public decimal? PhanTramGiam { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public bool IsUsed { get; set; }
    }
}
