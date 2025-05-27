using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Hoadon
{
    public int MaHd { get; set; }

    public int MaKh { get; set; }

    public int? MaNv { get; set; }

    public string? MaCode { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? BatDauGiao { get; set; }

    public DateTime? NgayNhan { get; set; }

    public string DiaChiNhanHang { get; set; } = null!;

    public DateTime? NgayThanhToan { get; set; }

    public string HinhThucTt { get; set; } = null!;

    public string TinhTrang { get; set; } = null!;

    public string? MoTa { get; set; }

    public string HoTen { get; set; } = null!;

    public string Sdt { get; set; } = null!;

    public string? LyDoHuy { get; set; }

    public bool? IsActive { get; set; }

    public decimal PhiVanChuyen { get; set; }

    public decimal TienGoc { get; set; }

    public virtual ICollection<Chitietcombohoadon> Chitietcombohoadons { get; set; } = new List<Chitietcombohoadon>();

    public virtual ICollection<Cthoadon> Cthoadons { get; set; } = new List<Cthoadon>();

    public virtual Macoupon? MaCodeNavigation { get; set; }

    public virtual Khachhang MaKhNavigation { get; set; } = null!;

    public virtual Nhanvien? MaNvNavigation { get; set; }
}
