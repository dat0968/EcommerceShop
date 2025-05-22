<template>
  <div class="row mb-4">
    <!-- Card Thống kê Sản phẩm -->
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Thống kê Sản phẩm</h5>
        </div>
        <div class="card-body">
          <div class="row mb-4">
            <div class="col-md-4" v-for="item in summaryList" :key="item.label">
              <div class="card stat-box h-100">
                <div class="card-body d-flex flex-column">
                  <h5 class="card-title stat-label text-muted">{{ item.label }}</h5>
                  <div class="stat-value mt-auto text-center">
                    <h2 class="font-weight-bold">{{ item.value }}</h2>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="text-center my-4">
            <label for="timePeriod">Chọn khoảng thời gian:</label>
            <select v-model="selectedTimePeriod" @change="updateSalesChart" class="form-select">
              <option value="date">Theo ngày</option>
              <option value="month">Theo tháng</option>
              <option value="year">Theo năm</option>
            </select>
          </div>

          <div v-if="isLoading" class="text-center my-4">
            <span>Đang tải dữ liệu...</span>
          </div>
          <div class="row g-1" v-show="!isLoading">
            <!-- Điều chỉnh khoảng cách giữa các biểu đồ -->
            <div class="col-12 col-md-8 border-end">
              <canvas id="salesQuantityChart" class="m-3" width="700" height="350"></canvas>
            </div>
            <div class="col-12 col-md-4 border-end">
              <canvas id="productChart" class="m-3" width="300" height="350"></canvas>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

export default {
  name: 'ProductStatistic',
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
      productChart: null,
      salesQuantityChart: null,
      selectedTimePeriod: 'date',
    }
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng doanh thu', value: this.formatCurrency(this.data?.totalRevenue) },
        { label: 'Tổng giảm giá', value: this.formatCurrency(this.data?.totalDiscount) },
        { label: 'Giá trung bình', value: this.formatCurrency(this.data?.averagePrice) },
      ]
    },
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => {
          this.renderProductChart()
          this.updateSalesChart()
        })
      }
    },
    data: {
      handler() {
        if (!this.isLoading) {
          this.$nextTick(() => {
            this.renderProductChart()
            this.updateSalesChart()
          })
        }
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderProductChart()
      this.updateSalesChart()
    }
  },
  methods: {
    renderProductChart() {
      const canvas = document.getElementById('productChart')
      if (!canvas) return
      const ctx = canvas.getContext('2d')
      if (this.productChart) {
        this.productChart.destroy()
      }
      this.productChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['Đang bán', 'Ngừng bán'],
          datasets: [
            {
              label: 'Tình trạng sản phẩm',
              data: [this.data?.totalActiveProducts ?? 0, this.data?.totalInactiveProducts ?? 0],
              backgroundColor: ['rgba(54, 162, 235, 0.7)', 'rgba(255, 99, 132, 0.7)'],
              borderColor: ['rgba(54, 162, 235, 1)', 'rgba(255, 99, 132, 1)'],
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: false, // Giữ tỷ lệ khung hình cho	canvas
          plugins: {
            legend: {
              position: 'bottom',
              labels: {
                boxWidth: 10,
                padding: 10,
              },
            },
          },
        },
      })
    },
    updateSalesChart() {
      const canvas = document.getElementById('salesQuantityChart')
      if (!canvas) return
      const ctx = canvas.getContext('2d')
      if (this.salesQuantityChart) {
        this.salesQuantityChart.destroy()
      }

      let salesData
      let labels
      let revenueData
      let quantityData

      if (this.selectedTimePeriod === 'date') {
        salesData = this.data.salesByTimes?.date || []
        labels = salesData.map((item) => item.date.split('T')[0])
        revenueData = salesData.map((item) => item.revenue)
        quantityData = salesData.map((item) => item.count)
      } else if (this.selectedTimePeriod === 'month') {
        salesData = this.data.salesByTimes?.month || []
        labels = salesData.map((item) => `${item.month}/${item.year}`)
        revenueData = salesData.map((item) => item.revenue)
        quantityData = salesData.map((item) => item.count)
      } else if (this.selectedTimePeriod === 'year') {
        salesData = this.data.salesByTimes?.year || []
        labels = salesData.map((item) => item.year)
        revenueData = salesData.map((item) => item.revenue)
        quantityData = salesData.map((item) => item.count)
      }

      this.salesQuantityChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: labels,
          datasets: [
            {
              label: 'Doanh thu',
              data: revenueData,
              type: 'line',
              borderColor: 'rgba(75, 192, 192, 1)',
              backgroundColor: 'rgba(75, 192, 192, 0.2)',
              borderWidth: 2,
              fill: true,
              yAxisID: 'y',
            },
            {
              label: 'Số lượng bán',
              data: quantityData,
              type: 'bar',
              backgroundColor: 'rgba(255, 206, 86, 0.7)',
              borderColor: 'rgba(255, 206, 86, 1)',
              borderWidth: 1,
              yAxisID: 'y1', // Sử dụng trục y thứ hai cho số lượng
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              position: 'left',
            },
            y1: {
              beginAtZero: true,
              position: 'right',
              grid: {
                drawOnChartArea: false, // Không vẽ lưới cho trục bên phải
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
}
</script>

<style scoped>
.stat-box {
  border: 1px solid #ddd;
  padding: 10px;
  border-radius: 5px;
  text-align: center;
}
canvas {
  min-height: 20em;
  max-height: 25em;
}
</style>
