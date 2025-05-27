<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import detailsOrderModal from '../orders/details.vue'
const listOrders = ref([])
const searchQuery = ref('')
const statusFilter = ref('')
const loading = ref(false)
const selectedOrder = ref(null)
const statusOrders = ref([])
const getUrlAPI = ref('https://localhost:7217/api')
const totalPages = ref(0)
const pageSelected = ref(1)
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
    loading.value = true
    const response = await axios.get(`${getUrlAPI.value}/Orders?search=${searchQuery.value}&filter=${statusFilter.value}&page=${pageSelected.value}`)
    listOrders.value = response.data.data
    totalPages.value = response.data.toTalPage
  } catch (error) {
    console.error('Lỗi khi tải dữ liệu:', error)
  } finally {
    loading.value = false
  }
}

// Lọc đơn hàng theo trạng thái và từ khóa tìm kiếm
function filterOrders(){
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
const handleStatusChange = async (orderId, newStatus) => {
  try {
    await axios.put(`${getUrlAPI.value}/Orders/${orderId}/status`, {
      tinhTrang: newStatus,
    })
    await fetchOrders() // Refresh lại dữ liệu
  } catch (error) {
    console.error('Lỗi khi cập nhật trạng thái:', error)
  }
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= totalPages.value) {
    pageSelected.value = page
    fetchOrders()
  }
}
onMounted(() => {
  fetchOrders()
})

const filteredStatusOptions = computed(() => {
  return (tinhTrang) => {
    if (tinhTrang?.toLowerCase() === 'chờ xác nhận') {
      return statusOrders.value.filter((status) => !['đang xử lý vnpay'].includes(status.toLowerCase()))
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
            <select @change="filterOrders()" v-model="statusFilter" class="form-select" style="width: 200px">
              <option value="">Tất cả trạng thái</option>
              <option v-for="status in statusOrders" :key="status" :value="status">
                {{ status }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="table-responsive">
      <table class="table table-hover">
        <thead class="table-light">
          <tr>
            <th>Mã đơn hàng</th>
            <th>Khách hàng</th>
            <th>Ngày đặt</th>
            <th>Tổng tiền</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="order in listOrders" :key="order.maHd">
            <td>{{ order.maHd }}</td>
            <td>{{ order.hoTen }}</td>
            <td>{{ formatDate(order.ngayTao) }}</td>
            <td>{{ formatCurrency(order.tienGoc + order.phiVanChuyen) }}</td>
            <td>
              <select
                class="form-select form-select-sm w-50"
                :value="order.tinhTrang"
                @change="handleStatusChange(order.maHd, $event.target.value)"
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
  </div>
</template>

<style>
.table th {
  white-space: nowrap;
}
</style>