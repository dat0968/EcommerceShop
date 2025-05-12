using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Product
{
    public class UpdateProductResquestDTO
    {
        public int MaSp { get; set; }
        public string TenSanPham { get; set; } = null!;
        public bool? IsActive { get; set; }
        public virtual ICollection<CategoryDetailsResponseDTO> CategoryDetails { get; set; } = new List<CategoryDetailsResponseDTO>();
        public virtual ICollection<UpdateProductDetailRequestDTO> ProductDetails { get; set; } = new List<UpdateProductDetailRequestDTO>();
    }
}
