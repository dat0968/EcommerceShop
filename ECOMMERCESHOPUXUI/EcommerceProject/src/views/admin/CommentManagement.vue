<template>
  <div class="container-fluid mt-5">
    <nav
      class="mb-3"
      style="display: flex; justify-content: center; padding: 15px 0; background-color: transparent"
    >
      <ol class="breadcrumb" style="display: flex; justify-content: center; margin: 0">
        <li class="breadcrumb-item active" style="display: flex; align-items: center">
          <h1 style="text-align: center; margin: 0; font-size: 3rem; font-weight: 700; color: #333">
            QUẢN LÝ BÌNH LUẬN
          </h1>
        </li>
      </ol>
    </nav>

    <div v-if="isLoading" class="text-center my-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else class="row">
      <!-- Filter Column -->
      <div class="col-md-3">
        <div class="card shadow-sm mb-3">
          <div class="card-header bg-light fw-bold"><i class="bi bi-funnel"></i> Bộ lọc</div>
          <div class="card-body">
            <label class="form-label fw-bold">Lọc theo trạng thái</label>
            <div class="form-check">
              <input
                class="form-check-input"
                type="radio"
                name="statusFilter"
                id="filterPending"
                value="pending"
                v-model="filterStatus"
              />
              <label class="form-check-label" for="filterPending">Chờ duyệt</label>
            </div>
            <div class="form-check">
              <input
                class="form-check-input"
                type="radio"
                name="statusFilter"
                id="filterApproved"
                value="approved"
                v-model="filterStatus"
              />
              <label class="form-check-label" for="filterApproved">Đã duyệt</label>
            </div>
            <div class="form-check">
              <input
                class="form-check-input"
                type="radio"
                name="statusFilter"
                id="filterRejected"
                value="rejected"
                v-model="filterStatus"
              />
              <label class="form-check-label" for="filterRejected">Đã từ chối</label>
            </div>
            <div class="form-check">
              <input
                class="form-check-input"
                type="radio"
                name="statusFilter"
                id="filterAll"
                value="all"
                v-model="filterStatus"
              />
              <label class="form-check-label" for="filterAll">Tất cả</label>
            </div>
          </div>
        </div>
      </div>

      <!-- Data Table Column -->
      <div class="col-md-9">
        <div class="card">
          <div class="card-header text-center">
            <h5>Danh sách bình luận</h5>
          </div>
          <div class="card-body">
            <table
              id="comments-datatable"
              class="table table-bordered table-striped"
              style="width: 100%"
            ></table>
          </div>
        </div>
      </div>
    </div>

    <!-- Rejection Reason Modal -->
    <div
      v-if="showReasonModal"
      class="modal fade show"
      style="display: block; background-color: rgba(0, 0, 0, 0.5)"
      tabindex="-1"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Lý do từ chối</h5>
            <button type="button" class="btn-close" @click="showReasonModal = false"></button>
          </div>
          <div class="modal-body">
            <p v-if="selectedComment">
              Bình luận của: <strong>{{ selectedComment.hoTen }}</strong>
            </p>
            <textarea
              v-model="rejectionReason"
              class="form-control"
              rows="3"
              placeholder="Nhập lý do..."
            ></textarea>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showReasonModal = false">
              Hủy
            </button>
            <button type="button" class="btn btn-primary" @click="rejectComment">Xác nhận</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import $ from 'jquery'
import 'datatables.net'
import 'datatables.net-dt/css/dataTables.dataTables.css'
import Swal from 'sweetalert2'
import { getFromApi, putToApi } from '@/utils/axiosClient'
import * as configsDt from '@/utils/configsDatatable.js'

