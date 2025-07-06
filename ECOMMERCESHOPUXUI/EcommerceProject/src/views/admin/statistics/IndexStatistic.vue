<template>
  <div style="margin-top: 0px" class="xp-contentbar">
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5"><strong>Thống kê</strong></li>
      </ol>
      <hr />
    </nav>

    <RevenueStatistic
      :data="revenueStatisticData"
      :is-loading="revenueIsLoading"
    ></RevenueStatistic>

    <div class="row align-items-stretch">
      <div class="col-md-12 col-lg-12 col-xl-7 m-b-30">
        <ProductStatistic
          :data="productStatisticData"
          :is-loading="productIsLoading"
        ></ProductStatistic>
      </div>

      <div class="col-md-12 col-lg-12 col-xl-5">
        <div class="flex-grow-1">
          <EmployeeStatistic
            :data="employeeStatisticsData"
            :is-loading="employeeIsLoading"
          ></EmployeeStatistic>
        </div>
        <div class="flex-grow-1">
          <CustomerStatistic
            :data="customerStatisticsData"
            :is-loading="customerIsLoading"
          ></CustomerStatistic>
        </div>
      </div>
    </div>

    <OrderSummary :data="orderSummaryData" :is-loading="orderSummaryIsLoading"></OrderSummary>

    <DatatableStatistic
      :data="datatableStatisticsResponse"
      :is-loading="datatableIsLoading"
      :coupon-data="couponStatisticData"
      :coupon-loading="couponIsLoading"
      :category-data="categoryStatisticData"
      :category-loading="categoryIsLoading"
      :inventory-data="inventoryAnalysisData"
      :inventory-loading="inventoryIsLoading"
      :review-data="reviewAnalysisData"
      :review-loading="reviewIsLoading"
    ></DatatableStatistic>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'

import Swal from 'sweetalert2'

import OrderSummary from '@/components/pages/admin/statistics/OrderSummary.vue'
import ProductStatistic from '@/components/pages/admin/statistics/ProductStatistic.vue'
import CustomerStatistic from '@/components/pages/admin/statistics/CustomerStatistic.vue'
import EmployeeStatistic from '@/components/pages/admin/statistics/EmployeeStatistic.vue'
import RevenueStatistic from '@/components/pages/admin/statistics/RevenueStatistic.vue'
import DatatableStatistic from '@/components/pages/admin/statistics/DatatableStatistic.vue'

