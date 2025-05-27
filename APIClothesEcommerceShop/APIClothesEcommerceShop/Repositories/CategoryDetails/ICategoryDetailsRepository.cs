using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.CategoryDetails
{
    public interface ICategoryDetailsRepository
    {
        Task<Chitietdanhmuc> Add(Chitietdanhmuc model);
        Task<Chitietdanhmuc> Update(Chitietdanhmuc model);
        Task Delete(int MaDanhMucCha, int MaDanhMucCon, int MaSp);
        Task<List<Chitietdanhmuc>> GetDetailCategoryByProductId(int model);
    }
}
