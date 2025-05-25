using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Categories.CategoryParent
{
    public class CategoryParentRequestDTO
    {
        public string TenDanhMucCha { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}