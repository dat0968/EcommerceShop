using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Mails;
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
        [HttpPost]
        public async Task<IActionResult> ContactUs([FromBody] ContactForm form)
        {
            try
            {
                await _googleSender.SendEmailContactAsync(form.Name, form.Email, form.Phone, form.Message);
                return Ok(new { success = true, message = "Gửi liên hệ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}