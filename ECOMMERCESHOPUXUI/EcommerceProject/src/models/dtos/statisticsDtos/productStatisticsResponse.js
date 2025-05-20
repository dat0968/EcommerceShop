export default class ProductStatisticsResponse {
  constructor() {
    this.totalProducts = 0 // Tổng số sản phẩm
    this.totalActiveProducts = 0 // Tổng số sản phẩm đang hoạt động
    this.totalInactiveProducts = 0 // Tổng số sản phẩm không hoạt động
    this.totalRevenue = 0.0 // Tổng doanh thu từ sản phẩm
    this.totalDiscount = 0.0 // Tổng giảm giá từ sản phẩm
    this.averagePrice = 0.0 // Giá trung bình của sản phẩm
  }

  static fromApiResponse(data) {
    const response = new ProductStatisticsResponse()
    response.totalProducts = data.totalProducts || 0
    response.totalActiveProducts = data.totalActiveProducts || 0
    response.totalInactiveProducts = data.totalInactiveProducts || 0
    response.totalRevenue = data.totalRevenue || 0.0
    response.totalDiscount = data.totalDiscount || 0.0
    response.averagePrice = data.averagePrice || 0.0

    return response
  }
}
