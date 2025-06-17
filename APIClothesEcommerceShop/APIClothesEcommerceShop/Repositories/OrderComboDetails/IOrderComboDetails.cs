using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.OrderComboDetails
{
    public interface IOrderComboDetails
    {
        Task<Chitietcombohoadon> AddDetailComboOrder(Chitietcombohoadon model);
    }
}
