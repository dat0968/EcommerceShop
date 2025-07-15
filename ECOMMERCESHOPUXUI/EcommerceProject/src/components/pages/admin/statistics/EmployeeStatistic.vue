<template>
  <div class="col-12">
    <!-- Khung dữ liệu tóm tắt và biểu đồ -->
    <div class="card m-b-30">
      <div class="card-header bg-white d-flex justify-content-between align-items-center">
        <h5 class="card-title text-black mb-0">Doanh thu theo {{ selectedPeriodText }}</h5>
        <div class="btn-group" role="group">
          <button
            type="button"
            class="btn btn-sm"
            :class="{
              'btn-primary': selectedPeriod === 'daily',
              'btn-outline-primary': selectedPeriod !== 'daily',
            }"
            @click="changePeriod('daily')"
          >
            Ngày
          </button>
          <button
            type="button"
            class="btn btn-sm"
            :class="{
              'btn-primary': selectedPeriod === 'weekly',
              'btn-outline-primary': selectedPeriod !== 'weekly',
            }"
            @click="changePeriod('weekly')"
          >
            Tuần
          </button>
          <button
            type="button"
            class="btn btn-sm"
            :class="{
              'btn-primary': selectedPeriod === 'monthly',
              'btn-outline-primary': selectedPeriod !== 'monthly',
            }"
            @click="changePeriod('monthly')"
          >
            Tháng
          </button>
          <button
            type="button"
            class="btn btn-sm"
            :class="{
              'btn-primary': selectedPeriod === 'yearly',
              'btn-outline-primary': selectedPeriod !== 'yearly',
            }"
            @click="changePeriod('yearly')"
          >
            Năm
          </button>
        </div>
      </div>
      <div class="card-body">
        <div v-if="isLoading" class="text-center my-4">
          <LoadingSpinner />
        </div>
        <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
          <NoDataMessage />
        </div>
        <div v-else class="row align-items-center g-3">
          <div class="col-md-6">
            <!-- Khung dữ liệu tóm tắt -->
            <div class="xp-chart-label">
              <ul class="list-inline text-center">
                <li class="list-inline-item mx-3">
                  <p class="text-black">Lương trung bình</p>
                  <h4 class="text-primary-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data?.averageSalary) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-black">Tổng lương</p>
                  <h4 class="text-success-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data?.totalSalary) }}
                  </h4>
                </li>
              </ul>
            </div>
          </div>
          <div class="col-md-6">
            <div class="border rounded-except-top-right border p-1 bg-white">
              <canvas ref="employeeChart"></canvas>
              <div v-show="isLoading" class="text-center my-4">
                <LoadingSpinner />
              </div>
            </div>
          </div>

          <div class="col-md-12">
            <!-- Khung biểu đồ -->
            <div class="chart-container position-relative">
              <canvas
                ref="revenueChartDaily"
                v-show="!isLoading && selectedPeriod === 'daily'"
              ></canvas>
              <canvas
                ref="revenueChartWeekly"
                v-show="!isLoading && selectedPeriod === 'weekly'"
              ></canvas>
              <canvas
                ref="revenueChartMonthly"
                v-show="!isLoading && selectedPeriod === 'monthly'"
              ></canvas>
              <canvas
                ref="revenueChartYearly"
                v-show="!isLoading && selectedPeriod === 'yearly'"
              ></canvas>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

import { formatCurrency } from '@/constants/formatCurrency'
Chart.register(...registerables)
export default {
  name: 'EmployeeStatistic',
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
      default: false,
    },
  },
  data() {
    return {
      employeeChart: null,
      revenueChartDaily: null,
      revenueChartWeekly: null,
      revenueChartMonthly: null,
      revenueChartYearly: null,
      chartError: false,
      selectedPeriod: 'daily',
    }
  },
  computed: {
    selectedPeriodText() {
      switch (this.selectedPeriod) {
        case 'daily':
          return 'Ngày'
        case 'weekly':
          return 'Tuần'
        case 'monthly':
          return 'Tháng'
        case 'yearly':
          return 'Năm'
        default:
          return ''
      }
    },
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => {
          this.renderAllCharts()
        })
      }
    },
    data: {
      handler() {
        this.$nextTick(() => {
          this.renderAllCharts()
        })
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderAllCharts()
    }
  },
  beforeUnmount() {
    this.destroyCharts()
  },
  methods: {
    formatCurrency,
    changePeriod(period) {
      this.selectedPeriod = period
    },
    destroyCharts() {
      if (this.revenueChartDaily) this.revenueChartDaily.destroy()
      if (this.revenueChartWeekly) this.revenueChartWeekly.destroy()
      if (this.revenueChartMonthly) this.revenueChartMonthly.destroy()
      if (this.revenueChartYearly) this.revenueChartYearly.destroy()
      if (this.employeeChart) this.employeeChart.destroy()
    },
    renderAllCharts() {
      this.destroyCharts()
      this.renderRevenueChart('daily')
      this.renderRevenueChart('weekly')
      this.renderRevenueChart('monthly')
      this.renderRevenueChart('yearly')
      this.renderEmployeeChart()
    },
    renderRevenueChart(period) {
      try {
        const refName = `revenueChart${period.charAt(0).toUpperCase() + period.slice(1)}`
        const canvas = this.$refs[refName]
        if (!canvas || !this.data || !this.data.revenueByTime) {
          return
        }
        const context = canvas.getContext('2d')
        if (!context) {
          return
        }

        const chartData = this.data.revenueByTime[period] || []
        const chartLabel = `Doanh thu theo ${period.charAt(0).toUpperCase() + period.slice(1)}`

        this[refName] = new Chart(context, {
          type: 'bar',
          data: {
            labels: chartData.map((d) => d.label),
            datasets: [
              {
                label: chartLabel,
                data: chartData.map((d) => d.revenue),
                backgroundColor: 'rgba(75, 192, 192, 0.7)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
              },
            ],
          },
          options: {
            responsive: true,
            plugins: {
              legend: {
                position: 'top',
              },
            },
            scales: {
              y: {
                beginAtZero: true,
              },
            },
          },
        })
      } catch (e) {
        console.error(`Failed to render ${period} revenue chart:`, e)
      }
    },
    renderEmployeeChart() {
      try {
        const canvas = this.$refs.employeeChart
        if (!canvas || !this.data) {
          return
        }
        const context = canvas.getContext('2d')
        if (!context) {
          return
        }
        // Kiểm tra dữ liệu hợp lệ
        const active = this.data?.totalActiveEmployees ?? 0
        const inactive = this.data?.totalInactiveEmployees ?? 0
        if (active === 0 && inactive === 0) {
          return
        }
        this.employeeChart = new Chart(context, {
          type: 'bar',
          data: {
            labels: ['Tổng số nhân viên'],
            datasets: [
              {
                label: 'Đang làm',
                data: [active],
                backgroundColor: 'rgba(54, 162, 235, 0.7)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1,
              },
              {
                label: 'Nghỉ việc',
                data: [inactive],
                backgroundColor: 'rgba(255, 99, 132, 0.7)',
                borderColor: 'rgba(255, 99, 132, 1)',
                borderWidth: 1,
              },
            ],
          },
          options: {
            responsive: true,
            plugins: {
              legend: {
                position: 'bottom',
              },
            },
            scales: {
              x: {
                stacked: true,
              },
              y: {
                stacked: true,
                beginAtZero: true,
              },
            },
          },
        })
      } catch (e) {
        console.error('Failed to render employee chart:', e)
      }
    },
  },
}
</script>
