using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByProductIdAsync(int maSp)
        {
            var response = new ResponseAPI<IEnumerable<ReviewResponseDTO>>();
            if (maSp <= 0)
            {
                response.SetErrorResponse("Mã sản phẩm không hợp lệ");
                return response;
            }

            try
            {
                var reviews = await _db.DanhGias
                    .Where(r => r.MaSp == maSp)
                    .Select(r => r.ToReviewResponseDTO())
                    .ToListAsync();

                if (!reviews.Any())
                {
                    response.SetErrorResponse("Không có đánh giá nào cho sản phẩm này");
                    return response;
                }

                response.SetSuccessResponse(data: reviews, message: "Lấy danh sách đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<OrderWithReview>> GetOrderWithDetailItemAndReviewByOrderIdAsync(int orderId, int userId)
        {
            var response = new ResponseAPI<OrderWithReview>();
            if (orderId <= 0 || userId <= 0)
            {
                response.SetErrorResponse("Mã hóa đơn hoặc mã người dùng không hợp lệ");
                return response;
            }

            try
            {
                var hoadon = await _db.Hoadons
                    .Include(h => h.Cthoadons)
                        .ThenInclude(ct => ct.MaCtspNavigation)
                            .ThenInclude(ctsp => ctsp.MaSpNavigation)
                                .ThenInclude(sp => sp.DanhGias)
                    .Include(h => h.Chitietcombohoadons)
                        .ThenInclude(ct => ct.MaComboNavigation)
                            .ThenInclude(combo => combo.DanhGias)
                    .FirstOrDefaultAsync(h => h.MaHd == orderId && h.MaKh == userId);

                if (hoadon == null)
                {
                    response.SetErrorResponse("Hóa đơn không tồn tại hoặc không thuộc về người dùng này");
                    return response;
                }

                var transferData = hoadon.ToOrderWithReview();
                transferData.Products.AddRange(hoadon.Cthoadons.Select(item => item.ToProductInOrderWithReview(item.MaCtspNavigation, userId)));
                transferData.Combos.AddRange(hoadon.Chitietcombohoadons.Select(item => item.ToComboInOrderWithReview(item.MaComboNavigation, userId)));

                response.SetSuccessResponse(data: transferData, message: "Lấy thông tin hóa đơn và đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<ReviewResponseDTO>> AddReviewForItemInOrderAsync(ReviewRequestDTO entity)
        {
            var response = new ResponseAPI<ReviewResponseDTO>();
            if (entity == null)
            {
                response.SetErrorResponse("Đánh giá không được để trống");
                return response;
            }

            try
            {
                ValidateReviewRequest(entity);

                // Kiểm tra xem đánh giá đã tồn tại chưa
                var existingReview = await _db.DanhGias
                    .FirstOrDefaultAsync(r => r.MaKh == entity.MaKh &&
                                              (r.MaSp == entity.MaSp || r.MaCombo == entity.MaCombo));

                // if (existingReview != null)
                // {
                //     response.SetErrorResponse("Bạn đã đánh giá sản phẩm hoặc combo này rồi.");
                //     return response;
                // }

                // Thêm đánh giá vào cơ sở dữ liệu
                var reviewTransform = entity.ToDanhGia();
                await _db.DanhGias.AddAsync(reviewTransform);
                await _db.SaveChangesAsync();
                response.SetSuccessResponse(data: reviewTransform.ToReviewResponseDTO(), message: "Thêm đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<string>> UpdateReviewAsync(ReviewRequestDTO entity)
        {
            var response = new ResponseAPI<string>();
            if (entity == null)
            {
                response.SetErrorResponse("Đánh giá không được để trống");
                return response;
            }

            try
            {
                ValidateReviewRequest(entity);

                var existingReview = await _db.DanhGias.FindAsync(entity.Id);
                if (existingReview == null)
                    throw new Exception("Đánh giá không tồn tại");

                // Cập nhật thông tin đánh giá
                existingReview.NoiDung = entity.NoiDung;
                existingReview.SoSao = entity.SoSao;
                existingReview.NgayDanhGia = DateTime.UtcNow;

                _db.DanhGias.Update(existingReview);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: "Cập nhật đánh giá thành công", message: "Cập nhật đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<string>> RemoveAsync(int reviewId, int userId)
        {
            var response = new ResponseAPI<string>();
            if (reviewId <= 0 || userId <= 0)
            {
                response.SetErrorResponse("Thông số dữ liệu không hợp lệ");
                return response;
            }

            try
            {
                var existingReview = await _db.DanhGias.FindAsync(reviewId);
                if (existingReview == null)
                    throw new KeyNotFoundException("Đánh giá không tồn tại");

                if (existingReview.MaKh != userId)
                    throw new Exception("Bạn không có quyền xóa đánh giá này");

                _db.DanhGias.Remove(existingReview);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: "Xóa đánh giá thành công", message: "Xóa đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private void ValidateReviewRequest(ReviewRequestDTO entity)
        {
            if (entity.MaKh <= 0) throw new ArgumentException("Mã khách hàng không hợp lệ");
            if (string.IsNullOrWhiteSpace(entity.NoiDung)) throw new ArgumentException("Nội dung đánh giá không được để trống");
            if (entity.SoSao < 1 || entity.SoSao > 5) throw new ArgumentOutOfRangeException(nameof(entity.SoSao), "Số sao phải nằm trong khoảng từ 1 đến 5");
        }
    }
}
