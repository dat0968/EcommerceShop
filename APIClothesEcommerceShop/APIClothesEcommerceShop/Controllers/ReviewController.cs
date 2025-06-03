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
        [HttpGet("{productId}")]
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
        [ProducesResponseType(typeof(ResponseAPI<Hoadon>), 200)]
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderWithReviews(int orderId = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var res = await _unit.Review.GetOrderWithDetailItemAndReviewByOrderIdAsync(orderId, userId);
            /* if (res.Data == null || !res.Data.Any())
            {
                res.SetErrorResponse("No reviews found.");
                return NotFound(res);
            } */
            return Ok(res);
        }
        // [ProducesResponseType(typeof(ResponseAPI<IEnumerable<ReviewResponseDTO>>), 200)]
        // [HttpGet]
        // public async Task<IActionResult> GetAllReviews(int orderId = 0)
        // {
        //     var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        //     var res = await _unit.Review.GetReviewsOfItemByOrderIdAsync(orderId, userId);
        //     /* if (res.Data == null || !res.Data.Any())
        //     {
        //         res.SetErrorResponse("No reviews found.");
        //         return NotFound(res);
        //     } */
        //     return Ok(res);
        // }
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewRequestDTO review)
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

            var res = await _unit.Review.AddReviewForItemInOrderAsync(review);
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }

        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPut]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewRequestDTO review)
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

            var res = await _unit.Review.UpdateReviewAsync(review);
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
    }
}