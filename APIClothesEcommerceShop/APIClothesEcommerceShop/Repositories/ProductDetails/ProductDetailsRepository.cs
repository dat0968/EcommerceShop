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
                FindProductDetails.IsActive = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Chitietsanpham> Update(Chitietsanpham model)
        {
            throw new NotImplementedException();
        }
    }
}
