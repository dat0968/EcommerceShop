using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIClothesEcommerceShop.Models;

public partial class Cthoadon
{
    public int Id { get; set; }

    public int MaHd { get; set; }

    public int? MaCtsp { get; set; }

    public int? MaCombo { get; set; }

    public int SoLuong { get; set; }

    public int Gia { get; set; }
    public decimal? GiamGia { get; set; }

    public virtual Combo? MaComboNavigation { get; set; }

    public virtual Chitietsanpham? MaCtspNavigation { get; set; }

    public virtual Hoadon MaHdNavigation { get; set; } = null!;
    public virtual DanhGia? DanhGia { get; set; }
}
