<template>
  <div class="col-12">
    <!-- Khung dữ liệu tóm tắt và biểu đồ -->
    <div class="card m-b-30">
      <div class="card-header bg-white">
        <h5 class="card-title text-black mb-0">Tổng quan lương nhân viên</h5>
      </div>
      <div class="card-body">
        <div v-if="isLoading" class="text-center my-4">
          <span>Đang tải dữ liệu...</span>
        </div>
        <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
          <span>Không có dữ liệu để hiển thị.</span>
        </div>
        <div v-else class="row align-items-center g-3">
          <div class="col-md-6">
            <!-- Khung dữ liệu tóm tắt -->
            <div class="xp-chart-label">
              <ul class="list-inline text-center">
                <li class="list-inline-item mx-3">
                  <p class="text-black">Lương trung bình</p>
                  <h4 class="text-primary-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(this.data?.averageSalary) }}
                  </h4>
                </li>
                <li class="list-inline-item mx-3">
                  <p class="text-black">Tổng lương</p>
                  <h4 class="text-success-gradient mb-3">
                    <i class="icon-wallet mr-2"></i>{{ formatCurrency(this.data?.totalSalary) }}
                  </h4>
                </li>
              </ul>
            </div>
          </div>

          <div class="col-md-6">
            <!-- Khung biểu đồ -->
            <div class="chart-container">
              <canvas id="employeeChart" v-show="!isLoading"></canvas>
              <div v-show="chartError" class="text-center my-4 text-danger">
                <span>Không thể tạo biểu đồ do thiếu hoặc lỗi dữ liệu.</span>
              </div>
              <div v-show="isLoading" class="text-center my-4">
                <span>Đang tải dữ liệu...</span>
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
    isLoading: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      employeeChart: null,
      chartError: false,
    }
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => this.renderEmployeeChart())
      }
    },
    data: {
      handler() {
        this.$nextTick(() => this.renderEmployeeChart())
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderEmployeeChart()
    }
  },
  methods: {
    renderEmployeeChart() {
      this.chartError = false
      try {
        const ctx = document.getElementById('employeeChart')
        if (!ctx || !this.data) {
          this.chartError = true
          return
        }
        const context = ctx.getContext('2d')
        if (this.employeeChart) {
          this.employeeChart.destroy()
        }
        // Kiểm tra dữ liệu hợp lệ
        const active = this.data?.totalActiveEmployees ?? 0
        const inactive = this.data?.totalInactiveEmployees ?? 0
        if (active === 0 && inactive === 0) {
          this.chartError = true
          return
        }
        this.employeeChart = new Chart(context, {
          type: 'doughnut',
          data: {
            labels: ['Đang làm', 'Nghỉ việc'],
            datasets: [
              {
                label: 'Tình trạng nhân viên',
                data: [active, inactive],
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
      } catch (e) {
        this.chartError = true
        console.error(e)
      }
    },
    formatCurrency(val) {
      if (typeof val !== 'number') return '0'
      return val.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })
    },
  },
}
</script>
