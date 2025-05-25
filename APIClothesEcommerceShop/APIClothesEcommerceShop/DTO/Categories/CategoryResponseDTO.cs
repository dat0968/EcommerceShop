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
        public bool? IsActiveDanhMucCha { get; set; }
        public bool? IsActiveDanhMucCon { get; set; }

    }
}