using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Sanpham
{
    public int MaSp { get; set; }

    public string TenSanPham { get; set; } = null!;

    public string? MoTa { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietdanhmuc> Chitietdanhmucs { get; set; } = new List<Chitietdanhmuc>();

    public virtual ICollection<Chitietsanpham> Chitietsanphams { get; set; } = new List<Chitietsanpham>();
}
