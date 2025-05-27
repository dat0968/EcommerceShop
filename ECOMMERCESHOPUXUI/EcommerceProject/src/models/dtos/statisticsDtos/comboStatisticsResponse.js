export default class ComboStatisticsResponse {
  constructor() {
    this.totalCombos = 0 // Tổng số combo
    this.totalActiveCombos = 0 // Tổng số combo đang hoạt động
    this.totalInactiveCombos = 0 // Tổng số combo không hoạt động
    this.averageComboPrice = 0.0 // Giá trung bình của combo
    this.totalComboRevenue = 0.0 // Tổng doanh thu từ combo
  }

  static fromApiResponse(data) {
    const response = new ComboStatisticsResponse()
    response.totalCombos = data.totalCombos || 0
    response.totalActiveCombos = data.totalActiveCombos || 0
    response.totalInactiveCombos = data.totalInactiveCombos || 0
    response.averageComboPrice = data.averageComboPrice || 0.0
    response.totalComboRevenue = data.totalComboRevenue || 0.0

    return response
  }
}
