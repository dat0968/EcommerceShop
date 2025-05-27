using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class ComboStatisticsResponse
    {
        public int TotalCombos { get; set; } // Tổng số combo
        public int TotalActiveCombos { get; set; } // Tổng số combo đang hoạt động
        public int TotalInactiveCombos { get; set; } // Tổng số combo không hoạt động
        public decimal AverageComboPrice { get; set; } // Giá trung bình của combo
        public decimal TotalComboRevenue { get; set; } // Tổng doanh thu từ combo
    }
}