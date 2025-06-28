using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Reviews;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Repository;
using APIClothesEcommerceShop.Repositories.WheelCoupon;
using APIClothesEcommerceShop.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Reviews
{
    public class WheelCouponRepository(EcommerceShopContext db) : Repository<Models.Macoupon>(db), IWheelCouponRepository
    {
        private readonly EcommerceShopContext _db = db;
        private string CompletelyStatus = "Đã nhận";
        public async Task<ResponseAPI<dynamic>> HavePrivateCoupon(int? userId)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                if (userId == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                bool havePrivateCoupon = await base.ExistsAsync(x => x.MaKhachHang == userId);
                response.SetSuccessResponse(data: havePrivateCoupon);
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
                var customer = await _db.Khachhangs.FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }
                bool isInWeekSteak = customer.Streak > 0 && customer.Streak % 7 == 0;

                int numPrivateCoupon = await _db.Macoupons.CountAsync(dg => dg.MaKhachHang == userId!.Value);
                decimal totalCompleted = customer.Hoadons
                    .Where(x => x.TinhTrang == CompletelyStatus)
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

        public async Task<ResponseAPI<dynamic>> IsInWeekSteak(int? userId)
        {
            ResponseAPI<dynamic> response = new();
            try
            {
                if (!userId.HasValue || userId == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs.FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }

                bool isInWeekSteak = customer.Streak > 0 && customer.Streak % 7 == 0;

                response.SetSuccessResponse(data: isInWeekSteak);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> Over2MillionUse(int? userId)
        {
            ResponseAPI<dynamic> response = new();
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

                int numPrivateCoupon = await _db.Macoupons.CountAsync(dg => dg.MaKhachHang == userId!.Value);
                decimal totalCompleted = customer.Hoadons
                    .Where(x => x.TinhTrang == CompletelyStatus)
                    .Sum(x => x.TienGoc - x.PhiVanChuyen);
                int times = (int)(totalCompleted / 2000000);
                bool isOver2MillionUse = numPrivateCoupon < times;
                response.SetSuccessResponse(data: isOver2MillionUse);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        public async Task<ResponseAPI<dynamic>> CreatePrivateCoupon(int? userId)
        {

            ResponseAPI<dynamic> response = new();
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
                bool isPercent = new Random().NextDouble() > 0.5;
                var coupon = new Models.Macoupon();
                coupon.MaCode = GenerateRandomCouponCode();
                coupon.MoTa = $"Mã coupon riêng cho khách hàng {customer.HoTen}";
                coupon.DonHangToiThieu = 1;
                coupon.NgayBatDau = DateTime.Now;
                coupon.NgayKetThuc = DateTime.MaxValue;
                coupon.SoLuong = 1;
                coupon.SoLuongDaDung = 0;
                coupon.TrangThai = true;
                if (isPercent)
                {
                    coupon.PhanTramGiam = (new Random().Next(10, 30));
                }
                else
                {
                    coupon.SoTienGiam = new Random().Next(10, 15) * 10000;
                }
                await base.AddAsync(coupon);
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
        public async Task<ResponseAPI<Khachhang>> UpdateLastLoginAndStreak(int? userId)
        {
            var response = new ResponseAPI<Khachhang>();
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
                var lastLogin = customer.TruyCapLlanCuoi.Date;
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
                customer.TruyCapLlanCuoi = now;
                await _db.SaveChangesAsync();
                response.SetSuccessResponse(data: customer);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region 

        private string GenerateRandomCouponCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion
    }
}
