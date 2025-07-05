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
        /// Lấy thông tin thống kê đơn hàng
        /// </summary>
        /// <returns>Thông tin thống kê đơn hàng</returns>
        public async Task<ResponseAPI<OrderSummaryResponse>> GetOrderSummaryByOrder()
        {
            ResponseAPI<OrderSummaryResponse> response = new();
            try
            {
                var dataMain = await GetHoadonsAsync();

                if (!dataMain.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu đơn hàng nào trong hệ thống.", 404);
                    return response;
                }

                var totalOrders = dataMain.Count;
                var totalRevenue = dataMain.Sum(x => x.TienGoc);
                var totalShippingFee = dataMain.Sum(x => x.PhiVanChuyen);
                var totalDiscount = dataMain.Sum(x => x.MaCodeNavigation != null ? (decimal?)(x.MaCodeNavigation.SoTienGiam) : 0);
                var totalCustomers = dataMain.Where(x => x.MaKh.HasValue).Select(x => x.MaKh).Distinct().Count();
                var totalProducts = dataMain.SelectMany(x => x.Cthoadons ?? Enumerable.Empty<Cthoadon>()).Count();

                response.Data = new OrderSummaryResponse
                {
                    TotalShippingFee = totalShippingFee,
                    TotalDiscount = totalDiscount,
                    TotalCustomers = totalCustomers,
                    TotalProducts = totalProducts,
                    OrderStatusStatistics = GetOrderStatusByTime(dataMain),
                    RevenueByTime = GetRevenueByTime(dataMain),
                };

                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private static Dictionary<string, List<OrderStatusStatistics>> GetOrderStatusByTime(IEnumerable<Hoadon> dataMain)
        {
            Dictionary<string, List<OrderStatusStatistics>> keyTimePairStatus = new();

            var now = DateTime.Now;
            // Tính ngày đầu tuần (giả sử tuần bắt đầu từ thứ 2)
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = now.Date.AddDays(-1 * diff);
            var weekEnd = weekStart.AddDays(7);

            // Lọc ngày trong tuần hiện tại
            var weekData = dataMain.Where(x => x.NgayTao.Date >= weekStart && x.NgayTao.Date < weekEnd);

            keyTimePairStatus["date"] = weekData
                .GroupBy(x => x.TinhTrang)
                .Select(g => new OrderStatusStatistics
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            // Lọc tháng trong năm hiện tại
            var year = now.Year;
            var monthData = dataMain.Where(x => x.NgayTao.Year == year);

            keyTimePairStatus["month"] = monthData
                .GroupBy(x => x.TinhTrang)
                .Select(g => new OrderStatusStatistics
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            keyTimePairStatus["year"] = dataMain.GroupBy(x => x.TinhTrang)
                .Select(g => new OrderStatusStatistics
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            return keyTimePairStatus;
        }
        /// <summary>
        /// Lấy danh sách dữ liệu thống kê về các sản phẩm tiềm năng 
        /// </summary>
        /// <param name="dataOrder"></param>
        /// <param name="dataProduct"></param>
        /// <returns></returns>
        private static List<TopProduct> GetTopProducts(IEnumerable<Hoadon> dataOrder, IEnumerable<Sanpham> dataProduct)
        {
            // Tạo dictionary ánh xạ MaSp -> Tên danh mục cha
            var productCategoryDict = dataProduct
                .ToDictionary(
                    sp => sp.MaSp,
                    sp => sp.Chitietdanhmucs != null && sp.Chitietdanhmucs.Any() && sp.Chitietdanhmucs.First().MaDanhMucChaNavigation != null
                        ? sp.Chitietdanhmucs.First().MaDanhMucChaNavigation.TenDanhMucCha ?? string.Empty
                        : string.Empty
                );

            // Gom tất cả các chi tiết hóa đơn
            var allDetails = dataOrder
                .SelectMany(x => x.Cthoadons ?? Enumerable.Empty<Cthoadon>())
                .Where(x => x.MaCtspNavigation != null && x.MaCtspNavigation.MaSpNavigation != null)
                .ToList();

            // Gom nhóm theo MaSp (sản phẩm cha)
            var topProducts = allDetails
                .Where(x => x.MaCtspNavigation != null)
                .GroupBy(x => x.MaCtspNavigation!.MaSp)
                .Select(g =>
                {
                    var maSp = g.Key;
                    var product = dataProduct.FirstOrDefault(sp => sp.MaSp == maSp);
                    var productName = product?.TenSanPham ?? "N/A";
                    productCategoryDict.TryGetValue(maSp, out var categoryName);

                    // Lấy danh sách chi tiết sản phẩm (ctsp) bán chạy nhất của sản phẩm này
                    var detailTopProducts = g
                        .GroupBy(x => x.MaCtsp)
                        .Select(ctspGroup =>
                        {
                            var ctsp = ctspGroup.First().MaCtspNavigation;
                            return new DetailTopProduct(
                                ctsp?.MaCtsp ?? 0,
                                ctsp?.MaSp ?? 0,
                                ctsp?.KichThuoc ?? string.Empty,
                                ctsp?.MauSac ?? string.Empty,
                                ctsp?.SoLuongTon ?? 0,
                                ctsp?.DonGia ?? 0,
                                ctsp?.Hinhanhs != null && ctsp.Hinhanhs.Any() ? ctsp.Hinhanhs.First().TenHinhAnh ?? string.Empty : string.Empty,
                                ctsp?.Cthoadons?.Sum(cthd => cthd.DanhGia?.SoSao ?? 0) ?? 0,
                                ctsp?.IsActive ?? false
                            );
                        })
                        .OrderByDescending(dtp =>
                            g.Where(x => x.MaCtsp == dtp.MaCtsp)
                             .Sum(x => (x.SoLuong))
                        )
                        .ToList();

                    return new TopProduct
                    {
                        ProductId = maSp,
                        ProductName = productName,
                        CategoryName = categoryName ?? string.Empty,
                        Revenue = g.Sum(x => (x.Gia * x.SoLuong)),
                        Count = g.Sum(x => x.SoLuong),
                        DetailTopProducts = detailTopProducts
                    };
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return topProducts;
        }
        /// <summary>
        /// Lấy danh sách dữ liệu thống kê về những khách hàng tiềm năng
        /// </summary>
        /// <param name="dataOrder">Dữ liệu đơn hàng gốc để xử lí</param>
        /// <returns></returns>
        private static List<TopCustomer> GetTopCustomers(IEnumerable<Hoadon> dataOrder)
        {
            // Tạo dictionary để truy xuất nhanh các đơn hàng theo mã khách hàng
            var ordersByCustomer = dataOrder
                .Where(x => x != null && x.MaKhNavigation != null && x.MaKh.HasValue)
                .GroupBy(x => x.MaKh!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.NgayTao).ToList());

            return ordersByCustomer.Select(kvp =>
            {
                var customerOrders = kvp.Value;
                var customer = customerOrders.First().MaKhNavigation;
                var ageGroup = "Không xác định";
                if (customer?.NgaySinh.HasValue == true)
                {
                    var age = DateTime.Now.Year - customer.NgaySinh.Value.Year;
                    if (age < 18) ageGroup = "Dưới 18";
                    else if (age < 30) ageGroup = "18-29";
                    else if (age < 40) ageGroup = "30-39";
                    else if (age < 50) ageGroup = "40-49";
                    else ageGroup = "50+";
                }

                return new TopCustomer
                {
                    CustomerId = customer?.MaKh ?? 0,
                    CustomerName = customer?.HoTen ?? "N/A",
                    Count = customerOrders.Count,
                    Revenue = customerOrders.Sum(x => x.TienGoc - x.PhiVanChuyen),
                    Location = customer?.DiaChi ?? "N/A",
                    AgeGroup = ageGroup,
                    OrderRecents = customerOrders
                        .Take(3)
                        .Select(o => new OrderRecentTopUser(
                            o.MaHd,
                            customer?.MaKh ?? 0,
                            customer?.HoTen ?? "N/A",
                            o.MaNv,
                            o.MaCode,
                            o.NgayTao,
                            o.DiaChiNhanHang ?? string.Empty,
                            o.HinhThucTt ?? string.Empty,
                            o.TinhTrang ?? string.Empty,
                            o.MoTa ?? string.Empty,
                            o.Sdt ?? string.Empty,
                            o.IsActive,
                            o.PhiVanChuyen,
                            o.TienGoc
                        )).ToList()
                };
            })
            .OrderByDescending(x => x.Count)
            .ToList();
        }
        /// <summary>
        /// Lấy danh sách thống kê nhân viên tiềm năng
        /// </summary>
        /// <param name="dataOrder"></param>
        /// <param name="dataEmployee"></param>
        /// <returns></returns>
        private static List<TopEmployee> GetTopEmployees(IEnumerable<Hoadon> dataOrder, IEnumerable<Nhanvien> dataEmployee)
        {
            // Tạo dictionary mã nhân viên -> danh sách hóa đơn đã sắp xếp mới nhất
            var ordersByEmployee = dataOrder
                .Where(x => x.MaNv != null && x.MaNvNavigation != null)
                .GroupBy(x => x.MaNv ?? 0)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.NgayTao).ToList());

            return dataEmployee
                .Where(e => e.MaChucVuNavigation != null)
                .Select(e =>
                {
                    ordersByEmployee.TryGetValue(e.MaNv, out var employeeOrders);
                    employeeOrders ??= new List<Hoadon>();

                    return new TopEmployee
                    {
                        EmployeeId = e.MaNv,
                        EmployeeName = e.HoTen ?? string.Empty,
                        PerformanceScore = (e.IsActive ?? false) ? 1 : 0,
                        PositionName = e.MaChucVuNavigation.TenChucVu ?? string.Empty,
                        Count = employeeOrders.Count,
                        SalesAmount = employeeOrders.Sum(x => x.TienGoc),
                        OrderRecents = employeeOrders
                            .Take(3)
                            .Select(o => new OrderRecentTopUser(
                                o.MaHd,
                                o.MaKh.HasValue ? o.MaKh.Value : 0,
                                o.MaKhNavigation?.HoTen ?? "N/A",
                                o.MaNv ?? 0,
                                o.MaCode ?? string.Empty,
                                o.NgayTao,
                                o.DiaChiNhanHang ?? string.Empty,
                                o.HinhThucTt ?? string.Empty,
                                o.TinhTrang ?? string.Empty,
                                o.MoTa ?? string.Empty,
                                o.Sdt ?? string.Empty,
                                o.IsActive,
                                o.PhiVanChuyen,
                                o.TienGoc
                            )).ToList()
                    };
                })
                .OrderByDescending(x => x.SalesAmount)
                .ToList();
        }
        private static Dictionary<string, List<RevenueByTime>> GetRevenueByTime(IEnumerable<Hoadon> dataMain)
        {
            Dictionary<string, List<RevenueByTime>> responseGet = new();

            var now = DateTime.Now;
            // Tính ngày đầu tuần (giả sử tuần bắt đầu từ thứ 2)
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = now.Date.AddDays(-1 * diff);
            var weekEnd = weekStart.AddDays(7);

            // Doanh thu theo ngày trong tuần hiện tại
            var weekData = dataMain.Where(x => x.NgayTao.Date >= weekStart && x.NgayTao.Date < weekEnd);
            responseGet["date"] = weekData
                .GroupBy(x => x.NgayTao.Date)
                .Select(g => new RevenueByTime
                {
                    Date = g.Key,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();

            // Doanh thu theo tháng trong năm hiện tại
            var year = now.Year;
            var monthData = dataMain.Where(x => x.NgayTao.Year == year);
            responseGet["month"] = monthData
                .GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                .Select(g => new RevenueByTime
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();

            // Doanh thu theo năm (tất cả các năm)
            responseGet["year"] = dataMain.GroupBy(x => x.NgayTao.Year)
                .Select(g => new RevenueByTime
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();

            return responseGet;
        }
        #endregion

        #region Sản phẩm

        /// <summary>
        /// Lấy thống kê sản phẩm (Đã tối ưu và sửa lỗi)
        /// </summary>
        /// <returns>Thống kê sản phẩm</returns>
        public async Task<ResponseAPI<ProductStatisticsResponse>> GetProductStatisticsAsync()
        {
            ResponseAPI<ProductStatisticsResponse> response = new();
            try
            {
                var products = _context.Sanphams.AsNoTracking();
                var orders = _context.Hoadons.Include(h => h.Cthoadons).AsNoTracking();

                if (!await products.AnyAsync())
                {
                    response.SetErrorResponse("Không có dữ liệu sản phẩm nào trong hệ thống.", 404);
                    return response;
                }

                var totalRevenue = await orders.SelectMany(o => o.Cthoadons).SumAsync(od => od.Gia * od.SoLuong);
                var productsSoldCount = await orders.SelectMany(o => o.Cthoadons).SumAsync(od => od.SoLuong);

                var priceStats = await products.SelectMany(p => p.Chitietsanphams).Select(ctsp => ctsp.DonGia).ToListAsync();
                var averagePrice = priceStats.Any() ? (decimal)priceStats.Average() : 0;

                var productStats = await products.GroupBy(p => 1).Select(g => new
                {
                    TotalProducts = g.Count(),
                    TotalActiveProducts = g.Count(p => p.IsActive ?? false),
                }).SingleAsync();

                response.Data = new ProductStatisticsResponse
                {
                    TotalProducts = productStats.TotalProducts,
                    TotalActiveProducts = productStats.TotalActiveProducts,
                    TotalInactiveProducts = productStats.TotalProducts - productStats.TotalActiveProducts,
                    TotalRevenue = totalRevenue,
                    ProductsSoldCount = productsSoldCount,
                    AveragePrice = averagePrice,
                    SalesByTimes = GetSalesByTimes(await orders.ToListAsync())
                };

                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        private static Dictionary<string, List<SalesByTime>> GetSalesByTimes(IEnumerable<Hoadon> dataOrder)
        {
            Dictionary<string, List<SalesByTime>> keyValuePairs = new();

            var now = DateTime.Now;
            // Tính ngày đầu tuần (giả sử tuần bắt đầu từ thứ 2)
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = now.Date.AddDays(-1 * diff);
            var weekEnd = weekStart.AddDays(7);

            // Theo ngày trong tuần hiện tại
            var weekData = dataOrder.Where(x => x.NgayTao.Date >= weekStart && x.NgayTao.Date < weekEnd);
            keyValuePairs["date"] = weekData
                .GroupBy(x => x.NgayTao.Date)
                .Select(g => new SalesByTime
                {
                    Date = g.Key,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.Cthoadons?.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0)) ?? 0),
                    Count = g.Count()
                }).ToList();

            // Theo tháng trong năm hiện tại
            var year = now.Year;
            var monthData = dataOrder.Where(x => x.NgayTao.Year == year);
            keyValuePairs["month"] = monthData
                .GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                .Select(g => new SalesByTime
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.Cthoadons?.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0)) ?? 0),
                    Count = g.Count()
                }).ToList();

            // Theo năm (tất cả các năm)
            keyValuePairs["year"] = dataOrder.GroupBy(x => x.NgayTao.Year)
                .Select(g => new SalesByTime
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.Cthoadons?.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0)) ?? 0),
                    Count = g.Count()
                }).ToList();

            return keyValuePairs;
        }
        #endregion

        #region Khách hàng

        /// <summary>
        /// Lấy thống kê khách hàng (Đã tối ưu và bổ sung dữ liệu)
        /// </summary>
        /// <returns>Thống kê khách hàng</returns>
        public async Task<ResponseAPI<CustomerStatisticsResponse>> GetCustomerStatisticsAsync()
        {
            ResponseAPI<CustomerStatisticsResponse> response = new();
            try
            {
                var customerData = await _context.Khachhangs
                    .Select(kh => new
                    {
                        IsActive = kh.IsActive ?? false,
                        TotalRevenue = kh.Hoadons.Sum(h => h.TienGoc),
                        OrderCount = kh.Hoadons.Count(),
                        FirstOrderDate = kh.Hoadons.Any() ? kh.Hoadons.Min(h => h.NgayTao) : (DateTime?)null
                    })
                    .ToListAsync();

                if (!customerData.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu khách hàng nào trong hệ thống.", 404);
                    return response;
                }

                var totalCustomers = customerData.Count;
                var totalActiveCustomers = customerData.Count(x => x.IsActive);
                var totalRevenue = customerData.Sum(x => x.TotalRevenue);
                var totalOrders = customerData.Sum(x => x.OrderCount);
                var averagePurchaseAmount = totalCustomers > 0 ? totalRevenue / totalCustomers : 0;
                var averageOrdersPerCustomer = totalCustomers > 0 ? (decimal)totalOrders / totalCustomers : 0;

                // Tính toán hoạt động khách hàng theo thời gian (số khách hàng mới theo tháng trong năm hiện tại)
                var currentYear = DateTime.Now.Year;
                var customerActivity = customerData
                    .Where(x => x.FirstOrderDate.HasValue && x.FirstOrderDate.Value.Year == currentYear)
                    .GroupBy(x => x.FirstOrderDate!.Value.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToDictionary(g => $"Tháng {g.Month}/{currentYear}", g => g.Count);

                response.Data = new CustomerStatisticsResponse
                {
                    TotalCustomers = totalCustomers,
                    TotalActiveCustomers = totalActiveCustomers,
                    TotalInactiveCustomers = totalCustomers - totalActiveCustomers,
                    TotalPurchaseAmount = totalRevenue,
                    AveragePurchaseAmount = averagePurchaseAmount,
                    TotalOrders = totalOrders,
                    AverageOrdersPerCustomer = averageOrdersPerCustomer,
                    CustomerActivityByTime = customerActivity
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }


        #endregion

        #region Nhân viên

        /// <summary>
        /// Lấy thống kê nhân viên (Đã tối ưu)
        /// </summary>
        /// <returns>Thống kê nhân viên</returns>
        public async Task<ResponseAPI<EmployeeStatisticsResponse>> GetEmployeeStatisticsAsync()
        {
            ResponseAPI<EmployeeStatisticsResponse> response = new();
            try
            {
                var employeeStats = await _context.Nhanviens
                    .GroupBy(e => 1)
                    .Select(g => new
                    {
                        TotalEmployees = g.Count(),
                        TotalActiveEmployees = g.Count(e => e.IsActive ?? false),
                        TotalSalary = g.Sum(e => e.MaChucVuNavigation.Luong),
                        AverageSalary = g.Average(e => e.MaChucVuNavigation.Luong)
                    })
                    .FirstOrDefaultAsync();

                if (employeeStats == null)
                {
                    response.SetErrorResponse("Không có dữ liệu nhân viên nào trong hệ thống.", 404);
                    return response;
                }

                response.Data = new EmployeeStatisticsResponse
                {
                    TotalEmployees = employeeStats.TotalEmployees,
                    TotalActiveEmployees = employeeStats.TotalActiveEmployees,
                    TotalInactiveEmployees = employeeStats.TotalEmployees - employeeStats.TotalActiveEmployees,
                    AverageSalary = employeeStats.AverageSalary,
                    TotalSalary = employeeStats.TotalSalary
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Doanh thu

        /// <summary>
        /// Lấy thống kê doanh thu (Đã tối ưu)
        /// </summary>
        /// <returns>Thống kê doanh thu</returns>
        public async Task<ResponseAPI<RevenueStatisticsResponse>> GetRevenueStatisticsAsync()
        {
            ResponseAPI<RevenueStatisticsResponse> response = new();
            try
            {
                var hoadons = _context.Hoadons.AsNoTracking();

                if (!await hoadons.AnyAsync())
                {
                    response.SetErrorResponse("Không có dữ liệu doanh thu nào trong hệ thống.", 404);
                    return response;
                }

                var totalRevenue = await hoadons.SumAsync(x => x.TienGoc);
                var highestRevenue = await hoadons.MaxAsync(x => x.TienGoc);
                var lowestRevenue = await hoadons.MinAsync(x => x.TienGoc);

                var dailyRevenue = await hoadons
                    .GroupBy(x => x.NgayTao.Date)
                    .Select(g => g.Sum(x => x.TienGoc))
                    .ToListAsync();

                var monthlyRevenue = await hoadons
                    .GroupBy(x => new { x.NgayTao.Year, x.NgayTao.Month })
                    .Select(g => g.Sum(x => x.TienGoc))
                    .ToListAsync();

                response.Data = new RevenueStatisticsResponse
                {
                    TotalRevenue = totalRevenue,
                    AverageDailyRevenue = dailyRevenue.Any() ? dailyRevenue.Average() : 0,
                    AverageMonthlyRevenue = monthlyRevenue.Any() ? monthlyRevenue.Average() : 0,
                    HighestRevenue = highestRevenue,
                    LowestRevenue = lowestRevenue,
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Combo

        /// <summary>
        /// Lấy thống kê combo (Đã sửa và tối ưu)
        /// </summary>
        /// <returns>Thống kê combo</returns>
        public async Task<ResponseAPI<ComboStatisticsResponse>> GetComboStatisticsAsync()
        {
            ResponseAPI<ComboStatisticsResponse> response = new();
            try
            {
                var comboStats = await _context.Combos
                    .Select(c => new
                    {
                        IsActive = c.IsActive ?? false,
                        Revenue = c.Chitietcombohoadons.Sum(ch => ch.DonGia * ch.SoLuong),
                        QuantitySold = c.Chitietcombohoadons.Sum(ch => ch.SoLuong)
                    })
                    .ToListAsync();

                if (!comboStats.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu combo nào trong hệ thống.", 404);
                    return response;
                }

                var totalCombos = comboStats.Count;
                var totalActiveCombos = comboStats.Count(c => c.IsActive);
                var totalRevenue = comboStats.Sum(c => c.Revenue);
                var totalQuantitySold = comboStats.Sum(c => c.QuantitySold);

                response.Data = new ComboStatisticsResponse
                {
                    TotalCombos = totalCombos,
                    TotalActiveCombos = totalActiveCombos,
                    TotalInactiveCombos = totalCombos - totalActiveCombos,
                    TotalComboRevenue = totalRevenue,
                    AverageComboPrice = totalQuantitySold > 0 ? totalRevenue / totalQuantitySold : 0
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Coupon

        /// <summary>
        /// Lấy thống kê mã giảm giá
        /// </summary>
        /// <returns>Thống kê mã giảm giá</returns>
        public async Task<ResponseAPI<CouponStatisticsResponse>> GetCouponStatisticsAsync()
        {
            ResponseAPI<CouponStatisticsResponse> response = new();
            try
            {
                var coupons = _context.Macoupons.AsNoTracking();

                if (!await coupons.AnyAsync())
                {
                    response.SetErrorResponse("Không có dữ liệu mã giảm giá nào trong hệ thống.", 404);
                    return response;
                }

                var couponStats = await coupons
                    .Select(c => new
                    {
                        UsageCount = c.Hoadons.Count(),
                        TotalDiscount = c.Hoadons.Sum(h => !string.IsNullOrEmpty(h.MaCode) && h.MaCodeNavigation != null ? (decimal?)h.MaCodeNavigation.SoTienGiam : 0),
                        RevenueGenerated = c.Hoadons.Sum(h => h.TienGoc)
                    })
                    .ToListAsync();

                var topCoupons = await coupons
                    .OrderByDescending(c => c.Hoadons.Count())
                    .Take(10)
                    .Select(c => new TopCoupon
                    {
                        CouponCode = c.MaCode,
                        UsageCount = c.Hoadons.Count(),
                        TotalDiscount = c.Hoadons.Sum(h => !string.IsNullOrEmpty(h.MaCode) && h.MaCodeNavigation != null ? (decimal?)h.MaCodeNavigation.SoTienGiam : 0),
                        RevenueGenerated = c.Hoadons.Sum(h => h.TienGoc)
                    })
                    .ToListAsync();

                response.Data = new CouponStatisticsResponse
                {
                    TotalCoupons = couponStats.Count(),
                    TotalActiveCoupons = 0, // Assuming no IsActive field, set to 0 or implement logic if needed
                    TotalInactiveCoupons = 0,
                    TotalDiscountAmount = couponStats.Sum(c => c.TotalDiscount),
                    TopCoupons = topCoupons
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Category

        /// <summary>
        /// Lấy thống kê danh mục
        /// </summary>
        /// <returns>Thống kê danh mục</returns>
        public async Task<ResponseAPI<CategoryStatisticsResponse>> GetCategoryStatisticsAsync()
        {
            ResponseAPI<CategoryStatisticsResponse> response = new();
            try
            {
                var categories = _context.Danhmuccons.AsNoTracking();

                if (!await categories.AnyAsync())
                {
                    response.SetErrorResponse("Không có dữ liệu danh mục nào trong hệ thống.", 404);
                    return response;
                }

                var topCategories = await _context.Chitietdanhmucs
                    .AsNoTracking()
                    .GroupBy(cd => cd.MaDanhMucConNavigation.TenDanhMucCon)
                    .Select(g => new TopCategory
                    {
                        CategoryName = g.Key,
                        ProductsSoldCount = g.SelectMany(cd => cd.MaSpNavigation.Chitietsanphams.SelectMany(ctsp => ctsp.Cthoadons)).Sum(cthd => cthd.SoLuong),
                        TotalRevenue = g.SelectMany(cd => cd.MaSpNavigation.Chitietsanphams.SelectMany(ctsp => ctsp.Cthoadons)).Sum(cthd => cthd.Gia * cthd.SoLuong)
                    })
                    .OrderByDescending(c => c.TotalRevenue)
                    .Take(10)
                    .ToListAsync();

                response.Data = new CategoryStatisticsResponse
                {
                    TotalCategories = await categories.CountAsync(),
                    TopCategories = topCategories
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Inventory

        /// <summary>
        /// Lấy phân tích tồn kho
        /// </summary>
        /// <returns>Phân tích tồn kho</returns>
        public async Task<ResponseAPI<InventoryAnalysisResponse>> GetInventoryAnalysisAsync()
        {
            ResponseAPI<InventoryAnalysisResponse> response = new();
            try
            {
                var lowStockProducts = await _context.Chitietsanphams
                    .AsNoTracking()
                    .Where(p => p.SoLuongTon < 10) // Cảnh báo khi số lượng tồn kho dưới 10
                    .Select(p => new LowStockProduct
                    {
                        ProductId = p.MaSp,
                        ProductName = p.MaSpNavigation.TenSanPham,
                        StockQuantity = p.SoLuongTon
                    })
                    .ToListAsync();

                response.Data = new InventoryAnalysisResponse
                {
                    LowStockProducts = lowStockProducts
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region Review

        /// <summary>
        /// Lấy phân tích đánh giá
        /// </summary>
        /// <returns>Phân tích đánh giá</returns>
        public async Task<ResponseAPI<ReviewAnalysisResponse>> GetReviewAnalysisAsync()
        {
            ResponseAPI<ReviewAnalysisResponse> response = new();
            try
            {
                var reviews = _context.DanhGias.AsNoTracking();

                if (!await reviews.AnyAsync())
                {
                    response.SetErrorResponse("Không có dữ liệu đánh giá nào trong hệ thống.", 404);
                    return response;
                }

                var productReviews = await _context.Sanphams
                    .AsNoTracking()
                    .Select(p => new ProductReviewSummary
                    {
                        ProductId = p.MaSp,
                        ProductName = p.TenSanPham,
                        AverageRating = p.DanhGias.Any() ? p.DanhGias.Average(r => r.SoSao) : 0,
                        ReviewCount = p.DanhGias.Count()
                    })
                    .ToListAsync();

                response.Data = new ReviewAnalysisResponse
                {
                    AverageRating = await reviews.AverageAsync(r => r.SoSao),
                    MostReviewedProduct = productReviews.OrderByDescending(p => p.ReviewCount).FirstOrDefault(),
                    HighestRatedProduct = productReviews.OrderByDescending(p => p.AverageRating).FirstOrDefault(),
                    LowestRatedProduct = productReviews.OrderBy(p => p.AverageRating).FirstOrDefault()
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #endregion

        #region [Mix datatable]
        public async Task<ResponseAPI<DatatableStatisticsResponse>> GetDatatableStatisticsAsync()
        {
            var response = new ResponseAPI<DatatableStatisticsResponse>();
            try
            {
                // Lấy dữ liệu cần thiết song song để tối ưu hiệu năng
                var dataProduct = await GetSanphamsAsync();
                var dataEmployee = await GetNhanviensAsync();
                var dataCombo = await _context.Combos
                    .Include(c => c.Chitietcombos)
                        .ThenInclude(ctbo => ctbo.MaSpNavigation)
                            .ThenInclude(sp => sp.Chitietsanphams)
                    .Include(c => c.Chitietcombohoadons)
                    .Include(c => c.DanhGias)
                    .Select(c => new
                    {
                        c.MaCombo,
                        c.TenCombo,
                        c.IsActive,
                        SalesCount = c.Chitietcombohoadons.Sum(hoadon => hoadon.SoLuong),
                        Revenue = c.Chitietcombohoadons.Sum(hoadon => (hoadon.DonGia * hoadon.SoLuong)),
                        StarCount = c.DanhGias.Any() ? (int)c.DanhGias.Average(dg => dg.SoSao) : 0,
                        c.Chitietcombos
                    })
                    .AsNoTracking().ToListAsync();
                var dataOrder = await _context.Hoadons
                    .Include(h => h.Cthoadons)
                        .ThenInclude(h => h.MaCtspNavigation)
                            .ThenInclude(h => h.MaSpNavigation)
                    .Include(h => h.Cthoadons)
                        .ThenInclude(h => h.MaCtspNavigation)
                            .ThenInclude(ctsp => ctsp.Hinhanhs)
                    .Include(h => h.Cthoadons)
                        .ThenInclude(ct => ct.DanhGia)
                    .Include(h => h.MaKhNavigation)
                    .Include(h => h.MaCodeNavigation)
                    .AsNoTracking().ToListAsync();
                var dataCustomer = await _context.Khachhangs
                    .Include(x => x.Hoadons)
                    .Select(kh => new
                    {
                        kh.MaKh,
                        kh.HoTen,
                        kh.IsActive,
                        kh.DiaChi,
                        kh.NgaySinh,
                        TotalOrders = kh.Hoadons.Count(),
                        TotalRevenue = kh.Hoadons.Sum(h => h.TienGoc)
                    })
                    .AsNoTracking().ToListAsync();

                var topProducts = GetTopProducts(dataOrder, dataProduct);
                var topCustomers = GetTopCustomers(dataOrder);
                var topEmployees = GetTopEmployees(dataOrder, dataEmployee);

                var topComboss = dataCombo.OrderByDescending(x => x.SalesCount)
                    .Select(x => new TopCombo
                    {
                        ComboId = x.MaCombo,
                        ComboName = x.TenCombo ?? string.Empty,
                        SalesCount = x.SalesCount,
                        Revenue = x.Revenue,
                        StarCount = x.StarCount,
                        DetailTopCombos = x.Chitietcombos.Select(ct => new DetailTopCombo(ct)).ToList()
                    }).ToList();

                // Khởi tạo response
                response.Data = new DatatableStatisticsResponse(
                    topProducts,
                    topCustomers,
                    topEmployees,
                    topComboss
                );
                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }
        #endregion


        #region [PRIVATE METHOD]
        private async Task<List<Hoadon>> GetHoadonsAsync()
        {
            List<Hoadon> data = new();

            try
            {
                data = await _context.Hoadons
                                .Include(x => x.Cthoadons)
                                    .ThenInclude(x => x.MaCtspNavigation)
                                        .ThenInclude(x => x.MaSpNavigation)
                                .Include(x => x.MaKhNavigation)
                                .Include(x => x.MaCodeNavigation)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                data = new List<Hoadon>();
                Console.WriteLine($"Lỗi khi lấy dữ liệu hóa đơn: {ex.Message}");
            }
            return data;
        }
        private async Task<List<Sanpham>> GetSanphamsAsync()
        {
            List<Sanpham> data = new();
            try
            {
                data = await _context.Sanphams
                                .Include(x => x.Chitietsanphams)
                                //.ThenInclude(ctsp => ctsp.Hinhanhs)
                                .Include(x => x.Chitietdanhmucs)
                                    .ThenInclude(ctdm => ctdm.MaDanhMucChaNavigation)
                                .Include(x => x.Chitietdanhmucs)
                                    .ThenInclude(ctdm => ctdm.MaDanhMucConNavigation)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu sản phẩm: {ex.Message}");
                data = new List<Sanpham>();
            }
            return data;
        }
        private async Task<List<Khachhang>> GetKhachhangsAsync()
        {
            List<Khachhang> data = new();
            try
            {
                data = await _context.Khachhangs
                                .Include(x => x.Hoadons)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu khách hàng: {ex.Message}");
                data = new List<Khachhang>();
            }
            return data;
        }
        private async Task<List<Nhanvien>> GetNhanviensAsync()
        {
            List<Nhanvien> data = new();
            try
            {
                data = await _context.Nhanviens
                                .Include(x => x.Hoadons)
                                .Include(x => x.MaChucVuNavigation)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu nhân viên: {ex.Message}");
                data = new List<Nhanvien>();
            }
            return data;
        }
        private async Task<List<APIClothesEcommerceShop.Models.Combo>> GetCombosAsync()
        {
            List<APIClothesEcommerceShop.Models.Combo> data = new();
            try
            {
                data = await _context.Combos
                                .Include(x => x.Chitietcombohoadons)
                                    .ThenInclude(x => x.MaHdNavigation)
                                .Include(x => x.Chitietcombohoadons)
                                    .ThenInclude(x => x.MaComboNavigation)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu combo: {ex.Message}");
                data = new List<APIClothesEcommerceShop.Models.Combo>();
            }
            return data;
        }

        private async Task<List<Chitietcombohoadon>> GetChitietcombohoadonsAsync()
        {
            List<Chitietcombohoadon> data = new();
            try
            {
                data = await _context.Chitietcombohoadons
                                .Include(x => x.MaComboNavigation)
                                .Include(x => x.MaHdNavigation)
                                .AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu chi tiết combo hóa đơn: {ex.Message}");
                data = new List<Chitietcombohoadon>();
            }
            return data;
        }
        #endregion
    }
}
