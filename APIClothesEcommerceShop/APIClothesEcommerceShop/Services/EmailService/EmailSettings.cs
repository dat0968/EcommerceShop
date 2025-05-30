using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Services.EmailService
{
    public class GoogleEmailSetting
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Displayname { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}