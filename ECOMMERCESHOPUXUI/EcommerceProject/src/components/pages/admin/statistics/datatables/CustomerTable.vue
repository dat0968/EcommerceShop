<template>
  <div v-if="customers.length" class="table-responsive">
    <table id="customerDatatable" class="table table-hover"></table>
  </div>
  <NoDataMessage v-else />
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'
import pathReplaceImg from '@/utils/processPathImg'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

export default {
  name: 'CustomerTable',
  components: {
    NoDataMessage,
  },
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
    async initDataTable() {
      await this.$nextTick()
      const dataSet = this.customers.map((customer) => ({
        customerId: customer.customerId,
        customerName: customer.customerName,
        revenue: customer.revenue, // Keep as a number
        location: customer.location,
        ageGroup: customer.ageGroup,
      }))

      // Khởi tạo DataTable
      const table = $('#customerDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'customerId', title: 'Mã khách hàng', className: 'text-center' },
          { data: 'customerName', title: 'Tên khách hàng' },
          {
            data: 'revenue',
            title: 'Doanh thu',
            className: 'text-right',
            render: function (data, type, row) {
              if (type === 'display') {
                return formatCurrency(data)
              }
              return data
            },
          },
          {
            data: 'location',
            title: 'Địa điểm',
            render: function (data, type, row) {
              if (type === 'sort') {
                const match = data.match(/^(\d+)/)
                return match ? parseInt(match[1], 10) : 0
              }
              return data
            },
          },
          { data: 'ageGroup', title: 'Nhóm tuổi' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#customerDatatable`, this.formatDetails.bind(this))
        },
      })
      configsDt.attachSearchDebounce('#customerDatatable', table)
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const customer = this.customers.find((x) => x.customerId == rowData.customerId)

      const detailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết đơn hàng gần đây của ${customer.customerName}</h6>
          <div class="row g-3">
            ${
              customer.orderRecents && customer.orderRecents.length > 0
                ? customer.orderRecents
                    .map(
                      (order) => `
                        <div class="col-sm-12 col-md-6 col-lg-4">
                          <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                              <div class="d-flex align-items-center mb-3">
                                <img src="${pathReplaceImg(undefined, 'HinhAnh/Avatar', order.avatar)}" class="rounded-circle me-3" style="width: 60px; height: 60px; object-fit: cover;" alt="Khách hàng">
                                <div>
                                  <h5 class="card-title mb-0">Mã hóa đơn: ${order.maHd}</h5>
                                  <p class="card-subtitle text-muted">${order.hoTen}</p>
                                </div>
                              </div>
                              <p class="mb-1"><strong>Ngày tạo:</strong> ${order.ngayTao ? new Date(order.ngayTao).toLocaleDateString() : '-'}</p>
                              <p class="mb-1"><strong>Trạng thái:</strong> <span class="badge ${order.isActive ? 'bg-success' : 'bg-danger'}">${order.tinhTrang}</span></p>
                              <p class="mb-0"><strong>Địa chỉ nhận:</strong> <span title="${order.diaChiNhanHang}">${order.diaChiNhanHang}</span></p>
                            </div>
                          </div>
                        </div>
                      `,
                    )
                    .join('')
                : '<div class="col-12"><p class="text-center text-muted">Không có đơn hàng nào để hiển thị.</p></div>'
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
