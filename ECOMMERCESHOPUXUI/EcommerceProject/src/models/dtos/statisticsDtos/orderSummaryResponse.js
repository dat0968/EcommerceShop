export class OrderSummaryResponse {
  constructor() {
    this.totalOrders = 0 // Tổng số đơn hàng
    this.totalRevenue = 0.0 // Tổng doanh thu
    this.averageOrderValue = 0.0 // Giá trị trung bình của mỗi đơn hàng
    this.totalCustomers = 0 // Tổng số khách hàng
    this.totalProducts = 0 // Tổng số sản phẩm được bán
    this.totalDiscount = 0.0 // Tổng số tiền giảm giá
    this.totalShippingFee = 0.0 // Tổng số tiền phí vận chuyển
    this.orderStatusStatistics = [] // Thống kê trạng thái đơn hàng
    this.bestSellingProducts = [] // Danh sách sản phẩm bán chạy nhất
    this.revenueByDate = [] // Doanh thu theo ngày
    this.revenueByMonth = [] // Doanh thu theo tháng
    this.revenueByYear = [] // Doanh thu theo năm
    this.topProductByOrder = [] // Danh sách sản phẩm được đặt hàng nhiều nhất
    this.topCustomerByOrder = [] // Danh sách khách hàng đặt hàng nhiều nhất
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

    // Ánh xạ sản phẩm bán chạy nhất
    response.bestSellingProducts = data.bestSellingProducts.map((product) => {
      const bestSellingProduct = new BestSellingProduct()
      bestSellingProduct.productId = product.productId || 0
      bestSellingProduct.productName = product.productName || ''
      bestSellingProduct.quantity = product.quantity || 0
      bestSellingProduct.revenue = product.revenue || 0.0
      return bestSellingProduct
    })

    // Ánh xạ doanh thu theo ngày
    response.revenueByDate = data.revenueByDate.map((revenue) => {
      const revenueByDate = new RevenueByDate()
      revenueByDate.date = new Date(revenue.date) // || new Date();
      revenueByDate.revenue = revenue.revenue || 0.0
      revenueByDate.count = revenue.count || 0
      return revenueByDate
    })

    // Ánh xạ doanh thu theo tháng
    response.revenueByMonth = data.revenueByMonth.map((revenue) => {
      const revenueByMonth = new RevenueByMonth()
      revenueByMonth.month = revenue.month || 0
      revenueByMonth.year = revenue.year || 0
      revenueByMonth.revenue = revenue.revenue || 0.0
      revenueByMonth.count = revenue.count || 0
      return revenueByMonth
    })

    // Ánh xạ doanh thu theo năm
    response.revenueByYear = data.revenueByYear.map((revenue) => {
      const revenueByYear = new RevenueByYear()
      revenueByYear.year = revenue.year || 0
      revenueByYear.revenue = revenue.revenue || 0.0
      revenueByYear.count = revenue.count || 0
      return revenueByYear
    })

    // Ánh xạ sản phẩm được đặt hàng nhiều nhất
    response.topProductByOrder = data.topProductByOrder.map((product) => {
      const topProduct = new TopProductByOrder()
      topProduct.productId = product.productId || 0
      topProduct.productName = product.productName || ''
      topProduct.count = product.count || 0
      return topProduct
    })

    // Ánh xạ khách hàng đặt hàng nhiều nhất
    response.topCustomerByOrder = data.topCustomerByOrder.map((customer) => {
      const topCustomer = new TopCustomerByOrder()
      topCustomer.customerId = customer.customerId || 0
      topCustomer.customerName = customer.customerName || ''
      topCustomer.count = customer.count || 0
      return topCustomer
    })

    return response
  }
}

class OrderStatusStatistics {
  constructor() {
    this.status = '' // Trạng thái đơn hàng (ví dụ: "Đang xử lý", "Đã giao hàng",...)
    this.count = 0 // Số lượng đơn hàng có trạng thái này
  }
}

class BestSellingProduct {
  constructor() {
    this.productId = 0 // Mã sản phẩm
    this.productName = '' // Tên sản phẩm
    this.quantity = 0 // Số lượng sản phẩm được bán
    this.revenue = 0.0 // Doanh thu từ sản phẩm này
  }
}

class RevenueByDate {
  constructor() {
    this.date = new Date() // Ngày
    this.revenue = 0.0 // Doanh thu trong ngày này
    this.count = 0 // Số lượng đơn hàng trong ngày này
  }
}

class RevenueByMonth {
  constructor() {
    this.month = 0 // Tháng
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu trong tháng này
    this.count = 0 // Số lượng đơn hàng trong này
  }
}

class RevenueByYear {
  constructor() {
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu trong năm này
    this.count = 0 // Số lượng đơn hàng trong này
  }
}

class TopProductByOrder {
  constructor() {
    this.productId = 0 // Mã sản phẩm
    this.productName = '' // Tên sản phẩm
    this.count = 0 // Số lượng đơn hàng có sản phẩm này
  }
}

class TopCustomerByOrder {
  constructor() {
    this.customerId = 0 // Mã khách hàng
    this.customerName = '' // Tên khách hàng
    this.count = 0 // Số lượng đơn hàng của khách hàng này
  }
}
