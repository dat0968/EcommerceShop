<template>
  <div class="row mb-4">
    <div class="col-12">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Phân tích tồn kho</h5>
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
              <div class="col-12">
                <h6 class="text-primary mb-3">Sản phẩm có số lượng tồn kho thấp</h6>
                <div class="table-responsive">
                  <table class="table table-hover">
                    <thead>
                      <tr>
                        <th>Mã sản phẩm</th>
                        <th>Tên sản phẩm</th>
                        <th>Số lượng tồn kho</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(product, index) in data.lowStockProducts" :key="index">
                        <td>{{ product.productId }}</td>
                        <td>{{ product.productName }}</td>
                        <td>{{ product.stockQuantity }}</td>
                      </tr>
                    </tbody>
                  </table>
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
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

export default {
  name: 'InventoryAnalysis',
  components: { LoadingSpinner, NoDataMessage },
  props: {
    data: {
      default: () => ({}),
    },
    isLoading: {
      type: Boolean,
      default: false,
    },
  },
}
</script>

<style scoped>
.table th,
.table td {
  vertical-align: middle;
}
</style>
