<template>
  <div>
    <br />
    <br />
    <br />
    <div class="" style="width: 1400px;margin-left: 70px; background-color: aliceblue;">
      <div class="bg-white sticky-header border-b p-4 mb-6">
        <h1 class="mb-0 text-primary modern-title"> <- Chỉnh sửa thông tin</h1>
      </div>

      <div v-if="loading" class="text-center">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Đang tải...</span>
        </div>
      </div>
      
      <div v-else-if="error" class="alert alert-danger modern-alert">
        {{ error }}
      </div>
      
      <div v-else-if="profile" class="row">
        <div class="col-md-3" style="margin-left: 20px;">
          <br>
          <br>
          <div class="modern-card">
            <div class="card-body text-center p-6">
              <div class="relative inline-block mb-4">
                <div class="avatar-container">
                  <br>
          <br>

                  <img
                    v-if="profile.hinhDaiDien"
                    :src="'https://localhost:7139' + profile.hinhDaiDien"
                    alt="Hình đại diện"
                    class="avatar-img mb-3"
                  />
                  <div v-else class="modern-avatar-placeholder">
                    {{ getInitials(profile.hoTen) }}
                  </div>
                </div>
                <p class="text-sm text-muted">Nhấn để thay đổi ảnh đại diện</p>
              </div>
              
            </div>
          </div>
        </div>
        <div class="col-md-8">
                   <br>
          <br>
          <div class="modern-card">
            <div class="card-header p-4">
              <h5 class="card-title mb-0 flex items-center">
                <i class="fas fa-user me-2"></i>
                Thông tin cá nhân
              </h5>
            </div>
            <div class="card-body p-4 space-y-4">
              <div class="info-row">
                <label class="info-label">Họ tên:</label>
                <span class="info-value">{{ profile.hoTen || 'Chưa cập nhật' }}</span>
              </div>
              <div class="info-row">
                <label class="info-label">Giới tính:</label>
                <span class="info-value">{{ profile.gioiTinh || 'Chưa cập nhật' }}</span>
              </div>
              <div class="info-row">
                <label class="info-label flex items-center">
                  <i class="fas fa-calendar me-2"></i>
                  Ngày sinh:
                </label>
                <span class="info-value">{{ formatDate(profile.ngaySinh) }}</span>
              </div>
              <div class="info-row">
                <label class="info-label flex items-center">
                  <i class="fas fa-phone me-2"></i>
                  Số điện thoại:
                </label>
                <span class="info-value">{{ profile.sdt || 'Chưa cập nhật' }}</span>
              </div>
              <div class="info-row">
                <label class="info-label flex items-center">
                  <i class="fas fa-envelope me-2"></i>
                  Email:
                </label>
                <span class="info-value">{{ profile.email || 'Chưa có' }}</span>
              </div>
             
              <div class="info-row">
                <label class="info-label">CCCD:</label>
                <span class="info-value">{{ profile.cccd || 'Chưa cập nhật' }}</span>
              </div>
            </div>
          </div>
          <br>
          <div class="modern-card">
            <div class="card-header p-4">
              <h5 class="card-title mb-0 flex items-center">
                <i class="fas fa-user me-2"></i>
               Địa chỉ
              </h5>
            </div>
            <div class="card-body p-4 space-y-4">
               <div class="info-row">
                <label class="info-label">Địa chỉ:</label>
                <span class="info-value">{{ profile.diaChi || 'Chưa cập nhật' }}</span>
              </div>
            </div>
          </div>
          <br>
          <div class="modern-card">
            <div class="card-header p-4">
              <h5 class="card-title mb-0 flex items-center">
                <i class="fas fa-user me-2"></i>
                Thông tin cá nhân
              </h5>
            </div>
            <div class="card-body p-4 space-y-4">
              <div class="info-row" style="margin-left: 20px;">
                <label class="info-label">Tên tài khoản:</label>
                <span class="info-value" style="margin-right: 20px;">{{ profile.tenTaiKhoan || 'Chưa cập nhật' }}</span>
              </div>
              <div class="info-row" style="margin-left: 20px;">
                <label class="info-label">Trạng thái:</label>
                <span  style="margin-right: 20px;"
                  :class="{ 
                    'status-badge status-active': profile.tinhTrang === 'Đang hoạt động',
                    'status-badge status-inactive': profile.tinhTrang === 'Đã tạm khóa',
                  }"
                >
                  {{ profile.tinhTrang || 'Chưa cập nhật' }}
                </span>
              </div>
            </div>
          </div>
        </div>
        <div class="col-12 mt-4">
          <div class="separator"></div>
          <div class="space-y-3">
            <button class="modern-btn modern-btn-primary w-100" @click="showEditModal">
              Chỉnh sửa hồ sơ
            </button>
          </div>
        </div>
      </div>
      
      <div v-else class="modern-alert alert-warning">
        Không tìm thấy thông tin khách hàng.
      </div>
      <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
          <div class="modal-content modern-modal">
            <div class="modal-header">
              <h5 class="modal-title modern-modal-title" id="editModalLabel">Chỉnh sửa hồ sơ</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4">
              <form @submit.prevent="updateProfile" enctype="multipart/form-data" class="space-y-6">
                <div class="modern-card mb-4">
                  <div class="card-body text-center p-6">
                    <div class="relative inline-block">
                      <div class="avatar-container">
                        <img
                          v-if="editProfile.hinhDaiDien && !editProfile.anh"
                          :src="`${getApiUrl}${editProfile.hinhDaiDien}?t=${new Date().getTime()}`"
                          alt="Hình đại diện"
                          class="modern-avatar"
                          @error="imageError"
                        />
                        <img
                          v-else-if="editProfile.anh"
                          :src="URL.createObjectURL(editProfile.anh)"
                          alt="Hình đại diện mới"
                          class="modern-avatar"
                        />
                        <div v-else class="modern-avatar-placeholder">
                          {{ getInitials(editProfile.hoTen) }}
                        </div>
                        <label class="avatar-upload-btn">
                          <i class="fas fa-camera"></i>
                          <input
                            type="file"
                            id="anh"
                            @change="onFileChange"
                            accept="image/jpeg,image/jpg,image/png"
                            class="d-none"
                          />
                        </label>
                      </div>
                    </div>
                    <p class="text-sm text-muted mt-3">Nhấn để thay đổi ảnh đại diện</p>
                  </div>
                </div>
                <div class="modern-card">
                  <div class="card-header p-4">
                    <h6 class="card-title mb-0 flex items-center">
                      <i class="fas fa-user me-2"></i>
                      Thông tin cá nhân
                    </h6>
                  </div>
                  <div class="card-body p-4 space-y-4">
                    <div class="form-group">
                      <label for="hoTen" class="form-label">Họ và tên</label>
                      <input
                        v-model="editProfile.hoTen"
                        type="text"
                        class="modern-input"
                        id="hoTen"
                        placeholder="Nhập họ và tên"
                        required
                      />
                    </div>
                    <div class="form-group">
                      <label for="gioiTinh" class="form-label">Giới tính</label>
                      <select
                        v-model="editProfile.gioiTinh"
                        class="modern-input"
                        id="gioiTinh"
                        required
                      >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>
                    <div class="form-group">
                      <label for="ngaySinh" class="form-label flex items-center">
                        <i class="fas fa-calendar me-2"></i>
                        Ngày sinh
                      </label>
                      <input
                        v-model="editProfile.ngaySinh"
                        type="date"
                        class="modern-input"
                        id="ngaySinh"
                      />
                    </div>
                    <div class="form-group">
                      <label for="sdt" class="form-label flex items-center">
                        <i class="fas fa-phone me-2"></i>
                        Số điện thoại
                      </label>
                      <input
                        v-model="editProfile.sdt"
                        type="text"
                        class="modern-input"
                        id="sdt"
                        placeholder="Nhập số điện thoại"
                        required
                      />
                    </div>
                    <div class="form-group">
                      <label for="email" class="form-label flex items-center">
                        <i class="fas fa-envelope me-2"></i>
                        Email
                      </label>
                      <input
                        v-model="editProfile.email"
                        type="email"
                        class="modern-input"
                        id="email"
                        placeholder="Nhập email"
                      />
                    </div>
                  </div>
                </div>
                <div class="modern-card">
                  <div class="card-header p-4">
                    <h6 class="card-title mb-0 flex items-center">
                      <i class="fas fa-map-marker-alt me-2"></i>
                      Địa chỉ
                    </h6>
                  </div>
                  <div class="card-body p-4 space-y-4">
                    <div class="form-group">
                      <label for="diaChi" class="form-label">Địa chỉ</label>
                      <textarea
                        v-model="editProfile.diaChi"
                        class="modern-input modern-textarea"
                        id="diaChi"
                        placeholder="Nhập địa chỉ của bạn"
                        required
                      ></textarea>
                    </div>
                    <div class="form-group">
                      <label for="cccd" class="form-label">CCCD</label>
                      <input
                        v-model="editProfile.cccd"
                        type="text"
                        class="modern-input"
                        id="cccd"
                        placeholder="Nhập số CCCD"
                        required
                      />
                    </div>
                  </div>
                </div>

                <div class="separator"></div>
                <div class="space-y-3">
                  <button type="submit" class="modern-btn modern-btn-primary w-100" :disabled="loading">
                    {{ loading ? 'Đang cập nhật...' : 'Lưu thay đổi' }}
                  </button>
                  <button type="button" class="modern-btn modern-btn-outline w-100" data-bs-dismiss="modal">
                    Hủy bỏ
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
    <br /><br />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { Modal } from 'bootstrap'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../constants/api.js'
