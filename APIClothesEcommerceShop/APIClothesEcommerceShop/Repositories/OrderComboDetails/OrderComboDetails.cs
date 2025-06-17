using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.OrderComboDetails
{
    public class OrderComboDetails : IOrderComboDetails
    {
        private readonly EcommerceShopContext db;
        public OrderComboDetails(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Chitietcombohoadon> AddDetailComboOrder(Chitietcombohoadon model)
        {
            try
            {
                db.Chitietcombohoadons.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
