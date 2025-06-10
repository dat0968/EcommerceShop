namespace APIClothesEcommerceShop.DTO
{
    public class SanPhamTrongComboDTO
    {
        public int MaCTSP { get; set; }
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string KichThuoc { get; set; }
        public string MauSac { get; set; }
        public int SoLuongTon { get; set; }
        public int DonGia { get; set; }
    }

    public class SanPhamComboRequestDTO
    {
        public int MaCTSP { get; set; }
        public int SoLuong { get; set; }
    }

    public class ComboStatusDTO
    {
        public int MaCombo { get; set; }
        public bool IsActive { get; set; }
    }
}
