using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public interface IReviewRepository : IRepository<BinhLuan>
    {
        Task<IEnumerable<BinhLuan>> GetReviewsByProductIdAsync(int productId);
        Task<BinhLuan> GetReviewByIdAsync(int reviewId);
        Task AddReviewAsync(BinhLuan review);
        Task UpdateReviewAsync(BinhLuan review);
        Task DeleteReviewAsync(int reviewId);
    }
}