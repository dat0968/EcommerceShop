namespace APIClothesEcommerceShop.Models
{
    public class Diachi
    {
        public int ID { get; set; } 
        public string diachichitiet { get; set; }
        public int MaKh {  get; set; }
        public Khachhang MaKhNavigation { get; set; }
    }
}
