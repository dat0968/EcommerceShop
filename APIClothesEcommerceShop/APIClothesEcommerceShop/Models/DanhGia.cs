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

        // Liên kết với cho SanPham
        public int? MaSp { get; set; }
        public int? MaCombo { get; set; }

        [ForeignKey("MaSp")]
        public virtual Sanpham? SanPham { get; set; }

        [ForeignKey("MaCombo")]
        public virtual Combo? Combo { get; set; }

        // Liên kết với khách hàng
        public int MaKh { get; set; }

        [ForeignKey("MaKh")]
        public virtual Khachhang KhachHang { get; set; } = null!;

        // Kiểm tra xem người dùng đã có đánh giá cho sản phẩm/combo này chưa
        public static bool DaCoDanhGia(List<DanhGia> danhSachDanhGia, int maKhachHang, int? maSp, int? maCombo)
        {
            return danhSachDanhGia.Any(dg =>
                dg.MaKh == maKhachHang &&
                ((dg.MaSp == maSp && maCombo == null) || (dg.MaCombo == maCombo && maSp == null)));
        }

        // Phương thức kiểm tra xem người dùng đã mua sản phẩm/combo chưa
        public static bool DaMuaSanPham(List<Hoadon> danhSachHoaDon, int maSp, int? maCombo, int maKhachHang)
        {
            return danhSachHoaDon.Any(hoaDon => hoaDon.MaKh == maKhachHang &&
                (hoaDon.Cthoadons.Any(ct => ct.MaCtsp == maSp) ||
                hoaDon.Chitietcombohoadons.Any(ct => ct.MaCombo == maCombo)));
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
                NoiDung = dto.NoiDung,
                SoSao = dto.SoSao,
                NgayDanhGia = dto.NgayDanhGia
            };
        }

        public static ReviewResponseDTO ToReviewResponseDTO(this DanhGia entity)
        {
            return new ReviewResponseDTO
            {
                Id = entity.Id,
                MaKh = entity.MaKh,
                TenKhachHang = entity.KhachHang?.HoTen ?? "Khách hàng không xác định",
                NoiDung = entity.NoiDung,
                SoSao = entity.SoSao,
                NgayDanhGia = entity.NgayDanhGia,
                ShopPhanHoi = entity.ShopPhanHoi,
                NgayPhanHoi = entity.NgayPhanHoi,
                MaSp = entity.MaSp, // Nếu cần thì thêm mã sản phẩm
                MaCombo = entity.MaCombo // Nếu cần thì thêm mã combo
            };
        }
    }
}