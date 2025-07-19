using System.ComponentModel.DataAnnotations;

namespace APIClothesEcommerceShop.Models
{
    public class LichSuXem
    {
        [Key]
        public int Id { get; set; }
        public int MaKh { get; set; }
        public int? MaSp { get; set; }
        public int? MaCombo { get; set; }
        public DateTime ThoiGianXem { get; set; }
        public Khachhang MaKhNavigation { get; set; }
        public Sanpham? MaSpNavigation { get; set; } 
        public Combo? MaComboNavigation { get; set; }
    }
}
