using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO;
using APIClothesEcommerceShop.DTO.Statistics;
using APIClothesEcommerceShop.Models;

namespace APIClothesEcommerceShop.Repositories.Statistics
{
    public interface IStatisticRepository
    {
        Task<ResponseAPI<OrderStatisticsResponse>> GetOrderStatisticsAsync();
        Task<ResponseAPI<ProductStatisticsResponse>> GetProductStatisticsAsync();
        Task<ResponseAPI<CustomerStatisticsResponse>> GetCustomerStatisticsAsync();
        Task<ResponseAPI<EmployeeStatisticsResponse>> GetEmployeeStatisticsAsync();
        Task<ResponseAPI<RevenueStatisticsResponse>> GetRevenueStatisticsAsync();
        Task<ResponseAPI<OrderSummaryResponse>> GetOrderSummaryByOrder();
        Task<ResponseAPI<ComboStatisticsResponse>> GetComboStatisticsAsync();
    }
}