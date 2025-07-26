<template>
  <div class="row">
    <div class="col-md-12 col-lg-12 col-xl-12 mb-4">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Thống kê Doanh thu</h5>
        </div>
        <div class="card-body">
          <div v-if="isLoading" class="text-center my-4">
            <LoadingSpinner />
          </div>
          <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
            <NoDataMessage />
          </div>
          <div v-else>
            <div class="row mt-4">
              <div class="col-md-12">
                <h6 class="text-center mb-3">Tổng quan doanh thu</h6>
                <canvas id="summaryRevenueChart"></canvas>
                <p v-if="!hasSummaryData" class="text-center text-muted mt-2">
                  Không có dữ liệu tổng quan doanh thu.
                </p>
              </div>
            </div>
            <hr />
            <div class="xp-chart-label">
              <ul class="list-inline text-center">
                <li class="list-inline-item mx-3">
                  <p class="text-black">Tổng doanh thu</p>
                  <h4 class="text-primary-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.totalRevenue) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-black">Doanh thu TB ngày</p>
                  <h4 class="text-success-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.averageDailyRevenue) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-info-gradient mb-3">Doanh thu TB tháng</p>
                  <h4 class="text-info-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.averageMonthlyRevenue) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-black">Doanh thu cao nhất</p>
                  <h4 class="text-warning-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.highestRevenue) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-black">Doanh thu thấp nhất</p>
                  <h4 class="text-danger-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.lowestRevenue) }}
                  </h4>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { formatCurrency } from '@/constants/formatCurrency'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import NoDataMessage from '@/components/common/NoDataMessage.vue'
import { Chart, registerables } from 'chart.js'
Chart.register(...registerables)

export default {
  name: 'RevenueStatistic',
  components: {
    LoadingSpinner,
    NoDataMessage,
  },
  props: {
    data: {
      default: () => ({}),
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      hasSummaryData: false,
    }
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng doanh thu', value: this.formatCurrency(this.data?.totalRevenue) },
        { label: 'Doanh thu TB ngày', value: this.formatCurrency(this.data?.averageDailyRevenue) },
        {
          label: 'Doanh thu TB tháng',
          value: this.formatCurrency(this.data?.averageMonthlyRevenue),
        },
        { label: 'Doanh thu cao nhất', value: this.formatCurrency(this.data?.highestRevenue) },
        { label: 'Doanh thu thấp nhất', value: this.formatCurrency(this.data?.lowestRevenue) },
      ]
    },
  },
  watch: {
    isLoading(newVal) {
      if (!newVal && this.data) {
        this.$nextTick(() => {
          this.renderCharts()
        })
      }
    },
    data: {
      handler() {
        if (!this.isLoading) {
          this.$nextTick(() => {
            this.renderCharts()
          })
        }
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading && this.data) {
      this.renderCharts()
    }
  },
  methods: {
    formatCurrency,
    renderCharts() {
      // Destroy existing charts to prevent memory leaks
      if (this.summaryRevenueChart) this.summaryRevenueChart.destroy()
      if (this.customerChart) this.customerChart.destroy()
      if (this.productChart) this.productChart.destroy()
      if (this.orderChart) this.orderChart.destroy()

      // --- Summary Revenue Chart ---
      // NOTE: Uses summary statistics for total, average daily, monthly, highest, and lowest revenue
      const summaryData = [
        { label: 'Tổng doanh thu', value: this.data.totalRevenue || 0 },
        { label: 'TB ngày', value: this.data.averageDailyRevenue || 0 },
        { label: 'TB tháng', value: this.data.averageMonthlyRevenue || 0 },
        { label: 'Cao nhất', value: this.data.highestRevenue || 0 },
        { label: 'Thấp nhất', value: this.data.lowestRevenue || 0 },
      ]
      this.hasSummaryData = summaryData.length > 0
      if (this.hasSummaryData) {
        const summaryLabels = summaryData.map((item) => item.label)
        const summaryValues = summaryData.map((item) => item.value)
        this.summaryRevenueChart = new Chart(document.getElementById('summaryRevenueChart'), {
          type: 'bar',
          data: {
            labels: summaryLabels,
            datasets: [
              {
                label: 'Tổng quan doanh thu',
                data: summaryValues,
                backgroundColor: [
                  'rgba(54, 162, 235, 0.6)',
                  'rgba(75, 192, 192, 0.6)',
                  'rgba(153, 102, 255, 0.6)',
                  'rgba(255, 205, 86, 0.6)',
                  'rgba(255, 99, 132, 0.6)',
                ],
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true } },
          },
        })
      }

      // --- Customer Chart ---
      // NOTE: Assumes data.topCustomers is an array of { name: string, totalSpent: number }
      const customerData = this.data.topCustomers || []
      this.hasCustomerData = customerData.length > 0
      if (this.hasCustomerData) {
        const customerLabels = customerData.slice(0, 5).map((item) => item.name)
        const customerValues = customerData.slice(0, 5).map((item) => item.totalSpent)
        this.customerChart = new Chart(document.getElementById('customerChart'), {
          type: 'doughnut',
          data: {
            labels: customerLabels,
            datasets: [
              {
                label: 'Chi tiêu khách hàng',
                data: customerValues,
                backgroundColor: [
                  'rgba(255, 99, 132, 0.6)',
                  'rgba(54, 162, 235, 0.6)',
                  'rgba(255, 206, 86, 0.6)',
                  'rgba(75, 192, 192, 0.6)',
                  'rgba(153, 102, 255, 0.6)',
                ],
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
          },
        })
      }

      // --- Product Chart ---
      // NOTE: Assumes data.topProducts is an array of { name: string, totalSold: number }
      const productData = this.data.topProducts || []
      this.hasProductData = productData.length > 0
      if (this.hasProductData) {
        const productLabels = productData.slice(0, 5).map((item) => item.name)
        const productValues = productData.slice(0, 5).map((item) => item.totalSold)
        this.productChart = new Chart(document.getElementById('productChart'), {
          type: 'pie',
          data: {
            labels: productLabels,
            datasets: [
              {
                label: 'Sản phẩm bán chạy',
                data: productValues,
                backgroundColor: [
                  'rgba(255, 99, 132, 0.6)',
                  'rgba(54, 162, 235, 0.6)',
                  'rgba(255, 206, 86, 0.6)',
                  'rgba(75, 192, 192, 0.6)',
                  'rgba(153, 102, 255, 0.6)',
                ],
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
          },
        })
      }

      // --- Order Chart ---
      // NOTE: Assumes data.orderStatusCount is an array of { status: string, count: number }
      const orderData = this.data.orderStatusCount || []
      this.hasOrderData = orderData.length > 0
      if (this.hasOrderData) {
        const orderLabels = orderData.map((item) => item.status)
        const orderValues = orderData.map((item) => item.count)
        this.orderChart = new Chart(document.getElementById('orderChart'), {
          type: 'bar',
          data: {
            labels: orderLabels,
            datasets: [
              {
                label: 'Đơn hàng theo trạng thái',
                data: orderValues,
                backgroundColor: 'rgba(54, 162, 235, 0.6)',
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } },
          },
        })
      }
    },
  },
}
</script>

<style scoped>
.revenue-statistic {
  padding: 20px;
}
.stat-box {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px; /* Có thể giữ padding để tạo không gian */
  text-align: center;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}
.stat-label {
  font-size: 16px;
  color: #666;
}
.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #222;
}
/* Styles for small charts */
canvas {
  max-height: 150px; /* Adjust as needed */
  width: 100% !important; /* Ensure responsiveness */
  height: auto !important;
}
</style>
