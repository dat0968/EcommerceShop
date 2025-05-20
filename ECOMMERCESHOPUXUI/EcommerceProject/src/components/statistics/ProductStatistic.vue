<template>
  <div>
    <h2>Thống kê Sản phẩm</h2>
    <div class="row mb-4">
      <div class="col" v-for="item in summaryList" :key="item.label">
        <div class="stat-box">
          <div class="stat-label">{{ item.label }}</div>
          <div class="stat-value">{{ item.value }}</div>
        </div>
      </div>
    </div>
    <canvas id="productChart"></canvas>
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
  },
  data() {
    return {
      productChart: null,
    }
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng sản phẩm', value: this.data?.totalProducts ?? 0 },
        { label: 'Sản phẩm đang bán', value: this.data?.totalActiveProducts ?? 0 },
        { label: 'Sản phẩm ngừng bán', value: this.data?.totalInactiveProducts ?? 0 },
        { label: 'Tổng doanh thu', value: this.formatCurrency(this.data?.totalRevenue) },
        { label: 'Tổng giảm giá', value: this.formatCurrency(this.data?.totalDiscount) },
        { label: 'Giá trung bình', value: this.formatCurrency(this.data?.averagePrice) },
      ]
    },
  },
  watch: {
    data: {
      handler() {
        this.$nextTick(() => this.renderProductChart())
      },
      deep: true,
    },
  },
  mounted() {
    this.renderProductChart()
  },
  methods: {
    renderProductChart() {
      const ctx = document.getElementById('productChart').getContext('2d')
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
          plugins: {
            legend: {
              position: 'bottom',
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
