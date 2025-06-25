using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Khachhang
{
    public int MaKh { get; set; }

    public string HoTen { get; set; } = null!;

    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? DiaChi { get; set; }

    public string? Cccd { get; set; }

    public string? Sdt { get; set; }

    public string Email { get; set; } = null!;

    public string? TenTaiKhoan { get; set; }

    public string? MatKhau { get; set; }

    public string? HinhDaiDien { get; set; }

    public DateTime NgayTao { get; set; }

    public string? TinhTrang { get; set; }

    public bool? IsActive { get; set; }
    public int Streak { get; set; } = 0;
    public DateTime LastLogged { get; set; } = DateTime.Now;
    public virtual ICollection<Macoupon>? MaCoupons { get; set; }

    public virtual ICollection<Giohang> Giohangs { get; set; } = new List<Giohang>();

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
    public virtual ICollection<Diachi> Diachichitiets { get; set; } = new List<Diachi>();
    public virtual ICollection<Sanphamyeuthich> Sanphamyeuthichs { get; set; } = new List<Sanphamyeuthich>();
}
