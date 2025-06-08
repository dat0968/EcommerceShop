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
        private static string pathImageReview = "wwwroot/HinhAnh/Reviews";
        private static string filterStatusOrder = "Đã nhận hàng"; // Trạng thái để lọc việc get danh sách đánh giá

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
                // Lấy danh sách mã biến thể của sản phẩm
                int[] listIds = await _db.Chitietsanphams.Where(x => x.MaSp == maSp).Select(x => x.MaCtsp).ToArrayAsync();

                // Lấy danh sách đánh giá cho sản phẩm cụ thể
                var reviews = await _db.DanhGias
                    .Where(r => r.MaSp == maSp)
                    .Include(x => x.Cthoadon)
                    .Include(r => r.KhachHang)
                    .Select(r => r.ToReviewResponseDTO(true))
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

        #region [Lấy danh sách đánh giá của combo]
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
                    .Include(r => r.KhachHang)
                    .Select(r => r.ToReviewResponseDTO(false))
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
                        .ThenInclude(ct => ct.DanhGia) // TempNote
                    .Include(h => h.Cthoadons)
                        .ThenInclude(ct => ct.MaCtspNavigation)
                            .ThenInclude(ctsp => ctsp.MaSpNavigation)
                    .Include(h => h.Cthoadons)
                        .ThenInclude(ct => ct.MaComboNavigation)
                    .FirstOrDefaultAsync(h => h.MaHd == orderId && h.MaKh == userId);

                if (hoadon == null)
                {
                    response.SetErrorResponse("Hóa đơn không tồn tại hoặc không thuộc về người dùng này");
                    return response;
                }

                var transferData = hoadon.ToOrderWithReview();
                transferData.Products.AddRange(hoadon.Cthoadons.Where(x => x.MaCtsp.HasValue).Select(item => item.ToProductInOrderWithReview()));
                transferData.Combos.AddRange(hoadon.Cthoadons.Where(x => x.MaCombo.HasValue).Select(item => item.ToComboInOrderWithReview()));

                response.SetSuccessResponse(data: transferData, message: "Lấy thông tin hóa đơn và đánh giá thành công");
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion

        #region [Lấy danh sách sản phẩm và combo trong hóa đơn cùng với đánh giá của người riêng người dùng]
        public async Task<ResponseAPI<Dictionary<string, List<ReviewResponseDTO>>>> GetAllReviewOfUser(int userId)
        {
            var response = new ResponseAPI<Dictionary<string, List<ReviewResponseDTO>>>();
            response.Data = new Dictionary<string, List<ReviewResponseDTO>>();
            try
            {
                DateTime today = DateTime.UtcNow;
                DateTime sevenDaysAgo = today.AddDays(-7);

                // Lấy tất cả cthoadon của user trong các hóa đơn đã nhận hàng trong 7 ngày gần nhất
                /*  && ct.MaHdNavigation.NgayNhan != null
                    && ct.MaHdNavigation.NgayNhan >= sevenDaysAgo */

                var cthoadons = await _db.Cthoadons
                    .Include(ct => ct.MaHdNavigation)
                        .Where(ct => ct.MaHdNavigation.MaKh == userId)
                    .Include(ct => ct.DanhGia)
                    .Include(ct => ct.MaCtspNavigation)
                        .ThenInclude(ctsp => ctsp.MaSpNavigation)
                    .Include(ct => ct.MaCtspNavigation)
                        .ThenInclude(ctsp => ctsp.Hinhanhs)
                    .Include(ct => ct.MaComboNavigation)
                    .ToListAsync();

                var notReviewIn7days = new List<ReviewResponseDTO>();
                var listReviewed = new List<ReviewResponseDTO>();

                foreach (var ct in cthoadons)
                {
                    if (ct.DanhGia != null)
                    {
                        bool isProduct = ct.DanhGia.MaSp != null && ct.DanhGia.MaSp != 0;
                        // Đã đánh giá
                        listReviewed.Add(ct.DanhGia.ToReviewResponseDTO(isProduct));
                    }
                    // ! Khi đặt filter trạng thái đơn hàng thì mở cmt dưới thay else
                    //  else if (ct.DanhGia != null && ct.DanhGia?.Cthoadon?.MaHdNavigation.NgayNhan != null && ct.DanhGia.Cthoadon.MaHdNavigation.NgayNhan >= sevenDaysAgo)
                    else
                    {
                        bool isProduct = ct.MaCtsp != null && ct.MaCtsp != 0;
                        // Chưa đánh giá, tạo ReviewResponseDTO với thông tin cơ bản
                        var dto = ReviewResponseDTO.MakeEmptyReview(ct, isProduct);
                        notReviewIn7days.Add(dto);
                    }
                }

                response.Data["notReviewIn7days"] = notReviewIn7days;
                response.Data["listReviewed"] = listReviewed;
                response.SetSuccessResponse();
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
                                              (r.MaSp == entity.MaSp) && r.MaCtHd == entity.MaCtHd);
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
                                              (r.MaCombo == entity.MaCombo) && r.MaCtHd == entity.MaCtHd);
                    if (existingReview != null)
                    {
                        response.SetErrorResponse("Bạn đã đánh giá combo này rồi, vui lòng chỉ sửa thông tin.");
                        return response;
                    }
                }

                // Thêm đánh giá vào cơ sở dữ liệu
                var reviewTransform = entity.ToDanhGia();

                // Lưu hình ảnh
                if (entity.HinhAnhs != null && entity.HinhAnhs.Length > 0)
                {
                    string[] listNameImgs = await SaveImagesReview(entity.HinhAnhs);
                    reviewTransform.CombineNameImg(listNameImgs);
                }
                await _db.DanhGias.AddAsync(reviewTransform);
                await _db.SaveChangesAsync();
                response.SetSuccessResponse(data: reviewTransform.ToReviewResponseDTO(isProduct), message: "Thêm đánh giá thành công");
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

        #region [Xóa đánh giá sản phẩm hoặc combo - Không chắc nó hoạt động hay không]
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
                string[] nameSavedFiles = existingReview.GetSavedListFileName();

                await _db.SaveChangesAsync();
                bool isDeletedSavedFiles = await DeleteSaveImages(nameSavedFiles);

                if (isDeletedSavedFiles)
                {
                    Console.WriteLine($">>>> Đã xóa các hình ảnh của đánh giá mã {existingReview.Id}");
                }
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
        /// Lấy tất cả đánh giá dưới dạng danh sách DTO [! Bad query]
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseAPI<IEnumerable<ReviewDetailDTO>>> GetAllReviewDtoAsync()
        {
            var response = new ResponseAPI<IEnumerable<ReviewDetailDTO>>();
            try
            {
                // Lấy danh sách mã biến thể của sản phẩm
                var listIds = await _db.Sanphams
                    .Include(x => x.Chitietsanphams)
                    .ToDictionaryAsync(
                        x => x.MaSp,
                        x => x.Chitietsanphams.Select(ct => ct.MaCtsp).ToList()
                    );

                var reviews = await _db.DanhGias
                    .Include(r => r.KhachHang)
                    .Include(r => r.SanPham)
                    .Include(r => r.Combo)
                    .ToListAsync();

                List<ReviewDetailDTO> transformDtos = new();
                foreach (var review in reviews)
                {
                    var orders = _db.Hoadons
                        .Where(hd => hd.MaKh == review.MaKh &&
                            (
                                (review.MaSp != null && hd.Cthoadons.Any(ct => listIds.ContainsKey(review.MaSp.Value) && listIds[review.MaSp.Value].Contains(ct.MaCtsp ?? 0)))
                                || (review.MaCombo != null && hd.Chitietcombohoadons.Any(ct => ct.MaCombo == review.MaCombo))
                            ))
                        .Select(hd => new OrderReviewInfoDTO
                        {
                            MaHd = hd.MaHd,
                            NgayTao = hd.NgayTao,
                            TrangThai = hd.TinhTrang,
                            MaCtsp = review.MaSp != null
                                ? (hd.Cthoadons.FirstOrDefault(ct => listIds.ContainsKey(review.MaSp.Value) && ct.MaCtsp != null && listIds[review.MaSp.Value].Contains(ct.MaCtsp.Value)) != null
                                    ? hd.Cthoadons.FirstOrDefault(ct => listIds.ContainsKey(review.MaSp.Value) && ct.MaCtsp != null && listIds[review.MaSp.Value].Contains(ct.MaCtsp.Value))!.MaCtsp
                                    : null)
                                : null,
                            MaCombo = review.MaCombo != null
                                ? (hd.Chitietcombohoadons.FirstOrDefault(ct => ct.MaCombo == review.MaCombo) != null
                                    ? hd.Chitietcombohoadons.FirstOrDefault(ct => ct.MaCombo == review.MaCombo)!.MaCombo
                                    : (int?)null)
                                : null,
                            SoLuong =
                                review.MaSp != null
                                    ? (
                                        (from ct in hd.Cthoadons
                                         where listIds.ContainsKey(review.MaSp.Value)
                                            && ct.MaCtsp != null
                                            && listIds[review.MaSp.Value].Contains(ct.MaCtsp.Value)
                                         select ct.SoLuong).FirstOrDefault()
                                      )
                                    : (review.MaCombo != null
                                        ? (
                                            (from ct in hd.Chitietcombohoadons
                                             where ct.MaCombo == review.MaCombo
                                             select ct.SoLuong).FirstOrDefault()
                                          )
                                        : null)
                        }).ToList();

                    var transformDto = new ReviewDetailDTO
                    {
                        Id = review.Id,
                        MaKh = review.MaKh,
                        TenKhachHang = review.KhachHang?.HoTen ?? "",
                        MaSp = review.MaSp,
                        MaCombo = review.MaCombo,
                        NoiDung = review.NoiDung,
                        SoSao = review.SoSao,
                        NgayDanhGia = review.NgayDanhGia,
                        ShopPhanHoi = review.ShopPhanHoi,
                        NgayPhanHoi = review.NgayPhanHoi,
                        Email = review.KhachHang?.Email ?? "",
                        SoDienThoai = review.KhachHang?.Sdt ?? "",
                        HoTen = review.KhachHang?.HoTen ?? "",
                        TenSanPham = review.SanPham != null ? review.SanPham.TenSanPham : (review.Combo != null ? review.Combo.TenCombo : ""),
                        HinhAnhs = review.TenCacHinhAnh ?? "",
                        DonGia = review.SanPham != null
                            ? (review.SanPham.Chitietsanphams.Any() ? (double)review.SanPham.Chitietsanphams.Average(x => x.DonGia) : 0)
                            : (review.Combo?.GiaCombo ?? 0),
                        LuotXem = review.SanPham?.LuotXem ?? 0,
                        SoLuong = review.SanPham != null
                            ? (review.SanPham.Chitietsanphams.Any() ? (int)review.SanPham.Chitietsanphams.Average(x => x.SoLuongTon) : 0)
                            : (review.Combo?.SoLuong ?? 0),
                        IsActive = review.SanPham?.IsActive ?? review.Combo?.IsActive,
                        Orders = orders
                    };
                    transformDtos.Add(transformDto);
                }
                if (!transformDtos.Any())
                {
                    response.SetErrorResponse("Không có đánh giá nào");
                    return response;
                }

                response.SetSuccessResponse(data: transformDtos, message: "Lấy danh sách đánh giá thành công");
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

        #region [Xử lí hình đánh giá]
        private async Task<string[]> SaveImagesReview(IFormFile[] fileForms)
        {
            try
            {
                if (fileForms == null || fileForms.Length == 0)
                {
                    return Array.Empty<string>();
                }

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), pathImageReview);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                List<string> savedFileNames = new List<string>();
                foreach (var file in fileForms)
                {
                    // Tạo tên file duy nhất
                    var ext = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName.Replace(' ', '_'))}_{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(folderPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    savedFileNames.Add(uniqueFileName);
                }
                return savedFileNames.ToArray();
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
        }
        private async Task<bool> DeleteSaveImages(string[] savedFiles)
        {
            if (savedFiles == null || savedFiles.Length == 0)
                return false;

            try
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), pathImageReview);
                foreach (var fileName in savedFiles)
                {
                    var filePath = Path.Combine(folderPath, fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #endregion
    }
}
