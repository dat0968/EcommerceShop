using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.OrderDetails
{
    public interface IOrderDetails
    {
        Task<Cthoadon> CreateOrderDetails(Cthoadon model);
    }
}
