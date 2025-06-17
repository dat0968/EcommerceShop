using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Combos;

namespace APIClothesEcommerceShop.Repositories.Combo
{
    public interface IComboRepository
    {
        Task<List<ComboResponseDTO>> GetAll(string? search);
        Task<ComboResponseDTO?> GetById(int id);
        Task<APIClothesEcommerceShop.Models.Combo> AddCombo(APIClothesEcommerceShop.Models.Combo newcombo);
        Task EditCombo(APIClothesEcommerceShop.Models.Combo model);
        Task CancelCombo(int id);
    }
}
