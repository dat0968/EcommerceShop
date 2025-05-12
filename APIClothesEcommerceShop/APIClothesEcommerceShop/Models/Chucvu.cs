using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Chucvu
{
    public int MaChucVu { get; set; }

    public string TenChucVu { get; set; } = null!;

    public decimal Luong { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Nhanvien> Nhanviens { get; set; } = new List<Nhanvien>();
}
