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

        public int IdKhachHang { get; set; } = 0;
        public int IdSanPham { get; set; } = 0;
        [MaxLength(54)]
        public string? HoTen { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(54)]
        public string? Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; } = 0;

        public DateTime? NgayDanhGia { get; set; } = DateTime.Now;

        [ForeignKey("IdSanPham")]
        public virtual Sanpham? SanPham { get; set; }

        [ForeignKey("IdKhachHang")]
        public virtual Khachhang? Khachhang { get; set; }
        public virtual ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
    }
    public static class DanhGiaExtensions
    {
        public static DanhGia ToDanhGia(this ReviewRequestDTO dto)
        {
            return new DanhGia
            {
                Id = dto.Id ?? 0,
                IdKhachHang = dto.IdKhachHang,
                IdSanPham = dto.IdSanPham,
                HoTen = dto.HoTen,
                Email = dto.Email,
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
                IdKhachHang = entity.IdKhachHang,
                IdSanPham = entity.IdSanPham,
                HoTen = entity.HoTen,
                Email = entity.Email,
                NoiDung = entity.NoiDung,
                SoSao = entity.SoSao,
                NgayDanhGia = entity.NgayDanhGia ?? DateTime.UtcNow
            };
        }

    }
}