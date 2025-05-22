using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.ProductDetails
{
    public class ProductDetailsRepository : IProductDetailsRepository
    {
        private readonly EcommerceShopContext db;
        public ProductDetailsRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Chitietsanpham> Add(Chitietsanpham model)
        {
            try
            {
                db.Chitietsanphams.Add(model);
                await db.SaveChangesAsync();
                return model;
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task Cancel(int id)
        {
            try
            {
                var FindProductDetails = await db.Chitietsanphams.AsNoTracking().FirstOrDefaultAsync(p => p.MaCtsp == id);
                if(FindProductDetails == null)
                {
                    throw new Exception("Not Found Details Product");
                }
                FindProductDetails.IsActive = false;
                db.Chitietsanphams.Update(FindProductDetails);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<Chitietsanpham>> GetDetailProductByProductId(int productId)
        {
            try
            {
                var FindProductDetails = await db.Chitietsanphams.AsNoTracking().Where(p => p.MaSpNavigation.MaSp == productId).ToListAsync();
                if (FindProductDetails == null)
                {
                    throw new Exception("Not Found Details Product");
                }
                return FindProductDetails;
            }
            catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Chitietsanpham> Update(Chitietsanpham model)
        {
            try
            {
                // Ngắt theo dõi thực thể cũ
                var TrackedProductDetails = db.ChangeTracker.Entries<Chitietsanpham>().FirstOrDefault(p => p.Entity.MaCtsp == model.MaCtsp);
                if(TrackedProductDetails != null)
                {
                    db.Entry(TrackedProductDetails.Entity).State = EntityState.Detached;
                }

                var TrackedProduct = db.ChangeTracker.Entries<Sanpham>().FirstOrDefault(p => p.Entity.MaSp == model.MaSp);
                if(TrackedProduct != null)
                {
                    db.Entry(TrackedProduct.Entity).State = EntityState.Detached;
                }
                db.Chitietsanphams.Update(model);
                await db.SaveChangesAsync();
                return model;
            }catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
