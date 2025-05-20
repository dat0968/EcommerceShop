using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    /// <summary>
    /// Phản hồi thống kê sản phẩm
    /// </summary>
    public class ProductStatisticsResponse
    {
        /// <summary>
        /// Tổng số sản phẩm
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Tổng số sản phẩm đang hoạt động
        /// </summary>
        public int TotalActiveProducts { get; set; }

        /// <summary>
        /// Tổng số sản phẩm không hoạt động
        /// </summary>
        public int TotalInactiveProducts { get; set; }

        /// <summary>
        /// Tổng doanh thu từ sản phẩm
        /// </summary>
        public decimal TotalRevenue { get; set; }
        /// <summary>
        /// Tổng giảm giá từ sản phẩm
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Giá trung bình của sản phẩm
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Doanh thu từ sản phẩm theo ngày
        /// </summary>
        public List<SalesByDate> SalesByDate { get; set; } = new();
        public List<SalesByMonth> SalesByMonth { get; set; } = new();
        public List<SalesByYear> SalesByYear { get; set; } = new();
    }

    /// <summary>
    /// Thống kê sản phẩm theo danh mục
    /// </summary>
    public class ProductCategoryStatistics
    {
        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số sản phẩm trong danh mục này
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Tổng doanh thu từ danh mục này
        /// </summary>
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Doanh thu từ sản phẩm theo ngày
    /// </summary>
    public class SalesByDate
    {
        /// <summary>
        /// Ngày
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Doanh thu từ sản phẩm trong ngày này
        /// </summary>
        public decimal Revenue { get; set; }
        public int Count { get; set; } // Số lượng đơn hàng trong ngày này
    }
    public class SalesByMonth
    {
        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Doanh thu từ sản phẩm trong tháng này
        /// </summary>
        public decimal Revenue { get; set; }
        public int Count { get; set; } // Số lượng đơn hàng trong tháng này
    }
    public class SalesByYear
    {
        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Doanh thu từ sản phẩm trong năm này
        /// </summary>
        public decimal Revenue { get; set; }
        public int Count { get; set; } // Số lượng đơn hàng trong năm này
    }
}