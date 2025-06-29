namespace APIClothesEcommerceShop.DTO.FavoriteProduct
{
    public class FavoritveResponsDTO
    {
        public int MaSp { get; set; }
        public int MaKh { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal? KhoangGia { get; set; } // Nullable để xử lý trường hợp không có giá
        public int? SoLuong { get; set; } 
      
    }
}
