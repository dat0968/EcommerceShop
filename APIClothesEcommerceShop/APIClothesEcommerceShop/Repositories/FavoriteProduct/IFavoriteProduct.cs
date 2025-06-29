using APIClothesEcommerceShop.DTO.FavoriteProduct;

namespace APIClothesEcommerceShop.Repositories.FavoriteProduct
{
    public interface IFavoriteProduct
    {
        Task<List<FavoritveResponsDTO>> GetFavoriteProducts(int idKhachHang);
        Task DeleteFavoriteProduct(FavoriteProductDTO fv);
        Task<FavoriteProductDTO> AddFavoriteProduct(FavoriteProductDTO fv);
        Task<bool> CheckFavoriteProduct(FavoriteProductDTO fv);
    }
}
