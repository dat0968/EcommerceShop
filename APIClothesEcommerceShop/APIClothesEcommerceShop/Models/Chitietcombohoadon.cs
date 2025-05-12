using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Chitietcombohoadon
{
    public int MaHd { get; set; }

    public int MaCtsp { get; set; }

    public int MaCombo { get; set; }

    public int SoLuong { get; set; }

    public int DonGia { get; set; }

    public virtual Combo MaComboNavigation { get; set; } = null!;

    public virtual Chitietsanpham MaCtspNavigation { get; set; } = null!;

    public virtual Hoadon MaHdNavigation { get; set; } = null!;
}
