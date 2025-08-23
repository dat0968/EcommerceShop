using System.Diagnostics;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Coupon;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Macoupon;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Coupon
{
    public class MaCouponRepository : IMaCouponRepository
    {
        private readonly EcommerceShopContext db;
        public MaCouponRepository(EcommerceShopContext db)
        {
            this.db = db;
        }

        public async Task<CouponDTO> Create(CouponDTO maCoupon)
        {
            try
            {
                string maCode = string.IsNullOrWhiteSpace(maCoupon.MaCode)
                    ? GenerateRandomCouponCode()
                    : maCoupon.MaCode;

                var newCouponCode = new APIClothesEcommerceShop.Models.Macoupon
                {
                    MaCode = maCode,
                    MoTa = maCoupon.MoTa,
                    SoTienGiam = maCoupon.SoTienGiam > 0 ? maCoupon.SoTienGiam : 0,
                    PhanTramGiam = maCoupon.PhanTramGiam > 0 ? maCoupon.PhanTramGiam : 0,
                    NgayKetThuc = maCoupon.NgayKetThuc,
                    NgayBatDau = maCoupon.NgayBatDau,
                    SoLuong = maCoupon.SoLuong,
                    TrangThai = true,
                    DonHangToiThieu = maCoupon.DonHangToiThieu,
                    SoLuongDaDung = 0,
                    MaKhachHang = maCoupon.MaKhachHang
                };

                db.Macoupons.Add(newCouponCode);
                await db.SaveChangesAsync();
                return maCoupon;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        private string GenerateRandomCouponCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task Cancel(string id)
        {
            var findMaCoupon = await db.Macoupons.FirstOrDefaultAsync(p => p.MaCode == id);
            if (findMaCoupon != null)
            {
                findMaCoupon.TrangThai = false;
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<CouponDTO>> GetAll(string? keywords, string? status, string? sort)
        {
            var listCouponCode = db.Macoupons
                .Include(c => c.KhachHang)
                .Where(p => p.SoTienGiam != 0 || p.PhanTramGiam != 0)
                .AsQueryable();
            var covertToListMaCouponVM = new List<CouponDTO>();

            if (!string.IsNullOrEmpty(keywords))
            {
                listCouponCode = listCouponCode.Where(p => p.MaCode.Contains(keywords));
            }

            switch (status)
            {
                case "Còn hiệu lực":
                    listCouponCode = listCouponCode.Where(p => p.TrangThai == true && p.NgayKetThuc > DateTime.Now && p.SoLuongDaDung < p.SoLuong);
                    break;
                case "Đã hủy":
                    listCouponCode = listCouponCode.Where(p => p.TrangThai == false);
                    break;
                case "Đã hết hạn":
                    listCouponCode = listCouponCode.Where(p => p.TrangThai == true && p.NgayKetThuc < DateTime.Now && p.SoLuongDaDung < p.SoLuong);
                    break;
                case "Đã hết":
                    listCouponCode = listCouponCode.Where(p => p.TrangThai == true && p.SoLuongDaDung >= p.SoLuong);
                    break;
                default:
                    listCouponCode = listCouponCode.OrderByDescending(p => p.NgayBatDau);
                    break;
            }

            switch (sort)
            {
                case "asc":
                    listCouponCode = listCouponCode.OrderBy(p => p.NgayBatDau);
                    break;
                default:
                    listCouponCode = listCouponCode.OrderByDescending(p => p.NgayBatDau);
                    break;
            }

            var result = await listCouponCode.ToListAsync();
            foreach (var item in result)
            {
                covertToListMaCouponVM.Add(new CouponDTO
                {
                    MaCode = item.MaCode,
                    PhanTramGiam = item.PhanTramGiam,
                    SoTienGiam = item.SoTienGiam,
                    NgayKetThuc = item.NgayKetThuc,
                    SoLuong = item.SoLuong,
                    SoLuongDaDung = item.SoLuongDaDung,
                    TrangThai = item.TrangThai,
                    NgayBatDau = item.NgayBatDau,
                    DonHangToiThieu = item.DonHangToiThieu,
                    MaKhachHang = item.MaKhachHang,
                    HoTen = item.KhachHang?.HoTen
                });
            }
            return covertToListMaCouponVM;
        }

        public async Task Update(CouponDTO maCoupon)
        {
            try
            {
                var editCouponCode = await db.Macoupons.FirstOrDefaultAsync(p => p.MaCode == maCoupon.MaCode);

                if (editCouponCode != null)
                {
                    editCouponCode.MoTa = maCoupon.MoTa;
                    editCouponCode.SoTienGiam = maCoupon.SoTienGiam > 0 ? maCoupon.SoTienGiam : 0;
                    editCouponCode.PhanTramGiam = maCoupon.PhanTramGiam > 0 ? maCoupon.PhanTramGiam : 0;
                    editCouponCode.NgayKetThuc = maCoupon.NgayKetThuc;
                    editCouponCode.SoLuong = maCoupon.SoLuong;
                    editCouponCode.TrangThai = maCoupon.TrangThai;
                    editCouponCode.NgayBatDau = maCoupon.NgayBatDau;
                    editCouponCode.SoLuongDaDung = maCoupon.SoLuongDaDung;
                    editCouponCode.DonHangToiThieu = maCoupon.DonHangToiThieu;
                    db.Macoupons.Update(editCouponCode);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<CouponDTO?> GetById(string macoupon)
        {
            var findCoupon = await db.Macoupons.AsNoTracking().FirstOrDefaultAsync(p => p.MaCode == macoupon.Trim());
            if (findCoupon != null)
            {
                var couponDTO = new CouponDTO
                {
                    MaCode = findCoupon.MaCode,
                    SoLuong = findCoupon.SoLuong,
                    SoLuongDaDung = findCoupon.SoLuongDaDung,
                    MoTa = findCoupon.MoTa,
                    PhanTramGiam = findCoupon.PhanTramGiam,
                    SoTienGiam = findCoupon.SoTienGiam,
                    DonHangToiThieu = findCoupon.DonHangToiThieu,
                    NgayBatDau = findCoupon.NgayBatDau,
                    NgayKetThuc = findCoupon.NgayKetThuc,
                    TrangThai = findCoupon.TrangThai,
                    MaKhachHang = findCoupon.MaKhachHang,
                    HoTen = findCoupon.KhachHang?.HoTen
                };
                return couponDTO;
            }
            return null;
        }

        public async Task<bool> CheckUser_CouponCode(int maUser, string couponcode)
        {
            var check = await db.Hoadons.AsNoTracking().FirstOrDefaultAsync(p => p.MaKh == maUser && p.MaCode == couponcode.Trim());
            if (check != null)
            {
                return true;
            }
            return false;
        }
    }
}