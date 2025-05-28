using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Wishlists
{
    public interface IWishlistRepository : IRepository<Models.YeuThich>
    {
        Task<IEnumerable<Models.YeuThich>> GetWishlistsByUserIdAsync(int userId);
        Task<Models.YeuThich> GetWishlistByIdAsync(int wishlistId);
        Task AddWishlistAsync(Models.YeuThich wishlist);
        Task UpdateWishlistAsync(Models.YeuThich wishlist);
        Task DeleteWishlistAsync(int wishlistId);
    }
}