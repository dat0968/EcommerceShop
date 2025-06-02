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
        [ProducesResponseType(typeof(ResponseAPI<ReviewResponseDTO>), 200)]
        [Authorize(Roles = "Customer")]
        [HttpPost("{productId}")]
        public async Task<IActionResult> UpsertReview(int productId, [FromBody] ReviewRequestDTO review)
        {
            if (review == null || review.IdSanPham != productId)
            {
                return BadRequest(new ResponseAPI<ReviewResponseDTO>
                {
                    Success = false,
                    Message = "Invalid review data."
                });
            }

            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            var res = await _unit.Review.UpsertReviewAsync(review, userId);
            if (!res.Success)
            {
                return StatusCode(res.StatusCode, res);
            }
            return Ok(res);
        }
    }
}