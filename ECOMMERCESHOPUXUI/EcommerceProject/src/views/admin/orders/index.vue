<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import detailsOrderModal from '../orders/details.vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { read } from 'xlsx'
const router = useRouter()
const listOrders = ref([])
const searchQuery = ref('')
const statusFilter = ref('')
const loading = ref(false)
const statusOrders = ref([])
const getUrlAPI = ref(GetApiUrl())
const totalPages = ref(0)
const pageSelected = ref(1)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const isOpenModal = ref(false)
const isOpenModalDetails = ref(false)
const reasonCancel = ref('')
const idUser = ref('')
const roleUser = ref('')
const pendingOrder = ref(null)
const pendingStatus = ref('')
const readToken = ref({})
const selectedOrder = ref({})

const openModal = () => {
  isOpenModal.value = !isOpenModal.value
}
const openDetailsModal = (order) => {
  isOpenModalDetails.value = !isOpenModalDetails.value
  selectedOrder.value = order
}
statusOrders.value = [
  'Đang xử lý VNPAY',
  'Chờ xác nhận',
  'Đã xác nhận',
  'Đã giao cho đơn vị vận chuyển',
  'Đã nhận',
  'Đã thanh toán',
  'Đã hủy',
  'Hoàn trả/Hoàn tiền',
]
const countByStatus = computed(() => {
  const counts = {}
  for (const status of statusOrders.value) {
    if (status.toLowerCase() === 'đang xử lý vnpay') continue
    counts[status] = listOrders.value.filter(
      (order) => order.tinhTrang?.toLowerCase() === status.toLowerCase()
    ).length
  }
  return counts
})
// Fetch dữ liệu từ API
const fetchOrders = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/LoginStaff')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      readToken.value = decodeToken(accessToken.value)
      idUser.value = readToken.value.IdUser
      roleUser.value = readToken.value.Role
      loading.value = true
      const response = await axios.get(
        `${getUrlAPI.value}/api/Orders?search=${searchQuery.value}&filter=${statusFilter.value}&page=${pageSelected.value}`,
        {
          headers: {
            Authorization: `Bearer ${accessToken.value}`,
          },
        }
      )
      listOrders.value = response.data.data
      totalPages.value = response.data.toTalPage
    }
  } catch (error) {
    if (error.response && error.response.status >= 400 && error.response.status <= 403) {
      router.push('/LoginStaff')
      return
    }
    console.error('Lỗi khi tải dữ liệu:', error)
  } finally {
    loading.value = false
  }
}

// Lọc đơn hàng theo trạng thái và từ khóa tìm kiếm
function filterOrders() {
  fetchOrders()
}

// Format số tiền
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount)
}

// Format ngày tháng
const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

// Xử lý cập nhật trạng thái
const handleStatusChange = async (order, oldStatus, newStatus) => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/LoginStaff')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      readToken.value = decodeToken(accessToken.value)
      idUser.value = readToken.value.IdUser
      roleUser.value = readToken.value.Role
      if (
        idUser.value.toLowerCase() != String(order?.maNv || '').toLowerCase() &&
        order?.maNv != null &&
        roleUser.value.toLowerCase() != 'admin'
      ) {
        console.log(idUser.value)
        Swal.fire({
          title:
            'Đơn hàng này đang được phụ trách bởi nhân viên khác. Bạn không có quyền thay đổi trạng thái đơn hàng này.',
          icon: 'error',
          timer: 2500,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        return
      }
      if (['hoàn trả/hoàn tiền', 'đã hủy'].includes(newStatus.toLowerCase())) {
        isOpenModal.value = true
        pendingOrder.value = order
        pendingStatus.value = newStatus
        return
      }
      // Chỉ gọi API nếu không phải 2 trạng thái đặc biệt
      const response = await fetch(`${getUrlAPI.value}/api/Orders/${order.maHd}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + accessToken.value,
        },
        body: JSON.stringify({
          status: newStatus,
          maNv: idUser.value,
          paymentMethod: order.hinhThucTt,
          reasonCancel: reasonCancel.value,
        }),
      })
      if (!response.ok) {
        if (response.status >= 400 && response.status <= 403) {
          router.push('/LoginStaff')
          return
        } else {
          throw new Error('Failed to updateStatusOrder')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Cập nhật trạng thái đơn hàng thành công.',
          icon: 'success',
          timer: 2500,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        await fetchOrders()
      }
    }
  } catch (error) {
    console.error('Lỗi khi cập nhật trạng thái:', error)
  }
}

const confirmCancel = async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/LoginStaff')
    return
  } else {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
    idUser.value = readToken.value.IdUser
    roleUser.value = readToken.value.Role
    if (!reasonCancel.value.trim()) {
      Swal.fire('Vui lòng nhập lý do!', '', 'warning')
      return
    }
    try {
      var readtoken = decodeToken(accessToken.value)
      idUser.value = readtoken.IdUser
      const content = {
        status: pendingStatus.value,
        maNv: idUser.value,
        paymentMethod: pendingOrder.value.hinhThucTt,
        reasonCancel: reasonCancel.value,
      }
      const response = await fetch(`${getUrlAPI.value}/api/Orders/${pendingOrder.value.maHd}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + accessToken.value,
        },
        body: JSON.stringify(content),
      })
      if (!response.ok) {
        if (response.status >= 400 && response.status <= 403) {
          router.push('/LoginStaff')
          return
        } else {
          throw new Error('Failed to updateStatusOrder')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Cập nhật trạng thái đơn hàng thành công.',
          icon: 'success',
          timer: 2500,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        isOpenModal.value = false
        pendingOrder.value = null
        pendingStatus.value = ''
        reasonCancel.value = ''
        await fetchOrders()
      }
    } catch (error) {
      console.error('Lỗi khi cập nhật trạng thái:', error)
    }
  }
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= totalPages.value) {
    pageSelected.value = page
    fetchOrders()
  }
}
onMounted(async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/LoginStaff')
    return
  } else {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
    idUser.value = readToken.value.IdUser
    roleUser.value = readToken.value.Role
    await fetchOrders()
  }
})

