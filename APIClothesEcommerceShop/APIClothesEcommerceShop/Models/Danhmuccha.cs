using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Danhmuccha
{
    public int MaDanhMucCha { get; set; }

    public string TenDanhMucCha { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Chitietdanhmuc> Chitietdanhmucs { get; set; } = new List<Chitietdanhmuc>();
}
