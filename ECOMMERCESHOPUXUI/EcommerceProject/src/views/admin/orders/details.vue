<script setup>
import { ref, onMounted, watch } from 'vue'
import { Modal } from 'bootstrap'

const props = defineProps({
  Order: {
    type: Object,
    required: true,
  },
})

const emit = defineEmits(['close'])

const order = ref({
  maHd: props.Order.maHd,
  maKh: props.Order.maKh,
  tenKh: props.Order.tenKh,
  maNv: props.Order.maNv,
  tenNv: props.Order.tenNv,
  maCode: props.Order.maCode,
  ngayTao: props.Order.ngayTao,
  batDauGiao: props.Order.batDauGiao,
  ngayNhan: props.Order.ngayNhan,
  diaChiNhanHang: props.Order.diaChiNhanHang,
  ngayThanhToan: props.Order.ngayThanhToan,
  hinhThucTt: props.Order.hinhThucTt,
  tinhTrang: props.Order.tinhTrang,
  moTa: props.Order.moTa,
  hoTen: props.Order.hoTen,
  sdt: props.Order.sdt,
  lyDoHuy: props.Order.lyDoHuy,
  phiVanChuyen: props.Order.phiVanChuyen,
  tienGoc: props.Order.tienGoc,
  giamGiaCoupon: props.Order.giamGiaCoupon || 0,
  chitietcombohoadons: props.Order.chitietcombohoadons ? [...props.Order.chitietcombohoadons] : [],
  cthoadons: props.Order.cthoadons ? [...props.Order.cthoadons] : [],
})
console.log(order.value)
// Watch để cập nhật dữ liệu khi props thay đổi
watch(
  () => props.Order,
  (newOrder) => {
    order.value = {
      maHd: newOrder.maHd,
      maKh: newOrder.maKh,
      tenKh: newOrder.tenKh,
      maNv: newOrder.maNv,
      tenNv: newOrder.tenNv,
      maCode: newOrder.maCode,
      ngayTao: newOrder.ngayTao,
      batDauGiao: newOrder.batDauGiao,
      ngayNhan: newOrder.ngayNhan,
      diaChiNhanHang: newOrder.diaChiNhanHang,
      ngayThanhToan: newOrder.ngayThanhToan,
      hinhThucTt: newOrder.hinhThucTt,
      tinhTrang: newOrder.tinhTrang,
      moTa: newOrder.moTa,
      hoTen: newOrder.hoTen,
      sdt: newOrder.sdt,
      lyDoHuy: newOrder.lyDoHuy,
      phiVanChuyen: newOrder.phiVanChuyen,
      tienGoc: newOrder.tienGoc,
      giamGiaCoupon: newOrder.giamGiaCoupon || 0,
      chitietcombohoadons: newOrder.chitietcombohoadons ? [...newOrder.chitietcombohoadons] : [],
      cthoadons: newOrder.cthoadons ? [...newOrder.cthoadons] : [],
    }
  },
  { deep: true }
)

const modalInstance = ref(null)

const closeDetails = () => {
  if (modalInstance.value) {
    modalInstance.value.hide()
  }
  emit('close')
}

onMounted(() => {
  const modalElement = document.getElementById(`orderDetailModal_${order.value.maHd}`)
  if (modalElement) {
    modalInstance.value = new Modal(modalElement)
  }
})

// Format số tiền
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount || 0)
}

// Format ngày tháng
const formatDate = (dateString) => {
  if (!dateString) return 'Chưa có'
  return new Date(dateString).toLocaleString('vi-VN')
}
</script>