import Cookies from 'js-cookie'
import { useRouter } from 'vue-router'

const router = useRouter()
const profile = ref(null)
const editProfile = ref({
  maKh: null,
  hoTen: '',
  gioiTinh: '',
  ngaySinh: '',
  diaChi: '',
  cccd: '',
  sdt: '',
  email: '',
  tenTaiKhoan: '',
  tinhTrang: 'Đang hoạt động',
  hinhDaiDien: '',
  anh: null,
})
const loading = ref(true)
const error = ref(null)
const getApiUrl = GetApiUrl()
const getInitials = (name) => {
  if (!name) return 'N/A'
  return name.split(' ').map(word => word.charAt(0)).join('').toUpperCase().slice(0, 2)
}

const fetchProfile = async () => {
  loading.value = true
  error.value = null
  try {
    const accessToken = Cookies.get('accessToken')
    if (!accessToken) {
      throw new Error('Vui lòng đăng nhập để xem thông tin cá nhân.')
    }

    const response = await axios.get(`${getApiUrl}/api/Profile/GetProfile`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    })
    console.log('API Response:', response.data)

    if (response.data.success) {
      profile.value = response.data.data || {}
      editProfile.value = {
        ...editProfile.value,
        ...response.data.data,
        anh: null,
      }
      // editProfile.value.hoTen = editProfile.value.hoTen || ''
      // editProfile.value.gioiTinh = editProfile.value.gioiTinh || ''
      // editProfile.value.diaChi = editProfile.value.diaChi || ''
      // editProfile.value.cccd = editProfile.value.cccd || ''
      // editProfile.value.sdt = editProfile.value.sdt || ''
      // editProfile.value.email = editProfile.value.email || ''
      // editProfile.value.tenTaiKhoan = editProfile.value.tenTaiKhoan || ''
    } else {
      error.value = response.data.message || 'Không tìm thấy thông tin khách hàng'
    }
  } catch (err) {
    console.error('Lỗi khi lấy thông tin hồ sơ:', err)
    error.value = err.response?.data?.message || err.message || 'Có lỗi xảy ra khi tải thông tin hồ sơ.'
    if (err.response?.status === 401) {
      await handleTokenRefresh()
      await fetchProfile() 
    }
  } finally {
    loading.value = false
  }
}

