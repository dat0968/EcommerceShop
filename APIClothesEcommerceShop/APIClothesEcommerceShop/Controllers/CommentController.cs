using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Comment;
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
        [ProducesResponseType(typeof(ResponseAPI<IEnumerable<CommentResponseDTO>>), 200)]
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetCommentsByProductId(int productId)
        {
            // ResponseAPI<IEnumerable<CommentResponseDTO>> res = new();
            // var comments = await _unit.Comment.GetCommentsByProductIdAsync(productId);
            // if (comments == null)
            // {
            //     res.SetErrorResponse("No comments found for this product.");
            //     return NotFound(res);
            // }
            return Ok();
        }
    }
}