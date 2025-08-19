<template>
  <div style="margin-top: 5rem" class="xp-contentbar">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
          <li class="breadcrumb-item active h5"><strong>Thống kê</strong></li>
        </ol>
      </nav>
      <button class="btn btn-primary" @click="reloadData" :disabled="isLoading">
        <i class="bi bi-arrow-clockwise"></i> Tải lại
      </button>
    </div>
    <hr />

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
      :category-data="categoryStatisticData"
      :category-loading="categoryIsLoading"
      :inventory-loading="inventoryIsLoading"
      :review-data="reviewAnalysisData"
      :review-loading="reviewIsLoading"
    ></DatatableStatistic>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import Cookies from 'js-cookie'

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
      categoryStatisticData: {},
      reviewAnalysisData: {},
      isLoading: false,
      revenueIsLoading: true,
      productIsLoading: true,
      customerIsLoading: true,
      employeeIsLoading: true,
      orderSummaryIsLoading: true,
      datatableIsLoading: true,
      categoryIsLoading: true,
      inventoryIsLoading: true,
      reviewIsLoading: true,
    }
  },
  computed: {},
  watch: {},
  async mounted() {
    // Proactively ensure the API base URL is initialized before doing anything else.
    // Now, proceed with waiting for the auth token and loading data.
    this.waitForAuthAndLoad()
  },
  methods: {
    waitForAuthAndLoad(retries = 50) {
      // Wait for both the auth token AND the API endpoint to be ready.
      if (Cookies.get('accessToken') && axiosConfig.isEndpointAvailable()) {
        this.reloadData()
      } else if (retries > 0) {
        setTimeout(() => this.waitForAuthAndLoad(retries - 1), 100)
      } else {
        let errorMessage = 'Không thể tải dữ liệu thống kê.'
        if (!Cookies.get('accessToken')) {
          errorMessage =
            'Không tìm thấy thông tin đăng nhập. Vui lòng thử đăng nhập lại.'
        } else if (!axiosConfig.isEndpointAvailable()) {
          errorMessage =
            'Không thể kết nối đến máy chủ API. Vui lòng kiểm tra lại kết nối.'
        }
        console.error(
          'Could not load statistics. Token or API endpoint not available in time.',
        )
        Swal.fire('Lỗi', errorMessage, 'error')

        // Set loading to false to stop spinners
        this.isLoading = false
        this.revenueIsLoading = false
        this.productIsLoading = false
        this.customerIsLoading = false
        this.employeeIsLoading = false
        this.orderSummaryIsLoading = false
        this.datatableIsLoading = false
        this.categoryIsLoading = false
        this.inventoryIsLoading = false
        this.reviewIsLoading = false
      }
    },
    async reloadData() {
      this.isLoading = true
      let errorMessage = ''
      let errorLogs = []

      const tasks = [
        this.loadOrderSummaryData(),
        this.loadProductStatisticsData(),
        this.loadCustomerStatisticsData(),
        this.loadEmployeeStatisticsData(),
        this.loadRevenueStatisticsData(),
        this.loadDatatableData(),
        this.loadCategoryStatisticsData(),
        this.loadReviewAnalysisData(),
      ]

      const results = await Promise.allSettled(tasks)

      results.forEach((result, index) => {
        if (result.status === 'rejected') {
          const errorSources = [
            'Đơn hàng',
            'Sản phẩm',
            'Khách hàng',
            'Nhân viên',
            'Doanh thu',
            'Datatable',
            'Mã giảm giá',
            'Danh mục',
            'Tồn kho',
            'Đánh giá',
          ]
          errorMessage += `${errorSources[index]}. `
          errorLogs.push(result.reason)
        }
      })

      if (errorMessage) {
        Swal.fire('Không thể tải lại dữ liệu từ các nguồn sau:', errorMessage, 'error')
        console.warn('Lỗi khi tải lại dữ liệu:', errorLogs)
      }

      this.isLoading = false
    },
    async loadOrderSummaryData() {
      this.orderSummaryIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetOrderSummary',
        ConfigsRequest.takeAuth(),
      )
      this.orderSummaryData = response.data || {}
      await this.$nextTick()
      this.orderSummaryIsLoading = false
    },
    async loadProductStatisticsData() {
      this.productIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetProductStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.productStatisticData = response.data || {}
      await this.$nextTick()
      this.productIsLoading = false
    },
    async loadCustomerStatisticsData() {
      this.customerIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCustomerStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.customerStatisticsData = response.data || {}
      await this.$nextTick()
      this.customerIsLoading = false
    },
    async loadEmployeeStatisticsData() {
      this.employeeIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetEmployeeStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.employeeStatisticsData = response.data || {}
      await this.$nextTick()
      this.employeeIsLoading = false
    },
    async loadRevenueStatisticsData() {
      this.revenueIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetRevenueStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.revenueStatisticData = response.data || {}
      await this.$nextTick()
      this.revenueIsLoading = false
    },
    async loadDatatableData() {
      this.datatableIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetDatatableStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.datatableStatisticsResponse = response.data || {}
      await this.$nextTick()
      this.datatableIsLoading = false
    },
    async loadCategoryStatisticsData() {
      this.categoryIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetCategoryStatistics',
        ConfigsRequest.takeAuth(),
      )
      this.categoryStatisticData = response.data || {}
      await this.$nextTick()
      this.categoryIsLoading = false
    },
    async loadReviewAnalysisData() {
      this.reviewIsLoading = true
      const response = await axiosConfig.getFromApi(
        '/Statistics/GetReviewAnalysis',
        ConfigsRequest.takeAuth(),
      )
      this.reviewAnalysisData = response.data || {}
      await this.$nextTick()
      this.reviewIsLoading = false
    },
    resetLoadingState() {
      this.isLoading = true
      this.revenueIsLoading = true
      this.productIsLoading = true
      this.customerIsLoading = true
      this.employeeIsLoading = true
      this.orderSummaryIsLoading = true
      this.datatableIsLoading = true
      this.categoryIsLoading = true
      this.inventoryIsLoading = true
      this.reviewIsLoading = true
    },
  },
  activated() {
    // Called when a cached component is made active.
    this.reloadData()
  },
  deactivated() {
    // Called when a cached component is deactivated.
    this.resetLoadingState()
  },
  unmounted() {
    // Called when the component is unmounted.
    this.resetLoadingState()
  },
  }

</script>
<style scoped></style>
