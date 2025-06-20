using APIClothesEcommerceShop.DTO.Cart_DetailsCombo;

namespace APIClothesEcommerceShop.DTO.Cart
{
    public class CartRequestDTO
    {
        public int MaKh { get; set; }

        public int? MaCtsp { get; set; }

        public int? MaCombo { get; set; }

        public int SoLuong { get; set; }

        public int DonGia { get; set; }

        public decimal? GiamGia { get; set; }
        public string TenHinhAnh { get; set; }

        public virtual ICollection<Cart_DetailsComboRequestDTO> Giohangctcombos { get; set; } = new List<Cart_DetailsComboRequestDTO>();
    }
}
