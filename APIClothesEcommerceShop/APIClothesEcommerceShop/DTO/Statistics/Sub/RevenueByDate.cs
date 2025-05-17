using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class RevenueByDate
    {
        public DateTime Date { get; set; } // Ngày
        public decimal Revenue { get; set; } // Doanh thu trong ngày này
        public int Count { get; set; } // Số lượng đơn hàng trong ngày này
    }
}