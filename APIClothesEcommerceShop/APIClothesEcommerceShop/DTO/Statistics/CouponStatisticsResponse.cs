namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class CouponStatisticsResponse
    {
        public int TotalCoupons { get; set; }
        public int TotalActiveCoupons { get; set; }
        public int TotalInactiveCoupons { get; set; }
        public decimal? TotalDiscountAmount { get; set; }
        public List<TopCoupon>? TopCoupons { get; set; }
    }

    public class TopCoupon
    {
        public string CouponCode { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal? RevenueGenerated { get; set; }
    }
}