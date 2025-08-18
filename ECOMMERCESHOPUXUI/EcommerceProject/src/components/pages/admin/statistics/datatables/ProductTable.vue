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
import StarRating from '@/components/common/StarRating.vue';
import { createApp } from 'vue';

export default {
  name: 'ProductTable',
  components: {
    NoDataMessage,
    StarRating
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
    formatCurrency, // Make it available in the template if needed
    async initDataTable() {
      await this.$nextTick()
      const productMap = new Map(this.products.map((p) => [p.productId, p]))
      const dataSet = this.products.map((product) => ({
        productId: product.productId,
        productName: product.productName,
        categoryName: product.categoryName,
        revenue: product.revenue,
        count: product.count,
        averageRating: product.averageRating, // Use the direct value from the API
      }));

      const table = $('#productDatatable').DataTable({
        data: dataSet,
        destroy: true,
        columns: [
          configsDt.defaultTdToShowDetail,
          { data: 'productId', title: 'Mã sản phẩm', className: 'text-center' },
          { data: 'productName', title: 'Tên sản phẩm' },
          {
            data: 'averageRating',
            title: 'Đánh giá TB',
            className: 'text-center',
            render: (data, type, row) => {
              if (type === 'display') {
                const container = document.createElement('div');
                const app = createApp(StarRating, {
                  rating: data, // data is now the correct averageRating
                  readOnly: true,
                  showRating: true,
                  starSize: 20
                });
                app.mount(container);
                return container.outerHTML;
              }
              return data;
            }
          },
          { data: 'categoryName', title: 'Tên danh mục' },
          {
            data: 'revenue',
            title: 'Doanh thu',
            className: 'text-right',
            render: (data, type, row) => {
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

      let detailsHtml = `
        <div class="container-fluid p-3">
          <h6 class="mb-3 text-primary">Chi tiết các biến thể: ${detailProduct.productName}</h6>`;

      if (detailProduct.detailTopProducts && detailProduct.detailTopProducts.length > 0) {
        detailsHtml += '<table class="table table-bordered table-sm">' +
          '<thead class="table-light"><tr><th>Mã CTSP</th><th>Màu sắc</th><th>Kích thước</th><th>Đơn giá</th><th>Đánh giá</th></tr></thead>' +
          '<tbody>';

        detailProduct.detailTopProducts.forEach(detail => {
          const ratingPlaceholderId = `rating-variant-${detail.maCtsp}`;
          detailsHtml += `<tr>
                          <td>${detail.maCtsp}</td>
                          <td>${detail.mauSac || '-'}</td>
                          <td>${detail.kichThuoc || '-'}</td>
                          <td>${this.formatCurrency(detail.donGia || 0)}</td>
                          <td><div id="${ratingPlaceholderId}"></div></td>
                        </tr>`;
        });

        detailsHtml += '</tbody></table>';
      } else {
        detailsHtml += '<p class="text-center text-muted">Không có biến thể nào để hiển thị.</p>';
      }

      detailsHtml += '</div>';
      div.html(detailsHtml);

      // Mount Vue components after HTML is in the DOM
      this.$nextTick(() => {
        if (detailProduct.detailTopProducts && detailProduct.detailTopProducts.length > 0) {
          detailProduct.detailTopProducts.forEach(detail => {
            const ratingPlaceholderId = `rating-variant-${detail.maCtsp}`;
            const container = div.find(`#${ratingPlaceholderId}`).get(0);
            if (container) {
              const app = createApp(StarRating, {
                rating: detail.soSao, // Use the corrected average rating for the variant
                readOnly: true,
                showRating: true,
                starSize: 16
              });
              app.mount(container);
            }
          });
        }
      });

      return div;
    },
  },
}
</script>

<style scoped></style>
