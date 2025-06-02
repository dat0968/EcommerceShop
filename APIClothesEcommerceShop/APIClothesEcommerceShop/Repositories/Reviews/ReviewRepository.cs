using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Reviews;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Utils;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public class ReviewRepository : Repository<DanhGia>, IReviewRepository
    {
        private readonly EcommerceShopContext _db;

        public ReviewRepository(EcommerceShopContext db) : base(db)
        {
            _db = db;
        }

        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int productId, int? userId = null)
        {
            var response = new ResponseAPI<IEnumerable<ReviewResponseDTO>>();
            try
            {
                IQueryable<DanhGia> query = _db.DanhGias.Where(r => r.IdSanPham == productId);

                if (userId.HasValue)
                {
                    query = query.Where(r => r.IdKhachHang == userId.Value);
                }

                var reviews = await query.Select(r => r.ToReviewResponseDTO()).ToListAsync();

                response.SetSuccessResponse(data: reviews, message: "Lấy danh sách đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [Upsert Review]
        public async Task<ResponseAPI<ReviewResponseDTO>> UpsertReviewAsync(ReviewRequestDTO review, int userId)
        {
            var response = new ResponseAPI<ReviewResponseDTO>();
            try
            {
                var existingReview = await _db.DanhGias.FirstOrDefaultAsync(r => r.IdKhachHang == userId && r.IdSanPham == review.IdSanPham);
                if (existingReview != null)
                {
                    UpdateExistingReview(existingReview, review);
                    await _db.SaveChangesAsync();

                    response.Data = existingReview.ToReviewResponseDTO();
                    response.SetSuccessResponse(data: response.Data, message: "Cập nhật đánh giá thành công");
                }
                else
                {
                    review.IdKhachHang = userId; // Gán IdKhachHang từ userId
                    review.NgayDanhGia = DateTime.Now; // Gán ngày đánh giá hiện tại
                    var newReview = review.ToDanhGia();
                    await _db.DanhGias.AddAsync(newReview);
                    await _db.SaveChangesAsync();

                    response.Data = newReview.ToReviewResponseDTO();
                    response.SetSuccessResponse(data: response.Data, message: "Thêm đánh giá thành công");
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private void UpdateExistingReview(DanhGia existingReview, ReviewRequestDTO review)
        {
            existingReview.HoTen = review.HoTen;
            existingReview.Email = review.Email;
            existingReview.NoiDung = review.NoiDung;
            existingReview.SoSao = review.SoSao;
            existingReview.NgayDanhGia = DateTime.Now; // không nhất thiết phải có IdKhachHang hay IdSanPham ở đây nữa
        }
        #endregion

        public async Task<ResponseAPI<string>> DeleteReviewAsync(int productId, int userId)
        {
            var response = new ResponseAPI<string>();
            try
            {
                var review = await _db.DanhGias.FirstOrDefaultAsync(x => x.IdSanPham == productId && x.IdKhachHang == userId);
                if (review == null)
                {
                    throw new KeyNotFoundException("Đánh giá không tồn tại");
                }

                _db.DanhGias.Remove(review);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(message: "Xóa đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
    }
}
