using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Comment;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Comments
{
    public class CommentRepository : Repository<BinhLuan>, ICommentRepository
    {
        private readonly EcommerceShopContext _db;

        public CommentRepository(EcommerceShopContext db) : base(db)
        {
            _db = db;
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByProductIdAsync(int productId)
        {
            var comments = await _db.BinhLuan
                .Where(c => c.IdSanPham == productId)
                .Include(c => c.Khachhang)
                .OrderByDescending(c => c.NgayBinhLuan)
                .ToListAsync();

            var commentViewModels = comments.Select(c => new CommentResponseDTO
            {
                Id = c.Id,
                MaSP = c.IdSanPham,
                MaCombo = null, // Explicitly null for product comments
                MaKh = c.MaKh,
                HoTen = c.Khachhang?.HoTen ?? "Anonymous",
                Avatar = c.Khachhang?.HinhDaiDien ?? "",
                NoiDung = c.NoiDung,
                NgayBinhLuan = c.NgayBinhLuan,
                ParentId = c.ParentId == 0 ? null : c.ParentId,
            }).ToList();

            var nestedComments = new List<CommentResponseDTO>();
            var commentLookup = commentViewModels.ToDictionary(c => c.Id);

            foreach (var comment in commentViewModels)
            {
                if (comment.ParentId.HasValue && commentLookup.TryGetValue(comment.ParentId.Value, out var parentComment))
                {
                    parentComment.Replies.Add(comment);
                }
                else
                {
                    nestedComments.Add(comment);
                }
            }

            return nestedComments;
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByComboIdAsync(int comboId)
        {
            var comments = await _db.BinhLuan
                .Where(c => c.IdCombo == comboId) // Filter by IdCombo
                .Include(c => c.Khachhang)
                .OrderByDescending(c => c.NgayBinhLuan)
                .ToListAsync();

            var commentViewModels = comments.Select(c => new CommentResponseDTO
            {
                Id = c.Id,
                MaSP = null, // Explicitly null for combo comments
                MaCombo = c.IdCombo, // Map IdCombo
                MaKh = c.MaKh,
                HoTen = c.Khachhang?.HoTen ?? "Anonymous",
                Avatar = c.Khachhang?.HinhDaiDien ?? "",
                NoiDung = c.NoiDung,
                NgayBinhLuan = c.NgayBinhLuan,
                ParentId = c.ParentId == 0 ? null : c.ParentId,
            }).ToList();

            var nestedComments = new List<CommentResponseDTO>();
            var commentLookup = commentViewModels.ToDictionary(c => c.Id);

            foreach (var comment in commentViewModels)
            {
                if (comment.ParentId.HasValue && commentLookup.TryGetValue(comment.ParentId.Value, out var parentComment))
                {
                    parentComment.Replies.Add(comment);
                }
                else
                {
                    nestedComments.Add(comment);
                }
            }

            return nestedComments;
        }

        public async Task AddCommentAsync(BinhLuan comment)
        {
            await _db.BinhLuan.AddAsync(comment);
        }

        public async Task UpdateCommentAsync(BinhLuan comment)
        {
            _db.BinhLuan.Update(comment);
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _db.BinhLuan.FindAsync(commentId);
            if (comment != null)
            {
                _db.BinhLuan.Remove(comment);
            }
        }
    }
}