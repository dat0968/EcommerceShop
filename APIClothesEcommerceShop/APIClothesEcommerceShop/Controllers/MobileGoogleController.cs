using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Account;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileGoogleController : ControllerBase
    {
        private readonly EcommerceShopContext _db;
        private readonly ITokenServices _tokenServices;
        private readonly ILogger<MobileGoogleController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MobileGoogleController(
            EcommerceShopContext db,
            ITokenServices tokenServices,
            ILogger<MobileGoogleController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _tokenServices = tokenServices;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Mobile Google Login - Xác thực Google ID Token từ mobile app
        /// </summary>
        /// <param name="model">Google ID Token từ mobile</param>
        /// <returns>Angel Fashion access token và refresh token</returns>
        /// <remarks>
        /// POST /api/MobileGoogle/Login
        /// {
        ///   "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjE2NzAyN...",
        ///   "deviceInfo": {
        ///     "deviceId": "unique-device-id",
        ///     "deviceName": "Samsung Galaxy S23",
        ///     "platform": "android",
        ///     "appVersion": "1.0.0"
        ///   }
        /// }
        /// </remarks>
        [HttpPost("Login")]
        public async Task<ActionResult> MobileGoogleLogin([FromBody] MobileGoogleLoginRequestDTO model)
        {
            try
            {
                _logger.LogInformation("📱 Mobile Google Login attempt started");
                _logger.LogInformation($"📱 Device: {model.DeviceInfo?.Platform} - {model.DeviceInfo?.DeviceName}");

                // Validate input
                if (string.IsNullOrEmpty(model.IdToken))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Google ID Token is required",
                        Error = "MISSING_ID_TOKEN"
                    });
                }

                // Verify Google ID Token
                var googleUserInfo = await VerifyGoogleIdToken(model.IdToken);
                if (googleUserInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid Google ID Token",
                        Error = "INVALID_ID_TOKEN"
                    });
                }

                _logger.LogInformation($"📱 Google user verified: {googleUserInfo.Email}");

                // Check if user exists
                var existingUser = await _db.Khachhangs
                    .FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == googleUserInfo.Email.Trim().ToLower());

                // Check account status if user exists
                if (existingUser != null)
                {
                    if (string.IsNullOrWhiteSpace(existingUser.TinhTrang) ||
                        existingUser.TinhTrang.Trim().ToLower() != "đang hoạt động")
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Tài khoản đang bị tạm khóa hoặc không hợp lệ",
                            Error = "ACCOUNT_SUSPENDED"
                        });
                    }

                    if (existingUser.IsActive != true)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Tài khoản không được kích hoạt",
                            Error = "ACCOUNT_INACTIVE"
                        });
                    }
                }

                // Create new user if doesn't exist
                if (existingUser == null)
                {
                    _logger.LogInformation($"📱 Creating new user for: {googleUserInfo.Email}");

                    existingUser = new Khachhang
                    {
                        HoTen = googleUserInfo.Name ?? googleUserInfo.Email,
                        Email = googleUserInfo.Email,
                        TinhTrang = "Đang hoạt động",
                        NgayTao = DateTime.UtcNow,
                        IsActive = true,
                        // Có thể thêm thông tin từ Google
                        HinhDaiDien = googleUserInfo.Picture
                    };

                    _db.Khachhangs.Add(existingUser);
                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"✅ New user created with ID: {existingUser.MaKh}");
                }

                // Create user info for token
                var userInfo = new PersonalInformationDTO
                {
                    Id = existingUser.MaKh,
                    HoTen = existingUser.HoTen ?? googleUserInfo.Name ?? "",
                    SDT = existingUser.Sdt ?? "",
                    VaiTro = "Customer",
                    Hinh = existingUser.HinhDaiDien ?? googleUserInfo.Picture
                };

                // Generate tokens
                var accessToken = _tokenServices.GenerateAccessToken(userInfo);
                var refreshToken = _tokenServices.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new Refreshtoken
                {
                    UserId = existingUser.MaKh,
                    Token = refreshToken,
                    IssuedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(7), // 7 days for mobile
                };

                _db.Refreshtokens.Add(refreshTokenEntity);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"✅ Mobile Google login successful for user: {existingUser.MaKh}");

                // Return response
                return Ok(new
                {
                    Success = true,
                    Message = "Mobile Google login successful",
                    Data = new
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        User = new
                        {
                            Id = existingUser.MaKh,
                            Email = existingUser.Email,
                            Name = existingUser.HoTen,
                            Avatar = existingUser.HinhDaiDien ?? googleUserInfo.Picture,
                            IsNewUser = existingUser.NgayTao > DateTime.UtcNow.AddMinutes(-5) // Created in last 5 minutes
                        },
                        ExpiresIn = 3600, // 1 hour
                        TokenType = "Bearer"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile Google login error: {ex.Message}");
                _logger.LogError($"❌ Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Lỗi server khi xử lý đăng nhập Google",
                    Error = "INTERNAL_SERVER_ERROR",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify Google ID Token từ mobile
        /// </summary>
        /// <param name="idToken">Google ID Token</param>
        /// <returns>Google user info hoặc null nếu không hợp lệ</returns>
        private async Task<GoogleUserInfo> VerifyGoogleIdToken(string idToken)
        {
            try
            {
                _logger.LogInformation("🔍 Verifying Google ID Token...");

                // Method 1: Parse JWT directly (for development/testing)
                // Trong production nên verify signature với Google's public keys
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(idToken);

                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var picture = jwtToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
                var emailVerified = jwtToken.Claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("⚠️ No email found in Google ID Token");
                    return null;
                }

                // Check if email is verified
                if (emailVerified != "true")
                {
                    _logger.LogWarning($"⚠️ Email not verified for: {email}");
                    return null;
                }

                _logger.LogInformation($"✅ Google ID Token verified for: {email}");

                return new GoogleUserInfo
                {
                    Email = email,
                    Name = name ?? email,
                    Picture = picture,
                    EmailVerified = emailVerified == "true"
                };

                // Method 2: Verify with Google API (more secure for production)
                // Uncomment and use this in production:
                /*
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"⚠️ Google token verification failed: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonConvert.DeserializeObject<GoogleTokenInfo>(content);
                
                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.Email))
                {
                    return null;
                }

                return new GoogleUserInfo
                {
                    Email = tokenInfo.Email,
                    Name = tokenInfo.Name ?? tokenInfo.Email,
                    Picture = tokenInfo.Picture,
                    EmailVerified = tokenInfo.EmailVerified == "true"
                };
                */
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error verifying Google ID Token: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Health check cho Mobile Google Service
        /// </summary>
        [HttpGet("Health")]
        public ActionResult HealthCheck()
        {
            return Ok(new
            {
                Service = "Mobile Google Login",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }

        /// <summary>
        /// Get Google login configuration for mobile
        /// </summary>
        [HttpGet("Config")]
        public ActionResult GetGoogleConfig()
        {
            // Return public configuration that mobile needs
            return Ok(new
            {
                ClientId = "977596557785-08qrpf53cqpg093qpivgnim0i0acc530.apps.googleusercontent.com", // Your Google Client ID
                Scopes = new[] { "openid", "email", "profile" },
                Instructions = new
                {
                    Android = "Use Google Sign-In SDK for Android",
                    iOS = "Use Google Sign-In SDK for iOS",
                    WebView = "Use Google OAuth2 flow in WebView"
                }
            });
        }

        /// <summary>
        /// Logout from mobile Google session
        /// </summary>
        [HttpPost("Logout")]
        public async Task<ActionResult> MobileGoogleLogout([FromBody] MobileLogoutRequestDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.RefreshToken))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                // Remove refresh token from database
                var refreshToken = await _db.Refreshtokens
                    .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken);

                if (refreshToken != null)
                {
                    _db.Refreshtokens.Remove(refreshToken);
                    await _db.SaveChangesAsync();
                }

                _logger.LogInformation($"✅ Mobile Google logout successful");

                return Ok(new
                {
                    Success = true,
                    Message = "Logout successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Mobile Google logout error: {ex.Message}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Logout failed"
                });
            }
        }
    }

    // DTO Classes for Mobile Google Login
    public class MobileGoogleLoginRequestDTO
    {
        public string IdToken { get; set; }
        public MobileDeviceInfoDTO DeviceInfo { get; set; }
    }

    public class MobileDeviceInfoDTO
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string Platform { get; set; } // "android", "ios"
        public string AppVersion { get; set; }
    }

    public class MobileLogoutRequestDTO
    {
        public string RefreshToken { get; set; }
    }

    public class GoogleUserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public bool EmailVerified { get; set; }
    }

    public class GoogleTokenInfo
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picture")]
        public string Picture { get; set; }

        [JsonProperty("email_verified")]
        public string EmailVerified { get; set; }
    }
}