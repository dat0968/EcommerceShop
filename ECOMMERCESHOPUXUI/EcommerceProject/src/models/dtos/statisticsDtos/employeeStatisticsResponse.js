export class EmployeeStatisticsResponse {
    constructor() {
        this.totalEmployees = 0; // Tổng số nhân viên
        this.totalActiveEmployees = 0; // Tổng số nhân viên đang hoạt động
        this.totalInactiveEmployees = 0; // Tổng số nhân viên không hoạt động
        this.employeesByDepartment = []; // Danh sách nhân viên theo phòng ban
        this.employeesByPosition = []; // Danh sách nhân viên theo chức vụ
        this.topEmployeesByPerformance = []; // Danh sách nhân viên có hiệu suất cao nhất
        this.topEmployeesBySales = []; // Danh sách nhân viên có doanh số cao nhất
        this.averageSalary = 0.0; // Lương trung bình của nhân viên
        this.totalSalary = 0.0; // Tổng lương của tất cả nhân viên
    }

    static fromApiResponse(data) {
        const response = new EmployeeStatisticsResponse();
        response.totalEmployees = data.totalEmployees || 0;
        response.totalActiveEmployees = data.totalActiveEmployees || 0;
        response.totalInactiveEmployees = data.totalInactiveEmployees || 0;
        response.averageSalary = data.averageSalary || 0.0;
        response.totalSalary = data.totalSalary || 0.0;

        // Ánh xạ nhân viên theo phòng ban
        response.employeesByDepartment = data.employeesByDepartment.map(department => {
            return EmployeeByDepartment.fromApiResponse(department);
        });

        // Ánh xạ nhân viên theo chức vụ
        response.employeesByPosition = data.employeesByPosition.map(position => {
            return EmployeeByPosition.fromApiResponse(position);
        });

        // Ánh xạ nhân viên có hiệu suất cao nhất
        response.topEmployeesByPerformance = data.topEmployeesByPerformance.map(employee => {
            return TopEmployeeByPerformance.fromApiResponse(employee);
        });

        // Ánh xạ nhân viên có doanh số cao nhất
        response.topEmployeesBySales = data.topEmployeesBySales.map(employee => {
            return TopEmployeeBySales.fromApiResponse(employee);
        });

        return response;
    }
}

class EmployeeByDepartment {
    constructor() {
        this.departmentName = ''; // Tên phòng ban
        this.count = 0; // Số lượng nhân viên trong phòng ban này
    }

    static fromApiResponse(data) {
        const department = new EmployeeByDepartment();
        department.departmentName = data.departmentName || '';
        department.count = data.count || 0;
        return department;
    }
}

class EmployeeByPosition {
    constructor() {
        this.positionName = ''; // Tên chức vụ
        this.count = 0; // Số lượng nhân viên có chức vụ này
    }

    static fromApiResponse(data) {
        const position = new EmployeeByPosition();
        position.positionName = data.positionName || '';
        position.count = data.count || 0;
        return position;
    }
}

class TopEmployeeByPerformance {
    constructor() {
        this.employeeId = 0; // Mã nhân viên
        this.employeeName = ''; // Tên nhân viên
        this.performanceScore = 0.0; // Điểm hiệu suất của nhân viên
    }

    static fromApiResponse(data) {
        const employee = new TopEmployeeByPerformance();
        employee.employeeId = data.employeeId || 0;
        employee.employeeName = data.employeeName || '';
        employee.performanceScore = data.performanceScore || 0.0;
        return employee;
    }
}

class TopEmployeeBySales {
    constructor() {
        this.employeeId = 0; // Mã nhân viên
        this.employeeName = ''; // Tên nhân viên
        this.salesAmount = 0.0; // Doanh số của nhân viên
    }

    static fromApiResponse(data) {
        const employee = new TopEmployeeBySales();
        employee.employeeId = data.employeeId || 0;
        employee.employeeName = data.employeeName || '';
        employee.salesAmount = data.salesAmount || 0.0;
        return employee;
    }
}
