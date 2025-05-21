<template>
  <div class="row">
    <div class="col-12">
      <!-- Khung dữ liệu tóm tắt và biểu đồ -->
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Tổng quan khách hàng</h5>
        </div>
        <div class="card-body">
          <div class="row align-items-center g-3">
            <div class="col-md-6">
              <!-- Khung dữ liệu tóm tắt -->
              <div class="xp-chart-label">
                <ul class="list-inline text-center">
                  <li class="list-inline-item mx-3">
                    <p class="text-black">Giá trị mua hàng TB</p>
                    <h4 class="text-primary-gradient mb-3">
                      <i class="icon-wallet mr-2"></i
                      >{{ formatCurrency(this.data?.averagePurchaseAmount) }}
                    </h4>
                  </li>
                  <li class="list-inline-item mx-3">
                    <p class="text-black">Tổng giá trị mua hàng</p>
                    <h4 class="text-success-gradient mb-3">
                      <i class="icon-wallet mr-2"></i
                      >{{ formatCurrency(this.data?.totalPurchaseAmount) }}
                    </h4>
                  </li>
                </ul>
              </div>
            </div>

            <div class="col-md-6">
              <!-- Khung biểu đồ -->
              <div class="chart-container">
                <canvas id="customerChart" v-show="!isLoading"></canvas>
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
.chart-container {
  position: relative;
  width: 100%;
  height: 250px; /* Tùy chỉnh chiều cao cho biểu đồ */
}

canvas {
  max-width: 100%; /* Đảm bảo canvas không vượt quá chiều rộng */
  height: auto; /* Thiết lập chiều cao tự động */
}
</style>
