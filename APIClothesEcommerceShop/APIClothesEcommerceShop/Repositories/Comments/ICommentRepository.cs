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
    }
}
