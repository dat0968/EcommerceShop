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

                bool isInWeekSteak = customer.Streak % 7 == 0;

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

        public async Task<ResponseAPI<Models.Macoupon>> CreatePrivateCoupon(int? userId, int? decreasePrice, bool? isPercent = true)
        {
            ResponseAPI<Models.Macoupon> response = new();
            try
            {
                if (userId == 0 || decreasePrice == 0)
                {
                    throw new KeyNotFoundException("Dữ liệu yêu cầu không hợp lệ.");
                }
                var customer = await _db.Khachhangs.Include(kh => kh.Hoadons)
                                                    .FirstOrDefaultAsync(x => x.MaKh == userId!.Value);
                if (customer == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy người dùng trong hệ thống");
                }

                Models.Macoupon coupon = new()
                {
                    MoTa = $"Mã coupon riêng cho khách hàng ${customer.HoTen}",
                    DonHangToiThieu = 1,
                    NgayBatDau = DateTime.MaxValue,
                    SoLuong = 1,
                    SoLuongDaDung = 0,
                    TrangThai = true,
                };

                if (isPercent!.Value && decreasePrice < 100)
                {
                    coupon.PhanTramGiam = decreasePrice;
                }
                else coupon.SoTienGiam = decreasePrice;

                await base.AddAsync(coupon);

                response.SetSuccessResponse(data: coupon);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
    }
}
