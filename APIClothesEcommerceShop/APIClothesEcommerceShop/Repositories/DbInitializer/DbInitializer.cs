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
            // InitTestOrder(3);
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

        private void InitTestOrder(int numCreate, string username = "customer.demo")
        {
            var khachHang = _db.Khachhangs.FirstOrDefault(kh => kh.TenTaiKhoan == username);
            if (khachHang == null) return;

            var sanphams = _db.Sanphams.ToList();
            var chitietsanphams = _db.Chitietsanphams.ToList();
            var random = new Random();

            for (int i = 1; i <= numCreate; i++)
            {
                // Tạo hóa đơn mới
                var hoadon = new Hoadon
                {
                    MaKh = khachHang.MaKh,
                    NgayTao = DateTime.Now,
                    DiaChiNhanHang = "123 Đường Demo, Quận 1, TP.HCM",
                    HoTen = khachHang.HoTen,
                    Sdt = khachHang.Sdt ?? "0900000001",
                    PhiVanChuyen = 20000,
                    TienGoc = 0, // Sẽ tính sau
                    HinhThucTt = "VnPay",
                    TinhTrang = "Chờ xử lý",
                    IsActive = true
                };

                int tongTien = 0;

                // Chọn ngẫu nhiên 1-3 sản phẩm cho hóa đơn này
                var ctspInOrder = chitietsanphams.Take(random.Next(1, 4)).ToList();
                List<Cthoadon> cthdInOrder = new();
                foreach (var ctsp in ctspInOrder)
                {
                    int soLuong = random.Next(1, 5);
                    int donGia = ctsp.DonGia;

                    Cthoadon newCtHd = (new Cthoadon
                    {
                        MaCtsp = ctsp.MaCtsp,
                        SoLuong = soLuong,
                        Gia = donGia
                    });

                    tongTien += donGia * soLuong;

                    cthdInOrder.Add(newCtHd);
                }

                var ctcbos = _db.Combos
                    .Take(random.Next(1, 3))
                    .ToList();
                List<Chitietcombohoadon> ctcboInOrders = new();
                foreach (var combo in ctcbos)
                {
                    int soLuong = random.Next(1, 3);
                    int donGia = combo.GiaCombo;

                    Chitietcombohoadon ctcbo = (new Chitietcombohoadon
                    {
                        MaCtsp = chitietsanphams.FirstOrDefault()?.MaCtsp ?? 3, // Combo không có mã chi tiết sản phẩm
                        SoLuong = soLuong,
                        DonGia = donGia,
                        MaCombo = combo.MaCombo // Gán ID của combo
                    });

                    tongTien += donGia * soLuong;
                    ctcboInOrders.Add(ctcbo);
                }

                // Gán tổng tiền là tổng giá các sản phẩm giảm 10%
                hoadon.TienGoc = (tongTien);

                _db.Hoadons.Add(hoadon);
                _db.SaveChanges();

                foreach (var cthd in cthdInOrder)
                {
                    cthd.MaHd = hoadon.MaHd;
                }

                foreach (var ctcbo in ctcboInOrders)
                {
                    ctcbo.MaHd = hoadon.MaHd;
                }

                _db.AddRange(cthdInOrder);
                _db.AddRange(ctcboInOrders);
                _db.SaveChanges();

                Console.WriteLine($">>> Đã tạo hóa đơn {hoadon.MaHd} cho khách hàng {khachHang.HoTen} với tổng tiền {hoadon.TienGoc} VNĐ");
            }

        }
    }
}