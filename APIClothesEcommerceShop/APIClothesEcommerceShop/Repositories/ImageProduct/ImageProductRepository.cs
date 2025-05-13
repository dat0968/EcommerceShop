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

        public async Task Delete(int id)
        {
            try
            {
                var FindImage = await db.Hinhanhs.FirstOrDefaultAsync(p => p.MaHinhAnh == id);
                db.Hinhanhs.Remove(FindImage);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
