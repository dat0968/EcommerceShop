using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopCustomerByPurchase
    {
        public int CustomerId { get; set; } // Mã khách hàng
        public string CustomerName { get; set; } = string.Empty; // Tên khách hàng
        public int PurchaseCount { get; set; } // Số lần mua hàng của khách hàng
    }
}