<template>
  <div v-if="customers.length" class="table-responsive">
    <table id="customerDatatable" class="table table-hover"></table>
  </div>
  <p v-else>Không có khách hàng nào để hiển thị.</p>
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'

export default {
  name: 'CustomerTable',
  props: {
    customers: {
      type: Array,
      required: true,
    },
  },
  mounted() {
    this.initDataTable()
  },
  methods: {
    initDataTable() {
      const dataSet = this.customers.map((customer) => ({
        customerId: customer.customerId,
        customerName: customer.customerName,
        revenue: formatCurrency(customer.revenue), // Định dạng doanh thu
        location: customer.location,
        ageGroup: customer.ageGroup,
      }))

      // Khởi tạo DataTable
      $('#customerDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          { data: 'customerId', title: 'Mã khách hàng', className: 'text-center' },
          { data: 'customerName', title: 'Tên khách hàng' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
          { data: 'location', title: 'Địa điểm' },
          { data: 'ageGroup', title: 'Nhóm tuổi' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
      })
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
