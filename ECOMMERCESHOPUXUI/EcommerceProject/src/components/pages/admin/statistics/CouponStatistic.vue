<template>
  <div class="row mb-4">
    <div class="col-12">
      <div v-if="isLoading" class="text-center my-4">
        <LoadingSpinner />
      </div>

      <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
        <NoDataMessage />
      </div>

      <div v-else class="position-relative bg-white border rounded p-3">
        <Overlay
          :is-visible="data.totalCoupons === 0"
          overlay-content="Không có dữ liệu mã giảm giá để thống kê."
        />
        <canvas ref="couponUsageChartCanvas" class="w-100" style="min-height: 22em;"></canvas>

        <div class="text-center mt-4">
          <p class="text-black mb-1">Tổng số mã giảm giá</p>
          <h4 class="text-primary-gradient">
            <i class="icon-wallet mr-2"></i>{{ data.totalCoupons }}
          </h4>
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
  name: 'CouponStatistic',
  components: { Overlay, LoadingSpinner, NoDataMessage },
  props: {
    data: { default: () => ({}) },
    isLoading: { type: Boolean, default: false },
  },
  data() {
    return {
      couponChart: null,
    }
  },
  watch: {
    isLoading(newVal) {
      if (!newVal) {
        this.$nextTick(this.renderChart)
      }
    },
    data: {
      handler() {
        this.$nextTick(this.renderChart)
      },
      deep: true,
    },
  },
  mounted() {
    if (!this.isLoading) {
      this.$nextTick(this.renderChart)
    }
  },
  beforeUnmount() {
    this.couponChart?.destroy()
  },
  methods: {
    renderChart() {
      const canvas = this.$refs.couponUsageChartCanvas
      if (!canvas || !this.data?.topCoupons?.length) {
        this.couponChart?.destroy()
        this.couponChart = null
        return
      }

      const ctx = canvas.getContext('2d')
      if (!ctx) return

      const labels = this.data.topCoupons.map((coupon) => coupon.couponCode)
      const values = this.data.topCoupons.map((coupon) => coupon.usageCount)

      const chartData = {
        labels,
        datasets: [
          {
            label: 'Số lần sử dụng',
            data: values,
            backgroundColor: 'rgba(255, 159, 64, 0.6)',
            borderColor: 'rgba(255, 159, 64, 1)',
            borderWidth: 1,
          },
        ],
      }

      const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: 'Số lần sử dụng',
            },
          },
          x: {
            title: {
              display: true,
              text: 'Mã giảm giá',
            },
          },
        },
        plugins: {
          title: {
            display: true,
            text: 'Top mã giảm giá được sử dụng nhiều nhất',
            font: {
              size: 16,
              weight: 'bold',
            },
          },
          legend: {
            display: false,
          },
        },
      }

      this.couponChart?.destroy()
      this.couponChart = new Chart(ctx, {
        type: 'bar',
        data: chartData,
        options: chartOptions,
      })
    },
  },
}
</script>

<style scoped>
.icon-wallet {
  margin-right: 5px;
}
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
