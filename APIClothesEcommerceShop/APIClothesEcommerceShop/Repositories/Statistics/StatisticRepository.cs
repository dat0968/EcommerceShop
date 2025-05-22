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
                    TotalShippingFee = totalShippingFee,
                    TotalDiscount = totalDiscount,
                    TotalCustomers = totalCustomers,
                    TotalProducts = totalProducts,
                    OrderStatusStatistics = GetOrderStatusStatistics(dataMain),
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

        // Các phương thức phụ trợ để xử lý các tính toán
        private static List<OrderStatusStatistics> GetOrderStatusStatistics(IEnumerable<Hoadon> dataMain)
        {
            return dataMain.GroupBy(x => x.TinhTrang)
                .Select(g => new OrderStatusStatistics
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();
        }
        /// <summary>
        /// Lấy danh sách dữ liệu thống kê về các sản phẩm tiềm năng 
        /// </summary>
        /// <param name="dataOrder"></param>
        /// <param name="dataProduct"></param>
        /// <param name="dataCategory"></param>
        /// <returns></returns>
        private static List<TopProduct> GetTopProducts(IEnumerable<Hoadon> dataOrder, IEnumerable<Sanpham> dataProduct, IEnumerable<Danhmuccha> dataCategory)
        {
            // Tạo dictionary ánh xạ MaSp -> Tên danh mục cha
            var productCategoryDict = dataProduct
                .ToDictionary(
                    sp => sp.MaSp,
                    sp => sp.Chitietdanhmucs?.FirstOrDefault()?.MaDanhMucChaNavigation?.TenDanhMucCha ?? string.Empty
                );

            // Gom tất cả các chi tiết hóa đơn
            var allDetails = dataOrder
                .SelectMany(x => x.Cthoadons ?? Enumerable.Empty<Cthoadon>())
                .Where(x => x.MaCtspNavigation != null && x.MaCtspNavigation.MaSpNavigation != null)
                .ToList();

            // Gom nhóm theo MaSp (sản phẩm cha)
            var topProducts = allDetails
                .Where(x => x.MaCtspNavigation != null)
                .GroupBy(x => x.MaCtspNavigation!.MaSp) // ? Here has a !
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
                                ctsp!.MaCtsp, // ? Here has a !
                                ctsp.MaSp,
                                ctsp.KichThuoc,
                                ctsp.MauSac,
                                ctsp.SoLuongTon,
                                ctsp.DonGia,
                                ctsp.Hinhanhs?.FirstOrDefault()?.TenHinhAnh ?? string.Empty,
                                ctsp.IsActive
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
                        Revenue = g.Sum(x => ((x.Gia) * (x.SoLuong)) - (x.GiamGia)),
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
                .Where(x => x.MaKhNavigation != null)
                .GroupBy(x => x.MaKh)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.NgayTao).ToList());

            return ordersByCustomer.Select(kvp =>
            {
                var customerOrders = kvp.Value;
                var customer = customerOrders.First().MaKhNavigation;
                var ageGroup = "Không xác định";
                if (customer.NgaySinh.HasValue)
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
                    CustomerId = customer.MaKh,
                    CustomerName = customer.HoTen ?? "N/A",
                    Count = customerOrders.Count,
                    Revenue = customerOrders.Sum(x => x.TienGoc - x.PhiVanChuyen),
                    Location = customer.DiaChi ?? "N/A",
                    AgeGroup = ageGroup,
                    OrderRecents = customerOrders
                        .Take(3)
                        .Select(o => new OrderRecentTopUser(
                            o.MaHd,
                            customer.MaKh,
                            customer.HoTen ?? "N/A",
                            o.MaNv,
                            o.MaCode,
                            o.NgayTao,
                            o.DiaChiNhanHang,
                            o.HinhThucTt,
                            o.TinhTrang,
                            o.MoTa,
                            o.Sdt,
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
                                o.MaKh,
                                o.MaKhNavigation?.HoTen ?? "N/A",
                                o.MaNv,
                                o.MaCode,
                                o.NgayTao,
                                o.DiaChiNhanHang,
                                o.HinhThucTt,
                                o.TinhTrang,
                                o.MoTa,
                                o.Sdt,
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
            responseGet["date"] = dataMain.GroupBy(x => x.NgayTao.Date)
                .Select(g => new RevenueByTime
                {
                    Date = g.Key,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.TienGoc),
                    Count = g.Count()
                }).ToList();
            responseGet["month"] = dataMain.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
            .Select(g => new RevenueByTime
            {
                Month = g.Key.Month,
                Year = g.Key.Year,
                Revenue = g.Sum(x => x.TienGoc),
                Count = g.Count()
            }).ToList();
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
        /// Lấy thống kê sản phẩm
        /// </summary>
        /// <returns>Thống kê sản phẩm</returns>
        public async Task<ResponseAPI<ProductStatisticsResponse>> GetProductStatisticsAsync()
        {
            ResponseAPI<ProductStatisticsResponse> response = new();
            try
            {
                var dataMain = await GetSanphamsAsync();
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
                    SalesByTimes = GetSalesByTimes(dataOrder)
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
            keyValuePairs["date"] = dataOrder.GroupBy(x => x.NgayTao.Date)
                .Select(g => new SalesByTime
                {
                    Date = g.Key,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
            keyValuePairs["month"] = dataOrder.GroupBy(x => new { x.NgayTao.Month, x.NgayTao.Year })
                .Select(g => new SalesByTime
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
            keyValuePairs["year"] = dataOrder.GroupBy(x => x.NgayTao.Year)
                .Select(g => new SalesByTime
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.Cthoadons.Sum(y => (y?.Gia ?? 0) * (y?.SoLuong ?? 0) - (y?.GiamGia ?? 0))),
                    Count = g.Count()
                }).ToList();
            return keyValuePairs;
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

                var averagePurchaseAmount = totalCustomers > 0 ? totalRevenue / totalCustomers : 0;

                // Gán kết quả vào response
                response.Data = new CustomerStatisticsResponse
                {
                    TotalCustomers = totalCustomers,
                    TotalActiveCustomers = totalActiveCustomers,
                    TotalInactiveCustomers = totalInactiveCustomers,
                    TotalPurchaseAmount = totalRevenue,
                    AveragePurchaseAmount = averagePurchaseAmount,
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
                var averageSalary = dataMain.Average(x => x.MaChucVuNavigation.Luong);
                var totalSalary = dataMain.Sum(x => x.MaChucVuNavigation.Luong);

                response.Data.TotalEmployees = totalEmployees;
                response.Data.TotalActiveEmployees = totalActiveEmployees;
                response.Data.TotalInactiveEmployees = totalInactiveEmployees;
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
                    .Include(h => h.MaCodeNavigation)
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
                    AverageComboPrice = data.Count > 0 ? data.Sum(x => x.GiaCombo) / data.Count : 0
                };

                response.SetSuccessResponse();
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.HandleException(ex, response);
            }
            return response;
        }

        #region [Mix datatable]
        public async Task<ResponseAPI<DatatableStatisticsResponse>> GetDatatableStatisticsAsync()
        {
            var response = new ResponseAPI<DatatableStatisticsResponse>();
            try
            {
                // Lấy dữ liệu cần thiết song song để tối ưu hiệu năng
                var getSanphamsTask = await GetSanphamsAsync();
                var getNhanviensTask = await GetNhanviensAsync();
                var getDanhmucchasTask = await _context.Danhmucchas.Include(x => x.Chitietdanhmucs).ToListAsync();
                var getCombosTask = await _context.Combos
                    .Include(c => c.Cthoadons)
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
                var getHoadonsTask = await _context.Hoadons
                    .Include(h => h.Cthoadons)
                        .ThenInclude(h => h.MaCtspNavigation)
                            .ThenInclude(h => h.MaSpNavigation)
                    .Include(h => h.MaKhNavigation)
                    .Include(h => h.MaCodeNavigation)
                    .ToListAsync();
                var getKhachhangsTask = await _context.Khachhangs
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
                    .ToListAsync();

                var dataProduct = getSanphamsTask;
                var dataEmployee = getNhanviensTask;
                var dataCategory = getDanhmucchasTask;
                var dataCombo = getCombosTask;
                var dataOrder = getHoadonsTask;
                var dataCustomer = getKhachhangsTask;

                // Thống kê sản phẩm
                /* var topProductByRevenue = dataOrder.SelectMany(h => h.Cthoadons)
                    .GroupBy(ct => new { ct.MaCtsp, ct.MaCtspNavigation?.MaSpNavigation.TenSanPham })
                    .Select(g => new TopProduct
                    {
                        ProductId = g.Key.MaCtsp ?? 0,
                        ProductName = g.Key.TenSanPham ?? string.Empty,
                        Revenue = g.Sum(ct => (ct.Gia * ct.SoLuong) - ct.GiamGia)
                    }).OrderByDescending(x => x.Revenue).ToList(); */

                var topProducts = GetTopProducts(dataOrder, dataProduct, dataCategory);
                var topCustomers = GetTopCustomers(dataOrder);
                var topEmployees = GetTopEmployees(dataOrder, dataEmployee);
                // ! Thống kê combo [bad]
                var topComboss = dataCombo.OrderByDescending(x => x.SalesCount)
                    .Select(x => new TopCombo
                    {
                        ComboId = x.MaCombo,
                        ComboName = x.TenCombo ?? string.Empty,
                        SalesCount = x.SalesCount
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