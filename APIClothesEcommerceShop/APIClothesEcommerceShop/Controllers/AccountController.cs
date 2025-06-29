using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Account;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Account;
using APIClothesEcommerceShop.Repositories.HashPassword;
using APIClothesEcommerceShop.Repositories.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        [HttpPost("VerifyRecaptcha")]
        public async Task<IActionResult> VerifyRecaptcha([FromBody] RecaptchaVerificationDTO model)
        {
            return await _accountRepository.VerifyRecaptcha(model);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            return await _accountRepository.Register(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// {
        /// "email_TenTaiKhoan": "customer.demo",
        /// "matKhau": "CustomerDemo@123"
        /// }
        /// </remarks>
        [HttpPost("LoginCustomer")]
        public async Task<IActionResult> LoginCustomer(LoginDTO model)
        {
            return await _accountRepository.LoginCustomer(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// {
        ///   "email_TenTaiKhoan": "staff.demo",
        ///   "matKhau": "StaffDemo@123"
        /// }
        /// </remarks>
        [HttpPost("LoginStaff")]
        public async Task<IActionResult> LoginStaff(LoginDTO model)
        {
            return await _accountRepository.LoginStaff(model);
        }

        [HttpDelete("Logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            return await _accountRepository.Logout(refreshToken);
        }


        [HttpGet("ForgotPasswordCustomer")]
        public async Task<IActionResult> ForgotPasswordCustomer(string email)
        {
            return await _accountRepository.ForgotPasswordCustomer(email);
        }


        [HttpGet("ForgotPasswordStaff")]
        public async Task<IActionResult> ForgotPasswordStaff(string email)
        {
            return await _accountRepository.ForgotPasswordStaff(email);
        }


        [HttpGet("VerifyResetPasswordCode")]
        public async Task<IActionResult> VerifyResetPasswordCode(string email, string code)
        {
            return await _accountRepository.VerifyResetPasswordCode(email, code);
        }

        [HttpPost("ResetPasswordCustomer")]
        public async Task<IActionResult> ResetPasswordCustomer([FromBody] ResetPasswordDTO model)
        {
            return await _accountRepository.ResetPasswordCustomer(model.Email, model.NewPassword, model.LoginAfterReset);
        }
        [HttpPost("ResetPasswordStaff")]
        public async Task<IActionResult> ResetPasswordStaff([FromBody] ResetPasswordDTO model)
        {
            return await _accountRepository.ResetPasswordStaff(model.Email, model.NewPassword, model.LoginAfterReset);
        }

        [HttpPost("RenewAccessToken")]
        public async Task<IActionResult> RenewToken([FromBody] PersonalInformationDTO model)
        {
            return await _accountRepository.RenewToken(model);
        }

        [HttpGet("LoginGoogle")]
        public async Task LoginGoogle()
        {
            await _accountRepository.LoginGoogle();
        }


        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            return await _accountRepository.GoogleResponse();
        }


        [HttpGet("checkCCCD")]
        public async Task<IActionResult> CheckCCCD(string cccd)
        {
            return await _accountRepository.CheckCCCD(cccd);
        }
        [HttpGet("CheckPassword")]
        public async Task<IActionResult> CheckPassword(string email, string password)
        {
            return await _accountRepository.CheckPassword(email, password);
        }

        [HttpGet("checkUsername")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            return await _accountRepository.CheckUsername(username);
        }

        [HttpGet("checkEmail")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            return await _accountRepository.CheckEmail(email);
        }
        [HttpGet("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(string email)
        {
            return await _accountRepository.SendVerificationCode(email);
        }


        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            return await _accountRepository.VerifyEmail(email, code);
        }
        [HttpPost("MobileGoogleLogin")]
        public async Task<IActionResult> MobileGoogleLogin([FromBody] MobileGoogleLoginDTO model)
        {
            return await _accountRepository.MobileGoogleLogin(model);
        }
        
    }
}
