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
            <hr />
            <div class="row mt-4">
              <div class="col-md-4">
                <h6 class="text-center mb-3">Doanh thu theo ngày</h6>
                <canvas id="dailyRevenueChart"></canvas>
                <p v-if="!hasDailyRevenueData" class="text-center text-muted mt-2">Không có dữ liệu doanh thu theo ngày.</p>
              </div>
              <div class="col-md-4">
                <h6 class="text-center mb-3">Doanh thu theo tháng</h6>
                <canvas id="monthlyRevenueChart"></canvas>
                <p v-if="!hasMonthlyRevenueData" class="text-center text-muted mt-2">Không có dữ liệu doanh thu theo tháng.</p>
              </div>
              <div class="col-md-4">
                <h6 class="text-center mb-3">Doanh thu theo năm</h6>
                <canvas id="yearlyRevenueChart"></canvas>
                <p v-if="!hasYearlyRevenueData" class="text-center text-muted mt-2">Không có dữ liệu doanh thu theo năm.</p>
              </div>
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
      type: Object,
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      dailyRevenueChart: null,
      monthlyRevenueChart: null,
      yearlyRevenueChart: null,
      hasDailyRevenueData: false,
      hasMonthlyRevenueData: false,
      hasYearlyRevenueData: false,
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
      if (this.dailyRevenueChart) this.dailyRevenueChart.destroy()
      if (this.monthlyRevenueChart) this.monthlyRevenueChart.destroy()
      if (this.yearlyRevenueChart) this.yearlyRevenueChart.destroy()

      // --- Daily Revenue Chart ---
      // NOTE: Assumes data.dailyRevenue is an array of { date: string, revenue: number }
      const dailyData = this.data.dailyRevenue || []
      this.hasDailyRevenueData = dailyData.length > 0
      if (this.hasDailyRevenueData) {
        const dailyLabels = dailyData.map(item => item.date)
        const dailyRevenues = dailyData.map(item => item.revenue)
        this.dailyRevenueChart = new Chart(document.getElementById('dailyRevenueChart'), {
          type: 'line',
          data: {
            labels: dailyLabels,
            datasets: [{
              label: 'Doanh thu ngày',
              data: dailyRevenues,
              borderColor: 'rgba(75, 192, 192, 1)',
              backgroundColor: 'rgba(75, 192, 192, 0.2)',
              fill: true,
              tension: 0.1
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } }
          }
        })
      }

      // --- Monthly Revenue Chart ---
      // NOTE: Assumes data.monthlyRevenue is an array of { month: string, revenue: number }
      const monthlyData = this.data.monthlyRevenue || []
      this.hasMonthlyRevenueData = monthlyData.length > 0
      if (this.hasMonthlyRevenueData) {
        const monthlyLabels = monthlyData.map(item => item.month)
        const monthlyRevenues = monthlyData.map(item => item.revenue)
        this.monthlyRevenueChart = new Chart(document.getElementById('monthlyRevenueChart'), {
          type: 'bar',
          data: {
            labels: monthlyLabels,
            datasets: [{
              label: 'Doanh thu tháng',
              data: monthlyRevenues,
              backgroundColor: 'rgba(153, 102, 255, 0.6)',
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } }
          }
        })
      }

      // --- Yearly Revenue Chart ---
      // NOTE: Assumes data.yearlyRevenue is an array of { year: string, revenue: number }
      const yearlyData = this.data.yearlyRevenue || []
      this.hasYearlyRevenueData = yearlyData.length > 0
      if (this.hasYearlyRevenueData) {
        const yearlyLabels = yearlyData.map(item => item.year)
        const yearlyRevenues = yearlyData.map(item => item.revenue)
        this.yearlyRevenueChart = new Chart(document.getElementById('yearlyRevenueChart'), {
          type: 'bar',
          data: {
            labels: yearlyLabels,
            datasets: [{
              label: 'Doanh thu năm',
              data: yearlyRevenues,
              backgroundColor: 'rgba(255, 159, 64, 0.6)',
            }]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: { x: { display: false }, y: { display: false } }
          }
        })
      }
    }
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
