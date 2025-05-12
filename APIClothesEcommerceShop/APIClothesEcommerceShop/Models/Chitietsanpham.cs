using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Chitietsanpham
{
    public int MaCtsp { get; set; }

    public int MaSp { get; set; }

    public string? KichThuoc { get; set; }

    public string? MauSac { get; set; }

    public int SoLuongTon { get; set; }

    public int DonGia { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietcombohoadon> Chitietcombohoadons { get; set; } = new List<Chitietcombohoadon>();

    public virtual ICollection<Cthoadon> Cthoadons { get; set; } = new List<Cthoadon>();

    public virtual ICollection<Giohangctcombo> Giohangctcombos { get; set; } = new List<Giohangctcombo>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual ICollection<Hinhanh> Hinhanhs { get; set; } = new List<Hinhanh>();

    public virtual Sanpham MaSpNavigation { get; set; } = null!;
}
