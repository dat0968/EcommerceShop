<template>
  <div class="row">
    <div class="col-12">
      <!-- Khung dữ liệu tóm tắt và biểu đồ -->
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Tổng quan lương nhân viên</h5>
        </div>
        <div class="card-body">
          <div class="row align-items-center g-3">
            <div class="col-md-6">
              <!-- Khung dữ liệu tóm tắt -->
              <div class="xp-widget-box text-black">
                <h4 class="mb-0 font-26">{{ formatCurrency(this.data?.averageSalary) }}</h4>
                <p class="mb-2">Lương trung bình</p>
                <p class="mb-0">
                  <span class="f-w-7">{{ formatCurrency(this.data?.totalSalary) }}</span>
                  <br />
                  <span class="font-12">Tổng lương</span>
                </p>
              </div>
            </div>

            <div class="col-md-6">
              <!-- Khung biểu đồ -->
              <div class="chart-container">
                <canvas id="employeeChart" v-show="!isLoading"></canvas>
                <div v-if="isLoading" class="text-center my-4">
                  <span>Đang tải dữ liệu...</span>
                </div>
              </div>
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
.chart-container {
  max-width: 400px;
  margin: 20px auto;
  display: block;
}
</style>
