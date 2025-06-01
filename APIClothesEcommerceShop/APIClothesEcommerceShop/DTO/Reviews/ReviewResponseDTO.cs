using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }

        public int IdKhachHang { get; set; } = 0;
        public int IdSanPham { get; set; } = 0;
        public string? HoTen { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public int SoSao { get; set; } = 0;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
    }
}