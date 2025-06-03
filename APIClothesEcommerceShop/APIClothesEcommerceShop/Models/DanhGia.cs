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

        public int MaKh { get; set; }  // Liên kết với khách hàng
        public int MaHd { get; set; }  // Liên kết với hóa đơn đã mua
        public int? MaCtsp { get; set; } // Liên kết với sản phẩm trong hóa đơn (nếu có)
        public int? MaCombo { get; set; } // Liên kết với combo trong hóa đơn (nếu có)

        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow;

        public string? ShopPhanHoi { get; set; } = null;

        [ForeignKey("MaKh")]
        public virtual Khachhang? KhachHang { get; set; }

        [ForeignKey("MaHd")]
        public virtual Hoadon? Hoadon { get; set; }

        [ForeignKey("MaCtsp")]
        public virtual Chitietsanpham? ChitietSanPham { get; set; }

        [ForeignKey("MaCombo")]
        public virtual Combo? Combo { get; set; }
    }


    public static class DanhGiaExtensions
    {
        public static DanhGia ToDanhGia(this ReviewRequestDTO dto)
        {
            return new DanhGia
            {
                Id = dto.Id ?? 0,
                MaKh = dto.MaKh,
                MaHd = dto.MaHd,
                MaCtsp = dto.MaCtsp,
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
                MaHd = entity.MaHd,
                MaCtsp = entity.MaCtsp,
                MaCombo = entity.MaCombo,
                NoiDung = entity.NoiDung,
                SoSao = entity.SoSao,
                NgayDanhGia = entity.NgayDanhGia,
                ShopPhanHoi = entity.ShopPhanHoi
            };
        }
    }

}