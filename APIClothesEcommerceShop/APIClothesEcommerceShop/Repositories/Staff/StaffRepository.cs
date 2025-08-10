using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Staff;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.HashPassword;
using APIClothesEcommerceShop.Repositories.Staff;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace APIClothesEcommerceShop.Repositories.Staff
{
    public class StaffRepository : IStaffRepository
    {
        private readonly EcommerceShopContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IPasswordHasher passwordHasher;

        public StaffRepository(EcommerceShopContext context, IWebHostEnvironment webHostEnvironment, IPasswordHasher passwordHasher)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            this.passwordHasher = passwordHasher;
        }

        public async Task<List<StaffDto>> GetAllStaffAsync(int pageSize, int pageNumber, string hoTen, string gioiTinh, string tinhTrang)
        {
            var query = _context.Nhanviens
                .Include(nv => nv.MaChucVuNavigation)
                .Where(nv => nv.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(hoTen))
                query = query.Where(nv => nv.HoTen.Contains(hoTen));
            if (!string.IsNullOrEmpty(gioiTinh))
                query = query.Where(nv => nv.GioiTinh == gioiTinh);
            if (!string.IsNullOrEmpty(tinhTrang))
                query = query.Where(nv => nv.TinhTrang == tinhTrang);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(nv => new StaffDto
                {
                    MaNV = nv.MaNv,
                    HoTen = nv.HoTen,
                    GioiTinh = nv.GioiTinh,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                    DiaChi = nv.DiaChi,
                    CCCD = nv.Cccd,
                    SDT = nv.Sdt,
                    Email = nv.Email,
                    NgayVaoLam = nv.NgayVaoLam.ToDateTime(new TimeOnly(0, 0)),
                    TenTaiKhoan = nv.TenTaiKhoan,
                    MatKhau = nv.MatKhau,
                    Hinh = nv.HinhDaiDien,
                    HinhDaiDien = null,
                    TinhTrang = nv.TinhTrang,
                    IsActive = nv.IsActive ?? false,
                    MaChucVu = nv.MaChucVu,
                    TenChucVu = nv.MaChucVuNavigation.TenChucVu
                })
                .ToListAsync();
        }

        public async Task<int> GetStaffCountAsync(string hoTen = null, string gioiTinh = null, string tinhTrang = null)
        {
            var query = _context.Nhanviens.AsQueryable();

            if (!string.IsNullOrEmpty(hoTen))
                query = query.Where(nv => nv.HoTen.Contains(hoTen));
            if (!string.IsNullOrEmpty(gioiTinh))
                query = query.Where(nv => nv.GioiTinh == gioiTinh);
            if (!string.IsNullOrEmpty(tinhTrang))
                query = query.Where(nv => nv.TinhTrang == tinhTrang);

            return await query.CountAsync();
        }

        public async Task<int> GetSearchCountAsync(string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null)
        {
            var query = _context.Nhanviens
                .Where(nv => nv.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(hoTen))
                query = query.Where(nv => nv.HoTen.Contains(hoTen));
            if (!string.IsNullOrEmpty(sdt))
                query = query.Where(nv => nv.Sdt.Contains(sdt));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(nv => nv.Email.Contains(email));
            if (!string.IsNullOrEmpty(cccd))
                query = query.Where(nv => nv.Cccd.Contains(cccd));
            if (!string.IsNullOrEmpty(diaChi))
                query = query.Where(nv => nv.DiaChi.Contains(diaChi));

            return await query.CountAsync();
        }

        public async Task<StaffDto> GetStaffByIdAsync(int maNV)
        {
            var staff = await _context.Nhanviens
                .Include(nv => nv.MaChucVuNavigation)
                .Where(nv => nv.MaNv == maNV)
                .Select(nv => new StaffDto
                {
                    MaNV = nv.MaNv,
                    HoTen = nv.HoTen,
                    GioiTinh = nv.GioiTinh,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                    DiaChi = nv.DiaChi,
                    CCCD = nv.Cccd,
                    SDT = nv.Sdt,
                    Email = nv.Email,
                    NgayVaoLam = nv.NgayVaoLam.ToDateTime(new TimeOnly(0, 0)),
                    TenTaiKhoan = nv.TenTaiKhoan,
                    MatKhau = nv.MatKhau,
                    Hinh = nv.HinhDaiDien,
                    HinhDaiDien = null,
                    TinhTrang = nv.TinhTrang,
                    IsActive = nv.IsActive ?? false,
                    MaChucVu = nv.MaChucVu,
                    TenChucVu = nv.MaChucVuNavigation.TenChucVu
                })
                .FirstOrDefaultAsync();
            return staff;
        }

        public async Task<ValidationResult> AddStaffAsync(StaffDto staffDto)
        {
            // Áp dụng Trim() cho các trường
            staffDto.CCCD = staffDto.CCCD?.Trim();
            staffDto.Email = staffDto.Email?.Trim();
            staffDto.SDT = staffDto.SDT?.Trim();
            staffDto.TenTaiKhoan = staffDto.TenTaiKhoan?.Trim();
            staffDto.MatKhau = staffDto.MatKhau?.Trim();

            // Validate bắt buộc các trường
            if (string.IsNullOrEmpty(staffDto.HoTen))
                return new ValidationResult(false, "Họ tên không được để trống");
            if (string.IsNullOrEmpty(staffDto.GioiTinh))
                return new ValidationResult(false, "Giới tính không được để trống");
            if (string.IsNullOrEmpty(staffDto.Email))
                return new ValidationResult(false, "Email không được để trống");
            if (string.IsNullOrEmpty(staffDto.TenTaiKhoan))
                return new ValidationResult(false, "Tên tài khoản không được để trống");
            if (string.IsNullOrEmpty(staffDto.MatKhau))
                return new ValidationResult(false, "Mật khẩu không được để trống");
            if (string.IsNullOrEmpty(staffDto.CCCD))
                return new ValidationResult(false, "CCCD không được để trống");
            if (string.IsNullOrEmpty(staffDto.SDT))
                return new ValidationResult(false, "SĐT không được để trống");
            if (staffDto.HinhDaiDien == null)
                return new ValidationResult(false, "Trường HinhDaiDien là bắt buộc");
            if (staffDto.MaChucVu <= 0)
                return new ValidationResult(false, "Mã chức vụ không hợp lệ");

            // Validate tuổi (không dưới 18 tuổi)
            int age = DateTime.Now.Year - staffDto.NgaySinh.Year;

            // Kiểm tra xem ngày sinh nhật trong năm nay đã qua chưa
            if (DateTime.Now.Month < staffDto.NgaySinh.Month ||
                (DateTime.Now.Month == staffDto.NgaySinh.Month && DateTime.Now.Day < staffDto.NgaySinh.Day))
            {
                age--; // Giảm 1 tuổi nếu ngày sinh nhật trong năm nay chưa đến
            }

            // Kiểm tra tuổi >= 18
            if (age < 18)
                return new ValidationResult(false, "Nhân viên phải từ 18 tuổi trở lên");

            // Kiểm tra chức vụ có tồn tại không
            var chucVu = await _context.Chucvus.FindAsync(staffDto.MaChucVu);
            if (chucVu == null)
                return new ValidationResult(false, "Chức vụ không tồn tại");

            // Validate CCCD (12 số, bắt đầu từ 0)
            if (!Regex.IsMatch(staffDto.CCCD, @"^[0][0-9]{11}$"))
                return new ValidationResult(false, "CCCD phải là 12 số và bắt đầu bằng 0");

            // Validate SĐT (10 số)
            if (!Regex.IsMatch(staffDto.SDT, @"^[0-9]{10}$"))
                return new ValidationResult(false, "SĐT phải là 10 số");

            // Validate Email
            if (!Regex.IsMatch(staffDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return new ValidationResult(false, "Email không hợp lệ");

            // Validate trùng CCCD
            if (await IsCccdExistsAsync(staffDto.CCCD))
                return new ValidationResult(false, "CCCD đã tồn tại");

            // Validate trùng SĐT
            if (await IsSdtExistsAsync(staffDto.SDT))
                return new ValidationResult(false, "SĐT đã tồn tại");

            // Validate trùng Email
            if (await IsEmailExistsAsync(staffDto.Email))
                return new ValidationResult(false, "Email đã tồn tại");

            // Validate tên tài khoản không trùng
            if (await IsTenTaiKhoanExistsAsync(staffDto.TenTaiKhoan))
                return new ValidationResult(false, "Tên tài khoản đã tồn tại");

            // Validate mật khẩu (ít nhất 6 ký tự)
            if (staffDto.MatKhau.Length < 6)
                return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự");

            // Hash mật khẩu
            //string hashedPassword = HashPassword(staffDto.MatKhau);
            string hashedPassword = passwordHasher.HashPassword(staffDto.MatKhau);
            staffDto.MatKhau = hashedPassword;

            // Lưu hình ảnh và chỉ lưu đường dẫn tương đối
            string filePath = null;
            if (staffDto.HinhDaiDien != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "AnhNhanVien");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(staffDto.HinhDaiDien.FileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await staffDto.HinhDaiDien.CopyToAsync(stream);
                }
                filePath = $"/AnhNhanVien/{fileName}";
            }

            var staff = new Nhanvien
            {
                HoTen = staffDto.HoTen,
                GioiTinh = staffDto.GioiTinh,
                NgaySinh = DateOnly.FromDateTime(staffDto.NgaySinh),
                DiaChi = staffDto.DiaChi,
                Cccd = staffDto.CCCD,
                Sdt = staffDto.SDT,
                Email = staffDto.Email,
                NgayVaoLam = DateOnly.FromDateTime(staffDto.NgayVaoLam),
                TenTaiKhoan = staffDto.TenTaiKhoan,
                MatKhau = staffDto.MatKhau,
                HinhDaiDien = filePath,
                TinhTrang = "Đang hoạt động",
                IsActive = true,
                MaChucVu = staffDto.MaChucVu
            };

            _context.Nhanviens.Add(staff);
            await _context.SaveChangesAsync();
            return new ValidationResult(true, "Thêm nhân viên thành công");
        }

        public async Task<bool> IsTenTaiKhoanExistsAsync(string tenTaiKhoan)
        {
            return await _context.Nhanviens.AnyAsync(nv => nv.TenTaiKhoan == tenTaiKhoan);
        }

        public async Task<bool> IsCccdExistsAsync(string cccd)
        {
            return await _context.Nhanviens.AnyAsync(nv => nv.Cccd == cccd);
        }

        public async Task<bool> IsSdtExistsAsync(string sdt)
        {
            return await _context.Nhanviens.AnyAsync(nv => nv.Sdt == sdt);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Nhanviens.AnyAsync(nv => nv.Email == email);
        }

        public async Task<List<StaffDto>> SearchStaffAsync(int pageSize, int pageNumber, string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null)
        {
            var query = _context.Nhanviens
                .Include(nv => nv.MaChucVuNavigation)
                .Where(nv => nv.IsActive == true)
                .AsQueryable();

            if (!string.IsNullOrEmpty(hoTen))
                query = query.Where(nv => nv.HoTen.Contains(hoTen));
            if (!string.IsNullOrEmpty(sdt))
                query = query.Where(nv => nv.Sdt.Contains(sdt));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(nv => nv.Email.Contains(email));
            if (!string.IsNullOrEmpty(cccd))
                query = query.Where(nv => nv.Cccd.Contains(cccd));
            if (!string.IsNullOrEmpty(diaChi))
                query = query.Where(nv => nv.DiaChi.Contains(diaChi));

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(nv => new StaffDto
                {
                    MaNV = nv.MaNv,
                    HoTen = nv.HoTen,
                    GioiTinh = nv.GioiTinh,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                    DiaChi = nv.DiaChi,
                    CCCD = nv.Cccd,
                    SDT = nv.Sdt,
                    Email = nv.Email,
                    NgayVaoLam = nv.NgayVaoLam.ToDateTime(new TimeOnly(0, 0)),
                    TenTaiKhoan = nv.TenTaiKhoan,
                    MatKhau = nv.MatKhau,
                    Hinh = nv.HinhDaiDien,
                    HinhDaiDien = null,
                    TinhTrang = nv.TinhTrang,
                    IsActive = nv.IsActive ?? false,
                    MaChucVu = nv.MaChucVu,
                    TenChucVu = nv.MaChucVuNavigation.TenChucVu
                })
                .ToListAsync();
        }

        public async Task<ValidationResult> UpdateStaffAsync(int maNV, StaffDto staffDto)
        {
            // Clean input data
            staffDto.CCCD = staffDto.CCCD?.Trim();
            staffDto.Email = staffDto.Email?.Trim();
            staffDto.SDT = staffDto.SDT?.Trim();
            staffDto.TenTaiKhoan = staffDto.TenTaiKhoan?.Trim();
            // DON'T trim password here - we need to check if it's provided for update

            // Find existing staff
            var existingStaff = await _context.Nhanviens.FindAsync(maNV);
            if (existingStaff == null)
                return new ValidationResult(false, "Nhân viên không tồn tại");

            // Required field validation
            if (!string.IsNullOrEmpty(staffDto.HoTen) && string.IsNullOrWhiteSpace(staffDto.HoTen))
                return new ValidationResult(false, "Họ tên không được để trống");
            if (!string.IsNullOrEmpty(staffDto.Email) && string.IsNullOrWhiteSpace(staffDto.Email))
                return new ValidationResult(false, "Email không được để trống");
            if (!string.IsNullOrEmpty(staffDto.CCCD) && string.IsNullOrWhiteSpace(staffDto.CCCD))
                return new ValidationResult(false, "CCCD không được để trống");
            if (!string.IsNullOrEmpty(staffDto.SDT) && string.IsNullOrWhiteSpace(staffDto.SDT))
                return new ValidationResult(false, "SĐT không được để trống");

            // Format validation
            if (!string.IsNullOrEmpty(staffDto.CCCD) && !Regex.IsMatch(staffDto.CCCD, @"^[0][0-9]{11}$"))
                return new ValidationResult(false, "CCCD phải là 12 số và bắt đầu bằng 0");
            if (!string.IsNullOrEmpty(staffDto.SDT) && !Regex.IsMatch(staffDto.SDT, @"^[0-9]{10}$"))
                return new ValidationResult(false, "SĐT phải là 10 số");
            if (!string.IsNullOrEmpty(staffDto.Email) && !Regex.IsMatch(staffDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return new ValidationResult(false, "Email không hợp lệ");

            // Password validation - ONLY if password is provided AND not empty
            if (!string.IsNullOrEmpty(staffDto.MatKhau?.Trim()) && staffDto.MatKhau.Trim().Length < 6)
                return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự");

            // Age validation - FIXED: Use proper date comparison
            if (staffDto.NgaySinh != DateTime.MinValue)
            {
                // Parse date properly to avoid timezone issues
                var birthDate = DateTime.SpecifyKind(staffDto.NgaySinh.Date, DateTimeKind.Local);
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                if (birthDate.AddYears(age) > today)
                {
                    age--;
                }
                if (age < 18)
                    return new ValidationResult(false, "Nhân viên phải từ 18 tuổi trở lên");
            }

            // Check if position exists
            if (staffDto.MaChucVu > 0 && staffDto.MaChucVu != existingStaff.MaChucVu)
            {
                var chucVu = await _context.Chucvus.FindAsync(staffDto.MaChucVu);
                if (chucVu == null)
                    return new ValidationResult(false, "Chức vụ không tồn tại");
            }

            // Check for duplicates - Fixed logic
            if (!string.IsNullOrEmpty(staffDto.CCCD) &&
                !string.Equals(staffDto.CCCD.Trim(), existingStaff.Cccd?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV &&
                    k.Cccd.Trim().ToLower() == staffDto.CCCD.Trim().ToLower()))
                    return new ValidationResult(false, "CCCD đã tồn tại");
            }

            if (!string.IsNullOrEmpty(staffDto.SDT) &&
                !string.Equals(staffDto.SDT.Trim(), existingStaff.Sdt?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV &&
                    k.Sdt.Trim().ToLower() == staffDto.SDT.Trim().ToLower()))
                    return new ValidationResult(false, "SĐT đã tồn tại");
            }

            if (!string.IsNullOrEmpty(staffDto.Email) &&
                !string.Equals(staffDto.Email.Trim(), existingStaff.Email?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV &&
                    k.Email.Trim().ToLower() == staffDto.Email.Trim().ToLower()))
                    return new ValidationResult(false, "Email đã tồn tại");
            }

            // Update fields - only update changed fields
            if (!string.IsNullOrEmpty(staffDto.HoTen))
                existingStaff.HoTen = staffDto.HoTen;
            if (!string.IsNullOrEmpty(staffDto.GioiTinh))
                existingStaff.GioiTinh = staffDto.GioiTinh;

            // FIXED: Date handling - only update if actually changed and not MinValue
            if (staffDto.NgaySinh != DateTime.MinValue)
            {
                var newBirthDate = DateOnly.FromDateTime(DateTime.SpecifyKind(staffDto.NgaySinh.Date, DateTimeKind.Local));
                if (existingStaff.NgaySinh != newBirthDate)
                    existingStaff.NgaySinh = newBirthDate;
            }

            if (!string.IsNullOrEmpty(staffDto.DiaChi))
                existingStaff.DiaChi = staffDto.DiaChi;
            if (!string.IsNullOrEmpty(staffDto.CCCD))
                existingStaff.Cccd = staffDto.CCCD;
            if (!string.IsNullOrEmpty(staffDto.SDT))
                existingStaff.Sdt = staffDto.SDT;
            if (!string.IsNullOrEmpty(staffDto.Email))
                existingStaff.Email = staffDto.Email;

            // FIXED: Date handling for NgayVaoLam
            if (staffDto.NgayVaoLam != DateTime.MinValue)
            {
                var newJoinDate = DateOnly.FromDateTime(DateTime.SpecifyKind(staffDto.NgayVaoLam.Date, DateTimeKind.Local));
                if (existingStaff.NgayVaoLam != newJoinDate)
                    existingStaff.NgayVaoLam = newJoinDate;
            }

            // FIXED: Password update - ONLY hash if new password is provided and not empty
            if (!string.IsNullOrEmpty(staffDto.MatKhau?.Trim()))
            {
                // Only hash new password when it's actually provided (not empty string)
                var newPassword = staffDto.MatKhau.Trim();
                existingStaff.MatKhau = passwordHasher.HashPassword(newPassword);
            }
            // If MatKhau is null or empty, don't change the existing password

            if (!string.IsNullOrEmpty(staffDto.TinhTrang))
                existingStaff.TinhTrang = staffDto.TinhTrang;
            if (staffDto.MaChucVu > 0)
                existingStaff.MaChucVu = staffDto.MaChucVu;
            if (staffDto.IsActive.HasValue)
                existingStaff.IsActive = staffDto.IsActive.Value;

            // Handle image update
            if (staffDto.HinhDaiDien != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingStaff.HinhDaiDien))
                {
                    string oldImagePath = existingStaff.HinhDaiDien.Replace("/", "");
                    string fullOldPath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath);
                    if (System.IO.File.Exists(fullOldPath))
                    {
                        try
                        {
                            System.IO.File.Delete(fullOldPath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi xóa hình ảnh cũ: {ex.Message}");
                        }
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "AnhNhanVien");
                Directory.CreateDirectory(uploadsFolder);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(staffDto.HinhDaiDien.FileName);
                string fullPath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await staffDto.HinhDaiDien.CopyToAsync(stream);
                }
                existingStaff.HinhDaiDien = $"/AnhNhanVien/{fileName}";
            }

            _context.Nhanviens.Update(existingStaff);
            await _context.SaveChangesAsync();
            return new ValidationResult(true, "Cập nhật nhân viên thành công");
        }
        public async Task<ValidationResult> DeleteStaffAsync(int maNV)
        {
            var staff = await _context.Nhanviens.FindAsync(maNV);
            if (staff == null)
                return new ValidationResult(false, "Nhân viên không tồn tại");

            // Perform soft delete
            staff.IsActive = false;
            staff.TinhTrang = "Đã Tạm Khóa";

            _context.Nhanviens.Update(staff);
            await _context.SaveChangesAsync();
            return new ValidationResult(true, "Xóa nhân viên thành công");
        }

        //private string HashPassword(string password)
        //{
        //    using (var sha256 = SHA256.Create())
        //    {
        //        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        //        return Convert.ToBase64String(hashedBytes);
        //    }
        //}

        public async Task<List<StaffExportDto>> GetStaffForExportAsync()
        {
            return await _context.Nhanviens
                .Include(nv => nv.MaChucVuNavigation)
                .Where(nv => nv.IsActive != null)
                .Select(nv => new StaffExportDto
                {
                    HoTen = nv.HoTen,
                    GioiTinh = nv.GioiTinh,
                    NgaySinh = nv.NgaySinh.HasValue ? nv.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                    DiaChi = nv.DiaChi,
                    SDT = nv.Sdt,
                    Email = nv.Email,
                    NgayVaoLam = nv.NgayVaoLam.ToDateTime(new TimeOnly(0, 0)),
                    TinhTrang = nv.TinhTrang,
                    TenChucVu = nv.MaChucVuNavigation.TenChucVu
                })
                .ToListAsync();
        }
        public async Task<List<Chucvu>> GetAllChucvusAsync()
        {
            return await _context.Chucvus.ToListAsync();
        }
    }
}