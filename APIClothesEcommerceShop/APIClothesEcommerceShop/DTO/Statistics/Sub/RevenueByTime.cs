using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class RevenueByTime
    {
        public DateTime Date { get; set; } // Ngày
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Revenue { get; set; } // Doanh thu trong ngày này
        public int Count { get; set; } // Số lượng đơn hàng trong ngày này
    }
}