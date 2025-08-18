using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.Comments;
using APIClothesEcommerceShop.Repositories.Reviews;
using APIClothesEcommerceShop.Repositories.WheelCoupon;
using APIClothesEcommerceShop.Services;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
        ICategoryRepository Category { get; }
        IReviewRepository Review { get; }
        IWheelCouponRepository WheelCoupon { get; }
        ICommentRepository Comment { get; }
    }
}