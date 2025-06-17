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
                // Tìm trong local trước, nếu không có thì tìm trên DB với AsNoTracking
                var FindCart_Combodetails = db.Giohangctcombos.Local.FirstOrDefault(p => p.MaGioHang == model.MaGioHang && p.MaCtsp == model.MaCtsp) ?? await db.Giohangctcombos.FirstOrDefaultAsync(p => p.MaGioHang == model.MaGioHang && p.MaCtsp == model.MaCtsp);
                if (FindCart_Combodetails != null)
                {
                    return await UpdateCart_DetailCombo(FindCart_Combodetails, model.SoLuong);
                }

                // Nếu chưa tồn tại thì thêm mới
                db.Giohangctcombos.Add(model);
                await db.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in AddCart_DetailCombo", ex);
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

        public async Task<List<Giohangctcombo>> DetailsCart_DetailCombo(int MaGioHang)
        {
            try
            {
                var FindDetailsCart_DetailCombo = await db.Giohangctcombos.AsNoTracking().Where(p => p.MaGioHang == MaGioHang).ToListAsync();
                if (FindDetailsCart_DetailCombo == null)
                {
                    throw new Exception("Error Not Found DetailsCart_DetailCombo");
                }
                return FindDetailsCart_DetailCombo;
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }
        }

        public async Task<Giohangctcombo> UpdateCart_DetailCombo(Giohangctcombo model, int Quantity)
        {
            try
            {
                //var FindCart_ComboDetails = db.Giohangctcombos.Local.FirstOrDefault(p => p.MaGioHang == model.MaGioHang && p.MaCtsp == model.MaCtsp) ?? await db.Giohangctcombos.FirstOrDefaultAsync(p => p.MaGioHang == model.MaGioHang && p.MaCtsp == model.MaCtsp);
                //if (FindCart_ComboDetails == null)
                //{
                //    throw new Exception("Not found Cart_DetailsCombo");
                //}
                model.SoLuong += Quantity;
                db.Giohangctcombos.Update(model);
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