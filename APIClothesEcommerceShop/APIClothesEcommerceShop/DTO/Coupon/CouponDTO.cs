using System.ComponentModel.DataAnnotations;

namespace APIClothesEcommerceShop.DTO.Coupon
{
    public class CouponDTO
    {
        [StringLength(50)]
        public string MaCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);

        public string? MoTa { get; set; }

        public decimal? PhanTramGiam { get; set; }

        public int? SoTienGiam { get; set; }

        public int? DonHangToiThieu { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public int SoLuong { get; set; }

        public int SoLuongDaDung { get; set; }

        public bool? TrangThai { get; set; }
    }
}