export default {
  name: 'StatisticsView',
  components: {
    OrderSummary,
    ProductStatistic,
    CustomerStatistic,
    EmployeeStatistic,
    RevenueStatistic,
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
      datatableStatisticsResponse: {},
      couponStatisticData: {},
      categoryStatisticData: {},
      inventoryAnalysisData: {},
      reviewAnalysisData: {},
      revenueIsLoading: true,
      productIsLoading: true,
      customerIsLoading: true,
      employeeIsLoading: true,
      orderSummaryIsLoading: true,
      datatableIsLoading: true,
      couponIsLoading: true,
      categoryIsLoading: true,
      inventoryIsLoading: true,
      reviewIsLoading: true,
    }
  },
  computed: {},
  watch: {},
  async mounted() {
    this.isLoading = true

    const CACHE_KEY = 'statisticsData'
    const CACHE_EXPIRE = 1 * 60 * 1000 // 5 phút

    let cached = localStorage.getItem(CACHE_KEY)
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
          this.datatableStatisticsResponse = await parsed.datatableStatisticsResponse
          this.isLoading = false
          this.revenueIsLoading = false
          this.productIsLoading = false
          this.customerIsLoading = false
          this.employeeIsLoading = false
          this.orderSummaryIsLoading = false
          this.datatableIsLoading = false
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
    try {
      await this.loadDatatableData()
    } catch (error) {
      errorMessage += 'Datatable. '
      errorLogs.push(error)
    }
    try {
      await this.loadCouponStatisticsData()
    } catch (error) {
      errorMessage += 'Mã giảm giá. '
      errorLogs.push(error)
    }
    try {
      await this.loadCategoryStatisticsData()
    } catch (error) {
      errorMessage += 'Danh mục. '
      errorLogs.push(error)
    }
    try {
      await this.loadInventoryAnalysisData()
    } catch (error) {
      errorMessage += 'Tồn kho. '
      errorLogs.push(error)
    }
    try {
      await this.loadReviewAnalysisData()
    } catch (error) {
      errorMessage += 'Đánh giá. '
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
        orderSummaryData: JSON.parse(JSON.stringify(this.orderSummaryData)),
        productStatisticData: JSON.parse(JSON.stringify(this.productStatisticData)),
        customerStatisticsData: JSON.parse(JSON.stringify(this.customerStatisticsData)),
        employeeStatisticsData: JSON.parse(JSON.stringify(this.employeeStatisticsData)),
        revenueStatisticData: JSON.parse(JSON.stringify(this.revenueStatisticData)),
        datatableStatisticsResponse: JSON.parse(JSON.stringify(this.datatableStatisticsResponse)),
        couponStatisticData: JSON.parse(JSON.stringify(this.couponStatisticData)),
        categoryStatisticData: JSON.parse(JSON.stringify(this.categoryStatisticData)),
        inventoryAnalysisData: JSON.parse(JSON.stringify(this.inventoryAnalysisData)),
        reviewAnalysisData: JSON.parse(JSON.stringify(this.reviewAnalysisData)),
        expire: now + CACHE_EXPIRE,
      }),
    )

    this.isLoading = false // Chuyển trạng thái loading sau khi hoàn thành
  },
  methods: {
    async loadOrderSummaryData() {
      this.orderSummaryIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetOrderSummary',
        ConfigsRequest.takeAuth(),
      )
      this.orderSummaryData = response.data
      await this.$nextTick()
      this.orderSummaryIsLoading = false
    },
    async loadProductStatisticsData() {
      this.productIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetProductStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.productStatisticData = response.data
      await this.$nextTick()
      this.productIsLoading = false
    },
    async loadCustomerStatisticsData() {
      this.customerIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCustomerStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.customerStatisticsData = response.data
      await this.$nextTick()
      this.customerIsLoading = false
    },
    async loadEmployeeStatisticsData() {
      this.employeeIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetEmployeeStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.employeeStatisticsData = response.data
      await this.$nextTick()
      this.employeeIsLoading = false
    },
    async loadRevenueStatisticsData() {
      this.revenueIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetRevenueStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.revenueStatisticData = response.data
      await this.$nextTick()
      this.revenueIsLoading = false
    },
    async loadDatatableData() {
      this.datatableIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetDatatableStatistics',
        ConfigsRequest.takeAuth(),
      )
      ;(this.datatableStatisticsResponse = await response.data), await this.$nextTick()
      this.datatableIsLoading = false
    },
    async loadCouponStatisticsData() {
      this.couponIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCouponStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.couponStatisticData = response.data
      await this.$nextTick()
      this.couponIsLoading = false
    },
    async loadCategoryStatisticsData() {
      this.categoryIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCategoryStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.categoryStatisticData = response.data
      await this.$nextTick()
      this.categoryIsLoading = false
    },
    async loadInventoryAnalysisData() {
      this.inventoryIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetInventoryAnalysis',
        ConfigsRequest.takeAuth(),
      )
      this.inventoryAnalysisData = response.data
      await this.$nextTick()
      this.inventoryIsLoading = false
    },
    async loadReviewAnalysisData() {
      this.reviewIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetReviewAnalysis',
        ConfigsRequest.takeAuth(),
      )
      this.reviewAnalysisData = response.data
      await this.$nextTick()
      this.reviewIsLoading = false
    },
  },
}
</script>
<style scoped></style>
