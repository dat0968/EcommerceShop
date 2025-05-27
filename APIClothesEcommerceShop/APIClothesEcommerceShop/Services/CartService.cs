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
                            SoLuong = NewCart.SoLuong * item.SoLuong,
                            DonGia = item.DonGia,
                        };
                        NewDetail = await cart_DetailComboRepository.AddCart_DetailCombo(NewDetail);
                    }
                }
              

                await db.Database.CommitTransactionAsync();
            }catch (Exception ex)
            {
                await db.Database.RollbackTransactionAsync();
                throw new Exception("Error", ex);
            }          
        }
        //public async Task UpdateToCart(int MaGioHang, int? MaCombo, int Quantity)
        //{
        //    try
        //    {
        //        await db.Database.BeginTransactionAsync();
        //        // Cập nhật giỏ hàng
        //        var UpdateCart = await cartRepository.UpdateCart(MaGioHang, Quantity);

        //        // Cập nhật Chitietgiohang_combo
        //        if(MaCombo != null)
        //        {
        //            foreach (var item in model.Giohangctcombos)
        //            {
        //                var NewDetail = new Giohangctcombo
        //                {
        //                    MaCtsp = item.MaCtsp,
        //                    MaGioHang = NewCart.Id,
        //                    SoLuong = NewCart.SoLuong * NewCart.SoLuong,
        //                    DonGia = NewCart.DonGia,
        //                };
        //                NewDetail = await cart_DetailComboRepository.AddCart_DetailCombo(NewDetail);
        //            }
        //        }

        //        await db.Database.CommitTransactionAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await db.Database.RollbackTransactionAsync();
        //        throw new Exception("Error", ex);
        //    }
        //}
    }
}
