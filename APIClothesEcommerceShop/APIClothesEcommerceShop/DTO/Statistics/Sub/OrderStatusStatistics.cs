using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class OrderStatusStatistics
    {
        public string Status { get; set; } = string.Empty; // Trạng thái đơn hàng (ví dụ: "Đang xử lý", "Đã giao hàng",...)
        public int Count { get; set; } // Số lượng đơn hàng có trạng thái này
    }
}