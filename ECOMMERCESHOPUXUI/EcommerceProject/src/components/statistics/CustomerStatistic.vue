<template>
  <div>
    <h2>Thống kê Khách hàng</h2>
    <div class="row mb-4">
      <div class="col" v-for="item in summaryList" :key="item.label">
        <div class="stat-box">
          <div class="stat-label">{{ item.label }}</div>
          <div class="stat-value">{{ item.value }}</div>
        </div>
      </div>
    </div>
    <canvas id="customerChart" v-show="!isLoading"></canvas>
    <div v-if="isLoading" class="text-center my-4">
      <span>Đang tải dữ liệu...</span>
    </div>
  </div>
</template>

<script>
// import CustomerStatisticsResponse from '@/models/dtos/statisticsDtos/customerStatisticsResponse'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

export default {
  name: 'CustomerStatistic',
  props: {
    data: {
      type: Object, // Để linh hoạt hơn, dùng Object thay vì CustomerStatisticsResponse
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      customerChart: null,
    }
  },
  computed: {
    summaryList() {
      // Hiển thị dữ liệu tĩnh dạng bảng/thẻ
      return [
        { label: 'Tổng khách hàng', value: this.data?.totalCustomers ?? 0 },
        { label: 'Khách hàng hoạt động', value: this.data?.totalActiveCustomers ?? 0 },
        { label: 'Khách hàng không hoạt động', value: this.data?.totalInactiveCustomers ?? 0 },
        {
          label: 'Giá trị mua hàng TB',
          value: this.formatCurrency(this.data?.averagePurchaseAmount),
        },
        {
          label: 'Tổng giá trị mua hàng',
          value: this.formatCurrency(this.data?.totalPurchaseAmount),
        },
      ]
    },
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => this.renderCustomerChart())
      }
    },
    data: {
      handler() {
        if (!this.isLoading) {
          this.$nextTick(() => this.renderCustomerChart())
        }
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderCustomerChart()
    }
  },
  methods: {
    renderCustomerChart() {
      const ctx = document.getElementById('customerChart').getContext('2d')
      if (this.customerChart) {
        this.customerChart.destroy()
      }
      const labels = ['Hoạt động', 'Không hoạt động']
      const dataValues = [
        this.data?.totalActiveCustomers ?? 0,
        this.data?.totalInactiveCustomers ?? 0,
      ]
      this.customerChart = new Chart(ctx, {
        type: 'pie',
        data: {
          labels: labels,
          datasets: [
            {
              label: 'Tỉ lệ khách hàng',
              data: dataValues,
              backgroundColor: ['rgba(75, 192, 192, 0.7)', 'rgba(255, 99, 132, 0.7)'],
              borderColor: ['rgba(75, 192, 192, 1)', 'rgba(255, 99, 132, 1)'],
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
