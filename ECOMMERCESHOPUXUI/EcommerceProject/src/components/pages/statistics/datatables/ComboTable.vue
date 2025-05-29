<template>
  <div v-if="combos.length" class="table-responsive">
    <table id="comboDatatable" class="table table-hover"></table>
  </div>
  <p v-else>Không có combo nào để hiển thị.</p>
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'

export default {
  name: 'ComboTable',
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
      $('#comboDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'comboId', title: 'Mã combo', className: 'text-center' },
          { data: 'comboName', title: 'Tên combo' },
          { data: 'salesCount', title: 'Số lượng bán', className: 'text-center' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#comboDatatable`, this.formatDetails.bind(this))
        },
      })
    },
    // ! Not certainly about this method
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const combo = this.combos.find((x) => x.comboId == rowData.comboId)

      const orderDetailsHtml =
        combo.orderRecents && combo.orderRecents.length > 0
          ? combo.orderRecents
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
          : '<p>Không có chi tiết sản phẩm trong combo này để hiển thị.</p>'

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
