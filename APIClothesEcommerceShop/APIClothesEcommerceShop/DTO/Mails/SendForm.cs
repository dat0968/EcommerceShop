using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Mails
{
    public class SendForm
    {
        public string MailTake { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}