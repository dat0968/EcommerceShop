using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Cthoadon
{
    public int Id { get; set; }

    public int MaHd { get; set; }

    public int? MaCtsp { get; set; }

    public int? MaCombo { get; set; }

    public int SoLuong { get; set; }

    public int Gia { get; set; }

    public int GiamGia { get; set; }

    public virtual Combo? MaComboNavigation { get; set; }

    public virtual Chitietsanpham? MaCtspNavigation { get; set; }

    public virtual Hoadon MaHdNavigation { get; set; } = null!;
}
