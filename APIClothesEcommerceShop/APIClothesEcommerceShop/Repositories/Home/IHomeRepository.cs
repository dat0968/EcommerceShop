using APIClothesEcommerceShop.DTO.Category.CategoryParent;
using APIClothesEcommerceShop.DTO.Product;

namespace APIClothesEcommerceShop.Repositories.Home
{
    public interface IHomeRepository
    {
        Task<List<ProductResponseDTO>> GetNewProducts();
        Task<List<ProductResponseDTO>> GetBestsellerProducts();
        Task<List<ProductResponseDTO>> GetFavoriteProduct();
        Task<List<CategoryParentResponseDTO>> GetPublicCategories();
    }
}
