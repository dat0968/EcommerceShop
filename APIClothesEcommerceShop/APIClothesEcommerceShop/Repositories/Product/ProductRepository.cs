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
            catch(Exception ex)
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

        public async Task<List<ProductResponseDTO>> GetAll(string? search, string? selectedCategory, string? sortByPrice)
        {
            try
            {
                var GetProduct = await db.Sanphams.AsNoTracking()
                    .Where(p => p.IsActive == true)
                    .Select(p => new ProductResponseDTO
                    {
                        MaSp = p.MaSp,
                        TenSanPham = p.TenSanPham,
                        KhoangGia = p.Chitietsanphams.Where(p => p.IsActive == true).Any()
                        ? (p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia) == p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)
                            ? $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ"
                            : $"{p.Chitietsanphams.Where(p => p.IsActive == true).Min(p => p.DonGia)} VNĐ - {p.Chitietsanphams.Where(p => p.IsActive == true).Max(p => p.DonGia)} VNĐ")
                        : "Chưa có giá",
                        SoLuong = p.Chitietsanphams.Where(p => p.IsActive == true).Sum(p => p.SoLuongTon),
                        MoTa = p.MoTa,
                        HasVariants = p.Chitietsanphams.Where(p => p.IsActive == true && (string.IsNullOrEmpty(p.MauSac) == true && string.IsNullOrEmpty(p.KichThuoc) == true)).Count() > 0 ? false : true,
                        CategoryDetails = p.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
                        {
                            MaDanhMucCha = p.MaDanhMucCha,
                            MaDanhMucCon = p.MaDanhMucCon
                        }).ToList(),
                        ProductDetails = p.Chitietsanphams.Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
                        {
                            MaCtsp = p.MaCtsp,
                            KichThuoc = p.KichThuoc,
                            MauSac = p.MauSac,
                            SoLuongTon = p.SoLuongTon,
                            DonGia = p.DonGia,
                            Images = p.Hinhanhs.Count() > 0 ? p.Hinhanhs.Select(p => new ImageProductResponseDTO
                            {
                                MaCtsp = p.MaCtsp,
                                TenHinhAnh = p.TenHinhAnh
                            }).ToList() : new List<ImageProductResponseDTO>(),
                        }).ToList(),
                    }).ToListAsync();

                if (!string.IsNullOrEmpty(search))
                {
                    GetProduct = GetProduct.Where(p => p.MaSp.ToString().Contains(search) || p.TenSanPham.ToLower().Contains(search.ToLower())).ToList();
                }
                if (!string.IsNullOrEmpty(selectedCategory))
                {
                    GetProduct = GetProduct.Where(p => p.CategoryDetails.Any(cd =>
                    cd.MaDanhMucCha.ToString().Contains(selectedCategory))).ToList();
                }
                if (!string.IsNullOrEmpty(sortByPrice))
                {
                    if (sortByPrice.ToLower() == "asc")
                    {
                        GetProduct = GetProduct.OrderBy(p => p.ProductDetails.Any() ? p.ProductDetails.Min(ct => ct.DonGia) : 0).ToList();
                    }
                    else if (sortByPrice.ToLower() == "desc")
                    {
                        GetProduct = GetProduct.OrderByDescending(p => p.ProductDetails.Any() ? p.ProductDetails.Min(ct => ct.DonGia) : 0).ToList();
                    }
                }
                return GetProduct;
            }catch(Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<ProductResponseDTO> GetById(int id)
        {
            try
            {
                var GetProductById = await db.Sanphams.AsNoTracking().FirstOrDefaultAsync(p => p.IsActive == true && p.MaSp == id);
                //var GetProductById = await db.Sanphams.AsNoTracking().FirstOrDefaultAsync(p => p.IsActive == true && p.MaSp == id).Select(p => new ProductResponseDTO
                //{
                //    MaSp = p.MaSp,
                //    TenSanPham = p.TenSanPham,
                //    MoTa = p.MoTa,
                //    CategoryDetails = p.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
                //    {
                //        MaDanhMucCha = p.MaDanhMucCha,
                //        MaDanhMucCon = p.MaDanhMucCon
                //    }).ToList(),
                //    ProductDetails = p.Chitietsanphams.Where(p => p.IsActive == true).Select(p => new ProductDetailResponseDTO
                //    {
                //        MaCtsp = p.MaCtsp,
                //        KichThuoc = p.KichThuoc,
                //        MauSac = p.MauSac,
                //        SoLuongTon = p.SoLuongTon,
                //        DonGia = p.DonGia,
                //        Images = p.Hinhanhs.Select(p => new ImageProductResponseDTO
                //        {
                //            MaCtsp = p.MaCtsp,
                //            TenHinhAnh = p.TenHinhAnh
                //        }).ToList(),
                //    }).ToList(),
                //});
                if (GetProductById == null)
                {
                    throw new Exception("Not Found Product");
                }
                var ResponseProduct = new ProductResponseDTO
                {
                    MaSp = GetProductById.MaSp,
                    TenSanPham = GetProductById.TenSanPham,
                    MoTa = GetProductById.MoTa,
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
                db.Sanphams.Update(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
