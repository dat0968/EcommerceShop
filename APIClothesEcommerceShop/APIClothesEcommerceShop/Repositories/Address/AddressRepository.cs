using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Repositories.Address
{
    public class AddressRepository : IAddressRepository
    {
        private readonly EcommerceShopContext db;
        public AddressRepository(EcommerceShopContext db)
        {
            this.db = db;
        }
        public async Task Delete(int id)
        {
            var FindAddress = await db.Diachis.FirstOrDefaultAsync(p => p.ID == id);
            if (FindAddress == null)
            {
                throw new Exception("Error to deleteAddress");
            }
            db.Diachis.Remove(FindAddress);
            await db.SaveChangesAsync();
        }

        public async Task<List<Diachi>> GetAll(int MaKh)
        {
            return await db.Diachis.AsNoTracking().ToListAsync();
        }
    }
}
