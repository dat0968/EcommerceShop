using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Customer;

namespace APIClothesEcommerceShop.Repositories.Customer
{
    public interface ICustomerRepository
    {
        Task<List<CustomerDto>> GetAllCustomersAsync(int pageSize, int pageNumber, string hoTen, string gioiTinh, string tinhTrang);
        Task<int> GetCustomersCountAsync(string hoTen = null, string gioiTinh = null, string tinhTrang = null);
        Task<int> GetSearchCountAsync(string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null);
        Task<CustomerDto> GetCustomerByIdAsync(int maKH);
        Task<ValidationResult> AddCustomerAsync(CustomerDto customerDto);
        Task<bool> IsTenTaiKhoanExistsAsync(string tenTaiKhoan);
        Task<bool> IsCccdExistsAsync(string cccd);
        Task<bool> IsSdtExistsAsync(string sdt);
        Task<bool> IsEmailExistsAsync(string email);
        Task<List<CustomerDto>> SearchCustomersAsync(int pageSize, int pageNumber, string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null);
        Task<ValidationResult> UpdateCustomerAsync(int maKH, CustomerDto customerDto);
        Task<ValidationResult> DeleteCustomerAsync(int maKH);
        Task<List<CustomerExportDto>> GetCustomersForExportAsync();
    }
}