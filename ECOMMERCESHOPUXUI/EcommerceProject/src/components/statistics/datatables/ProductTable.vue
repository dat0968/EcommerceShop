<template>
  <div v-if="products.length" class="table-responsive">
    <table id="productDatatable" class="table table-hover"></table>
  </div>
  <p v-else>Không có sản phẩm nào để hiển thị.</p>
</template>

<script>
import * as configsDt from '@/utils/configsDatatable.js'
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import { formatCurrency } from '@/constants/formatCurrency'

export default {
  name: 'ProductTable',
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
    initDataTable() {
      const dataSet = this.products.map((product) => ({
        productId: product.productId,
        productName: product.productName,
        categoryName: product.categoryName,
        revenue: formatCurrency(product.revenue),
        count: product.count,
      }))

      $('#productDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'productId', title: 'Mã sản phẩm', className: 'text-center' },
          { data: 'productName', title: 'Tên sản phẩm' },
          { data: 'categoryName', title: 'Tên danh mục' },
          { data: 'revenue', title: 'Doanh thu', className: 'text-right' },
          { data: 'count', title: 'Số lượng bán', className: 'text-center' },
        ],
        language: configsDt.defaultLanguageDatatable,
        initComplete: () => {
          configsDt.attachDetailsControl(`#productDatatable`, this.formatDetails.bind(this))
        },
      })
    },
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      const detailProduct = this.products.find((x) => x.productId == rowData.productId)

      const detailsHtml = `
        <div class="container">
            <div class="row">
                <p><strong>${rowData.productName}</strong> - ${rowData.categoryName}</p>
            </div>
            <div class="row mb-3 gap-1 justify-content-between detail-list">
                ${
                  detailProduct.detailTopProducts && detailProduct.detailTopProducts.length > 0
                    ? detailProduct.detailTopProducts
                        .map(
                          (detail) => `
                                <div class="col-5 detail-item">
                                    <div class="row border p-2 bg-light">
                                        <div class="col-md-4 d-flex align-items-center">
                                            <img src="${detail.hinhAnh || '/images/default.png'}" class="img-fluid rounded" alt="Hình ảnh sản phẩm">
                                        </div>
                                        <div class="col-md-8">
                                            <div class="text-primary flex flex-flow-column justify-content-between"><span class="col-auto">Màu: ${detail.mauSac || '-'}</span> | <span class="col-auto">Size: ${detail.kichThuoc || '-'}</span></div>
                                            <p><strong>Giá:</strong> <span class="text-danger">${formatCurrency(detail.donGia || 0)}</span></p>
                                            <p><strong>Số lượng tồn:</strong> <span class="text-warning">${detail.soLuongTon}</span></p>
                                            <p><strong>Trạng thái:</strong> <span class="${detail.isActive ? 'text-success' : 'text-danger'}">${detail.isActive ? 'Đang bán' : 'Ngừng bán'}</span></p>
                                        </div>
                                    </div>
                                </div>
                            `,
                        )
                        .join('')
                    : '<p>Không có biến thể nào để hiển thị.</p>'
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

.detail-list {
  max-height: 200px; /* Đặt chiều cao tối đa cho danh sách biến thể */
  overflow-y: auto; /* Thêm scrollbar nếu cần */
}

.detail-item {
  margin-bottom: 10px; /* Khoảng cách giữa các biến thể */
  border: 1px solid #ddd; /* Thêm border cho mỗi biến thể */
  border-radius: 5px; /* Làm tròn các cạnh */
}

.bg-light {
  background-color: #f8f9fa !important; /* Đặt màu nền cho item */
}

.text-primary {
  color: #007bff !important; /* Màu chữ cho thông tin màu và kích thước */
}

.text-danger {
  color: #dc3545 !important; /* Màu chữ cho giá */
}

.text-warning {
  color: #ffc107 !important; /* Màu chữ cho số lượng tồn */
}

.text-success {
  color: #28a745 !important; /* Màu chữ cho trạng thái đang bán */
}
</style>
