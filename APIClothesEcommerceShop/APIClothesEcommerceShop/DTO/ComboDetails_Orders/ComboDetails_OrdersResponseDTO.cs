using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.ComboDetails_Orders
{
    public class ComboDetails_OrdersResponseDTO
    {
        public int MaHd { get; set; }

        public int MaCtsp { get; set; }
        public string TenSanPham { get; set; }
        public string? MauSac { get; set; }
        public string? KichThuoc { get; set; }  

        public int MaCombo { get; set; }

        public int SoLuong { get; set; }

        public int DonGia { get; set; }
    }
}
