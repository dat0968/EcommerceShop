<template>
  <div style="margin-top: 90px" class="xp-contentbar position-relative">
    <Overlay :is-visible="isDisabled" :overlayContent="overlayContent" />
    <!-- Breadcrumb trạng thái -->
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5">Quản lý danh mục</li>
      </ol>
    </nav>

    <div class="col-12">
      <div class="row justify-content-between align-items-center mb-3">
        <div class="col-md-3">
          <label class="form-label">Lọc theo đánh giá</label>
          <select class="form-select" v-model="filterByStar" @change="filterByRating">
            <option value="">Tất cả</option>
            <option v-for="n in 5" :key="n" :value="n">{{ n }} sao</option>
          </select>
        </div>
        <div class="col-md-3">
          <button class="btn btn-primary" :disabled="isDisabled" @click="updateShopResponse">
            Cập nhật phản hồi của shop
          </button>
        </div>
      </div>
      <table
        id="datatableReviews"
        class="table table-bordered table-striped"
        style="width: 100%"
      ></table>
    </div>
  </div>
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

export default {
  name: 'IndexReview',
  components: { Overlay },
  props: {},
  data() {
    return {
      listReview: [],
      filtedListReview: [],
      isLoading: true,
      isDisabled: false,
      isEndpointActive: true, // Biến để kiểm tra kết nối API
      overlayContent: 'Đang tải dữ liệu đánh giá...',
      selectedReview: [],
      filterByStar: null,
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
  },
  methods: {
    // -- Hàm lấy dữ liệu đánh giá từ API
    async getListReview() {
      try {
        const response = await axiosConfig.getFromApi(
          '/review/all',
          ConfigsRequest.getSkipAuthConfig(),
        )
        this.listReview = response.data
        this.isLoading = false
      } catch (error) {
        console.error('Lỗi khi lấy danh sách đánh giá:', error)
        this.isDisabled = true
      }
    },
    // -- Lọc dữ liệu đánh giá theo số sao
    filterByRating() {
      this.filtedListReview = this.listReview.filter((item) => {
        if (this.filterByStar === null) return true // Không lọc nếu không có lựa chọn
        return item.soSao == this.filterByStar
      })
      if (this.filtedListReview.length === 0) {
        Swal.fire({
          icon: 'info',
          title: 'Không có đánh giá phù hợp',
          text: `Không có đánh giá nào với ${this.filterByStar} sao.`,
          confirmButtonText: 'Đóng',
        })
      }
      this.initDataTable() // Cập nhật DataTable sau khi lọc
    },
    // -- Hàm khởi tạo DataTable
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableReviews')) {
          $('#datatableReviews').DataTable().destroy()
        }
        this.datatable = $('#datatableReviews').DataTable({
          data: vm.filtedListReview,
          columns: [
            // configsDt.defaultTdToShowDetail,
            {
              data: null, // Dữ liệu sẽ được xác định trong render
              title: 'Đối tượng đánh giá',
              className: 'text-center',
              render: function (data) {
                const isProduct = data.maSp != null
                const idObject = isProduct ? data.maSp : data.maCombo
                if (isProduct) {
                  return `<span class="badge bg-primary" title=${idObject}>Sản phẩm</span>`
                } else {
                  return `<span class="badge bg-secondary" title=${idObject}>Combo</span>`
                }
              },
            },
            { data: 'tenKhachHang', title: 'Tên khách hàng', className: 'text-center' },
            { data: 'soSao', title: 'Số sao', className: 'text-center' },
            {
              data: 'ngayDanhGia',
              title: 'Ngày đánh giá',
              className: 'text-center',
              render: function (data) {
                return formatDate(data)
              },
            },
            {
              data: 'shopPhanHoi',
              title: 'Phản hồi của shop',
              className: 'text-center',
            },
            {
              data: 'ngayPhanHoi',
              title: 'Ngày phản hồi',
              className: 'text-center',
              render: function (data) {
                return formatDate(data)
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
            // configsDt.attachDetailsControl(`#datatableReviews`, this.formatDetails.bind(this))

            // Sự kiện chọn tất cả
            $(document)
              .off('change', '#selectAllReviews')
              .on('change', '#selectAllReviews', function () {
                const checked = $(this).is(':checked')
                $('.row-checkbox').prop('checked', checked).trigger('change')
              })

            // Sự kiện từng dòng (giữ nguyên hoặc cập nhật lại nếu cần)
            $(document)
              .off('change', '.row-checkbox')
              .on('change', '.row-checkbox', function () {
                const id = $(this).val()
                if ($(this).is(':checked')) {
                  if (!vm.selectedReview.includes(id)) vm.selectedReview.push(id)
                } else {
                  vm.selectedReview = vm.selectedReview.filter((item) => item !== id)
                }
                // Nếu bỏ chọn bất kỳ dòng nào thì bỏ chọn checkbox tổng
                if (!$(this).is(':checked')) {
                  $('#selectAllReviews').prop('checked', false)
                }
                // Nếu tất cả đều được chọn thì chọn checkbox tổng
                if ($('.row-checkbox:checked').length === $('.row-checkbox').length) {
                  $('#selectAllReviews').prop('checked', true)
                }
              })
          },
        })
      })
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
          title: 'Cập nhật phản hồi của shop',
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
          ConfigsRequest.getSkipAuthConfig(),
        ) // ! Fix lại chỗ này
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
  },
}
</script>

<style scoped></style>
