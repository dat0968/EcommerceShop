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
                var FindCart_Product = await db.Giohangs.AsNoTracking().FirstOrDefaultAsync(p => ((p.MaCtsp != null && p.MaCtsp == model.MaCtsp) || (p.MaCombo != null && p.MaCombo == model.MaCombo)) && p.MaKh == model.MaKh);              
                if(FindCart_Product != null)
                {
                    if(FindCart_Product.MaCombo == null)
                    {
                        // Cập nhật số lượng giỏ hàng
                        var Cart_Product = await UpdateCart(FindCart_Product.Id, model.SoLuong);
                        return Cart_Product;
                    }
                    else
                    {
                        var selectedVariants = model.Giohangctcombos.Select(p => p.MaCtsp).ToList();
                        var checkComboVariants = await db.Giohangs.AsNoTracking().FirstOrDefaultAsync(p => p.MaCombo == model.MaCombo && p.MaKh == model.MaKh && p.Giohangctcombos.All(ghct => selectedVariants.Contains(ghct.MaCtsp)));
                        if(checkComboVariants != null)
                        {
                            var findCombo = await db.Combos.AsNoTracking().FirstOrDefaultAsync(p => p.MaCombo == model.MaCombo);
                            var QuantityCombo = findCombo?.SoLuong;
                            checkComboVariants.SoLuong += model.SoLuong;
                            db.Giohangs.Update(checkComboVariants);
                            if (checkComboVariants.SoLuong > QuantityCombo)
                            {
                                throw new Exception($"Số lượng trong giỏ hàng vượt quá số lượng tồn kho tối đa là {QuantityCombo} combo");
                            }
                            foreach (var details in checkComboVariants.Giohangctcombos)
                            {
                                var GetMaSp = await db.Chitietsanphams.AsNoTracking().FirstOrDefaultAsync(p => p.MaCtsp == details.MaCtsp);
                                var GetDetailProduct = await db.Chitietcombos.AsNoTracking().FirstOrDefaultAsync(p => p.MaSp == GetMaSp.MaSp);
                                var QuantityProduct = GetDetailProduct.SoLuongSP;
                                details.SoLuong = QuantityProduct * checkComboVariants.SoLuong;
                            }
                            await db.SaveChangesAsync();
                            return checkComboVariants;
                        }
                        //else
                        //{
                        //    var newCart = new Giohang
                        //    {
                        //        MaCtsp = model.MaCtsp,
                        //        MaKh = model.MaKh,
                        //        MaCombo = model.MaCombo,
                        //        SoLuong = model.SoLuong,
                        //        DonGia = model.DonGia,
                        //        TenHinhAnh = model.TenHinhAnh,
                        //    };
                        //    db.Giohangs.Add(newCart);
                        //    await db.SaveChangesAsync();
                        //    foreach (var detail in model.Giohangctcombos)
                        //    {
                        //        var NewCartDetail = new Giohangctcombo
                        //        {
                        //            MaGioHang = newCart.Id,
                        //            MaCtsp = detail.MaCtsp,
                        //            DonGia = detail.DonGia,
                        //            SoLuong = detail.SoLuong,
                        //        };
                        //        db.Giohangctcombos.Add(NewCartDetail);
                        //        await db.SaveChangesAsync();
                        //    }
                        //    return newCart;
                        //}
                        
                    }
                }
                var newCart = new Giohang
                {
                    MaCtsp = model.MaCtsp,
                    MaKh = model.MaKh,
                    MaCombo = model.MaCombo,
                    SoLuong = model.SoLuong,
                    DonGia = model.DonGia,
                    TenHinhAnh = model.TenHinhAnh,
                };
                db.Giohangs.Add(newCart);
                await db.SaveChangesAsync();
                return newCart;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task DeleteCart(int IdCart)
        {
            try
            {
                var FindCartDetailCombo = await db.Giohangctcombos.Where(p => p.MaGioHang == IdCart).ToListAsync();
                if (FindCartDetailCombo != null && FindCartDetailCombo.Count() != 0)
                {
                    db.RemoveRange(FindCartDetailCombo);
                }
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
                var GetAll = await db.Giohangs.AsNoTracking().Include(p => p.MaCtspNavigation).Include(p => p.MaComboNavigation).Select(p => new CartResponseDTO
                {
                    Id = p.Id,
                    MaCombo = p.MaCombo,
                    MaCtsp = p.MaCtsp,
                    TenSanPham_TenCombo = p.MaCtspNavigation != null ? p.MaCtspNavigation.MaSpNavigation.TenSanPham : (p.MaComboNavigation != null ? p.MaComboNavigation.TenCombo : null),
                    MaKh = p.MaKh,
                    KichThuoc = p.MaCtspNavigation != null ? p.MaCtspNavigation.KichThuoc : null,
                    Mau = p.MaCtspNavigation != null ? p.MaCtspNavigation.MauSac : null,
                    DonGia = p.DonGia,
                    SoLuong = p.SoLuong,
                    SoLuongToiDa = p.MaCtspNavigation != null
                    ? p.MaCtspNavigation.SoLuongTon
                    : (p.MaComboNavigation != null ? p.MaComboNavigation.SoLuong : 0),
                    TenHinhAnh = p.TenHinhAnh,
                    Giohangctcombos = p.Giohangctcombos.Select(ct => new Cart_DetailsComboResponseDTO
                    {
                        Id = ct.Id,
                        MaGioHang = ct.MaGioHang,
                        MaCtsp = ct.MaCtsp,
                        TenSanPham = ct.MaCtspNavigation.MaSpNavigation.TenSanPham,
                        MauSac = ct.MaCtspNavigation.MauSac,
                        KichThuoc = ct.MaCtspNavigation.KichThuoc,
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
