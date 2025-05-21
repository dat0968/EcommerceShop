<template>
  <div v-if="!isLoading" class="row">
    <div class="col-md-12 col-lg-8 col-xl-8 align-self-center">
      <div class="card bg-white m-b-30">
        <div class="card-header bg-white flex justify-content-between">
          <h5 class="card-title text-black mb-0">{{ tableTitle }}</h5>
          <div class="mb-3">
            <select id="dataSelect" class="form-select" v-model="selectedTable">
              <option value="products" :disabled="!data.topProducts.length">
                Sản phẩm bán chạy nhất
              </option>
              <option value="customers" :disabled="!data.topCustomers.length">
                Khách hàng hàng đầu
              </option>
              <option value="employees" :disabled="!data.topEmployees.length">
                Nhân viên hàng đầu
              </option>
              <option value="combos" :disabled="!data.topCombos.length">Combo hàng đầu</option>
            </select>
          </div>
        </div>
        <div class="card-body">
          <div class="">
            <ProductTable v-if="selectedTable === 'products'" :products="data.topProducts" />
          </div>
          <div class="">
            <CustomerTable v-if="selectedTable === 'customers'" :customers="data.topCustomers" />
          </div>
          <div class="">
            <EmployeeTable v-if="selectedTable === 'employees'" :employees="data.topEmployees" />
          </div>
          <div class="">
            <ComboTable v-if="selectedTable === 'combos'" :combos="data.topCombos" />
          </div>
        </div>
      </div>
    </div>

    <div class="col-md-12 col-lg-4 col-xl-4">
      <div class="card m-b-30">
        <div class="card-header bg-white">
          <h5 class="card-title text-black mb-0">Lịch</h5>
        </div>
        <div class="card-body">
          <div data-language="en" class="datepicker-here"></div>
        </div>
      </div>
    </div>
  </div>

  <div v-if="isLoading" class="text-center my-4">
    <span>Đang tải dữ liệu...</span>
  </div>
</template>

<script>
import DatatableStatisticsResponse from '@/models/dtos/statisticsDtos/datatableStatisticsResponse'
import ProductTable from '@/components/statistics/datatables/ProductTable.vue'
import CustomerTable from '@/components/statistics/datatables/CustomerTable.vue'
import EmployeeTable from '@/components/statistics/datatables/EmployeeTable.vue'
import ComboTable from '@/components/statistics/datatables/ComboTable.vue'

export default {
  name: 'DatatableStatistic',
  components: {
    ProductTable,
    CustomerTable,
    EmployeeTable,
    ComboTable,
  },
  props: {
    data: {
      type: DatatableStatisticsResponse,
      required: true,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      selectedTable: 'products', // Giá trị mặc định là sản phẩm
    }
  },
  computed: {
    tableTitle() {
      switch (this.selectedTable) {
        case 'customers':
          return 'Khách hàng hàng đầu'
        case 'employees':
          return 'Nhân viên hàng đầu'
        case 'combos':
          return 'Combo hàng đầu'
        default: // 'products'
          return 'Sản phẩm bán chạy nhất'
      }
    },
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
  mounted() {},
  methods: {},
}
</script>

<style scoped>
.table th,
.table td {
  vertical-align: middle;
}
</style>
