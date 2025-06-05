using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Reviews
{
    public class RequestReplyRequestDTO
    {
        public int[] ListId { get; set; } = new int[0];
        public string ResponseContent { get; set; } = string.Empty;
    }
}