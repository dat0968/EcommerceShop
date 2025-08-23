<template>
  <div class="" style="margin-top: 20px; margin-left: 100px; margin-right: 100px">
    <!-- Header -->
    <nav aria-label="breadcrumb" class="mb-4">
      <ol class="breadcrumb">
        <li class="breadcrumb-item">
          <router-link to="/" class="text-decoration-none text-muted">Trang chủ</router-link>
        </li>
        <li class="breadcrumb-item">
          <a href="/Shop" class="text-decoration-none text-muted">Sản phẩm</a>
        </li>
        <li class="breadcrumb-item active text-muted" aria-current="page">Sản phẩm yêu thích</li>
      </ol>
    </nav>
    <div class="header-content">
      <button class="back-btn d-none d-md-flex" @click="router.back()">
        <i class="fas fa-arrow-left"></i>
      </button>
      <h1 class="orders-title">Đơn hàng của tôi</h1>
    </div>

    <!-- Search and Filter -->
    <div class="search-filter-section">
      <div class="search-input-wrapper">
        <i class="fas fa-search search-icon"></i>
        <input
          v-model="search"
          @input="handleSearch"
          class="search-input"
          placeholder="Tìm kiếm đơn hàng..."
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <div class="loading-text">Đang tải dữ liệu...</div>
    </div>

    <!-- Orders Content -->
    <div v-else class="orders-content">
      <!-- Status Tabs -->
      <div class="status-tabs">
        <button
          v-for="tab in statusTabs"
          :key="tab.value"
          :class="['tab-button', { active: activeTab === tab.value }]"
          @click="setActiveTab(tab.value)"
        >
          {{ tab.label }}
        </button>
      </div>

      <!-- Orders List -->
      <div class="orders-list">
        <div v-for="order in filteredOrders" :key="order.maHd" class="order-card">
          <!-- Order Header -->
          <div class="order-header">
            <div class="order-info">
              <h3 class="order-id">Đơn hàng #{{ order.maHd }}</h3>
              <p class="order-date">{{ formatDate(order.ngayTao) }}</p>
            </div>
            <div class="order-status">
              <span :class="['status-badge', getStatusClass(order.tinhTrang)]">
                <i :class="getStatusIcon(order.tinhTrang)"></i>
                {{ order.tinhTrang }}
              </span>
            </div>
          </div>

          <!-- Order Items Preview -->
          <div class="order-items">
            <div
              v-for="(item, index) in order.cthoadons.slice(0, 2)"
              :key="index"
              class="order-item"
            >
              <div class="item-image" style="width: 200px; height: 200px">
                <img :src="'/placeholder.svg'" :alt="item.tenSanPham" />
              </div>
              <div class="item-details" style="margin-left: 50px">
                <p class="item-name" style="font-size: 1.2rem">
                  {{ item.tenSanPham }} {{ item.tenCombo }}
                </p>
                <p class="item-variant" v-if="item.bienThe" style="font-size: 1.2rem">
                  {{ item.bienThe }}
                </p>
                <p class="item-quantity" style="font-size: 1.2rem">x{{ item.soLuong }}</p>
              </div>
              <div class="item-price" style="font-size: 1.2rem">
                {{ formatCurrency(item.gia) }}
              </div>
            </div>

            <!-- Show more items indicator -->
            <div v-if="order.cthoadons.length > 2" class="more-items">
              +{{ order.cthoadons.length - 2 }} sản phẩm khác
            </div>
          </div>

          <!-- Order Footer -->
          <div class="order-footer">
            <div class="order-total">
              <span class="total-label">Tổng cộng:</span>
              <span class="total-amount" style="font-size: 1.5rem">
                {{ formatCurrency(order.tienGoc + order.phiVanChuyen - order.giamGiaCoupon) }}
              </span>
            </div>

            <div class="order-actions">
              <!-- Cancel/Return button -->
              <button
                v-if="canCancelOrder(order)"
                class="action-btn cancel-btn"
                @click="openCancelModal(order)"
              >
                Hủy đơn hàng
              </button>

              <!-- Rate button for delivered orders -->
              <button
                v-if="order.tinhTrang.toLowerCase() === 'đã nhận'"
                class="action-btn rate-btn"
              >
                <i class="fas fa-star"></i>
                Đánh giá
              </button>

              <!-- View details button -->
              <button class="action-btn details-btn" @click="openModal(order)">Xem chi tiết</button>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-if="filteredOrders.length === 0" class="empty-state">
          <i class="fas fa-box-open empty-icon"></i>
          <h3>Không có đơn hàng nào</h3>
          <p>{{ getEmptyStateMessage() }}</p>
        </div>
      </div>

      <!-- Pagination -->
      <nav v-if="totalPage > 1" class="pagination-nav">
        <ul class="pagination">
          <li :class="['page-item', { disabled: page === 1 }]">
            <button class="page-link" @click="changePage(page - 1)" :disabled="page === 1">
              Trước
            </button>
          </li>
          <li v-for="p in displayedPages" :key="p" :class="['page-item', { active: p === page }]">
            <button class="page-link" @click="changePage(p)">{{ p }}</button>
          </li>
          <li :class="['page-item', { disabled: page === totalPage }]">
            <button class="page-link" @click="changePage(page + 1)" :disabled="page === totalPage">
              Sau
            </button>
          </li>
        </ul>
      </nav>
    </div>

    <!-- Order Details Modal -->
    <div v-if="showModal && selectedOrder" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop style="width: 1000px">
        <div class="modal-header">
          <h2 class="modal-title">Chi tiết đơn hàng: {{ selectedOrder.maHd }}</h2>
          <button class="modal-close" @click="closeModal">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <div class="modal-body">
          <!-- Order Summary -->
          <div class="order-summary-grid">
            <div class="summary-section">
              <h4>Thông tin đơn hàng</h4>
              <div class="info-grid">
                <div class="info-item">
                  <span class="label">Ngày tạo:</span>
                  <span class="value">{{ formatDate(selectedOrder.ngayTao) }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Ngày thanh toán:</span>
                  <span class="value">{{
                    selectedOrder.ngayThanhToan
                      ? formatDate(selectedOrder.ngayThanhToan)
                      : 'Chưa xác nhận'
                  }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Người nhận:</span>
                  <span class="value">{{ selectedOrder.hoTen }}</span>
                </div>
                <div class="info-item">
                  <span class="label">SĐT:</span>
                  <span class="value">{{ selectedOrder.sdt }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Địa chỉ:</span>
                  <span class="value">{{ selectedOrder.diaChiNhanHang }}</span>
                </div>
              </div>
            </div>

            <div class="summary-section">
              <h4>Thanh toán & Vận chuyển</h4>
              <div class="info-grid">
                <div class="info-item">
                  <span class="label">Hình thức thanh toán:</span>
                  <span class="value">{{ selectedOrder.hinhThucTt }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Tình trạng:</span>
                  <span :class="['value', 'status-text', getStatusClass(selectedOrder.tinhTrang)]">
                    {{ selectedOrder.tinhTrang }}
                  </span>
                </div>
                <div class="info-item">
                  <span class="label">Phí vận chuyển:</span>
                  <span class="value">{{ formatCurrency(selectedOrder.phiVanChuyen) }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Giảm giá:</span>
                  <span class="value text-success"
                    >-{{ formatCurrency(selectedOrder.giamGiaCoupon) }}</span
                  >
                </div>
                <div class="info-item total-row">
                  <span class="label">Tổng thanh toán:</span>
                  <span class="value total-price">
                    {{
                      formatCurrency(
                        selectedOrder.tienGoc -
                          selectedOrder.giamGiaCoupon +
                          selectedOrder.phiVanChuyen
                      )
                    }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Products Table -->
          <div class="products-section">
            <h4>Danh sách sản phẩm</h4>
            <div class="products-table">
              <div class="table-header">
                <div class="col-product">Sản phẩm</div>
                <div class="col-price">Đơn giá</div>
                <div class="col-quantity">Số lượng</div>
                <div class="col-total">Thành tiền</div>
              </div>
              <div
                v-for="(item, idx) in selectedOrder.cthoadons.filter((i) => !i.maCombo)"
                :key="'product-' + idx"
                class="table-row"
              >
                <div class="col-product">
                  <div class="product-info">
                    <strong>{{ item.tenSanPham }}</strong>
                    <div v-if="item.bienThe" class="product-variant">{{ item.bienThe }}</div>
                  </div>
                </div>
                <div class="col-price">
                  <span class="current-price">{{ formatCurrency(item.gia) }}</span>
                  <span v-if="item.gia != item.giaGoc" class="original-price">
                    {{ formatCurrency(item.giaGoc) }}
                  </span>
                </div>
                <div class="col-quantity">{{ item.soLuong }}</div>
                <div class="col-total">{{ formatCurrency(item.gia * item.soLuong) }}</div>
              </div>
            </div>
          </div>

          <!-- Combos Table (if any) -->
          <div v-if="selectedOrder.cthoadons.some((i) => i.maCombo)" class="combos-section">
            <h4>Danh sách combo</h4>
            <div class="combos-table">
              <div class="table-header">
                <div class="col-combo">Combo</div>
                <div class="col-price">Đơn giá</div>
                <div class="col-quantity">Số lượng</div>
                <div class="col-total">Thành tiền</div>
              </div>
              <div
                v-for="(comboItem, cidx) in selectedOrder.cthoadons.filter((i) => i.maCombo)"
                :key="'combo-' + cidx"
                class="table-row"
              >
                <div class="col-combo">
                  <div class="combo-info">
                    <strong>{{ comboItem.tenCombo }}</strong>
                    <div class="combo-items">
                      <div
                        v-for="combo in selectedOrder.chitietcombohoadons.filter(
                          (c) => c.maCombo === comboItem.maCombo
                        )"
                        :key="combo.maCtsp"
                        class="combo-item"
                      >
                        {{ combo.tenSanPham }} ({{ combo.mauSac }}-{{ combo.kichThuoc }})
                        <span class="combo-quantity"
                          >({{ combo.soLuong / comboItem.soLuong }} sản phẩm)</span
                        >
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-price">
                  <span class="current-price">{{ formatCurrency(comboItem.gia) }}</span>
                  <span class="original-price">{{ formatCurrency(comboItem.giaGoc) }}</span>
                </div>
                <div class="col-quantity">{{ comboItem.soLuong }}</div>
                <div class="col-total">{{ formatCurrency(comboItem.gia * comboItem.soLuong) }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Cancel Order Modal -->
    <div v-if="showCancelModal" class="modal-overlay" @click="closeCancelModal">
      <div class="cancel-modal-content" @click.stop>
        <div class="modal-header">
          <h3 class="modal-title">Hủy đơn hàng #{{ orderToCancel?.maHd }}</h3>
          <button class="modal-close" @click="closeCancelModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <p class="cancel-description">
            Vui lòng cho chúng tôi biết lý do bạn muốn hủy đơn hàng này:
          </p>
          <div class="reason-options">
            <label v-for="reason in cancelReasons" :key="reason" class="reason-option">
              <input
                type="radio"
                :value="reason"
                v-model="selectedCancelReason"
                name="cancelReason"
              />
              <span class="reason-text">{{ reason }}</span>
            </label>
          </div>
          <div v-if="selectedCancelReason === 'Lý do khác'" class="custom-reason">
            <textarea
              v-model="cancelReason"
              class="custom-reason-input"
              placeholder="Nhập lý do cụ thể..."
              rows="3"
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" @click="closeCancelModal">Hủy</button>
          <button class="btn btn-danger" @click="confirmCancelOrder" :disabled="waitingCancel">
            <span v-if="!waitingCancel">Xác nhận hủy đơn</span>
            <span v-else> <i class="fas fa-spinner fa-spin"></i> Đang xử lý... </span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { decodeToken, validateToken } from '@/utils/auth'
import { useRoute, useRouter } from 'vue-router'

// Reactive variables
const getUrlAPI = ref(GetApiUrl())
const orders = ref([])
const showModal = ref(false)
const selectedOrder = ref(null)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const router = useRouter()
const showCancelModal = ref(false)
const cancelReason = ref('')
const selectedCancelReason = ref('')
const orderToCancel = ref(null)
const page = ref(1)
const totalPage = ref(1)
const search = ref('')
const loading = ref(false)
const filter = ref('')
const activeTab = ref('all')
const waitingCancel = ref(false)
// Status options and tabs
const statusOptions = [
  'Đang xử lý VNPAY',
  'Chờ xác nhận',
  'Đã xác nhận',
  'Đã giao cho đơn vị vận chuyển',
  'Đã nhận',
  'Đã thanh toán',
  'Đã hủy',
  'Hoàn trả/Hoàn tiền',
]

const statusTabs = [
  { value: 'all', label: 'Tất cả' },
  { value: 'processing', label: 'Xử lý' },
  { value: 'shipping', label: 'Giao hàng' },
  { value: 'delivered', label: 'Đã giao' },
  { value: 'cancelled', label: 'Đã hủy' },
]

const cancelReasons = [
  'Tôi đổi ý không muốn mua nữa',
  'Tìm được sản phẩm tương tự với giá tốt hơn',
  'Thời gian giao hàng quá lâu',
  'Tôi đặt nhầm sản phẩm/số lượng',
  'Lý do khác',
]

// Computed properties
const filteredOrders = computed(() => {
  let filtered = orders.value

  if (activeTab.value !== 'all') {
    filtered = filtered.filter((order) => {
      const status = order.tinhTrang.toLowerCase()
      switch (activeTab.value) {
        case 'processing':
          return status.includes('xử lý') || status.includes('chờ xác nhận')
        case 'shipping':
          return status.includes('giao') && !status.includes('đã nhận')
        case 'delivered':
          return status.includes('đã nhận') || status.includes('đã thanh toán')
        case 'cancelled':
          return status.includes('đã hủy') || status.includes('hoàn trả')
        default:
          return true
      }
    })
  }

  return filtered
})

const displayedPages = computed(() => {
  // Show pagination logic for better UX
  const pages = []
  const maxPages = 5
  let start = Math.max(1, page.value - Math.floor(maxPages / 2))
  let end = Math.min(totalPage.value, start + maxPages - 1)

  if (end - start + 1 < maxPages) {
    start = Math.max(1, end - maxPages + 1)
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return pages
})

// Helper functions
function formatCurrency(val) {
  return val?.toLocaleString('vi-VN') + ' ₫'
}

function formatDate(dateStr) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString('vi-VN')
}

function getStatusClass(status) {
  const statusLower = status.toLowerCase()
  if (statusLower.includes('chờ') || statusLower.includes('xử lý')) return 'status-processing'
  if (statusLower.includes('giao') && !statusLower.includes('đã nhận')) return 'status-shipping'
  if (statusLower.includes('đã nhận') || statusLower.includes('đã thanh toán'))
    return 'status-delivered'
  if (statusLower.includes('hủy') || statusLower.includes('hoàn trả')) return 'status-cancelled'
  return 'status-default'
}

function getStatusIcon(status) {
  const statusLower = status.toLowerCase()
  if (statusLower.includes('chờ') || statusLower.includes('xử lý')) return 'fas fa-clock'
  if (statusLower.includes('giao') && !statusLower.includes('đã nhận')) return 'fas fa-truck'
  if (statusLower.includes('đã nhận') || statusLower.includes('đã thanh toán'))
    return 'fas fa-check-circle'
  if (statusLower.includes('hủy') || statusLower.includes('hoàn trả')) return 'fas fa-times-circle'
  return 'fas fa-box'
}

function canCancelOrder(order) {
  const status = order.tinhTrang.toLowerCase()
  return (
    status === 'chờ xác nhận' ||
    (status === 'đã thanh toán' &&
      Date.now() - new Date(order.ngayThanhToan).getTime() <= 3 * 24 * 60 * 60 * 1000)
  )
}

function getEmptyStateMessage() {
  if (activeTab.value === 'all') return 'Bạn chưa có đơn hàng nào'
  return `Không có đơn hàng ${statusTabs
    .find((tab) => tab.value === activeTab.value)
    ?.label.toLowerCase()}`
}

// Tab functions
function setActiveTab(tab) {
  activeTab.value = tab
  page.value = 1
}

// Modal functions
function openModal(order) {
  selectedOrder.value = order
  showModal.value = true
  document.body.style.overflow = 'hidden'
}

function closeModal() {
  showModal.value = false
  selectedOrder.value = null
  document.body.style.overflow = 'auto'
}

function openCancelModal(order) {
  orderToCancel.value = order
  selectedCancelReason.value = ''
  cancelReason.value = ''
  showCancelModal.value = true
  document.body.style.overflow = 'hidden'
}

function closeCancelModal() {
  showCancelModal.value = false
  orderToCancel.value = null
  selectedCancelReason.value = ''
  cancelReason.value = ''
  document.body.style.overflow = 'auto'
}

// API functions
async function fetchAPI() {
  loading.value = true
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (!validatetoken.isValid) {
      router.push('/Login')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      const readToken = decodeToken(accessToken.value)
      const response = await fetch(
        `${getUrlAPI.value}/api/CustomerOrders/${readToken.IdUser}?search=${search.value}&filter=${filter.value}&page=${page.value}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      )
      if (!response.ok) {
        throw new Error('Error to fetchAPIOrderCustomer')
      }
      const result = await response.json()
      orders.value = result.data
      totalPage.value = result.toTalPage
    }
  } finally {
    loading.value = false
  }
}

async function cancelOrder(order) {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (!validatetoken.isValid) {
    router.push('/Login')
    return
  }

  const content = {
    id: order.maHd,
    selectedCancelStatus:
      order.tinhTrang.toLowerCase() === 'chờ xác nhận' ? 'Đã hủy' : 'Hoàn trả/Hoàn tiền',
    reasonCancel:
      selectedCancelReason.value === 'Lý do khác' ? cancelReason.value : selectedCancelReason.value,
  }

  try {
    const response = await fetch(`${getUrlAPI.value}/api/CustomerOrders`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(content),
    })

    if (!response.ok) {
      throw new Error('Failed to Cancel Order')
    }

    const result = await response.json()
    if (result.success) {
      Swal.fire({
        title: 'Hủy đơn hàng thành công',
        icon: 'success',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
      // Refresh orders list
      await fetchAPI()
    }
  } catch (error) {
    console.error(error)
    Swal.fire({
      title: 'Có lỗi xảy ra',
      text: error.message,
      icon: 'error',
    })
  }
}

async function confirmCancelOrder() {
  if (
    !selectedCancelReason.value ||
    (selectedCancelReason.value === 'Lý do khác' && !cancelReason.value.trim())
  ) {
    Swal.fire('Vui lòng chọn lý do hủy đơn!', '', 'warning')
    return
  }

  const confirmResult = await Swal.fire({
    title: 'Xác nhận hủy đơn hàng?',
    text: 'Bạn có chắc chắn muốn hủy đơn hàng này không?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Có, hủy đơn!',
    cancelButtonText: 'Không',
  })

  if (confirmResult.isConfirmed) {
    try {
      waitingCancel.value = true
      await cancelOrder(orderToCancel.value)
      closeCancelModal()
    } catch (error) {
      console.log(error)
    } finally {
      waitingCancel.value = false
    }
  }
}

// Event handlers
function handleSearch() {
  page.value = 1
  fetchAPI()
}

function handleFilter() {
  page.value = 1
  fetchAPI()
}

function changePage(p) {
  if (p !== page.value && p >= 1 && p <= totalPage.value) {
    page.value = p
    fetchAPI()
  }
}

// Lifecycle
onMounted(async () => {
  await fetchAPI()
})

// Watch for search changes with debounce
let searchTimeout
watch(search, () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    handleSearch()
  }, 500)
})
</script>

<style scoped>
/* Container and Layout */
.orders-container {
  min-height: 100vh;
  background-color: #f8fafc;
}

.orders-header {
  background: white;
  border-bottom: 1px solid #e2e8f0;
  padding: 1rem 0;
  position: sticky;
  top: 0;
  z-index: 40;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.back-btn {
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 0.5rem;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
}

.back-btn:hover {
  background-color: #f1f5f9;
  color: #334155;
}

.orders-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
}

/* Search and Filter */
.search-filter-section {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1.5rem 1rem;
}

.search-input-wrapper {
  position: relative;
  max-width: 400px;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 0.875rem;
}

.search-input {
  width: 100%;
  padding: 0.75rem 0.75rem 0.75rem 2.5rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  transition: all 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Loading */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 1rem;
}

.loading-spinner {
  width: 3rem;
  height: 3rem;
  border: 3px solid #e2e8f0;
  border-top: 3px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.loading-text {
  margin-top: 1rem;
  color: #64748b;
  font-weight: 600;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

/* Orders Content */
.orders-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem 2rem;
}

/* Status Tabs */
.status-tabs {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 0.25rem;
  background: white;
  padding: 0.25rem;
  border-radius: 0.75rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-button {
  padding: 0.75rem 0.5rem;
  border: none;
  background: transparent;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
}

.tab-button:hover {
  background-color: #f1f5f9;
  color: #334155;
}

.tab-button.active {
  background-color: #3b82f6;
  color: white;
}

/* Order Cards */
.orders-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-card {
  background: white;
  border-radius: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: all 0.2s;
}

.order-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.25rem 1.25rem 0.75rem;
}

.order-info h3.order-id {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 0.25rem 0;
}

.order-date {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.375rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

.status-processing {
  background-color: #fef3c7;
  color: #d97706;
}

.status-shipping {
  background-color: #dbeafe;
  color: #2563eb;
}

.status-delivered {
  background-color: #dcfce7;
  color: #16a34a;
}

.status-cancelled {
  background-color: #fee2e2;
  color: #dc2626;
}

.status-default {
  background-color: #f1f5f9;
  color: #64748b;
}

/* Order Items */
.order-items {
  padding: 0 1.25rem;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.order-item:last-child {
  border-bottom: none;
}

.item-image {
  width: 3rem;
  height: 3rem;
  flex-shrink: 0;
}

.item-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 0.5rem;
  background-color: #f1f5f9;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-name {
  font-weight: 500;
  color: #1e293b;
  margin: 0 0 0.125rem 0;
  font-size: 0.875rem;
}

.item-variant {
  font-size: 0.75rem;
  color: #64748b;
  margin: 0 0 0.125rem 0;
}

.item-quantity {
  font-size: 0.75rem;
  color: #64748b;
  margin: 0;
}

.item-price {
  font-weight: 600;
  color: #1e293b;
  font-size: 0.875rem;
}

.more-items {
  padding: 0.5rem 0;
  font-size: 0.75rem;
  color: #64748b;
  text-align: center;
  font-style: italic;
}

/* Order Footer */
.order-footer {
  padding: 1rem 1.25rem 1.25rem;
  border-top: 1px solid #f1f5f9;
  background-color: #fafbfc;
}

.order-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.total-label {
  font-weight: 500;
  color: #64748b;
}

.total-amount {
  font-weight: 700;
  color: #3b82f6;
  font-size: 1.125rem;
}

.order-actions {
  display: flex;
  gap: 0.75rem;
}

.action-btn {
  flex: 1;
  padding: 0.625rem 1rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
}

.details-btn {
  background-color: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.details-btn:hover {
  background-color: #2563eb;
  border-color: #2563eb;
}

.cancel-btn {
  background-color: transparent;
  color: #dc2626;
  border-color: #dc2626;
}

.cancel-btn:hover {
  background-color: #dc2626;
  color: white;
}

.rate-btn {
  background-color: transparent;
  color: #f59e0b;
  border-color: #f59e0b;
}

.rate-btn:hover {
  background-color: #f59e0b;
  color: white;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 4rem 1rem;
}

.empty-icon {
  font-size: 4rem;
  color: #d1d5db;
  margin-bottom: 1rem;
}

.empty-state h3 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 0.5rem 0;
}

.empty-state p {
  color: #6b7280;
  margin: 0;
}

/* Pagination */
.pagination-nav {
  margin-top: 2rem;
  display: flex;
  justify-content: center;
}

.pagination {
  display: flex;
  list-style: none;
  gap: 0.25rem;
  margin: 0;
  padding: 0;
}

.page-item {
  display: flex;
}

.page-link {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  background-color: white;
  color: #374151;
  text-decoration: none;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
}

.page-link:hover:not(:disabled) {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.page-item.active .page-link {
  background-color: #3b82f6;
  border-color: #3b82f6;
  color: white;
}

.page-item.disabled .page-link {
  color: #9ca3af;
  cursor: not-allowed;
}

/* Modals */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  overflow-y: auto;
}

.modal-content {
  background: white;
  border-radius: 0.75rem;
  width: 100%;
  max-width: 4xl;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
}

.cancel-modal-content {
  background: white;
  border-radius: 0.75rem;
  width: 100%;
  max-width: 28rem;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 1.5rem 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 0.375rem;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
}

.modal-close:hover {
  background-color: #f3f4f6;
  color: #374151;
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem 1.5rem;
  border-top: 1px solid #e5e7eb;
}

/* Order Summary Grid */
.order-summary-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  margin-bottom: 2rem;
}

.summary-section h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
}

.info-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.info-item .label {
  font-weight: 500;
  color: #6b7280;
  flex-shrink: 0;
}

.info-item .value {
  text-align: right;
  color: #1f2937;
}

.total-row {
  padding-top: 0.75rem;
  border-top: 1px solid #e5e7eb;
  font-weight: 600;
}

.total-price {
  font-size: 1.25rem;
  color: #3b82f6;
}

.status-text.status-delivered {
  color: #16a34a;
}

.status-text.status-processing {
  color: #d97706;
}

.status-text.status-shipping {
  color: #2563eb;
}

.status-text.status-cancelled {
  color: #dc2626;
}

/* Products Section */
.products-section h4,
.combos-section h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
}

.products-table,
.combos-table {
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  overflow: hidden;
}

.table-header {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  background-color: #f9fafb;
  padding: 0.75rem;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
  border-bottom: 1px solid #e5e7eb;
}

.table-header.combo-header {
  grid-template-columns: 3fr 1fr 1fr 1fr;
}

.table-row {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  padding: 1rem 0.75rem;
  border-bottom: 1px solid #f3f4f6;
  align-items: center;
}

.table-row.combo-row {
  grid-template-columns: 3fr 1fr 1fr 1fr;
}

.table-row:last-child {
  border-bottom: none;
}

.col-product,
.col-combo {
  font-size: 0.875rem;
}

.product-info strong,
.combo-info strong {
  color: #1f2937;
  display: block;
  margin-bottom: 0.25rem;
}

.product-variant {
  font-size: 0.75rem;
  color: #6b7280;
}

.combo-items {
  margin-top: 0.5rem;
}

.combo-item {
  font-size: 0.75rem;
  color: #6b7280;
  margin-bottom: 0.25rem;
}

.combo-quantity {
  color: #9ca3af;
}

.col-price {
  text-align: center;
}

.current-price {
  font-weight: 600;
  color: #1f2937;
  display: block;
}

.original-price {
  font-size: 0.75rem;
  color: #ef4444;
  text-decoration: line-through;
}

.col-quantity,
.col-total {
  text-align: center;
  font-weight: 500;
  color: #1f2937;
}

/* Cancel Modal Specific */
.cancel-description {
  color: #6b7280;
  margin-bottom: 1.5rem;
  line-height: 1.5;
}

.reason-options {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.reason-option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  padding: 0.75rem;
  border-radius: 0.5rem;
  transition: background-color 0.2s;
}

.reason-option:hover {
  background-color: #f9fafb;
}

.reason-option input[type='radio'] {
  margin: 0;
  accent-color: #3b82f6;
}

.reason-text {
  color: #374151;
  font-size: 0.875rem;
  line-height: 1.5;
}

.custom-reason {
  margin-top: 1rem;
}

.custom-reason-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  resize: vertical;
  transition: border-color 0.2s;
}

.custom-reason-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Buttons */
.btn {
  padding: 0.625rem 1.25rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-secondary {
  background-color: #f3f4f6;
  color: #374151;
  border-color: #d1d5db;
}

.btn-secondary:hover {
  background-color: #e5e7eb;
  border-color: #9ca3af;
}

.btn-danger {
  background-color: #dc2626;
  color: white;
  border-color: #dc2626;
}

.btn-danger:hover {
  background-color: #b91c1c;
  border-color: #b91c1c;
}

/* Responsive Design */
@media (max-width: 768px) {
  .orders-title {
    font-size: 1.25rem;
  }

  .status-tabs {
    grid-template-columns: repeat(5, 1fr);
  }

  .tab-button {
    padding: 0.5rem 0.25rem;
    font-size: 0.75rem;
  }

  .order-header {
    padding: 1rem 1rem 0.5rem;
  }

  .order-items {
    padding: 0 1rem;
  }

  .order-footer {
    padding: 0.75rem 1rem 1rem;
  }

  .order-actions {
    flex-direction: column;
    gap: 0.5rem;
  }

  .item-image {
    width: 2.5rem;
    height: 2.5rem;
  }

  .order-summary-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .table-header,
  .table-row {
    grid-template-columns: 2fr 1fr 1fr;
    font-size: 0.75rem;
  }

  .col-total {
    display: none;
  }

  .modal-content {
    margin: 1rem;
    max-height: calc(100vh - 2rem);
  }
}

@media (max-width: 640px) {
  .header-content {
    padding: 0 0.75rem;
  }

  .search-filter-section,
  .orders-content {
    padding-left: 0.75rem;
    padding-right: 0.75rem;
  }

  .item-name {
    font-size: 0.75rem;
  }

  .item-variant,
  .item-quantity {
    font-size: 0.625rem;
  }

  .item-price {
    font-size: 0.75rem;
  }
}
</style>