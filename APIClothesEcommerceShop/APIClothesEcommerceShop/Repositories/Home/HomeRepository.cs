using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.ImageProduct;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Prng.Drbg;

namespace APIClothesEcommerceShop.Repositories.Home
{
    public class HomeRepository : IHomeRepository
    {
        private readonly EcommerceShopContext db;
        public HomeRepository(EcommerceShopContext db)
        {
            this.db = db;
        }

        public async Task<List<ProductResponseDTO>> GetBestsellerProducts()
        {
            try
            {
                var products = (from ct in db.Cthoadons
                               join ctsp in db.Chitietsanphams on ct.MaCtsp equals ctsp.MaCtsp
                               join sp in db.Sanphams on ctsp.MaSp equals sp.MaSp
                               group new { sp, ctsp, ct } by new { sp.MaSp, sp.TenSanPham, sp.MoTa } into g
                               orderby g.Sum(x => x.ct.SoLuong) descending
                               select new ProductResponseDTO
                               {
                                   MaSp = g.Key.MaSp,
                                   TenSanPham = g.Key.TenSanPham,
                                   MoTa = g.Key.MoTa,
                                   KhoangGia = g.Select(x => x.ctsp).Where(p => p.IsActive == true).Any()
                                    ? (g.Select(x => x.ctsp).Where(p => p.IsActive == true).Min(p => p.DonGia) == g.Select(x => x.ctsp).Where(p => p.IsActive == true).Max(p => p.DonGia)
                                        ? $"{g.Select(x => x.ctsp).Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                                        : $"{g.Select(x => x.ctsp).Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {g.Select(x => x.ctsp).Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                                    : "Chưa có giá",
                                   ProductDetails = g.Select(x => x.ctsp).Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
                                   {
                                       MaCtsp = p.MaCtsp,
                                       KichThuoc = p.KichThuoc,
                                       MauSac = p.MauSac,
                                       SoLuongTon = p.SoLuongTon,
                                       DonGia = p.DonGia,
                                       Images = p.Hinhanhs.Select(p => new ImageProductResponseDTO
                                       {
                                           MaCtsp = p.MaCtsp,
                                           TenHinhAnh = p.TenHinhAnh
                                       }).ToList(),
                                   }).ToList(),
                               }).Take(4).ToListAsync();
                return await products;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<ProductResponseDTO>> GetHotProducts()
        {
            try
            {
                var products = await db.Sanphams.AsNoTracking().Select(p => new ProductResponseDTO
                {
                    MaSp = p.MaSp,
                    TenSanPham = p.TenSanPham,
                    MoTa = p.MoTa,
                    NgayTao = p.NgayTao,
                    LuotXem = p.LuotXem,
                    KhoangGia = p.Chitietsanphams.Where(p => p.IsActive == true).Any()
                        ? (p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia) == p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)
                            ? $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                            : $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                        : "Chưa có giá",
                    ProductDetails = p.Chitietsanphams.Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
                    {
                        MaCtsp = p.MaCtsp,
                        KichThuoc = p.KichThuoc,
                        MauSac = p.MauSac,
                        SoLuongTon = p.SoLuongTon,
                        DonGia = p.DonGia,
                        Images = p.Hinhanhs.Select(p => new ImageProductResponseDTO
                        {
                            MaCtsp = p.MaCtsp,
                            TenHinhAnh = p.TenHinhAnh
                        }).ToList(),
                    }).ToList(),
                }).OrderByDescending(d => d.LuotXem).Take(4).ToListAsync();
                return products;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<ProductResponseDTO>> GetNewProducts()
        {
            try
            {
                var products = await db.Sanphams.AsNoTracking()
                    .Include(p => p.Chitietsanphams)
                    .Include(p => p.Chitietsanphams)
                    .ThenInclude(ct => ct.Hinhanhs).Where(p => p.IsActive == true).Select(p => new ProductResponseDTO
                {
                    MaSp = p.MaSp,
                    TenSanPham = p.TenSanPham,
                    MoTa = p.MoTa,
                    NgayTao = p.NgayTao,
                    KhoangGia = p.Chitietsanphams.Where(p => p.IsActive == true).Any()
                        ? (p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia) == p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)
                            ? $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                            : $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                        : "Chưa có giá",
                    ProductDetails = p.Chitietsanphams.Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
                    {
                        MaCtsp = p.MaCtsp,
                        KichThuoc = p.KichThuoc,
                        MauSac = p.MauSac,
                        SoLuongTon = p.SoLuongTon,
                        DonGia = p.DonGia,
                        Images = p.Hinhanhs.Select(p => new ImageProductResponseDTO
                        {
                            MaCtsp = p.MaCtsp,
                            TenHinhAnh = p.TenHinhAnh
                        }).ToList(),
                    }).ToList(),
                }).OrderByDescending(d => d.NgayTao).Take(8).ToListAsync();
                return products;
            }catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
            
        }
    }
}
