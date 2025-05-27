using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.Models;

public partial class Chitietdanhmuc
{
    public int MaDanhMucCha { get; set; }

    public int MaDanhMucCon { get; set; }

    public int MaSp { get; set; }

    public virtual Danhmuccha MaDanhMucChaNavigation { get; set; } = null!;

    public virtual Danhmuccon MaDanhMucConNavigation { get; set; } = null!;

    public virtual Sanpham MaSpNavigation { get; set; } = null!;
}
