using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Reviews;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IUnitOfWork _unit;
        public ReviewController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        /// <summary>
        /// Lấy danh sách đánh giá của sản phẩm theo mã sản phẩm.
        /// </summary>
        /// <param name="productId">Mã sản phẩm</param>
        /// <returns>Danh sách đánh giá sản phẩm</returns>
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            var res = await _unit.Review.GetReviewsByProductIdAsync(productId);
            return Ok(res);
        }

        /// <summary>
        /// Lấy danh sách đánh giá của combo theo mã combo.
        /// </summary>
        /// <param name="comboId">Mã combo</param>
        /// <returns>Danh sách đánh giá combo</returns>
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        [HttpGet("combos/{comboId}")]
        public async Task<IActionResult> GetReviewsByComboId(int comboId)
        {
            var res = await _unit.Review.GetReviewsByComboIdAsync(comboId);
            return Ok(res);
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng cùng đánh giá của người dùng theo mã đơn hàng (chỉ cho khách hàng).
        /// </summary>
        /// <param name="orderId">Mã đơn hàng</param>
        /// <returns>Chi tiết đơn hàng và đánh giá</returns>
        [ProducesResponseType(typeof(ResponseAPI<OrderWithReview>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderWithReviews(int orderId = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.GetOrderWithDetailItemAndReviewByOrderIdAsync(orderId, userId);
            return Ok(res);
        }

        /// <summary>
        /// Lấy danh sách đánh giá của khách hàng dựa vào số liệu đơn hàng của khách
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ResponseAPI<Dictionary<string, List<ReviewResponseDTO>>>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllReviewOfUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.GetAllReviewOfUser(userId);
            return Ok(res);
        }

        /// <summary>
        /// Thêm mới đánh giá cho sản phẩm hoặc combo (chỉ cho khách hàng).
        /// </summary>
        /// <param name="review">Thông tin đánh giá</param>
        /// <param name="isProduct">Đánh giá cho sản phẩm hay combo</param>
        /// <returns>Kết quả thêm đánh giá</returns>
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> AddReview([FromForm] ReviewRequestDTO review, bool isProduct = true)
        {
            if (review == null)
            {
                return BadRequest(new ResponseAPI<ReviewResponseDTO>
                {
                    Success = false,
                    Message = "Invalid review data."
                });
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            review.MaKh = userId;

            var res = await _unit.Review.AddReviewForItemInOrderAsync(review, isProduct);
            return Ok(res);
        }

        /// <summary>
        /// Cập nhật đánh giá cho sản phẩm hoặc combo (chỉ cho khách hàng).
        /// </summary>
        /// <param name="review">Thông tin đánh giá cần cập nhật</param>
        /// <param name="isProduct">Đánh giá cho sản phẩm hay combo</param>
        /// <returns>Kết quả cập nhật đánh giá</returns>
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPut]
        public async Task<IActionResult> UpdateReview([FromForm] ReviewRequestDTO review, bool isProduct = true)
        {
            if (review == null)
            {
                return BadRequest(new ResponseAPI<ReviewResponseDTO>
                {
                    Success = false,
                    Message = "Invalid review data."
                });
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            review.MaKh = userId;

            var res = await _unit.Review.UpdateReviewAsync(review, isProduct);
            return Ok(res);
        }

        /// <summary>
        /// Xóa đánh giá của người dùng (chỉ cho khách hàng).
        /// </summary>
        /// <param name="reviewId">Mã đánh giá</param>
        /// <returns>Kết quả xóa đánh giá</returns>
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.RemoveAsync(reviewId, userId);
            return Ok(res);
        }

        /// <summary>
        /// Lấy tất cả đánh giá chi tiết (dành cho nhân viên/quản trị).
        /// </summary>
        /// <returns>Danh sách đánh giá chi tiết</returns>
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewDetailDTO>>), 200)]
        [Authorize(Roles = "Admin,Nhân viên")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReview()
        {
            var reviews = await _unit.Review.GetAllReviewDtoAsync();
            return Ok(reviews);
        }

        /// <summary>
        /// Phản hồi đánh giá của khách hàng (dành cho nhân viên/quản trị).
        /// </summary>
        /// <param name="request">Thông tin phản hồi và danh sách mã đánh giá</param>
        /// <returns>Kết quả phản hồi</returns>
        [ProducesResponseType(typeof(ResponseAPI<string>), 200)]
        [Authorize(Roles = "Admin,Nhân viên")]
        [HttpPut("shop-response")]
        public async Task<IActionResult> ResponseToReview([FromBody] ReviewReplyRequestDTO request)
        {
            if (request.ListId.Length == 0 || string.IsNullOrEmpty(request.ResponseContent))
            {
                return BadRequest(new ResponseAPI<string>
                {
                    Success = false,
                    Message = "Yêu cầu dữ liệu không hợp lệ."
                });
            }

            var res = await _unit.Review.UpdateShopReplyAsync(request);
            return Ok(res);
        }
    }
}