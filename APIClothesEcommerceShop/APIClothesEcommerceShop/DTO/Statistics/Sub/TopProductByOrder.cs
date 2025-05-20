using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopProductByOrder
    {
        public int ProductId { get; set; } // Mã sản phẩm
        public string ProductName { get; set; } = string.Empty; // Tên sản phẩm
        public int Count { get; set; } // Số lượng đơn hàng có sản phẩm này
    }
}