using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Account;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static APIClothesEcommerceShop.Repositories.Account.AccountRepository;

namespace APIClothesEcommerceShop.Repositories.Account
{
    public interface IAccountRepository
    {
        Task<IActionResult> Register(RegisterDTO model);

        Task<IActionResult> VerifyRecaptcha(RecaptchaVerificationDTO model);

        Task<IActionResult> LoginCustomer(LoginDTO model);
        Task<IActionResult> LoginStaff(LoginDTO model);

        Task<IActionResult> Logout(string refreshToken);

        Task<IActionResult> ForgotPasswordCustomer(string email);

        Task<IActionResult> ForgotPasswordStaff(string email);
        Task<IActionResult> VerifyResetPasswordCode(string email, string code);

        Task<IActionResult> ResetPasswordCustomer(string email, string newPassword, bool loginAfterReset);

       
        Task<IActionResult> ResetPasswordStaff(string email, string newPassword, bool loginAfterReset);
       
        Task<IActionResult> RenewToken(PersonalInformationDTO model);

        Task LoginGoogle();

       
        Task<IActionResult> GoogleResponse();

        Task<IActionResult> CheckCCCD(string cccd);

      
        Task<IActionResult> CheckUsername(string username);
        Task<IActionResult> CheckPassword(string email, string password);
        Task<IActionResult> CheckEmail(string email);
        
        Task<IActionResult> SendVerificationCode(string email);
        Task<IActionResult> VerifyEmail(string email, string code);
        Task<IActionResult> MobileGoogleLogin(MobileGoogleLoginDTO model);
        Task LoginGoogleCustom(string redirectUri); // API đăng nhập Google mới
        Task<IActionResult> GoogleResponseCustom();
    }
}
