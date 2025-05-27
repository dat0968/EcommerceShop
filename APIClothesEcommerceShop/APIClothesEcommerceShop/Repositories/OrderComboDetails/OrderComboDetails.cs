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
        public async Task<Chitietcombohoadon> CreateComboOrderDetails(Chitietcombohoadon model)
        {
            try
            {
                var NewComboDetailsOrder = new Chitietcombohoadon
                {
                    MaHd = model.MaHd,
                    MaCtsp = model.MaCtsp,
                    MaCombo = model.MaCombo,
                    SoLuong = model.SoLuong,
                    DonGia = model.DonGia,
                };
                db.Chitietcombohoadons.Add(NewComboDetailsOrder);
                await db.SaveChangesAsync();
                return NewComboDetailsOrder;
            }
            catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