const handleTokenRefresh = async () => {
  const refreshToken = Cookies.get('refreshToken')
  if (!refreshToken) {
    await Swal.fire({
      icon: 'error',
      title: 'Phiên hết hạn',
      text: 'Vui lòng đăng nhập lại.',
      confirmButtonText: 'OK',
    })
    router.push('/login')
    return
  }

  try {
    const response = await axios.post(`${getApiUrl}/api/Account/RenewAccessToken`, {
      Id: profile.value?.maKh,
      HoTen: profile.value?.hoTen,
      SDT: profile.value?.sdt,
      RefreshToken: refreshToken,
    })
    if (response.data.success) {
      Cookies.set('accessToken', response.data.data.accessToken, { expires: 2 / 24, secure: true, sameSite: 'Strict' })
    } else {
      throw new Error(response.data.message || 'Không thể làm mới token')
    }
  } catch (err) {
    console.error('Lỗi khi làm mới token:', err)
    await Swal.fire({
      icon: 'error',
      title: 'Phiên hết hạn',
      text: 'Vui lòng đăng nhập lại.',
      confirmButtonText: 'OK',
    })
    router.push('/login')
  }
}

const showEditModal = () => {
  const modal = new Modal(document.getElementById('editModal'))
  modal.show()
}
const onFileChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    if (file.size > 5 * 1024 * 1024) {
      Swal.fire('Lỗi!', 'Kích thước file không được vượt quá 5MB.', 'error')
      return
    }
    const validExtensions = ['.jpg', '.jpeg', '.png']
    const extension = `.${file.name.split('.').pop().toLowerCase()}`
    if (!validExtensions.includes(extension)) {
      Swal.fire('Lỗi!', 'Chỉ hỗ trợ file .jpg, .jpeg, .png.', 'error')
      return
    }
    editProfile.value.anh = file
  }
}

