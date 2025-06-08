using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Staff;

namespace APIClothesEcommerceShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly EcommerceShopContext _db;
        private readonly IStaffRepository _staffRepository;
        public ChatController(EcommerceShopContext db, IStaffRepository staffRepository)
        {
            _db = db;
            _staffRepository = staffRepository;
        }

        [HttpGet("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin người dùng" });
                }

                if (!int.TryParse(userId, out int customerId))
                {
                    return BadRequest(new { Success = false, Message = "ID người dùng không hợp lệ" });
                }

                var customer = await _db.Khachhangs.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy khách hàng" });
                }

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        id = customer.MaKh,
                        hoTen = customer.HoTen,
                        email = customer.Email,
                        sdt = customer.Sdt,
                        hinh = customer.HinhDaiDien
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpGet("GetStaffInfo")]
        public async Task<IActionResult> GetStaffInfo()
        {
            try
            {
                var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(staffId))
                {
                    return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin người dùng" });
                }

                if (!int.TryParse(staffId, out int nhanVienId))
                {
                    return BadRequest(new { Success = false, Message = "ID người dùng không hợp lệ" });
                }

                var nhanVien = await _db.Nhanviens.FindAsync(nhanVienId);
                if (nhanVien == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy nhân viên" });
                }

                var chucVu = await _db.Chucvus.FindAsync(nhanVien.MaChucVu);
                string tenChucVu = chucVu?.TenChucVu ?? "Staff";

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        id = nhanVien.MaNv,
                        hoTen = nhanVien.HoTen,
                        email = nhanVien.Email,
                        sdt = nhanVien.Sdt,
                        vaiTro = tenChucVu,
                        hinh = nhanVien.HinhDaiDien
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpGet("GetCustomerById")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _db.Khachhangs.FindAsync(id);
                if (customer == null)
                {
                    return NotFound(new { Success = false, Message = "Không tìm thấy khách hàng" });
                }

                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        id = customer.MaKh,
                        hoTen = customer.HoTen,
                        email = customer.Email,
                        sdt = customer.Sdt,
                        diaChi = customer.DiaChi,
                        ngayTao = customer.NgayTao,
                        tinhTrang = customer.TinhTrang,
                        hinh = customer.HinhDaiDien
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost("upload-media")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Không có file được upload" });
                }

                // Kiểm tra loại file
                var allowedTypes = new[]
                {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp",
            "video/mp4", "video/webm", "video/ogg"
        };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Chỉ chấp nhận hình ảnh (JPG, PNG, GIF, WebP) hoặc video (MP4, WebM, OGG)" });
                }

                // Tăng giới hạn lên 20MB
                if (file.Length > 20 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File quá lớn. Tối đa 20MB" });
                }

                // Tạo tên file unique
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // Đường dẫn lưu file
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "chat");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Log upload info
                var userIdClaim = User.FindFirst("id")?.Value;
                var userNameClaim = User.FindFirst("name")?.Value;

                Console.WriteLine($"📤 User {userNameClaim} ({userIdClaim}) uploaded media: {fileName}");

                return Ok(new
                {
                    success = true,
                    message = "Upload thành công",
                    data = new
                    {
                        fileName = fileName,
                        originalName = file.FileName,
                        size = file.Length,
                        url = $"/api/Chat/media/{fileName}",
                        uploadedBy = userNameClaim,
                        uploadDate = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi upload media: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Lỗi server khi upload file" });
            }
        }
        [HttpGet("GetAllStaff")]
        [Authorize] // Yêu cầu xác thực
        public async Task<IActionResult> GetAllStaff()
        {
            try
            {
                // Gọi GetAllStaffAsync từ StaffRepository
                var staffList = await _staffRepository.GetAllStaffAsync(
                    pageSize: 100, // Lấy tối đa 100 nhân viên (có thể điều chỉnh)
                    pageNumber: 1,  // Trang 1
                    hoTen: null,    // Không lọc theo tên
                    gioiTinh: null, // Không lọc theo giới tính
                    tinhTrang: null // Không lọc theo trạng thái
                );

                return Ok(new
                {
                    Success = true,
                    Data = staffList.Select(s => new
                    {
                        id = s.MaNV,
                        hoTen = s.HoTen
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi hệ thống: {ex.Message}"
                });
            }
        }
        [HttpGet("media/{fileName}")]
        public IActionResult GetMedia(string fileName)
        {
            try
            {
                // Validate fileName để tránh path traversal
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest("Tên file không hợp lệ");
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "chat", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File không tồn tại");
                }

                var fileStream = System.IO.File.OpenRead(filePath);
                var mimeType = GetMimeType(fileName);

                return File(fileStream, mimeType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi get media: {ex.Message}");
                return StatusCode(500, "Lỗi server khi tải file");
            }
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".ogg" => "video/ogg",
                _ => "application/octet-stream"
            };
        }
        [HttpPost("GetOrderHistories")]
        [Authorize]
        public async Task<IActionResult> GetOrderHistories([FromBody] List<int> customerIds)
        {
            try
            {
                if (customerIds == null || !customerIds.Any())
                {
                    return BadRequest(new { Success = false, Message = "Danh sách ID khách hàng không hợp lệ" });
                }

                var orders = await _db.Hoadons
                    .Where(h => customerIds.Contains(h.MaKh) && h.IsActive == false)
                    .GroupBy(h => h.MaKh)
                    .Select(g => new
                    {
                        customerId = g.Key,
                        orders = g.Select(h => new
                        {
                            orderId = h.MaHd,
                            orderDate = h.NgayTao,
                            status = h.TinhTrang,
                            totalAmount = h.TienGoc + h.PhiVanChuyen,
                            items = h.Cthoadons.Select(ct => new
                            {
                                productName = ct.MaCtspNavigation != null ? ct.MaCtspNavigation.MaSpNavigation.TenSanPham : "Unknown",
                                quantity = ct.SoLuong,
                                price = ct.Gia - ct.GiamGia
                            }).ToList()
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Lỗi server: {ex.Message}" });
            }
        }

    }
}