using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Reviews;
using APIClothesEcommerceShop.DTO.WheelCoupon;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Repositories.WheelCoupon;
using APIClothesEcommerceShop.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public class WheelCouponRepository(EcommerceShopContext db, IConfiguration configuration) : Repository<Models.Macoupon>(db), IWheelCouponRepository
    {
        private readonly EcommerceShopContext _db = db;
        private readonly IConfiguration _configuration = configuration;
        private static readonly Random _random = new(); // Static instance for true randomness
        private static string[] filterStatusOrder = ["đã nhận", "đã thanh toán"];

        public async Task<ResponseAPI<PrivateCouponInfoDTO>> HavePrivateCoupon(int? userId)
        {
            var response = new ResponseAPI<PrivateCouponInfoDTO>();
            try
            {
                if (!userId.HasValue || userId == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }

                var customer = await _db.Khachhangs
                                        .Include(kh => kh.Hoadons)
                                        .Include(kh => kh.MaCoupons)
                                        .FirstOrDefaultAsync(kh => kh.MaKh == userId.Value);

                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }

                var totalOrderValue = customer.Hoadons
                    .Where(hd => hd.TinhTrang != null && filterStatusOrder.Contains(hd.TinhTrang.ToLower()))
                    .Sum(hd => hd.TienGoc - hd.PhiVanChuyen);

                var allUserCoupons = customer.MaCoupons.Where(mc => mc.MaKhachHang == userId.Value).ToList();

                var wonSpins = allUserCoupons.Count(mc => mc.SoTienGiam > 0 || mc.PhanTramGiam > 0);
                var blankSpins = allUserCoupons.Count(mc => (mc.SoTienGiam == 0 || mc.SoTienGiam == null) && (mc.PhanTramGiam == 0 || mc.PhanTramGiam == null));

                var privateCoupons = allUserCoupons
                    .Where(mc => mc.TrangThai == true && mc.SoLuong > 0)
                    .Select(mc => new CouponInfoDTO
                    {
                        MaCode = mc.MaCode,
                        MoTa = mc.MoTa ?? "Coupon này không có mô tả",
                        SoTienGiam = mc.SoTienGiam,
                        PhanTramGiam = mc.PhanTramGiam,
                        NgayKetThuc = mc.NgayKetThuc
                    }).ToList();

                var result = new PrivateCouponInfoDTO
                {
                    Streak = customer.Streak,
                    TotalOrderValue = totalOrderValue,
                    WonSpins = wonSpins,
                    BlankSpins = blankSpins,
                    PrivateCoupons = privateCoupons
                };

                response.SetSuccessResponse(data: result);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> TimeCanSpinWheelCoupon(int? userId)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                if (!userId.HasValue || userId == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs
                        .Include(kh => kh.Hoadons)
                        .FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }
                bool isInWeekSteak = customer.Streak > 0 && customer.Streak % 7 == 0;

                int numPrivateCoupon = await _db.Macoupons.CountAsync(dg => dg.MaKhachHang == userId!.Value);
                decimal totalCompleted = customer.Hoadons
                    .Where(x => filterStatusOrder.Contains(x.TinhTrang.ToLower()))
                    .Sum(x => x.TienGoc - x.PhiVanChuyen);
                int totalSpin = (int)(totalCompleted / 2000000) + (isInWeekSteak ? 1 : 0);

                int spinsLeft = totalSpin - numPrivateCoupon;
                if (spinsLeft < 0) spinsLeft = 0;

                response.SetSuccessResponse(data: spinsLeft);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                response.Data = 0;
            }
            return response;
        }

        public async Task<ResponseAPI<WheelCouponCustomerStreakResponse>> UpdateLastLoginAndStreak(int? userId)
        {
            var response = new ResponseAPI<WheelCouponCustomerStreakResponse>();
            try
            {
                if (userId == 0 || userId == null)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs.FirstOrDefaultAsync(x => x.MaKh == userId.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }
                var now = DateTime.Now;
                var lastLogin = customer.TruyCapLanCuoi.Date;
                var today = now.Date;
                if (lastLogin == today)
                {
                    throw new Exception("Đã đăng nhập hôm nay, không tăng streak");
                }
                else if (lastLogin == today.AddDays(-1))
                {
                    customer.Streak += 1;
                }
                else
                {
                    customer.Streak = 1;
                }
                customer.TruyCapLanCuoi = now;
                await _db.SaveChangesAsync();
                response.SetSuccessResponse(data: customer.ToWheelCouponCustomerStreakResponse());
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> SpinWheelAndGenerateCoupon(int? userId, WheelCouponCreateRequest? request)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                if (!await CanSpin(userId))
                {
                    throw new Exception("Không đủ điều kiện để quay vòng quay.");
                }

                bool isWin = _random.Next(1, 101) <= 90; // 90% chance to win

                if (isWin)
                {
                    response = await CreatePrivateCoupon(userId, request);
                }
                else
                {
                    response = await CreateBlankCoupon(userId);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<CouponPresetDTO>> GenerateCouponPreset()
        {
            var response = new ResponseAPI<CouponPresetDTO>();
            try
            {
                var presets = new List<CouponValue>();
                for (int i = 0; i < 9; i++)
                {
                    bool isPercent = _random.NextDouble() > 0.5;
                    presets.Add(new CouponValue
                    {
                        IsPercent = isPercent,
                        Value = isPercent ? _random.Next(5, 21) : _random.Next(5, 21) * 10000 // 5-20% or 50k-210k
                    });
                }

                var payload = JsonSerializer.Serialize(presets);
                var secretKey = _configuration["TokenSettings:SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");
                var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secretKey), Encoding.UTF8.GetBytes(payload));

                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload)) + "." + Convert.ToBase64String(signature);

                var displayValues = presets.Select(p => p.IsPercent ? $"{p.Value}%" : p.Value.ToString("N0") + "₫").ToList();

                response.SetSuccessResponse(data: new CouponPresetDTO { PresetToken = token, DisplayValues = displayValues });
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> ClaimPresetCoupon(int? userId, ClaimPresetCouponRequest request)
        {
            var response = new ResponseAPI<dynamic>();
            try
            {
                if (!await CanSpin(userId))
                {
                    throw new Exception("Không đủ điều kiện để quay vòng quay.");
                }

                var tokenParts = request.PresetToken.Split('.');
                if (tokenParts.Length != 2)
                {
                    throw new ArgumentException("Số liệu không không hợp lệ.");
                }

                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[0]));
                var signatureFromRequest = Convert.FromBase64String(tokenParts[1]);

                var secretKey = _configuration["TokenSettings:SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");
                var expectedSignature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secretKey), Encoding.UTF8.GetBytes(payloadJson));

                if (!signatureFromRequest.SequenceEqual(expectedSignature))
                {
                    throw new UnauthorizedAccessException("Phát hiện hoạt động của bạn không được xác thực, vui lòng thử lại sau.");
                }

                var presets = JsonSerializer.Deserialize<List<CouponValue>>(payloadJson);
                if (presets == null || request.WonIndex >= presets.Count || request.WonIndex < 0)
                {
                    throw new ArgumentException("Tải trọng hoặc chỉ mục không hợp lệ.");
                }

                var wonCoupon = presets[request.WonIndex];

                // Decide if this spin is a winning one
                bool isWin = _random.Next(1, 101) <= 90; // 90% chance to win

                if (isWin)
                {
                    var createRequest = new WheelCouponCreateRequest
                    {
                        IsPercent = wonCoupon.IsPercent,
                        DecreaseValue = wonCoupon.Value
                    };
                    return await CreatePrivateCoupon(userId, createRequest);
                }
                else
                {
                    return await CreateBlankCoupon(userId);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [PRIVATE METHOD]

        private async Task<ResponseAPI<dynamic>> CreatePrivateCoupon(int? userId, WheelCouponCreateRequest? request = null)
        {
            ResponseAPI<dynamic> response = new();
            bool isRequestCreateNull = request == null;
            try
            {
                if (userId == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs.Include(kh => kh.Hoadons)
                                                    .FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }
                bool isPercent = isRequestCreateNull ? _random.NextDouble() > 0.5 : request!.IsPercent!.Value;

                var coupon = new Models.Macoupon()
                {
                    MaCode = GenerateRandomCouponCode(),
                    MoTa = $"Mã coupon riêng cho khách hàng {customer.HoTen}",
                    DonHangToiThieu = 1,
                    NgayBatDau = DateTime.Now,
                    NgayKetThuc = DateTime.MaxValue,
                    SoLuong = 1,
                    SoLuongDaDung = 0,
                    TrangThai = true,
                    MaKhachHang = userId
                };
                if (isPercent)
                {
                    coupon.PhanTramGiam = isRequestCreateNull ? (_random.Next(10, 30)) : request!.DecreaseValue!.Value;
                }
                else
                {
                    coupon.SoTienGiam = isRequestCreateNull ? _random.Next(10, 15) * 10000 : (int)request!.DecreaseValue!.Value;
                }
                await _db.AddAsync(coupon);
                await _db.SaveChangesAsync();
                var transformData = new
                {
                    maCode = coupon.MaCode,
                    moTa = coupon.MoTa,
                    phanTramGiam = coupon.PhanTramGiam,
                    soTienGiam = coupon.SoTienGiam,
                    isPercent = isPercent,
                };
                response.SetSuccessResponse(data: transformData);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private async Task<ResponseAPI<dynamic>> CreateBlankCoupon(int? userId)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                if (userId == 0 || userId == null)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs.FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }

                var blankCoupon = new Models.Macoupon()
                {
                    MaCode = GenerateRandomCouponCode(15, "BNK"),
                    MoTa = $"Mã coupon rỗng cho khách hàng {customer.HoTen} (không trúng giải)",
                    DonHangToiThieu = 0,
                    NgayBatDau = DateTime.Now,
                    NgayKetThuc = DateTime.Now.AddDays(-1), // Expired in the past
                    SoLuong = 1,
                    SoLuongDaDung = 1,
                    TrangThai = false,
                    PhanTramGiam = 0,
                    SoTienGiam = 0,
                    MaKhachHang = userId
                };

                await base.AddAsync(blankCoupon);
                await _db.SaveChangesAsync();

                response.SetSuccessResponse(data: new { message = "Phiếu giảm giá trống được tạo thành công", isWin = false });
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private static string GenerateRandomCouponCode(int length = 8, string? headCode = null)
        {
            if (length > 20)
            {
                length = 20;
            }
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string newCode = string.Empty;
            if (string.IsNullOrEmpty(headCode))
            {
                newCode = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
            else
            {
                newCode = headCode + new string(Enumerable.Repeat(chars, length - headCode.Length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
            return newCode;
        }

        private async Task<bool> CanSpin(int? userId)
        {
            if (!userId.HasValue || userId == 0)
            {
                return false;
            }
            var customer = await _db.Khachhangs
                    .Include(kh => kh.Hoadons)
                    .FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
            if (customer == null)
            {
                return false;
            }
            bool isInWeekSteak = customer.Streak > 0 && customer.Streak % 7 == 0;

            int numPrivateCoupon = await _db.Macoupons.CountAsync(dg => dg.MaKhachHang == userId!.Value);
            decimal totalCompleted = customer.Hoadons
                .Where(x => filterStatusOrder.Contains(x.TinhTrang.ToLower()))
                .Sum(x => x.TienGoc - x.PhiVanChuyen);
            int totalSpin = (int)(totalCompleted / 2000000) + (isInWeekSteak ? 1 : 0);

            int spinsLeft = totalSpin - numPrivateCoupon;
            return spinsLeft > 0;
        }
        #endregion
    }
}