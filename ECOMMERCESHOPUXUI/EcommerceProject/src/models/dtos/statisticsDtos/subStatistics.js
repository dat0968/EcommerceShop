class OrderStatusStatistics {
  constructor() {
    this.status = '' // Trạng thái đơn hàng (ví dụ: "Đang xử lý", "Đã giao hàng",...)
    this.count = 0 // Số lượng đơn hàng có trạng thái này
  }

  static fromApiResponse(data) {
    const status = new OrderStatusStatistics()
    status.status = data.status || ''
    status.count = data.count || 0
    return status
  }
}

class RevenueByTime {
  constructor() {
    this.date = new Date() // Ngày
    this.month = 0
    this.year = 0
    this.revenue = 0.0 // Doanh thu trong ngày này
    this.count = 0 // Số lượng đơn hàng trong ngày này
  }

  static fromApiResponse(data) {
    const revenue = new RevenueByTime()
    revenue.date = data.date ? new Date(data.date) : new Date()
    revenue.month = data.month || 0
    revenue.year = data.year || 0
    revenue.revenue = data.revenue || 0.0
    revenue.count = data.count || 0
    return revenue
  }
}

class SalesByTimes {
  constructor() {
    this.date = new Date() // Ngày
    this.month = 0
    this.year = 0
    this.revenue = 0.0 // Doanh thu từ sản phẩm trong ngày này
    this.count = 0 // Số lượng đơn hàng trong ngày này
  }

  static fromApiResponse(data) {
    const sale = new SalesByTimes()
    sale.date = new Date(data.date) // || new Date()
    sale.month = data.month || 0
    sale.year = data.year || 0
    sale.revenue = data.revenue || 0.0
    sale.count = data.count || 0
    return sale
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
// TopProduct.js
class TopProduct {
  constructor({
    productId = 0,
    productName = '',
    categoryName = '',
    revenue = 0,
    count = 0,
    detailTopProducts = [],
  } = {}) {
    this.productId = productId // Mã sản phẩm
    this.productName = productName // Tên sản phẩm
    this.categoryName = categoryName // Tên danh mục sản phẩm
    this.revenue = revenue // Doanh thu từ sản phẩm này
    this.count = count // Số lượng bán được
    this.detailTopProducts = detailTopProducts
  }

  static fromApiResponse(data = {}) {
    return new TopProduct({
      productId: data.productId,
      productName: data.productName,
      categoryName: data.categoryName,
      revenue: data.revenue,
      count: data.count,
      detailTopProducts: data.detailTopProducts,
    })
  }
}

// TopCustomer.js
class TopCustomer {
  constructor({
    customerId = 0,
    customerName = '',
    revenue = 0,
    location = '',
    ageGroup = '',
    count = 0,
    orderRecents = [],
  } = {}) {
    this.customerId = customerId // Mã khách hàng
    this.customerName = customerName // Tên khách hàng
    this.revenue = revenue // Doanh thu từ khách hàng này
    this.location = location // Địa điểm
    this.ageGroup = ageGroup // Nhóm tuổi
    this.count = count // Số lượng khách hàng tại địa điểm này
    this.orderRecents = orderRecents
  }

  static fromApiResponse(data = {}) {
    return new TopCustomer({
      customerId: data.customerId,
      customerName: data.customerName,
      revenue: data.revenue,
      location: data.location,
      ageGroup: data.ageGroup,
      count: data.count,
      orderRecents: data.orderRecents,
    })
  }
}

// TopEmployee.js
class TopEmployee {
  constructor({
    employeeId = 0,
    employeeName = '',
    performanceScore = 0,
    positionName = '',
    departmentName = '',
    salesAmount = 0,
    count = 0,
    orderRecents = [],
  } = {}) {
    this.employeeId = employeeId // Mã nhân viên
    this.employeeName = employeeName // Tên nhân viên
    this.performanceScore = performanceScore // Điểm hiệu suất của nhân viên
    this.positionName = positionName // Tên chức vụ
    this.departmentName = departmentName // Tên phòng ban
    this.salesAmount = salesAmount // Doanh số của nhân viên
    this.count = count // Số lượng nhân viên trong phòng ban này
    this.orderRecents = orderRecents
  }

  static fromApiResponse(data = {}) {
    return new TopEmployee({
      employeeId: data.employeeId,
      employeeName: data.employeeName,
      performanceScore: data.performanceScore,
      positionName: data.positionName,
      departmentName: data.departmentName,
      salesAmount: data.salesAmount,
      count: data.count,
      orderRecents: data.orderRecents,
    })
  }
}

// TopCombo.js
class TopCombo {
  constructor({
    comboId = 0,
    comboName = '',
    salesCount = 0,
    revenue = 0,
    detailTopCombos = [],
  } = {}) {
    this.comboId = comboId // Mã combo
    this.comboName = comboName // Tên combo
    this.salesCount = salesCount // Số lượng combo được bán
    this.revenue = revenue // Doanh thu từ combo
    this.detailTopCombos = detailTopCombos
  }

  static fromApiResponse(data = {}) {
    return new TopCombo({
      comboId: data.comboId,
      comboName: data.comboName,
      salesCount: data.salesCount,
      revenue: data.revenue,
      detailTopCombos: data.detailTopCombos,
    })
  }
}

// Nếu muốn default export dạng object:
export {
  OrderStatusStatistics,
  RevenueByTime,
  SalesByTimes,
  TopCustomerByRevenue,
  TopProduct,
  TopCustomer,
  TopEmployee,
  TopCombo,
}
