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
    public class ReviewRepository(EcommerceShopContext db) : Repository<DanhGia>(db), IReviewRepository
    {
        private readonly EcommerceShopContext _db = db;

        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int productId, int? userId = null)
        {
            ResponseAPI<IEnumerable<ReviewResponseDTO>> response = new();
            try
            {
                IQueryable<DanhGia> query = _db.DanhGias.Where(r => r.IdSanPham == productId);

                if (userId.HasValue)
                {
                    query = query.Where(r => r.IdKhachHang == userId.Value);
                }

                IEnumerable<ReviewResponseDTO> reviews = await query.Select(r => r.ToReviewResponseDTO()).ToListAsync();

                response.SetSuccessResponse(data: reviews, message: "Lấy danh sách đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            finally
            {

            }
            return response;
        }
        public async Task<ResponseAPI<ReviewResponseDTO>> AddReviewAsync(ReviewRequestDTO review)
        {
            ResponseAPI<ReviewResponseDTO> response = new();
            try
            {
                DanhGia danhGia = review.ToDanhGia();
                await _db.DanhGias.AddAsync(danhGia);
                await _db.SaveChangesAsync();

                response.Data = danhGia.ToReviewResponseDTO();
                response.SetSuccessResponse(data: danhGia.ToReviewResponseDTO(), message: "Thêm đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            finally
            {

            }
            return response;
        }
        public async Task<ResponseAPI<ReviewResponseDTO>> UpdateReviewAsync(ReviewRequestDTO review)
        {
            ResponseAPI<ReviewResponseDTO> response = new();
            try
            {
                DanhGia? existingReview = await _db.DanhGias.FindAsync(review.Id);
                if (existingReview == null)
                {
                    throw new KeyNotFoundException("Đánh giá không tồn tại");
                }

                existingReview.IdKhachHang = review.IdKhachHang;
                existingReview.IdSanPham = review.IdSanPham;
                existingReview.HoTen = review.HoTen;
                existingReview.Email = review.Email;
                existingReview.NoiDung = review.NoiDung;
                existingReview.SoSao = review.SoSao;
                existingReview.NgayDanhGia = DateTime.Now;

                _db.DanhGias.Update(existingReview);
                await _db.SaveChangesAsync();

                response.Data = existingReview.ToReviewResponseDTO();
                response.SetSuccessResponse(data: existingReview.ToReviewResponseDTO(), message: "Cập nhật đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            finally
            {

            }
            return response;
        }
        public async Task<ResponseAPI<string>> DeleteReviewAsync(int reviewId)
        {
            ResponseAPI<string> response = new();
            try
            {
                DanhGia? review = await _db.DanhGias.FindAsync(reviewId);
                if (review == null)
                {
                    throw new KeyNotFoundException("Đánh giá không tồn tại");
                }

                _db.DanhGias.Remove(review);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(message: "Xóa đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            finally
            {

            }
            return response;
        }
    }
}