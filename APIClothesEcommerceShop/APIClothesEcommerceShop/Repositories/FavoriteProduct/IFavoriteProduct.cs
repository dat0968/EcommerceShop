using APIClothesEcommerceShop.DTO.FavoriteProduct;

namespace APIClothesEcommerceShop.Repositories.FavoriteProduct
{
    public interface IFavoriteProduct
    {
        Task<List<FavoritveResponsDTO>> GetFavoriteProducts(int idKhachHang);
        Task DeleteFavoriteProduct(int idKhachHang, int idSanPham);
        Task<FavoriteProductDTO> AddFavoriteProduct(FavoriteProductDTO fv);
    }
}
