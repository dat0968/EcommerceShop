using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Comment;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
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
            var response = new ResponseAPI<List<CommentResponseDTO>>();
            try
            {
                var comments = await _unit.Comment.GetCommentsByProductIdAsync(productId);
                response.SetSuccessResponse(data: comments, message: "Bình luận lấy lại thành công ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.SetErrorResponse(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpGet("combo/{comboId}")]
        public async Task<IActionResult> GetCommentsByComboId(int comboId)
        {
            var response = new ResponseAPI<List<CommentResponseDTO>>();
            try
            {
                var comments = await _unit.Comment.GetCommentsByComboIdAsync(comboId);
                response.SetSuccessResponse(data: comments, message: "Bình luận lấy lại thành công");
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.SetErrorResponse(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] CommentRequestDTO commentDto)
        {
            var response = new ResponseAPI<string>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    response.SetErrorResponse("Người dùng không tìm thấy");
                    return Unauthorized(response);
                }

                var comment = new BinhLuan
                {
                    MaKh = int.Parse(userId),
                    NoiDung = commentDto.NoiDung,
                    ParentId = commentDto.ParentId ?? 0,
                    NgayBinhLuan = DateTime.UtcNow
                };

                if (commentDto.MaSP.HasValue)
                {
                    comment.IdSanPham = commentDto.MaSP.Value;
                }
                else if (commentDto.MaCombo.HasValue)
                {
                    comment.IdCombo = commentDto.MaCombo.Value;
                }
                else
                {
                    response.SetErrorResponse("Hiện không thể bình luận.");
                    Console.WriteLine("Không tìm thấy thông số Id");
                    return BadRequest(response);
                }

                await _unit.Comment.AddCommentAsync(comment);
                await _unit.SaveAsync();

                response.SetSuccessResponse("Bình luận được thêm thành công");
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.SetErrorResponse(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CommentRequestDTO commentDto)
        {
            var response = new ResponseAPI<string>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    response.SetErrorResponse("Người dùng không tìm thấy");
                    return Unauthorized(response);
                }

                var existingComment = await _unit.Comment.GetAsync(c => c.Id == commentId);
                if (existingComment == null)
                {
                    response.SetErrorResponse("Bình luận không tìm thấy");
                    return NotFound(response);
                }

                if (existingComment.MaKh != int.Parse(userId) && !User.IsInRole("Admin"))
                {
                    response.SetErrorResponse("Bạn không được phép cập nhật bình luận này");
                    return Forbid();
                }

                existingComment.NoiDung = commentDto.NoiDung;

                await _unit.Comment.UpdateCommentAsync(existingComment);
                await _unit.SaveAsync();

                response.SetSuccessResponse("Bình luận cập nhật thành công");
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.SetErrorResponse(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var response = new ResponseAPI<string>();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    response.SetErrorResponse("Người dùng không tìm thấy");
                    return Unauthorized(response);
                }

                var existingComment = await _unit.Comment.GetAsync(c => c.Id == commentId);
                if (existingComment == null)
                {
                    response.SetErrorResponse("Bình luận không tìm thấy");
                    return NotFound(response);
                }

                if (existingComment.MaKh != int.Parse(userId) && !User.IsInRole("Admin"))
                {
                    response.SetErrorResponse("Bạn không được phép xóa bình luận này");
                    return Forbid();
                }

                await _unit.Comment.DeleteCommentAsync(commentId);
                await _unit.SaveAsync();

                response.SetSuccessResponse("Bình luận đã xóa thành công");
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.SetErrorResponse(ex.Message);
                return BadRequest(response);
            }
        }
    }
}