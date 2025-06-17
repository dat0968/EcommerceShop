using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Address
{
    public interface IAddressRepository
    {
        Task<List<Diachi>> GetAll(int MaKh);
        Task Delete(int id);
    }
}
