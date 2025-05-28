using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public class ReviewRepository(EcommerceShopContext db) : Repository<BinhLuan>(db), IReviewRepository
    {
        private readonly EcommerceShopContext _db = db;

        public Task AddReviewAsync(BinhLuan review)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReviewAsync(int reviewId)
        {
            throw new NotImplementedException();
        }


        public Task<BinhLuan> GetReviewByIdAsync(int reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BinhLuan>> GetReviewsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateReviewAsync(BinhLuan review)
        {
            throw new NotImplementedException();
        }
    }
}