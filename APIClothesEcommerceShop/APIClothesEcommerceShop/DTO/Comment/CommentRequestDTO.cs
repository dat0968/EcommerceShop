
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Comment
{
    public class CommentRequestDTO
    {
        public int MaKh { get; set; }
        public int? MaSP { get; set; }
        public int? MaCombo { get; set; }
        [MaxLength(500)]
        public string NoiDung { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
