using APIClothesEcommerceShop.DTO.Product;

namespace APIClothesEcommerceShop.Repositories.Shop
{
    public interface IShopRepository
    {
        Task<List<ProductResponseDTO>> GetAll(string? search, string? selectedBigCategory, string? selectedSmallCategory, string? sortByPrice, string? filterPrice);
        Task<ProductResponseDTO> GetById(int id);
    }
}
