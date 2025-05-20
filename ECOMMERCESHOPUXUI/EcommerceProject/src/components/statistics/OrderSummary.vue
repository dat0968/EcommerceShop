<template>
  <div>
    <div class="text-center my-4">
      <h2>Tổng quan đơn hàng</h2>
      <p>Tổng số đơn hàng: {{ data.totalOrders }}</p>
      <p>Tổng doanh thu: {{ data.totalRevenue }}</p>
      <p>Giá trị đơn hàng trung bình: {{ data.averageOrderValue }}</p>
    </div>

    <div class="text-center my-4">
      <label for="timePeriod">Chọn khoảng thời gian:</label>
      <select v-model="selectedTimePeriod" @change="updateCharts">
        <option value="date">Theo ngày</option>
        <option value="month">Theo tháng</option>
        <option value="year">Theo năm</option>
      </select>
    </div>

    <canvas id="revenueChartByTime" v-show="!isLoading"></canvas>
    <canvas id="orderStatusChart" v-show="!isLoading"></canvas>

    <div v-if="isLoading" class="text-center my-4">
      <span>Đang tải dữ liệu...</span>
    </div>
  </div>
</template>

<script>
import OrderSummaryResponse from '@/models/dtos/statisticsDtos/orderSummaryResponse'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

export default {
  name: 'OrderSummary',
  props: {
    data: {
      type: OrderSummaryResponse,
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      selectedTimePeriod: 'date', // Mặc định là theo ngày
      revenueChartByTime: null, // Lưu trữ phiên bản biểu đồ doanh thu
      orderStatusChart: null, // Lưu trữ phiên bản biểu đồ trạng thái đơn hàng
    }
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => {
          this.updateCharts()
        })
      }
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.updateCharts()
    }
  },
  methods: {
    updateCharts() {
      this.renderrevenueChartByTime()
      this.renderOrderStatusChart()
    },
    renderrevenueChartByTime() {
      const ctx = document.getElementById('revenueChartByTime').getContext('2d')

      // Hủy biểu đồ hiện tại nếu nó đã tồn tại
      if (this.revenueChartByTime) {
        this.revenueChartByTime.destroy()
      }

      let revenueData, countData, labels

      // Tùy thuộc vào khoảng thời gian đã chọn, chúng ta lấy doanh thu và số lượng
      if (this.selectedTimePeriod === 'date') {
        revenueData = this.data.revenueByTimes['date'].map((item) => item.revenue)
        countData = this.data.revenueByTimes['date'].map((item) => item.count)
        labels = this.data.revenueByTimes['date'].map((item) => item.date.split('T')[0]) // Chỉ lấy ngày
      } else if (this.selectedTimePeriod === 'month') {
        revenueData = this.data.revenueByTimes['month'].map((item) => item.revenue)
        countData = this.data.revenueByTimes['month'].map((item) => item.count)
        labels = this.data.revenueByTimes['month'].map((item) => `${item.month}/${item.year}`)
      } else {
        // Theo năm
        revenueData = this.data.revenueByTimes['year'].map((item) => item.revenue)
        countData = this.data.revenueByTimes['year'].map((item) => item.count)
        labels = this.data.revenueByTimes['year'].map((item) => item.year)
      }

      // Khởi tạo biểu đồ mới
      this.revenueChartByTime = new Chart(ctx, {
        type: 'bar', // Sử dụng bar cho doanh thu
        data: {
          labels: labels,
          datasets: [
            {
              label: 'Doanh thu',
              data: revenueData,
              backgroundColor: 'rgba(75, 192, 192, 0.2)',
              borderColor: 'rgba(75, 192, 192, 1)',
              borderWidth: 1,
              type: 'line', // Dữ liệu doanh thu sẽ là đường
            },
            {
              label: 'Số lượng đơn hàng',
              data: countData,
              backgroundColor: 'rgba(255, 99, 132, 0.2)',
              borderColor: 'rgba(255, 99, 132, 1)',
              borderWidth: 1,
              type: 'bar', // Dữ liệu số lượng đơn sẽ là cột
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
            },
          },
          plugins: {
            tooltip: {
              mode: 'index',
              intersect: false,
            },
            legend: {
              display: true,
            },
          },
        },
      })
    },
    renderOrderStatusChart() {
      const ctx = document.getElementById('orderStatusChart').getContext('2d')

      // Hủy biểu đồ hiện tại nếu nó đã tồn tại
      if (this.orderStatusChart) {
        this.orderStatusChart.destroy()
      }

      const statusData = this.data.orderStatusStatistics.map((item) => item.count)
      const statusLabels = this.data.orderStatusStatistics.map((item) => item.status)

      this.orderStatusChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: statusLabels,
          datasets: [
            {
              label: 'Số lượng đơn hàng theo trạng thái',
              data: statusData,
              backgroundColor: 'rgba(153, 102, 255, 0.2)',
              borderColor: 'rgba(153, 102, 255, 1)',
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
            },
          },
        },
      })
    },
  },
  filters: {
    currency(value) {
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)
    },
  },
}
</script>

<style scoped>
canvas {
  max-width: 600px;
  margin: 20px auto;
}
table {
  width: 100%;
  border-collapse: collapse;
  margin: 20px 0;
}
th,
td {
  border: 1px solid #ddd;
  padding: 8px;
}
th {
  background-color: #f2f2f2;
}
</style>
