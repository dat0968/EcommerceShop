using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.OrderDetails
{
    public class OrderDetailsResponseDTO
    {
        public int Id { get; set; }

        public int MaHd { get; set; }
        public string TenSanPham { get; set; }
        public string TenCombo { get; set; }
        public string? BienThe { get; set; }

        public int? MaCtsp { get; set; }

        public int? MaCombo { get; set; }

        public int SoLuong { get; set; }

        public int Gia { get; set; }
        public decimal? GiamGia { get; set; }
        public decimal GiaGoc { get; set; }
    }
}
