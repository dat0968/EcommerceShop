using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Account;
using APIClothesEcommerceShop.Repositories.Customer;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace APIClothesEcommerceShop.Repositories.Contact
{
    public class ContactRepository : IContactRepository
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;
        private readonly EcommerceShopContext _db;

        public ContactRepository(
            ICustomerRepository customerRepository,
            IConfiguration configuration,
            EcommerceShopContext db)
        {
            _customerRepository = customerRepository;
            _configuration = configuration;
            _db = db;
        }

        public async Task<IActionResult> SendContact(ContactRequestDTO request, HttpContext httpContext)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) ||
                string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.Message))
            {
                return new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = "Tên, tiêu đề và nội dung là bắt buộc"
                });
            }
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return new UnauthorizedObjectResult(new
                {
                    Success = false,
                    Message = "Vui lòng đăng nhập để gửi yêu cầu liên hệ"
                });
            }

            try
            {
                var identity = httpContext.User.Identity as ClaimsIdentity;
                var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return new UnauthorizedObjectResult(new
                    {
                        Success = false,
                        Message = "Không thể xác định thông tin người dùng từ token"
                    });
                }

                var customer = await _customerRepository.GetCustomerByIdAsync(userId);
                if (customer == null || string.IsNullOrEmpty(customer.Email))
                {
                    return new BadRequestObjectResult(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin khách hàng hoặc email không hợp lệ"
                    });
                }

                string userEmail = customer.Email;
                int? maKH = customer.MaKH;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:SenderName"] ?? "DARK BEE FOOD",
                    _configuration["EmailSettings:SenderEmail"] ?? "khongbiet12kk@gmail.com"
                ));
                message.To.Add(new MailboxAddress("Admin", "pcbhoaanong@gmail.com"));
                message.Subject = request.Subject;

                message.ReplyTo.Add(new MailboxAddress(request.Name, userEmail));

                message.Body = new TextPart("html")
                {
                    Text = $@"
                    <h3>Thông tin liên hệ</h3>
                    <p><b>Tên:</b> {request.Name}</p>
                    <p><b>Email:</b> {userEmail}</p>
                    <p><b>Tiêu đề:</b> {request.Subject}</p>
                    <p><b>Nội dung:</b> {request.Message}</p>
                    {(maKH != null ? $"<p><b>Mã khách hàng:</b> {maKH}</p>" : "")}"
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(
                        _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
                        int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                        MailKit.Security.SecureSocketOptions.StartTls
                    );
                    await client.AuthenticateAsync(
                        _configuration["EmailSettings:Username"] ?? "khongbiet12kk@gmail.com",
                        _configuration["EmailSettings:Password"] ?? "vupb omuo wppx iccu"
                    );
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Gửi liên hệ thành công"
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}