using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.CategoryDetails;
using APIClothesEcommerceShop.DTO.ImageProduct;
using APIClothesEcommerceShop.DTO.Product;
using APIClothesEcommerceShop.DTO.ProductDetails;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<List<ProductResponseDTO>> GetAll()
        {
            try
            {
                var GetProduct = await db.Sanphams.AsNoTracking().Select(p => new ProductResponseDTO
                {
                    MaSp = p.MaSp,
                    TenSanPham = p.TenSanPham,
                    MoTa = p.MoTa,
                    CategoryDetails = p.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
                    {
                        MaDanhMucCha = p.MaDanhMucCha,
                        MaDanhMucCon = p.MaDanhMucCon
                    }).ToList(),
                    ProductDetails = p.Chitietsanphams.Select(p => new ProductDetailResponseDTO
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
                    }).Where(p => p.IsActive == true).ToList(),
                }).Where(p => p.IsActive == true).ToListAsync();
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
                var GetProductById = await db.Sanphams.AsNoTracking().Select(p => new ProductResponseDTO
                {
                    MaSp = p.MaSp,
                    TenSanPham = p.TenSanPham,
                    MoTa = p.MoTa,
                    CategoryDetails = p.Chitietdanhmucs.Select(p => new CategoryDetailsResponseDTO
                    {
                        MaDanhMucCha = p.MaDanhMucCha,
                        MaDanhMucCon = p.MaDanhMucCon
                    }).ToList(),
                    ProductDetails = p.Chitietsanphams.Select(p => new ProductDetailResponseDTO
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
                    }).Where(p => p.IsActive == true).ToList(),
                }).FirstOrDefaultAsync(p => p.IsActive == true && p.MaSp == id);
                if (GetProductById == null)
                {
                    throw new Exception("Not Found Product");
                }
                return GetProductById;
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
