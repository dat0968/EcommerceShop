using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Category;
using APIClothesEcommerceShop.Repositories.Comments;
using APIClothesEcommerceShop.Repositories.Reviews;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
        ICategoryRepository Category { get; }
        IReviewRepository Review { get; }
        // ICommentRepository Comment { get; }
    }
}