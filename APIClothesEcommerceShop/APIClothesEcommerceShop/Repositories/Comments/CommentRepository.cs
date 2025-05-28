using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;

namespace APIClothesEcommerceShop.Repositories.Comments
{
    public class CommentRepository(EcommerceShopContext db) : Repository<Models.BinhLuan>(db), ICommentRepository
    {
        private readonly EcommerceShopContext _db = db;

        public Task AddCommentAsync(BinhLuan comment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCommentAsync(int commentId)
        {
            throw new NotImplementedException();
        }


        public Task<BinhLuan> GetCommentByIdAsync(int commentId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BinhLuan>> GetCommentsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCommentAsync(BinhLuan comment)
        {
            throw new NotImplementedException();
        }
    }
}