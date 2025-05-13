using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace APIClothesEcommerceShop.Repositories.CategoryDetails
{
    public class CategoryDetailsRepository : ICategoryDetailsRepository
    {
        private readonly EcommerceShopContext db;
        public CategoryDetailsRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Chitietdanhmuc> Add(Chitietdanhmuc model)
        {
            try
            {
                db.Chitietdanhmucs.Add(model);
                await db.SaveChangesAsync();
                return model;
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task Delete(int MaDanhMucCha, int MaDanhMucCon, int MaSp)
        {
            try
            {
                var findDetailProduct = await db.Chitietdanhmucs.FirstOrDefaultAsync(p => p.MaDanhMucCha == MaDanhMucCha && p.MaDanhMucCon == MaDanhMucCon && p.MaSp == MaSp);
                if(findDetailProduct == null)
                {
                    throw new Exception("Not Found Product category details ");
                }
                db.Chitietdanhmucs.Remove(findDetailProduct);
                await db.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Chitietdanhmuc> Update(Chitietdanhmuc model)
        {
            try
            {
                db.Chitietdanhmucs.Update(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex) { 
                throw new Exception("Error", ex);
            }
        }
    }
}
