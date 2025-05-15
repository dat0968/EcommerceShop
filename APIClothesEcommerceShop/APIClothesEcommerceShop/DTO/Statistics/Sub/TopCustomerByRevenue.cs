using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopCustomerByRevenue
    {
        public int CustomerId { get; set; } // Mã khách hàng
        public string CustomerName { get; set; } = string.Empty; // Tên khách hàng
        public decimal Revenue { get; set; } // Doanh thu từ khách hàng này
    }
}