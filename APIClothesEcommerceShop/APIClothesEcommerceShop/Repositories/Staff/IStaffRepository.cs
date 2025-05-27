using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Staff;
using APIClothesEcommerceShop.Models;
namespace APIClothesEcommerceShop.Repositories.Staff
{
    public interface IStaffRepository
    {
        Task<List<StaffDto>> GetAllStaffAsync(int pageSize, int pageNumber, string hoTen, string gioiTinh, string tinhTrang);
        Task<int> GetStaffCountAsync(string hoTen = null, string gioiTinh = null, string tinhTrang = null);
        Task<int> GetSearchCountAsync(string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null);
        Task<StaffDto> GetStaffByIdAsync(int maNV);
        Task<ValidationResult> AddStaffAsync(StaffDto staffDto);
        Task<bool> IsTenTaiKhoanExistsAsync(string tenTaiKhoan);
        Task<bool> IsCccdExistsAsync(string cccd);
        Task<bool> IsSdtExistsAsync(string sdt);
        Task<bool> IsEmailExistsAsync(string email);
        Task<List<StaffDto>> SearchStaffAsync(int pageSize, int pageNumber, string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null);
        Task<ValidationResult> UpdateStaffAsync(int maNV, StaffDto staffDto);
        Task<ValidationResult> DeleteStaffAsync(int maNV);
        Task<List<StaffExportDto>> GetStaffForExportAsync();
        Task<List<Chucvu>> GetAllChucvusAsync();
    }
}