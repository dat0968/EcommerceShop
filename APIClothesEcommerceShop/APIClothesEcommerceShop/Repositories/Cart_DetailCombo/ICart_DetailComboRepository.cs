using APIClothesEcommerceShop.DTO.Cart;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Cart_DetailCombo
{
    public interface ICart_DetailComboRepository
    {
        Task<Giohangctcombo> AddCart_DetailCombo(Giohangctcombo model);
        Task<List<Giohangctcombo>> DetailsCart_DetailCombo(int MaGioHang);
        Task<Giohangctcombo> UpdateCart_DetailCombo(int MaGioHang, int Quantity);
        Task DeleteCart_DetailCombo(int MaGioHang);
    }
}
