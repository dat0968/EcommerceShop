using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Statistics;
using APIClothesEcommerceShop.DTO.Statistics.Sub;
using APIClothesEcommerceShop.Utils;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Statistics
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly EcommerceShopContext _context;
        public StatisticRepository(EcommerceShopContext context)
        {
            _context = context;
        }

        #region Đơn hàng

        /// <summary>
        /// Lấy thống kê đơn hàng
        /// </summary>
        /// <returns>Thống kê đơn hàng</returns>
        public Task<ResponseAPI<OrderStatisticsResponse>> GetOrderStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy thông tin chi tiết đơn hàng theo mã đơn hàng
        /// </summary>
        /// <returns>Thông tin chi tiết đơn hàng</returns>
        public async Task<ResponseAPI<OrderSummaryResponse>> GetOrderSummaryByOrder()
        {
            ResponseAPI<OrderSummaryResponse> response = new();
            try
            {
                var data = await _context.Hoadons.ToListAsync();
                if (data?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu đơn hàng nào trong hệ thống.", 404);
                    return response;
                }
                var totalOrders = data.Count;
                var totalRevenue = data.Sum(x => x.TienGoc);
                var totalShippingFee = data.Sum(x => x.PhiVanChuyen);

                if (response.Data == null)
                {
                    response.Data = new OrderSummaryResponse();
                }

                response.Data.AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                response.Data.TotalOrders = totalOrders;
                response.Data.TotalRevenue = totalRevenue;
                response.Data.TotalShippingFee = totalShippingFee;

                // ! Cần phải tính toán lại
                response.Data.TotalDiscount = data.Sum(x => x.MaCodeNavigation?.SoTienGiam ?? 0);
                response.Data.TotalCustomers = data.Select(x => x.MaKh).Distinct().Count();
                response.Data.TotalProducts = data.SelectMany(x => x.Cthoadons).Count();
                response.Data.OrderStatusStatistics = data.GroupBy(x => x.TinhTrang)
                    .Select(g => new OrderStatusStatistics
                    {
                        Status = g.Key,
                        Count = g.Count()
                    }).ToList();

                // ! Cần phải tính toán lại
                response.Data.BestSellingProducts = data.SelectMany(x => x.Cthoadons)
                    .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                    .Select(g => new BestSellingProduct
                    {
                        ProductId = g.Key.MaCtsp ?? 0,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Quantity = g.Sum(x => x.SoLuong)
                    }).OrderByDescending(x => x.Quantity).ToList();
                response.Data.RevenueByDate = data.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new RevenueByDate
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                response.Data.RevenueByMonth = data.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                    .Select(g => new RevenueByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                response.Data.RevenueByYear = data.GroupBy(x => x.NgayTao.Year)
                    .Select(g => new RevenueByYear
                    {
                        Year = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();

                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }
        #endregion

        #region Sản phẩm

        /// <summary>
        /// Lấy thống kê sản phẩm
        /// </summary>
        /// <returns>Thống kê sản phẩm</returns>
        public Task<ResponseAPI<ProductStatisticsResponse>> GetProductStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Khách hàng

        /// <summary>
        /// Lấy thống kê khách hàng
        /// </summary>
        /// <returns>Thống kê khách hàng</returns>
        public Task<ResponseAPI<CustomerStatisticsResponse>> GetCustomerStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nhân viên

        /// <summary>
        /// Lấy thống kê nhân viên
        /// </summary>
        /// <returns>Thống kê nhân viên</returns>
        public Task<ResponseAPI<EmployeeStatisticsResponse>> GetEmployeeStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Doanh thu

        /// <summary>
        /// Lấy thống kê doanh thu
        /// </summary>
        /// <returns>Thống kê doanh thu</returns>
        public Task<ResponseAPI<RevenueStatisticsResponse>> GetRevenueStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Combo

        /// <summary>
        /// Lấy thống kê combo
        /// </summary>
        /// <returns>Thống kê combo</returns>
        public Task<ResponseAPI<ComboStatisticsResponse>> GetComboStatisticsAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}