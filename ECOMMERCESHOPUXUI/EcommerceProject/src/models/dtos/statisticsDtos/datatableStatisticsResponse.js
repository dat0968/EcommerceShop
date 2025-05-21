import { TopProduct, TopCustomer, TopEmployee, TopCombo } from './subStatistics'

export default class DatatableStatisticsResponse {
  constructor({ topProducts = [], topCustomers = [], topEmployees = [], topCombos = [] } = {}) {
    this.topProducts = topProducts
    this.topCustomers = topCustomers
    this.topEmployees = topEmployees
    this.topCombos = topCombos
  }

  static fromApiResponse(data = {}) {
    const instance = new DatatableStatisticsResponse()

    instance.topProducts = (data.topProducts || []).map((item) => TopProduct.fromApiResponse(item))

    instance.topCustomers = (data.topCustomers || []).map((item) =>
      TopCustomer.fromApiResponse(item),
    )

    instance.topEmployees = (data.topEmployees || []).map((item) =>
      TopEmployee.fromApiResponse(item),
    )

    instance.topCombos = (data.topCombos || []).map((item) => TopCombo.fromApiResponse(item))

    return instance
  }
}
