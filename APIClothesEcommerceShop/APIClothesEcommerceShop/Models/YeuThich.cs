using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Models
{
    [Table("YEUTHICH")]
    public class YeuThich
    {
        [Key]
        public int Id { get; set; }
        public bool DaThich { get; set; } = false;
        public int IdKhachHang { get; set; }
        public int IdDanhGia { get; set; }

        [ForeignKey("IdKhachHang")]
        public virtual Khachhang KhachHang { get; set; } = null!;

        // [ForeignKey("IdDanhGia")]
        // public virtual DanhGia DanhGia { get; set; } = null!;
    }
}