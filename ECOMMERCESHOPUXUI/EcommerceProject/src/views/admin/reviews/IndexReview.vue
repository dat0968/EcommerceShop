<template>
  <div style="margin-top: 90px" class="xp-contentbar position-relative">
    <Overlay :is-visible="isDisabled" :overlayContent="overlayContent" isCoverPage="true" />
    <!-- Breadcrumb trạng thái -->
    <nav aria-label="breadcrumb" class="mb-3">
      <ol class="breadcrumb">
        <li class="breadcrumb-item active h5">Quản lý danh mục</li>
      </ol>
    </nav>

    <div class="col-12">
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

export default {
  name: 'IndexReview',
  components: { Overlay },
  props: {},
  data() {
    return {
      listReview: [],
      isLoading: true,
      isDisabled: false,
      isEndpointActive: true, // Biến để kiểm tra kết nối API
      overlayContent: 'Đang tải dữ liệu đánh giá...',
      selectedReview: [],
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
      this.initDataTable() // Khởi tạo DataTable nếu có dữ liệu
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
    // -- Hàm khởi tạo DataTable
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#datatableReviews')) {
          $('#datatableReviews').DataTable().destroy()
        }
        this.datatable = $('#datatableReviews').DataTable({
          data: vm.listReview,
          columns: [
            configsDt.defaultTdToShowDetail,
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
          ],
          destroy: true,
          language: configsDt.defaultLanguageDatatable,
          initComplete: () => {
            configsDt.attachDetailsControl(`#datatableReviews`, this.formatDetails.bind(this))
          },
        })
      })
    },
    // -- Hàm cập nhập phản hồi của shop
    async updateShopResponse(contentResponse) {
      try {
        const body = {
          id: this.selectedReview,
          shopPhanHoi: contentResponse,
        }
        await axiosConfig.putToApi('/review/shop-response', body)
        Swal.fire({
          icon: 'success',
          title: 'Cập nhật thành công',
          text: 'Phản hồi của shop đã được cập nhật.',
          confirmButtonText: 'Đóng',
        })
        this.getListReview() // Cập nhật lại danh sách đánh giá
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
