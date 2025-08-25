using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.HashPassword;
using APIClothesEcommerceShop.Repositories.Token;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Net.Mail;
using MailKit.Net.Smtp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Caching.Memory;
using APIClothesEcommerceShop.DTO.Account;
using Newtonsoft.Json;
using System.Net.Http;
using static APIClothesEcommerceShop.Controllers.AccountController;
using static ServiceStack.Diagnostics.Events;
using NetHttpClient = System.Net.Http.HttpClient;

namespace APIClothesEcommerceShop.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly NetHttpClient _httpClient;

        private readonly IPasswordHasher _hasher;
        private readonly ITokenServices _tokenServices;
        private readonly EcommerceShopContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public AccountRepository(IHttpClientFactory httpClientFactory ,IConfiguration configuration ,IPasswordHasher hasher, ITokenServices tokenServices, EcommerceShopContext db, IHttpContextAccessor httpContextAccessor,IMemoryCache cache, NetHttpClient httpClient)
        {
            _hasher = hasher;
            _tokenServices = tokenServices;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
        }
        [HttpPost("VerifyRecaptcha")]
        public async Task<IActionResult> VerifyRecaptcha([FromBody] RecaptchaVerificationDTO model)
        {
            if (string.IsNullOrEmpty(model.RecaptchaToken))
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Token reCAPTCHA không được cung cấp."
                });
            }

            var client = _httpClientFactory.CreateClient();
            var recaptchaSecret = _configuration["Recaptcha:SecretKey"]; // Lấy từ appsettings.json
            var recaptchaUrl = "https://www.google.com/recaptcha/api/siteverify";
            var recaptchaContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("secret", recaptchaSecret),
            new KeyValuePair<string, string>("response", model.RecaptchaToken)
        });

            try
            {
                var recaptchaResponse = await client.PostAsync(recaptchaUrl, recaptchaContent);
                if (!recaptchaResponse.IsSuccessStatusCode)
                {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Lỗi khi xác minh reCAPTCHA."
                    });
                }

                var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(await recaptchaResponse.Content.ReadAsStringAsync());
                if (!recaptchaResult.Success)
                {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Xác minh reCAPTCHA không thành công: " + string.Join(", ", recaptchaResult.ErrorCodes ?? new string[] { })
                    });
                }

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Xác minh reCAPTCHA thành công."
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = $"Lỗi khi xác minh reCAPTCHA: {ex.Message}"
                });
            }
        }
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!_cache.TryGetValue($"VerificationCode_{model.Email.ToLower()}", out string storedCode))
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email chưa được xác minh. Vui lòng gửi và xác minh mã trước khi đăng ký."
                });
            }
            string hashPass = _hasher.HashPassword(model.MatKhau);
            try
            {
                var addCustomerDb = new Khachhang
                {
                    HoTen = model.HoTen,
                    TenTaiKhoan = model.TenTaiKhoan,
                    Email = model.Email,
                    MatKhau = hashPass,
                    NgayTao = DateTime.Now,
                    IsActive = true,
                    TinhTrang = "Đang hoạt động",
                };
                _db.Khachhangs.Add(addCustomerDb);
                await _db.SaveChangesAsync();
                _cache.Remove($"VerificationCode_{model.Email.ToLower()}");
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Register successfully",
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }
        public async Task<IActionResult> SendVerificationCode(string email)
        {
            // Kiểm tra xem email đã tồn tại trong Khachhangs chưa
            var existingUser = await _db.Khachhangs
                .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower());
            if (existingUser != null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email này đã được đăng ký."
                });
            }

            // Tạo mã xác minh
            string verificationCode = new Random().Next(100000, 999999).ToString();

            // Lưu mã xác minh vào cache với thời hạn 10 phút
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            _cache.Set($"VerificationCode_{email.ToLower()}", verificationCode, cacheEntryOptions);

            // Gửi email chứa mã xác minh
            try
            {
                await SendEmailAsync(
                    email,
                    "Xác minh email",
                    $"Mã xác minh của bạn là: {verificationCode}. Mã này có hiệu lực trong 10 phút."
                );
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Mã xác minh đã được gửi tới email của bạn."
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Lỗi khi gửi email: " + ex.Message
                });
            }
        }
        public Task<IActionResult> VerifyEmail(string email, string code)
        {
            // Kiểm tra mã xác minh trong cache
            if (!_cache.TryGetValue($"VerificationCode_{email.ToLower()}", out string storedCode))
            {
                return Task.FromResult<IActionResult>(new OkObjectResult(new
                {
                    Success = false,
                    Message = "Mã xác minh không tồn tại hoặc đã hết hạn."
                }));
            }

            if (storedCode != code)
            {
                return Task.FromResult<IActionResult>(new OkObjectResult(new
                {
                    Success = false,
                    Message = "Mã xác minh không đúng."
                }));
            }

            return Task.FromResult<IActionResult>(new OkObjectResult(new
            {
                Success = true,
                Message = "Xác minh email thành công."
            }));
        }
        public async Task<IActionResult> LoginCustomer(LoginDTO model)
        {
            var findUser = await _db.Khachhangs.AsNoTracking()
                .FirstOrDefaultAsync(p => p.TenTaiKhoan.Trim().ToLower() == model.Email_TenTaiKhoan.Trim().ToLower() ||
                                         p.Email.Trim().ToLower() == model.Email_TenTaiKhoan.Trim().ToLower());
            if (findUser == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại"
                });
            }

            if (findUser.TinhTrang.Trim().ToLower() != "Đang hoạt động".ToLower())
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Tài khoản đang bị tạm khóa"
                });
            }
            if (findUser.IsActive != true)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "false"
                });
            }
            bool isPasswordValid = _hasher.VerifyPassword(model.MatKhau, findUser.MatKhau.Trim());
            if (!isPasswordValid)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Sai mật khẩu"
                });
            }

            var khachhang = new PersonalInformationDTO
            {
                Id = findUser.MaKh,
                HoTen = findUser.HoTen,
                SDT = findUser.Sdt,
                VaiTro = "Customer",
                Hinh = findUser.HinhDaiDien
            };
            var accessToken = _tokenServices.GenerateAccessToken(khachhang);
            var refreshToken = _tokenServices.GenerateRefreshToken();
            var addRefreshTokenDb = new Refreshtoken
            {
                UserId = findUser.MaKh,
                Token = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(1),
            };
            _db.Refreshtokens.Add(addRefreshTokenDb);
            await _db.SaveChangesAsync();
            khachhang.RefreshToken = refreshToken;
            return new OkObjectResult(new
            {
                Success = true,
                Message = "Login successfully",
                Data = new TokenResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                },
            });
        }

        public async Task<IActionResult> LoginStaff(LoginDTO model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email_TenTaiKhoan) || string.IsNullOrEmpty(model.MatKhau))
            {
                return new BadRequestObjectResult(new
                {
                    Success = false,
                    Message = "Email/Tên tài khoản và mật khẩu là bắt buộc"
                });
            }
            var findUser = await _db.Nhanviens.AsNoTracking()
                .FirstOrDefaultAsync(p => (p.TenTaiKhoan != null && p.TenTaiKhoan.Trim().ToLower() == model.Email_TenTaiKhoan.Trim().ToLower()) ||
                                         (p.Email != null && p.Email.Trim().ToLower() == model.Email_TenTaiKhoan.Trim().ToLower()));
            if (findUser == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại"
                });
            }

            if (findUser.TinhTrang?.Trim().ToLower() != "Đang hoạt động".ToLower())
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Tài khoản đang bị tạm khóa"
                });
            }
            if (findUser.IsActive != true)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "false"
                });
            }
            bool isPasswordValid = _hasher.VerifyPassword(model.MatKhau, findUser.MatKhau.Trim());
            if (!isPasswordValid)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Sai mật khẩu"
                });
            }

            var cv = await _db.Chucvus.FirstOrDefaultAsync(p => p.MaChucVu == findUser.MaChucVu);
            string tenCv = cv?.TenChucVu.Trim() ?? "";
            var nhanvien = new PersonalInformationDTO
            {
                Id = findUser.MaNv,
                HoTen = findUser.HoTen,
                SDT = findUser.Sdt,
                VaiTro = tenCv,
                Hinh = null
            };
            var accessToken = _tokenServices.GenerateAccessToken(nhanvien);
            var refreshToken = _tokenServices.GenerateRefreshToken();
            var addRefreshTokenDb = new Refreshtoken
            {
                UserId = findUser.MaNv,
                Token = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(1),
            };
            _db.Refreshtokens.Add(addRefreshTokenDb);
            await _db.SaveChangesAsync();
            nhanvien.RefreshToken = refreshToken;
            return new OkObjectResult(new
            {
                Success = true,
                Message = "Login successfully",
                Data = new TokenResponseDTO
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                },
            });
        }

        public async Task<IActionResult> Logout(string refreshToken)
        {
            var checkRefreshToken = await _db.Refreshtokens.FirstOrDefaultAsync(p => p.Token == refreshToken);
            if (checkRefreshToken == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Logout failed"
                });
            }
            try
            {
                _db.Refreshtokens.Remove(checkRefreshToken);
                await _db.SaveChangesAsync();
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Logout successfully"
                });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<IActionResult> ForgotPasswordCustomer(string email)
        {
            var checkEmail = await _db.Khachhangs.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == email.Trim().ToLower());
            if (checkEmail == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email chưa được đăng ký với tài khoản nào"
                });
            }

            // Tạo mã xác minh
            string verificationCode = new Random().Next(100000, 999999).ToString();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            _cache.Set($"ResetPasswordCode_{email.ToLower()}", verificationCode, cacheEntryOptions);

            // Gửi mã xác minh qua email
            try
            {
                await SendEmailAsync(email, "Xác minh đặt lại mật khẩu",
                    $"Mã xác minh của bạn là: {verificationCode}. Mã này có hiệu lực trong 1 phút.");
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Mã xác minh đã được gửi tới email của bạn."
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Lỗi khi gửi email: " + ex.Message
                });
            }
        }

        public async Task<IActionResult> ForgotPasswordStaff(string email)
        {
            var checkEmail = await _db.Nhanviens.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == email.Trim().ToLower());
            if (checkEmail == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email chưa được đăng ký với tài khoản nào"
                });
            }

            // Tạo mã xác minh
            string verificationCode = new Random().Next(100000, 999999).ToString();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            _cache.Set($"ResetPasswordCode_{email.ToLower()}", verificationCode, cacheEntryOptions);

            // Gửi mã xác minh qua email
            try
            {
                await SendEmailAsync(email, "Xác minh đặt lại mật khẩu",
                    $"Mã xác minh của bạn là: {verificationCode}. Mã này có hiệu lực trong 1 phút.");
                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Mã xác minh đã được gửi tới email của bạn."
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Lỗi khi gửi email: " + ex.Message
                });
            }
        }

        public Task<IActionResult> VerifyResetPasswordCode(string email, string code)
        {
            if (!_cache.TryGetValue($"ResetPasswordCode_{email.ToLower()}", out string storedCode))
            {
                return Task.FromResult<IActionResult>(new OkObjectResult(new
                {
                    Success = false,
                    Message = "Mã xác minh không tồn tại hoặc đã hết hạn."
                }));
            }

            if (storedCode != code)
            {
                return Task.FromResult<IActionResult>(new OkObjectResult(new
                {
                    Success = false,
                    Message = "Mã xác minh không đúng."
                }));
            }

            return Task.FromResult<IActionResult>(new OkObjectResult(new
            {
                Success = true,
                Message = "Xác minh thành công. Vui lòng nhập mật khẩu mới."
            }));
        }

        public async Task<IActionResult> ResetPasswordCustomer(string email, string newPassword, bool loginAfterReset)
        {
            var checkEmail = await _db.Khachhangs.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == email.Trim().ToLower());
            if (checkEmail == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email không tồn tại."
                });
            }

            // Cập nhật mật khẩu mới
            checkEmail.MatKhau = _hasher.HashPassword(newPassword);
            _db.Update(checkEmail);
            await _db.SaveChangesAsync();

            // Xóa mã xác minh khỏi cache
            _cache.Remove($"ResetPasswordCode_{email.ToLower()}");

            // Nếu người dùng chọn đăng nhập ngay
            if (loginAfterReset)
            {
                var khachhang = new PersonalInformationDTO
                {
                    Id = checkEmail.MaKh,
                    HoTen = checkEmail.HoTen,
                    SDT = checkEmail.Sdt,
                    VaiTro = "Customer",
                    Hinh = checkEmail.HinhDaiDien
                };
                var accessToken = _tokenServices.GenerateAccessToken(khachhang);
                var refreshToken = _tokenServices.GenerateRefreshToken();
                var addRefreshTokenDb = new Refreshtoken
                {
                    UserId = checkEmail.MaKh,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(1),
                };
                _db.Refreshtokens.Add(addRefreshTokenDb);
                await _db.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu và đăng nhập thành công.",
                    Data = new TokenResponseDTO
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    }
                });
            }

            return new OkObjectResult(new
            {
                Success = true,
                Message = "Đặt lại mật khẩu thành công."
            });
        }

        public async Task<IActionResult> ResetPasswordStaff(string email, string newPassword, bool loginAfterReset)
        {
            var checkEmail = await _db.Nhanviens.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == email.Trim().ToLower());
            if (checkEmail == null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email không tồn tại."
                });
            }
            // Cập nhật mật khẩu mới
            checkEmail.MatKhau = _hasher.HashPassword(newPassword);
            // Lưu ý: Mật khẩu nhân viên không mã hóa trong mã gốc
            _db.Update(checkEmail);
            await _db.SaveChangesAsync();

            // Xóa mã xác minh khỏi cache
            _cache.Remove($"ResetPasswordCode_{email.ToLower()}");

            // Nếu người dùng chọn đăng nhập ngay
            if (loginAfterReset)
            {
                var cv = await _db.Chucvus.FirstOrDefaultAsync(p => p.MaChucVu == checkEmail.MaChucVu);
                string tenCv = cv?.TenChucVu.Trim() ?? "";
                var nhanvien = new PersonalInformationDTO
                {
                    Id = checkEmail.MaNv,
                    HoTen = checkEmail.HoTen,
                    SDT = checkEmail.Sdt,
                    VaiTro = tenCv,
                    Hinh = null
                };
                var accessToken = _tokenServices.GenerateAccessToken(nhanvien);
                var refreshToken = _tokenServices.GenerateRefreshToken();
                var addRefreshTokenDb = new Refreshtoken
                {
                    UserId = checkEmail.MaNv,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(1),
                };
                _db.Refreshtokens.Add(addRefreshTokenDb);
                await _db.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu và đăng nhập thành công.",
                    Data = new TokenResponseDTO
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    }
                });
            }

            return new OkObjectResult(new
            {
                Success = true,
                Message = "Đặt lại mật khẩu thành công."
            });
        }

        public Task<IActionResult> RenewToken(PersonalInformationDTO model)
        {
            var checkRefreshToken = _db.Refreshtokens.AsNoTracking().FirstOrDefault(p => p.Token == model.RefreshToken);
            if (checkRefreshToken == null || checkRefreshToken.ExpiredAt < DateTime.UtcNow)
            {
                return Task.FromResult<IActionResult>(new OkObjectResult(new
                {
                    Success = false,
                    Message = "RefreshToken has expired. Login again",
                }));
            }
            var information = new PersonalInformationDTO
            {
                Id = model.Id,
                HoTen = model.HoTen,
                SDT = model.SDT,
                VaiTro = model.VaiTro,
            };
            var generateAccessToken = _tokenServices.GenerateAccessToken(information);
            return Task.FromResult<IActionResult>(new OkObjectResult(new
            {
                Success = true,
                Message = "Renew AccessToken successfully",
                Data = new TokenResponseDTO
                {
                    AccessToken = generateAccessToken,
                    RefreshToken = model.RefreshToken,
                }
            }));
        }

        public async Task LoginGoogle()
        {
            await _httpContextAccessor.HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = "/api/Account/GoogleResponse" // Điều chỉnh theo cấu hình URL của bạn
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await _httpContextAccessor.HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return new RedirectResult($"http://localhost:5173/GoogleLoginSuccess?error={HttpUtility.UrlEncode("Xác thực Google thất bại. Vui lòng thử lại!")}");
            }
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var existingUser = await _db.Khachhangs.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null && (string.IsNullOrWhiteSpace(existingUser.TinhTrang) || existingUser.TinhTrang.Trim().ToLower() != "đang hoạt động"))
            {
                return new RedirectResult($"http://localhost:5173/GoogleLoginSuccess?error={HttpUtility.UrlEncode("Tài khoản đang bị tạm khóa hoặc không hợp lệ")}");
            }
            if (existingUser == null)
            {
                existingUser = new Khachhang
                {
                    HoTen = name,
                    Email = email,
                    TinhTrang = "Đang hoạt động",
                    NgayTao = DateTime.Now,
                    IsActive = true,
                };
                _db.Khachhangs.Add(existingUser);
                await _db.SaveChangesAsync();
            }
            var model = new PersonalInformationDTO
            {
                Id = existingUser.MaKh,
                HoTen = existingUser.HoTen ?? "",
                SDT = existingUser.Sdt ?? "",
                VaiTro = "Customer"
            };
            var accessToken = _tokenServices.GenerateAccessToken(model);
            var refreshToken = _tokenServices.GenerateRefreshToken();
            var addRefreshTokenDb = new Refreshtoken
            {
                UserId = existingUser.MaKh,
                Token = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(1),
            };
            _db.Refreshtokens.Add(addRefreshTokenDb);
            await _db.SaveChangesAsync();
            return new RedirectResult($"http://localhost:5173/GoogleLoginSuccess?access_token={accessToken}&refresh_token={refreshToken}");
        }

        public async Task<IActionResult> CheckCCCD(string cccd)
        {
            var findCCCD = await _db.Khachhangs.AsNoTracking().FirstOrDefaultAsync(p => p.Cccd == cccd);
            if (findCCCD != null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "CCCD này đã tồn tại"
                });
            }
            return new OkObjectResult(new
            {
                Success = true,
                Message = "CCCD này hợp lệ"
            });
        }

        public async Task<IActionResult> CheckUsername(string username)
        {
            var findUsername = await _db.Khachhangs.AsNoTracking().FirstOrDefaultAsync(p => p.TenTaiKhoan == username);
            if (findUsername != null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Tên tài khoản này đã tồn tại"
                });
            }
            return new OkObjectResult(new
            {
                Success = true,
                Message = "Tên tài khoản này hợp lệ"
            });
        }
        public async Task<IActionResult> CheckPassword(string email, string password)
        {
            var findPassWordByEmail = await _db.Khachhangs.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email);
            bool isPasswordValid = _hasher.VerifyPassword(password, findPassWordByEmail.MatKhau.Trim());
            if (isPasswordValid)
            {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Mật khẩu bạn nhập trùng với mật khẩu cũ, vui lòng nhập mật khẩu khác"
                    });  
            }
            return new OkObjectResult(new
            {
                Success = true,
                Message = "Mật khẩu hợp lệ"
            });
        }

        public async Task<IActionResult> CheckEmail(string email)
        {
            var findEmail = await _db.Khachhangs.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email);
            if (findEmail != null)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Email này đã tồn tại"
                });
            }
            return new OkObjectResult(new
            {
                Success = true,
                Message = "Email này hợp lệ"
            });
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Angel Fashion", "khongbiet12kk@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync("khongbiet12kk@gmail.com", "baey jlku ooat epom");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<IActionResult> MobileGoogleLogin(MobileGoogleLoginDTO model)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken;
                try
                {
                    jwtToken = handler.ReadJwtToken(model.AccessToken);
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Invalid Google access token: " + ex.Message
                    });
                }

                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Không thể lấy email từ Google token"
                    });
                }

                var existingUser = await _db.Khachhangs.FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower());
                if (existingUser != null && (string.IsNullOrWhiteSpace(existingUser.TinhTrang) || existingUser.TinhTrang.Trim().ToLower() != "đang hoạt động"))
                {
                    return new OkObjectResult(new
                    {
                        Success = false,
                        Message = "Tài khoản đang bị tạm khóa hoặc không hợp lệ"
                    });
                }

                if (existingUser == null)
                {
                    existingUser = new Khachhang
                    {
                        HoTen = name,
                        Email = email,
                        TinhTrang = "Đang hoạt động",
                        NgayTao = DateTime.Now,
                        IsActive = true,
                    };
                    _db.Khachhangs.Add(existingUser);
                    await _db.SaveChangesAsync();
                }

                var userInfo = new PersonalInformationDTO
                {
                    Id = existingUser.MaKh,
                    HoTen = existingUser.HoTen ?? "",
                    SDT = existingUser.Sdt ?? "",
                    VaiTro = "Customer"
                };

                var accessToken = _tokenServices.GenerateAccessToken(userInfo);
                var refreshToken = _tokenServices.GenerateRefreshToken();

                var addRefreshTokenDb = new Refreshtoken
                {
                    UserId = existingUser.MaKh,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(1),
                };
                _db.Refreshtokens.Add(addRefreshTokenDb);
                await _db.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    Success = true,
                    Message = "Mobile Google login successful",
                    Data = new TokenResponseDTO
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    }
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new
                {
                    Success = false,
                    Message = "Lỗi đăng nhập Google trên mobile: " + ex.Message
                });
            }
        }
        public async Task LoginGoogleCustom(string redirectUri)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUri != null
                    ? $"{redirectUri}?returnUrl=/api/Account/GoogleResponseCustom"
                    : "/api/Account/GoogleResponseCustom"
            };
            await _httpContextAccessor.HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, properties);
        }

        public async Task<IActionResult> GoogleResponseCustom()
        {
            var result = await _httpContextAccessor.HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return new RedirectResult($"http://localhost:8080/google-login-success?error={HttpUtility.UrlEncode("Xác thực Google thất bại. Vui lòng thử lại!")}");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var existingUser = await _db.Khachhangs.FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null && (string.IsNullOrWhiteSpace(existingUser.TinhTrang) || existingUser.TinhTrang.Trim().ToLower() != "đang hoạt động"))
            {
                return new RedirectResult($"http://localhost:8080/google-login-success?error={HttpUtility.UrlEncode("Tài khoản đang bị tạm khóa hoặc không hợp lệ")}");
            }

            if (existingUser == null)
            {
                existingUser = new Khachhang
                {
                    HoTen = name,
                    Email = email,
                    TinhTrang = "Đang hoạt động",
                    NgayTao = DateTime.UtcNow,
                    IsActive = true,
                };
                _db.Khachhangs.Add(existingUser);
                await _db.SaveChangesAsync();
            }

            var model = new PersonalInformationDTO
            {
                Id = existingUser.MaKh,
                HoTen = existingUser.HoTen ?? "",
                SDT = existingUser.Sdt ?? "",
                VaiTro = "Customer"
            };

            var accessToken = _tokenServices.GenerateAccessToken(model);
            var refreshToken = _tokenServices.GenerateRefreshToken();
            var addRefreshTokenDb = new Refreshtoken
            {
                UserId = existingUser.MaKh,
                Token = refreshToken,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddDays(1),
            };
            _db.Refreshtokens.Add(addRefreshTokenDb);
            await _db.SaveChangesAsync();

            return new RedirectResult($"http://localhost:8080/google-login-success?access_token={accessToken}&refresh_token={refreshToken}");
        }

        public Task<bool> VerifyRecaptchaAsync(string recaptchaToken)
        {
            throw new NotImplementedException();
        }
    }
}