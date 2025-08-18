using APIClothesEcommerceShop.DTO.Account;
using APIClothesEcommerceShop.Repositories.Contact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;

        public ContactController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpPost]
        public async Task<IActionResult> SendContact([FromBody] ContactRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = ModelState
                });
            }

            return await _contactRepository.SendContact(request, HttpContext);
        }
    }
}

public class ContactRequestDTO
{
    [Required(ErrorMessage = "Tên là bắt buộc")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    public string Subject { get; set; }

    [Required(ErrorMessage = "Nội dung là bắt buộc")]
    public string Message { get; set; }
    public string? Email { get; internal set; }
    public CancellationToken Password { get; internal set; }
}

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}