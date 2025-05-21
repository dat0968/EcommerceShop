using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics
{
    public class EmployeeStatisticsResponse
    {
        public int TotalEmployees { get; set; } // Tổng số nhân viên
        public int TotalActiveEmployees { get; set; } // Tổng số nhân viên đang hoạt động
        public int TotalInactiveEmployees { get; set; } // Tổng số nhân viên không hoạt động
        public decimal AverageSalary { get; set; } // Lương trung bình của nhân viên
        public decimal TotalSalary { get; set; } // Tổng lương của tất cả nhân viên
    }
}