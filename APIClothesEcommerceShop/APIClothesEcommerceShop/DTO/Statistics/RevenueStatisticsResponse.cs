using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class RevenueStatisticsResponse
    {
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal AverageDailyRevenue { get; set; } // Doanh thu trung bình hàng ngày
        public decimal AverageMonthlyRevenue { get; set; } // Doanh thu trung bình hàng tháng
        public decimal HighestRevenue { get; set; } // Doanh thu cao nhất
        public decimal LowestRevenue { get; set; } // Doanh thu thấp nhất
        public List<RevenueByDate> RevenueByDate { get; set; } = new(); // Doanh thu theo ngày
        public List<RevenueByMonth> RevenueByMonth { get; set; } = new(); // Doanh thu theo tháng
        public List<RevenueByYear> RevenueByYear { get; set; } = new(); // Doanh thu theo năm
        public List<TopProductByRevenue> TopProductByRevenue { get; set; } = new(); // Danh sách sản phẩm có doanh thu cao nhất
        public List<TopCustomerByRevenue> TopCustomerByRevenue { get; set; } = new(); // Danh sách khách hàng có doanh thu cao nhất
    }

    public class RevenueByMonth
    {
        public int Month { get; set; } // Tháng
        public int Year { get; set; } // Năm
        public decimal Revenue { get; set; } // Doanh thu trong tháng này
    }

    public class RevenueByYear
    {
        public int Year { get; set; } // Năm
        public decimal Revenue { get; set; } // Doanh thu trong năm này
    }

    public class TopProductByRevenue
    {
        public int ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public decimal Revenue { get; set; } // Doanh thu từ sản phẩm này
    }
}