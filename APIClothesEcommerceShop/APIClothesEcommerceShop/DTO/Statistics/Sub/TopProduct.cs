using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class TopProduct
    {
        public int ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; } // Doanh thu từ sản phẩm này
        public int Count { get; set; }
    }
}