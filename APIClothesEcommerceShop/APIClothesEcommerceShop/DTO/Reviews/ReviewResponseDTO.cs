using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public int MaKh { get; set; } = 0;
        public string TenKhachHang { get; set; } = string.Empty; // Thêm tên khách hàng vào phản hồi
        public string Avatar { get; set; } = string.Empty;
        public int? MaSp { get; set; } // Thêm mã sản phẩm vào phản hồi
        public int? MaCombo { get; set; } // Thêm mã combo vào phản hồi
        public int MaCthd { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public int SoSao { get; set; } = 0;
        public string[]? HinhAnhs { get; set; }
        // Thông tin chi tiết về ctsp
        public string TenDoiTuong { get; set; } = string.Empty;
        public string TenHinhAnh { get; set; } = string.Empty;
        public string? KichThuoc { get; set; }
        public string? MauSac { get; set; }
        public int SoLuongTon { get; set; }
        public int DonGia { get; set; }
        // Thông tin phụ
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        public string? ShopPhanHoi { get; set; } = null;
        public DateTime? NgayPhanHoi { get; set; } = null;

        public static ReviewResponseDTO MakeEmptyReview(Cthoadon entity, bool isProduct)
        {
            ReviewResponseDTO dTO = new ReviewResponseDTO
            {
                Id = entity.Id,
                MaKh = entity.MaHdNavigation.MaKh ?? 0,
                TenKhachHang = entity.MaHdNavigation.MaKhNavigation?.HoTen ?? "**** ***",
                Avatar = entity.MaHdNavigation.MaKhNavigation?.HinhDaiDien ?? string.Empty,
                MaSp = entity.MaCtspNavigation?.MaSp,
                MaCombo = entity.MaCombo,
                MaCthd = entity.Id,
                TenDoiTuong = entity.MaCtspNavigation?.MaSpNavigation?.TenSanPham ?? entity.MaComboNavigation?.TenCombo ?? "N/A",
                TenHinhAnh = entity.MaCtspNavigation?.Hinhanhs?.FirstOrDefault()?.TenHinhAnh ?? entity?.MaComboNavigation?.Hinh ?? "",
            };
            if (isProduct)
            {
                dTO.KichThuoc = entity?.MaCtspNavigation?.KichThuoc;
                dTO.MauSac = entity?.MaCtspNavigation?.MauSac;
                dTO.SoLuongTon = entity?.MaCtspNavigation?.SoLuongTon ?? 0;
                dTO.DonGia = entity?.MaCtspNavigation?.DonGia ?? 9999;
            }
            return dTO;
        }
    }
}