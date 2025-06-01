namespace APIClothesEcommerceShop.Models
{
    public class Chitietcombo
    {
        public int MaSp { get; set; }
        public int MaCombo { get; set; }
        public decimal PhanTramGiam { get; set; }

        public int SoTienGiam { get; set; }
        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }
        public int SoLuong { get; set; }
        public bool? IsActive { get; set; }

        public virtual Sanpham MaSpNavigation { get; set; } = null!;
        public virtual Combo MaComboNavigation { get; set; } = null!;

    }
}
