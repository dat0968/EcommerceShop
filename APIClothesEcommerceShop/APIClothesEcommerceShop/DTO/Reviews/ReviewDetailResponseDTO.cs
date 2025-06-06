using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class ReviewDetailDTO
    {
        public int Id { get; set; }
        public int MaKh { get; set; } = 0;
        public string TenKhachHang { get; set; } = string.Empty; // Thêm tên khách hàng vào phản hồi
        public int? MaSp { get; set; } // Thêm mã sản phẩm vào phản hồi
        public int? MaCombo { get; set; } // Thêm mã combo vào phản hồi
        public string NoiDung { get; set; } = string.Empty;
        public int SoSao { get; set; } = 0;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        public string? ShopPhanHoi { get; set; } = null;
        public DateTime? NgayPhanHoi { get; set; } = null;

        // Thông tin khách
        public string Email { get; set; } = string.Empty; // Email của khách hàng
        public string SoDienThoai { get; set; } = string.Empty; // Số điện thoại của khách hàng
        public string HoTen { get; set; } = string.Empty; // Họ tên của khách hàng


        // Thông tin chi tiết về sản phẩm hoặc combo được đánh giá
        public string TenSanPham { get; set; } = string.Empty; // Tên sản phẩm
        public string HinhAnhs { get; set; } = string.Empty; // Hình ảnh sản phẩm
        public double DonGia { get; set; }
        public int LuotXem { get; set; }
        public int SoLuong { get; set; }
        public bool? IsActive { get; set; }

        // Thông tin các đơn hàng liên quan đến đánh giá này
        public List<OrderReviewInfoDTO> Orders { get; set; } = new();
    }
    // Thông tin đơn hàng liên quan đến đánh giá
    public class OrderReviewInfoDTO
    {
        public int MaHd { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int? MaCtsp { get; set; } // Nếu là sản phẩm lẻ
        public int? MaCombo { get; set; } // Nếu là combo
        public int? SoLuong { get; set; }
    }
    public static class ReviewDetailResponseDTOExtensions
    {
        public static ReviewDetailDTO ToDetailResponseDTO(
                this ReviewResponseDTO review,
                Khachhang? khachHang = null,
                Sanpham? sanPham = null,
                Combo? combo = null
            )
        {
            return new ReviewDetailDTO
            {
                Id = review.Id,
                MaKh = review.MaKh,
                TenKhachHang = khachHang?.HoTen ?? review.TenKhachHang,
                MaSp = review.MaSp,
                MaCombo = review.MaCombo,
                NoiDung = review.NoiDung,
                SoSao = review.SoSao,
                NgayDanhGia = review.NgayDanhGia,
                ShopPhanHoi = review.ShopPhanHoi,
                NgayPhanHoi = review.NgayPhanHoi,
                Email = khachHang?.Email ?? string.Empty,
                SoDienThoai = khachHang?.Sdt ?? string.Empty,
                HoTen = khachHang?.HoTen ?? string.Empty,
                TenSanPham = sanPham?.TenSanPham ?? combo?.TenCombo ?? string.Empty,
                HinhAnhs = combo?.Hinh ?? string.Empty,
                DonGia = sanPham != null
                    ? (sanPham.Chitietsanphams.Any() ? (double)sanPham.Chitietsanphams.Average(x => x.DonGia) : 0)
                    : (combo?.GiaCombo ?? 0),
                LuotXem = sanPham?.LuotXem ?? 0,
                SoLuong = sanPham != null
                    ? (sanPham.Chitietsanphams.Any() ? (int)sanPham.Chitietsanphams.Average(x => x.SoLuongTon) : 0)
                    : (combo?.SoLuong ?? 0),
                IsActive = sanPham?.IsActive ?? combo?.IsActive
            };
        }
    }
}