const updateProfile = async () => {
  loading.value = true
  try {
    const fields = [
      { key: 'hoTen', message: 'Họ tên không được để trống' },
      { key: 'gioiTinh', message: 'Giới tính không được để trống' },
      { key: 'diaChi', message: 'Địa chỉ không được để trống' },
      { key: 'cccd', message: 'CCCD không được để trống' },
      { key: 'sdt', message: 'Số điện thoại không được để trống' },
    ]

    for (const field of fields) {
      const value = editProfile.value[field.key]
      if (!value || (typeof value === 'string' && !value.trim())) {
        throw new Error(field.message)
      }
    }

    if (!/^\d{12}$/.test(editProfile.value.cccd)) {
      throw new Error('CCCD phải là 12 chữ số')
    }
    if (!/^0\d{9,10}$/.test(editProfile.value.sdt)) {
      throw new Error('Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số')
    }
    if (editProfile.value.email && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(editProfile.value.email)) {
      throw new Error('Email không hợp lệ')
    }

    const formData = new FormData()
    formData.append('MaKh', editProfile.value.maKh || 104)
    formData.append('HoTen', editProfile.value.hoTen.trim())
    formData.append('GioiTinh', editProfile.value.gioiTinh.trim())
    if (editProfile.value.ngaySinh) formData.append('NgaySinh', editProfile.value.ngaySinh)
    formData.append('DiaChi', editProfile.value.diaChi.trim())
    formData.append('Cccd', editProfile.value.cccd.trim())
    formData.append('Sdt', editProfile.value.sdt.trim())
    if (editProfile.value.email) formData.append('Email', editProfile.value.email.trim())
    formData.append('TenTaiKhoan', editProfile.value.tenTaiKhoan.trim())
    formData.append('TinhTrang', editProfile.value.tinhTrang || 'Đang hoạt động')
    if (editProfile.value.anh) formData.append('Anh', editProfile.value.anh)

    const accessToken = Cookies.get('accessToken')
    const response = await axios.put(`${getApiUrl}/api/Profile/UpdateProfile`, formData, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        'Content-Type': 'multipart/form-data',
      },
    })

    if (response.data.success) {
      await Swal.fire('Thành công!', 'Cập nhật thông tin thành công!', 'success')
      const modal = Modal.getInstance(document.getElementById('editModal'))
      modal.hide()
      await fetchProfile() 
      editProfile.value.anh = null
    } else {
      throw new Error(response.data.message)
    }
  } catch (error) {
    console.error('Lỗi khi cập nhật thông tin:', error)
    await Swal.fire('Lỗi!', error.message || 'Có lỗi xảy ra khi cập nhật thông tin.', 'error')
  } finally {
    loading.value = false
  }
}

