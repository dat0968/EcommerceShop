using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class OrderSummaryResponse
    {
        public int TotalCustomers { get; set; } // Tổng số khách hàng
        public int TotalProducts { get; set; } // Tổng số sản phẩm được bán
        public decimal TotalDiscount { get; set; } // Tổng số tiền giảm giá
        public decimal TotalShippingFee { get; set; } // Tổng số tiền phí vận chuyển
        public List<OrderStatusStatistics> OrderStatusStatistics { get; set; } = new(); // Thống kê trạng thái đơn hàng
        public Dictionary<string, List<RevenueByTime>> RevenueByTime { get; set; } = new(); // Doanh thu theo ngày
    }
}