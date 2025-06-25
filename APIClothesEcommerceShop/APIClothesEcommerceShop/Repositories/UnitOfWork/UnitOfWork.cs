using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Data;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.Comments;
using APIClothesEcommerceShop.Repositories.Reviews;
using APIClothesEcommerceShop.Repositories.WheelCoupon;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EcommerceShopContext _context;

        public ICategoryRepository Category { get; private set; }
        public IReviewRepository Review { get; private set; }
        public IWheelCouponRepository WheelCoupon { get; private set; }

        // public ICommentRepository Comment { get; private set; }

        public UnitOfWork(EcommerceShopContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Review = new ReviewRepository(_context);
            WheelCoupon = new WheelCouponRepository(_context);
            // Comment = new CommentRepository(_context);
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