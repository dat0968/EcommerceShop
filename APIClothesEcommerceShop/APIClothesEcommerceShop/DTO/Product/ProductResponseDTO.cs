using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ProductDetails;
namespace APIClothesEcommerceShop.DTO.Product
{
    public class ProductResponseDTO
    {
        public int MaSp { get; set; }
        public string TenSanPham { get; set; } = null!;

        public string? MoTa { get; set; }
        public bool? HasVariants { get; set; }  
        public string KhoangGia { get; set; }
        public DateTime NgayTao { get; set; }
        public int LuotYeuThich { get; set; }
        public int SoLuong { get; set; }
        public string? AnhDaiDien { get; set; }
        public virtual ICollection<CategoryDetailsResponseDTO> CategoryDetails { get; set; } = new List<CategoryDetailsResponseDTO>();
        public virtual ICollection<ProductDetailResponseDTO> ProductDetails { get; set; } = new List<ProductDetailResponseDTO>();
    }
}
