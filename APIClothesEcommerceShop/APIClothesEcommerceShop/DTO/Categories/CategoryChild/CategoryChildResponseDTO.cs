using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Categories.CategoryChild
{
    public class CategoryChildResponseDTO
    {
        public int MaDanhMucCon { get; set; }
        public string TenDanhMucCon { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}