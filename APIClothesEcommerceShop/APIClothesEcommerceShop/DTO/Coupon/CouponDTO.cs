using System.ComponentModel.DataAnnotations;

namespace APIClothesEcommerceShop.DTO.Coupon
{
    public class CouponDTO
    {
        [StringLength(50)]
        public string MaCode { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);
        public string? MoTa { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public decimal? PhanTramGiam { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số tiền giảm phải lớn hơn hoặc bằng 0")]
        public int? SoTienGiam { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public int? DonHangToiThieu { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime NgayKetThuc { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
        public int SoLuongDaDung { get; set; }

        public bool? TrangThai { get; set; }

        public int? MaKhachHang { get; set; }
        public string? HoTen { get; set; }
    }
}