const filteredStatusOptions = computed(() => {
  return (tinhTrang) => {
    if (tinhTrang?.toLowerCase() === 'chờ xác nhận') {
      return statusOrders.value.filter(
        (status) => !['đang xử lý vnpay', 'đã nhận', 'đã thanh toán'].includes(status.toLowerCase())
      )
    }
    if (tinhTrang?.toLowerCase() === 'đã xác nhận') {
      return statusOrders.value.filter(
        (status) =>
          !['chờ xác nhận', 'đang xử lý vnpay', 'hoàn trả/hoàn tiền'].includes(status.toLowerCase())
      )
    }
    if (tinhTrang?.toLowerCase() === 'đã giao cho đơn vị vận chuyển') {
      return statusOrders.value.filter(
        (status) =>
          !['chờ xác nhận', 'đã xác nhận', 'đang xử lý vnpay', 'hoàn trả/hoàn tiền'].includes(
            status.toLowerCase()
          )
      )
    }
    if (tinhTrang?.toLowerCase() === 'đã nhận') {
      return statusOrders.value.filter(
        (status) =>
          ![
            'chờ xác nhận',
            'đã xác nhận',
            'đã giao cho đơn vị vận chuyển',
            'đang xử lý vnpay',
            'hoàn trả/hoàn tiền',
          ].includes(status.toLowerCase())
      )
    }
    if (tinhTrang?.toLowerCase() === 'đã thanh toán') {
      return statusOrders.value.filter(
        (status) =>
          ![
            'chờ xác nhận',
            'đã xác nhận',
            'đã giao cho đơn vị vận chuyển',
            'đã nhận',
            'đang xử lý vnpay',
            'đã hủy',
          ].includes(status.toLowerCase())
      )
    }
    if (tinhTrang?.toLowerCase() === 'đã hủy') {
      return ['Đã hủy']
    }
    if (tinhTrang?.toLowerCase() === 'hoàn trả/hoàn tiền') {
      return ['Hoàn trả/Hoàn tiền']
    }
    return statusOrders.value
  }
})

const exportOrder = async (order) => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/LoginStaff')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  axios
    .get(getUrlAPI.value + `/api/Orders/xuat-pdf/${order.maHd}`, {
      headers: { Authorization: `Bearer ${accessToken.value}` },
      responseType: 'blob',
    })
    .then((res) => {
      const blob = new Blob([res.data], { type: 'application/pdf' })
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `HoaDon_${order.maHd}.pdf`
      link.click()
    })
}
</script>

