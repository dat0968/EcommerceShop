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
                var totalDiscount = dataMain.Sum(x => x.MaCodeNavigation?.SoTienGiam ?? 0);
                var totalCustomers = dataMain.Select(x => x.MaKh).Distinct().Count();
                var totalProducts = dataMain.SelectMany(x => x.Cthoadons).Count();

                response.Data = new OrderSummaryResponse
                {
                    AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    TotalShippingFee = totalShippingFee,
                    TotalDiscount = totalDiscount,
                    TotalCustomers = totalCustomers,
                    TotalProducts = totalProducts,
                    OrderStatusStatistics = GetOrderStatusStatistics(dataMain),
                    BestSellingProducts = GetBestSellingProducts(dataMain),
                    RevenueByDate = GetRevenueByDate(dataMain),
                    RevenueByMonth = GetRevenueByMonth(dataMain),
                    RevenueByYear = GetRevenueByYear(dataMain),
                    TopProductByOrder = GetTopProductByOrder(dataMain),
                    TopCustomerByOrder = GetTopCustomerByOrder(dataMain)
                };

                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        // Các phương thức phụ trợ để xử lý các tính toán
        private List<OrderStatusStatistics> GetOrderStatusStatistics(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => x.TinhTrang)
                .Select(g => new OrderStatusStatistics
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();
        }

        private List<BestSellingProduct> GetBestSellingProducts(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.SelectMany(x => x.Cthoadons)
                .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                .Select(g => new BestSellingProduct
                {
                    ProductId = g.Key.MaCtsp ?? 0,
                    ProductName = g.Key.TenSanPham ?? string.Empty,
                    Quantity = g.Sum(x => x.SoLuong)
                }).OrderByDescending(x => x.Quantity).ToList();
        }

        private List<RevenueByDate> GetRevenueByDate(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => x.NgayTao.Date)
                .Select(g => new RevenueByDate
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();
        }

        private List<RevenueByMonth> GetRevenueByMonth(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                .Select(g => new RevenueByMonth
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();
        }

        private List<RevenueByYear> GetRevenueByYear(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => x.NgayTao.Year)
                .Select(g => new RevenueByYear
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();
        }

        private List<TopProductByOrder> GetTopProductByOrder(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.SelectMany(x => x.Cthoadons)
                .GroupBy(x => new { x.MaCtsp, x.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                .Select(g => new TopProductByOrder
                {
                    ProductId = g.Key.MaCtsp ?? 0,
                    ProductName = g.Key.TenSanPham ?? string.Empty,
                    Count = g.Count()
                }).OrderByDescending(x => x.Count).ToList();
        }

        private List<TopCustomerByOrder> GetTopCustomerByOrder(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => new { x.MaKh, x.MaKhNavigation?.HoTen })
                .Select(g => new TopCustomerByOrder
                {
                    CustomerId = g.Key.MaKh,
                    CustomerName = g.Key.HoTen ?? string.Empty,
                    Count = g.Count()
                }).OrderByDescending(x => x.Count).ToList();
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

                if (!dataMain.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu sản phẩm nào trong hệ thống.", 404);
                    return response;
                }

                response.Data = new ProductStatisticsResponse
                {
                    TotalProducts = dataMain.Count,
                    TotalActiveProducts = dataMain.Count(x => x.IsActive ?? false),
                    TotalInactiveProducts = dataMain.Count(x => !x.IsActive ?? false),
                    TotalRevenue = dataMain.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia)),
                    TotalDiscount = 0, // ! Cần phải tính toán lại
                    AveragePrice = dataMain.Count > 0 ? dataMain.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia)) / dataMain.Count : 0,
                    BestSellingProducts = GetBestSellingProducts(dataMain),
                    ProductCategoryStatistics = GetProductCategoryStatistics(dataMain, dataCategory),
                    SalesByDate = GetSalesByDate(dataOrder),
                    SalesByMonth = GetSalesByMonth(dataOrder),
                    SalesByYear = GetSalesByYear(dataOrder)
                };

                response.SetSuccessResponse();
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        // Các phương thức phụ trợ
        private List<BestSellingProduct> GetBestSellingProducts(IEnumerable<Sanpham> dataMain)
        {
            return dataMain.SelectMany(x => x.Chitietsanphams)
                .GroupBy(x => new { x.MaCtsp, x.MaSpNavigation?.TenSanPham })
                .Select(g => new BestSellingProduct
                {
                    ProductId = g.Key.MaCtsp,
                    ProductName = g.Key.TenSanPham ?? string.Empty,
                    Quantity = g.Sum(x => x.SoLuongTon)
                }).OrderByDescending(x => x.Quantity).ToList();
        }

        private List<ProductCategoryStatistics> GetProductCategoryStatistics(IEnumerable<Sanpham> dataMain, IEnumerable<Danhmuccha> dataCategory)
        {
            return dataMain.GroupBy(x => x.Chitietdanhmucs?.Select(y => y.MaDanhMucChaNavigation))
                .Select(g => new ProductCategoryStatistics
                {
                    CategoryName = g.Key?.FirstOrDefault()?.TenDanhMucCha ?? string.Empty,
                    TotalProducts = g.Count(),
                    TotalRevenue = g.Sum(x => x.Chitietsanphams.Sum(y => y.DonGia))
                }).ToList();
        }

        private List<SalesByDate> GetSalesByDate(IEnumerable<Hoadon> dataOrder)
        {
            return dataOrder.GroupBy(x => x.NgayTao.Date)
                .Select(g => new SalesByDate
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
        }
        private List<SalesByMonth> GetSalesByMonth(IEnumerable<Hoadon> dataOrder)
        {
            return dataOrder.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                .Select(g => new SalesByMonth
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
        }
        private List<SalesByYear> GetSalesByYear(IEnumerable<Hoadon> dataOrder)
        {
            return dataOrder.GroupBy(x => x.NgayTao.Year)
                .Select(g => new SalesByYear
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
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
                var data = await _context.Khachhangs
                    .Include(x => x.Hoadons) // Tải trước hóa đơn liên quan
                    .Select(kh => new
                    {
                        kh.MaKh,
                        kh.HoTen,
                        kh.IsActive,
                        kh.DiaChi,
                        kh.NgaySinh,
                        TotalOrders = kh.Hoadons.Count(),
                        TotalRevenue = kh.Hoadons.Sum(h => h.TienGoc) // Tính toán doanh thu tổng
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu khách hàng nào trong hệ thống.", 404);
                    return response;
                }

                // Tính toán các thống kê cần thiết
                var totalCustomers = data.Count;
                var totalActiveCustomers = data.Count(x => x.IsActive ?? false);
                // Tính toán số lượng khách hàng không hoạt động
                var totalInactiveCustomers = totalCustomers - totalActiveCustomers;

                var totalRevenue = data.Sum(x => x.TotalRevenue);

                // Nhóm khách hàng theo địa chỉ
                var customersByLocation = data.GroupBy(x => x.DiaChi)
                    .Select(g => new CustomerByLocation
                    {
                        Location = g.Key ?? "Dữ liệu không xác định",
                        Count = g.Count()
                    }).ToList();

                // Nhóm khách hàng theo độ tuổi (giả định có trường NgaySinh)
                var customersByAgeGroup = data.GroupBy(x => x?.NgaySinh?.Year / 10) // Nhóm theo độ tuổi (thập kỷ)
                    .Select(g => new CustomerByAgeGroup
                    {
                        AgeGroup = g.Key.HasValue ? $"{g.Key * 10} - {(g.Key + 1) * 10 - 1}" : "Không xác định",
                        Count = g.Count()
                    }).ToList();

                // Top khách hàng theo doanh thu
                var topCustomersByRevenue = data.OrderByDescending(x => x.TotalRevenue)
                    .Take(10) // Chỉ lấy top 10
                    .Select(x => new TopCustomerByRevenue
                    {
                        CustomerId = x.MaKh,
                        CustomerName = x.HoTen ?? string.Empty,
                        Revenue = x.TotalRevenue
                    }).ToList();

                var averagePurchaseAmount = totalCustomers > 0 ? totalRevenue / totalCustomers : 0;

                // Gán kết quả vào response
                response.Data = new CustomerStatisticsResponse
                {
                    TotalCustomers = totalCustomers,
                    TotalActiveCustomers = totalActiveCustomers,
                    TotalInactiveCustomers = totalInactiveCustomers,
                    TotalPurchaseAmount = totalRevenue,
                    CustomersByLocation = customersByLocation,
                    CustomersByAgeGroup = customersByAgeGroup,
                    AveragePurchaseAmount = averagePurchaseAmount,
                    TopCustomersByRevenue = topCustomersByRevenue
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
        /// Lấy thống kê nhân viên
        /// </summary>
        /// <returns>Thống kê nhân viên</returns>
        public async Task<ResponseAPI<EmployeeStatisticsResponse>> GetEmployeeStatisticsAsync()
        {
            ResponseAPI<EmployeeStatisticsResponse> response = new();
            try
            {
                var dataMain = await GetNhanviensAsync();
                if (dataMain?.Any() != true)
                {
                    response.SetErrorResponse("Không có dữ liệu nhân viên nào trong hệ thống.", 404);
                    return response;
                }
                if (response.Data == null)
                {
                    response.Data = new EmployeeStatisticsResponse();
                }
                var totalEmployees = dataMain.Count;
                var totalActiveEmployees = dataMain.Count(x => x.IsActive ?? false);
                var totalInactiveEmployees = totalEmployees - totalActiveEmployees;
                var employeeByPosition = dataMain.GroupBy(x => x.MaChucVuNavigation.TenChucVu)
                    .Select(g => new EmployeeByPosition
                    {
                        PositionName = g.Key ?? string.Empty,
                        Count = g.Count()
                    }).ToList();
                var topEmployeesByPerformance = dataMain
                    .GroupBy(x => new { x.MaNv, x.HoTen })
                    .Select(g => new TopEmployeeByPerformance
                    {
                        EmployeeId = g.Key.MaNv,
                        EmployeeName = g.Key.HoTen ?? string.Empty,
                        PerformanceScore = g.Sum(x => x.TinhTrang == "Hoàn thành" ? 1 : 0)
                    }).OrderByDescending(x => x.PerformanceScore).ToList();

                var topEmployeesBySales = dataMain
                    .GroupBy(x => new { x.MaNv, x.HoTen })
                    .Select(g => new TopEmployeeBySales
                    {
                        EmployeeId = g.Key.MaNv,
                        EmployeeName = g.Key.HoTen ?? string.Empty,
                        SalesAmount = g.Sum(x => x.Hoadons.Sum(y => y.TienGoc))
                    }).OrderByDescending(x => x.SalesAmount).ToList();
                var averageSalary = dataMain.Average(x => x.MaChucVuNavigation.Luong);
                var totalSalary = dataMain.Sum(x => x.MaChucVuNavigation.Luong);

                response.Data.TotalEmployees = totalEmployees;
                response.Data.TotalActiveEmployees = totalActiveEmployees;
                response.Data.TotalInactiveEmployees = totalInactiveEmployees;
                response.Data.EmployeesByDepartment = dataMain.GroupBy(x => x.MaChucVuNavigation.TenChucVu)
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
                var data = await _context.Hoadons
                    .Include(h => h.Cthoadons) // Tải trước chi tiết hóa đơn liên quan
                        .ThenInclude(h => h.MaCtspNavigation) // Tải trước sản phẩm liên quan
                            .ThenInclude(h => h.MaSpNavigation) // Tải trước thông tin sản phẩm
                    .Include(h => h.MaKhNavigation) // Tải trước thông tin khách hàng
                    .Select(static h => new
                    {
                        h.TienGoc,
                        h.NgayTao,
                        h.MaKh,
                        h.MaKhNavigation.HoTen,
                        h.MaCodeNavigation.SoTienGiam,
                        h.PhiVanChuyen,
                        h.TinhTrang,
                        h.MaKhNavigation,
                        // Tải trước thông tin chi tiết hóa đơn
                        Cthoadons = h.Cthoadons.Select(static ct => new
                        {
                            ct.MaCtsp,
                            ct.MaCtspNavigation.MaSpNavigation.TenSanPham,
                            ct.Gia,
                            ct.SoLuong,
                            ct.GiamGia
                        }).ToList()
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu doanh thu nào trong hệ thống.", 404);
                    return response;
                }

                // Khởi tạo thông tin thống kê
                response.Data = new RevenueStatisticsResponse
                {
                    TotalRevenue = data.Sum(x => x.TienGoc),
                    AverageDailyRevenue = data.GroupBy(x => x.NgayTao.Date)
                        .Select(g => g.Sum(x => x.TienGoc))
                        .Average(),
                    AverageMonthlyRevenue = data.GroupBy(x => new { x.NgayTao.Year, x.NgayTao.Month })
                        .Select(g => g.Sum(x => x.TienGoc))
                        .Average(),
                    HighestRevenue = data.Max(x => x.TienGoc),
                    LowestRevenue = data.Min(x => x.TienGoc),
                    RevenueByDate = data.GroupBy(x => x.NgayTao.Date)
                        .Select(g => new RevenueByDate
                        {
                            Date = g.Key,
                            Revenue = g.Sum(x => x.TienGoc)
                        }).ToList(),
                    RevenueByMonth = data.GroupBy(x => new { x.NgayTao.Year, x.NgayTao.Month })
                        .Select(g => new RevenueByMonth
                        {
                            Month = g.Key.Month,
                            Year = g.Key.Year,
                            Revenue = g.Sum(x => x.TienGoc)
                        }).ToList(),
                    RevenueByYear = data.GroupBy(x => x.NgayTao.Year)
                        .Select(g => new RevenueByYear
                        {
                            Year = g.Key,
                            Revenue = g.Sum(x => x.TienGoc)
                        }).ToList(),
                    TopProductByRevenue = data.SelectMany(h => h.Cthoadons)
                        .GroupBy(ct => new { ct.MaCtsp, ct.TenSanPham })
                        .Select(g => new TopProductByRevenue
                        {
                            ProductId = g.Key.MaCtsp ?? 0,
                            ProductName = g.Key.TenSanPham ?? string.Empty,
                            Revenue = g.Sum(ct => (ct.Gia * ct.SoLuong) - ct.GiamGia)
                        }).OrderByDescending(x => x.Revenue).ToList(),
                    TopCustomerByRevenue = data.GroupBy(h => new { h.MaKh, h.HoTen })
                        .Select(g => new TopCustomerByRevenue
                        {
                            CustomerId = g.Key.MaKh,
                            CustomerName = g.Key.HoTen ?? string.Empty,
                            Revenue = g.Sum(h => h.TienGoc)
                        }).OrderByDescending(x => x.Revenue).ToList()
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
        /// Lấy thống kê combo
        /// </summary>
        /// <returns>Thống kê combo</returns>
        public async Task<ResponseAPI<ComboStatisticsResponse>> GetComboStatisticsAsync()
        {
            ResponseAPI<ComboStatisticsResponse> response = new();
            try
            {
                // Kết hợp việc lấy thông tin combo và hóa đơn trong một truy vấn
                var data = await _context.Combos
                    .Include(c => c.Cthoadons) // Tải trước thông tin chi tiết hóa đơn
                    .Select(c => new
                    {
                        c.MaCombo,
                        c.TenCombo,
                        c.GiaCombo,
                        c.IsActive,
                        SalesCount = c.Cthoadons.Sum(hoadon => hoadon.SoLuong),
                        Revenue = c.Cthoadons.Sum(hoadon => (hoadon.Gia * hoadon.SoLuong) - hoadon.GiamGia)
                    })
                    .ToListAsync();

                if (!data.Any())
                {
                    response.SetErrorResponse("Không có dữ liệu combo nào trong hệ thống.", 404);
                    return response;
                }

                // Khởi tạo thông tin thống kê
                response.Data = new ComboStatisticsResponse
                {
                    TotalCombos = data.Count,
                    TotalActiveCombos = data.Count(x => x.IsActive ?? false),
                    TotalInactiveCombos = data.Count(x => !(x.IsActive ?? false)),
                    TotalComboRevenue = data.Sum(x => x.Revenue),
                    AverageComboPrice = data.Count > 0 ? data.Sum(x => x.GiaCombo) / data.Count : 0,
                    TopCombosBySales = data.OrderByDescending(x => x.SalesCount)
                                            .Select(x => new TopComboBySales
                                            {
                                                ComboId = x.MaCombo,
                                                ComboName = x.TenCombo ?? string.Empty,
                                                SalesCount = x.SalesCount
                                            })
                                            .ToList(),
                    TopCombosByRevenue = data.OrderByDescending(x => x.Revenue)
                                              .Select(x => new TopComboByRevenue
                                              {
                                                  ComboId = x.MaCombo,
                                                  ComboName = x.TenCombo ?? string.Empty,
                                                  Revenue = x.Revenue
                                              })
                                              .ToList()
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
                                .ToListAsync();
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
                                .Include(x => x.Chitietdanhmucs)
                                .ToListAsync();
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
                                .ToListAsync();
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
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu nhân viên: {ex.Message}");
                data = new List<Nhanvien>();
            }
            return data;
        }
        private async Task<List<Combo>> GetCombosAsync()
        {
            List<Combo> data = new();
            try
            {
                data = await _context.Combos
                                .Include(x => x.Chitietcombohoadons)
                                    .ThenInclude(x => x.MaHdNavigation)
                                .Include(x => x.Chitietcombohoadons)
                                    .ThenInclude(x => x.MaComboNavigation)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy dữ liệu combo: {ex.Message}");
                data = new List<Combo>();
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
                                .ToListAsync();
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