using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Cart;
using APIClothesEcommerceShop.DTO.Cart_DetailsCombo;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Cart
{
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceShopContext db;
        public CartRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task<Giohang> AddCart(Giohang model)
        {
            try
            {
                var FindCart_Product = await db.Giohangs.AsNoTracking().FirstOrDefaultAsync(p => p.MaCtsp == model.MaCtsp && p.MaKh == model.MaKh);              
                if(FindCart_Product != null)
                {
                    // Cập nhật số lượng giỏ hàng
                    var Cart_Product = await UpdateCart(FindCart_Product.Id, model.SoLuong);
                    return Cart_Product;
                }
                else
                {
                    db.Giohangs.Add(model);
                    await db.SaveChangesAsync();
                }
                return model;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteCart(int IdCart)
        {
            try
            {
                var FindCart = await db.Giohangs.FirstOrDefaultAsync(p => p.Id == IdCart);
                if(FindCart == null)
                {
                    throw new Exception("Not found Cart");
                }

                db.Remove(FindCart);
                await db.SaveChangesAsync();
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<CartResponseDTO>> GetAll(int MaKh)
        {
            try
            {
                var GetAll = await db.Giohangs.AsNoTracking().Include(p => p.MaCtspNavigation).Select(p => new CartResponseDTO
                {
                    Id = p.Id,
                    MaCombo = p.MaCombo,
                    MaCtsp = p.MaCtsp,
                    TenSanPham = p.MaCtspNavigation != null ? p.MaCtspNavigation.MaSpNavigation.TenSanPham : null,
                    MaKh = p.MaKh,
                    KichThuoc = p.MaCtspNavigation != null ? p.MaCtspNavigation.KichThuoc : null,
                    Mau = p.MaCtspNavigation != null ? p.MaCtspNavigation.MauSac : null,
                    DonGia = p.DonGia,
                    SoLuong = p.SoLuong,
                    SoLuongToiDa = p.MaCtspNavigation != null ? p.MaCtspNavigation.SoLuongTon : 0,
                    TenHinhAnh = p.TenHinhAnh,
                    Giohangctcombos = p.Giohangctcombos.Select(ct => new Cart_DetailsComboResponseDTO
                    {
                        Id = ct.Id,
                        MaGioHang = ct.MaGioHang,
                        MaCtsp = ct.MaCtsp,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia
                    }).ToList()
                }).Where(p => p.MaKh == MaKh).ToListAsync();
                return GetAll;
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Giohang> UpdateCart(int id, int Quantity)
        {
            try
            {
                var FindCart = await db.Giohangs.FirstOrDefaultAsync(p => p.Id == id);
                if(FindCart == null)
                {
                    throw new Exception("Not Found Cart");
                }
                var Findproduct = await db.Chitietsanphams.FirstOrDefaultAsync(p => p.MaCtsp == FindCart.MaCtsp);
                var QuantityProduct = Findproduct?.SoLuongTon;
                FindCart.SoLuong += Quantity;
                if(FindCart.SoLuong > QuantityProduct)
                {
                    throw new Exception($"Số lượng sản phẩm trong giỏ hàng vượt quá số lượng tồn kho tối đa là {QuantityProduct} sản phẩm");
                }
                db.Giohangs.Update(FindCart);
                await db.SaveChangesAsync();
                return FindCart;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
