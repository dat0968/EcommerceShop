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

        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int productId)
        {
            ResponseAPI<IEnumerable<ReviewResponseDTO>> response = new();
            try
            {
                if (productId <= 0)
                {
                    throw new ArgumentException("Mã sản phẩm không hợp lệ");
                }

                var reviews = await _db.DanhGias
                    .Where(r => r.MaCtsp == productId)
                    .Select(r => r.ToReviewResponseDTO())
                    .ToListAsync();

                if (reviews.Count == 0)
                {
                    throw new KeyNotFoundException("Không có đánh giá nào cho sản phẩm này");
                }

                response.SetSuccessResponse(data: reviews, message: "Lấy danh sách đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsOfItemByOrderIdAsync(int orderId, int userId)
        {
            ResponseAPI<IEnumerable<ReviewResponseDTO>> response = new();
            try
            {
                if (orderId == 0)
                {
                    throw new ArgumentException("Thông số dữ liệu đơn hàng không hợp lệ");
                }

                if (userId == 0)
                {
                    throw new ArgumentException("Thông số dữ liệu người dùng không hợp lệ");
                }

                Hoadon? hoadon = await _db.Hoadons
                    .Include(h => h.Cthoadons)
                    .FirstOrDefaultAsync(h => h.MaHd == orderId && h.MaKh == userId);
                if (hoadon == null)
                {
                    throw new KeyNotFoundException("Hóa đơn không tồn tại hoặc không thuộc về người dùng này");
                }
                // Lấy danh sách sản phẩm trong hóa đơn
                var productIds = hoadon.Cthoadons.Select(ct => ct.MaCtsp).ToList();
                if (productIds.Count == 0)
                {
                    throw new KeyNotFoundException("Hóa đơn không có sản phẩm nào");
                }
                // Lấy danh sách đánh giá của các sản phẩm trong hóa đơn
                var reviews = await _db.DanhGias
                    .Where(r => productIds.Contains(r.MaCtsp) && r.MaKh == userId)
                    .Select(r => r.ToReviewResponseDTO())
                    .ToListAsync();
                if (reviews.Count == 0)
                {
                    throw new KeyNotFoundException("Không có đánh giá nào cho các sản phẩm trong hóa đơn này");
                }
                response.SetSuccessResponse(data: reviews, message: "Lấy danh sách đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<ReviewResponseDTO>> AddReviewForItemInOrderAsync(ReviewRequestDTO entity)
        {
            ResponseAPI<ReviewResponseDTO> response = new();
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity), "Đánh giá không được để trống");
                }

                if (entity.MaKh <= 0)
                {
                    throw new ArgumentException("Mã khách hàng không hợp lệ");
                }

                if (entity.MaHd <= 0)
                {
                    throw new ArgumentException("Mã hóa đơn không hợp lệ");
                }

                if (string.IsNullOrWhiteSpace(entity.NoiDung))
                {
                    throw new ArgumentException("Nội dung đánh giá không được để trống");
                }

                if ((entity.MaCombo == 0 && entity.MaCtsp == 0) || (entity.MaCombo != 0 && entity.MaCtsp != 0))
                {
                    throw new ArgumentException("Phải cung cấp mã sản phẩm hoặc mã combo, nhưng không được cả hai");
                }

                if (entity.SoSao < 1 || entity.SoSao > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(entity.SoSao), "Số sao phải nằm trong khoảng từ 1 đến 5");
                }

                // Kiểm tra xem người dùng có quyền đánh giá sản phẩm này không
                bool canReview = await CanUserReviewProductAsync(entity.MaHd, entity.MaCtsp ?? 0, entity.MaKh);
                if (!canReview)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền đánh giá sản phẩm này");
                }

                // Thêm đánh giá vào cơ sở dữ liệu
                DanhGia reviewTransform = entity.ToDanhGia();
                await _db.DanhGias.AddAsync(entity.ToDanhGia());
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: reviewTransform.ToReviewResponseDTO(), message: "Thêm đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<string>> UpdateReviewAsync(ReviewRequestDTO entity)
        {
            ResponseAPI<string> response = new();
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity), "Đánh giá không được để trống");
                }

                if (entity.Id <= 0)
                {
                    throw new ArgumentException("Mã đánh giá không hợp lệ");
                }

                if (string.IsNullOrWhiteSpace(entity.NoiDung))
                {
                    throw new ArgumentException("Nội dung đánh giá không được để trống");
                }

                if ((entity.MaCombo == 0 && entity.MaCtsp == 0) || (entity.MaCombo != 0 && entity.MaCtsp != 0))
                {
                    throw new ArgumentException("Phải cung cấp mã sản phẩm hoặc mã combo, nhưng không được cả hai");
                }

                if (entity.SoSao < 1 || entity.SoSao > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(entity.SoSao), "Số sao phải nằm trong khoảng từ 1 đến 5");
                }

                // Kiểm tra xem đánh giá có tồn tại không
                DanhGia? existingReview = await _db.DanhGias.FindAsync(entity.Id);
                if (existingReview == null)
                {
                    throw new KeyNotFoundException("Đánh giá không tồn tại");
                }

                // Cập nhật thông tin đánh giá
                existingReview.NoiDung = entity.NoiDung;
                existingReview.SoSao = entity.SoSao;
                existingReview.NgayDanhGia = DateTime.UtcNow;

                _db.DanhGias.Update(existingReview);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: "Cập nhật đánh giá thành công", message: "Cập nhật đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }


        public async Task<ResponseAPI<string>> RemoveAsync(int reviewId, int userId)
        {
            ResponseAPI<string> response = new();
            try
            {
                if (reviewId <= 0)
                {
                    throw new ArgumentException("Mã đánh giá không hợp lệ");
                }

                if (userId <= 0)
                {
                    throw new ArgumentException("Mã người dùng không hợp lệ");
                }

                // Kiểm tra xem đánh giá có tồn tại không
                DanhGia? existingReview = await _db.DanhGias.FindAsync(reviewId);
                if (existingReview == null)
                {
                    throw new KeyNotFoundException("Đánh giá không tồn tại");
                }

                // Kiểm tra xem người dùng có quyền xóa đánh giá này không
                if (existingReview.MaKh != userId)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này");
                }

                _db.DanhGias.Remove(existingReview);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: "Xóa đánh giá thành công", message: "Xóa đánh giá thành công");
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [Private methods]
        private async Task<bool> CanUserReviewProductAsync(int orderId, int productId, int userId)
        {
            Hoadon? hoadon = await _db.Hoadons
                .Include(h => h.Cthoadons)
                .FirstOrDefaultAsync(h => h.MaHd == orderId && h.MaKh == userId);
            if (hoadon == null)
            {
                return false; // Không tìm thấy hóa đơn
            }
            // Kiểm tra xem sản phẩm có trong hóa đơn không
            return hoadon.Cthoadons.Any(ct => ct.MaCtsp == productId);
        }
        #endregion
    }
}
