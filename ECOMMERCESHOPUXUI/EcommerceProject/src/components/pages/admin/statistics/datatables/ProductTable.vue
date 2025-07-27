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
      const productMap = new Map(this.products.map((p) => [p.productId, p]))
      const dataSet = this.products.map((product) => ({
        productId: product.productId,
        productName: product.productName,
        categoryName: product.categoryName,
        revenue: product.revenue, // Keep as a number
        count: product.count,
        averageRating: product.averageRating ?? 0, // Assuming product has averageRating
      }));

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
              const averageRating = row.averageRating ?? 0;
              const fullStars = Math.floor(averageRating);
              const emptyStars = 5 - Math.ceil(averageRating);
              let starsHtml = '';

              for (let i = 0; i < fullStars; i++) {
                starsHtml += '<span style="color: #ffc107">★</span>';
              }
              if (averageRating % 1 !== 0) { // Check for fractional part
                starsHtml += '<span style="color: #ffc107; position: relative;">★<span style="position: absolute; width: ' + (averageRating % 1) * 100 + '%; overflow: hidden; display: inline-block;">★</span></span>'; // Partial star
              }
              for (let i = 0; i < emptyStars; i++) {
                starsHtml += '<span style="color: #e4e5e9">★</span>';
              }
              return `<span>${starsHtml}</span>`;
            },
          },
          { data: 'categoryName', title: 'Tên danh mục' },
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
          { data: 'count', title: 'Số lượng bán', className: 'text-center' },
        ],
        language: configsDt.defaultLanguageDatatable,
        initComplete: () => {
          configsDt.attachDetailsControl(
            `#productDatatable`,
            this.formatDetails.bind(this, productMap),
          )
        },
      })
      configsDt.attachSearchDebounce('#productDatatable', table)
    },
    formatDetails(productMap, rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const detailProduct = productMap.get(rowData.productId)

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
