using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIClothesEcommerceShop.DTO.Statistics.Sub
{

    public class TopEmployee
    {
        public int EmployeeId { get; set; } // Mã nhân viên
        public string EmployeeName { get; set; } = string.Empty; // Tên nhân viên
        public decimal PerformanceScore { get; set; } // Điểm hiệu suất của nhân viên
        public string PositionName { get; set; } = string.Empty; // Tên chức vụ
        public string DepartmentName { get; set; } = string.Empty; // Tên phòng ban
        public decimal SalesAmount { get; set; } // Doanh số của nhân viên
        public int Count { get; set; } // Số lượng nhân viên trong phòng ban này
        public List<OrderRecentTopUser> OrderRecents { get; set; } = new();
    }
}