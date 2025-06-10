using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.DetailCombo
{
    public class DetailCombo : IDetailCombo
    {
        private readonly EcommerceShopContext db;
        public DetailCombo(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task AddDetailCombo(Chitietcombo model)
        {
            try
            {
                db.Chitietcombos.Add(model);
                await db.SaveChangesAsync();
            }catch(Exception ex)
            {
                throw;
            }
        }
        public async Task EditDetailCombo(Chitietcombo model)
        {
            try
            {
                db.Chitietcombos.Update(model);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task DeleteDetailComboByMaCombo(int MaCombo)
        {
            try
            {
                var FindDetailCombo = await db.Chitietcombos.Where(p => p.MaCombo == MaCombo).ToListAsync();
                db.Chitietcombos.RemoveRange(FindDetailCombo);
                await db.SaveChangesAsync();
            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}
