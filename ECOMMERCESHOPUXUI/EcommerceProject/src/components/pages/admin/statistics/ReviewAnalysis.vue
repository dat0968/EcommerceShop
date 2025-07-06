<template>
  <div class="row mb-4">
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Phân tích đánh giá</h5>
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
                  :is-visible="!data.averageRating"
                  overlay-content="Không có dữ liệu đánh giá để thống kê."
                />
                <canvas id="reviewRatingChart" width="400" height="300" class="m-3"></canvas>
              </div>
              <div class="col-12 col-md-6">
                <h6 class="text-primary mb-3">Thông tin sản phẩm được đánh giá</h6>
                <div class="table-responsive">
                  <table class="table table-hover">
                    <thead>
                      <tr>
                        <th>Loại</th>
                        <th>Tên sản phẩm</th>
                        <th>Đánh giá trung bình</th>
                        <th>Số lượng đánh giá</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="data.mostReviewedProduct">
                        <td>Được đánh giá nhiều nhất</td>
                        <td>{{ data.mostReviewedProduct.productName }}</td>
                        <td>{{ data.mostReviewedProduct.averageRating.toFixed(1) }}</td>
                        <td>{{ data.mostReviewedProduct.reviewCount }}</td>
                      </tr>
                      <tr v-if="data.highestRatedProduct">
                        <td>Được đánh giá cao nhất</td>
                        <td>{{ data.highestRatedProduct.productName }}</td>
                        <td>{{ data.highestRatedProduct.averageRating.toFixed(1) }}</td>
                        <td>{{ data.highestRatedProduct.reviewCount }}</td>
                      </tr>
                      <tr v-if="data.lowestRatedProduct">
                        <td>Được đánh giá thấp nhất</td>
                        <td>{{ data.lowestRatedProduct.productName }}</td>
                        <td>{{ data.lowestRatedProduct.averageRating.toFixed(1) }}</td>
                        <td>{{ data.lowestRatedProduct.reviewCount }}</td>
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
                <p class="text-black">Đánh giá trung bình</p>
                <h4 class="text-primary-gradient mb-3">
                  <i class="icon-star mr-2"></i
                  >{{ data.averageRating ? data.averageRating.toFixed(1) : 0 }}
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
Chart.register(...registerables)

export default {
  name: 'ReviewAnalysis',
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
    renderChart() {
      if (!this.data || !this.data.averageRating) return

      const ctx = document.getElementById('reviewRatingChart')
      const context = ctx.getContext('2d')
      new Chart(context, {
        type: 'bar',
        data: {
          labels: ['Đánh giá trung bình'],
          datasets: [
            {
              label: 'Điểm đánh giá',
              data: [this.data.averageRating],
              backgroundColor: 'rgba(255, 206, 86, 0.2)',
              borderColor: 'rgba(255, 206, 86, 1)',
              borderWidth: 1,
            },
          ],
        },
        options: {
          responsive: true,
          scales: {
            y: {
              beginAtZero: true,
              max: 5,
              title: {
                display: true,
                text: 'Điểm đánh giá (0-5)',
              },
            },
          },
          plugins: {
            title: {
              display: true,
              text: 'Đánh giá trung bình của sản phẩm',
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
