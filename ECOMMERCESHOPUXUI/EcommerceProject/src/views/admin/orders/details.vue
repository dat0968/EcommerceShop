<script setup>
import { ref, onMounted, watch } from 'vue'
import { Modal } from 'bootstrap'

const props = defineProps({
  Order: {
    type: Object,
    required: true,
  },
  isOpenModalDetails: Boolean
})

const emit = defineEmits(['close'])
const isOpenModalDetails = ref(false)
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
watch(() => props.isOpenModalDetails, (newValue) => {
  isOpenModalDetails.value = newValue
})
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

const closeDetails = () => {
  emit('close')
}



const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount || 0)
}

const formatDate = (dateString) => {
  if (!dateString) return 'Chưa có'
  return new Date(dateString).toLocaleString('vi-VN')
}

const statusSteps = [
  { label: 'Chờ xác nhận', key: 'Chờ xác nhận', color: 'bg-warning' },
  { label: 'Đã xác nhận', key: 'Đã xác nhận', color: 'bg-success' },
  {
    label: 'Đã giao cho đơn vị vận chuyển',
    key: 'Đã giao cho đơn vị vận chuyển',
    color: 'bg-info',
  },
  { label: 'Đã nhận', key: 'Đã nhận', color: 'bg-primary' },
  { label: 'Đã thanh toán', key: 'Đã thanh toán', color: 'bg-dark' },
  { label: 'Đã hủy', key: 'Đã hủy', color: 'bg-danger' },
  { label: 'Hoàn trả/Hoàn tiền', key: 'Hoàn trả/Hoàn tiền', color: 'bg-secondary' },
]
const getStatusIndex = (status) => {
  const safeStatus = typeof status === 'string' ? status.toLowerCase() : ''
  return statusSteps.findIndex((step) => step.key.toLowerCase() === safeStatus)
}

const isStatusActive = (step) => {
  const currentIndex = getStatusIndex(order.value.tinhTrang)
  const stepIndex = statusSteps.findIndex((s) => s.key === step.key)
  return stepIndex <= currentIndex
}
</script>

<template>
  <div
    class="modal fade show custom-modal-overlay"
    v-if="isOpenModalDetails"
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
            @click="closeDetails"
          ></button>
        </div>
        <div class="modal-body">
          <!-- Trạng thái đơn hàng -->
          <div class="status-timeline mb-4">
            <h6 class="fw-bold mb-3 text-primary">Trạng thái đơn hàng</h6>
            <div class="d-flex justify-content-between position-relative">
              <div
                v-for="(step, index) in statusSteps"
                :key="step.key"
                class="text-center flex-fill"
              >
                <div
                  :class="[
                    'status-circle',
                    isStatusActive(step) ? step.color : 'bg-light',
                    isStatusActive(step) ? 'text-white' : 'text-muted',
                  ]"
                >
                  {{ index + 1 }}
                </div>
                <div :class="['mt-2', isStatusActive(step) ? 'fw-bold' : 'text-muted']">
                  {{ step.label }}
                </div>
              </div>
              <div class="status-line"></div>
            </div>
          </div>

          <!-- Thông tin đơn hàng -->
          <div class="row mb-4" style="font-size: 1rem">
            <div class="col-12">
              <h6 class="fw-bold mb-3 text-primary">Thông tin đơn hàng</h6>
              <div class="table-responsive">
                <table class="table table-bordered">
                  <tbody>
                    <tr>
                      <td class="fw-medium">Mã khách hàng</td>
                      <td>{{ order.maKh }}</td>
                      <td class="fw-medium">Họ tên người đặt</td>
                      <td>{{ order.tenKh }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Họ tên người nhận</td>
                      <td>{{ order.hoTen }}</td>
                      <td class="fw-medium">Số điện thoại</td>
                      <td>{{ order.sdt }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Địa chỉ giao hàng</td>
                      <td>{{ order.diaChiNhanHang }}</td>
                      <td class="fw-medium">Hình thức thanh toán</td>
                      <td>{{ order.hinhThucTt }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Mô tả</td>
                      <td>{{ order.moTa || 'Không có' }}</td>
                      <td class="fw-medium">Lý do hủy</td>
                      <td>{{ order.lyDoHuy || 'Không có' }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Mã nhân viên</td>
                      <td>{{ order.maNv || 'Chưa có' }}</td>
                      <td class="fw-medium">Tên nhân viên</td>
                      <td>{{ order.tenNv || 'Chưa có' }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Ngày tạo</td>
                      <td>{{ formatDate(order.ngayTao) }}</td>
                      <td class="fw-medium">Ngày bắt đầu giao</td>
                      <td>{{ formatDate(order.batDauGiao) }}</td>
                    </tr>
                    <tr>
                      <td class="fw-medium">Ngày nhận</td>
                      <td>{{ formatDate(order.ngayNhan) }}</td>
                      <td class="fw-medium">Ngày thanh toán</td>
                      <td>{{ formatDate(order.ngayThanhToan) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <!-- Danh sách sản phẩm -->
          <div
            class="product-list mb-4"
            v-if="order.cthoadons.some((i) => !i.maCombo)"
            style="font-size: 1rem"
          >
            <h6 class="fw-bold mb-3 text-success">Sản phẩm trong đơn hàng</h6>
            <div class="table-responsive">
              <table class="table table-sm align-middle table-bordered">
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
                      <div class="text-sm text-gray-500 italic">
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
          <div
            class="combo-list mb-4"
            v-if="order.cthoadons.some((i) => i.maCombo)"
            style="font-size: 1rem"
          >
            <h6 class="fw-bold mb-3 text-success">Combo trong đơn hàng</h6>
            <div class="table-responsive">
              <table class="table table-sm align-middle table-bordered">
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
                      <span class="text-danger text-decoration-line-through">
                        {{ formatCurrency(comboItem.giaGoc) }}
                      </span>
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
          <div class="row justify-content-end mt-4" style="font-size: 1.2rem">
            <div class="col-md-4">
              <table class="table table-bordered">
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
                  <td style="color: red">Tổng cộng:</td>
                  <td class="text-end">
                    {{ formatCurrency(order.tienGoc + order.phiVanChuyen - order.giamGiaCoupon) }}
                  </td>
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
  font-size: 0.5rem;
}

.table td,
.table th {
  vertical-align: middle;
}

.status-timeline {
  position: relative;
}

.status-circle {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  font-weight: bold;
}

.status-line {
  position: absolute;
  top: 15px;
  left: 10%;
  right: 10%;
  height: 4px;
  background: linear-gradient(to right, #007bff 50%, #e9ecef 50%);
  z-index: -1;
}
.custom-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  z-index: 1050;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>