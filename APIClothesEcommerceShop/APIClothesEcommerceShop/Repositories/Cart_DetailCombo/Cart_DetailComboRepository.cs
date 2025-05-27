using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace APIClothesEcommerceShop.Repositories.Cart_DetailCombo
{
    public class Cart_DetailComboRepository : ICart_DetailComboRepository
    {
        private readonly EcommerceShopContext db;
        public Cart_DetailComboRepository(EcommerceShopContext db)
        { 
            this.db = db;
        }
        public async Task<Giohangctcombo> AddCart_DetailCombo(Giohangctcombo model)
        {
            try
            {
                var FindCart_Combodetails = await db.Giohangctcombos.AsNoTracking().FirstOrDefaultAsync(p => p.MaGioHang == model.MaGioHang);
                if(FindCart_Combodetails != null)
                {
                    // Cập nhật số lượng của giỏ hàng_chitiecombo
                    FindCart_Combodetails.SoLuong += model.SoLuong;
                    var Cart_Combodetails = await UpdateCart_DetailCombo(FindCart_Combodetails.MaGioHang, model.SoLuong);
                    return Cart_Combodetails;
                }

                else if(FindCart_Combodetails == null)
                {
                    db.Giohangctcombos.Add(model);
                    await db.SaveChangesAsync();
                }
                return model;
            }catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task DeleteCart_DetailCombo(int MaGioHang)
        {
            try
            {
                var FindCart_ComboDetails = await db.Giohangctcombos.FirstOrDefaultAsync(c => c.MaGioHang == MaGioHang);
                if (FindCart_ComboDetails == null)
                {
                    throw new Exception("Not found Cart_DetailsCombo");
                }
                db.Remove(FindCart_ComboDetails);              
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Giohangctcombo> UpdateCart_DetailCombo(int MaGioHang, int Quantity)
        {
            try
            {
                var FindCart_ComboDetails = await db.Giohangctcombos.FirstOrDefaultAsync(c => c.MaGioHang == MaGioHang);
                if (FindCart_ComboDetails == null)
                {
                    throw new Exception("Not found Cart_DetailsCombo");
                }
                FindCart_ComboDetails.SoLuong += Quantity;
                db.Update(FindCart_ComboDetails);
                await db.SaveChangesAsync();
                return FindCart_ComboDetails;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }
    }
}
