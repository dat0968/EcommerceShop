<script setup>
import { ref, onMounted, watch } from 'vue'
import Swal from 'sweetalert2'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import { useRouter } from 'vue-router'
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const readToken = ref({})
const roleUser = ref('')
const router = useRouter()
onMounted(async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/LoginStaff')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  readToken.value = decodeToken(accessToken.value)
  roleUser.value = readToken.value.Role
})

// Hàm kiểm tra và cập nhật trạng thái coupon hết hạn
const checkAndUpdateExpiredCoupons = async (couponsList) => {
  const today = new Date()
  today.setHours(0, 0, 0, 0) // Đặt thời gian về 00:00:00 để so sánh ngày
  for (const coupon of couponsList) {
    const endDate = new Date(coupon.ngayKetThuc)
    if (coupon.trangThai === true && endDate < today && !isCouponUsedUp(coupon)) {
      // Coupon còn hiệu lực nhưng đã hết hạn
      try {
        const response = await fetch(`${baseUrl}/Update`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            ...coupon,
            trangThai: 'expired', // Chuyển trạng thái sang Hết hiệu lực
            ngayBatDau: coupon.ngayBatDau ? new Date(coupon.ngayBatDau).toISOString() : null,
            ngayKetThuc: coupon.ngayKetThuc ? new Date(coupon.ngayKetThuc).toISOString() : null,
            donHangToiThieu: coupon.donHangToiThieu || null,
            soTienGiam: coupon.soTienGiam || null,
            phanTramGiam: coupon.phanTramGiam || null,
          }),
        })

        const data = response.ok
          ? await response.json()
          : { success: false, message: `HTTP ${response.status}: ${await response.text()}` }
        if (!data.success) {
          console.error(`Không thể cập nhật trạng thái coupon ${coupon.maCode}: ${data.message}`)
        }
      } catch (error) {
        console.error(`Lỗi khi cập nhật trạng thái coupon ${coupon.maCode}: ${error.message}`)
      }
    }
  }
}

const getApiUrl = ref(GetApiUrl())
const coupons = ref([])
const customers = ref([])
const showAddModal = ref(false)
const showEditModal = ref(false)
const isEdit = ref(false)
const couponForm = ref({
  maCode: '',
  maKhachHang: '',
  hoTen: '',
  moTa: '',
  soTienGiam: null,
  phanTramGiam: null,
  ngayBatDau: '',
  ngayKetThuc: '',
  trangThai: true,
  donHangToiThieu: null,
  soLuong: null,
  soLuongDaDung: 0,
})

// Bộ lọc
const searchQuery = ref('')
const sortOrder = ref('asc')
const filterStatus = ref('all')
const currentPage = ref(1)
const itemsPerPage = ref(5)
const totalItems = ref(0)
const totalPages = ref(1)

const baseUrl = `${getApiUrl.value}/api/Coupon`

// Track discount type for add/edit modal (amount or percentage)
const discountType = ref('amount') // 'amount' for soTienGiam, 'percent' for phanTramGiam

// Reset discount fields based on discountType
const resetDiscountFields = () => {
  if (discountType.value === 'amount') {
    couponForm.value.phanTramGiam = null
  } else {
    couponForm.value.soTienGiam = null
  }
}

// Watch discountType to reset fields
watch(discountType, () => {
  resetDiscountFields()
})

