using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Giohang
{
    public int Id { get; set; }

    public int MaKh { get; set; }

    public int? MaCtsp { get; set; }

    public int? MaCombo { get; set; }

    public int SoLuong { get; set; }

    public int DonGia { get; set; }

    public virtual ICollection<Giohangctcombo> Giohangctcombos { get; set; } = new List<Giohangctcombo>();

    public virtual Combo? MaComboNavigation { get; set; }

    public virtual Chitietsanpham? MaCtspNavigation { get; set; }

    public virtual Khachhang MaKhNavigation { get; set; } = null!;
}
