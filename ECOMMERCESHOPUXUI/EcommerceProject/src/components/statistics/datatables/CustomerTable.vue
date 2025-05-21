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
          configsDt.defaultTdToShowDetail,
          { data: 'customerId', title: 'Mã khách hàng', className: 'text-center' },
          { data: 'customerName', title: 'Tên khách hàng' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
          { data: 'location', title: 'Địa điểm' },
          { data: 'ageGroup', title: 'Nhóm tuổi' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#customerDatatable`, this.formatDetails.bind(this))
        },
      })
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const customer = this.customers.find((x) => x.customerId == rowData.customerId)

      const orderDetailsHtml =
        customer.orderRecents && customer.orderRecents.length > 0
          ? customer.orderRecents
              .map(
                (order) => `
                  <div class="row">
                    <div class="col-6 col-md-12 order-item border rounded p-2 mb-2 bg-light position-relative">
                      <p><strong>Mã hóa đơn:</strong> ${order.maHd}</p>
                      <p><strong>Ngày tạo:</strong> ${new Date(order.ngayTao).toLocaleDateString()}</p>
                      <p><strong>Trạng thái:</strong> <span class="${order.isActive ? 'text-success' : 'text-danger'}">${order.tinhTrang}</span></p>
                    
                      <span class="tooltip-icon position-absolute top-0 end-0 m-1" data-toggle="tooltip" title="${order.diaChiNhanHang}">
                        <i class="fas fa-info-circle"></i>
                      </span>
                    </div>
                  </div>
                `,
              )
              .join('')
          : '<p>Không có đơn hàng nào để hiển thị.</p>'

      div.html(orderDetailsHtml)

      return div
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
