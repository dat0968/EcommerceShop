<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { decodeToken, validateToken } from '@/utils/auth'
import { useRoute, useRouter } from 'vue-router'
const getUrlAPI = ref(GetApiUrl())
const orders = ref([])
const showModal = ref(false)
const selectedOrder = ref(null)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const router = useRouter()
const showCancelModal = ref(false)
const cancelReason = ref('')
const orderToCancel = ref(null)
const page = ref(1)
const totalPage = ref(1)
const search = ref('')
const loading = ref(false)
const filter = ref('')
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
function openModal(order) {
  selectedOrder.value = order
  showModal.value = true
}
function closeModal() {
  showModal.value = false
  selectedOrder.value = null
}
function formatCurrency(val) {
  return val?.toLocaleString('vi-VN') + ' ₫'
}
function formatDate(dateStr) {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString('vi-VN')
}
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

onMounted(async () => {
  await fetchAPI()
})

async function cancelOrder(order) {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (!validatetoken.isValid) {
    router.push('/Login')
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

  if (!confirmResult.isConfirmed) {
    return
  }
  const content = {
    id: order.maHd,
    selectedCancelStatus:
      order.tinhTrang.toLowerCase() === 'chờ xác nhận' ? 'Đã hủy' : 'Hoàn trả/Hoàn tiền',
    reasonCancel: cancelReason.value,
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

function openCancelModal(order) {
  orderToCancel.value = order
  cancelReason.value = ''
  showCancelModal.value = true
}
function closeCancelModal() {
  showCancelModal.value = false
  orderToCancel.value = null
  cancelReason.value = ''
}
async function confirmCancelOrder() {
  if (!cancelReason.value.trim()) {
    Swal.fire('Vui lòng nhập lý do hủy/hoàn trả!', '', 'warning')
    return
  }
  await cancelOrder({ ...orderToCancel.value, reasonCancel: cancelReason.value })
  closeCancelModal()
}

function handleSearch() {
  fetchAPI()
}
function handleFilter() {
  fetchAPI()
}
function changePage(p) {
  if (p != page.value) {
    page.value = p
    fetchAPI()
  }
}
</script>
<template>
  <div class="container py-4">
    <div class="d-flex gap-2 mb-3">
      <input
        v-model="search"
        @input="handleSearch"
        class="form-control"
        style="max-width: 250px"
        placeholder="Tìm kiếm đơn hàng..."
      />
      <select v-model="filter" @change="handleFilter" class="form-select" style="max-width: 220px">
        <option value="">Tất cả trạng thái</option>
        <option v-for="status in statusOptions" :key="status" :value="status">{{ status }}</option>
      </select>
    </div>
    <div v-if="loading" class="my-loading-spinner">
      <div class="lds-dual-ring"></div>
      <div class="fw-semibold text-primary mt-2">Đang tải dữ liệu...</div>
    </div>
    <div v-else>
      <!-- Table danh sách đơn hàng như cũ -->
      <h2 class="mb-4 text-center">Đơn hàng cá nhân</h2>
      <div class="table-responsive">
        <table class="table table-bordered align-middle">
          <thead class="table-light">
            <tr>
              <th>Mã đơn hàng</th>
              <th>Ngày tạo</th>
              <th>Trạng thái</th>
              <th>Người đặt</th>
              <th>SĐT</th>
              <th>Địa chỉ nhận</th>
              <th>Tổng tiền</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="order in orders" :key="order.maHd">
              <td>{{ order.maHd }}</td>
              <td>{{ formatDate(order.ngayTao) }}</td>
              <td>
                <span class="badge bg-info text-dark">{{ order.tinhTrang }}</span>
              </td>
              <td>{{ order.tenKh }}</td>
              <td>{{ order.sdt }}</td>
              <td>{{ order.diaChiNhanHang }}</td>
              <td>
                {{ formatCurrency(order.tienGoc + order.phiVanChuyen - order.giamGiaCoupon) }}
              </td>
              <td>
                <div class="d-flex justify-content-center align-items-center gap-2 flex-wrap">
                  <button class="btn btn-sm btn-primary" @click="openModal(order)">
                    Xem chi tiết
                  </button>
                  <button
                    v-if="
                      order.tinhTrang.toLowerCase() == 'chờ xác nhận' ||
                      (order.tinhTrang.toLowerCase() == 'Đã thanh toán' &&
                        Date.now - order.ngayThanhToan <= 3)
                    "
                    class="btn btn-sm btn-danger"
                    @click="openCancelModal(order)"
                  >
                    Hủy/Hoàn trả
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Modal chi tiết đơn hàng -->
      <div
        v-if="showModal && selectedOrder"
        class="modal fade show"
        style="display: block; background: rgba(0, 0, 0, 0.4)"
      >
        <div class="modal-dialog modal-lg modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Chi tiết đơn hàng: {{ selectedOrder.maHd }}</h5>
              <button type="button" class="btn-close" @click="closeModal"></button>
            </div>
            <div class="modal-body">
              <div class="row mb-2">
                <div class="col-md-6">
                  <div><strong>Ngày tạo:</strong> {{ formatDate(selectedOrder.ngayTao) }}</div>
                  <div>
                    <strong>Ngày thanh toán:</strong>
                    {{
                      selectedOrder.ngayThanhToan
                        ? formatDate(selectedOrder.ngayThanhToan)
                        : 'Chưa xác nhận'
                    }}
                  </div>
                  <div><strong>Người nhận:</strong> {{ selectedOrder.hoTen }}</div>
                  <div>
                    <strong>Người đặt:</strong> {{ selectedOrder.maKh }} - {{ selectedOrder.tenKh }}
                  </div>
                  <div>
                    <strong>Nhân viên phụ trách:</strong>
                    {{
                      selectedOrder.maNv && selectedOrder.maNv
                        ? selectedOrder.maNv + ' - ' + selectedOrder.tenNv
                        : 'Chưa xác nhận'
                    }}
                  </div>
                  <div><strong>SĐT:</strong> {{ selectedOrder.sdt }}</div>
                  <div><strong>Địa chỉ nhận:</strong> {{ selectedOrder.diaChiNhanHang }}</div>
                  <div><strong>Mô tả:</strong> {{ selectedOrder.moTa || '---' }}</div>
                </div>
                <div class="col-md-6">
                  <div><strong>Hình thức thanh toán:</strong> {{ selectedOrder.hinhThucTt }}</div>
                  <div><strong>Tình trạng:</strong> {{ selectedOrder.tinhTrang }}</div>
                  <div>
                    <strong>Phí vận chuyển:</strong>
                    {{ formatCurrency(selectedOrder.phiVanChuyen) }}
                  </div>
                  <div>
                    <strong>Tổng tiền hàng:</strong> {{ formatCurrency(selectedOrder.tienGoc) }}
                  </div>
                  <div>
                    <strong>Giảm giá coupon:</strong> -
                    {{ formatCurrency(selectedOrder.giamGiaCoupon) }}
                  </div>
                  <div>
                    <strong>Tổng thanh toán:</strong>
                    {{
                      formatCurrency(
                        selectedOrder.tienGoc -
                          selectedOrder.giamGiaCoupon +
                          selectedOrder.phiVanChuyen
                      )
                    }}
                  </div>
                  <div
                    v-if="
                      selectedOrder.tinhTrang.toLowerCase == 'đã hủy' ||
                      selectedOrder.tinhTrang.toLowerCase() == 'hoàn trả/hoàn tiền'
                    "
                  >
                    <strong>Lý do hủy:</strong>
                    {{ selectedOrder.lyDoHuy }}
                  </div>
                </div>
              </div>
              <div class="mb-3">
                <strong>DANH SÁCH SẢN PHẨM ĐƠN LẺ:</strong>
                <table class="table table-bordered table-sm mt-2">
                  <thead class="table-light">
                    <tr>
                      <th>#</th>
                      <th>Tên sản phẩm</th>
                      <th>Đơn giá/Sản phẩm</th>
                      <th>Số lượng mua</th>
                      <th>Thành tiền</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="(item, idx) in selectedOrder.cthoadons.filter((i) => !i.maCombo)"
                      :key="'product-' + idx"
                    >
                      <td>{{ idx + 1 }}</td>
                      <td>
                        {{ item.tenSanPham }} <br />
                        <span style="color: gray">({{ item.bienThe }})</span>
                      </td>
                      <td>
                        {{ formatCurrency(item.gia) }}
                        <span
                          v-if="item.gia != item.giaGoc"
                          style="text-decoration-line: line-through; color: red"
                          >{{ formatCurrency(item.giaGoc) }}</span
                        >
                      </td>
                      <td>{{ item.soLuong }}</td>
                      <td>{{ formatCurrency(item.gia * item.soLuong) }}</td>
                    </tr>
                    <tr v-if="selectedOrder.cthoadons.filter((i) => !i.maCombo).length === 0">
                      <td colspan="5" class="text-center text-muted">Không có sản phẩm đơn lẻ</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="mb-3" v-if="selectedOrder.cthoadons.some((i) => i.maCombo)">
                <strong>DANH SÁCH COMBO:</strong>
                <table class="table table-bordered table-sm mt-2">
                  <thead class="table-light">
                    <tr>
                      <th>#</th>
                      <th>Tên combo</th>
                      <th>Đơn giá/Combo</th>
                      <th>Số lượng mua</th>
                      <th>Mô tả</th>
                      <th>Thành tiền</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="(comboItem, cidx) in selectedOrder.cthoadons.filter((i) => i.maCombo)"
                      :key="'combo-' + cidx"
                    >
                      <td>{{ cidx + 1 }}</td>
                      <td>{{ comboItem.tenCombo }}</td>
                      <td>
                        {{ formatCurrency(comboItem.gia) }}
                        <span style="text-decoration-line: line-through; color: red">{{
                          formatCurrency(comboItem.giaGoc)
                        }}</span>
                      </td>
                      <td>{{ comboItem.soLuong }}</td>
                      <td>
                        <div class="mb-0">
                          <li
                            v-for="combo in selectedOrder.chitietcombohoadons.filter(
                              (c) => c.maCombo === comboItem.maCombo
                            )"
                            :key="combo.maCtsp"
                          >
                            {{ combo.tenSanPham }} ({{ combo.mauSac }}-{{ combo.kichThuoc }}) <br />
                            <span style="color: gray"
                              >({{ combo.soLuong / comboItem.soLuong }} sản phẩm)</span
                            >
                          </li>
                        </div>
                      </td>
                      <td>
                        {{ formatCurrency(comboItem.gia * comboItem.soLuong) }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div v-if="showModal" class="modal-backdrop fade show"></div>

      <!-- Modal nhập lý do hủy/hoàn trả -->
      <div
        class="modal fade"
        :class="{ show: showCancelModal }"
        tabindex="-1"
        :style="{ display: showCancelModal ? 'block' : 'none', background: 'rgba(0,0,0,0.5)' }"
        v-if="showCancelModal"
      >
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Nhập lý do hủy/hoàn trả</h5>
              <button type="button" class="btn-close" @click="closeCancelModal"></button>
            </div>
            <div class="modal-body">
              <textarea
                v-model="cancelReason"
                class="form-control"
                rows="4"
                placeholder="Nhập lý do..."
              ></textarea>
            </div>
            <div class="modal-footer">
              <button class="btn btn-secondary" @click="closeCancelModal">Đóng</button>
              <button class="btn btn-danger" @click="confirmCancelOrder">Xác nhận</button>
            </div>
          </div>
        </div>
      </div>
      <!-- ... -->
      <nav v-if="totalPage > 0" class="mt-3">
        <ul class="pagination justify-content-center">
          <li class="page-item" :class="{ disabled: page === 1 }">
            <a class="page-link" href="#" @click="changePage(page - 1)">Trước</a>
          </li>
          <li v-for="p in totalPage" :key="p" class="page-item" :class="{ active: p === page }">
            <a class="page-link" href="#" @click="changePage(p)">{{ p }}</a>
          </li>
          <li class="page-item" :class="{ disabled: page === totalPage }">
            <a class="page-link" href="#" @click="changePage(page + 1)">Sau</a>
          </li>
        </ul>
      </nav>
    </div>
  </div>
</template>

<style scoped>
.table {
  margin-bottom: 0;
}
.modal {
  z-index: 1050;
}
.modal-backdrop {
  z-index: 1040;
}
.my-loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  min-height: 120px;
  justify-content: center;
}
.lds-dual-ring {
  display: inline-block;
  width: 48px;
  height: 48px;
}
.lds-dual-ring:after {
  content: ' ';
  display: block;
  width: 38px;
  height: 38px;
  margin: 5px;
  border-radius: 50%;
  border: 5px solid #0d6efd;
  border-color: #0d6efd transparent #0d6efd transparent;
  animation: lds-dual-ring 1.2s linear infinite;
}
@keyframes lds-dual-ring {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
.table td .btn {
  min-width: 110px;
  font-size: 0.95rem;
  padding: 6px 12px;
}
.table td .btn + .btn {
  margin-left: 0;
}
.table td .d-flex {
  gap: 10px;
}
</style>