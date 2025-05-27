using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class DetailTopCombo
    {
        public int ComboId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public int DonGia { get; set; }
        public string HinhAnh { get; set; } = string.Empty;
    }
}