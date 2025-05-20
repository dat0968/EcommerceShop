<template>
  <div>
    <code>
      OrderSummary:
      {{ orderSummaryData }}
      <hr />
      ProductStatistic:
      {{ productStatisticData }}
      <hr />
      CustomerStatistic:
      {{ customerStatisticsData }}
      <hr />
      EmployeeStatistic:
      {{ employeeStatisticsData }}
      <hr />
      RevenueStatistic:
      {{ revenueStatisticData }}
      <hr />
      ComboStatictis:
      {{ comboStatisticsaryData }}
    </code>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'

import { OrderSummaryResponse } from '@/models/dtos/statisticsDtos/orderSummaryResponse'

import toastr from 'toastr'
import { CustomerStatisticsResponse } from '@/models/dtos/statisticsDtos/customerStatisticsResponse'
import { ProductStatisticsResponse } from '@/models/dtos/statisticsDtos/productStatisticsResponse'
import { EmployeeStatisticsResponse } from '@/models/dtos/statisticsDtos/employeeStatisticsResponse'
import { RevenueStatisticsResponse } from '@/models/dtos/statisticsDtos/revenueStatisticsResponse'
import { ComboStatisticsResponse } from '@/models/dtos/statisticsDtos/comboStatisticsResponse'
export default {
  name: 'OrderSummary',
  components: {},
  props: {},
  data() {
    return {
      orderSummaryData: {},
      productStatisticData: {},
      customerStatisticsData: {},
      employeeStatisticsData: {},
      revenueStatisticData: {},
      comboStatisticsaryData: {},
    }
  },
  computed: {},
  watch: {},
  async mounted() {
    let errorMessage = ''
    let errorLogs = []

    try {
      await this.loadOrderSummaryData()
    } catch (error) {
      errorMessage += 'Đơn hàng.'
      errorLogs.push(error)
    }
    try {
      await this.loadProductStatisticsData()
    } catch (error) {
      errorMessage += 'Sản phẩm.'
      errorLogs.push(error)
    }
    try {
      await this.loadCustomerStatisticsData()
    } catch (error) {
      errorMessage += 'Khách hàng.'
      errorLogs.push(error)
    }
    try {
      await this.loadEmployeeStatisticsData()
    } catch (error) {
      errorMessage += 'Nhân viên.'
      errorLogs.push(error)
    }
    try {
      await this.loadRevenueStatisticsData()
    } catch (error) {
      errorMessage += 'Doanh thu.'
      errorLogs.push(error)
    }
    try {
      await this.loadComboStatisticsData()
    } catch (error) {
      errorMessage += 'Combo.'
      errorLogs.push(error)
    }

    if (errorMessage === '') {
      toastr.error('Hiện không thể load dữ liệu: ' + errorMessage)
      console.warn(errorLogs)
    }
  },
  methods: {
    async loadOrderSummaryData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetOrderSummary',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.orderSummaryData = OrderSummaryResponse.fromApiResponse(response.data)
    },
    async loadProductStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetProductStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.productStatisticData = ProductStatisticsResponse.fromApiResponse(response.data)
    },
    async loadCustomerStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCustomerStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.customerStatisticsData = CustomerStatisticsResponse.fromApiResponse(response.data)
    },
    async loadEmployeeStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetEmployeeStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.employeeStatisticsData = EmployeeStatisticsResponse.fromApiResponse(response.data)
    },
    async loadRevenueStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetRevenueStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.revenueStatisticData = RevenueStatisticsResponse.fromApiResponse(response.data)
    },
    async loadComboStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetComboStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      this.comboStatisticsaryData = ComboStatisticsResponse.fromApiResponse(response.data)
    },
  },
}
</script>

<style scoped></style>
