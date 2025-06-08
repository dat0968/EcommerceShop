using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Reviews;

namespace APIClothesEcommerceShop.Models
{
    [Table("DANHGIA")]
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }


        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;

        public string? ShopPhanHoi { get; set; } = string.Empty;
        public DateTime? NgayPhanHoi { get; set; } = null;
        public string? TenCacHinhAnh { get; set; } = string.Empty;

        // Liên kết với cho SanPham
        public int? MaSp { get; set; }
        public int? MaCombo { get; set; }
        public int MaCtHd { get; set; }

        [ForeignKey("MaSp")]
        public virtual Sanpham? SanPham { get; set; }

        [ForeignKey("MaCombo")]
        public virtual Combo? Combo { get; set; }
        [ForeignKey("MaCtHd")]
        public virtual Cthoadon? Cthoadon { get; set; }

        // Liên kết với khách hàng
        public int MaKh { get; set; }

        [ForeignKey("MaKh")]
        public virtual Khachhang KhachHang { get; set; } = null!;

        public void CombineNameImg(string[] listNameImg)
        {
            this.TenCacHinhAnh = String.Join(",", listNameImg);
        }
        public string[]? GetSavedListFileName()
        {
            return string.IsNullOrEmpty(TenCacHinhAnh) ? null : TenCacHinhAnh?.Split(',');
        }
    }


    public static class DanhGiaExtensions
    {
        public static DanhGia ToDanhGia(this ReviewRequestDTO dto)
        {
            return new DanhGia
            {
                Id = dto.Id ?? 0,
                MaKh = dto.MaKh,
                MaSp = dto.MaSp,
                MaCombo = dto.MaCombo,
                MaCtHd = dto.MaCtHd,
                NoiDung = dto.NoiDung,
                SoSao = dto.SoSao,
                NgayDanhGia = dto.NgayDanhGia
            };
        }

        public static ReviewResponseDTO ToReviewResponseDTO(this DanhGia entity, bool isProduct)
        {
            ReviewResponseDTO dTO = new ReviewResponseDTO
            {
                Id = entity.Id,
                MaKh = entity.MaKh,
                TenKhachHang = entity.KhachHang?.HoTen ?? "**** ***",
                Avatar = entity.KhachHang?.HinhDaiDien ?? string.Empty,
                MaSp = entity.MaSp,
                MaCombo = entity.MaCombo,
                MaCthd = entity.MaCtHd,
                NoiDung = entity.NoiDung,
                SoSao = entity.SoSao,
                TenDoiTuong = entity.SanPham?.TenSanPham ?? entity.Combo?.TenCombo ?? "N/A",
                TenHinhAnh = entity.Cthoadon?.MaCtspNavigation?.Hinhanhs?.FirstOrDefault()?.TenHinhAnh ?? entity.Cthoadon?.MaComboNavigation?.Hinh ?? "",
                HinhAnhs = entity.GetSavedListFileName(),
                NgayDanhGia = entity.NgayDanhGia,
                ShopPhanHoi = entity.ShopPhanHoi,
                NgayPhanHoi = entity.NgayPhanHoi
            };
            if (isProduct)
            {
                dTO.KichThuoc = entity.Cthoadon?.MaCtspNavigation?.KichThuoc;
                dTO.MauSac = entity.Cthoadon?.MaCtspNavigation?.MauSac;
                dTO.SoLuongTon = entity.Cthoadon?.MaCtspNavigation?.SoLuongTon ?? 0;
                dTO.DonGia = entity.Cthoadon?.MaCtspNavigation?.DonGia ?? 9999;
            }
            return dTO;
        }
    }
}