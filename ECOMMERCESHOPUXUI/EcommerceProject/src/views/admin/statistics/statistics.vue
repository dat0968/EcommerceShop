<template>
  <!-- Start XP Contentbar -->
  <div style="margin-top: 100px" class="xp-contentbar">
    <!-- Breadcrumb trạng thái -->
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5"><strong>Thống kê</strong></li>
      </ol>
      <hr />
    </nav>
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

  <!-- <div class="">
    <ComboStatistic :data="comboStatisticsaryData" :is-loading="isLoading"></ComboStatistic>
  </div> -->
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'

import Swal from 'sweetalert2'

import OrderSummaryResponse from '@/models/dtos/statisticsDtos/orderSummaryResponse'
import CustomerStatisticsResponse from '@/models/dtos/statisticsDtos/customerStatisticsResponse'
import ProductStatisticsResponse from '@/models/dtos/statisticsDtos/productStatisticsResponse'
import EmployeeStatisticsResponse from '@/models/dtos/statisticsDtos/employeeStatisticsResponse'
import RevenueStatisticsResponse from '@/models/dtos/statisticsDtos/revenueStatisticsResponse'
import ComboStatisticsResponse from '@/models/dtos/statisticsDtos/comboStatisticsResponse'
import DatatableStatisticsResponse from '@/models/dtos/statisticsDtos/datatableStatisticsResponse'

import OrderSummary from '@/components/pages/statistics/OrderSummary.vue'
import ProductStatistic from '@/components/pages/statistics/ProductStatistic.vue'
import CustomerStatistic from '@/components/pages/statistics/CustomerStatistic.vue'
import EmployeeStatistic from '@/components/pages/statistics/EmployeeStatistic.vue'
import RevenueStatistic from '@/components/pages/statistics/RevenueStatistic.vue'
// import ComboStatistic from '@/components/pages/statistics/ComboStatistic.vue'
import DatatableStatistic from '@/components/pages/statistics/DatatableStatistic.vue'

export default {
  name: 'StatisticsView',
  components: {
    OrderSummary,
    ProductStatistic,
    CustomerStatistic,
    EmployeeStatistic,
    RevenueStatistic,
    // ComboStatistic,
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
    this.isLoading = true

    const CACHE_KEY = 'statisticsData'
    const CACHE_EXPIRE = 5 * 60 * 1000 // 5 phút

    let cached = await localStorage.getItem(CACHE_KEY)
    let now = Date.now()
    if (cached) {
      try {
        const parsed = await JSON.parse(cached)
        const isExpired = parsed.expire && parsed.expire < now
        if (!isExpired) {
          this.orderSummaryData = await parsed.orderSummaryData
          this.productStatisticData = await parsed.productStatisticData
          this.customerStatisticsData = await parsed.customerStatisticsData
          this.employeeStatisticsData = await parsed.employeeStatisticsData
          this.revenueStatisticData = await parsed.revenueStatisticData
          this.comboStatisticsaryData = await parsed.comboStatisticsaryData
          this.datatableStatisticsResponse = await parsed.datatableStatisticsResponse
          this.isLoading = false
          return // Dừng nếu dữ liệu không hết hạn
        }
        localStorage.removeItem(CACHE_KEY) // Xóa cache nếu hết hạn
      } catch (e) {
        localStorage.removeItem(CACHE_KEY)
        console.error(e)
      }
    }

    // Nếu không có cache hoặc cache đã hết hạn
    let errorMessage = ''
    let errorLogs = []

    try {
      await this.loadOrderSummaryData()
    } catch (error) {
      errorMessage += 'Đơn hàng. '
      errorLogs.push(error)
    }
    try {
      await this.loadProductStatisticsData()
    } catch (error) {
      errorMessage += 'Sản phẩm. '
      errorLogs.push(error)
    }
    try {
      await this.loadCustomerStatisticsData()
    } catch (error) {
      errorMessage += 'Khách hàng. '
      errorLogs.push(error)
    }
    try {
      await this.loadEmployeeStatisticsData()
    } catch (error) {
      errorMessage += 'Nhân viên. '
      errorLogs.push(error)
    }
    try {
      await this.loadRevenueStatisticsData()
    } catch (error) {
      errorMessage += 'Doanh thu. '
      errorLogs.push(error)
    }
    /* try {
      await this.loadComboStatisticsData()
    } catch (error) {
      errorMessage += 'Combo. '
      errorLogs.push(error)
    } */
    try {
      await this.loadDatatableData()
    } catch (error) {
      errorMessage += 'Datatable. '
      errorLogs.push(error)
    }

    if (errorMessage !== '') {
      Swal.fire('Hiện không thể load các dữ liệu dưới', errorMessage, 'error')
      console.warn(errorLogs)
    }
    // Lưu cache với thời gian hết hạn
    localStorage.setItem(
      CACHE_KEY,
      JSON.stringify({
        orderSummaryData: JSON.parse(JSON.stringify(this.orderSummaryData)), // Chuyển đổi thành đối tượng thường
        productStatisticData: JSON.parse(JSON.stringify(this.productStatisticData)),
        customerStatisticsData: JSON.parse(JSON.stringify(this.customerStatisticsData)),
        employeeStatisticsData: JSON.parse(JSON.stringify(this.employeeStatisticsData)),
        revenueStatisticData: JSON.parse(JSON.stringify(this.revenueStatisticData)),
        comboStatisticsaryData: JSON.parse(JSON.stringify(this.comboStatisticsaryData)),
        datatableStatisticsResponse: JSON.parse(JSON.stringify(this.datatableStatisticsResponse)),
        expire: now + CACHE_EXPIRE,
      }),
    )

    this.isLoading = false // Chuyển trạng thái loading sau khi hoàn thành
  },
  methods: {
    async loadOrderSummaryData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetOrderSummary',
        ConfigsRequest.takeAuth(),
      )
      this.orderSummaryData = OrderSummaryResponse.fromApiResponse(response.data)
    },
    async loadProductStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetProductStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.productStatisticData = ProductStatisticsResponse.fromApiResponse(response.data)
    },
    async loadCustomerStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCustomerStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.customerStatisticsData = CustomerStatisticsResponse.fromApiResponse(response.data)
    },
    async loadEmployeeStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetEmployeeStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.employeeStatisticsData = EmployeeStatisticsResponse.fromApiResponse(response.data)
    },
    async loadRevenueStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetRevenueStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.revenueStatisticData = RevenueStatisticsResponse.fromApiResponse(response.data)
    },
    async loadComboStatisticsData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetComboStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.comboStatisticsaryData = ComboStatisticsResponse.fromApiResponse(response.data)
    },
    async loadDatatableData() {
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetDatatableStatistics',
        ConfigsRequest.takeAuth(),
      )
      // console.log(response.data)
      this.datatableStatisticsResponse = DatatableStatisticsResponse.fromApiResponse(response.data)
      // console.log(this.datatableStatisticsResponse)
    },
  },
}
</script>

<style scoped></style>
