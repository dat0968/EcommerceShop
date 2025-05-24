using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.Repositories.Category;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
        ICategoryRepository Category { get; }
    }
}