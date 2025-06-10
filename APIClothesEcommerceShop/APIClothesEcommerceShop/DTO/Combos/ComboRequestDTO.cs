namespace APIClothesEcommerceShop.DTO.Combos
{
    public class ComboRequestDTO
    {
        public string TenCombo { get; set; }
        public IFormFile? Hinh { get; set; }
        public int SoLuong { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public float PhanTramGiam { get; set; }
        public decimal SoTienGiam { get; set; }
        public List<SanPhamComboRequestDTO> SanPhams { get; set; }
        public List<DetaisComboRequestDTO> Chitietcombos { get; set; } = new List<DetaisComboRequestDTO>();
    }
    public class DetaisComboRequestDTO
    {
        public int MaSp { get; set; }
        public int SoLuongSp { get; set; }
    }
}
