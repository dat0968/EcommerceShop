using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.FavoriteProduct;
using APIClothesEcommerceShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.FavoriteProduct
{
    public class FavoriteProduct : IFavoriteProduct
    {
        private readonly EcommerceShopContext _context;
        public FavoriteProduct(EcommerceShopContext context)
        {
            _context = context;
        }
        public async Task<FavoriteProductDTO> AddFavoriteProduct(FavoriteProductDTO fvProduct)
        {// Task<bool?>  (int userId, int productId, bool isDelete)
            try
            {
                // Kiểm tra xem bản ghi đã tồn tại chưa
                var existingFavorite = await _context.Sanphamyeuthiches
                    .FirstOrDefaultAsync(f => f.MaKh == fvProduct.MaKh && f.MaSp == fvProduct.MaSp);

                if (existingFavorite != null)
                {
                    throw new Exception("Sản phẩm này đã tồn tại");
                }

                // Chuyển đổi từ DTO sang model
                var newFavorite = new Sanphamyeuthich
                {
                    MaKh = fvProduct.MaKh,
                    MaSp = fvProduct.MaSp
                };

                // Thêm vào context
                _context.Sanphamyeuthiches.Add(newFavorite);
                await _context.SaveChangesAsync();

                // Trả về DTO với trạng thái thành công
                return new FavoriteProductDTO
                {
                    MaKh = fvProduct.MaKh,
                    MaSp = fvProduct.MaSp
                }; 
            }
            catch (Exception ex)
            {
                throw new Exception("có lỗi xảy ra", ex);
                
            }
        }

        public async Task<bool> CheckFavoriteProduct(FavoriteProductDTO fv)
        {
            var favorite = await _context.Sanphamyeuthiches
                                            .FirstOrDefaultAsync(d => d.MaKh == fv.MaKh && d.MaSp == fv.MaSp);
            if(favorite != null)
            {
                return true;
            }
            return false;
        }

        public async Task DeleteFavoriteProduct(FavoriteProductDTO fv)
        {
            try
            {
                var favorite = await _context.Sanphamyeuthiches
                                            .FirstOrDefaultAsync(d => d.MaKh == fv.MaKh && d.MaSp == fv.MaSp);

                if (favorite == null)
                {
                    throw new Exception("Sản phẩm yêu thích không tồn tại");

                }

                _context.Sanphamyeuthiches.Remove(favorite);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("error", ex);
            }
        }

        public async Task<List<FavoritveResponsDTO>> GetFavoriteProducts(int idKhachHang) // Lưu ý: Có lỗi đánh máy "FavoritveResponsDTO"
        {
            try
            {
                var favoriteProducts = _context.Sanphamyeuthiches
                    .AsNoTracking()
                    .Where(p => p.MaKh == idKhachHang)
                    .Include(p => p.MaSpNavigation) // Lấy thông tin sản phẩm
                    .ThenInclude(sp => sp.Chitietsanphams) // Lấy chi tiết sản phẩm
                    .ThenInclude(ct => ct.Hinhanhs) // Lấy hình ảnh
                    .Select(p => new FavoritveResponsDTO
                    {
                        MaSp = p.MaSp,
                        MaKh = p.MaKh,
                        TenSanPham = p.MaSpNavigation.TenSanPham,
                        HinhAnh = p.MaSpNavigation.Chitietsanphams.SelectMany(ct => ct.Hinhanhs)
                            .Select(h => h.TenHinhAnh).FirstOrDefault(),
                        KhoangGia = p.MaSpNavigation.Chitietsanphams.Select(ct => ct.DonGia).FirstOrDefault(),
                        SoLuong = p.MaSpNavigation.Chitietsanphams.Select(ct => ct.SoLuongTon).FirstOrDefault()
                    })
                    .ToListAsync(); // Sử dụng ToListAsync để lấy danh sách



                return await favoriteProducts;
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra", ex);
            }
        }
    }
}
