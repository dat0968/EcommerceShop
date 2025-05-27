using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopCustomer
    {
        public int CustomerId { get; set; } // Mã khách hàng
        public string CustomerName { get; set; } = string.Empty;
        public decimal Revenue { get; set; } // Doanh thu từ khách hàng này
        public string Location { get; set; } = string.Empty; // Địa điểm
        public string AgeGroup { get; set; } = string.Empty; // Nhóm tuổi
        public int Count { get; set; } // Số lượng khách hàng tại địa điểm này
        public List<OrderRecentTopUser> OrderRecents { get; set; } = new();
    }
}