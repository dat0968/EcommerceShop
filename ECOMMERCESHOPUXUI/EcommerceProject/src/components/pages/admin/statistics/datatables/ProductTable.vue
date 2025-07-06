<template>
  <div v-if="products.length" class="table-responsive">
    <table id="productDatatable" class="table table-hover"></table>
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
  name: 'ProductTable',
  components: {
    NoDataMessage,
  },
  props: {
    products: {
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
      const dataSet = this.products.map((product) => ({
        productId: product.productId,
        productName: product.productName,
        categoryName: product.categoryName,
        revenue: formatCurrency(product.revenue),
        count: product.count,
        detailTopProducts: product.detailTopProducts,
      }))

      const table = $('#productDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'productId', title: 'Mã sản phẩm', className: 'text-center' },
          { data: 'productName', title: 'Tên sản phẩm' },
          {
            data: null,
            title: 'Đánh giá',
            render: function (data, type, row) {
              const totalReviewStar =
                row.detailTopProducts && Array.isArray(row.detailTopProducts)
                  ? row.detailTopProducts.reduce((total, x) => total + x.soSao, 0)
                  : 0
              return `
              <span class="star-rating">
                ${Array.from(
                  { length: totalReviewStar },
                  () => `<span class="star filled">★</span>`,
                ).join('')}
                ${Array.from(
                  { length: 5 - totalReviewStar },
                  () => `<span class="star">★</span>`,
                ).join('')}
              </span>
              `
            },
          },
          { data: 'categoryName', title: 'Tên danh mục' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
          { data: 'count', title: 'Số lượng bán', className: 'text-center' },
        ],
        language: configsDt.defaultLanguageDatatable,
        initComplete: () => {
          configsDt.attachDetailsControl(`#productDatatable`, this.formatDetails.bind(this))
        },
      })
      configsDt.attachSearchDebounce('#productDatatable', table)
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const detailProduct = this.products.find((x) => x.productId == rowData.productId)

      const detailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết sản phẩm: ${detailProduct.productName}</h6>
          <div class="row g-3">
            ${
              detailProduct.detailTopProducts && detailProduct.detailTopProducts.length > 0
                ? detailProduct.detailTopProducts
                    .map(
                      (detail) => `
                        <div class="col-sm-12 col-md-6 col-lg-4">
                          <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                              <div class="d-flex align-items-center mb-3">
                                <img src="${pathReplaceImg(undefined, 'HinhAnh/Products', detail.hinhAnh)}" class="rounded me-3" style="width: 80px; height: 80px; object-fit: cover;" alt="Hình ảnh sản phẩm">
                                <div>
                                  <h5 class="card-title mb-0">Màu: ${detail.mauSac || '-'}</h5>
                                  <p class="card-subtitle text-muted">Size: ${detail.kichThuoc || '-'}</p>
                                </div>
                              </div>
                              <p class="mb-1"><strong>Giá:</strong> <span class="text-danger">${formatCurrency(detail.donGia || 0)}</span></p>
                              <p class="mb-1"><strong>Số lượng tồn:</strong> <span class="text-warning">${detail.soLuongTon}</span></p>
                              <p class="mb-0"><strong>Trạng thái:</strong> <span class="badge ${detail.isActive ? 'bg-success' : 'bg-danger'}">${detail.isActive ? 'Đang bán' : 'Ngừng bán'}</span></p>
                            </div>
                          </div>
                        </div>
                      `,
                    )
                    .join('')
                : '<div class="col-12"><p class="text-center text-muted">Không có biến thể nào để hiển thị.</p></div>'
            }
          </div>
        </div>`
      div.html(detailsHtml)
      return div
    },
  },
}
</script>

<style scoped></style>
