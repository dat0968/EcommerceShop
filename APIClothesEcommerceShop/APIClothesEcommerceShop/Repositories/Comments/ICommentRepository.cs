using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Comments
{
    public interface ICommentRepository : IRepository<Models.BinhLuan>
    {
        Task<IEnumerable<Models.BinhLuan>> GetCommentsByProductIdAsync(int productId);
        Task<Models.BinhLuan> GetCommentByIdAsync(int commentId);
        Task AddCommentAsync(Models.BinhLuan comment);
        Task UpdateCommentAsync(Models.BinhLuan comment);
        Task DeleteCommentAsync(int commentId);
    }
}