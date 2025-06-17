namespace APIClothesEcommerceShop.DTO.Cart_DetailsCombo
{
    public class Cart_DetailsComboResponseDTO
    {
        public int Id { get; set; }

        public int MaGioHang { get; set; }

        public int MaCtsp { get; set; }
        public string TenSanPham { get; set; }
        public string? MauSac { get; set;}
        public string? KichThuoc { get; set; }

        public int SoLuong { get; set; }

        public int DonGia { get; set; }
    }
}
