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
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            var res = await _unit.Review.GetReviewsByProductIdAsync(productId);
            if (res.Data == null)
            {
                res.SetErrorResponse("No reviews found for this product.");
                return NotFound(res);
            }
            return Ok(res);
        }

        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        [HttpGet("combos/{comboId}")]
        public async Task<IActionResult> GetReviewsByComboId(int comboId)
        {
            var res = await _unit.Review.GetReviewsByComboIdAsync(comboId);
            if (res.Data == null)
            {
                res.SetErrorResponse("No reviews found for this product.");
                return NotFound(res);
            }
            return Ok(res);
        }
        [ProducesResponseType(typeof(ResponseAPI<OrderWithReview>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderWithReviews(int orderId = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.GetOrderWithDetailItemAndReviewByOrderIdAsync(orderId, userId);
            return Ok(res);
        }
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequestDTO review, bool isProduct = true)
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
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }

        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPut]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewRequestDTO review, bool isProduct = true)
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
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }

        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteReview(int productId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.RemoveAsync(productId, userId);
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }

        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllReview()
        {
            var reviews = await _unit.Review.GetAllReviewDtoAsync();
            return Ok(reviews);
        }

        [HttpPut("shop-response")]
        public async Task<IActionResult> ResponseToReview([FromBody] RequestReplyRequestDTO request)
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
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }
    }
}