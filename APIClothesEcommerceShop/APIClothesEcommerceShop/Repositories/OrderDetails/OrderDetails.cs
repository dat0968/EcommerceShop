using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.OrderDetails
{
    public class OrderDetails : IOrderDetails
    {
        private readonly EcommerceShopContext db;
        public OrderDetails(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Cthoadon> CreateOrderDetails(Cthoadon model)
        {
            try
            {
                var NewOrderDetails = new Cthoadon
                {
                    MaHd = model.MaHd,
                    MaCtsp = model.MaCtsp,
                    MaCombo = model.MaCombo,
                    SoLuong = model.SoLuong,
                    Gia = model.Gia,
                    GiamGia = model.GiamGia,
                };
                db.Cthoadons.Add(NewOrderDetails);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
