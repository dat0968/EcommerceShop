<template>
  <div class="col-12">
    <div class="card m-b-30">
      <div class="card-header bg-white d-flex justify-content-between align-items-center">
        <h5 class="card-title text-black mb-0">Thống kê doanh thu & nhân sự</h5>
        <div class="btn-group btn-group-sm" role="group">
          <button
            v-for="period in periods"
            :key="period.value"
            type="button"
            class="btn"
            :class="{
              'btn-primary': selectedPeriod === period.value,
              'btn-outline-primary': selectedPeriod !== period.value,
            }"
            @click="selectedPeriod = period.value"
          >
            {{ period.label }}
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
        <div v-else class="row align-items-start g-4">
          <!-- Icon tình trạng nhân sự -->
          <div class="col-12 position-relative">
            <div class="bg-white border rounded p-3">

              <div class="row">
                <h6 class="col-md-4 col-sm-12">Tình trạng nhân sự</h6>
                <!-- Nhân viên đang làm -->
                <div class="col-md-4 col-sm-12">
                  <div class="d-flex align-items-center ">
                    <div class="d-flex align-items-center gap-2">
                      <span class="status-circle bg-primary"></span>
                      <span>Đang làm: </span>
                    </div>
                    <strong>{{ data.totalActiveEmployees ?? 0 }}</strong>
                  </div>
                </div>

                <!-- Nhân viên nghỉ việc -->
                <div class="col-md-4 col-sm-12 mt-2 mt-md-0">
                  <div class="d-flex align-items-center ">
                    <div class="d-flex align-items-center gap-2">
                      <span class="status-circle bg-danger"></span>
                      <span>Nghỉ việc: </span>
                    </div>
                    <strong>{{ data.totalInactiveEmployees ?? 0 }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>


          <!-- Biểu đồ doanh thu -->
          <div class="col-12">
            <div class="chart-container border p-2 rounded bg-white position-relative">
              <canvas
                v-for="period in periods"
                :key="period.value"
                :ref="`chart_${period.value}`"
                v-show="selectedPeriod === period.value"
              ></canvas>
            </div>
          </div>

          <!-- Thống kê tiền lương -->
          <div class="col-md-12 mt-4">
            <div class="xp-chart-label">
              <ul class="list-inline text-center mb-0">
                <li class="list-inline-item mx-4">
                  <p class="text-black mb-1">Doanh thu trung bình</p>
                  <h4 class="text-primary-gradient">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data?.averageSalary) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-4">
                  <p class="text-black mb-1">Tổng doanh thu</p>
                  <h4 class="text-success-gradient">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(data?.totalSalary) }}
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
import { Chart, registerables } from 'chart.js'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import NoDataMessage from '@/components/common/NoDataMessage.vue'
import { formatCurrency } from '@/constants/formatCurrency'

Chart.register(...registerables)

export default {
  name: 'EmployeeStatistic',
  components: { LoadingSpinner, NoDataMessage },
  props: {
    data: { default: () => ({}) },
    isLoading: { type: Boolean, default: false },
  },
  data() {
    return {
      selectedPeriod: 'daily',
      chartInstances: {},
      periods: [
        { label: 'Ngày', value: 'daily' },
        { label: 'Tuần', value: 'weekly' },
        { label: 'Tháng', value: 'monthly' },
        { label: 'Năm', value: 'yearly' },
      ],
    }
  },
  watch: {
    data: {
      handler() {
        this.$nextTick(this.renderAllCharts)
      },
      deep: true,
    },
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(this.renderAllCharts)
      }
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderAllCharts()
    }
  },
  beforeUnmount() {
    Object.values(this.chartInstances).forEach(chart => chart.destroy?.())
  },
  methods: {
    formatCurrency,
    renderAllCharts() {
      this.periods.forEach(({ value }) => this.renderChart(value))
    },
    renderChart(period) {
      const canvasArray = this.$refs[`chart_${period}`]
      const canvas = Array.isArray(canvasArray) ? canvasArray[0] : canvasArray

      if (!canvas || !this.data?.revenueByTime?.[period]) return

      const ctx = canvas.getContext('2d')
      if (!ctx) return

      // Hủy biểu đồ cũ nếu có
      if (this.chartInstances[period]) {
        this.chartInstances[period].destroy()
      }

      const chartData = this.data.revenueByTime[period] || []

      this.chartInstances[period] = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: chartData.map((d) => d.label),
          datasets: [
            {
              label: `Doanh thu (${period})`,
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
            legend: { position: 'top' },
            tooltip: { enabled: true },
          },
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'Doanh thu (VNĐ)',
              },
            },
          },
        },
      })
    }

  },
}
</script>

<style scoped>
.chart-container {
  width: 100%;
  min-height: 10em;
}
.status-circle {
  width: 10px;
  height: 10px;
  display: inline-block;
  border-radius: 50%;
}
.bg-primary {
  background-color: #007bff !important;
}
.bg-danger {
  background-color: #dc3545 !important;
}
canvas {
  max-height:10rem
}
</style>
