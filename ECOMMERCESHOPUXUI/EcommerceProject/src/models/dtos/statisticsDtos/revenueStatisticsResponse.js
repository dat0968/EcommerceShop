export class RevenueStatisticsResponse {
  constructor() {
    this.totalRevenue = 0.0 // Tổng doanh thu
    this.averageDailyRevenue = 0.0 // Doanh thu trung bình hàng ngày
    this.averageMonthlyRevenue = 0.0 // Doanh thu trung bình hàng tháng
    this.highestRevenue = 0.0 // Doanh thu cao nhất
    this.lowestRevenue = 0.0 // Doanh thu thấp nhất
    this.revenueByDate = [] // Doanh thu theo ngày
    this.revenueByMonth = [] // Doanh thu theo tháng
    this.revenueByYear = [] // Doanh thu theo năm
    this.topProductByRevenue = [] // Danh sách sản phẩm có doanh thu cao nhất
    this.topCustomerByRevenue = [] // Danh sách khách hàng có doanh thu cao nhất
  }

  static fromApiResponse(data) {
    const response = new RevenueStatisticsResponse()
    response.totalRevenue = data.totalRevenue || 0.0
    response.averageDailyRevenue = data.averageDailyRevenue || 0.0
    response.averageMonthlyRevenue = data.averageMonthlyRevenue || 0.0
    response.highestRevenue = data.highestRevenue || 0.0
    response.lowestRevenue = data.lowestRevenue || 0.0

    // Ánh xạ doanh thu theo ngày
    response.revenueByDate = data.revenueByDate.map((revenue) => {
      return RevenueByDate.fromApiResponse(revenue)
    })

    // Ánh xạ doanh thu theo tháng
    response.revenueByMonth = data.revenueByMonth.map((revenue) => {
      return RevenueByMonth.fromApiResponse(revenue)
    })

    // Ánh xạ doanh thu theo năm
    response.revenueByYear = data.revenueByYear.map((revenue) => {
      return RevenueByYear.fromApiResponse(revenue)
    })

    // Ánh xạ sản phẩm có doanh thu cao nhất
    response.topProductByRevenue = data.topProductByRevenue.map((product) => {
      return TopProductByRevenue.fromApiResponse(product)
    })

    // Ánh xạ khách hàng có doanh thu cao nhất
    response.topCustomerByRevenue = data.topCustomerByRevenue.map((customer) => {
      return TopCustomerByRevenue.fromApiResponse(customer)
    })

    return response
  }
}

class RevenueByDate {
  constructor() {
    this.date = new Date() // Ngày
    this.revenue = 0.0 // Doanh thu trong ngày này
    this.count = 0 // Số lượng đơn hàng trong ngày này
  }

  static fromApiResponse(data) {
    const revenue = new RevenueByDate()
    revenue.date = new Date(data.date) // || new Date()
    revenue.revenue = data.revenue || 0.0
    revenue.count = data.count || 0
    return revenue
  }
}

class RevenueByMonth {
  constructor() {
    this.month = 0 // Tháng
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu trong tháng này
    this.count = 0 // Số lượng đơn hàng trong tháng này
  }

  static fromApiResponse(data) {
    const revenue = new RevenueByMonth()
    revenue.month = data.month || 0
    revenue.year = data.year || 0
    revenue.revenue = data.revenue || 0.0
    revenue.count = data.count || 0
    return revenue
  }
}

class RevenueByYear {
  constructor() {
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu trong năm này
    this.count = 0 // Số lượng đơn hàng trong năm này
  }

  static fromApiResponse(data) {
    const revenue = new RevenueByYear()
    revenue.year = data.year || 0
    revenue.revenue = data.revenue || 0.0
    revenue.count = data.count || 0
    return revenue
  }
}

class TopProductByRevenue {
  constructor() {
    this.productId = 0 // Mã sản phẩm
    this.productName = '' // Tên sản phẩm
    this.revenue = 0.0 // Doanh thu từ sản phẩm này
  }

  static fromApiResponse(data) {
    const product = new TopProductByRevenue()
    product.productId = data.productId || 0
    product.productName = data.productName || ''
    product.revenue = data.revenue || 0.0
    return product
  }
}

class TopCustomerByRevenue {
  constructor() {
    this.customerId = 0 // Mã khách hàng
    this.customerName = '' // Tên khách hàng
    this.revenue = 0.0 // Doanh thu từ khách hàng này
  }

  static fromApiResponse(data) {
    const customer = new TopCustomerByRevenue()
    customer.customerId = data.customerId || 0
    customer.customerName = data.customerName || ''
    customer.revenue = data.revenue || 0.0
    return customer
  }
}
