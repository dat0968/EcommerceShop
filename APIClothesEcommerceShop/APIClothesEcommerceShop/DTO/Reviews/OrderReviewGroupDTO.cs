using System;
using System.Collections.Generic;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class OrderReviewGroupDTO
    {
        public int MaHd { get; set; }
        public DateTime NgayTao { get; set; }
        public string TinhTrang { get; set; } = string.Empty;
        public List<ReviewResponseDTO> Items { get; set; } = new List<ReviewResponseDTO>();
    }
}