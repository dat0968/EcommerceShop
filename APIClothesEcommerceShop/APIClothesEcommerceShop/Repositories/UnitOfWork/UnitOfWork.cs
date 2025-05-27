using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceShopContext _context;

        public UnitOfWork(EcommerceShopContext context)
        {
            _context = context;
        }
        public void Save()
        {
            _context.SaveChanges();
        }
        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}