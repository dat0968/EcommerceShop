using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Mails
{
    public class ContactForm
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}