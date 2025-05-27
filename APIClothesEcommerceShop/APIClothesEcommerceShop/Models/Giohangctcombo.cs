using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Giohangctcombo
{
    public int Id { get; set; }

    public int MaGioHang { get; set; }

    public int MaCtsp { get; set; }

    public int SoLuong { get; set; }

    public int DonGia { get; set; }

    public virtual Chitietsanpham MaCtspNavigation { get; set; } = null!;

    public virtual Giohang MaGioHangNavigation { get; set; } = null!;
}
