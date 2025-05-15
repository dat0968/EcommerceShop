using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class CustomerByLocation
    {
        public string Location { get; set; } = string.Empty; // Địa điểm
        public int Count { get; set; } // Số lượng khách hàng tại địa điểm này
    }
}