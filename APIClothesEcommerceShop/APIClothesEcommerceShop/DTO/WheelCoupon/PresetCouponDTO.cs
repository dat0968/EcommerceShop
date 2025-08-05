using System.ComponentModel.DataAnnotations;

namespace APIClothesEcommerceShop.DTO.WheelCoupon
{
    public class CouponValue
    {
        public int Value { get; set; }
        public bool IsPercent { get; set; }
    }

    public class CouponPresetDTO
    {
        public string PresetToken { get; set; }
        public List<string> DisplayValues { get; set; }
    }

    public class ClaimPresetCouponRequest
    {
        [Required]
        public string PresetToken { get; set; }

        [Required]
        [Range(0, 9)] // Assuming 10 slots, indexed 0-9
        public int WonIndex { get; set; }
    }
}
