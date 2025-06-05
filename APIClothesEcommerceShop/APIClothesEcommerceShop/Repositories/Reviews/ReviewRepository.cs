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

        // #region [REVIEW METHODS FOR PRODUCTS AND COMBOS ONLY FOR CUSTOMERS]
        #region [Lấy danh sách đánh giá của sản phẩm]
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
        #endregion

        #region  [Lấy danh sách đánh giá của sản phẩm]
        public async Task<ResponseAPI<IEnumerable<ReviewResponseDTO>>> GetReviewsByComboIdAsync(int maCombo)
        {
            var response = new ResponseAPI<IEnumerable<ReviewResponseDTO>>();
            if (maCombo <= 0)
            {
                response.SetErrorResponse("Mã combo không hợp lệ");
                return response;
            }

            try
            {
                var reviews = await _db.DanhGias
                    .Where(r => r.MaCombo == maCombo)
                    .Select(r => r.ToReviewResponseDTO())
                    .ToListAsync();

                if (!reviews.Any())
                {
                    response.SetErrorResponse("Không có đánh giá nào cho combo này");
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
        #endregion

        #region [Lấy danh sách sản phẩm và combo trong hóa đơn cùng với đánh giá của người riêng người dùng theo số hóa đơn]
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
        #endregion

        #region [Thêm đánh giá cho sản phẩm hoặc combo]
        public async Task<ResponseAPI<ReviewResponseDTO>> AddReviewForItemInOrderAsync(ReviewRequestDTO entity, bool isProduct)
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
                DanhGia? existingReview = new();
                if (isProduct)
                {
                    existingReview = await _db.DanhGias
                    .FirstOrDefaultAsync(r => r.MaKh == entity.MaKh &&
                                              (r.MaSp == entity.MaSp));
                    if (existingReview != null)
                    {
                        response.SetErrorResponse("Bạn đã đánh giá sản phẩm này rồi, vui lòng chỉ sửa thông tin.");
                        return response;
                    }
                }
                else
                {
                    existingReview = await _db.DanhGias
                    .FirstOrDefaultAsync(r => r.MaKh == entity.MaKh &&
                                              (r.MaCombo == entity.MaCombo));
                    if (existingReview != null)
                    {
                        response.SetErrorResponse("Bạn đã đánh giá combo này rồi, vui lòng chỉ sửa thông tin.");
                        return response;
                    }
                }

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
        #endregion

        #region [Cập nhập đánh giá sản phẩm hoặc combo]
        public async Task<ResponseAPI<string>> UpdateReviewAsync(ReviewRequestDTO entity, bool isProduct)
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


                // Kiểm tra xem đánh giá đã tồn tại chưa
                DanhGia? existingReview = new();
                if (isProduct)
                {
                    existingReview = await _db.DanhGias
                    .FirstOrDefaultAsync(r => r.MaKh == entity.MaKh &&
                                              (r.MaSp == entity.MaSp));
                }
                else
                {
                    existingReview = await _db.DanhGias
                    .FirstOrDefaultAsync(r => r.MaKh == entity.MaKh &&
                                              (r.MaCombo == entity.MaCombo));
                }
                if (existingReview == null)
                {
                    response.SetErrorResponse("Đánh giá không tồn tại, vui lòng thêm mới đánh giá trước");
                    return response;
                }
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
        #endregion

        #region [Xóa đánh giá sản phẩm hoặc combo]
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
        #endregion

        #region [PRIVATE METHODS]
        #region [Kiểm tra tính hợp lệ của yêu cầu đánh giá]
        private void ValidateReviewRequest(ReviewRequestDTO entity)
        {
            if (entity.MaKh <= 0) throw new ArgumentException("Mã khách hàng không hợp lệ");
            if (string.IsNullOrWhiteSpace(entity.NoiDung)) throw new ArgumentException("Nội dung đánh giá không được để trống");
            if (entity.SoSao < 1 || entity.SoSao > 5) throw new ArgumentOutOfRangeException(nameof(entity.SoSao), "Số sao phải nằm trong khoảng từ 1 đến 5");
        }
        #endregion
        #endregion
        // #endregion


        #region [REVIEW METHODS FOR PRODUCTS AND COMBOS ONLY FOR STAFFS]
        /// <summary>
        /// Lấy tất cả đánh giá dưới dạng danh sách DTO
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseAPI<IEnumerable<ReviewDetailResponseDTO>>> GetAllReviewDtoAsync()
        {
            var response = new ResponseAPI<IEnumerable<ReviewDetailResponseDTO>>();
            try
            {
                var reviews = await _db.DanhGias
                    .Include(r => r.KhachHang)
                    .Include(r => r.SanPham)
                        .ThenInclude(sp => sp.Chitietsanphams)
                    .Include(r => r.Combo)
                    .Select(r => r.ToReviewResponseDTO().ToDetailResponseDTO(r.KhachHang, r.SanPham, r.Combo))
                    .ToListAsync();

                if (!reviews.Any())
                {
                    response.SetErrorResponse("Không có đánh giá nào");
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

        public async Task<ResponseAPI<string>> UpdateShopReplyAsync(ReviewReplyRequestDTO request)
        {
            var response = new ResponseAPI<string>();
            if (request == null || request.ListId.Length == 0 || string.IsNullOrWhiteSpace(request.ResponseContent))
            {
                response.SetErrorResponse("Thông tin đánh giá hoặc nội dung phản hồi không hợp lệ");
                return response;
            }

            try
            {
                var reviews = await _db.DanhGias
                    .Where(r => request.ListId.Contains(r.Id))
                    .ToListAsync();

                if (!reviews.Any())
                {
                    response.SetErrorResponse("Không tìm thấy đánh giá nào để cập nhật phản hồi");
                    return response;
                }

                foreach (var review in reviews)
                {
                    review.ShopPhanHoi = request.ResponseContent;
                    review.NgayPhanHoi = DateTime.UtcNow;
                }

                _db.DanhGias.UpdateRange(reviews);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: "Cập nhật phản hồi thành công", message: "Cập nhật phản hồi thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion
    }
}
