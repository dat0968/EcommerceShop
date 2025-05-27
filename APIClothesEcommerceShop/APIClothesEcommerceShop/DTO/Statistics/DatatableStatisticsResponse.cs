using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIClothesEcommerceShop.DTO.Statistics.Sub;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class DatatableStatisticsResponse
    {
        public DatatableStatisticsResponse(
            List<TopProduct>? TopProduct = null,
            List<TopCustomer>? customersByLocation = null,
            List<TopEmployee>? topEmployees = null,
            List<TopCombo>? topCombosBySales = null
        )
        {
            TopProducts = TopProduct ?? new();
            TopEmployees = topEmployees ?? new();
            TopCustomers = customersByLocation ?? new();
            TopCombos = topCombosBySales ?? new();
        }
        /// <summary>
        /// Danh sách sản phẩm bán chạy nhất
        /// </summary>
        public List<TopProduct> TopProducts { get; set; } = new(); // Danh sách sản phẩm có doanh thu cao nhất
        public List<TopCustomer> TopCustomers { get; set; } = new(); // Danh sách khách hàng theo địa điểm
        public List<TopEmployee> TopEmployees { get; set; } = new();
        public List<TopCombo> TopCombos { get; set; } = new(); // Danh sách combo có doanh số cao nhất
    }
}