export default class EmployeeStatisticsResponse {
  constructor() {
    this.totalEmployees = 0 // Tổng số nhân viên
    this.totalActiveEmployees = 0 // Tổng số nhân viên đang hoạt động
    this.totalInactiveEmployees = 0 // Tổng số nhân viên không hoạt động
    this.averageSalary = 0.0 // Lương trung bình của nhân viên
    this.totalSalary = 0.0 // Tổng lương của tất cả nhân viên
  }

  static fromApiResponse(data) {
    const response = new EmployeeStatisticsResponse()
    response.totalEmployees = data.totalEmployees || 0
    response.totalActiveEmployees = data.totalActiveEmployees || 0
    response.totalInactiveEmployees = data.totalInactiveEmployees || 0
    response.averageSalary = data.averageSalary || 0.0
    response.totalSalary = data.totalSalary || 0.0

    return response
  }
}
