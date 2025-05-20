<template>
  <div>
    <h2>Thống kê Nhân viên</h2>
    <div class="row mb-4">
      <div class="col" v-for="item in summaryList" :key="item.label">
        <div class="stat-box">
          <div class="stat-label">{{ item.label }}</div>
          <div class="stat-value">{{ item.value }}</div>
        </div>
      </div>
    </div>
    <canvas id="employeeChart"></canvas>
  </div>
</template>

<script>
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)
export default {
  name: 'EmployeeStatistic',
  props: {
    data: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      employeeChart: null,
    }
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng nhân viên', value: this.data?.totalEmployees ?? 0 },
        { label: 'Nhân viên đang làm', value: this.data?.totalActiveEmployees ?? 0 },
        { label: 'Nhân viên nghỉ việc', value: this.data?.totalInactiveEmployees ?? 0 },
        { label: 'Lương trung bình', value: this.formatCurrency(this.data?.averageSalary) },
        { label: 'Tổng lương', value: this.formatCurrency(this.data?.totalSalary) },
      ]
    },
  },
  watch: {
    data: {
      handler() {
        this.$nextTick(() => this.renderEmployeeChart())
      },
      deep: true,
    },
  },
  mounted() {
    this.renderEmployeeChart()
  },
  methods: {
    renderEmployeeChart() {
      const ctx = document.getElementById('employeeChart').getContext('2d')
      if (this.employeeChart) {
        this.employeeChart.destroy()
      }
      this.employeeChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['Đang làm', 'Nghỉ việc'],
          datasets: [
            {
              label: 'Tình trạng nhân viên',
              data: [this.data?.totalActiveEmployees ?? 0, this.data?.totalInactiveEmployees ?? 0],
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
