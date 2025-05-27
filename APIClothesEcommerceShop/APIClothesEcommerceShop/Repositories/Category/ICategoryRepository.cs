using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository
    {
        Task<List<Danhmuccha>> GetAllBigCategories();
        Task<List<Danhmuccon>> GetAllSmallCategories();
    }
}
