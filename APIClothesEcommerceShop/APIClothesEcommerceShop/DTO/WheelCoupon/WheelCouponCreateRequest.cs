using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.WheelCoupon
{
    public class WheelCouponCreateRequest
    {
        public decimal? DecreaseValue { get; set; } = null;
        public bool? IsPercent { get; set; } = true;
    }
}