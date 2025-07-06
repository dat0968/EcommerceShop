<template>
  <div class="row mb-4">
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Thống kê mã giảm giá</h5>
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
                  :is-visible="data.totalCoupons === 0"
                  overlay-content="Không có dữ liệu mã giảm giá để thống kê."
                />
                <canvas id="couponUsageChart" width="400" height="300" class="m-3"></canvas>
              </div>
              <div class="col-12 col-md-6">
                <h6 class="text-primary mb-3">Top mã giảm giá được sử dụng nhiều nhất</h6>
                <div class="table-responsive">
                  <table class="table table-hover">
                    <thead>
                      <tr>
                        <th>Mã giảm giá</th>
                        <th>Số lần sử dụng</th>
                        <th>Tổng giảm giá</th>
                        <th>Doanh thu tạo ra</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(coupon, index) in data.topCoupons" :key="index">
                        <td>{{ coupon.couponCode }}</td>
                        <td>{{ coupon.usageCount }}</td>
                        <td>{{ formatCurrency(coupon.totalDiscount) }}</td>
                        <td>{{ formatCurrency(coupon.revenueGenerated) }}</td>
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
                <p class="text-black">Tổng số mã giảm giá</p>
                <h4 class="text-primary-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ data.totalCoupons }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Tổng số tiền giảm giá</p>
                <h4 class="text-success-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.totalDiscountAmount) }}
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
  name: 'CouponStatistic',
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
      if (!this.data || !this.data.topCoupons || this.data.topCoupons.length === 0) return

      const ctx = document.getElementById('couponUsageChart')
      const context = ctx.getContext('2d')
      new Chart(context, {
        type: 'bar',
        data: {
          labels: this.data.topCoupons.map((coupon) => coupon.couponCode),
          datasets: [
            {
              label: 'Số lần sử dụng',
              data: this.data.topCoupons.map((coupon) => coupon.usageCount),
              backgroundColor: 'rgba(54, 162, 235, 0.2)',
              borderColor: 'rgba(54, 162, 235, 1)',
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
                text: 'Số lần sử dụng',
              },
            },
          },
          plugins: {
            title: {
              display: true,
              text: 'Top mã giảm giá được sử dụng nhiều nhất',
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
