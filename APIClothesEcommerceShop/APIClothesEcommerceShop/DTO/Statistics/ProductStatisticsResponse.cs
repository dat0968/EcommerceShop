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
        /// Thống kê giá với thời gian
        /// </summary>
        public Dictionary<string, List<SalesByTime>> SalesByTimes { get; set; } = [];
    }


    /// <summary>
    /// Doanh thu từ sản phẩm theo ngày
    /// </summary>
    public class SalesByTime
    {
        /// <summary>
        /// Ngày
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Tháng
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Năm
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Doanh thu từ sản phẩm trong ngày này
        /// </summary>
        public decimal Revenue { get; set; }
        public int Count { get; set; } // Số lượng đơn hàng trong ngày này
    }
}