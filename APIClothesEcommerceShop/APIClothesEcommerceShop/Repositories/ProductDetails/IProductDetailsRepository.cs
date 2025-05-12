using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.ProductDetails
{
    public interface IProductDetailsRepository
    {
        Task<Chitietsanpham> Add(Chitietsanpham model);
        Task<Chitietsanpham> Update(Chitietsanpham model);
        Task Cancel(int id);
    }
}