// Validate form
const validateForm = () => {
  if (!couponForm.value.moTa) {
    Swal.fire('Lỗi', 'Vui lòng nhập mô tả', 'error')
    return false
  }
  if (!couponForm.value.ngayBatDau) {
    Swal.fire('Lỗi', 'Vui lòng chọn ngày bắt đầu', 'error')
    return false
  }
  if (!couponForm.value.ngayKetThuc) {
    Swal.fire('Lỗi', 'Vui lòng chọn ngày kết thúc', 'error')
    return false
  }
  if (!couponForm.value.soLuong || couponForm.value.soLuong < 1) {
    Swal.fire('Lỗi', 'Vui lòng nhập số lượng', 'error')
    return false
  }
  if (couponForm.value.donHangToiThieu === null || couponForm.value.donHangToiThieu < 0) {
    Swal.fire('Lỗi', 'Vui lòng nhập giá trị đơn hàng tối thiểu', 'error')
    return false
  }
  if (
    discountType.value === 'amount' &&
    (couponForm.value.soTienGiam === null || couponForm.value.soTienGiam < 0)
  ) {
    Swal.fire('Lỗi', 'Vui lòng nhập số tiền giảm', 'error')
    return false
  }
  if (
    discountType.value === 'percent' &&
    (couponForm.value.phanTramGiam === null ||
      couponForm.value.phanTramGiam < 0 ||
      couponForm.value.phanTramGiam > 50)
  ) {
    Swal.fire('Lỗi', 'Phần trăm giảm không được vượt quá 50%', 'error')
    return false
  }
  // Kiểm tra số tiền giảm không vượt quá 60% giá trị đơn hàng tối thiểu
  if (
    discountType.value === 'amount' &&
    couponForm.value.soTienGiam > 0.5 * couponForm.value.donHangToiThieu
  ) {
    Swal.fire('Lỗi', 'Số tiền giảm không được vượt quá 50% giá trị đơn hàng tối thiểu', 'error')
    return false
  }
  const startDate = new Date(couponForm.value.ngayBatDau)
  const endDate = new Date(couponForm.value.ngayKetThuc)
  if (startDate > endDate) {
    Swal.fire('Lỗi', 'Ngày bắt đầu không được lớn hơn ngày kết thúc', 'error')
    return false
  }

  return true
}

