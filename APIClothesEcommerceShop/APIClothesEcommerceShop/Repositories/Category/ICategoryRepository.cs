using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository : IRepository<Danhmuccha>
    {
        // Danh mục cha
        Task<ResponseAPI<List<Danhmuccha>>> GetAllCategoriesAsync();
        Task<ResponseAPI<Danhmuccha>> GetCategoryByIdAsync(int id);
        Task<ResponseAPI<Danhmuccha>> UpsertCategoryAsync(Danhmuccha category);
        Task<ResponseAPI<dynamic>> DeleteCategoryAsync(int id);

        // Danh mục con
        Task<ResponseAPI<List<Danhmuccon>>> GetAllSubCategoriesAsync();
        Task<ResponseAPI<Danhmuccon>> GetSubCategoryByIdAsync(int id);
        Task<ResponseAPI<Danhmuccon>> UpsertSubCategoryAsync(Danhmuccon subCategory);
        Task<ResponseAPI<dynamic>> DeleteSubCategoryAsync(int id);

        // Chi tiết danh mục
        Task<ResponseAPI<List<Chitietdanhmuc>>> GetAllCategoryDetailsAsync();
        Task<ResponseAPI<Chitietdanhmuc>> GetCategoryDetailByIdAsync(int maDanhMucCha, int maDanhMucCon, int maSp);
        Task<ResponseAPI<Chitietdanhmuc>> UpsertCategoryDetailAsync(Chitietdanhmuc detail);
        Task<ResponseAPI<dynamic>> DeleteCategoryDetailAsync(int maDanhMucCha, int maDanhMucCon, int maSp);

    }
}
