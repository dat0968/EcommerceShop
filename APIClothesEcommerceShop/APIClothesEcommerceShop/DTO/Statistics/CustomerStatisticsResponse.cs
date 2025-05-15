using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class CustomerStatisticsResponse
    {
        public int TotalCustomers { get; set; } // Tổng số khách hàng
        public int TotalActiveCustomers { get; set; } // Tổng số khách hàng đang hoạt động
        public int TotalInactiveCustomers { get; set; } // Tổng số khách hàng không hoạt động
        public List<CustomerByLocation> CustomersByLocation { get; set; } = new(); // Danh sách khách hàng theo địa điểm
        public List<CustomerByAgeGroup> CustomersByAgeGroup { get; set; } = new(); // Danh sách khách hàng theo nhóm tuổi
        public List<TopCustomerByPurchase> TopCustomersByPurchase { get; set; } = new(); // Danh sách khách hàng có số lần mua hàng cao nhất
        public List<TopCustomerByRevenue> TopCustomersByRevenue { get; set; } = new(); // Danh sách khách hàng có doanh thu cao nhất
        public decimal AveragePurchaseAmount { get; set; } // Số tiền mua hàng trung bình của khách hàng
        public decimal TotalPurchaseAmount { get; set; } // Tổng số tiền mua hàng của tất cả khách hàng
    }
}