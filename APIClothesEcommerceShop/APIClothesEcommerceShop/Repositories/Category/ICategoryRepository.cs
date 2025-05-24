using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository : IRepository<Danhmuccha>
    {
        Task<ResponseAPI<List<Danhmuccha>>> GetAllCategories();
    }
}
