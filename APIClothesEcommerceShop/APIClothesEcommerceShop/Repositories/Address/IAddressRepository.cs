using APIClothesEcommerceShop.DTO.Addresses;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Address
{
    public interface IAddressRepository
    {
        Task<Diachi?> UpdateDefaultAddress(int? id, bool defaultAddress);
        Task<IEnumerable<AddressesResponseDTO>> GetByCustomerAsync(int maKh);
        Task<Diachi> AddAsync(Diachi diachi);
        Task<Diachi?> UpdateAsync(Diachi diachi);
        Task<bool> DeleteAsync(int id);
    }
}
