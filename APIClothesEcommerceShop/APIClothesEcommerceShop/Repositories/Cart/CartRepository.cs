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
                throw new Exception("Error", ex);
            }
        }

        public async Task DeleteCart(int id)
        {
            try
            {
                var FindCart = await db.Giohangs.FindAsync(id);
                if(FindCart == null)
                {
                    throw new Exception("Not found Cart");
                }
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<List<CartResponseDTO>> GetAll()
        {
            try
            {
                var GetAll = await db.Giohangs.AsNoTracking().Select(p => new CartResponseDTO
                {
                    Id = p.Id,
                    MaCombo = p.MaCombo,
                    MaCtsp = p.MaCtsp,
                    MaKh = p.MaKh,
                    DonGia = p.DonGia,
                    Giohangctcombos = p.Giohangctcombos.Select(ct => new Cart_DetailsComboResponseDTO
                    {
                        Id = ct.Id,
                        MaGioHang = ct.MaGioHang,
                        MaCtsp = ct.MaCtsp,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia
                    }).ToList()
                }).ToListAsync();
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
                FindCart.SoLuong += Quantity;
                db.Giohangs.Update(FindCart);
                await db.SaveChangesAsync();
                return FindCart;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