<template>
  <div
    class="modal fade"
    :id="`orderDetailModal_${order.maHd}`"
    tabindex="-1"
    aria-labelledby="orderDetailModalLabel"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-xl">
      <div class="modal-content fs-5">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title fw-bold">Chi tiết đơn hàng #{{ order.maHd }}</h5>
          <button
            type="button"
            class="btn-close btn-close-white"
            data-bs-dismiss="modal"
            aria-label="Close"
            @click="closeDetails"
          ></button>
        </div>
        <div class="modal-body">
          <!-- Thông tin khách hàng -->
          <div class="row mb-4">
            <div class="col-md-6 border-end">
              <h6 class="fw-bold mb-2 text-primary">Thông tin khách hàng</h6>
              <p class="mb-1"><strong>Mã khách hàng:</strong> {{ order.maKh }}</p>
              <p class="mb-1">
                <strong>Họ tên người đặt:</strong> {{ order.maKh }} - {{ order.tenKh }}
              </p>
              <p class="mb-1"><strong>Họ tên người nhận:</strong> {{ order.hoTen }}</p>
              <p class="mb-1"><strong>Số điện thoại:</strong> {{ order.sdt }}</p>
            </div>
            <div class="col-md-6 ps-md-4">
              <h6 class="fw-bold mb-2 text-primary">Thông tin đơn hàng</h6>
              <p class="mb-1"><strong>Địa chỉ giao hàng:</strong> {{ order.diaChiNhanHang }}</p>
              <p class="mb-1"><strong>Hình thức thanh toán:</strong> {{ order.hinhThucTt }}</p>
              <p class="mb-1"><strong>Tình trạng:</strong> {{ order.tinhTrang }}</p>
              <p class="mb-1"><strong>Mô tả:</strong> {{ order.moTa || 'Không có' }}</p>
              <p class="mb-1"><strong>Lý do hủy:</strong> {{ order.lyDoHuy || 'Không có' }}</p>
            </div>
          </div>

          <!-- Thông tin nhân viên và thời gian -->
          <div class="row mb-4">
            <div class="col-md-6 border-end">
              <h6 class="fw-bold mb-2 text-primary">Thông tin nhân viên</h6>
              <p class="mb-1"><strong>Mã nhân viên:</strong> {{ order.maNv || 'Chưa có' }}</p>
              <p class="mb-1"><strong>Tên nhân viên:</strong> {{ order.tenNv }}</p>
            </div>
            <div class="col-md-6 ps-md-4">
              <h6 class="fw-bold mb-2 text-primary">Thời gian</h6>
              <p class="mb-1"><strong>Ngày tạo:</strong> {{ formatDate(order.ngayTao) }}</p>
              <p class="mb-1">
                <strong>Ngày bắt đầu giao:</strong> {{ formatDate(order.batDauGiao) }}
              </p>
              <p class="mb-1"><strong>Ngày nhận:</strong> {{ formatDate(order.ngayNhan) }}</p>
              <p class="mb-1">
                <strong>Ngày thanh toán:</strong> {{ formatDate(order.ngayThanhToan) }}
              </p>
            </div>
          </div>

          <!-- Danh sách sản phẩm -->
          <div class="product-list mb-4" v-if="order.cthoadons.some((i) => !i.maCombo)">
            <h6 class="fw-bold mb-3 text-success">Sản phẩm trong đơn hàng</h6>
            <div class="table-responsive">
              <table class="table table-sm align-middle">
                <thead class="table-light">
                  <tr>
                    <th>STT</th>
                    <th>Tên sản phẩm</th>
                    <th>Đơn giá</th>
                    <th>Số lượng</th>
                    <th>Giảm giá</th>
                    <th>Tổng tiền</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(item, index) in order.cthoadons.filter((p) => !p.maCombo)"
                    :key="index"
                  >
                    <td>{{ index + 1 }}</td>
                    <td>
                      <div class="font-semibold text-gray-800">
                        {{ item.tenSanPham || 'Không có tên' }}
                      </div>
                      <div
                        style="
                          color: #6b7280;
                          font-size: 0.875rem;
                          font-style: italic;
                          margin-left: 4px;
                        "
                        class="text-sm text-gray-500 italic"
                      >
                        {{ item.bienThe }}
                      </div>
                    </td>
                    <td>{{ formatCurrency(item.gia) }}</td>
                    <td>{{ item.soLuong }}</td>
                    <td>
                      <span class="text-danger"> - {{ formatCurrency(item.giamGia || 0) }} </span>
                    </td>
                    <td>{{ formatCurrency(item.gia * item.soLuong - (item.giamGia || 0)) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Danh sách combo -->
          <div class="combo-list mb-4" v-if="order.cthoadons.some((i) => i.maCombo)">
            <h6 class="fw-bold mb-3 text-success">Combo trong đơn hàng</h6>
            <div class="table-responsive">
              <table class="table table-sm align-middle">
                <thead class="table-light">
                  <tr>
                    <th>STT</th>
                    <th>Tên combo</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Thành tiền</th>
                    <th>Chi tiết sản phẩm</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="(comboItem, cidx) in order.cthoadons.filter((i) => i.maCombo)"
                    :key="'combo-' + cidx"
                  >
                    <td>{{ cidx + 1 }}</td>
                    <td>{{ comboItem.tenCombo }}</td>
                    <td>{{ comboItem.soLuong }}</td>
                    <td>
                      {{ formatCurrency(comboItem.gia) }}
                      <span style="text-decoration-line: line-through; color: red">{{
                        formatCurrency(comboItem.giaGoc)
                      }}</span>
                    </td>
                    <td>{{ formatCurrency(comboItem.gia * comboItem.soLuong) }}</td>
                    <td>
                      <ul class="list-unstyled mb-0">
                        <li
                          v-for="(detail, idx) in order.chitietcombohoadons.filter(
                            (c) => c.maCombo === comboItem.maCombo
                          )"
                          :key="idx"
                          class="mb-2"
                        >
                          <div>
                            <strong>Tên SP:</strong> {{ detail.tenSanPham }}
                            <span v-if="detail.mauSac || detail.kichThuoc">
                              ({{ detail.kichThuoc }} {{ '- ' + detail.mauSac }})</span
                            >
                            - <strong>Số lượng:</strong> {{ detail.soLuong }}
                          </div>
                        </li>
                      </ul>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Tổng tiền -->
          <div class="row justify-content-end mt-4">
            <div class="col-md-4">
              <table class="table">
                <tr>
                  <td class="fw-medium">Tạm tính:</td>
                  <td class="text-end">{{ formatCurrency(order.tienGoc) }}</td>
                </tr>
                <tr>
                  <td class="fw-medium">Phí vận chuyển:</td>
                  <td class="text-end">{{ formatCurrency(order.phiVanChuyen) }}</td>
                </tr>
                <tr v-if="order.giamGiaCoupon > 0">
                  <td class="fw-medium text-danger">Giảm giá coupon:</td>
                  <td class="text-end text-danger">-{{ formatCurrency(order.giamGiaCoupon) }}</td>
                </tr>
                <tr class="fw-bold text-primary">
                  <td>Tổng cộng:</td>
                  <td class="text-end">{{ formatCurrency(order.tienGoc + order.phiVanChuyen - order.giamGiaCoupon) }}</td>
                </tr>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-xl {
  max-width: 90%;
}

.modal-content {
  font-size: 1.1rem; /* Tăng kích thước chữ toàn bộ modal */
}

.table td,
.table th {
  vertical-align: middle;
}
</style>

