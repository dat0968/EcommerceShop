using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            try
            {
                var comments = await _unit.Comment.GetAsync(x => x.IdSanPham == productId);
                if (comments == null)
                {
                    return NotFound(new { Success = false, Message = "No comments found for this product." });
                }
                return Ok(new { Success = true, Data = comments });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}