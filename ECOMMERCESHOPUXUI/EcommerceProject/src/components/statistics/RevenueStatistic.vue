<template>
  <div>
    <h2>Thống kê Doanh thu</h2>
    <div class="row mb-4">
      <div class="col" v-for="item in summaryList" :key="item.label">
        <div class="stat-box">
          <div class="stat-label">{{ item.label }}</div>
          <div class="stat-value">{{ item.value }}</div>
        </div>
      </div>
    </div>
    <canvas id="revenueChart" v-show="!isLoading"></canvas>
    <div v-if="isLoading" class="text-center my-4">
      <span>Đang tải dữ liệu...</span>
    </div>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

export default {
  name: 'RevenueStatistic',
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
      revenueChart: null,
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
      if (!newVal) {
        this.$nextTick(() => this.renderRevenueChart())
      }
    },
    data: {
      handler() {
        if (!this.isLoading) {
          this.$nextTick(() => this.renderRevenueChart())
        }
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderRevenueChart()
    }
  },
  methods: {
    renderRevenueChart() {
      // Đảm bảo canvas tồn tại và không bị trùng lặp chart
      const canvas = document.getElementById('revenueChart')
      if (!canvas) return
      const ctx = canvas.getContext('2d')

      // Hủy chart hiện tại nếu nó tồn tại
      if (this.revenueChart) {
        this.revenueChart.destroy()
      }

      // Chỉ tạo chart khi không loading
      if (this.isLoading) return

      this.revenueChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['Tổng doanh thu', 'TB ngày', 'TB tháng', 'Cao nhất', 'Thấp nhất'],
          datasets: [
            {
              label: 'Doanh thu (VND)',
              data: [
                this.data?.totalRevenue ?? 0,
                this.data?.averageDailyRevenue ?? 0,
                this.data?.averageMonthlyRevenue ?? 0,
                this.data?.highestRevenue ?? 0,
                this.data?.lowestRevenue ?? 0,
              ],
              backgroundColor: [
                'rgba(54, 162, 235, 0.7)',
                'rgba(255, 206, 86, 0.7)',
                'rgba(75, 192, 192, 0.7)',
                'rgba(255, 99, 132, 0.7)',
                'rgba(153, 102, 255, 0.7)',
              ],
              borderColor: [
                'rgba(54, 162, 235, 1)',
                'rgba(255, 206, 86, 1)',
                'rgba(75, 192, 192, 1)',
                'rgba(255, 99, 132, 1)',
                'rgba(153, 102, 255, 1)',
              ],
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          plugins: {
            legend: { display: false },
          },
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                callback: function (value) {
                  return value.toLocaleString('vi-VN')
                },
              },
            },
          },
        },
      })
    },
    formatCurrency(val) {
      if (typeof val !== 'number') return '0'
      return val.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })
    },
  },
  beforeUnmount() {
    if (this.revenueChart) {
      this.revenueChart.destroy()
      this.revenueChart = null
    }
  },
}
</script>

<style scoped>
canvas {
  max-width: 400px;
  margin: 20px auto;
  display: block;
}
.stat-box {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 12px 8px;
  text-align: center;
  margin-bottom: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}
.stat-label {
  font-size: 14px;
  color: #666;
}
.stat-value {
  font-size: 20px;
  font-weight: bold;
  color: #222;
}
</style>
