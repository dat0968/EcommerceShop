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

class CustomerByLocation {
  constructor() {
    this.location = '' // Địa điểm
    this.count = 0 // Số lượng khách hàng tại địa điểm này
  }

  static fromApiResponse(data) {
    const location = new CustomerByLocation()
    location.location = data.location || ''
    location.count = data.count || 0
    return location
  }
}

class CustomerByAgeGroup {
  constructor() {
    this.ageGroup = '' // Nhóm tuổi
    this.count = 0 // Số lượng khách hàng trong nhóm tuổi này
  }

  static fromApiResponse(data) {
    const ageGroup = new CustomerByAgeGroup()
    ageGroup.ageGroup = data.ageGroup || ''
    ageGroup.count = data.count || 0
    return ageGroup
  }
}

class TopCustomerByOrders {
  constructor() {
    this.customerId = 0 // Mã khách hàng
    this.customerName = '' // Tên khách hàng
    this.count = 0 // Số lần mua hàng của khách hàng
    this.revenue = 0.0
  }

  static fromApiResponse(data) {
    const customer = new TopCustomerByOrders()
    customer.customerId = data.customerId || 0
    customer.customerName = data.customerName || ''
    customer.count = data.count || 0
    customer.revenue = data.revenue
    return customer
  }
}

class EmployeeByDepartment {
  constructor() {
    this.departmentName = '' // Tên phòng ban
    this.count = 0 // Số lượng nhân viên trong phòng ban này
  }

  static fromApiResponse(data) {
    const department = new EmployeeByDepartment()
    department.departmentName = data.departmentName || ''
    department.count = data.count || 0
    return department
  }
}

class EmployeeByPosition {
  constructor() {
    this.positionName = '' // Tên chức vụ
    this.count = 0 // Số lượng nhân viên có chức vụ này
  }

  static fromApiResponse(data) {
    const position = new EmployeeByPosition()
    position.positionName = data.positionName || ''
    position.count = data.count || 0
    return position
  }
}

class TopEmployeeByPerformance {
  constructor() {
    this.employeeId = 0 // Mã nhân viên
    this.employeeName = '' // Tên nhân viên
    this.performanceScore = 0.0 // Điểm hiệu suất của nhân viên
  }

  static fromApiResponse(data) {
    const employee = new TopEmployeeByPerformance()
    employee.employeeId = data.employeeId || 0
    employee.employeeName = data.employeeName || ''
    employee.performanceScore = data.performanceScore || 0.0
    return employee
  }
}

class TopEmployeeBySales {
  constructor() {
    this.employeeId = 0 // Mã nhân viên
    this.employeeName = '' // Tên nhân viên
    this.salesAmount = 0.0 // Doanh số của nhân viên
  }

  static fromApiResponse(data) {
    const employee = new TopEmployeeBySales()
    employee.employeeId = data.employeeId || 0
    employee.employeeName = data.employeeName || ''
    employee.salesAmount = data.salesAmount || 0.0
    return employee
  }
}

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

class TopProductByOrder {
  constructor() {
    this.productId = 0 // Mã sản phẩm
    this.productName = '' // Tên sản phẩm
    this.count = 0 // Số lượng đơn hàng có sản phẩm này
  }

  static fromApiResponse(data) {
    const product = new TopProductByOrder()
    product.productId = data.productId || 0
    product.productName = data.productName || ''
    product.count = data.count || 0
    return product
  }
}
class ProductCategoryStatistics {
  constructor() {
    this.categoryName = '' // Tên danh mục
    this.totalProducts = 0 // Tổng số sản phẩm trong danh mục này
    this.totalRevenue = 0.0 // Tổng doanh thu từ danh mục này
  }

  static fromApiResponse(data) {
    const category = new ProductCategoryStatistics()
    category.categoryName = data.categoryName || ''
    category.totalProducts = data.totalProducts || 0
    category.totalRevenue = data.totalRevenue || 0.0
    return category
  }
}

class SalesByDate {
  constructor() {
    this.date = new Date() // Ngày
    this.revenue = 0.0 // Doanh thu từ sản phẩm trong ngày này
    this.count = 0 // Số lượng đơn hàng trong ngày này
  }

  static fromApiResponse(data) {
    const sale = new SalesByDate()
    sale.date = new Date(data.date) // || new Date()
    sale.revenue = data.revenue || 0.0
    sale.count = data.count || 0
    return sale
  }
}

class SalesByMonth {
  constructor() {
    this.month = 0 // Tháng
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu từ sản phẩm trong tháng này
    this.count = 0 // Số lượng đơn hàng trong tháng này
  }

  static fromApiResponse(data) {
    const sale = new SalesByMonth()
    sale.month = data.month || 0
    sale.year = data.year || 0
    sale.revenue = data.revenue || 0.0
    sale.count = data.count || 0
    return sale
  }
}

class SalesByYear {
  constructor() {
    this.year = 0 // Năm
    this.revenue = 0.0 // Doanh thu từ sản phẩm trong năm này
    this.count = 0 // Số lượng đơn hàng trong năm này
  }

  static fromApiResponse(data) {
    const sale = new SalesByYear()
    sale.year = data.year || 0
    sale.revenue = data.revenue || 0.0
    sale.count = data.count || 0
    return sale
  }
}

class BestSellingProduct {
  constructor() {
    this.productId = 0 // Mã sản phẩm
    this.productName = '' // Tên sản phẩm
    this.quantity = 0 // Số lượng sản phẩm được bán
    this.revenue = 0.0 // Doanh thu từ sản phẩm này
  }

  static fromApiResponse(data) {
    const product = new BestSellingProduct()
    product.productId = data.productId || 0
    product.productName = data.productName || ''
    product.quantity = data.quantity || 0
    product.revenue = data.revenue || 0.0
    return product
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

// Nếu muốn default export dạng object:
export {
  TopComboByRevenue,
  TopComboBySales,
  CustomerByLocation,
  CustomerByAgeGroup,
  TopCustomerByOrders,
  EmployeeByDepartment,
  EmployeeByPosition,
  TopEmployeeByPerformance,
  TopEmployeeBySales,
  OrderStatusStatistics,
  RevenueByTime,
  TopProductByOrder,
  ProductCategoryStatistics,
  SalesByDate,
  SalesByMonth,
  SalesByYear,
  BestSellingProduct,
  TopProductByRevenue,
  TopCustomerByRevenue,
}
