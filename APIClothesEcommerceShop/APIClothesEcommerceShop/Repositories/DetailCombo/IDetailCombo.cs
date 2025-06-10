using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.DetailCombo
{
    public interface IDetailCombo
    {
        Task AddDetailCombo(Chitietcombo model);
        Task EditDetailCombo(Chitietcombo model);
        Task DeleteDetailComboByMaCombo(int MaCombo);
    }
}
