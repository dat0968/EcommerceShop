using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.ImageProduct
{
    public class ImageProductRepository : IImageProductRepository
    {
        private readonly EcommerceShopContext db;
        public ImageProductRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Hinhanh> Add(Hinhanh model)
        {
            try
            {
                db.Hinhanhs.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task DeleteImageProductByMaCtSp(int ProductDetailsId)
        {
            try
            {
                var FindImage = await db.Hinhanhs.Where(p => p.MaCtsp == ProductDetailsId).ToListAsync();
                db.Hinhanhs.RemoveRange(FindImage);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
