import {
  TopProductByRevenue,
  BestSellingProduct,
  ProductCategoryStatistics,
  TopProductByOrder,
  TopCustomerByOrders,
  EmployeeByDepartment,
  EmployeeByPosition,
  TopEmployeeByPerformance,
  TopEmployeeBySales,
  CustomerByLocation,
  CustomerByAgeGroup,
  TopComboBySales,
  TopComboByRevenue,
} from './subStatistics'

export default class DatatableStatisticsResponse {
  constructor({
    topProductByRevenue = [],
    bestSellingProducts = [],
    productCategoryStatistics = [],
    topProductByOrder = [],
    topCustomerByOrder = [],
    employeesByDepartment = [],
    employeesByPosition = [],
    topEmployeesByPerformance = [],
    topEmployeesBySales = [],
    customersByLocation = [],
    customersByAgeGroup = [],
    topCombosBySales = [],
    topCombosByRevenue = [],
  } = {}) {
    this.topProductByRevenue = topProductByRevenue
    this.bestSellingProducts = bestSellingProducts
    this.productCategoryStatistics = productCategoryStatistics
    this.topProductByOrder = topProductByOrder
    this.topCustomerByOrder = topCustomerByOrder
    this.employeesByDepartment = employeesByDepartment
    this.employeesByPosition = employeesByPosition
    this.topEmployeesByPerformance = topEmployeesByPerformance
    this.topEmployeesBySales = topEmployeesBySales
    this.customersByLocation = customersByLocation
    this.customersByAgeGroup = customersByAgeGroup
    this.topCombosBySales = topCombosBySales
    this.topCombosByRevenue = topCombosByRevenue
  }
  static fromApiResponse(data = {}) {
    const instance = new DatatableStatisticsResponse()
    instance.topProductByRevenue = (data.topProductByRevenue || []).map((item) =>
      TopProductByRevenue.fromApiResponse(item),
    )
    instance.bestSellingProducts = (data.bestSellingProduct || []).map((item) =>
      BestSellingProduct.fromApiResponse(item),
    )
    instance.productCategoryStatistics = (data.productCategoryStatistics || []).map((item) =>
      ProductCategoryStatistics.fromApiResponse(item),
    )
    instance.topProductByOrder = (data.topProductByOrder || []).map((item) =>
      TopProductByOrder.fromApiResponse(item),
    )
    instance.topCustomerByOrder = (data.topCustomerByOrders || []).map((item) =>
      TopCustomerByOrders.fromApiResponse(item),
    )
    instance.employeesByDepartment = (data.employeeByDepartment || []).map((item) =>
      EmployeeByDepartment.fromApiResponse(item),
    )
    instance.employeesByPosition = (data.employeeByPosition || []).map((item) =>
      EmployeeByPosition.fromApiResponse(item),
    )
    instance.topEmployeesByPerformance = (data.topEmployeeByPerformance || []).map((item) =>
      TopEmployeeByPerformance.fromApiResponse(item),
    )
    instance.topEmployeesBySales = (data.topEmployeeBySales || []).map((item) =>
      TopEmployeeBySales.fromApiResponse(item),
    )
    instance.customersByLocation = (data.customerByLocation || []).map((item) =>
      CustomerByLocation.fromApiResponse(item),
    )
    instance.customersByAgeGroup = (data.customerByAgeGroup || []).map((item) =>
      CustomerByAgeGroup.fromApiResponse(item),
    )
    instance.topCombosBySales = (data.topComboBySales || []).map((item) =>
      TopComboBySales.fromApiResponse(item),
    )
    instance.topCombosByRevenue = (data.topComboByRevenue || []).map((item) =>
      TopComboByRevenue.fromApiResponse(item),
    )
    return instance
  }
}
