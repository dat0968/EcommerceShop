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
          :is-visible="data.totalCategories === 0"
          overlay-content="Không có dữ liệu danh mục để thống kê."
        />
        <canvas ref="categoryRevenueChartCanvas" class="w-100" style="min-height: 22em;"></canvas>

        <div class="text-center mt-4">
          <p class="text-black mb-1">Tổng số danh mục</p>
          <h4 class="text-primary-gradient">
            <i class="icon-wallet mr-2"></i>{{ data.totalCategories }}
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
  data() {
    return {
      categoryChart: null,
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
    this.categoryChart?.destroy()
  },
  methods: {
    formatCurrency,
    renderChart() {
      const canvas = this.$refs.categoryRevenueChartCanvas
      if (!canvas || !this.data?.topCategories?.length) {
        this.categoryChart?.destroy()
        this.categoryChart = null
        return
      }

      const context = canvas.getContext('2d')
      if (!context) return

      const chartData = {
        labels: this.data.topCategories.map((cat) => cat.categoryName),
        datasets: [
          {
            label: 'Doanh thu',
            data: this.data.topCategories.map((cat) => cat.totalRevenue),
            backgroundColor: 'rgba(54, 162, 235, 0.6)',
            borderColor: 'rgba(54, 162, 235, 1)',
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
              text: 'Doanh thu (VNĐ)',
            },
          },
          x: {
            title: {
              display: true,
              text: 'Danh mục',
            },
          },
        },
        plugins: {
          title: {
            display: true,
            text: 'Top danh mục có doanh thu cao nhất',
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

      this.categoryChart?.destroy()
      this.categoryChart = new Chart(context, {
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
canvas {
  width: 100%;
  min-height: 20em;
  max-height: 30em;
}
</style>
