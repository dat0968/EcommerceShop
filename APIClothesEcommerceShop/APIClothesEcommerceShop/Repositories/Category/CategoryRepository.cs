using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Category;
using APIClothesEcommerceShop.DTO.CategoryDetails;
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
        public async Task<List<CategoryResponseDTO>> GetAllBigCategories()
        {
            try
            {
                var GetBigCategories = await db.Danhmucchas
                .Include(p => p.Chitietdanhmucs)
                .ThenInclude(p => p.MaDanhMucConNavigation)
                .AsNoTracking()
                .ToListAsync();


                var result = GetBigCategories.Select(d => new CategoryResponseDTO
                {
                    MaDanhMucCha = d.MaDanhMucCha,
                    TenDanhMucCha = d.TenDanhMucCha,
                    Chitietdanhmucs = d.Chitietdanhmucs
                    .GroupBy(ct => ct.MaDanhMucCon)
                    .Select(g => g.First())
                    .Select(ct => new CategoryDetailsResponseDTO
                    {
                        MaDanhMucCha = ct.MaDanhMucCha,
                        MaDanhMucCon = ct.MaDanhMucCon,
                        TenDanhMucCon = ct.MaDanhMucConNavigation.TenDanhMucCon,
                    }).ToList()
                }).ToList();
                return result;
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

        public Task<List<Danhmuccha>> GetCategories()
        {
            throw new NotImplementedException();
        }
    }
}