const formatDate = (date) => {
  if (!date) return 'Chưa cập nhật'
  const d = new Date(date)
  return isNaN(d) ? 'Chưa cập nhật' : d.toLocaleDateString('vi-VN')
}

onMounted(() => {
  fetchProfile()
})
</script>

<style scoped>
.container { 
  max-width: 1200px;
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 0 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.sticky-header {
  position: sticky;
  top: 0;
  z-index: 40;
  background: white;
  border-bottom: 1px solid #e2e8f0;
}

.modern-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e40af;
  margin: 0;
}
.space-y-6 > * + * {
  margin-top: 1.5rem;
}

.space-y-4 > * + * {
  margin-top: 1rem;
}

.space-y-3 > * + * {
  margin-top: 0.75rem;
}

.flex {
  display: flex;
}

.items-center {
  align-items: center;
}

.relative {
  position: relative;
}

.inline-block {
  display: inline-block;
}

.text-center {
  text-align: center;
}

.text-sm {
  font-size: 0.875rem;
}

.text-muted {
  color: #64748b;
}

.w-100 {
  width: 100%;
}

.me-2 {
  margin-right: 0.5rem;
}

.mb-0 {
  margin-bottom: 0;
}

.mb-4 {
  margin-bottom: 1rem;
}

.mt-3 {
  margin-top: 0.75rem;
}

.mt-4 {
  margin-top: 1rem;
}

.d-none {
  display: none;
}
.modern-card {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06);
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.card-header {
  border-bottom: 1px solid #e2e8f0;
  background: #f9fafb;
}

.card-title {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
}

.card-body {
  background: white;
}
.avatar-container {
  position: relative;
  display: inline-block;
}

.avatar-img,
.modern-avatar {
  width: 6rem;
  height: 6rem;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #e2e8f0;
}

.modern-avatar-placeholder {
  width: 6rem;
  height: 6rem;
  border-radius: 50%;
  background: linear-gradient(135deg, #6B46C1 0%, #ED64A6 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
  border: 3px solid #e2e8f0;
}

.avatar-upload-btn {
  position: absolute;
  bottom: 0;
  right: 0;
  background: #3b82f6;
  color: white;
  padding: 0.5rem;
  border-radius: 50%;
  cursor: pointer;
  border: 2px solid white;
  transition: all 0.2s ease;
}

.avatar-upload-btn:hover {
  background: #2563eb;
  transform: scale(1.05);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.info-row:last-child {
  border-bottom: none;
}

.info-label {
  font-weight: 500;
  color: #475569;
  margin: 0;
}

.info-value {
  color: #1e293b;
  font-weight: 500;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.status-active {
  background-color: #dcfce7;
  color: #166534;
}

.status-inactive {
  background-color: #fef3c7;
  color: #92400e;
}

.modern-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.375rem;
  padding: 0.75rem 1.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  border: none;
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
}

.modern-btn-primary {
  background-color: #3b82f6;
  color: white;
}

.modern-btn-primary:hover:not(:disabled) {
  background-color: #2563eb;
  transform: translateY(-1px);
}

.modern-btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.modern-btn-outline {
  background-color: transparent;
  color: #6b7280;
  border: 1px solid #d1d5db;
}

.modern-btn-outline:hover {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.form-group {
  margin-bottom: 1rem;
}

.form-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.5rem;
}

.modern-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  transition: all 0.2s ease;
  background-color: white;
}

.modern-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.modern-textarea {
  min-height: 5rem;
  resize: vertical;
}

.separator {
  height: 1px;
  background-color: #e2e8f0;
  margin: 1.5rem 0;
}

.modern-alert {
  padding: 1rem;
}

.modern-modal {
  border-radius: 0.5rem;
}

.modern-modal-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
}
</style>