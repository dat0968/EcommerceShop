using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
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
                var findDetailCategory = await db.Chitietdanhmucs.FirstOrDefaultAsync(p => p.MaDanhMucCha == MaDanhMucCha && p.MaDanhMucCon == MaDanhMucCon && p.MaSp == MaSp);
                if(findDetailCategory == null)
                {
                    throw new Exception("Not Found Product category details ");
                }
                db.Chitietdanhmucs.Remove(findDetailCategory);
                await db.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<Chitietdanhmuc>> GetDetailCategoryByProductId(int id)
        {
            try
            {
                var findDetailCategory = await db.Chitietdanhmucs.AsNoTracking().Where(p => p.MaSp == id).ToListAsync();
                if(findDetailCategory == null)
                {
                    throw new Exception("Not Found Product category details ");
                }
                return findDetailCategory;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Chitietdanhmuc> Update(Chitietdanhmuc model)
        {
            try
            {
                var findDetailCategory = await db.Chitietdanhmucs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.MaDanhMucCha == model.MaDanhMucCha && p.MaDanhMucCon == model.MaDanhMucCon && p.MaSp == model.MaSp);
                if (findDetailCategory == null)
                {
                    throw new Exception("Not Found Product category details ");
                }

                var TrackedDanhMucCha = db.ChangeTracker.Entries<Danhmuccha>().FirstOrDefault(p => p.Entity.MaDanhMucCha == model.MaDanhMucCha);
                if(TrackedDanhMucCha != null)
                {
                    db.Entry(TrackedDanhMucCha.Entity).State = EntityState.Detached;
                }
                var TrackedDanhMucCon = db.ChangeTracker.Entries<Danhmuccon>().FirstOrDefault(p => p.Entity.MaDanhMucCon == model.MaDanhMucCon);
                if (TrackedDanhMucCon != null)
                {
                    db.Entry(TrackedDanhMucCon.Entity).State = EntityState.Detached;
                }
                var TrackedSanPham = db.ChangeTracker.Entries<Sanpham>().FirstOrDefault(p => p.Entity.MaSp == model.MaSp);
                if (TrackedSanPham != null)
                {
                    db.Entry(TrackedSanPham.Entity).State = EntityState.Detached;
                }

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
