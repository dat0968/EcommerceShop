using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.OrderComboDetails
{
    public interface IOrderComboDetails
    {
        Task<Chitietcombohoadon> CreateComboOrderDetails(Chitietcombohoadon model);
    }
}
