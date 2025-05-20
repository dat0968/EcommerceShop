export default class CustomerStatisticsResponse {
  constructor() {
    this.totalCustomers = 0 // Tổng số khách hàng
    this.totalActiveCustomers = 0 // Tổng số khách hàng đang hoạt động
    this.totalInactiveCustomers = 0 // Tổng số khách hàng không hoạt động
    this.averagePurchaseAmount = 0.0 // Số tiền mua hàng trung bình của khách hàng
    this.totalPurchaseAmount = 0.0 // Tổng số tiền mua hàng của tất cả khách hàng
  }

  static fromApiResponse(data) {
    const response = new CustomerStatisticsResponse()
    response.totalCustomers = data.totalCustomers || 0
    response.totalActiveCustomers = data.totalActiveCustomers || 0
    response.totalInactiveCustomers = data.totalInactiveCustomers || 0
    response.averagePurchaseAmount = data.averagePurchaseAmount || 0.0
    response.totalPurchaseAmount = data.totalPurchaseAmount || 0.0

    return response
  }
}
