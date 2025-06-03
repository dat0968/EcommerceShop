using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public int MaKh { get; set; } = 0;
        public int MaHd { get; set; }
        public int? MaCtsp { get; set; } = null;
        public int? MaCombo { get; set; } = null;

        public string NoiDung { get; set; } = string.Empty;
        public int SoSao { get; set; } = 0;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        public string? ShopPhanHoi { get; set; } = null;
    }
}