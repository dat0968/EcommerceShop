<template>
  <div class="row mb-4">
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Thống kê danh mục</h5>
        </div>
        <div class="card-body">
          <div v-if="isLoading" class="text-center my-4">
            <LoadingSpinner />
          </div>
          <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
            <NoDataMessage />
          </div>
          <div v-else>
            <div class="row g-3">
              <div class="col-12 col-md-6 border-end position-relative">
                <Overlay
                  :is-visible="data.totalCategories === 0"
                  overlay-content="Không có dữ liệu danh mục để thống kê."
                />
                <canvas id="categoryRevenueChart" width="400" height="300" class="m-3"></canvas>
              </div>
              <div class="col-12 col-md-6">
                <h6 class="text-primary mb-3">Top danh mục có doanh thu cao nhất</h6>
                <div class="table-responsive">
                  <table class="table table-hover">
                    <thead>
                      <tr>
                        <th>Tên danh mục</th>
                        <th>Số sản phẩm bán ra</th>
                        <th>Doanh thu</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(category, index) in data.topCategories" :key="index">
                        <td>{{ category.categoryName }}</td>
                        <td>{{ category.productsSoldCount }}</td>
                        <td>{{ formatCurrency(category.totalRevenue) }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="card-footer" v-if="!isLoading && data && Object.keys(data).length > 0">
          <div class="xp-chart-label">
            <ul class="list-inline text-center">
              <li class="list-inline-item mx-3">
                <p class="text-black">Tổng số danh mục</p>
                <h4 class="text-primary-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ data.totalCategories }}
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
import Overlay from '@/components/common/Overlay.vue'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import NoDataMessage from '@/components/common/NoDataMessage.vue'
import { Chart, registerables } from 'chart.js'
import { formatCurrency } from '@/constants/formatCurrency'
Chart.register(...registerables)

export default {
  name: 'CategoryStatistic',
  components: { Overlay, LoadingSpinner, NoDataMessage },
  props: {
    data: {
      default: () => ({}),
    },
    isLoading: {
      type: Boolean,
      default: false,
    },
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(() => {
          this.renderChart()
        })
      }
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.renderChart()
    }
  },
  methods: {
    formatCurrency,
    renderChart() {
      if (!this.data || !this.data.topCategories || this.data.topCategories.length === 0) return

      const ctx = document.getElementById('categoryRevenueChart')
      const context = ctx.getContext('2d')
      new Chart(context, {
        type: 'bar',
        data: {
          labels: this.data.topCategories.map((category) => category.categoryName),
          datasets: [
            {
              label: 'Doanh thu',
              data: this.data.topCategories.map((category) => category.totalRevenue),
              backgroundColor: 'rgba(75, 192, 192, 0.2)',
              borderColor: 'rgba(75, 192, 192, 1)',
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              title: {
                display: true,
                text: 'Doanh thu',
              },
            },
          },
          plugins: {
            title: {
              display: true,
              text: 'Top danh mục có doanh thu cao nhất',
              font: {
                size: 16,
              },
            },
          },
        },
      })
    },
  },
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
