using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Category.CategoryChild
{
    public class CategoryChildRequestDTO
    {
        public string TenDanhMucCon { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
}