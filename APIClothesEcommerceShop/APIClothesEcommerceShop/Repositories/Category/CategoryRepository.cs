using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Utils;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public class CategoryRepository(EcommerceShopContext db) : Repository<Danhmuccha>(db), ICategoryRepository
    {
        private readonly EcommerceShopContext _db = db;
        public async Task<ResponseAPI<List<Danhmuccha>>> GetAllCategories()
        {
            ResponseAPI<List<Danhmuccha>> response = new();
            try
            {
                var GetBigCategories = await _db.Danhmucchas.Include(x => x.Chitietdanhmucs).AsNoTracking().ToListAsync();

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
    }
}
