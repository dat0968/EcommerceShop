using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Danhmuccon
{
    public int MaDanhMucCon { get; set; }

    public string TenDanhMucCon { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietdanhmuc> Chitietdanhmucs { get; set; } = new List<Chitietdanhmuc>();
}
