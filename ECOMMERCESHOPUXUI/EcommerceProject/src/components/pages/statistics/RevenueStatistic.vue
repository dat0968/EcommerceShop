<template>
  <div class="row">
    <div class="col-md-12 col-lg-12 col-xl-12 mb-4">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Thống kê Doanh thu</h5>
        </div>
        <div class="card-body">
          <div v-if="isLoading" class="text-center my-4">
            <span>Đang tải dữ liệu...</span>
          </div>
          <div v-else-if="!data || Object.keys(data).length === 0" class="text-center my-4">
            <span>Không có dữ liệu để hiển thị.</span>
          </div>
          <div v-else class="xp-chart-label">
            <ul class="list-inline text-center">
              <li class="list-inline-item mx-3">
                <p class="text-black">Tổng doanh thu</p>
                <h4 class="text-primary-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.totalRevenue) }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Doanh thu TB ngày</p>
                <h4 class="text-success-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.averageDailyRevenue) }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Doanh thu TB tháng</p>
                <h4 class="text-info-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.averageMonthlyRevenue) }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Doanh thu cao nhất</p>
                <h4 class="text-warning-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.highestRevenue) }}
                </h4>
              </li>
              <li class="list-inline-item mx-3">
                <p class="text-black">Doanh thu thấp nhất</p>
                <h4 class="text-danger-gradient mb-3">
                  <i class="icon-wallet mr-2"></i>{{ formatCurrency(data.lowestRevenue) }}
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
import { formatCurrency } from '@/constants/formatCurrency'
export default {
  name: 'RevenueStatistic',
  props: {
    data: {
      type: Object,
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  computed: {
    summaryList() {
      return [
        { label: 'Tổng doanh thu', value: this.formatCurrency(this.data?.totalRevenue) },
        { label: 'Doanh thu TB ngày', value: this.formatCurrency(this.data?.averageDailyRevenue) },
        {
          label: 'Doanh thu TB tháng',
          value: this.formatCurrency(this.data?.averageMonthlyRevenue),
        },
        { label: 'Doanh thu cao nhất', value: this.formatCurrency(this.data?.highestRevenue) },
        { label: 'Doanh thu thấp nhất', value: this.formatCurrency(this.data?.lowestRevenue) },
      ]
    },
  },
  methods: {
    formatCurrency,
  },
}
</script>

<style scoped>
.revenue-statistic {
  padding: 20px;
}
.stat-box {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px; /* Có thể giữ padding để tạo không gian */
  text-align: center;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
}
.stat-label {
  font-size: 16px;
  color: #666;
}
.stat-value {
  font-size: 24px;
  font-weight: bold;
  color: #222;
}
</style>