// Fetch all customers for dropdown
const fetchCustomers = async () => {
  try {
    const response = await fetch(`${getApiUrl.value}/api/C/GetAll`)
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    const data = await response.json()
    if (data.success) {
      customers.value = data.data
    } else {
      console.error('Unable to fetch customers:', data.message)
    }
  } catch (error) {
    console.error('Error fetching customers:', error)
  }
}
// Fetch coupons with pagination
const fetchCoupons = async () => {
  try {
    const params = new URLSearchParams({
      keywords: searchQuery.value,
      status:
        filterStatus.value === 'all'
          ? ''
          : filterStatus.value === 'active'
          ? 'Còn hiệu lực'
          : filterStatus.value === 'noactive'
          ? 'Đã hết hạn'
          : filterStatus.value === 'noactives'
          ? 'Đã hết'
          : 'Đã hủy',
      sort: sortOrder.value,
      page: currentPage.value,
      pageSize: itemsPerPage.value,
    })
    const response = await fetch(`${baseUrl}/GetAllCouponCodeByPage?${params}`)
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`)
    }
    const data = await response.json()
    if (data.success) {
      coupons.value = data.data.map((coupon) => ({
        ...coupon,
        ngayBatDau: coupon.ngayBatDau
          ? new Date(coupon.ngayBatDau).toISOString().split('T')[0]
          : '',
        ngayKetThuc: coupon.ngayKetThuc
          ? new Date(coupon.ngayKetThuc).toISOString().split('T')[0]
          : '',
        hoTen: getCustomerName(coupon.maKhachHang),
      }))

      // Kiểm tra và cập nhật trạng thái coupon hết hạn
      await checkAndUpdateExpiredCoupons(coupons.value)

      totalItems.value = data.totalItems
      totalPages.value = data.totalPages
    } else {
      Swal.fire('Lỗi', data.message || 'Không thể tải danh sách coupon', 'error')
    }
  } catch (error) {
    Swal.fire('Lỗi', `Không thể tải danh sách coupon: ${error.message}`, 'error')
    console.error('Fetch error:', error)
  }
}

// Get customer name by maKhachHang
const getCustomerName = (maKhachHang) => {
  if (!maKhachHang) return ''
  const customer = customers.value.find((c) => c.maKhachHang === maKhachHang)
  return customer ? customer.hoTen : ''
}

// Format date
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return isNaN(date.getTime()) ? '' : date.toLocaleDateString('vi-VN')
}

// Check if coupon is used up
const isCouponUsedUp = (coupon) => {
  return coupon.soLuongDaDung >= coupon.soLuong
}

// Get coupon status text
const getCouponStatus = (coupon) => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const endDate = new Date(coupon.ngayKetThuc)

  if (isCouponUsedUp(coupon)) {
    return { text: 'Đã hết', color: 'red' }
  }
  if (coupon.trangThai === 'expired' || (coupon.trangThai === true && endDate < today)) {
    return { text: 'Đã Hết Hạn', color: 'orange' }
  }
  if (coupon.trangThai === true) {
    return { text: 'Còn hiệu lực', color: 'green' }
  }
  return { text: 'Đã hủy', color: 'gray' }
}

// Modal handlers
const openAddModal = () => {
  isEdit.value = false
  discountType.value = 'amount' // Default to amount-based discount
  couponForm.value = {
    maCode: '',
    moTa: '',
    maKhachHang: '',
    hoTen: '',
    soTienGiam: null,
    phanTramGiam: null,
    ngayBatDau: '',
    ngayKetThuc: '',
    trangThai: true,
    donHangToiThieu: null,
    soLuong: null,
    soLuongDaDung: 0,
  }
  showAddModal.value = true
}

const openEditModal = (coupon) => {
  isEdit.value = true
  couponForm.value = {
    maCode: coupon.maCode,
    maKhachHang: coupon.maKhachHang,
    hoTen: coupon.hoTen,
    moTa: coupon.moTa || '',
    soTienGiam: coupon.soTienGiam,
    phanTramGiam: coupon.phanTramGiam,
    ngayBatDau: coupon.ngayBatDau ? new Date(coupon.ngayBatDau).toISOString().split('T')[0] : '',
    ngayKetThuc: coupon.ngayKetThuc ? new Date(coupon.ngayKetThuc).toISOString().split('T')[0] : '',
    trangThai: coupon.trangThai,
    donHangToiThieu: coupon.donHangToiThieu,
    soLuong: coupon.soLuong,
    soLuongDaDung: coupon.soLuongDaDung || 0,
  }
  // Set discountType based on current coupon values
  discountType.value = coupon.soTienGiam ? 'amount' : coupon.phanTramGiam ? 'percent' : 'amount'
  resetDiscountFields()
  showEditModal.value = true
}

const closeAddModal = () => {
  showAddModal.value = false
}

const closeEditModal = () => {
  showEditModal.value = false
}

// CRUD operations
// CRUD operations
const createCoupon = async () => {
  if (!validateForm()) return

  try {
    const content = {
      ...couponForm.value,

      maKhachHang: couponForm.value.maKhachHang ? parseInt(couponForm.value.maKhachHang) : null, // Chuyển thành số nguyên hoặc null
      ngayBatDau: couponForm.value.ngayBatDau
        ? new Date(couponForm.value.ngayBatDau).toISOString()
        : null,
      ngayKetThuc: couponForm.value.ngayKetThuc
        ? new Date(couponForm.value.ngayKetThuc).toISOString()
        : null,

      donHangToiThieu: couponForm.value.donHangToiThieu || null,
      soLuongDaDung: 0,
    }
    const response = await fetch(`${baseUrl}/Create`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(content),
    })

    const data = response.ok
      ? await response.json()
      : { success: false, message: `HTTP ${response.status}: ${await response.text()}` }
    if (data.success) {
      const newCoupon = {
        ...couponForm.value,
        maCode: data.data?.maCode || couponForm.value.maCode,
        ngayBatDau: couponForm.value.ngayBatDau
          ? new Date(couponForm.value.ngayBatDau).toISOString().split('T')[0]
          : '',
        ngayKetThuc: couponForm.value.ngayKetThuc
          ? new Date(couponForm.value.ngayKetThuc).toISOString().split('T')[0]
          : '',
        soLuongDaDung: 0,
      }
      coupons.value.unshift(newCoupon)
      totalItems.value += 1
      totalPages.value = Math.ceil(totalItems.value / itemsPerPage.value)

      Swal.fire('Thành công', data.message, 'success')
      closeAddModal()
      fetchCoupons()
    } else {
      Swal.fire('Lỗi', data.message || 'Không thể thêm coupon', 'error')
    }
  } catch (error) {
    Swal.fire('Lỗi', `Không thể thêm coupon: ${error.message}`, 'error')
    console.error('Create error:', error)
  }
}

const updateCoupon = async () => {
  if (!validateForm()) return

  try {
    const content = {
      ...couponForm.value,
      maKhachHang: couponForm.value.maKhachHang ? parseInt(couponForm.value.maKhachHang) : null, // Chuyển thành số nguyên hoặc null
      soTienGiam: couponForm.value.soTienGiam || null,
      phanTramGiam: couponForm.value.phanTramGiam || null,
      ngayBatDau: couponForm.value.ngayBatDau
        ? new Date(couponForm.value.ngayBatDau).toISOString()
        : null,
      ngayKetThuc: couponForm.value.ngayKetThuc
        ? new Date(couponForm.value.ngayKetThuc).toISOString()
        : null,
      donHangToiThieu: couponForm.value.donHangToiThieu || null,
    }
    const response = await fetch(`${baseUrl}/Update`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(content),
    })

    const data = response.ok
      ? await response.json()
      : { success: false, message: `HTTP ${response.status}: ${await response.text()}` }
    if (data.success) {
      Swal.fire('Thành công', data.message, 'success')
      closeEditModal()
      fetchCoupons()
    } else {
      Swal.fire('Lỗi', data.message || 'Không thể cập nhật coupon', 'error')
    }
  } catch (error) {
    Swal.fire('Lỗi', `Không thể cập nhật coupon: ${error.message}`, 'error')
    console.error('Update error:', error)
  }
}

const deleteCoupon = async (id) => {
  const result = await Swal.fire({
    title: 'Bạn có chắc?',
    text: 'Bạn muốn hủy coupon này?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Có',
    cancelButtonText: 'Không',
  })

  if (result.isConfirmed) {
    try {
      const response = await fetch(`${baseUrl}/Cancel/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
      })

      const data = response.ok
        ? await response.json()
        : { success: false, message: `HTTP ${response.status}: ${await response.text()}` }
      if (data.success) {
        Swal.fire('Thành công', data.message, 'success')
        fetchCoupons()
      } else {
        Swal.fire('Lỗi', data.message || 'Không thể hủy coupon', 'error')
      }
    } catch (error) {
      Swal.fire('Lỗi', `Không thể hủy coupon: ${error.message}`, 'error')
      console.error('Delete error:', error)
    }
  }
}

