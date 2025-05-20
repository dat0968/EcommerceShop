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

    public class EmployeeByDepartment
    {
        public string DepartmentName { get; set; } = string.Empty; // Tên phòng ban
        public int Count { get; set; } // Số lượng nhân viên trong phòng ban này
    }

    public class EmployeeByPosition
    {
        public string PositionName { get; set; } = string.Empty; // Tên chức vụ
        public int Count { get; set; } // Số lượng nhân viên có chức vụ này
    }

    public class TopEmployeeByPerformance
    {
        public int EmployeeId { get; set; } // Mã nhân viên
        public string EmployeeName { get; set; } = string.Empty; // Tên nhân viên
        public decimal PerformanceScore { get; set; } // Điểm hiệu suất của nhân viên
    }

    public class TopEmployeeBySales
    {
        public int EmployeeId { get; set; } // Mã nhân viên
        public string EmployeeName { get; set; } = string.Empty; // Tên nhân viên
        public decimal SalesAmount { get; set; } // Doanh số của nhân viên
    }
}