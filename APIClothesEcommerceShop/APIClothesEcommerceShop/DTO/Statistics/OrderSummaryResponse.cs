using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class OrderSummaryResponse
    {
        public int TotalOrders { get; set; } // Tổng số đơn hàng
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal AverageOrderValue { get; set; } // Giá trị trung bình của mỗi đơn hàng
        public int TotalCustomers { get; set; } // Tổng số khách hàng
        public int TotalProducts { get; set; } // Tổng số sản phẩm được bán
        public decimal TotalDiscount { get; set; } // Tổng số tiền giảm giá
        public decimal TotalShippingFee { get; set; } // Tổng số tiền phí vận chuyển
        public List<OrderStatusStatistics> OrderStatusStatistics { get; set; } = new(); // Thống kê trạng thái đơn hàng
        public List<BestSellingProduct> BestSellingProducts { get; set; } = new(); // Danh sách sản phẩm bán chạy nhất
        public List<RevenueByDate> RevenueByDate { get; set; } = new(); // Doanh thu theo ngày
        public List<RevenueByMonth> RevenueByMonth { get; set; } = new(); // Doanh thu theo tháng
        public List<RevenueByYear> RevenueByYear { get; set; } = new(); // Doanh thu theo năm
    }
}