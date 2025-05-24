using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Category;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceShopContext _context;

        public ICategoryRepository Category { get; private set; }

        public UnitOfWork(EcommerceShopContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
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