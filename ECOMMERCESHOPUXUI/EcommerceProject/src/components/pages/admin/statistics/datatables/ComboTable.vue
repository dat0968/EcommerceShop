<template>
  <div v-if="combos.length" class="table-responsive">
    <table id="comboDatatable" class="table table-hover"></table>
  </div>
  <NoDataMessage v-else />
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'
import NoDataMessage from '@/components/common/NoDataMessage.vue'

export default {
  name: 'ComboTable',
  components: { NoDataMessage },
  props: {
    combos: {
      type: Array,
      required: true,
    },
  },
  mounted() {
    this.initDataTable()
  },
  methods: {
    initDataTable() {
      const dataSet = this.combos.map((combo) => ({
        comboId: combo.comboId,
        comboName: combo.comboName,
        salesCount: combo.salesCount,
        revenue: formatCurrency(combo.revenue), // Định dạng doanh thu
      }))

      // Khởi tạo DataTable
      const table = $('#comboDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'comboId', title: 'Mã combo', className: 'text-center' },
          { data: 'comboName', title: 'Tên combo' },
          {
            data: null,
            title: 'Đánh giá',
            render: function (data, type, row) {
              const totalReviewStar = row.starCount ?? 0
              return `
              <span>
                ${Array.from(
                  { length: totalReviewStar },
                  () => `<span style="color: #ffc107">★</span>`,
                ).join('')}
                ${Array.from(
                  { length: 5 - totalReviewStar },
                  () => `<span style="color: #e4e5e9">★</span>`,
                ).join('')}
              </span>
              `
            },
          },
          { data: 'salesCount', title: 'Số lượng bán', className: 'text-center' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#comboDatatable`, this.formatDetails.bind(this))
          configsDt.attachSearchDebounce('#comboDatatable', table)
        },
      })
    },
    // ! Not certainly about this method. Damn
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const combo = this.combos.find((x) => x.comboId == rowData.comboId)

      const orderDetailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết đơn hàng gần đây của Combo: ${combo.comboName}</h6>
          <div class="row g-3">
            ${
              combo.orderRecents && combo.orderRecents.length > 0
                ? combo.orderRecents
                    .map(
                      (order) => `
                        <div class="col-sm-12 col-md-6 col-lg-4">
                          <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                              <h5 class="card-title mb-2">Mã hóa đơn: ${order.maHd}</h5>
                              <p class="mb-1"><strong>Ngày tạo:</strong> ${new Date(order.ngayTao).toLocaleDateString()}</p>
                              <p class="mb-1"><strong>Trạng thái:</strong> <span class="badge ${order.isActive ? 'bg-success' : 'bg-danger'}">${order.tinhTrang}</span></p>
                              <p class="mb-0"><strong>Địa chỉ nhận:</strong> <span title="${order.diaChiNhanHang}">${order.diaChiNhanHang}</span></p>
                            </div>
                          </div>
                        </div>
                      `,
                    )
                    .join('')
                : '<div class="col-12"><p class="text-center text-muted">Không có chi tiết sản phẩm trong combo này để hiển thị.</p></div>'
            }
          </div>
        </div>`

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
