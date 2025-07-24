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
import pathReplaceImg from '@/utils/processPathImg'
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
    async initDataTable() {
      await this.$nextTick()
      const dataSet = this.combos.map((combo) => ({
        comboId: combo.comboId,
        comboName: combo.comboName,
        salesCount: combo.salesCount,
        revenue: combo.revenue, // Keep as a number
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
        ],
        language: configsDt.defaultLanguageDatatable, // Sử dụng ngôn ngữ từ configs
        initComplete: () => {
          configsDt.attachDetailsControl(`#comboDatatable`, this.formatDetails.bind(this))
        },
      })
      configsDt.attachSearchDebounce('#comboDatatable', table)
    },
    // ! Not certainly about this method. Damn
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const combo = this.combos.find((x) => x.comboId == rowData.comboId)

      const orderDetailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết combo: ${combo.comboName}</h6>
          <div class="row g-3">
            ${
              combo.detailTopCombos && combo.detailTopCombos.length > 0
                ? combo.detailTopCombos
                    .map(
                      (dCbo) => `
                        <div class="col-sm-12 col-md-6 col-lg-4">
                          <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                              <div class="d-flex align-items-center mb-3">
                                <img src="${pathReplaceImg(undefined, 'HinhAnh/Products', dCbo.hinhAnh)}" class="rounded me-3" style="width: 80px; height: 80px; object-fit: cover;" alt="Hình ảnh sản phẩm">
                                <div>
                                  <h5 class="card-title mb-0">Sản phẩm: ${dCbo.tenSanPham}</h5>
                                  <p class="card-subtitle text-muted">Mã sản phẩm: ${dCbo.comboId}</p>
                                </div>
                              </div>
                              <p class="mb-1"><strong>Số lượng:</strong> <span class="text-info">${dCbo.soLuong}</span></p>
                              <p class="mb-1"><strong>Đơn giá:</strong> <span class="text-danger">${formatCurrency(dCbo.donGia)}</span></p>
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
