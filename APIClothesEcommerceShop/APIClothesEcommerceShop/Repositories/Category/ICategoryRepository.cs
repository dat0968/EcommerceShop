using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Categories;
using APIClothesEcommerceShop.DTO.Categories.CategoryChild;
using APIClothesEcommerceShop.DTO.Categories.CategoryDetail;
using APIClothesEcommerceShop.DTO.Categories.CategoryParent;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Category
{
    public interface ICategoryRepository : IRepository<Danhmuccha>
    {
        // Danh mục cha
        Task<ResponseAPI<List<CategoryResponseDTO>>> GetAllCategoriesAsync();
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
