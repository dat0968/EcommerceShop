using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
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
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetReviewsByProductId(int productId)
        {
            var res = new ResponseAPI<DanhGia>();
            try
            {
                var reviews = await _unit.Review.GetAsync(x => x.IdSanPham == productId);
                if (reviews == null)
                {
                    res.SetErrorResponse("No reviews found for this product.");
                    return NotFound(res);
                }
                res.SetSuccessResponse(data: reviews);
                return Ok(res);
            }
            catch (Exception ex)
            {
                res.SetErrorResponse(ex.Message);
                return BadRequest(res);
            }
        }
    }
}