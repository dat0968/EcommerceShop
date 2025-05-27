using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopCombo
    {
        public int ComboId { get; set; } // Mã combo
        public string ComboName { get; set; } = string.Empty; // Tên combo
        public int SalesCount { get; set; } // Số lượng combo được bán
        public decimal Revenue { get; set; } // Doanh thu từ combo
        public List<DetailTopCombo> DetailTopCombos { get; set; } = new();
    }
}