export default {
  name: 'CommentManagement',
  data() {
    return {
      allComments: [],
      isLoading: true,
      filterStatus: 'all', // Default filter
      datatable: null,
      showReasonModal: false,
      rejectionReason: '',
      selectedComment: null,
    }
  },
  computed: {
    filteredComments() {
      if (this.filterStatus === 'all') {
        return this.allComments
      }
      if (this.filterStatus === 'pending') {
        return this.allComments.filter((c) => !c.trangThai && c.lyDoHuy === null)
      }
      if (this.filterStatus === 'approved') {
        return this.allComments.filter((c) => c.trangThai)
      }
      if (this.filterStatus === 'rejected') {
        return this.allComments.filter((c) => !c.trangThai && c.lyDoHuy !== null)
      }
      return []
    },
  },
  async mounted() {
    await this.fetchComments()
  },
  methods: {
    async fetchComments() {
      this.isLoading = true
      try {
        const response = await getFromApi('/staff-comments')
        if (response.success) {
          this.allComments = response.data.sort(
            (a, b) => new Date(b.ngayBinhLuan) - new Date(a.ngayBinhLuan)
          )
          this.reloadDataTable()
        } else {
          Swal.fire('Lỗi', 'Không thể tải danh sách bình luận.', 'error')
        }
      } catch (error) {
        console.error('Failed to fetch comments:', error)
        Swal.fire('Lỗi', 'Có lỗi xảy ra khi kết nối tới máy chủ.', 'error')
      } finally {
        this.isLoading = false
      }
    },
    initDataTable() {
      const vm = this
      this.$nextTick(() => {
        if ($.fn.DataTable.isDataTable('#comments-datatable')) {
          $('#comments-datatable').DataTable().destroy()
        }
        this.datatable = $('#comments-datatable').DataTable({
          data: vm.filteredComments,
          columns: [
            { data: 'hoTen', title: 'Người bình luận' },
            {
              data: null,
              title: 'Sản phẩm/Combo',
              render: (data, type, row) => {
                if (row.tenSanPham) {
                  return `<span class="badge bg-info">${row.tenSanPham}</span>`
                } else if (row.tenCombo) {
                  return `<span class="badge bg-primary">${row.tenCombo}</span>`
                }
                return ''
              },
            },
            { data: 'noiDung', title: 'Nội dung', width: '35%' },
            {
              data: 'ngayBinhLuan',
              title: 'Ngày bình luận',
              render: (data) => new Date(data).toLocaleString(),
            },
            {
              data: 'trangThai',
              title: 'Trạng thái',
              render: (data, type, row) => {
                if (data) {
                  return '<span class="badge bg-success">Đã duyệt</span>'
                } else {
                  return row.lyDoHuy
                    ? `<span class="badge bg-danger" title="Lý do: ${row.lyDoHuy}">Đã từ chối</span>`
                    : '<span class="badge bg-warning text-dark">Chờ duyệt</span>'
                }
              },
            },
            {
              data: null,
              title: 'Hành động',
              orderable: false,
              className: 'text-center',
              render: (data, type, row) => {
                if (!row.trangThai && !row.lyDoHuy) {
                  // Only show for pending comments
                  return `<button class="btn btn-success btn-sm me-2 btn-approve" data-id="${row.id}" title="Phê duyệt bình luận"><i class="bi bi-check-circle"></i></button>
                           <button class="btn btn-danger btn-sm btn-reject" data-id="${row.id}" title="Từ chối bình luận"><i class="bi bi-x-circle"></i></button>`
                }
                return ''
              },
            },
          ],
          responsive: true,
          autoWidth: false,
          language: configsDt.defaultLanguageDatatable,
          destroy: true,
        })

        $('#comments-datatable tbody')
          .off('click')
          .on('click', 'button', function () {
            const commentId = $(this).data('id')
            const comment = vm.allComments.find((c) => c.id === commentId)
            if ($(this).hasClass('btn-approve')) {
              vm.approveComment(commentId)
            } else if ($(this).hasClass('btn-reject')) {
              vm.openRejectModal(comment)
            }
          })
      })
    },
    reloadDataTable() {
      this.initDataTable()
    },
    async approveComment(commentId) {
      try {
        const response = await putToApi(`/staff-comments/${commentId}`, {
          trangThai: true,
          lyDoHuy: null,
        })
        if (response.success) {
          Swal.fire({
            icon: 'success',
            title: 'Thành công!',
            text: 'Bình luận đã được phê duyệt.',
            timer: 1500,
            showConfirmButton: false,
          })
          await this.fetchComments()
        } else {
          Swal.fire('Lỗi', response.message || 'Không thể phê duyệt bình luận.', 'error')
        }
      } catch (error) {
        console.error('Failed to approve comment:', error)
        Swal.fire('Lỗi', 'Thao tác thất bại.', 'error')
      }
    },
    openRejectModal(comment) {
      this.selectedComment = comment
      this.rejectionReason = ''
      this.showReasonModal = true
    },
    async rejectComment() {
      if (!this.selectedComment) return
      try {
        const response = await putToApi(`/staff-comments/${this.selectedComment.id}`, {
          trangThai: false,
          lyDoHuy: this.rejectionReason,
        })
        if (response.success) {
          Swal.fire({
            icon: 'success',
            title: 'Thành công!',
            text: 'Bình luận đã bị từ chối.',
            timer: 1500,
            showConfirmButton: false,
          })
          this.showReasonModal = false
          await this.fetchComments()
        } else {
          Swal.fire('Lỗi', response.message || 'Không thể từ chối bình luận.', 'error')
        }
      } catch (error) {
        console.error('Failed to reject comment:', error)
        Swal.fire('Lỗi', 'Thao tác thất bại.', 'error')
      }
    },
  },
  watch: {
    filterStatus() {
      this.reloadDataTable()
    },
  },
}
</script>

<style scoped>
.card-header h3 {
  margin: 0;
  font-size: 1.25rem;
}
</style>
