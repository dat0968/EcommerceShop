using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class OrderStatisticsResponse
    {
        public int TotalOrders { get; set; } // Tổng số đơn hàng
        public int TotalCompletedOrders { get; set; } // Tổng số đơn hàng đã hoàn thành
        public int TotalPendingOrders { get; set; } // Tổng số đơn hàng đang chờ xử lý
        public int TotalCancelledOrders { get; set; } // Tổng số đơn hàng đã hủy
        public decimal TotalOrderValue { get; set; } // Tổng giá trị đơn hàng
        public decimal AverageOrderValue { get; set; } // Giá trị trung bình của mỗi đơn hàng
        public List<OrderStatusStatistics> OrderStatusStatistics { get; set; } = new(); // Thống kê trạng thái đơn hàng
        public List<OrderByDate> OrdersByDate { get; set; } = new(); // Đơn hàng theo ngày
        public List<OrderByMonth> OrdersByMonth { get; set; } = new(); // Đơn hàng theo tháng
        public List<OrderByYear> OrdersByYear { get; set; } = new(); // Đơn hàng theo năm
        public List<TopProductByOrder> TopProductByOrder { get; set; } = new(); // Danh sách sản phẩm được đặt hàng nhiều nhất
        public List<TopCustomerByOrder> TopCustomerByOrder { get; set; } = new(); // Danh sách khách hàng đặt hàng nhiều nhất
    }

    public class OrderByDate
    {
        public DateTime Date { get; set; } // Ngày
        public int Count { get; set; } // Số lượng đơn hàng trong ngày này
    }

    public class OrderByMonth
    {
        public int Month { get; set; } // Tháng
        public int Year { get; set; } // Năm
        public int Count { get; set; } // Số lượng đơn hàng trong tháng này
    }

    public class OrderByYear
    {
        public int Year { get; set; } // Năm
        public int Count { get; set; } // Số lượng đơn hàng trong năm này
    }

    public class TopProductByOrder
    {
        public int ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public int Count { get; set; } // Số lượng đơn hàng có sản phẩm này
    }

    public class TopCustomerByOrder
    {
        public int CustomerId { get; set; } // Mã khách hàng
        public string CustomerName { get; set; } = string.Empty; // Tên khách hàng
        public int Count { get; set; } // Số lượng đơn hàng của khách hàng này
    }
}