namespace APIClothesEcommerceShop.Models
{
    public class Sanphamyeuthich
    {
        public int MaSp { get; set; }
        public int MaKh { get; set; }
        public virtual Sanpham MaSpNavigation { get; set; } 
        public virtual Khachhang MaKhNavigation { get; set; }
    }
}
