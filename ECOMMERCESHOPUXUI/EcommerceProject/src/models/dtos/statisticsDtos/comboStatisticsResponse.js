export class ComboStatisticsResponse {
  constructor() {
    this.totalCombos = 0 // Tổng số combo
    this.totalActiveCombos = 0 // Tổng số combo đang hoạt động
    this.totalInactiveCombos = 0 // Tổng số combo không hoạt động
    this.topCombosBySales = [] // Danh sách combo có doanh số cao nhất
    this.topCombosByRevenue = [] // Danh sách combo có doanh thu cao nhất
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

    // Ánh xạ danh sách combo có doanh số cao nhất
    response.topCombosBySales = data.topCombosBySales.map((combo) => {
      return TopComboBySales.fromApiResponse(combo)
    })

    // Ánh xạ danh sách combo có doanh thu cao nhất
    response.topCombosByRevenue = data.topCombosByRevenue.map((combo) => {
      return TopComboByRevenue.fromApiResponse(combo)
    })

    return response
  }
}

class TopComboBySales {
  constructor() {
    this.comboId = 0 // Mã combo
    this.comboName = '' // Tên combo
    this.salesCount = 0 // Số lượng combo được bán
  }

  static fromApiResponse(data) {
    const combo = new TopComboBySales()
    combo.comboId = data.comboId || 0
    combo.comboName = data.comboName || ''
    combo.salesCount = data.salesCount || 0
    return combo
  }
}

class TopComboByRevenue {
  constructor() {
    this.comboId = 0 // Mã combo
    this.comboName = '' // Tên combo
    this.revenue = 0.0 // Doanh thu từ combo
  }

  static fromApiResponse(data) {
    const combo = new TopComboByRevenue()
    combo.comboId = data.comboId || 0
    combo.comboName = data.comboName || ''
    combo.revenue = data.revenue || 0.0
    return combo
  }
}
