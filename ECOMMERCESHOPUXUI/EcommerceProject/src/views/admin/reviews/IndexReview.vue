<template>
  <div style="margin-top: 90px" class="xp-contentbar position-relative">
    <Overlay :is-visible="isDisabled" :overlayContent="overlayContent" />
    <!-- Breadcrumb trạng thái -->
     
    <h1 class="fw-bold text-uppercase text-center text-dark mb-4" style="font-size: 3rem">Quản lý đánh giá</h1>
    <!-- <nav aria-label="breadcrumb  text-center" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5"><strong>Quản lý đánh giá</strong></li>
      </ol>
      <hr />
    </nav> -->

    <div class="row">
      <div class="col-md-3">
        <div class="card shadow-sm mb-3">
          <div class="card-header bg-light fw-bold"><i class="bi bi-funnel"></i> Bộ lọc</div>
          <div class="card-body">
            <div class="mb-3">
              <label for="filterStar" class="form-label">Lọc theo số sao:</label>
              <select
                id="filterStar"
                class="form-select"
                v-model="filterByStar"
                @change="filterByRating"
              >
                <option :value="null">Tất cả sao</option>
                <option v-for="n in 5" :key="n" :value="n">{{ n }} sao</option>
              </select>
            </div>
            <div class="mb-3">
              <label for="filterImage" class="form-label">Lọc theo ảnh:</label>
              <select
                id="filterImage"
                class="form-select"
                v-model="filterHasImage"
                @change="filterByRating"
              >
                <option value="">Tất cả</option>
                <option value="1">Có ảnh</option>
                <option value="0">Không ảnh</option>
              </select>
            </div>
            <div class="mb-3">
              <OffensiveWords />
            </div>
            <button class="btn btn-outline-secondary w-100" @click="resetFilters">
              <i class="bi bi-x-circle"></i> Xóa lọc
            </button>
          </div>
        </div>
      </div>
      <div class="col-md-9">
        <div class="card">
          <div class="card-header text-center">
            <h5>Danh sách đánh giá</h5>
          </div>
          <div class="card-body">
            <button
              class="btn btn-primary mb-3"
              :disabled="isDisabled || isSubmittingShopResponse"
              @click="updateShopResponse"
            >
              <span
                v-if="isSubmittingShopResponse"
                class="spinner-border spinner-border-sm"
                role="status"
                aria-hidden="true"
              ></span>
              Cập nhật phản hồi của shop
            </button>
            <table
              v-if="filteredListReview.length > 0"
              id="datatableReviews"
              class="table table-bordered table-striped"
              style="width: 100%"
            ></table>
            <NoDataMessage v-else contentText="Không có đánh giá nào để hiển thị." />
          </div>
        </div>
      </div>
    </div>
  </div>
  <VueEasyLight
    :visible="isLightboxOpen"
    :imgs="lightboxImages"
    :index="lightboxIndex"
    @hide="closeLightbox"
  />
</template>

<script>
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import Swal from 'sweetalert2'

import * as axiosConfig from '@/utils/axiosClient'
import ConfigsRequest from '@/models/ConfigsRequest'
import * as configsDt from '@/utils/configsDatatable.js'
import Overlay from '@/components/common/Overlay.vue'
import { formatDate } from '@/constants/formatDatetime'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'
import VueEasyLight from 'vue-easy-lightbox'
import OffensiveWords from '@/components/pages/admin/reviews/OffensiveWords.vue'
// import { formatCurrency } from '@/constants/formatCurrency'

