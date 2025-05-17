using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Statistics;
using APIClothesEcommerceShop.DTO.Statistics.Sub;
using APIClothesEcommerceShop.Models;
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
        /// Lấy thông tin chi tiết đơn hàng theo mã đơn hàng
        /// </summary>
        /// <returns>Thông tin chi tiết đơn hàng</returns>
        public async Task<ResponseAPI<OrderSummaryResponse>> GetOrderSummaryByOrder()
        {
            ResponseAPI<OrderSummaryResponse> response = new();
            try
            {
                var dataMain = await GetHoadonsAsync();
                if (dataMain?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu đơn hàng nào trong hệ thống.", 404);
                    return response;
                }
                var totalOrders = dataMain.Count;
                var totalRevenue = dataMain.Sum(x => x.TienGoc);
                var totalShippingFee = dataMain.Sum(x => x.PhiVanChuyen);

                if (response.Data == null)
                {
                    response.Data = new OrderSummaryResponse();
                }

                response.Data.AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                response.Data.TotalOrders = totalOrders;
                response.Data.TotalRevenue = totalRevenue;
                response.Data.TotalShippingFee = totalShippingFee;

                // ! Cần phải tính toán lại
                response.Data.TotalDiscount = dataMain.Sum(x => x.MaCodeNavigation?.SoTienGiam ?? 0);
                response.Data.TotalCustomers = dataMain.Select(x => x.MaKh).Distinct().Count();
                response.Data.TotalProducts = dataMain.SelectMany(x => x.Cthoadons).Count();
                response.Data.OrderStatusStatistics = dataMain.GroupBy(x => x.TinhTrang)
                    .Select(g => new OrderStatusStatistics
                    {
                        Status = g.Key,
                        Count = g.Count()
                    }).ToList();

                // ! Cần phải tính toán lại
                response.Data.BestSellingProducts = dataMain.SelectMany(x => x.Cthoadons)
                    .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                    .Select(g => new BestSellingProduct
                    {
                        ProductId = g.Key.MaCtsp ?? 0,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Quantity = g.Sum(x => x.SoLuong)
                    }).OrderByDescending(x => x.Quantity).ToList();
                response.Data.RevenueByDate = dataMain.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new RevenueByDate
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                response.Data.RevenueByMonth = dataMain.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                    .Select(g => new RevenueByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                response.Data.RevenueByYear = dataMain.GroupBy(x => x.NgayTao.Year)
                    .Select(g => new RevenueByYear
                    {
                        Year = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                response.Data.OrdersByDate = dataMain.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new OrderByDate
                    {
                        Date = g.Key,
                        Count = g.Count()
                    }).ToList();
                response.Data.OrdersByMonth = dataMain.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                    .Select(g => new OrderByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Count = g.Count()
                    }).ToList();
                response.Data.OrdersByYear = dataMain.GroupBy(x => x.NgayTao.Year)
                    .Select(g => new OrderByYear
                    {
                        Year = g.Key,
                        Count = g.Count()
                    }).ToList();
                response.Data.TopProductByOrder = dataMain.SelectMany(x => x.Cthoadons)
                    .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                    .Select(g => new TopProductByOrder
                    {
                        ProductId = g.Key.MaCtsp ?? 0,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Count = g.Count()
                    }).OrderByDescending(x => x.Count).ToList();
                response.Data.TopCustomerByOrder = dataMain.GroupBy(x => new { x.MaKh, x.MaKhNavigation?.HoTen })
                    .Select(g => new TopCustomerByOrder
                    {
                        CustomerId = g.Key.MaKh,
                        CustomerName = g.Key.HoTen ?? string.Empty,
                        Count = g.Count()
                    }).OrderByDescending(x => x.Count).ToList();


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
        public async Task<ResponseAPI<ProductStatisticsResponse>> GetProductStatisticsAsync()
        {
            ResponseAPI<ProductStatisticsResponse> response = new();
            try
            {
                var dataMain = await GetSanphamsAsync();
                var dataCategory = await _context.Danhmucchas
                                        .Include(x => x.Chitietdanhmucs)
                                        .ToListAsync();
                var dataOrder = await _context.Hoadons
                                        .Include(x => x.Cthoadons)
                                            .ThenInclude(x => x.MaCtspNavigation)
                                                .ThenInclude(x => x.MaSpNavigation)
                                        .ToListAsync();

                if (dataMain?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu sản phẩm nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new ProductStatisticsResponse();
                }
                var totalProducts = dataMain.Count;
                var totalActiveProducts = dataMain.Select(x => x.IsActive).Count();
                var totalInactiveProducts = totalProducts - totalActiveProducts;
                var totalRevenue = dataMain.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia));
                // var totalDiscount = data.Sum(x => x.Chitietsanphams.Sum(y => y.GiamGia));
                var totalCategories = dataCategory.Select(x => x.TenDanhMucCha).Distinct().Count();

                response.Data.TotalProducts = totalProducts;
                response.Data.TotalActiveProducts = totalActiveProducts;
                response.Data.TotalInactiveProducts = totalInactiveProducts;
                response.Data.TotalRevenue = totalRevenue;
                //response.Data.TotalDiscount = 0; // ! Cần phải tính toán lại
                response.Data.AveragePrice = totalProducts > 0 ? totalRevenue / totalProducts : 0;
                // ! Cần phải tính toán lại
                response.Data.BestSellingProducts = dataMain.SelectMany(x => x.Chitietsanphams)
                    .GroupBy(x => new { x.MaCtsp, x.MaSpNavigation?.TenSanPham })
                    .Select(g => new BestSellingProduct
                    {
                        ProductId = g.Key.MaCtsp,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Quantity = g.Sum(x => x.SoLuongTon)
                    }).OrderByDescending(x => x.Quantity).ToList();
                response.Data.ProductCategoryStatistics = dataMain.GroupBy(x => x.Chitietdanhmucs?.Select(y => y.MaDanhMucChaNavigation))
                    .Select(g => new ProductCategoryStatistics
                    {
                        CategoryName = g.Key?.FirstOrDefault()?.TenDanhMucCha ?? string.Empty,
                        TotalProducts = g.Count(),
                        TotalRevenue = g.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia))
                    }).ToList();
                /* ! Cần kiểm tra có nên tinh toán lại không
                response.Data.ProductBrandStatistics = dataMain.GroupBy(x => x.Chitietdanhmucs?.Select(y => y.MaDanhMucChaNavigation))
                    .Select(g => new ProductBrandStatistics
                    {
                        BrandName = g.Key?.FirstOrDefault()?.TenDanhMucCha ?? string.Empty,
                        TotalProducts = g.Count(),
                        TotalRevenue = g.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia))
                    }).ToList(); */

                response.Data.SalesByDate = dataOrder.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new SalesByDate
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia * y?.SoLuong) - y?.GiamGia ?? 0))
                    }).ToList();

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }

        #endregion

        #region Khách hàng

        /// <summary>
        /// Lấy thống kê khách hàng
        /// </summary>
        /// <returns>Thống kê khách hàng</returns>
        public async Task<ResponseAPI<CustomerStatisticsResponse>> GetCustomerStatisticsAsync()
        {
            ResponseAPI<CustomerStatisticsResponse> response = new();
            try
            {
                var data = await GetKhachhangsAsync();
                var dataOrder = await GetHoadonsAsync();
                if (data?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu khách hàng nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new CustomerStatisticsResponse();
                }
                var totalCustomers = data.Count;
                var totalActiveCustomers = data.Count(x => x.IsActive ?? false);
                var totalInactiveCustomers = totalCustomers - totalActiveCustomers;
                var totalOrders = dataOrder.Count();
                var totalRevenue = dataOrder.Sum(x => x.TienGoc);
                // ! Cần phải tính toán lại
                var totalDiscount = dataOrder.Sum(x => x.MaCodeNavigation?.SoTienGiam == null ? 0 : x.MaCodeNavigation.SoTienGiam);
                var totalProducts = await _context.Cthoadons.CountAsync();
                var averagePurchaseAmount = totalOrders > 0 ? totalRevenue / totalOrders : 0;
                var totalPurchaseAmount = dataOrder.Sum(x => x.Cthoadons.Sum(y => (y?.Gia * y?.SoLuong) - y?.GiamGia ?? 0));
                var customersByLocation = data.GroupBy(x => x.DiaChi)
                    .Select(g => new CustomerByLocation
                    {
                        Location = g.Key ?? "Dữ liệu không xác định",
                        Count = g.Count()
                    }).ToList();
                var customersByAgeGroup = data.GroupBy(x => x.NgaySinh)
                    .Select(g => new CustomerByAgeGroup
                    {
                        AgeGroup = g.Key.ToString() ?? "Dữ liệu không xác định",
                        Count = g.Count()
                    }).ToList();
                var topCustomersByPurchase = dataOrder
                    .GroupBy(x => new { x.MaKh, x.MaKhNavigation?.HoTen })
                    .Select(g => new TopCustomerByPurchase
                    {
                        CustomerId = g.Key.MaKh,
                        CustomerName = g.Key.HoTen ?? string.Empty,
                        PurchaseCount = g.Count()
                    }).OrderByDescending(x => x.PurchaseCount).ToList();

                response.Data.TotalCustomers = totalCustomers;
                response.Data.TotalActiveCustomers = totalActiveCustomers;
                response.Data.TotalInactiveCustomers = totalInactiveCustomers;
                response.Data.CustomersByLocation = customersByLocation;
                response.Data.CustomersByAgeGroup = customersByAgeGroup;
                response.Data.TopCustomersByPurchase = topCustomersByPurchase;
                response.Data.TopCustomersByRevenue = dataOrder
                    .GroupBy(x => new { x.MaKh, x.MaKhNavigation?.HoTen })
                    .Select(g => new TopCustomerByRevenue
                    {
                        CustomerId = g.Key.MaKh,
                        CustomerName = g.Key.HoTen ?? string.Empty,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).OrderByDescending(x => x.Revenue).ToList();
                response.Data.AveragePurchaseAmount = averagePurchaseAmount;
                response.Data.TotalPurchaseAmount = totalPurchaseAmount;

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }

        #endregion

        #region Nhân viên

        /// <summary>
        /// Lấy thống kê nhân viên
        /// </summary>
        /// <returns>Thống kê nhân viên</returns>
        public async Task<ResponseAPI<EmployeeStatisticsResponse>> GetEmployeeStatisticsAsync()
        {
            ResponseAPI<EmployeeStatisticsResponse> response = new();
            try
            {
                var data = await GetNhanviensAsync();
                if (data?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu nhân viên nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new EmployeeStatisticsResponse();
                }
                var totalEmployees = data.Count;
                var totalActiveEmployees = data.Count(x => x.IsActive ?? false);
                var totalInactiveEmployees = totalEmployees - totalActiveEmployees;
                // ! Xem bỏ đi có được không
                // var totalDepartments = data.Select(x => x.MaBoPhanNavigation?.TenBoPhan).Distinct().Count();
                var employeeByPosition = data.GroupBy(x => x.MaChucVuNavigation.TenChucVu)
                    .Select(g => new EmployeeByPosition
                    {
                        PositionName = g.Key ?? string.Empty,
                        Count = g.Count()
                    }).ToList();
                var topEmployeesByPerformance = data
                    .GroupBy(x => new { x.MaNv, x.HoTen })
                    .Select(g => new TopEmployeeByPerformance
                    {
                        EmployeeId = g.Key.MaNv,
                        EmployeeName = g.Key.HoTen ?? string.Empty,
                        PerformanceScore = g.Sum(x => x.TinhTrang == "Hoàn thành" ? 1 : 0)
                    }).OrderByDescending(x => x.PerformanceScore).ToList();

                var topEmployeesBySales = data
                    .GroupBy(x => new { x.MaNv, x.HoTen })
                    .Select(g => new TopEmployeeBySales
                    {
                        EmployeeId = g.Key.MaNv,
                        EmployeeName = g.Key.HoTen ?? string.Empty,
                        SalesAmount = g.Sum(x => x.Hoadons.Sum(y => y.TienGoc))
                    }).OrderByDescending(x => x.SalesAmount).ToList();
                var averageSalary = data.Average(x => x.MaChucVuNavigation.Luong);
                var totalSalary = data.Sum(x => x.MaChucVuNavigation.Luong);

                response.Data.TotalEmployees = totalEmployees;
                response.Data.TotalActiveEmployees = totalActiveEmployees;
                response.Data.TotalInactiveEmployees = totalInactiveEmployees;
                response.Data.EmployeesByDepartment = data.GroupBy(x => x.MaChucVuNavigation.TenChucVu)
                    .Select(g => new EmployeeByDepartment
                    {
                        DepartmentName = g.Key ?? string.Empty,
                        Count = g.Count()
                    }).ToList();
                response.Data.EmployeesByPosition = employeeByPosition;
                response.Data.TopEmployeesByPerformance = topEmployeesByPerformance;
                response.Data.TopEmployeesBySales = topEmployeesBySales;
                response.Data.AverageSalary = averageSalary;
                response.Data.TotalSalary = totalSalary;


                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }

        #endregion

        #region Doanh thu

        /// <summary>
        /// Lấy thống kê doanh thu
        /// </summary>
        /// <returns>Thống kê doanh thu</returns>
        public async Task<ResponseAPI<RevenueStatisticsResponse>> GetRevenueStatisticsAsync()
        {
            ResponseAPI<RevenueStatisticsResponse> response = new();
            try
            {
                var data = await GetHoadonsAsync();
                if (data?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu doanh thu nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new RevenueStatisticsResponse();
                }
                var totalRevenue = data.Sum(x => x.TienGoc);
                var averageDailyRevenue = data.GroupBy(x => x.NgayTao.Date)
                    .Select(g => g.Sum(x => x.TienGoc))
                    .Average();
                var averageMonthlyRevenue = data.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                    .Select(g => g.Sum(x => x.TienGoc))
                    .Average();
                var highestRevenue = data.Max(x => x.TienGoc);
                var lowestRevenue = data.Min(x => x.TienGoc);
                var revenueByDate = data.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new RevenueByDate
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                var revenueByMonth = data.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                    .Select(g => new RevenueByMonth
                    {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                var revenueByYear = data.GroupBy(x => x.NgayTao.Year)
                    .Select(g => new RevenueByYear
                    {
                        Year = g.Key,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).ToList();
                var topProductByRevenue = data.SelectMany(x => x.Cthoadons)
                    .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                    .Select(g => new TopProductByRevenue
                    {
                        ProductId = g.Key.MaCtsp ?? 0,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Revenue = g.Sum(x => (x.Gia * x.SoLuong) - x.GiamGia)
                    }).OrderByDescending(x => x.Revenue).ToList();
                var topCustomerByRevenue = data.GroupBy(x => new { x.MaKh, x.MaKhNavigation?.HoTen })
                    .Select(g => new TopCustomerByRevenue
                    {
                        CustomerId = g.Key.MaKh,
                        CustomerName = g.Key.HoTen ?? string.Empty,
                        Revenue = g.Sum(x => x.TienGoc)
                    }).OrderByDescending(x => x.Revenue).ToList();
                response.Data.TotalRevenue = totalRevenue;
                response.Data.AverageDailyRevenue = averageDailyRevenue;
                response.Data.AverageMonthlyRevenue = averageMonthlyRevenue;
                response.Data.HighestRevenue = highestRevenue;
                response.Data.LowestRevenue = lowestRevenue;
                response.Data.RevenueByDate = revenueByDate;
                response.Data.RevenueByMonth = revenueByMonth;
                response.Data.RevenueByYear = revenueByYear;
                response.Data.TopProductByRevenue = topProductByRevenue;
                response.Data.TopCustomerByRevenue = topCustomerByRevenue;

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }

        #endregion

        #region Combo

        /// <summary>
        /// Lấy thống kê combo
        /// </summary>
        /// <returns>Thống kê combo</returns>
        public async Task<ResponseAPI<ComboStatisticsResponse>> GetComboStatisticsAsync()
        {
            ResponseAPI<ComboStatisticsResponse> response = new();
            try
            {
                var data = await GetCombosAsync();
                var dataOrder = await GetHoadonsAsync();

                if (data?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu combo nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new ComboStatisticsResponse();
                }
                var totalCombos = data.Count;
                var totalActiveCombos = data.Count(x => x.IsActive ?? false);
                var totalInactiveCombos = totalCombos - totalActiveCombos;
                var totalRevenue = data.Sum(x => x.GiaCombo);
                // var totalDiscount = data.Sum(x => x.GiamGia);
                var averagePrice = totalCombos > 0 ? totalRevenue / totalCombos : 0;
                var topCombosBySales = data
                    .GroupBy(x => new { x.MaCombo, x.TenCombo })
                    .Select(g => new TopComboBySales
                    {
                        ComboId = g.Key.MaCombo,
                        ComboName = g.Key.TenCombo ?? string.Empty,
                        SalesCount = g.Sum(x => x.Cthoadons.Sum(y => y.SoLuong))
                    }).OrderByDescending(x => x.SalesCount).ToList();
                var topCombosByRevenue = data
                    .GroupBy(x => new { x.MaCombo, x.TenCombo })
                    .Select(g => new TopComboByRevenue
                    {
                        ComboId = g.Key.MaCombo,
                        ComboName = g.Key.TenCombo ?? string.Empty,
                        Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia * y?.SoLuong) - y?.GiamGia ?? 0))
                    }).OrderByDescending(x => x.Revenue).ToList();
                /* var comboSalesByDate = data.GroupBy(x => x.NgayTao.Date)
                    .Select(g => new ComboSalesByDate
                    {
                        Date = g.Key,
                        SalesCount = g.Sum(x => x.Cthoadons.Sum(y => y.SoLuong))
                    }).ToList(); */
                /* var comboByCategory = data.GroupBy(x => x.Chitietcombohoadons?.MaDanhMuc)
                    .Select(g => new ComboByCategory
                    {
                        CategoryName = g.Key?.TenDanhMuc ?? string.Empty,
                        Count = g.Count()
                    }).ToList(); */

                var totalComboRevenue = dataOrder.Sum(x => x.Chitietcombohoadons.Sum(cto => cto.DonGia));

                response.Data.TotalCombos = totalCombos;
                response.Data.TotalActiveCombos = totalActiveCombos;
                response.Data.TotalInactiveCombos = totalInactiveCombos;
                // response.Data.CombosByCategory = combosByCategory;
                response.Data.TopCombosBySales = topCombosBySales;
                response.Data.TopCombosByRevenue = topCombosByRevenue;
                // response.Data.AverageComboPrice = averageComboPrice;
                response.Data.TotalComboRevenue = totalComboRevenue;
                // response.Data.ComboSalesByDate = comboSalesByDate;

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
                throw;
            }
            return response;
        }

        #endregion


        #region [PRIVATE METHOD]
        private async Task<List<Hoadon>> GetHoadonsAsync()
        {
            var data = await _context.Hoadons
                            .Include(x => x.Cthoadons)
                                .ThenInclude(x => x.MaCtspNavigation)
                                    .ThenInclude(x => x.MaSpNavigation)
                            .Include(x => x.MaKhNavigation)
                            .Include(x => x.MaCodeNavigation)
                            .ToListAsync();
            return data;
        }
        private async Task<List<Sanpham>> GetSanphamsAsync()
        {
            var data = await _context.Sanphams
                            .Include(x => x.Chitietsanphams)
                            .Include(x => x.Chitietdanhmucs)
                            .ToListAsync();
            return data;
        }
        private async Task<List<Khachhang>> GetKhachhangsAsync()
        {
            var data = await _context.Khachhangs
                            .Include(x => x.Hoadons)
                            .ToListAsync();
            return data;
        }
        private async Task<List<Nhanvien>> GetNhanviensAsync()
        {
            var data = await _context.Nhanviens
                            .Include(x => x.Hoadons)
                            .ToListAsync();
            return data;
        }
        private async Task<List<Combo>> GetCombosAsync()
        {
            var data = await _context.Combos
                            .Include(x => x.Chitietcombohoadons)
                            .ToListAsync();
            return data;
        }
        private async Task<List<Chitietcombohoadon>> GetChitietcombohoadonsAsync()
        {
            var data = await _context.Chitietcombohoadons
                            .Include(x => x.MaComboNavigation)
                            .Include(x => x.MaHdNavigation)
                            .ToListAsync();
            return data;
        }
        #endregion
    }
}