using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class CustomerByAgeGroup
    {
        public string AgeGroup { get; set; } = string.Empty; // Nhóm tuổi
        public int Count { get; set; } // Số lượng khách hàng trong nhóm tuổi này
    }
}