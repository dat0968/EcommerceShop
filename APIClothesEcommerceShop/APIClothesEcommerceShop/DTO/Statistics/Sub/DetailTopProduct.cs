using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class DetailTopProduct
    {
        public DetailTopProduct(
            int maCtsp,
            int maSp,
            string? kichThuoc,
            string? mauSac,
            int soLuongTon,
            int donGia,
            string hinhAnh,
            bool? isActive)
        {
            MaCtsp = maCtsp;
            MaSp = maSp;
            KichThuoc = kichThuoc;
            MauSac = mauSac;
            SoLuongTon = soLuongTon;
            DonGia = donGia;
            HinhAnh = hinhAnh;
            IsActive = isActive;
        }
        public int MaCtsp { get; set; }

        public int MaSp { get; set; }

        public string? KichThuoc { get; set; }

        public string? MauSac { get; set; }

        public int SoLuongTon { get; set; }

        public int DonGia { get; set; }
        public string HinhAnh { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}