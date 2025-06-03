using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Comment;
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

        public Task<BinhLuan> GetCommentByIdAsync(int commentId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseAPI<IEnumerable<CommentResponseDTO>>> GetCommentsByProductIdAsync(int productId)
        {
            throw new NotImplementedException();

            // ResponseAPI<IEnumerable<CommentResponseDTO>> response = new();
            // try
            // {
            //     var comments = _db.BinhLuans
            //         .Where(c => c.IdSanPham == productId)
            //         .Select(c => new CommentResponseDTO
            //         {
            //             Id = c.Id,
            //             IdSanPham = c.IdSanPham,
            //             MaKh = c.MaKh,
            //             HoTen = c.HoTen,
            //             Email = c.Email,
            //             NoiDung = c.NoiDung,
            //             NgayBinhLuan = c.NgayBinhLuan,
            //             ParentId = c.ParentId
            //         })
            //         .ToList();

            //     response.SetSuccessResponse(data: comments);
            // }
            // catch (Exception ex)
            // {
            //     response.SetErrorResponse(ex.Message);
            // }

            // return Task.FromResult(response);
        }

        public Task UpdateCommentAsync(BinhLuan comment)
        {
            throw new NotImplementedException();
        }
    }
}