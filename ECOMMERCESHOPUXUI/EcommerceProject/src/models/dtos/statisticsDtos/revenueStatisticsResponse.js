export default class RevenueStatisticsResponse {
  constructor() {
    this.totalRevenue = 0.0 // Tổng doanh thu
    this.averageDailyRevenue = 0.0 // Doanh thu trung bình hàng ngày
    this.averageMonthlyRevenue = 0.0 // Doanh thu trung bình hàng tháng
    this.highestRevenue = 0.0 // Doanh thu cao nhất
    this.lowestRevenue = 0.0 // Doanh thu thấp nhất
  }

  static fromApiResponse(data) {
    const response = new RevenueStatisticsResponse()
    response.totalRevenue = data.totalRevenue || 0.0
    response.averageDailyRevenue = data.averageDailyRevenue || 0.0
    response.averageMonthlyRevenue = data.averageMonthlyRevenue || 0.0
    response.highestRevenue = data.highestRevenue || 0.0
    response.lowestRevenue = data.lowestRevenue || 0.0

    return response
  }
}
