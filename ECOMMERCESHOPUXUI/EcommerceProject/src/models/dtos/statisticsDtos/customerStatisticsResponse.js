export class CustomerStatisticsResponse {
  constructor() {
    this.totalCustomers = 0 // Tổng số khách hàng
    this.totalActiveCustomers = 0 // Tổng số khách hàng đang hoạt động
    this.totalInactiveCustomers = 0 // Tổng số khách hàng không hoạt động
    this.customersByLocation = [] // Danh sách khách hàng theo địa điểm
    this.customersByAgeGroup = [] // Danh sách khách hàng theo nhóm tuổi
    this.topCustomersByPurchase = [] // Danh sách khách hàng có số lần mua hàng cao nhất
    this.topCustomersByRevenue = [] // Danh sách khách hàng có doanh thu cao nhất
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

    // Ánh xạ khách hàng theo địa điểm
    response.customersByLocation = data.customersByLocation.map((location) => {
      const customerByLocation = new CustomerByLocation()
      customerByLocation.location = location.location || ''
      customerByLocation.count = location.count || 0
      return customerByLocation
    })

    // Ánh xạ khách hàng theo nhóm tuổi
    response.customersByAgeGroup = data.customersByAgeGroup.map((ageGroup) => {
      const customerByAgeGroup = new CustomerByAgeGroup()
      customerByAgeGroup.ageGroup = ageGroup.ageGroup || ''
      customerByAgeGroup.count = ageGroup.count || 0
      return customerByAgeGroup
    })

    // Ánh xạ khách hàng có số lần mua hàng cao nhất
    response.topCustomersByPurchase = data.topCustomersByPurchase.map((customer) => {
      const topCustomerByPurchase = new TopCustomerByPurchase()
      topCustomerByPurchase.customerId = customer.customerId || 0
      topCustomerByPurchase.customerName = customer.customerName || ''
      topCustomerByPurchase.purchaseCount = customer.purchaseCount || 0
      return topCustomerByPurchase
    })

    // Ánh xạ khách hàng có doanh thu cao nhất
    response.topCustomersByRevenue = data.topCustomersByRevenue.map((customer) => {
      const topCustomerByRevenue = new TopCustomerByRevenue()
      topCustomerByRevenue.customerId = customer.customerId || 0
      topCustomerByRevenue.customerName = customer.customerName || ''
      topCustomerByRevenue.revenue = customer.revenue || 0.0
      return topCustomerByRevenue
    })

    return response
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

class TopCustomerByPurchase {
  constructor() {
    this.customerId = 0 // Mã khách hàng
    this.customerName = '' // Tên khách hàng
    this.purchaseCount = 0 // Số lần mua hàng của khách hàng
  }

  static fromApiResponse(data) {
    const customer = new TopCustomerByPurchase()
    customer.customerId = data.customerId || 0
    customer.customerName = data.customerName || ''
    customer.purchaseCount = data.purchaseCount || 0
    return customer
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
