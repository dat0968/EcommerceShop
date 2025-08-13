
using System;
using System.Collections.Generic;

namespace APIClothesEcommerceShop.DTO.Comment
{
    public class CommentResponseDTO
    {
        public int Id { get; set; }
        public int? MaSP { get; set; }
        public int? MaCombo { get; set; }
        public int MaKh { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayBinhLuan { get; set; }
        public int? ParentId { get; set; }
        public List<CommentResponseDTO> Replies { get; set; } = new List<CommentResponseDTO>();
    }
}
