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
    }
}