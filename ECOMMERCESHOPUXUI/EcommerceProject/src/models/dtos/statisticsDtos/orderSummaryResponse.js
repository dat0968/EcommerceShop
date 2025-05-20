import { OrderStatusStatistics, RevenueByTime } from './subStatistics'
export default class OrderSummaryResponse {
  constructor() {
    this.totalOrders = 0 // Tổng số đơn hàng
    this.totalRevenue = 0.0 // Tổng doanh thu
    this.averageOrderValue = 0.0 // Giá trị trung bình của mỗi đơn hàng
    this.totalCustomers = 0 // Tổng số khách hàng
    this.totalProducts = 0 // Tổng số sản phẩm được bán
    this.totalDiscount = 0.0 // Tổng số tiền giảm giá
    this.totalShippingFee = 0.0 // Tổng số tiền phí vận chuyển
    this.orderStatusStatistics = [] // Thống kê trạng thái đơn hàng
    this.revenueByTimes = {} // Doanh thu theo thời gian
  }

  static fromApiResponse(data) {
    const response = new OrderSummaryResponse()
    response.totalOrders = data.totalOrders || 0
    response.totalRevenue = data.totalRevenue || 0.0
    response.averageOrderValue = data.averageOrderValue || 0.0
    response.totalCustomers = data.totalCustomers || 0
    response.totalProducts = data.totalProducts || 0
    response.totalDiscount = data.totalDiscount || 0.0
    response.totalShippingFee = data.totalShippingFee || 0.0

    // Ánh xạ thống kê trạng thái đơn hàng
    response.orderStatusStatistics = data.orderStatusStatistics.map((status) => {
      const orderStatus = new OrderStatusStatistics()
      orderStatus.status = status.status || ''
      orderStatus.count = status.count || 0
      return orderStatus
    })

    // Ánh xạ doanh thu theo thời gian
    response.revenueByTimes = data.revenueByTime || {} // sử dụng dữ liệu từ API một cách trực tiếp

    // Kiểm tra và đảm bảo cấu trúc đúng
    Object.keys(response.revenueByTimes).forEach((key) => {
      response.revenueByTimes[key] = response.revenueByTimes[key].map((revenue) => {
        const revenueByTime = new RevenueByTime()
        revenueByTime.date = revenue.date || new Date() // Ngày
        revenueByTime.month = revenue.month || 0
        revenueByTime.year = revenue.year || 0
        revenueByTime.revenue = revenue.revenue || 0.0
        revenueByTime.count = revenue.count || 0
        return revenueByTime
      })
    })

    return response
  }
}
