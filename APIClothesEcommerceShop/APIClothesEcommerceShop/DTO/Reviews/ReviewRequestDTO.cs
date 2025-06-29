using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class ReviewRequestDTO
    {
        public int? Id { get; set; }
        public int MaKh { get; set; } = 0;
        public int? MaSp { get; set; } // Sử dụng MaSp thay cho MaCtsp
        public int? MaCombo { get; set; }
        public int MaCtHd { get; set; }

        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;

        [Range(1, 5)]
        public int SoSao { get; set; } = 0;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        public IFormFile[]? HinhAnhs { get; set; }
    }
}