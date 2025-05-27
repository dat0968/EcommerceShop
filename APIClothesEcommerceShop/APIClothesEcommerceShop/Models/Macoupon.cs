using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Macoupon
{
    public string MaCode { get; set; } = null!;

    public string MoTa { get; set; } = null!;

    public decimal PhanTramGiam { get; set; }

    public int SoTienGiam { get; set; }

    public int DonHangToiThieu { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public int SoLuong { get; set; }

    public int SoLuongDaDung { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
