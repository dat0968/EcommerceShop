using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class DatatableStatisticsResponse
    {
        public DatatableStatisticsResponse(
            List<TopProductByRevenue>? topProductByRevenue = null,
            List<BestSellingProduct>? bestSellingProducts = null,
            List<ProductCategoryStatistics>? productCategoryStatistics = null,
            List<TopProductByOrder>? topProductByOrder = null,
            List<TopCustomerByOrder>? topCustomerByOrder = null,
            List<EmployeeByDepartment>? employeesByDepartment = null,
            List<EmployeeByPosition>? employeesByPosition = null,
            List<TopEmployeeByPerformance>? topEmployeesByPerformance = null,
            List<TopEmployeeBySales>? topEmployeesBySales = null,
            List<CustomerByLocation>? customersByLocation = null,
            List<CustomerByAgeGroup>? customersByAgeGroup = null,
            List<TopComboBySales>? topCombosBySales = null,
            List<TopComboByRevenue>? topCombosByRevenue = null
        )
        {
            TopProductByRevenue = topProductByRevenue ?? new();
            BestSellingProducts = bestSellingProducts ?? new();
            ProductCategoryStatistics = productCategoryStatistics ?? new();
            TopProductByOrder = topProductByOrder ?? new();
            TopCustomerByOrder = topCustomerByOrder ?? new();
            EmployeesByDepartment = employeesByDepartment ?? new();
            EmployeesByPosition = employeesByPosition ?? new();
            TopEmployeesByPerformance = topEmployeesByPerformance ?? new();
            TopEmployeesBySales = topEmployeesBySales ?? new();
            CustomersByLocation = customersByLocation ?? new();
            CustomersByAgeGroup = customersByAgeGroup ?? new();
            TopCombosBySales = topCombosBySales ?? new();
            TopCombosByRevenue = topCombosByRevenue ?? new();
        }
        public List<TopProductByRevenue> TopProductByRevenue { get; set; } = new(); // Danh sách sản phẩm có doanh thu cao nhất

        /// <summary>
        /// Danh sách sản phẩm bán chạy nhất
        /// </summary>
        public List<BestSellingProduct> BestSellingProducts { get; set; } = new();

        /// <summary>
        /// Thống kê sản phẩm theo danh mục
        /// </summary>
        public List<ProductCategoryStatistics> ProductCategoryStatistics { get; set; } = new();

        public List<TopProductByOrder> TopProductByOrder { get; set; } = new(); // Danh sách sản phẩm được đặt hàng nhiều nhất
        public List<TopCustomerByOrder> TopCustomerByOrder { get; set; } = new(); // Danh sách khách hàng đặt hàng nhiều nhất

        public List<EmployeeByDepartment> EmployeesByDepartment { get; set; } = new(); // Danh sách nhân viên theo phòng ban
        public List<EmployeeByPosition> EmployeesByPosition { get; set; } = new(); // Danh sách nhân viên theo chức vụ
        public List<TopEmployeeByPerformance> TopEmployeesByPerformance { get; set; } = new(); // Danh sách nhân viên có hiệu suất cao nhất
        public List<TopEmployeeBySales> TopEmployeesBySales { get; set; } = new(); // Danh sách nhân viên có doanh số cao nhất

        public List<CustomerByLocation> CustomersByLocation { get; set; } = new(); // Danh sách khách hàng theo địa điểm
        public List<CustomerByAgeGroup> CustomersByAgeGroup { get; set; } = new(); // Danh sách khách hàng theo nhóm tuổi
        public List<TopCustomerByOrder> TopCustomerByOrders { get; set; } = new(); // Danh sách khách hàng thống kê theo việc mua đơn hàng

        public List<TopComboBySales> TopCombosBySales { get; set; } = new(); // Danh sách combo có doanh số cao nhất
        public List<TopComboByRevenue> TopCombosByRevenue { get; set; } = new(); // Danh sách combo có doanh thu cao nhất
    }
}