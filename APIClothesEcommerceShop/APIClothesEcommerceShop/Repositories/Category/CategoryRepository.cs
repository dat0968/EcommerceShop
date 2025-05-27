using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EcommerceShopContext db;
        public CategoryRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<List<Danhmuccha>> GetAllBigCategories()
        {
            try
            {
                var GetBigCategories = await db.Danhmucchas.AsNoTracking().ToListAsync();
                return GetBigCategories;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<Danhmuccon>> GetAllSmallCategories()
        {
            try
            {
                var GetSmallCategories = await db.Danhmuccons.AsNoTracking().ToListAsync();
                return GetSmallCategories;
            }
            catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
