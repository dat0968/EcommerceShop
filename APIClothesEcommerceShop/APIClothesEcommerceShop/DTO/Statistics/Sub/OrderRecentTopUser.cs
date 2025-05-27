using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{
    public class OrderRecentTopUser
    {
        public OrderRecentTopUser(
                int maHd,
                int maKh,
                string hoTen,
                int? maNv,
                string? maCode,
                DateTime ngayTao,
                string diaChiNhanHang,
                string hinhThucTt,
                string tinhTrang,
                string? moTa,
                string sdt,
                bool? isActive,
                decimal phiVanChuyen,
                decimal tienGoc)
        {
            MaHd = maHd;
            MaKh = maKh;
            HoTen = hoTen;
            MaNv = maNv;
            MaCode = maCode;
            NgayTao = ngayTao;
            DiaChiNhanHang = diaChiNhanHang;
            HinhThucTt = hinhThucTt;
            TinhTrang = tinhTrang;
            MoTa = moTa;
            Sdt = sdt;
            IsActive = isActive;
            PhiVanChuyen = phiVanChuyen;
            TienGoc = tienGoc;
        }
        public int MaHd { get; set; }
        public int MaKh { get; set; }
        public string HoTen { get; set; } = null!;
        public int? MaNv { get; set; }
        public string? MaCode { get; set; }
        public DateTime NgayTao { get; set; }
        public string DiaChiNhanHang { get; set; } = null!;
        public string HinhThucTt { get; set; } = null!;
        public string TinhTrang { get; set; } = null!;
        public string? MoTa { get; set; }
        public string Sdt { get; set; } = null!;
        public bool? IsActive { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TienGoc { get; set; }
    }
}