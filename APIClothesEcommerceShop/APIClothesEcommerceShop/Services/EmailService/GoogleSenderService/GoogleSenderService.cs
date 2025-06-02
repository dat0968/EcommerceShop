using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MimeKit;
using Markdig;
using APIClothesEcommerceShop.Services.EmailService;

namespace APIClothesEcommerceShop.Services.EmailService.GoogleSenderService
{
    public class GoogleSenderService
    {
        private readonly GoogleEmailSetting? emailSettings;
        public GoogleSenderService(IOptions<GoogleEmailSetting> options)
        {
            emailSettings = options.Value;
        }
        /// <summary>
        /// Gửi email được theo mẫu HTML
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailSettings.Email);
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            // Đọc nội dung tệp HTML
            var templatePath = DefaultPathAndGetNameTemplate("basic-template.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);

            // Thay thế các placeholder với dữ liệu thật
            var htmlBody = htmlTemplate
                .Replace("{{title}}", subject)
                .Replace("{{time}}", DateTime.UtcNow.ToString("dd/MM/yyyy"))
                .Replace("{{name}}", emailSettings.Displayname)
                .Replace("{{email}}", toEmail)
                .Replace("{{message}}", message);

            email.Body = new TextPart("html") { Text = htmlBody };
            var smtp = new SmtpClient();
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Email, emailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendTemplateEmailAsync(string toEmail)
        {
            // Tạo đối tượng MimeMessage
            MimeMessage message = new MimeMessage();

            // Thiết lập thông tin người gửi
            message.From.Add(new MailboxAddress("Người gửi", emailSettings.Email)); // Sử dụng email của người gửi từ emailSettings

            // Thiết lập thông tin người nhận
            message.To.Add(new MailboxAddress("Người nhận", toEmail));

            // Thiết lập tiêu đề email
            message.Subject = "Tiêu đề email";

            // Thiết lập nội dung email
            BodyBuilder bodyBuilder = new BodyBuilder();

            // Đọc nội dung HTML từ tệp
            var templatePath = DefaultPathAndGetNameTemplate("dark-bee-food-template.html");
            bodyBuilder.HtmlBody = await File.ReadAllTextAsync(templatePath); // Thiết lập nội dung HTML Markdown.ToHtml

            message.Body = bodyBuilder.ToMessageBody(); // Tạo nội dung email từ bodyBuilder

            // Thiết lập thông tin SMTP server và gửi email
            using (SmtpClient client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(emailSettings.Email, emailSettings.Password);
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi ở đây (log, throw hoặc xử lý tùy thuộc vào yêu cầu của bạn)
                    Console.WriteLine("Đã xảy ra lỗi khi gửi email: " + ex.Message);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
        public async Task SendEmailMimeWithAIContentAsync(string mailTake, string htmlContent)
        {
            // Tạo đối tượng MimeMessage
            MimeMessage message = new MimeMessage();

            // Thiết lập thông tin người gửi
            message.From.Add(new MailboxAddress("Người gửi", emailSettings.Email)); // Sử dụng email của người gửi từ emailSettings

            // Thiết lập thông tin người nhận
            message.To.Add(new MailboxAddress("Người nhận", mailTake));

            // Thiết lập tiêu đề email
            message.Subject = "Tiêu đề email";

            // Thiết lập nội dung email
            BodyBuilder bodyBuilder = new BodyBuilder();

            // Đính kèm file (nếu cần)
            var templatePath = DefaultPathAndGetNameTemplate("basic-template.html");
            bodyBuilder.Attachments.Add(templatePath);

            // Đọc nội dung HTML từ tệp
            bodyBuilder.HtmlBody = htmlContent; // Thiết lập nội dung HTML Markdown.ToHtml

            message.Body = bodyBuilder.ToMessageBody(); // Tạo nội dung email từ bodyBuilder

            // Thiết lập thông tin SMTP server và gửi email
            using (SmtpClient client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(emailSettings.Email, emailSettings.Password);
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi ở đây (log, throw hoặc xử lý tùy thuộc vào yêu cầu của bạn)
                    Console.WriteLine("Đã xảy ra lỗi khi gửi email: " + ex.Message);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        public async Task SendEmailContactAsync(string name, string emailContact, string phoneNumber, string message)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailSettings.Email);
            email.To.Add(MailboxAddress.Parse("smtpmvc555@gmail.com")); // Địa chỉ email người nhận
            email.Subject = "Liên hệ từ khách hàng";

            // Đọc nội dung tệp HTML
            var templatePath = DefaultPathAndGetNameTemplate("basic-template.html");
            var htmlTemplate = await File.ReadAllTextAsync(templatePath);

            // Thay thế các placeholder với dữ liệu thật
            var htmlBody = htmlTemplate
                .Replace("{{name}}", name)
                .Replace("{{email}}", emailContact)
                .Replace("{{phone}}", phoneNumber)
                .Replace("{{message}}", message);

            email.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Email, emailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);



            var emailResend = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailSettings.Email);
            email.To.Add(MailboxAddress.Parse(emailContact)); // Địa chỉ email người nhận
            email.Subject = "Liên hệ từ khách hàng";

            // Đọc nội dung tệp HTML
            var templatePathResend = DefaultPathAndGetNameTemplate("basic-template.html");
            var htmlTemplateResend = await File.ReadAllTextAsync(templatePath);

            // Thay thế các placeholder với dữ liệu thật
            var htmlBodyResend = htmlTemplate
                .Replace("{{name}}", name)
                .Replace("{{email}}", emailContact)
                .Replace("{{phone}}", phoneNumber)
                .Replace("{{message}}", message);

            email.Body = new TextPart("html") { Text = htmlBody };

            using var smtpResend = new SmtpClient();
            smtp.Connect(emailSettings.Host, emailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(emailSettings.Email, emailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        private static string DefaultPathAndGetNameTemplate(string nameTemplate)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "EmailTemplates", nameTemplate);
        }
    }
}