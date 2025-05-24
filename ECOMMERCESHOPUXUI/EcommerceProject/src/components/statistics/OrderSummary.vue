<template>
  <div class="row mb-4">
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Doanh thu theo thời gian</h5>
        </div>
        <div class="card-body">
          <div v-if="isLoading" class="text-center my-4">
            <span>Đang tải dữ liệu...</span>
          </div>
          <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
            <span>Không có dữ liệu để hiển thị.</span>
          </div>
          <div v-else>
            <div class="text-center my-4">
              <select v-model="selectedTimePeriod" @change="updateCharts" class="form-select">
                <option value="date">Theo ngày</option>
                <option value="month">Theo tháng</option>
                <option value="year">Theo năm</option>
              </select>
            </div>
            <div class="row g-1">
              <div class="col-12 col-md-8 border-end">
                <canvas id="revenueChartByTime" class="m-3" width="700" height="350"></canvas>
              </div>
              <div class="col-12 col-md-4 border-end">
                <canvas id="orderStatusChart" class="m-3" width="300" height="350"></canvas>
              </div>
            </div>
          </div>
        </div>
        <div class="card-footer" v-if="!isLoading && data && Object.keys(data).length > 0">
          <div class="xp-chart-label">
            <ul class="list-inline text-center">
              <li class="list-inline-item mx-3">
                <p class="text-black">Tổng số đơn hàng</p>
                <h4 class="text-primary-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ totalOrders ?? 0 }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Tổng doanh thu</p>
                <h4 class="text-success-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(totalRevenue ?? 0) }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Giá trị đơn hàng trung bình</p>
                <h4 class="text-info-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(averageOrderValue ?? 0) }}
                </h4>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import OrderSummaryResponse from '@/models/dtos/statisticsDtos/orderSummaryResponse'
import { Chart, registerables } from 'chart.js'

import { formatCurrency } from '@/constants/formatCurrency'
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
      totalOrders: 0, // Tổng số đơn hàng
      totalRevenue: 0, // Tổng doanh thu
      averageOrderValue: 0, // Giá trị đơn hàng trung bình
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
    formatCurrency,
    updateCharts() {
      this.calculateOverviewData() // Tính toán dữ liệu tổng quan
      this.renderrevenueChartByTime()
      this.renderOrderStatusChart()
    },
    calculateOverviewData() {
      var statusDataByTime
      // console.log(this.data)

      switch (this.selectedTimePeriod) {
        case 'date': {
          statusDataByTime = this.data.revenueByTimes.date
          break
        }
        case 'month': {
          statusDataByTime = this.data.revenueByTimes.month
          break
        }
        case 'year': {
          statusDataByTime = this.data.revenueByTimes.year
          break
        }
      }
      this.totalOrders = statusDataByTime.reduce((acc, item) => acc + item.count, 0) // Tổng số đơn hàng
      this.totalRevenue = statusDataByTime.reduce((acc, item) => acc + item.revenue, 0) // Tổng doanh thu
      this.averageOrderValue = this.totalOrders > 0 ? this.totalRevenue / this.totalOrders : 0 // Giá trị đơn hàng trung bình
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
              yAxisID: 'y-revenue', // Gán trục y cho doanh thu
            },
            {
              label: 'Số lượng đơn hàng',
              data: countData,
              backgroundColor: 'rgba(255, 99, 132, 0.2)',
              borderColor: 'rgba(255, 99, 132, 1)',
              borderWidth: 1,
              type: 'bar', // Dữ liệu số lượng đơn sẽ là cột
              yAxisID: 'y-count', // Gán trục y cho số lượng
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            'y-revenue': {
              beginAtZero: true,
              title: {
                display: true,
                text: 'Doanh thu',
              },
            },
            'y-count': {
              // Trục y cho số lượng đơn hàng
              beginAtZero: true,
              position: 'right',
              title: {
                display: true,
                text: 'Số lượng đơn hàng',
              },
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
      // console.log(this.data.orderStatusStatistics)

      var statusDataByTime
      switch (this.selectedTimePeriod) {
        case 'date': {
          statusDataByTime = this.data.orderStatusStatistics.date
          break
        }
        case 'month': {
          statusDataByTime = this.data.orderStatusStatistics.month
          break
        }
        case 'year': {
          statusDataByTime = this.data.orderStatusStatistics.year
          break
        }
      }
      // console.log(statusDataByTime)

      const statusData = statusDataByTime.map((item) => item.count)
      const statusLabels = statusDataByTime.map((item) => item.status)

      this.orderStatusChart = new Chart(ctx, {
        type: 'pie', // Thay đổi loại biểu đồ thành 'pie'
        data: {
          labels: statusLabels,
          datasets: [
            {
              label: 'Số lượng đơn hàng theo trạng thái',
              data: statusData,
              backgroundColor: [
                'rgba(255, 99, 132, 0.2)',
                'rgba(54, 162, 235, 0.2)',
                'rgba(255, 206, 86, 0.2)',
                'rgba(75, 192, 192, 0.2)',
                'rgba(153, 102, 255, 0.2)',
                'rgba(255, 159, 64, 0.2)',
              ],
              borderColor: [
                'rgba(255, 99, 132, 1)',
                'rgba(54, 162, 235, 1)',
                'rgba(255, 206, 86, 1)',
                'rgba(75, 192, 192, 1)',
                'rgba(153, 102, 255, 1)',
                'rgba(255, 159, 64, 1)',
              ],
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          plugins: {
            tooltip: {
              callbacks: {
                label: function (tooltipItem) {
                  const label = tooltipItem.label || ''
                  const value = tooltipItem.raw || 0 // Giá trị tương ứng
                  return `${label}: ${value}` // Định dạng label với giá trị
                },
              },
            },
            legend: {
              position: 'right', // Thiết lập vị trí legend ở bên phải
            },
          },
        },
      })
    },
  },
  filters: {},
}
</script>

<style scoped>
.chart-container {
  position: relative;
  width: 100%;
}

canvas {
  width: 100%;
  min-height: 20em;
  max-height: 30em;
}
</style>
