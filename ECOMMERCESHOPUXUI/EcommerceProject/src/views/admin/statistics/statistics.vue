<template>
  <!-- Start XP Contentbar -->
  <div style="margin-top: 100px" class="xp-contentbar">
    <!-- Start Widget -->
    <RevenueStatistic :data="revenueStatisticData" :is-loading="isLoading"></RevenueStatistic>

    <!-- Start XP Row -->
    <div class="row align-items-stretch">
      <!-- Start XP Col -->
      <div class="col-md-12 col-lg-12 col-xl-7 m-b-30">
        <ProductStatistic :data="productStatisticData" :is-loading="isLoading"></ProductStatistic>
      </div>
      <!-- End XP Col -->

      <!-- Start XP Col -->
      <div class="col-md-12 col-lg-12 col-xl-5">
        <!-- Start XP Col -->
        <div class="flex-grow-1">
          <EmployeeStatistic
            :data="employeeStatisticsData"
            :is-loading="isLoading"
          ></EmployeeStatistic>
        </div>
        <div class="flex-grow-1">
          <!-- End XP Col -->
          <CustomerStatistic
            :data="customerStatisticsData"
            :is-loading="isLoading"
          ></CustomerStatistic>
        </div>
      </div>
      <!-- End XP Col -->
    </div>
    <!-- End XP Row -->

    <!-- End XP Row -->
    <OrderSummary :data="orderSummaryData" :is-loading="isLoading"></OrderSummary>

    <!-- End XP Row -->

    <!-- Start Project -->
    <!-- End XP Row -->
    <DatatableStatistic
      :data="datatableStatisticsResponse"
      :is-loading="isLoading"
    ></DatatableStatistic>
    <!-- End XP Row -->
  </div>
  <!-- End XP Contentbar -->

  <div class="">
    <ComboStatistic :data="comboStatisticsaryData" :is-loading="isLoading"></ComboStatistic>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'

import toastr from 'toastr'

import OrderSummaryResponse from '@/models/dtos/statisticsDtos/orderSummaryResponse'
import CustomerStatisticsResponse from '@/models/dtos/statisticsDtos/customerStatisticsResponse'
import ProductStatisticsResponse from '@/models/dtos/statisticsDtos/productStatisticsResponse'
import EmployeeStatisticsResponse from '@/models/dtos/statisticsDtos/employeeStatisticsResponse'
import RevenueStatisticsResponse from '@/models/dtos/statisticsDtos/revenueStatisticsResponse'
import ComboStatisticsResponse from '@/models/dtos/statisticsDtos/comboStatisticsResponse'
import DatatableStatisticsResponse from '@/models/dtos/statisticsDtos/datatableStatisticsResponse'

import OrderSummary from '@/components/statistics/OrderSummary.vue'
import ProductStatistic from '@/components/statistics/ProductStatistic.vue'
import CustomerStatistic from '@/components/statistics/CustomerStatistic.vue'
import EmployeeStatistic from '@/components/statistics/EmployeeStatistic.vue'
import RevenueStatistic from '@/components/statistics/RevenueStatistic.vue'
import ComboStatistic from '@/components/statistics/ComboStatistic.vue'
import DatatableStatistic from '@/components/statistics/DatatableStatistic.vue'

export default {
  name: 'StatisticsView',
  components: {
    OrderSummary,
    ProductStatistic,
    CustomerStatistic,
    EmployeeStatistic,
    RevenueStatistic,
    ComboStatistic,
    DatatableStatistic,
  },
  props: {},
  data() {
    return {
      orderSummaryData: {},
      productStatisticData: {},
      customerStatisticsData: {},
      employeeStatisticsData: {},
      revenueStatisticData: {},
      comboStatisticsaryData: {},
      datatableStatisticsResponse: {},
      isLoading: true,
    }
  },
  computed: {},
  watch: {},
  async mounted() {
    let errorMessage = ''
    let errorLogs = []
    this.loading = true
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
    try {
      await this.loadDatatableData()
    } catch (error) {
      errorMessage += 'Datatable.'
      errorLogs.push(error)
    }

    if (errorMessage != '') {
      toastr.error('Hiện không thể load dữ liệu: ' + errorMessage)
      console.warn(errorLogs)
    }
    this.isLoading = false
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
    async loadDatatableData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetDatatableStatistics',
        ConfigsRequest.getSkipAuthConfig(),
      )
      // console.log(response.data)
      this.datatableStatisticsResponse = DatatableStatisticsResponse.fromApiResponse(response.data)
      // console.log(this.datatableStatisticsResponse)
    },
  },
}
</script>

<style scoped></style>
