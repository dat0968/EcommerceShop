using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Combo
{
    public int MaCombo { get; set; }

    public string TenCombo { get; set; } = null!;

    public string? Hinh { get; set; }

    public int GiaCombo { get; set; }

    public int SoLuong { get; set; }

    public string? MoTa { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietcombohoadon> Chitietcombohoadons { get; set; } = new List<Chitietcombohoadon>();

    public virtual ICollection<Cthoadon> Cthoadons { get; set; } = new List<Cthoadon>();

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();
}
