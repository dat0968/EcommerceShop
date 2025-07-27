using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.Comments;
using APIClothesEcommerceShop.Repositories.Reviews;
using APIClothesEcommerceShop.Repositories.WheelCoupon;
using APIClothesEcommerceShop.Services;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceShopContext _context;
        private readonly IGeminiAIService _ai;

        public ICategoryRepository Category { get; private set; }
        public IReviewRepository Review { get; private set; }
        public IWheelCouponRepository WheelCoupon { get; private set; }
        // public ICommentRepository Comment { get; private set; }

        public UnitOfWork(EcommerceShopContext context, IGeminiAIService ai)
        {
            _context = context;
            _ai = ai;
            Category = new CategoryRepository(_context);
            Review = new ReviewRepository(_context, _ai);
            WheelCoupon = new WheelCouponRepository(_context);
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