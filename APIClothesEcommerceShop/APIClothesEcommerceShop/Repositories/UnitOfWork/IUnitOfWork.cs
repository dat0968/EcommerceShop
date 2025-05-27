using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task SaveAsync();
    }
}