<template>
  <div class="card" style="height: 100%">
    <div class="card-header bg-white d-flex justify-content-between align-items-center">
      <h5 class="card-title text-black mb-0">Sản phẩm thu theo thời gian</h5>
      <div class="gap-2 d-flex gap-5 align-items-center" style="position: relative">
        <div class="text-center my-4">
          <select v-model="selectedTimePeriod" @change="updateSalesChart" class="form-select">
            <option value="date">Theo ngày</option>
            <option value="month">Theo tháng</option>
            <option value="year">Theo năm</option>
          </select>
        </div>
        <span
          @click="toggleProductStatusChart"
          class="icon-layers"
          style="cursor: pointer"
          title="Biểu đồ trạng thái sản phẩm"
        ></span>
        <div
          class="border rounded-except-top-right border p-1 bg-white"
          v-show="showProductStatusChart"
          style="position: absolute; top: 60px; right: 0"
        >
          <canvas id="productChart" v-if="!isLoading"></canvas>
          <div v-show="isLoading" class="text-center my-4">
            <span>Đang tải dữ liệu...</span>
          </div>
        </div>
      </div>
    </div>
    <div class="card-body flex align-items-center m-3">
      <div class="chart-container flex align-items-center position-relative">
        <Overlay
          :is-visible="!hasSalesChartData && !isLoading"
          overlay-content="Không có dữ liệu để hiển thị biểu đồ."
        />
        <div v-if="isLoading" class="text-center my-4">
          <span>Đang tải dữ liệu...</span>
        </div>
        <div v-else>
          <canvas id="salesQuantityChartByDay" v-if="selectedTimePeriod === 'date'"></canvas>
          <canvas id="salesQuantityChartByMonth" v-if="selectedTimePeriod === 'month'"></canvas>
          <canvas id="salesQuantityChartByYear" v-if="selectedTimePeriod === 'year'"></canvas>
        </div>
      </div>
    </div>
    <div class="card-footer" v-if="!isLoading && data && Object.keys(data).length > 0">
      <div class="xp-chart-label">
        <ul class="list-inline text-center">
          <li class="list-inline-item mx-3" v-for="item in summaryList" :key="item.label">
            <p class="text-black">{{ item.label }}</p>
            <h4 class="text-primary-gradient mb-3">
              <i class="icon-wallet mr-2"></i>{{ item.value }}
            </h4>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js'
import { formatCurrency } from '@/constants/formatCurrency'
import Overlay from '../common/Overlay.vue'
Chart.register(...registerables)

export default {
  name: 'ProductStatistic',
  components: { Overlay },
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
      showProductStatusChart: false,
      hasSalesChartData: true,
    }
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng doanh thu', value: this.formatCurrency(this.data?.totalRevenue ?? 0) },
        { label: 'Tổng giảm giá', value: this.formatCurrency(this.data?.totalDiscount ?? 0) },
        { label: 'Giá trung bình', value: this.formatCurrency(this.data?.averagePrice ?? 0) },
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
    formatCurrency,
    toggleProductStatusChart() {
      this.showProductStatusChart = !this.showProductStatusChart
    },
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
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'right',
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
      let canvasId = ''
      if (this.selectedTimePeriod === 'date') {
        canvasId = 'salesQuantityChartByDay'
      } else if (this.selectedTimePeriod === 'month') {
        canvasId = 'salesQuantityChartByMonth'
      } else if (this.selectedTimePeriod === 'year') {
        canvasId = 'salesQuantityChartByYear'
      }

      const canvas = document.getElementById(canvasId)
      if (!canvas) return
      const ctx = canvas.getContext('2d')
      if (this.salesQuantityChart) {
        this.salesQuantityChart.destroy()
      }

      let salesData = []
      let labels = []
      let revenueData = []
      let quantityData = []

      // Lấy dữ liệu theo khoảng thời gian đã chọn
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

      // Kiểm tra dữ liệu để hiển thị overlay
      this.hasSalesChartData =
        salesData &&
        salesData.length > 0 &&
        (revenueData.some((v) => v > 0) || quantityData.some((v) => v > 0))

      // Nếu không có dữ liệu, tạo biểu đồ trắng với khung hình
      if (!this.hasSalesChartData) {
        labels = ['']
        revenueData = [0]
        quantityData = [0]
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
              yAxisID: 'y1',
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
                drawOnChartArea: false,
              },
            },
          },
        },
      })
    },
  },
}
</script>
