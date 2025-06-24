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
const reasonCancel = ref('')
const idUser = ref('')
const roleUser = ref('')
const pendingOrder = ref(null)
const pendingStatus = ref('')
const readToken = ref({})
const openModal = () => {
  isOpenModal.value = !isOpenModal.value
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
// Fetch dữ liệu từ API
const fetchOrders = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/Login')
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
      router.push('/Login')
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
          'Authorization': 'Bearer ' + accessToken.value
        },
        body: JSON.stringify({
          status: newStatus,
          maNv: idUser.value,
          paymentMethod: order.hinhThucTt,
          reasonCancel: reasonCancel.value,
        }),
      })
      if (!response.ok) {
        throw new Error('Failed to updateStatusOrder')
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
    router.push('/Login')
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
      console.log(content)
      const response = await fetch(`${getUrlAPI.value}/api/Orders/${pendingOrder.value.maHd}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + accessToken.value
        },
        body: JSON.stringify(content),
      })
      if (!response.ok) {
        throw new Error('Failed to updateStatusOrder')
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
    router.push('/Login')
    return
  } else {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
    idUser.value = readToken.value.IdUser
    roleUser.value = readToken.value.Role
    fetchOrders()
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
</script>

<template>
  <div style="margin-top: 100px" class="container-fluid">
    <!-- Header -->
    <div class="mb-4">
      <h2 class="text-center mb-4">Quản lý đơn hàng</h2>
      <div class="row justify-content-center">
        <div class="col-md-8">
          <div class="d-flex gap-2 justify-content-center">
            <div class="input-group" style="width: 300px">
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
    <div v-else class="table-responsive">
      <table class="table table-hover">
        <thead class="table-light">
          <tr>
            <th>Mã đơn hàng</th>
            <th>Người đặt</th>
            <th>Ngày đặt</th>
            <th>Tổng tiền</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in listOrders" :key="order.maHd">
            <td>{{ order.maHd }}</td>
            <td>{{ order.tenKh }}</td>
            <td>{{ formatDate(order.ngayTao) }}</td>
            <td>
              {{ formatCurrency(order.tienGoc + order.phiVanChuyen - (order.giamGiaCoupon || 0)) }}
            </td>
            <td>
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
                class="btn btn-sm btn-info me-1"
                title="Xem chi tiết"
                data-bs-toggle="modal"
                :data-bs-target="`#orderDetailModal_${order.maHd}`"
              >
                <i class="fas fa-eye"></i>
              </button>
              <detailsOrderModal :Order="order" @close="fetchOrders" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <nav aria-label="Page navigation" class="mt-4">
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
</style>