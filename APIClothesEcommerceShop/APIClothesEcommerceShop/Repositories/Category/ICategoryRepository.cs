using APIClothesEcommerceShop.DTO.Category;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository
    {
        Task<List<Danhmuccha>> GetCategories();
        Task<List<CategoryResponseDTO>> GetAllBigCategories();
        Task<List<Danhmuccon>> GetAllSmallCategories();
    }
}
