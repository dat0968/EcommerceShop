using System.Collections.Generic;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Comment;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Comments
{
    public interface ICommentRepository : IRepository<BinhLuan>
    {
        Task<List<CommentResponseDTO>> GetCommentsByProductIdAsync(int productId);
        Task<List<CommentResponseDTO>> GetCommentsByComboIdAsync(int comboId);
        Task AddCommentAsync(BinhLuan comment);
        Task UpdateCommentAsync(BinhLuan comment);
        Task DeleteCommentAsync(int commentId);
        Task<List<CommentResponseDTO>> GetCommentsByUserIdAsync(int userId);
        Task UpdateCommentStatusAsync(int commentId, bool trangThai, string? lyDoHuy);
        Task<IEnumerable<CommentResponseDTO>> GetAllCommentsForStaffAsync();
    }
}
