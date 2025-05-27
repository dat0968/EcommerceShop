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
  chitietcombohoadons: props.Order.chitietcombohoadons ? [...props.Order.chitietcombohoadons] : [],
  cthoadons: props.Order.cthoadons ? [...props.Order.cthoadons] : [],
  hoTenNguoiNhan: props.Order.hoTenNguoiNhan || props.Order.hoTen,
  hoTenNguoiDat: props.Order.hoTenNguoiDat || props.Order.hoTen,
  hoTenNv: props.Order.hoTenNv || 'Chưa có',
  giamGiaCoupon: props.Order.giamGiaCoupon || 0,
  tongtien: props.Order.tongtien || props.Order.tienGoc + props.Order.phiVanChuyen,
})

// Watch để cập nhật dữ liệu khi props thay đổi
watch(
  () => props.Order,
  (newOrder) => {
    order.value = {
      maHd: newOrder.maHd,
      maKh: newOrder.maKh,
      maNv: newOrder.maNv,
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
      chitietcombohoadons: newOrder.chitietcombohoadons ? [...newOrder.chitietcombohoadons] : [],
      cthoadons: newOrder.cthoadons ? [...newOrder.cthoadons] : [],
      hoTenNguoiNhan: newOrder.hoTenNguoiNhan || newOrder.hoTen,
      hoTenNguoiDat: newOrder.hoTenNguoiDat || newOrder.hoTen,
      hoTenNv: newOrder.hoTenNv || 'Chưa có',
      giamGiaCoupon: newOrder.giamGiaCoupon || 0,
      tongtien: newOrder.tongtien || newOrder.tienGoc + newOrder.phiVanChuyen,
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

console.log(order.value)
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
              <p class="mb-1"><strong>Họ tên người đặt:</strong> {{ order.hoTenNguoiDat }}</p>
              <p class="mb-1"><strong>Họ tên người nhận:</strong> {{ order.hoTenNguoiNhan }}</p>
              <p class="mb-1"><strong>Số điện thoại:</strong> {{ order.sdt }}</p>
              <p class="mb-1"><strong>Mã coupon:</strong> {{ order.maCode || 'Không có' }}</p>
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
          <div class="product-list mb-4">
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
                      <div style="color: #6b7280; font-size: 0.875rem; font-style: italic; margin-left: 4px;" class="text-sm text-gray-500 italic">
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
          <!-- <div class="combo-list mb-4">
            <h6 class="fw-bold mb-3 text-success">Combo trong đơn hàng</h6>
            <div class="table-responsive">
              <table class="table table-sm align-middle">
                <thead class="table-light">
                  <tr>
                    <th>STT</th>
                    <th>Tên combo</th>
                    <th>Số lượng</th>
                    <th>Đơn giá</th>
                    <th>Giá gốc</th>
                    <th>Giảm giá</th>
                    <th>Tổng giá</th>
                    <th>Chi tiết sản phẩm</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item, index) in order.chitietcombohoadons" :key="index">
                    <td>{{ index + 1 }}</td>
                    <td>{{ item.tenCombo }}</td>
                    <td>{{ item.soLuong }}</td>
                    <td>{{ formatCurrency(item.gia) }}</td>
                    <td>{{ formatCurrency(item.gia * item.soLuong) }}</td>
                    <td class="text-danger">- {{ formatCurrency(item.giamGia || 0) }}</td>
                    <td>{{ formatCurrency(item.gia * item.soLuong - (item.giamGia || 0)) }}</td>
                    <td>
                      <ul class="list-unstyled mb-0">
                        <li v-for="(detail, idx) in item.chiTietCombo" :key="idx" class="mb-2">
                          <div><strong>Tên SP:</strong> {{ detail.tenSpCombo }}</div>
                          <div v-if="detail.kichThuoc || detail.huongVi">
                            <strong>Biến thể:</strong> <br />
                            <span v-if="detail.kichThuoc">Kích thước: {{ detail.kichThuoc }}</span>
                            <span v-if="detail.kichThuoc && detail.huongVi"> | </span>
                            <span v-if="detail.huongVi">Hương vị: {{ detail.huongVi }}</span>
                          </div>
                          <div><strong>Số lượng:</strong> {{ detail.soLuong }}</div>
                          <div><strong>Đơn giá:</strong> {{ formatCurrency(detail.donGia) }}</div>
                        </li>
                      </ul>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div> -->

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
                  <td class="text-end">{{ formatCurrency(order.tongtien) }}</td>
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

