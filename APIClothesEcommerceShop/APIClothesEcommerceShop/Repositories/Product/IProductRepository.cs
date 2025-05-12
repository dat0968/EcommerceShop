using APIClothesEcommerceShop.DTO.Product;

namespace APIClothesEcommerceShop.Repositories.Product
{
    public interface IProductRepository
    {
        Task<ProductResponseDTO> GetAll();
        Task<ProductResponseDTO> GetById(int id);
        Task<ProductResponseDTO> Add(AddProductResquestDTO model);
        Task<ProductResponseDTO> Update(UpdateProductResquestDTO model);
        Task Cancel(int id);
    }
}