// Sort and pagination handlers
const changeSort = () => {
  sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  fetchCoupons()
}

const goToPage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    fetchCoupons()
  }
}

onMounted(() => {
  fetchCoupons()
})
</script>

<template>
  <div>
    <br />
    <br />
    <div class="container mt-4" style="padding-top: 50px; text-align: center">
      <h1>QUẢN LÝ COUPON</h1>
      <br />
      <br />
      <!-- Bộ lọc -->
      <div class="row mb-3" style="background-color: #fff; padding: 20px; border-radius: 20px">
        <div class="col-md-4">
          <input
            v-model="searchQuery"
            type="text"
            class="form-control"
            placeholder="Tìm kiếm mã, số tiền, phần trăm..."
            @input="fetchCoupons"
            style="font-weight: 700; color: #000"
          />
        </div>
        <div class="col-md-3">
          <select
            v-model="filterStatus"
            class="form-control"
            @change="fetchCoupons"
            style="font-weight: 700; color: #000"
          >
            <option value="all">Tất cả trạng thái</option>
            <option value="active">Còn hiệu lực</option>
            <option value="noactive">Đã hết hạn</option>
            <option value="noactives">Đã hết</option>
            <option value="inactive">Đã hủy</option>
          </select>
        </div>
        <!-- <div class="col-md-1">
    <select v-model="itemsPerPage" class="form-control" @change="fetchCoupons" style="font-weight: 700; color: #000;">
      <option value="5">5</option>
      <option value="10">10</option>
      <option value="20">20</option>
      <option value="50">50</option>
    </select>
  </div> -->
        <div class="col-md-3">
          <button
            class="btn btn-primary w-100"
            @click="openAddModal"
            style="font-weight: 700; color: #fff"
            v-if="roleUser.toLowerCase() == 'admin'"
          >
            Thêm mới
          </button>
        </div>
      </div>

      <!-- Bảng coupons -->

      <table class="table table-striped" style="width: 70% !important">
        <thead>
          <tr>
            <th>Mã Coupon</th>
            <th>Mã Khách Hàng</th>
            <th @click="changeSort" class="sortable">
              Số tiền giảm
              <span v-if="sortOrder === 'asc'">↑</span>
              <span v-else>↓</span>
            </th>
            <th @click="changeSort" class="sortable">
              Phần trăm giảm
              <span v-if="sortOrder === 'asc'">↑</span>
              <span v-else>↓</span>
            </th>
            <th>Hạn sử dụng</th>
            <th>Đơn tối thiểu</th>

            <th>Số lượng</th>
            <!-- <th>Đã dùng</th> -->
            <th>Trạng thái</th>
            <th v-if="roleUser.toLowerCase() == 'admin'">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="coupon in coupons" :key="coupon.maCode">
            <td>{{ coupon.maCode }}</td>
            <td>{{ coupon.maKhachHang }}</td>
            <td>{{ coupon.soTienGiam || 0 }}</td>
            <td>{{ coupon.phanTramGiam || 0 }}%</td>
            <td>
              {{ formatDate(coupon.ngayBatDau) }}
              <br />
              -
              <br />
              {{ formatDate(coupon.ngayKetThuc) }}
            </td>
            <td>{{ coupon.donHangToiThieu || 0 }}</td>

            <td>{{ coupon.soLuongDaDung }}/{{ coupon.soLuong }}</td>
            <td :style="{ color: getCouponStatus(coupon).color }">
              {{ getCouponStatus(coupon).text }}
            </td>
            <td v-if="roleUser.toLowerCase() == 'admin'">
              <div
                v-if="
                  !isCouponUsedUp(coupon) &&
                  coupon.trangThai !== 'expired' &&
                  coupon.trangThai !== false
                "
              >
                <button
                  class="btn btn-warning btn-sm mb-2"
                  style="width: 60px"
                  @click="openEditModal(coupon)"
                  
                >
                  Sửa
                </button>
                <button
                  class="btn btn-danger btn-sm"
                  style="width: 60px"
                  @click="deleteCoupon(coupon.maCode)"
                  
                >
                  Hủy
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Phân trang -->
      <div class="d-flex justify-content-between align-items-center">
        <div>
          Hiển thị {{ (currentPage - 1) * itemsPerPage + 1 }} -
          {{ Math.min(currentPage * itemsPerPage, totalItems) }}
          của {{ totalItems }} kết quả
        </div>
        <div>
          <button
            class="btn btn-secondary me-2"
            :disabled="currentPage === 1"
            @click="goToPage(currentPage - 1)"
          >
            Trước
          </button>
          <span>Trang {{ currentPage }} / {{ totalPages }}</span>
          <button
            class="btn btn-secondary ms-2"
            :disabled="currentPage === totalPages"
            @click="goToPage(currentPage + 1)"
          >
            Sau
          </button>
        </div>
      </div>

      <!-- Modal Thêm Coupon -->
      <div class="modal" :class="{ 'd-block': showAddModal }" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header" style="background-color: #4c7cf3">
              <h5 class="modal-title">Thêm Coupon</h5>
              <button type="button" class="btn-close" @click="closeAddModal"></button>
            </div>
            <div class="modal-body">
              <form @submit.prevent="createCoupon()">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Mô tả</label>
                    <input v-model="couponForm.moTa" class="form-control" required />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Loại giảm giá</label>
                    <select v-model="discountType" class="form-control">
                      <option value="amount">Số tiền giảm</option>
                      <option value="percent">Phần trăm giảm</option>
                    </select>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Số tiền giảm</label>
                    <input
                      v-model="couponForm.soTienGiam"
                      type="number"
                      class="form-control"
                      min="0"
                      :disabled="discountType === 'percent'"
                      :required="discountType === 'amount'"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Phần trăm giảm</label>
                    <input
                      v-model="couponForm.phanTramGiam"
                      type="number"
                      class="form-control"
                      min="0"
                      max="100"
                      :disabled="discountType === 'amount'"
                      :required="discountType === 'percent'"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Giá trị đơn hàng tối thiểu</label>
                    <input
                      v-model="couponForm.donHangToiThieu"
                      type="number"
                      class="form-control"
                      min="0"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Hạn sử dụng</label>
                    <input
                      v-model="couponForm.ngayBatDau"
                      type="date"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Ngày kết thúc</label>
                    <input
                      v-model="couponForm.ngayKetThuc"
                      type="date"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Số lượng</label>
                    <input
                      v-model="couponForm.soLuong"
                      type="number"
                      class="form-control"
                      min="1"
                      required
                    />
                  </div>
                </div>
                <button type="submit" class="btn btn-primary">Lưu</button>
              </form>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Sửa Coupon -->
      <div class="modal" :class="{ 'd-block': showEditModal }" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Sửa Coupon</h5>
              <button type="button" class="btn-close" @click="closeEditModal"></button>
            </div>
            <div class="modal-body">
              <form @submit.prevent="updateCoupon()">
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Mã Coupon</label>
                    <input v-model="couponForm.maCode" class="form-control" disabled />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Mô tả</label>
                    <input v-model="couponForm.moTa" class="form-control" />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Loại giảm giá</label>
                    <select v-model="discountType" class="form-control">
                      <option value="amount">Số tiền giảm</option>
                      <option value="percent">Phần trăm giảm</option>
                    </select>
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Số tiền giảm</label>
                    <input
                      v-model="couponForm.soTienGiam"
                      type="number"
                      class="form-control"
                      min="0"
                      :disabled="discountType === 'percent'"
                      :required="discountType === 'amount'"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Phần trăm giảm</label>
                    <input
                      v-model="couponForm.phanTramGiam"
                      type="number"
                      class="form-control"
                      min="0"
                      max="100"
                      :disabled="discountType === 'amount'"
                      :required="discountType === 'percent'"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Giá trị đơn hàng tối thiểu</label>
                    <input
                      v-model="couponForm.donHangToiThieu"
                      type="number"
                      class="form-control"
                      min="0"
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Ngày bắt đầu</label>
                    <input
                      v-model="couponForm.ngayBatDau"
                      type="date"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Ngày kết thúc</label>
                    <input
                      v-model="couponForm.ngayKetThuc"
                      type="date"
                      class="form-control"
                      required
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Số lượng</label>
                    <input
                      v-model="couponForm.soLuong"
                      type="number"
                      class="form-control"
                      min="1"
                      required
                    />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label class="form-label">Trạng thái</label>
                    <select v-model="couponForm.trangThai" class="form-control">
                      <option :value="true">Còn hiệu lực</option>
                      <option :value="false">Đã hủy</option>
                    </select>
                  </div>
                </div>
                <button type="submit" class="btn btn-primary">Lưu</button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<!-- hhhhh12/06/2025 -->
