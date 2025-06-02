using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly EcommerceShopContext _db;
        public DbInitializer(EcommerceShopContext db)
        {
            _db = db;
        }

        public void InitializeDb()
        {
            // _db.Database.EnsureCreated();
            InitTestAccount();
        }
        private void InitCombo(int numCreate, int? idOrder = null)
        {
            var sanphams = _db.Sanphams.ToList();
            var chitietsanphams = _db.Chitietsanphams.ToList();
            idOrder = idOrder ?? _db.Hoadons.First().MaHd;

            var random = new Random();

            for (int i = 1; i <= numCreate; i++)
            {
                // Tạo combo mới
                var combo = new Combo
                {
                    TenCombo = $"Combo {i}",
                    Hinh = null,
                    GiaCombo = 0, // Sẽ tính sau
                    SoLuong = random.Next(5, 20),
                    MoTa = $"Mô tả cho combo {i}",
                    IsActive = true
                };

                // Chọn ngẫu nhiên 2-4 chi tiết sản phẩm cho combo này
                var ctspInCombo = chitietsanphams.OrderBy(x => random.Next()).Take(random.Next(2, 5)).ToList();

                int tongGia = 0;
                foreach (var ctsp in ctspInCombo)
                {
                    int soLuong = random.Next(1, 5);
                    int donGia = ctsp.DonGia;

                    combo.Chitietcombohoadons.Add(new Chitietcombohoadon
                    {
                        MaHd = idOrder.Value,
                        MaCtsp = ctsp.MaCtsp,
                        SoLuong = soLuong,
                        DonGia = donGia
                    });

                    tongGia += donGia * soLuong;
                }

                // Gán giá combo là tổng giá các sản phẩm giảm 10%
                combo.GiaCombo = (int)(tongGia * 0.9);

                _db.Combos.Add(combo);
            }

            _db.SaveChanges();
        }

        private void InitTestAccount()
        {
            // Kiểm tra nếu đã có tài khoản mẫu thì không tạo lại
            if (!_db.Khachhangs.Any(kh => kh.Email == "customer.demo@email.com"))
            {
                var customer = new Khachhang
                {
                    HoTen = "Khách Hàng Demo",
                    TenTaiKhoan = "customer.demo",
                    Email = "customer.demo@email.com",
                    MatKhau = new HashPassword.PasswordHasher().HashPassword("CustomerDemo@123"),
                    NgayTao = DateTime.Now,
                    IsActive = true,
                    TinhTrang = "Đang hoạt động",
                    Sdt = "0900000001",
                    DiaChi = "123 Đường Demo, Quận 1, TP.HCM",
                    Cccd = "123456789012",
                    NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                    GioiTinh = "Nam",
                    HinhDaiDien = null
                };
                _db.Khachhangs.Add(customer);
            }

            if (!_db.Nhanviens.Any(nv => nv.Email == "staff6real.demo@email.com"))
            {
                // Lấy mã chức vụ đầu tiên hoặc tạo mới nếu chưa có
                var chucVu = _db.Chucvus.Skip(1).FirstOrDefault() ?? new Chucvu { TenChucVu = "Nhân viên" };
                if (chucVu.MaChucVu == 0)
                {
                    _db.Chucvus.Add(chucVu);
                    _db.SaveChanges();
                }

                var staff = new Nhanvien
                {
                    HoTen = "Nhân Viên Demo Real",
                    TenTaiKhoan = "staff6real.demo",
                    Email = "staff6real.demo@email.com",
                    MatKhau = "staff6realDemo@123", // Nhân viên không mã hóa mật khẩu như AccountRepository
                    NgayVaoLam = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true,
                    TinhTrang = "Đang hoạt động",
                    Sdt = "0900060002",
                    DiaChi = "666 Đường Demo, Quận 6, TP.Bình Hòa",
                    Cccd = "423456689012",
                    NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-26)),
                    GioiTinh = "Nữ",
                    MaChucVu = chucVu.MaChucVu
                };
                _db.Nhanviens.Add(staff);
            }

            if (!_db.Nhanviens.Any(nv => nv.Email == "staff.demo@email.com"))
            {
                // Lấy mã chức vụ đầu tiên hoặc tạo mới nếu chưa có
                var chucVu = _db.Chucvus.FirstOrDefault() ?? new Chucvu { TenChucVu = "Nhân viên" };
                if (chucVu.MaChucVu == 0)
                {
                    _db.Chucvus.Add(chucVu);
                    _db.SaveChanges();
                }

                var staff = new Nhanvien
                {
                    HoTen = "Nhân Viên Demo",
                    TenTaiKhoan = "staff.demo",
                    Email = "staff.demo@email.com",
                    MatKhau = "StaffDemo@123", // Nhân viên không mã hóa mật khẩu như AccountRepository
                    NgayVaoLam = DateOnly.FromDateTime(DateTime.Now),
                    IsActive = true,
                    TinhTrang = "Đang hoạt động",
                    Sdt = "0900000002",
                    DiaChi = "143 Đường Demo, Quận 1, TP.HCM",
                    Cccd = "423456789012",
                    NgaySinh = DateOnly.FromDateTime(DateTime.Now.AddYears(-21)),
                    GioiTinh = "Nam",
                    MaChucVu = chucVu.MaChucVu
                };
                _db.Nhanviens.Add(staff);
            }

            _db.SaveChanges();
        }
    }
}