using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ImageProduct;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;
using Mscc.GenerativeAI;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace APIClothesEcommerceShop.Repositories.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceShopContext db;
        public ProductRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Sanpham> Add(Sanpham model)
        {
            try
            {
                db.Sanphams.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task Cancel(int id)
        {
            try
            {
                var findProduct = await db.Sanphams.FirstOrDefaultAsync(p => p.MaSp == id);
                if (findProduct == null)
                {
                    throw new Exception("Not Found Product");
                }
                findProduct.IsActive = false;
                db.Update(findProduct);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<ProductResponseDTO>> GetAll(string? search, string? selectedBigCategory, string? selectedSmallCategory, string? sortByPrice, string? filterPrice)
        {
            try
            {
                var products = db.Sanphams.AsNoTracking()
                    .Where(p => p.IsActive == true);
                if (!string.IsNullOrEmpty(search))
                {
                    products = products.Where(p => p.MaSp.ToString().Contains(search) || p.TenSanPham.ToLower().Contains(search.ToLower()));
                }
                if (!string.IsNullOrEmpty(selectedBigCategory))
                {
                    products = products.Where(p => p.Chitietdanhmucs.Any(cd =>
                    cd.MaDanhMucCha.ToString().Contains(selectedBigCategory)));
                }
                if (!string.IsNullOrEmpty(selectedSmallCategory))
                {
                    products = products.Where(p => p.Chitietdanhmucs.Any(cd =>
                    cd.MaDanhMucCon.ToString().Contains(selectedSmallCategory)));
                }
                if (!string.IsNullOrEmpty(sortByPrice))
                {
                    if (sortByPrice.ToLower() == "asc")
                    {
                        products = products.OrderBy(p => p.Chitietsanphams.Any() ? p.Chitietsanphams.Min(ct => ct.DonGia) : 0);
                    }
                    else if (sortByPrice.ToLower() == "desc")
                    {
                        products = products.OrderByDescending(p => p.Chitietsanphams.Any() ? p.Chitietsanphams.Min(ct => ct.DonGia) : 0);
                    }
                }
                switch (filterPrice?.ToLower())
                {
                    case "dưới 300k":
                        products = products.Where(p => p.Chitietsanphams.Max(ct => ct.DonGia) < 300000);
                        break;
                    case "300k - 1 triệu":
                        products = products.Where(p => p.Chitietsanphams.Max(ct => ct.DonGia) >= 300000 && p.Chitietsanphams.Max(ct => ct.DonGia) <= 1000000);
                        break;
                    case "1 triệu - 2 triệu":
                        products = products.Where(p => p.Chitietsanphams.Max(ct => ct.DonGia) >= 1000000 && p.Chitietsanphams.Max(ct => ct.DonGia) <= 2000000);
                        break;
                    case "trên 2 triệu":
                        products = products.Where(p => p.Chitietsanphams.Max(ct => ct.DonGia) >= 2000000);
                        break;
                    default:
                        products = products;
                        break;
                }
                var GetProduct = await products
                    .Select(p => new ProductResponseDTO
                    {
                        MaSp = p.MaSp,
                        TenSanPham = p.TenSanPham,
                        ReviewCount = p.DanhGias.Count(),
                        AverageRating = p.DanhGias.Any() ? p.DanhGias.Average(dg => dg.SoSao) : 5,
                        KhoangGia = p.Chitietsanphams.Where(p => p.IsActive == true).Any()
                        ? (p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia) == p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)
                            ? $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                            : $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                        : "Chưa có giá",
                        SoLuong = p.Chitietsanphams.Where(p => p.IsActive == true).Sum(p => p.SoLuongTon),
                        AnhDaiDien = p.Chitietsanphams.FirstOrDefault(p => p.IsActive == true).Hinhanhs.First().TenHinhAnh,
                    }).ToListAsync();




                return GetProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<ProductResponseDTO> GetById(int id)
        {
            try
            {
                var GetProductById = await db.Sanphams.AsNoTracking()
                    .Include(p => p.Chitietdanhmucs)
                    .Include(p => p.DanhGias)
                    .Include(p => p.Chitietsanphams)
                    .ThenInclude(p => p.Hinhanhs)
                    .FirstOrDefaultAsync(p => p.IsActive == true && p.MaSp == id);
                if (GetProductById == null)
                {
                    throw new Exception("Not Found Product");
                }
                var ResponseProduct = new ProductResponseDTO
                {
                    MaSp = GetProductById.MaSp,
                    TenSanPham = GetProductById.TenSanPham,
                    MoTa = GetProductById.MoTa,
                    NgayTao = GetProductById.NgayTao,
                    ReviewCount = GetProductById.DanhGias.Count(),
                    AverageRating = GetProductById.DanhGias.Any() ? GetProductById.DanhGias.Average(dg => dg.SoSao) : 5,
                    HasVariants = GetProductById.Chitietsanphams.Where(p => p.IsActive == true && (string.IsNullOrEmpty(p.MauSac) == true && string.IsNullOrEmpty(p.KichThuoc) == true)).Count() > 0 ? false : true,
                    LuotYeuThich = GetProductById.Sanphamyeuthichs.Count(),
                    CategoryDetails = GetProductById.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
                    {
                        MaDanhMucCha = p.MaDanhMucCha,
                        MaDanhMucCon = p.MaDanhMucCon
                    }).ToList(),
                    ProductDetails = GetProductById.Chitietsanphams.Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
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
                };
                return ResponseProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Sanpham> Update(Sanpham model)
        {
            try
            {
                var findProduct = db.Sanphams.Local.FirstOrDefault(p => p.MaSp == model.MaSp) ?? await db.Sanphams.FirstOrDefaultAsync(p => p.MaSp == model.MaSp);
                if (findProduct == null)
                {
                    throw new Exception("Sản phẩm không tồn tại");
                }
                db.Entry(findProduct).CurrentValues.SetValues(model);
                //db.Sanphams.Update(model);
                await db.SaveChangesAsync();
                return findProduct;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
