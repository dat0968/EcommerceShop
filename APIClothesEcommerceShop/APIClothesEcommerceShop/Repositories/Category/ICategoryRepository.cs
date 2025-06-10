using APIClothesEcommerceShop.DTO.Category;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.DTO.Category.CategoryParent;
using APIClothesEcommerceShop.DTO.Category.CategoryChild;
using APIClothesEcommerceShop.DTO.Category.CategoryDetail;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository : IRepository<Danhmuccha>
    {
        Task<List<Danhmuccha>> GetCategories();
        Task<List<CategoryResponseDTO>> GetAllBigCategories();
        // Lấy toàn bộ dữ liệu 
        Task<ResponseAPI<List<CategoryResponseDTO>>> GetAllCategoriesAsync();
        // Danh mục cha
        Task<ResponseAPI<List<CategoryParentResponseDTO>>> GetCategoryParentAsync();
        Task<ResponseAPI<CategoryParentResponseDTO>> GetCategoryByIdAsync(int id);
        Task<ResponseAPI<CategoryParentResponseDTO>> UpsertCategoryAsync(int id, CategoryParentRequestDTO categoryDto);
        Task<ResponseAPI<dynamic>> DeleteCategoryAsync(int id);

        // Danh mục con
        Task<ResponseAPI<List<CategoryChildResponseDTO>>> GetAllSubCategoriesAsync();
        Task<ResponseAPI<CategoryChildResponseDTO>> GetSubCategoryByIdAsync(int id);
        Task<ResponseAPI<CategoryChildResponseDTO>> UpsertSubCategoryAsync(int id, CategoryChildRequestDTO subCategoryDto);
        Task<ResponseAPI<dynamic>> DeleteSubCategoryAsync(int id);

        // Chi tiết danh mục
        Task<ResponseAPI<List<CategoryDetailResponseDTO>>> GetAllCategoryDetailsAsync();
        Task<ResponseAPI<CategoryDetailResponseDTO>> GetCategoryDetailByIdAsync(int maDanhMucCha, int maDanhMucCon, int maSp);
        Task<ResponseAPI<CategoryDetailResponseDTO>> UpsertCategoryDetailAsync(CategoryDetailRequestDTO detailDto);
        Task<ResponseAPI<dynamic>> DeleteCategoryDetailAsync(int maDanhMucCha, int maDanhMucCon, int maSp);

    }
}
