using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Comment;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Comments
{
    public interface ICommentRepository : IRepository<Models.BinhLuan>
    {
        Task<ResponseAPI<IEnumerable<CommentResponseDTO>>> GetCommentsByProductIdAsync(int productId);
        Task<Models.BinhLuan> GetCommentByIdAsync(int commentId);
        Task AddCommentAsync(Models.BinhLuan comment);
        Task UpdateCommentAsync(Models.BinhLuan comment);
    }
}