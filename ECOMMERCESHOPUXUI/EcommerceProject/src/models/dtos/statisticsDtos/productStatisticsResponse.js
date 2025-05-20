export class ProductStatisticsResponse {
  constructor() {
    this.totalProducts = 0 // Tổng số sản phẩm
    this.totalActiveProducts = 0 // Tổng số sản phẩm đang hoạt động
    this.totalInactiveProducts = 0 // Tổng số sản phẩm không hoạt động
    this.totalRevenue = 0.0 // Tổng doanh thu từ sản phẩm
    this.totalDiscount = 0.0 // Tổng giảm giá từ sản phẩm
    this.averagePrice = 0.0 // Giá trung bình của sản phẩm
    this.bestSellingProducts = [] // Danh sách sản phẩm bán chạy nhất
    this.productCategoryStatistics = [] // Thống kê sản phẩm theo danh mục
    this.salesByDate = [] // Doanh thu từ sản phẩm theo ngày
    this.salesByMonth = [] // Doanh thu từ sản phẩm theo tháng
    this.salesByYear = [] // Doanh thu từ sản phẩm theo năm
  }

  static fromApiResponse(data) {
    const response = new ProductStatisticsResponse()
    response.totalProducts = data.totalProducts || 0
    response.totalActiveProducts = data.totalActiveProducts || 0
    response.totalInactiveProducts = data.totalInactiveProducts || 0
    response.totalRevenue = data.totalRevenue || 0.0
    response.totalDiscount = data.totalDiscount || 0.0
    response.averagePrice = data.averagePrice || 0.0

    // Ánh xạ sản phẩm bán chạy nhất
    response.bestSellingProducts = data.bestSellingProducts.map((product) => {
      return BestSellingProduct.fromApiResponse(product)
    })

    // Ánh xạ thống kê sản phẩm theo danh mục
    response.productCategoryStatistics = data.productCategoryStatistics.map((category) => {
      return ProductCategoryStatistics.fromApiResponse(category)
    })

    // Ánh xạ doanh thu từ sản phẩm theo ngày
    response.salesByDate = data.salesByDate.map((sale) => {
      return SalesByDate.fromApiResponse(sale)
    })

    // Ánh xạ doanh thu từ sản phẩm theo tháng
    response.salesByMonth = data.salesByMonth.map((sale) => {
      return SalesByMonth.fromApiResponse(sale)
    })

    // Ánh xạ doanh thu từ sản phẩm theo năm
    response.salesByYear = data.salesByYear.map((sale) => {
      return SalesByYear.fromApiResponse(sale)
    })
    console.log(response)
    return response
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
