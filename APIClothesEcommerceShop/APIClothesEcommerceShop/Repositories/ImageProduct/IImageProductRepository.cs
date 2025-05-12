using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.ImageProduct
{
    public interface IImageProductRepository
    {
        Task<Hinhanh> Add(Hinhanh model);
        Task Delete(int id);
    }
}
