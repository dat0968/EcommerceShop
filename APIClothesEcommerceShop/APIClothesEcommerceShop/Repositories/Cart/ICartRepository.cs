using APIClothesEcommerceShop.DTO.Cart;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Cart
{
    public interface ICartRepository
    {
        Task<List<CartResponseDTO>> GetAll();
        Task<Giohang> AddCart(Giohang model);
        Task<Giohang> UpdateCart(int id, int Quantity);
        Task DeleteCart(int IdCart);
    }
}
