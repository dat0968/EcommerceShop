using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Reviews;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public interface IReviewRepository : IRepository<DanhGia>
    {
        Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int productId, int? userId = null);
        Task<ResponseAPI<ReviewResponseDTO>> UpsertReviewAsync(ReviewRequestDTO review, int userId);
        Task<ResponseAPI<string>> DeleteReviewAsync(int reviewId, int userId);
    }
}