using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public interface IReviewRepository : IRepository<DanhGia>
    {
        Task<IEnumerable<DanhGia>> GetReviewsByProductIdAsync(int productId);
        Task<DanhGia> GetReviewByIdAsync(int reviewId);
        Task AddReviewAsync(DanhGia review);
        Task UpdateReviewAsync(DanhGia review);
        Task DeleteReviewAsync(int reviewId);
    }
}