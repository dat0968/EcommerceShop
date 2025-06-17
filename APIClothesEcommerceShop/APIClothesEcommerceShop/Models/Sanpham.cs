using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Sanpham
{
    public int MaSp { get; set; }

    public string TenSanPham { get; set; } = null!;
    public DateTime NgayTao { get; set; }
    public int LuotXem { get; set; }
    public string? MoTa { get; set; }
    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietdanhmuc> Chitietdanhmucs { get; set; } = new List<Chitietdanhmuc>();

    public virtual ICollection<Chitietsanpham> Chitietsanphams { get; set; } = new List<Chitietsanpham>();
    public virtual ICollection<Chitietcombo> Chitietcombos { get; set; } = new List<Chitietcombo>();
    public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    public virtual ICollection<Sanphamyeuthich> Sanphamyeuthichs { get; set; } = new List<Sanphamyeuthich>();
}
