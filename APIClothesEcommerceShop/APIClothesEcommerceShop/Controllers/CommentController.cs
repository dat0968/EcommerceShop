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
    public class CommentController : ControllerBase
    {
        private readonly IUnitOfWork _unit;
        public CommentController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetCommentsByProductId(int productId)
        {
            ResponseAPI<BinhLuan> res = new();
            try
            {
                var comments = await _unit.Comment.GetAsync(x => x.IdSanPham == productId);
                if (comments == null)
                {
                    res.SetErrorResponse("No comments found for this product.");
                    return NotFound(res);
                }
                res.SetSuccessResponse(data: comments);
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