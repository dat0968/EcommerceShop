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
        /// Giá trung bình của sản phẩm
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Danh sách sản phẩm bán chạy nhất
        /// </summary>
        public List<BestSellingProduct> BestSellingProducts { get; set; } = new();

        /// <summary>
        /// Thống kê sản phẩm theo danh mục
        /// </summary>
        public List<ProductCategoryStatistics> ProductCategoryStatistics { get; set; } = new();

        /// <summary>
        /// Thống kê sản phẩm theo thương hiệu
        /// </summary>
        public List<ProductBrandStatistics> ProductBrandStatistics { get; set; } = new();

        /// <summary>
        /// Doanh thu từ sản phẩm theo ngày
        /// </summary>
        public List<SalesByDate> SalesByDate { get; set; } = new();
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
    /// Thống kê sản phẩm theo thương hiệu
    /// </summary>
    public class ProductBrandStatistics
    {
        /// <summary>
        /// Tên thương hiệu
        /// </summary>
        public string BrandName { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số sản phẩm của thương hiệu này
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Tổng doanh thu từ thương hiệu này
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
    }
}