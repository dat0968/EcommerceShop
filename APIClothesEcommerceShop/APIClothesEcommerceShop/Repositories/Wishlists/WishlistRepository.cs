using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Wishlists
{
    public class WishlistRepository(EcommerceShopContext db) : Repository<Models.YeuThich>(db), IWishlistRepository
    {
        private readonly EcommerceShopContext _db = db;

        public Task AddWishlistAsync(YeuThich wishlist)
        {
            throw new NotImplementedException();
        }

        public Task DeleteWishlistAsync(int wishlistId)
        {
            throw new NotImplementedException();
        }

        public Task<YeuThich> GetWishlistByIdAsync(int wishlistId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<YeuThich>> GetWishlistsByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWishlistAsync(YeuThich wishlist)
        {
            throw new NotImplementedException();
        }
    }
}