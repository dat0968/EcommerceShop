using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class DetailTopCombo
    {
        public DetailTopCombo(Chitietcombo chitietcombo)
        {
            ComboId = chitietcombo.MaCombo;
            TenSanPham = chitietcombo.MaSpNavigation.TenSanPham;
            SoLuong = chitietcombo.SoLuongSP;
            DonGia = chitietcombo.MaSpNavigation.Chitietsanphams.Average(x => x.DonGia);
        }
        public int ComboId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double DonGia { get; set; }
        public string HinhAnh { get; set; } = string.Empty;
    }
}