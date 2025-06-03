using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Models
{
    [Table("BINHLUAN")]
    public class BinhLuan
    {
        [Key]
        public int Id { get; set; }

        public int IdSanPham { get; set; }
        public int MaKh { get; set; } = 0;

        [MaxLength(54)]
        public string? HoTen { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(54)]
        public string? Email { get; set; } = string.Empty;
        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayBinhLuan { get; set; } = DateTime.Now;

        [ForeignKey("IdSanPham")]
        public virtual Sanpham? SanPham { get; set; }

        [ForeignKey("MaKh")]
        public virtual Khachhang? Khachhang { get; set; }

        public int ParentId { get; set; } = 0;
    }
}