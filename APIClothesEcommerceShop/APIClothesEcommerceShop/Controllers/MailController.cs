using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Services.EmailService.GoogleSenderService;
using Microsoft.AspNetCore.Mvc;

namespace APIClothesEcommerceShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MailController : ControllerBase
    {
        private readonly GoogleSenderService _googleSender;

        public MailController(GoogleSenderService googleSenderService)
        {
            _googleSender = googleSenderService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMailTest([FromForm] SendForm formInfo)
        {
            try
            {
                await _googleSender.SendTemplateEmailAsync(formInfo.MailTake, formInfo.File);
                return Ok("Email sent successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
    public class SendForm
    {
        public string MailTake { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}