using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Categories
{
    public class CategoryResponseDTO
    {
        public int MaDanhMucCha { get; set; }
        public string TenDanhMucCha { get; set; } = null!;
        public int MaDanhMucCon { get; set; }
        public string TenDanhMucCon { get; set; } = null!;
        public int MaSp { get; set; }
        public string TenSanPham { get; set; } = null!;
        public string? MoTa { get; set; }
        public bool? IsActiveDanhMucCha { get; set; }
        public bool? IsActiveDanhMucCon { get; set; }
        public List<DtProduct> DetailProducts { get; set; } = new List<DtProduct>();
    }
    public class DtProduct
    {
        public int MaCtsp { get; set; }

        public string? KichThuoc { get; set; }

        public string? MauSac { get; set; }

        public int SoLuongTon { get; set; }

        public int DonGia { get; set; }

        public bool? IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}