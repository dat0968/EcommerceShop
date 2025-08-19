using APIClothesEcommerceShop.DTO.ProductDetails;

namespace APIClothesEcommerceShop.DTO.Combos
{
    public class ComboResponseDTO
    {
        public int MaCombo { get; set; }
        public string TenCombo { get; set; }
        public string Hinh { get; set; }
        public int SoLuong { get; set; }
        public string MoTa { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public float? PhanTramGiam { get; set; }
        public decimal? SoTienGiam { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public List<DetaisComboResponseDTO> Chitietcombos { get; set; } = new List<DetaisComboResponseDTO>();
    }
    public class DetaisComboResponseDTO
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; }
        public int SoLuongSp { get; set; }
        public List<ProductDetailResponseDTO> SanPhamCTs { get; set; }
    }
}
