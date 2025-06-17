using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO.Cart;
using APIClothesEcommerceShop.Models;
using APIClothesEcommerceShop.Repositories.Cart;
using APIClothesEcommerceShop.Repositories.Cart_DetailCombo;

namespace APIClothesEcommerceShop.Services
{
    public class CartService
    {
        private readonly EcommerceShopContext db;
        public readonly ICartRepository cartRepository;
        public readonly ICart_DetailComboRepository cart_DetailComboRepository;
        public CartService(EcommerceShopContext db, ICartRepository cartRepository, ICart_DetailComboRepository cart_DetailComboRepository) { 
            this.db = db;
            this.cartRepository = cartRepository;
            this.cart_DetailComboRepository = cart_DetailComboRepository;   
        } 
        public async Task AddToCart(CartRequestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();
                // Thêm giỏ hàng
                var NewCart = new Giohang
                {
                    MaCtsp = model.MaCtsp,
                    MaCombo = model.MaCombo,
                    SoLuong = model.SoLuong,
                    DonGia = model.DonGia,
                    MaKh = model.MaKh,
                    TenHinhAnh = model.TenHinhAnh,
                    Giohangctcombos = model.Giohangctcombos.Select(p => new Giohangctcombo
                    {
                        MaCtsp = p.MaCtsp,
                        SoLuong = p.SoLuong,    
                        DonGia = p.DonGia,
                    }).ToList(),
                };
                NewCart = await cartRepository.AddCart(NewCart);
                if (model.MaCombo  != null)
                {
                    // Thêm Chitietgiohang_combo
                    foreach (var item in model.Giohangctcombos)
                    {
                        var NewDetail = new Giohangctcombo
                        {
                            MaCtsp = item.MaCtsp,
                            MaGioHang = NewCart.Id,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                        };
                        NewDetail = await cart_DetailComboRepository.AddCart_DetailCombo(NewDetail);
                    }
                }
              

                await db.Database.CommitTransactionAsync();
            }catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
            }          
        }
        public async Task UpdateToCart(int id, CartRequestDTO model)
        {
            try
            {
                await db.Database.BeginTransactionAsync();
                // Cập nhật giỏ hàng
                var UpdateCart = await cartRepository.UpdateCart(id, model.SoLuong);

                // Cập nhật Chitietgiohang_combo
                if (model.MaCombo != null)
                {
                    foreach (var item in model.Giohangctcombos)
                    {
                        var NewDetail = new Giohangctcombo
                        {
                            MaCtsp = item.MaCtsp,
                            MaGioHang = UpdateCart.Id,                           
                            SoLuong = model.SoLuong * item.SoLuong,
                            DonGia = item.DonGia,
                        };
                        NewDetail = await cart_DetailComboRepository.UpdateCart_DetailCombo(NewDetail, NewDetail.SoLuong);
                    }
                }

                await db.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteCart(int IdCart)
        {
            try
            {
                await db.Database.BeginTransactionAsync();
                var cart_DetailCombo = await cart_DetailComboRepository.DetailsCart_DetailCombo(IdCart);
                if(cart_DetailCombo != null)
                {
                    await cart_DetailComboRepository.DeleteCart_DetailCombo(IdCart);
                }
                await cartRepository.DeleteCart(IdCart);
                await db.Database.CommitTransactionAsync();

            }
            catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Error", ex);
            }
        }
    }
}
