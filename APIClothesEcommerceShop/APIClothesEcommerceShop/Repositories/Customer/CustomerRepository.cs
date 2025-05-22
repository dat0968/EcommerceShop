using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Customer;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Customer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class CustomerRepository : ICustomerRepository
{
    private readonly EcommerceShopContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CustomerRepository(EcommerceShopContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<List<CustomerDto>> GetAllCustomersAsync(int pageSize, int pageNumber, string hoTen, string gioiTinh, string tinhTrang)
    {
        var query = _context.Khachhangs.Where(kh => kh.IsActive == true).AsQueryable();

        if (!string.IsNullOrEmpty(hoTen))
            query = query.Where(kh => kh.HoTen.Contains(hoTen));
        if (!string.IsNullOrEmpty(gioiTinh))
            query = query.Where(kh => kh.GioiTinh == gioiTinh);
        if (!string.IsNullOrEmpty(tinhTrang))
            query = query.Where(kh => kh.TinhTrang == tinhTrang);

        // Sắp xếp theo NgayTao giảm dần
        query = query.OrderByDescending(kh => kh.NgayTao);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(kh => new CustomerDto
            {
                MaKH = kh.MaKh,
                HoTen = kh.HoTen,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh.HasValue ? kh.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                DiaChi = kh.DiaChi,
                CCCD = kh.Cccd,
                SDT = kh.Sdt,
                Email = kh.Email,
                TenTaiKhoan = kh.TenTaiKhoan,
                MatKhau = kh.MatKhau,
                Hinh = kh.HinhDaiDien,
                HinhDaiDien = null,
                TinhTrang = kh.TinhTrang,
                IsActive = kh.IsActive ?? false,
                NgayTao = kh.NgayTao,
            })
            .ToListAsync();
    }
    public async Task<int> GetCustomersCountAsync(string hoTen = null, string gioiTinh = null, string tinhTrang = null)
    {
        var query = _context.Khachhangs.Where(kh => kh.IsActive == true).AsQueryable();

        if (!string.IsNullOrEmpty(hoTen))
            query = query.Where(kh => kh.HoTen.Contains(hoTen));
        if (!string.IsNullOrEmpty(gioiTinh))
            query = query.Where(kh => kh.GioiTinh == gioiTinh);
        if (!string.IsNullOrEmpty(tinhTrang))
            query = query.Where(kh => kh.TinhTrang == tinhTrang);

        return await query.CountAsync();
    }
    public async Task<int> GetSearchCountAsync(string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null)
    {
        var query = _context.Khachhangs.AsQueryable();

        if (!string.IsNullOrEmpty(hoTen))
            query = query.Where(kh => kh.HoTen.Contains(hoTen));
        if (!string.IsNullOrEmpty(sdt))
            query = query.Where(kh => kh.Sdt.Contains(sdt));
        if (!string.IsNullOrEmpty(email))
            query = query.Where(kh => kh.Email.Contains(email));
        if (!string.IsNullOrEmpty(cccd))
            query = query.Where(kh => kh.Cccd.Contains(cccd));
        if (!string.IsNullOrEmpty(diaChi))
            query = query.Where(kh => kh.DiaChi.Contains(diaChi));

        return await query.CountAsync();
    }
    public async Task<CustomerDto> GetCustomerByIdAsync(int maKH)
    {
        var customer = await _context.Khachhangs
            .Where(kh => kh.MaKh == maKH)
            .Select(kh => new CustomerDto
            {
                MaKH = kh.MaKh,
                HoTen = kh.HoTen,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh.HasValue ? kh.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                DiaChi = kh.DiaChi,
                CCCD = kh.Cccd,
                SDT = kh.Sdt,
                Email = kh.Email,
                TenTaiKhoan = kh.TenTaiKhoan,
                MatKhau = kh.MatKhau,
                Hinh = kh.HinhDaiDien, 
                HinhDaiDien = null,
                TinhTrang = kh.TinhTrang,
                IsActive = kh.IsActive ?? false
            })
            .FirstOrDefaultAsync();
        return customer;
    }

    public async Task<ValidationResult> AddCustomerAsync(CustomerDto customerDto)
    {
        // Áp dụng Trim() cho các trường
        customerDto.CCCD = customerDto.CCCD?.Trim();
        customerDto.Email = customerDto.Email?.Trim();
        customerDto.SDT = customerDto.SDT?.Trim();
        customerDto.TenTaiKhoan = customerDto.TenTaiKhoan?.Trim();
        customerDto.MatKhau = customerDto.MatKhau?.Trim();

        // Validate bắt buộc các trường
        if (string.IsNullOrEmpty(customerDto.HoTen))
            return new ValidationResult(false, "Họ tên không được để trống");
        if (string.IsNullOrEmpty(customerDto.Email))
            return new ValidationResult(false, "Email không được để trống");
        if (string.IsNullOrEmpty(customerDto.TenTaiKhoan))
            return new ValidationResult(false, "Tên tài khoản không được để trống");
        if (string.IsNullOrEmpty(customerDto.MatKhau))
            return new ValidationResult(false, "Mật khẩu không được để trống");
        if (string.IsNullOrEmpty(customerDto.CCCD))
            return new ValidationResult(false, "CCCD không được để trống");
        if (string.IsNullOrEmpty(customerDto.SDT))
            return new ValidationResult(false, "SĐT không được để trống");
        if (customerDto.HinhDaiDien == null)
            return new ValidationResult(false, "Trường HinhDaiDien là bắt buộc");

        // Validate tuổi (không dưới 10 tuổi)
        int age = DateTime.Now.Year - customerDto.NgaySinh.Year;

        // Kiểm tra xem ngày sinh nhật trong năm nay đã qua chưa
        if (DateTime.Now.Month < customerDto.NgaySinh.Month ||
            (DateTime.Now.Month == customerDto.NgaySinh.Month && DateTime.Now.Day < customerDto.NgaySinh.Day))
        {
            age--; // Giảm 1 tuổi nếu ngày sinh nhật trong năm nay chưa đến
        }

        // Kiểm tra tuổi >= 10
        if (age < 10)
            return new ValidationResult(false, "Khách hàng phải từ 10 tuổi trở lên");

        // Validate CCCD (12 số, bắt đầu từ 0)
        if (!Regex.IsMatch(customerDto.CCCD, @"^[0][0-9]{11}$"))
            return new ValidationResult(false, "CCCD phải là 12 số và bắt đầu bằng 0");

        // Validate SĐT (10 số)
        if (!Regex.IsMatch(customerDto.SDT, @"^[0-9]{10}$"))
            return new ValidationResult(false, "SĐT phải là 10 số");

        // Validate Email
        if (!Regex.IsMatch(customerDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return new ValidationResult(false, "Email không hợp lệ");

        // Validate trùng CCCD
        if (await IsCccdExistsAsync(customerDto.CCCD))
            return new ValidationResult(false, "CCCD đã tồn tại");

        // Validate trùng SĐT
        if (await IsSdtExistsAsync(customerDto.SDT))
            return new ValidationResult(false, "SĐT đã tồn tại");

        // Validate trùng Email
        if (await IsEmailExistsAsync(customerDto.Email))
            return new ValidationResult(false, "Email đã tồn tại");

        // Validate tên tài khoản không trùng
        if (await IsTenTaiKhoanExistsAsync(customerDto.TenTaiKhoan))
            return new ValidationResult(false, "Tên tài khoản đã tồn tại");

        // Validate mật khẩu (ít nhất 6 ký tự)
        if (customerDto.MatKhau.Length < 6)
            return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự");

        // Hash mật khẩu
        string hashedPassword = HashPassword(customerDto.MatKhau);
        customerDto.MatKhau = hashedPassword;

        // Lưu hình ảnh và chỉ lưu đường dẫn tương đối
        string filePath = null;
        if (customerDto.HinhDaiDien != null)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "AnhKhachHang");
            Directory.CreateDirectory(uploadsFolder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(customerDto.HinhDaiDien.FileName);
            string fullPath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await customerDto.HinhDaiDien.CopyToAsync(stream);
            }
            filePath = $"/wwwroot/AnhKhachHang/{fileName}";
        }

        var customer = new Khachhang
        {
            HoTen = customerDto.HoTen,
            GioiTinh = customerDto.GioiTinh,
            NgaySinh = DateOnly.FromDateTime(customerDto.NgaySinh),
            DiaChi = customerDto.DiaChi,
            Cccd = customerDto.CCCD,
            Sdt = customerDto.SDT,
            Email = customerDto.Email,
            TenTaiKhoan = customerDto.TenTaiKhoan,
            MatKhau = customerDto.MatKhau,
            HinhDaiDien = filePath,
            NgayTao = DateTime.Now,
            TinhTrang = "Đang hoạt động",
            IsActive = true
        };

        _context.Khachhangs.Add(customer);
        await _context.SaveChangesAsync();
        return new ValidationResult(true, "Thêm khách hàng thành công");
    }

    public async Task<bool> IsTenTaiKhoanExistsAsync(string tenTaiKhoan)
    {
        return await _context.Khachhangs.AnyAsync(kh => kh.TenTaiKhoan == tenTaiKhoan);
    }

    public async Task<bool> IsCccdExistsAsync(string cccd)
    {
        return await _context.Khachhangs.AnyAsync(kh => kh.Cccd == cccd);
    }

    public async Task<bool> IsSdtExistsAsync(string sdt)
    {
        return await _context.Khachhangs.AnyAsync(kh => kh.Sdt == sdt);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Khachhangs.AnyAsync(kh => kh.Email == email);
    }

    public async Task<List<CustomerDto>> SearchCustomersAsync(int pageSize, int pageNumber, string hoTen = null, string sdt = null, string email = null, string cccd = null, string diaChi = null)
    {
        var query = _context.Khachhangs.Where(kh => kh.IsActive == true).AsQueryable();

        if (!string.IsNullOrEmpty(hoTen))
            query = query.Where(kh => kh.HoTen.Contains(hoTen));
        if (!string.IsNullOrEmpty(sdt))
            query = query.Where(kh => kh.Sdt.Contains(sdt));
        if (!string.IsNullOrEmpty(email))
            query = query.Where(kh => kh.Email.Contains(email));
        if (!string.IsNullOrEmpty(cccd))
            query = query.Where(kh => kh.Cccd.Contains(cccd));
        if (!string.IsNullOrEmpty(diaChi))
            query = query.Where(kh => kh.DiaChi.Contains(diaChi));

        // Sắp xếp theo NgayTao giảm dần
        query = query.OrderByDescending(kh => kh.NgayTao);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(kh => new CustomerDto
            {
                MaKH = kh.MaKh,
                HoTen = kh.HoTen,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh.HasValue ? kh.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                DiaChi = kh.DiaChi,
                CCCD = kh.Cccd,
                SDT = kh.Sdt,
                Email = kh.Email,
                TenTaiKhoan = kh.TenTaiKhoan,
                MatKhau = kh.MatKhau,
                Hinh = kh.HinhDaiDien,
                HinhDaiDien = null,
                TinhTrang = kh.TinhTrang,
                IsActive = kh.IsActive ?? false
            })
            .ToListAsync();
    }

    public async Task<ValidationResult> UpdateCustomerAsync(int maKH, CustomerDto customerDto)
    {
        // Làm sạch dữ liệu đầu vào
        customerDto.CCCD = customerDto.CCCD?.Trim();
        customerDto.Email = customerDto.Email?.Trim();
        customerDto.SDT = customerDto.SDT?.Trim();
        customerDto.TenTaiKhoan = customerDto.TenTaiKhoan?.Trim();
        customerDto.MatKhau = customerDto.MatKhau?.Trim();

        // Tìm khách hàng hiện tại
        var existingCustomer = await _context.Khachhangs.FindAsync(maKH);
        if (existingCustomer == null)
            return new ValidationResult(false, "Khách hàng không tồn tại");

        // Kiểm tra tên tài khoản
        if (customerDto.TenTaiKhoan != null && customerDto.TenTaiKhoan != existingCustomer.TenTaiKhoan)
            return new ValidationResult(false, "Không được phép sửa tên tài khoản");

        // Kiểm tra các trường bắt buộc
        if (!string.IsNullOrEmpty(customerDto.HoTen) && string.IsNullOrWhiteSpace(customerDto.HoTen))
            return new ValidationResult(false, "Họ tên không được để trống");

        if (!string.IsNullOrEmpty(customerDto.Email) && string.IsNullOrWhiteSpace(customerDto.Email))
            return new ValidationResult(false, "Email không được để trống");

        if (!string.IsNullOrEmpty(customerDto.CCCD) && string.IsNullOrWhiteSpace(customerDto.CCCD))
            return new ValidationResult(false, "CCCD không được để trống");

        if (!string.IsNullOrEmpty(customerDto.SDT) && string.IsNullOrWhiteSpace(customerDto.SDT))
            return new ValidationResult(false, "SĐT không được để trống");

        // Kiểm tra định dạng
        if (!string.IsNullOrEmpty(customerDto.CCCD) && !Regex.IsMatch(customerDto.CCCD, @"^[0][0-9]{11}$"))
            return new ValidationResult(false, "CCCD phải là 12 số và bắt đầu bằng 0");

        if (!string.IsNullOrEmpty(customerDto.SDT) && !Regex.IsMatch(customerDto.SDT, @"^[0-9]{10}$"))
            return new ValidationResult(false, "SĐT phải là 10 số");

        if (!string.IsNullOrEmpty(customerDto.Email) && !Regex.IsMatch(customerDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return new ValidationResult(false, "Email không hợp lệ");

        if (!string.IsNullOrEmpty(customerDto.MatKhau) && customerDto.MatKhau.Length < 6)
            return new ValidationResult(false, "Mật khẩu phải có ít nhất 6 ký tự");

        // Kiểm tra tuổi
        if (customerDto.NgaySinh != DateTime.MinValue)
        {
            var today = DateTime.Today;
            var birthDate = customerDto.NgaySinh.Date;
            var age = today.Year - birthDate.Year;

            if (birthDate.AddYears(age) > today)
            {
                age--;
            }

            if (age < 10)
                return new ValidationResult(false, "Khách hàng phải từ 10 tuổi trở lên");
        }

        // Kiểm tra trùng CCCD, SĐT, Email CHỈ KHI có thay đổi
        if (!string.IsNullOrEmpty(customerDto.CCCD) &&
            !string.Equals(customerDto.CCCD, existingCustomer.Cccd, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Khachhangs.AnyAsync(k => k.MaKh != maKH && k.Cccd == customerDto.CCCD))
                return new ValidationResult(false, "CCCD đã tồn tại");
        }

        if (!string.IsNullOrEmpty(customerDto.SDT) &&
            !string.Equals(customerDto.SDT, existingCustomer.Sdt, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Khachhangs.AnyAsync(k => k.MaKh != maKH && k.Sdt == customerDto.SDT))
                return new ValidationResult(false, "SĐT đã tồn tại");
        }

        if (!string.IsNullOrEmpty(customerDto.Email) &&
            !string.Equals(customerDto.Email, existingCustomer.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Khachhangs.AnyAsync(k => k.MaKh != maKH && k.Email == customerDto.Email))
                return new ValidationResult(false, "Email đã tồn tại");
        }

        // Cập nhật thông tin - chỉ cập nhật các trường đã được thay đổi
        existingCustomer.HoTen = string.IsNullOrEmpty(customerDto.HoTen) ? existingCustomer.HoTen : customerDto.HoTen;
        existingCustomer.GioiTinh = string.IsNullOrEmpty(customerDto.GioiTinh) ? existingCustomer.GioiTinh : customerDto.GioiTinh;

        if (customerDto.NgaySinh != DateTime.MinValue)
            existingCustomer.NgaySinh = DateOnly.FromDateTime(customerDto.NgaySinh);

        existingCustomer.DiaChi = string.IsNullOrEmpty(customerDto.DiaChi) ? existingCustomer.DiaChi : customerDto.DiaChi;
        existingCustomer.Cccd = string.IsNullOrEmpty(customerDto.CCCD) ? existingCustomer.Cccd : customerDto.CCCD;
        existingCustomer.Sdt = string.IsNullOrEmpty(customerDto.SDT) ? existingCustomer.Sdt : customerDto.SDT;
        existingCustomer.Email = string.IsNullOrEmpty(customerDto.Email) ? existingCustomer.Email : customerDto.Email;

        if (!string.IsNullOrEmpty(customerDto.MatKhau))
            existingCustomer.MatKhau = HashPassword(customerDto.MatKhau);

        existingCustomer.TinhTrang = string.IsNullOrEmpty(customerDto.TinhTrang) ? existingCustomer.TinhTrang : customerDto.TinhTrang;

        if (customerDto.IsActive != existingCustomer.IsActive)
            existingCustomer.IsActive = customerDto.IsActive;

        existingCustomer.NgayTao = DateTime.Now; 

        // Xử lý hình ảnh
        if (customerDto.HinhDaiDien != null)
        {
            if (!string.IsNullOrEmpty(existingCustomer.HinhDaiDien))
            {
                string oldImagePath = existingCustomer.HinhDaiDien.Replace("/wwwroot", "");
                string fullOldPath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));

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

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "AnhKhachHang");
            Directory.CreateDirectory(uploadsFolder);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(customerDto.HinhDaiDien.FileName);
            string fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await customerDto.HinhDaiDien.CopyToAsync(stream);
            }

            existingCustomer.HinhDaiDien = $"/AnhKhachHang/{fileName}";
        }

        _context.Khachhangs.Update(existingCustomer);
        await _context.SaveChangesAsync();
        return new ValidationResult(true, "Cập nhật khách hàng thành công");
    }
    public async Task<ValidationResult> DeleteCustomerAsync(int maKH)
    {
        var customer = await _context.Khachhangs.FindAsync(maKH);
        if (customer == null)
            return new ValidationResult(false, "Khách hàng không tồn tại");

        // Perform soft delete
        customer.IsActive = false; 
        customer.TenTaiKhoan = null; 
        customer.TinhTrang = "Đã tạm khóa";

        _context.Khachhangs.Update(customer);
        await _context.SaveChangesAsync();
        return new ValidationResult(true, "Xóa khách hàng thành công");
    }
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    public async Task<List<CustomerExportDto>> GetCustomersForExportAsync()
    {
        return await _context.Khachhangs
            .Where(kh => kh.IsActive != null)
            .Select(kh => new CustomerExportDto
            {
                HoTen = kh.HoTen,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh.HasValue ? kh.NgaySinh.Value.ToDateTime(new TimeOnly(0, 0)) : DateTime.MinValue,
                DiaChi = kh.DiaChi,
                SDT = kh.Sdt,
                Email = kh.Email,
                NgayTao = kh.NgayTao,
                TinhTrang = kh.TinhTrang
            })
            .ToListAsync();
    }
}