export default {
  name: 'IndexReview',
  components: { Overlay, VueEasyLight, OffensiveWords },
  props: {},
  data() {
    return {
      listReview: [],
      filteredListReview: [],
      isLoading: true,
      isDisabled: false,
      isEndpointActive: true, // Biến để kiểm tra kết nối API
      overlayContent: 'Đang tải dữ liệu đánh giá...',
      selectedReview: [],
      filterByStar: null,
      filterHasImage: '',
      pathReplaceImg,
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
      isSubmittingShopResponse: false,
    }
  },
  computed: {},
  watch: {},
  async mounted() {
    this.isLoading = true // Đặt trạng thái loading là true khi bắt đầu

    // Kiểm tra endpoint trước khi load dữ liệu
    this.isEndpointActive = await axiosConfig.isEndpointAvailable?.()
    if (!this.isEndpointActive) {
      Swal.fire({
        icon: 'error',
        title: 'Không có kết nối API',
        text: 'Không thể kết nối tới máy chủ API. Vui lòng kiểm tra lại kết nối hoặc cấu hình endpoint.',
        confirmButtonText: 'Đóng',
      })
      this.overlayContent = 'Không thể kết nối tới API để quản lý.'
      return
    }

    await this.getListReview() // Lấy dữ liệu đánh giá từ API
    if (this.listReview && this.listReview.length > 0) {
      this.filterByRating() // Khởi tạo DataTable nếu có dữ liệu
    } else {
      Swal.fire({
        icon: 'info',
        title: 'Không có dữ liệu',
        text: 'Hiện tại không có đánh giá nào để hiển thị.',
        confirmButtonText: 'Đóng',
      })
      this.isDisabled = true // Vô hiệu hóa khi không có dữ liệu
      this.overlayContent = 'Không có đánh giá nào để hiển thị.'
    }
    this.isLoading = false // Đặt trạng thái loading là false sau khi hoàn thành

    window.vueIndexReviewOpenLightbox = this.openLightbox
  },
  beforeUnmount() {
    if (window.vueIndexReviewOpenLightbox === this.openLightbox) {
      window.vueIndexReviewOpenLightbox = undefined
    }
  },
  methods: {
    // -- Hàm lấy dữ liệu đánh giá từ API
    async getListReview() {
      try {
        const response = await axiosConfig.getFromApi('/review/all', ConfigsRequest.takeAuth())
        this.listReview = response.data
        this.isLoading = false
      } catch (error) {
        console.error('Lỗi khi lấy danh sách đánh giá:', error)
        this.isDisabled = true
      }
    },
    // -- Lọc dữ liệu đánh giá theo số sao
    filterByRating() {
      this.filteredListReview = this.listReview.filter((item) => {
        // Lọc theo số sao
        if (
          this.filterByStar != null &&
          this.filterByStar != 'null' &&
          item.soSao != this.filterByStar
        )
          return false
        // Lọc theo có ảnh/không ảnh
        const hasImg = item.hinhAnhs && item.hinhAnhs.length > 0
        if (this.filterHasImage === '1' && !hasImg) return false
        if (this.filterHasImage === '0' && hasImg) return false
        return true
      })
      this.initDataTable()
    },
    // -- Hàm khởi tạo DataTable
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableReviews')) {
          $('#datatableReviews').DataTable().destroy()
        }
        this.datatable = $('#datatableReviews').DataTable({
          data: vm.filteredListReview,
          columns: [
            configsDt.defaultTdToShowDetail,
            {
              data: null,
              title: 'Đối tượng đánh giá',
              className: 'text-center',
              render: function (data) {
                const isProduct = data.maSp !== null
                const idObject = isProduct ? data.maSp : data.maCombo
                return isProduct
                  ? `<span class="badge bg-primary" title="${idObject}">Sản phẩm</span>`
                  : `<span class="badge bg-secondary" title="${idObject}">Combo</span>`
              },
            },
            { data: 'tenKhachHang', title: 'Tên khách hàng', className: 'text-center' },
            { data: 'soSao', title: 'Số sao', className: 'text-center' },
            {
              data: null,
              title: 'Số sản phẩm đã mua',
              className: 'text-center',
              render: function (data) {
                // Tính tổng số lượng hàng hóa đã mua từ các đơn hàng
                return data.orders.reduce((total, order) => {
                  return total + order.soLuong
                }, 0)
              },
            },
            {
              data: 'ngayDanhGia',
              title: 'Ngày đánh giá',
              className: 'text-center',
              render: function (date) {
                return formatDate(date)
              },
            },
            {
              data: 'id',
              title: `<input type="checkbox" id="selectAllReviews" class="form-check-input">`,
              className: 'text-center',
              orderable: false,
              render: function (data) {
                return `<input type="checkbox" class="form-check-input row-checkbox" value="${data}">`
              },
            },
          ],
          destroy: true,
          language: configsDt.defaultLanguageDatatable,
          initComplete: () => {
            configsDt.attachDetailsControl(`#datatableReviews`, this.formatDetails.bind(this))
            configsDt.attachSearchDebounce('#datatableReviews', this.datatable)

            $(document)
              .off('change', '#selectAllReviews')
              .on('change', '#selectAllReviews', function () {
                const checked = $(this).is(':checked')
                $('.row-checkbox').prop('checked', checked).trigger('change')
              })

            $(document)
              .off('change', '.row-checkbox')
              .on('change', '.row-checkbox', function () {
                const id = $(this).val()
                if ($(this).is(':checked')) {
                  if (!vm.selectedReview.includes(id)) vm.selectedReview.push(id)
                } else {
                  vm.selectedReview = vm.selectedReview.filter((item) => item !== id)
                }
                if (!$(this).is(':checked')) {
                  $('#selectAllReviews').prop('checked', false)
                }
                if ($('.row-checkbox:checked').length === $('.row-checkbox').length) {
                  $('#selectAllReviews').prop('checked', true)
                }
              })
            // Gắn sự kiện click cho ảnh review-lightbox-img để mở lightbox
            $(document)
              .off('click', '.review-lightbox-img')
              .on('click', '.review-lightbox-img', function () {
                const imgs = $(this).data('imgs')
                const idx = parseInt($(this).data('idx')) || 0
                // Chuyển đổi sang mảng path đầy đủ
                const fullImgs = imgs.map((img) =>
                  vm.pathReplaceImg(undefined, 'HinhAnh/Reviews', img),
                )
                vm.openLightbox(fullImgs, idx)
              })
          },
        })
      })
    },
    // -- Mở mục xem chi tiết nội dung đánh giá
    formatDetails(rowData) {
      const div = $('<div/>').addClass('loading').text('Loading...')
      // Tìm kiếm đánh giá dựa trên id của rowData
      const evaluation = this.filteredListReview.find((x) => x.id == rowData.id)

      // Kiểm tra sự tồn tại của đánh giá
      if (!evaluation) {
        div.html('<p>Không tìm thấy thông tin đánh giá.</p>')
        return div
      }

      // Thống kê số lượng theo trạng thái
      const statusCount = evaluation.orders.reduce((acc, order) => {
        acc[order.trangThai] = (acc[order.trangThai] || 0) + order.soLuong
        return acc
      }, {})

      const statusSummaryHtml = Object.entries(statusCount)
        .map(([status, count]) => `<p><strong>${status}:</strong> ${count}</p>`)
        .join('')

      const detailsHtml = `
      <div class="container-fluid">
          <div class="row p-3">
              <div class="col-lg-3 col-md-4 col-sm-6 col-12 p-1">
                  <strong>Tên sản phẩm:</strong> ${evaluation.tenSanPham}
              </div>
              <div class="col-lg-3 col-md-4 col-sm-6 col-12 p-1">
                  <strong>Mã khách hàng:</strong> ${evaluation.maKh}
              </div>
              <div class="col-lg-3 col-md-4 col-sm-6 col-12 p-1">
                  <strong>Email:</strong> ${evaluation.email}
              </div>
              <div class="col-lg-3 col-md-4 col-sm-6 col-12 p-1">
                  <strong>Số điện thoại:</strong> ${evaluation.soDienThoai}
              </div>
              <div class="col-12 mt-2">
                  <strong>Nội dung đánh giá:</strong>
                  <p>${evaluation.noiDung}</p>
              </div>
              ${
                evaluation.shopPhanHoi
                  ? `<blockquote class="col-12 border-start border-2 border-secondary ps-3 my-3">
                      <strong>Phản hồi của shop:</strong> 
                      ${evaluation.shopPhanHoi} <small class="text-muted">(${formatDate(evaluation.ngayPhanHoi)})</small>
                    </blockquote>`
                  : ''
              }
              <hr/>  
          </div>
          <div class="row mb-3">
              <div class="col-12">
                  <h6>Đối tượng đánh giá trong các đơn hàng:</h6>
                  ${statusSummaryHtml}
              </div>
          </div>
          <div class="row mb-3 detail-list">
            ${
              evaluation.hinhAnhs && evaluation.hinhAnhs.split(',').length > 0
                ? evaluation.hinhAnhs
                    .split(',')
                    .map(
                      (img, idx, arr) => `
                        <div class="col-auto d-flex mb-2">
                          <img 
                            src="${pathReplaceImg(undefined, 'HinhAnh/Reviews', img)}" 
                            class="img-fluid border border-light rounded-3 review-lightbox-img"
                            style="max-width: 7em; height: 7em; cursor: pointer"
                            alt="Hình ảnh đánh giá"
                            data-imgs='${JSON.stringify(arr)}'
                            data-idx='${idx}'
                          >
                        </div>
                      `,
                    )
                    .join('')
                : '<div class="col-12"><p>Không có hình ảnh đánh giá để hiển thị.</p></div>'
            }
          </div>
      </div>`

      return div.html(detailsHtml)
    },
    // -- Hàm cập nhập phản hồi của shop
    async updateShopResponse() {
      try {
        if (this.selectedReview.length === 0) {
          Swal.fire({
            icon: 'warning',
            title: 'Chưa chọn đánh giá',
            text: 'Vui lòng chọn ít nhất một đánh giá để cập nhật phản hồi của shop.',
            confirmButtonText: 'Đóng',
          })
          return
        }
        let contentResponse = ''
        Swal.fire({
          title: `Cập nhật phản hồi của shop với ` + this.selectedReview.length + ` đánh giá`,
          input: 'textarea',
          inputLabel: 'Nội dung phản hồi',
          inputPlaceholder: 'Nhập nội dung phản hồi của shop...',
          showCancelButton: true,
          confirmButtonText: 'Cập nhật',
          cancelButtonText: 'Hủy',
        }).then(async (result) => {
          if (result.isConfirmed) {
            contentResponse = result.value
            if (!contentResponse || contentResponse.trim() === '') {
              Swal.fire({
                icon: 'warning',
                title: 'Nội dung phản hồi không hợp lệ',
                text: 'Vui lòng nhập nội dung phản hồi.',
                confirmButtonText: 'Đóng',
              })
              return
            }
            await this.submitShopResponse(contentResponse)
          }
        })
      } catch (error) {
        console.error('Lỗi khi cập nhật phản hồi của shop:', error)
        Swal.fire({
          icon: 'error',
          title: 'Cập nhật thất bại',
          text: 'Đã xảy ra lỗi khi cập nhật phản hồi của shop. Vui lòng thử lại sau.',
          confirmButtonText: 'Đóng',
        })
      }
    },
    // -- Hàm gửi phản hồi của shop
    async submitShopResponse(contentResponse) {
      try {
        const body = {
          listId: this.selectedReview,
          responseContent: contentResponse,
        }
        const res = await axiosConfig.putToApi(
          `/review/shop-response`,
          body,
          ConfigsRequest.takeAuth(),
        )
        // ! Fix lại chỗ này
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
          return
        }
        Swal.fire({
          icon: 'success',
          title: 'Cập nhật thành công',
          text: 'Phản hồi của shop đã được cập nhật.',
          confirmButtonText: 'Đóng',
        })
        await this.getListReview() // Cập nhật lại danh sách đánh giá
        this.filterByRating() // Cập nhật DataTable sau khi phản hồi
      } catch (error) {
        console.error('Lỗi khi cập nhật phản hồi của shop:', error)
        Swal.fire({
          icon: 'error',
          title: 'Cập nhật thất bại',
          text: 'Đã xảy ra lỗi khi cập nhật phản hồi của shop. Vui lòng thử lại sau.',
          confirmButtonText: 'Đóng',
        })
      }
    },
    // - Xử lí về LightBox hình ảnh
    openLightbox(imgs, idx = 0) {
      this.lightboxImages = imgs
      this.lightboxIndex = idx
      this.isLightboxOpen = true
    },
    closeLightbox() {
      this.isLightboxOpen = false
    },
    getReviewImages(item) {
      // Trả về mảng tên file ảnh (không path)
      if (!item.hinhAnhs) return []
      return Array.isArray(item.hinhAnhs) ? item.hinhAnhs : item.hinhAnhs.split(',')
    },
    getReviewImagesFullPath(item) {
      // Trả về mảng path đầy đủ cho lightbox
      return this.getReviewImages(item).map((img) =>
        this.pathReplaceImg(undefined, 'HinhAnh/Reviews', img),
      )
    },
    resetFilters() {
      this.filterByStar = null
      this.filterHasImage = ''
      this.filterByRating()
    },
  },
}
</script>

<style scoped></style>
