using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Comment
{
    public class CommentResponseDTO
    {
        public int? Id { get; set; }

        public int IdSanPham { get; set; }
        public int IdKhachHang { get; set; } = 0;

        public string? HoTen { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayBinhLuan { get; set; } = DateTime.Now;

        public int ParentId { get; set; } = 0;
    }
}