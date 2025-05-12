using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Product
{
    public class AddProductResquestDTO
    {
        public string TenSanPham { get; set; } = null!;
        public bool? IsActive { get; set; }
        public virtual ICollection<CategoryDetailsResponseDTO> CategoryDetails { get; set; } = new List<CategoryDetailsResponseDTO>();
        public virtual ICollection<AddProductDetailResponseDTO> ProductDetails { get; set; } = new List<AddProductDetailResponseDTO>();
    }
}
