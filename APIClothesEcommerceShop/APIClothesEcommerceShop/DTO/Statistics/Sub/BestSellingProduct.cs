using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class BestSellingProduct
    {
        public int ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public int Quantity { get; set; } // Số lượng sản phẩm được bán
        public decimal Revenue { get; set; } // Doanh thu từ sản phẩm này
    }
}