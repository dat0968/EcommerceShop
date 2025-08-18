using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Comment;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Comments;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/staff-comments")]
    [ApiController]
    [Authorize(Roles = "Admin, Staff")]
    public class StaffCommentsController : ControllerBase
    {
        private readonly IUnitOfWork _unit;

        public StaffCommentsController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        // GET: api/staff-comments
        [HttpGet]
        public async Task<IActionResult> GetComments()
        {
            var comments = await _unit.Comment.GetAllCommentsForStaffAsync();
            return Ok(new { Success = true, Data = comments });
        }

        // PUT: api/staff-comments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommentStatus(int id, [FromBody] UpdateCommentStatusDto dto)
        {
            // Note: Depending on the Unit of Work implementation, SaveChanges might be needed here.
            // Assuming it's handled at a higher level for now.
            await _unit.Comment.UpdateCommentStatusAsync(id, dto.TrangThai, dto.LyDoHuy);
            return Ok(new { Success = true, Message = "Comment status updated successfully." });
        }
    }
}