<template>
  <div style="margin-top: 100px; width: 1170px" class="container-fluid">
    <!-- Header -->
    <div class="mb-4">
      <h2 class="text-center mb-4" style="font-size: 3rem">QUẢN LÝ ĐƠN HÀNG</h2>
      <div class="row justify-content-center">
        <div class="col-md-8">
          <div class="d-flex gap-2 justify-content-center">
            <div class="input-group" style="width: 700px">
              <input
                @input="filterOrders()"
                v-model="searchQuery"
                style="background-color: white"
                type="text"
                class="form-control"
                placeholder="Tìm kiếm đơn hàng..."
                aria-label="Tìm kiếm đơn hàng"
              />
            </div>
            <select
              @change="filterOrders()"
              v-model="statusFilter"
              class="form-select"
              style="width: 200px"
            >
              <option value="">Tất cả trạng thái</option>
              <option v-for="status in statusOrders" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>
    <div v-if="loading" class="my-loading-spinner text-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <div class="fw-semibold text-primary mt-2">Đang tải dữ liệu...</div>
    </div>
    <!-- Table -->
    <div v-else class="table-responsive" >
      <!-- Thống kê số lượng theo trạng thái -->
      <div
        class="status-summary mb-4"
        style="display: grid; grid-template-columns: repeat(7, 1fr); gap: 8px"
      >
        <div
          v-for="status in statusOrders.filter((s) => s.toLowerCase() !== 'đang xử lý vnpay')"
          :key="status"
          class="card text-center p-2"
          style="min-height: 80px"
        >
          <div style="font-size: 0.75rem; line-height: 1rem">{{ status }}</div>
          <div class="fw-bold" style="font-size: 0.9rem">{{ countByStatus[status] }} đơn</div>
        </div>
      </div>

      <table class="table table-hover table-bordered" >
        <thead class="table-light">
          <tr>
            <th style="border-right: 1px solid #dee2e6">Mã đơn hàng</th>
            <th style="border-right: 1px solid #dee2e6">Người đặt</th>
            <th style="border-right: 1px solid #dee2e6">Ngày đặt</th>
            <th style="border-right: 1px solid #dee2e6">Tổng tiền</th>
            <th style="border-right: 1px solid #dee2e6">Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in listOrders" :key="order.maHd">
            <td style="border-right: 1px solid #dee2e6">{{ order.maHd }}</td>
            <td style="border-right: 1px solid #dee2e6">{{ order.tenKh }}</td>
            <td style="border-right: 1px solid #dee2e6">{{ formatDate(order.ngayTao) }}</td>
            <td style="border-right: 1px solid #dee2e6">
              {{ formatCurrency(order.tienGoc + order.phiVanChuyen - (order.giamGiaCoupon || 0)) }}
            </td>
            <td style="border-right: 1px solid #dee2e6">
              <select
                :disabled="
                  idUser != order.maNv &&
                  order.maNv != undefined &&
                  roleUser.toLowerCase() != 'admin'
                "
                class="form-select form-select-sm w-50"
                :value="order.tinhTrang"
                @change="handleStatusChange(order, order.tinhTrang, $event.target.value)"
              >
                <option
                  :selected="status.toLowerCase() === order.tinhTrang.toLowerCase()"
                  v-for="status in filteredStatusOptions(order.tinhTrang)"
                  :key="status"
                  :value="status"
                >
                  {{ status }}
                </option>
              </select>
              <span
                v-if="
                  idUser != order.maNv &&
                  order.maNv != undefined &&
                  roleUser.toLowerCase() != 'admin'
                "
                class="text-danger small fst-italic"
              >
                Đơn hàng này đang được phụ trách bởi nhân viên khác. Bạn không có quyền thay đổi
                trạng thái đơn hàng này.
              </span>
            </td>
            <td>
              <button
                type="button"
                class="btn btn-sm btn-info me-1"
                title="Xem chi tiết"
                @click="openDetailsModal(order)"
              >
                Chi tiết
              </button>
              <button
                @click="exportOrder(order)"
                class="btn btn-sm me-1"
                style="background-color: #2eb938; color: white"
              >
                Tải xuống
              </button>
            </td>
          </tr>
          <detailsOrderModal
            :Order="selectedOrder"
            @close="isOpenModalDetails = $event"
            :isOpenModalDetails="isOpenModalDetails"
          />
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <nav aria-label="Page navigation" class="mt-4" style="margin-bottom: 30px">
      <ul class="pagination justify-content-center">
        <li @click="ChangePage(1)" class="page-item disabled">
          <a class="page-link" href="#" tabindex="-1" aria-disabled="true">Trước</a>
        </li>
        <li
          @click="ChangePage(page)"
          v-for="page in totalPages"
          :key="page"
          :class="{ 'page-item': true, active: page === pageSelected }"
        >
          <a class="page-link" href="#">{{ page }}</a>
        </li>
        <li @click="ChangePage(totalPages)" class="page-item">
          <a class="page-link" href="#">Sau</a>
        </li>
      </ul>
    </nav>
    <!-- Modal nhập lý do hủy/hoàn trả -->
    <div
      v-if="isOpenModal"
      class="modal fade show"
      style="display: block; background-color: rgba(0, 0, 0, 0.5)"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              Nhập lý do {{ pendingStatus === 'đã hủy' ? 'hủy' : 'hoàn tiền' }}
            </h5>
            <button type="button" class="btn-close" @click="isOpenModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label for="cancelReason" class="form-label">Lý do:</label>
              <textarea
                id="cancelReason"
                v-model="reasonCancel"
                class="form-control"
                rows="4"
                placeholder="Nhập lý do..."
              ></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="isOpenModal = false">
              Đóng
            </button>
            <button type="button" class="btn btn-primary" @click="confirmCancel">Xác nhận</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style>
.table th {
  white-space: nowrap;
}
.modal {
  display: none;
  position: fixed;
}
.modal.show {
  display: block;
}
.table th,
.table td {
  border-right: 1px solid #dee2e6;
}
.table th:last-child,
.table td:last-child {
  border-right: none;
}
</style>