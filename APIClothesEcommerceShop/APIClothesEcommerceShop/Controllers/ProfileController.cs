using APIClothesEcommerceShop.DTO.Customer;
using APIClothesEcommerceShop.Repositories.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(ICustomerRepository customerRepository, IWebHostEnvironment environment)
        {
            _customerRepository = customerRepository;
            _environment = environment;
        }

        [HttpGet("GetProfile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                Console.WriteLine("User Claims: " + string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Không tìm thấy userId trong token"
                    });
                }

                var customer = await _customerRepository.GetCustomerByIdAsync(userId);
                if (customer == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với MaKh = {userId}"
                    });
                }

                var profile = new CustomerDto
                {
                    MaKH = customer.MaKH,
                    HoTen = customer.HoTen,
                    GioiTinh = customer.GioiTinh,
                    NgaySinh = customer.NgaySinh,
                    DiaChi = customer.DiaChi,
                    CCCD = customer.CCCD,
                    SDT = customer.SDT,
                    Email = customer.Email,
                    TenTaiKhoan = customer.TenTaiKhoan,
                    MatKhau = null,
                    Hinh = customer.Hinh,
                    HinhDaiDien = null, 
                    TinhTrang = customer.TinhTrang ?? "Đang hoạt động",
                    IsActive = customer.IsActive,
                    NgayTao = customer.NgayTao
                };

                return Ok(new
                {
                    Success = true,
                    Message = "Lấy thông tin cá nhân thành công.",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] CustomerDto customerDto)
        {
            try
            {
                Console.WriteLine("Nhận được request UpdateProfile...");
                Console.WriteLine($"Dữ liệu nhận được: HoTen={customerDto.HoTen}, SDT={customerDto.SDT}, Email={customerDto.Email}");

                //if (string.IsNullOrWhiteSpace(customerDto.HoTen))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "Họ tên không được để trống"
                //    });
                //}
                //if (string.IsNullOrWhiteSpace(customerDto.GioiTinh))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "Giới tính không được để trống"
                //    });
                //}
                //if (string.IsNullOrWhiteSpace(customerDto.DiaChi))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "Địa chỉ không được để trống"
                //    });
                //}
                //if (string.IsNullOrWhiteSpace(customerDto.CCCD) || !Regex.IsMatch(customerDto.CCCD, @"^\d{12}$"))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "CCCD phải là 12 chữ số"
                //    });
                //}
                //if (string.IsNullOrWhiteSpace(customerDto.SDT) || !Regex.IsMatch(customerDto.SDT, @"^0\d{9,10}$"))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số"
                //    });
                //}
                //if (!string.IsNullOrWhiteSpace(customerDto.Email) && !Regex.IsMatch(customerDto.Email, @"^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$"))
                //{
                //    return BadRequest(new
                //    {
                //        Success = false,
                //        Message = "Email không hợp lệ"
                //    });
                //}

                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để cập nhật thông tin cá nhân."
                    });
                }
                var existingCustomer = await _customerRepository.GetCustomerByIdAsync(userId);
                if (existingCustomer == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = $"Không tìm thấy khách hàng với MaKh = {userId}"
                    });
                }

                if (customerDto.HinhDaiDien != null && customerDto.HinhDaiDien.Length > 0)
                {
                    Console.WriteLine("Bắt đầu xử lý file ảnh...");
                    if (customerDto.HinhDaiDien.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Kích thước file không được vượt quá 5MB"
                        });
                    }

                    var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(customerDto.HinhDaiDien.FileName).ToLower();
                    if (!validExtensions.Contains(extension))
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "Chỉ hỗ trợ file .jpg, .jpeg, .png"
                        });
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Console.WriteLine("Tạo thư mục uploads...");
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}_{customerDto.HinhDaiDien.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await customerDto.HinhDaiDien.CopyToAsync(stream);
                    }

                    customerDto.Hinh = $"/images/{fileName}";
                }


                var result = await _customerRepository.UpdateCustomerAsync(userId, customerDto);
                if (!result.IsSuccess)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                Console.WriteLine("Cập nhật thành công!");
                return Ok(new
                {
                    Success = true,
                    Message = "Cập nhật thông tin cá nhân thành công."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật hồ sơ: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }
    }
}