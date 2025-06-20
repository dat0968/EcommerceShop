using APIClothesEcommerceShop.DTO.Cart_DetailsCombo;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Cart
{
    public class CartResponseDTO
    {
        public int Id { get; set; }

        public int MaKh { get; set; }

        public int? MaCtsp { get; set; }
        public string? TenSanPham_TenCombo { get; set; }
        public string? KichThuoc { get; set; }
        public string? Mau { get; set; }

        public int? MaCombo { get; set; }

        public int SoLuong { get; set; }
        public int SoLuongToiDa { get; set; }

        public int DonGia { get; set; }
        public decimal? GiamGia { get; set; }
        public decimal? GiaTruocKhiGiam { get; set; }
        public string TenHinhAnh { get; set; }

        public virtual ICollection<Cart_DetailsComboResponseDTO> Giohangctcombos { get; set; } = new List<Cart_DetailsComboResponseDTO>();
    }
}
