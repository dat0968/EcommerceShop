using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Staff;
using APIClothesEcommerceShop.Models;
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

        public StaffRepository(EcommerceShopContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
            string hashedPassword = HashPassword(staffDto.MatKhau);
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
            // Làm sạch dữ liệu đầu vào
            staffDto.CCCD = staffDto.CCCD?.Trim();
            staffDto.Email = staffDto.Email?.Trim();
            staffDto.SDT = staffDto.SDT?.Trim();
            staffDto.MatKhau = staffDto.MatKhau?.Trim();
            // Không xử lý tenTaiKhoan vì không cho phép cập nhật

            // Tìm nhân viên hiện tại
            var existingStaff = await _context.Nhanviens.FindAsync(maNV);
            if (existingStaff == null)
                return new ValidationResult(false, "Nhân viên không tồn tại");

            // Kiểm tra các trường bắt buộc
            if (!string.IsNullOrEmpty(staffDto.HoTen) && string.IsNullOrWhiteSpace(staffDto.HoTen))
                return new ValidationResult(false, "Họ tên không được để trống");
            if (!string.IsNullOrEmpty(staffDto.Email) && string.IsNullOrWhiteSpace(staffDto.Email))
                return new ValidationResult(false, "Email không được để trống");
            if (!string.IsNullOrEmpty(staffDto.CCCD) && string.IsNullOrWhiteSpace(staffDto.CCCD))
                return new ValidationResult(false, "CCCD không được để trống");
            if (!string.IsNullOrEmpty(staffDto.SDT) && string.IsNullOrWhiteSpace(staffDto.SDT))
                return new ValidationResult(false, "SĐT không được để trống");

            // Kiểm tra định dạng
            if (!string.IsNullOrEmpty(staffDto.CCCD) && !Regex.IsMatch(staffDto.CCCD, @"^[0][0-9]{11}$"))
                return new ValidationResult(false, "CCCD phải là 12 số và bắt đầu bằng 0");
            if (!string.IsNullOrEmpty(staffDto.SDT) && !Regex.IsMatch(staffDto.SDT, @"^[0-9]{10}$"))
                return new ValidationResult(false, "SĐT phải là 10 số");
            if (!string.IsNullOrEmpty(staffDto.Email) && !Regex.IsMatch(staffDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return new ValidationResult(false, "Email không hợp lệ");
            if (!string.IsNullOrEmpty(staffDto.MatKhau) && staffDto.MatKhau.Length < 6)
                return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự");

            // Kiểm tra tuổi
            if (staffDto.NgaySinh != DateTime.MinValue)
            {
                var today = DateTime.Today;
                var birthDate = staffDto.NgaySinh.Date;
                var age = today.Year - birthDate.Year;
                if (birthDate.AddYears(age) > today)
                {
                    age--;
                }
                if (age < 18)
                    return new ValidationResult(false, "Nhân viên phải từ 18 tuổi trở lên");
            }

            // Kiểm tra chức vụ có tồn tại không
            if (staffDto.MaChucVu > 0 && staffDto.MaChucVu != existingStaff.MaChucVu)
            {
                var chucVu = await _context.Chucvus.FindAsync(staffDto.MaChucVu);
                if (chucVu == null)
                    return new ValidationResult(false, "Chức vụ không tồn tại");
            }

            // Kiểm tra trùng CCCD, SĐT, Email CHỈ KHI có thay đổi
            if (!string.IsNullOrEmpty(staffDto.CCCD) &&
                !string.Equals(staffDto.CCCD, existingStaff.Cccd, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV && k.Cccd == staffDto.CCCD))
                    return new ValidationResult(false, "CCCD đã tồn tại");
            }
            if (!string.IsNullOrEmpty(staffDto.SDT) &&
                !string.Equals(staffDto.SDT, existingStaff.Sdt, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV && k.Sdt == staffDto.SDT))
                    return new ValidationResult(false, "SĐT đã tồn tại");
            }
            if (!string.IsNullOrEmpty(staffDto.Email) &&
                !string.Equals(staffDto.Email, existingStaff.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Nhanviens.AnyAsync(k => k.MaNv != maNV && k.Email == staffDto.Email))
                    return new ValidationResult(false, "Email đã tồn tại");
            }

            // Cập nhật thông tin - chỉ cập nhật các trường đã được thay đổi
            existingStaff.HoTen = string.IsNullOrEmpty(staffDto.HoTen) ? existingStaff.HoTen : staffDto.HoTen;
            existingStaff.GioiTinh = string.IsNullOrEmpty(staffDto.GioiTinh) ? existingStaff.GioiTinh : staffDto.GioiTinh;
            if (staffDto.NgaySinh != DateTime.MinValue)
                existingStaff.NgaySinh = DateOnly.FromDateTime(staffDto.NgaySinh);
            existingStaff.DiaChi = string.IsNullOrEmpty(staffDto.DiaChi) ? existingStaff.DiaChi : staffDto.DiaChi;
            existingStaff.Cccd = string.IsNullOrEmpty(staffDto.CCCD) ? existingStaff.Cccd : staffDto.CCCD;
            existingStaff.Sdt = string.IsNullOrEmpty(staffDto.SDT) ? existingStaff.Sdt : staffDto.SDT;
            existingStaff.Email = string.IsNullOrEmpty(staffDto.Email) ? existingStaff.Email : staffDto.Email;
            if (staffDto.NgayVaoLam != DateTime.MinValue)
                existingStaff.NgayVaoLam = DateOnly.FromDateTime(staffDto.NgayVaoLam);
            if (!string.IsNullOrEmpty(staffDto.MatKhau))
                existingStaff.MatKhau = HashPassword(staffDto.MatKhau);
            existingStaff.TinhTrang = string.IsNullOrEmpty(staffDto.TinhTrang) ? existingStaff.TinhTrang : staffDto.TinhTrang;
            if (staffDto.MaChucVu > 0)
                existingStaff.MaChucVu = staffDto.MaChucVu;
            if (staffDto.IsActive.HasValue)
                existingStaff.IsActive = staffDto.IsActive.Value;


            // Xử lý hình ảnh
            if (staffDto.HinhDaiDien != null)
            {
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

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