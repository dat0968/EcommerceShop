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

      const detailsHtml = `
    <div class="container">
      <div class="row mb-3 gap-1 justify-content-between detail-list">
        ${
          customer.orderRecents && customer.orderRecents.length > 0
            ? customer.orderRecents
                .map(
                  (order) => `
                    <div class="col-sm-12 col-md-6 p-3 detail-item">
                      <div class="row border p-1 rounded bg-light">
                        <div class="col-4 d-flex align-items-center justify-content-center">
                          <img src="${order.avatar || '/images/default.png'}" class="img-fluid rounded" alt="Khách hàng">
                        </div>
                        <div class="col-8">
                          <div class="text-primary flex flex-flow-column justify-content-between">
                            <span class="col-auto">Mã hóa đơn: ${order.maHd}</span>
                          </div>
                          <p><strong>Tên khách hàng:</strong> ${order.hoTen}</p>
                          <p><strong>Ngày tạo:</strong> ${new Date(order.ngayTao).toLocaleDateString()}</p>
                          <p><strong>Trạng thái:</strong> <span class="${order.isActive ? 'text-success' : 'text-danger'}">${order.tinhTrang}</span></p>
                          <p>
                            <strong>Địa chỉ nhận:</strong>
                            <span title="${order.diaChiNhanHang}">
                              ${order.diaChiNhanHang}
                            </span>
                          </p>
                        </div>
                      </div>
                    </div>
                  `,
                )
                .join('')
            : '<p>Không có đơn hàng nào để hiển thị.</p>'
        }
      </div>
    </div>`
      div.html(detailsHtml)
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
