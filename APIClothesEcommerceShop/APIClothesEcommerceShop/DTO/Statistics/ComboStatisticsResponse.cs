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
        public List<TopComboBySales> TopCombosBySales { get; set; } = new(); // Danh sách combo có doanh số cao nhất
        public List<TopComboByRevenue> TopCombosByRevenue { get; set; } = new(); // Danh sách combo có doanh thu cao nhất
        public decimal AverageComboPrice { get; set; } // Giá trung bình của combo
        public decimal TotalComboRevenue { get; set; } // Tổng doanh thu từ combo
    }

    public class TopComboBySales
    {
        public int ComboId { get; set; } // Mã combo
        public string ComboName { get; set; } = string.Empty; // Tên combo
        public int SalesCount { get; set; } // Số lượng combo được bán
    }

    public class TopComboByRevenue
    {
        public int ComboId { get; set; } // Mã combo
        public string ComboName { get; set; } = string.Empty; // Tên combo
        public decimal Revenue { get; set; } // Doanh thu từ combo
    }
}