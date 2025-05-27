using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Product
{
    public interface IProductRepository
    {
        Task<List<ProductResponseDTO>> GetAll();
        Task<ProductResponseDTO> GetById(int id);
        Task<Sanpham> Add(Sanpham model);
        Task<Sanpham> Update(Sanpham model);
        Task Cancel(int id);
    }
}
