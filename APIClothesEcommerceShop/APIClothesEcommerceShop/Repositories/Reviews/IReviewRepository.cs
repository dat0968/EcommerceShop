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
        Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int productId);
        Task<ResponseAPI<Hoadon>> GetOrderWithDetailItemAndReviewByOrderIdAsync(int orderId, int userId);
        Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsOfItemByOrderIdAsync(int orderId, int userId);
        Task<ResponseAPI<ReviewResponseDTO>> AddReviewForItemInOrderAsync(ReviewRequestDTO entity);
        Task<ResponseAPI<string>> UpdateReviewAsync(ReviewRequestDTO entity);
        Task<ResponseAPI<string>> RemoveAsync(int reviewId, int userId);
    }
}