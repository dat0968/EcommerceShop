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
    public class ReviewRepository(EcommerceShopContext db) : Repository<DanhGia>(db), IReviewRepository
    {
        private readonly EcommerceShopContext _db = db;

        public Task AddReviewAsync(DanhGia review)
        {
            throw new NotImplementedException();
        }

        public Task DeleteReviewAsync(int reviewId)
        {
            throw new NotImplementedException();
        }


        public Task<DanhGia> GetReviewByIdAsync(int reviewId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DanhGia>> GetReviewsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateReviewAsync(DanhGia review)
        {
            throw new NotImplementedException();
        }
    }
}