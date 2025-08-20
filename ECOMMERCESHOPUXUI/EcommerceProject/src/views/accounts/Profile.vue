
<template>
  <div>
    <br /><br /><br />
    <div style="width: 1400px; margin-left:80px; background-color: aliceblue;">
      <div class="bg-white sticky-header border-b p-4 mb-6">
        <h1 class="mb-0 text-primary modern-title">← Chỉnh sửa thông tin</h1>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-8">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Đang tải...</span>
        </div>
      </div>
      
      <div v-else-if="error" class="alert alert-danger modern-alert mx-4">
        <i class="fas fa-exclamation-triangle me-2"></i>
        {{ error }}
      </div>
      
      <div v-else-if="profile" class="row" >
      
        <div class="col-md-3" style="margin-left: 0px;">
          <br /><br />
          <div class="modern-card">
            <div class="card-body text-center p-6">
              <div class="relative inline-block mb-4">
                <div class="avatar-container">
                  <br /><br />
                  <img
                  style="width: 250px; height: auto;"
                    v-if="profile.hinh"
                    :src="getImageUrl(profile.hinh)"
                    alt="Hình đại diện"
                    class="avatar-img mb-3"
                    @error="handleImageError"
                  />
                  <div v-else class="modern-avatar-placeholder">
                    {{ getInitials(profile.hoTen) }}
                  </div>
                </div>
                
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-8">
          <br /><br />
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
          
          <br />
          
          <!-- Address Card -->
          <div class="modern-card">
            <div class="card-header p-4">
              <h5 class="card-title mb-0 flex items-center">
                <i class="fas fa-map-marker-alt me-2"></i>
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
          
          <br />
          
          <!-- Account Information Card -->
          <div class="modern-card">
            <div class="card-header p-4">
              <h5 class="card-title mb-0 flex items-center">
                <i class="fas fa-user-cog me-2"></i>
                Thông tin tài khoản
              </h5>
            </div>
            <div class="card-body p-4 space-y-4">
              <div class="info-row" style="margin-left: 20px;">
                <label class="info-label">Tên tài khoản:</label>
                <span class="info-value" style="margin-right: 20px;">{{ profile.tenTaiKhoan || 'Chưa cập nhật' }}</span>
              </div>
              <div class="info-row" style="margin-left: 20px;">
                <label class="info-label">Trạng thái:</label>
                <span
                  style="margin-right: 20px;"
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
        
        <!-- Edit Button -->
        <div class="col-12 mt-4">
          <div class="separator"></div>
          <div class="space-y-3">
            <button 
              class="modern-btn modern-btn-primary w-100" 
              @click="showEditModal"
              :disabled="loading"
            >
              <i class="fas fa-edit me-2"></i>
              Chỉnh sửa hồ sơ
            </button>
          </div>
        </div>
      </div>
      
      <!-- No Profile Found -->
      <div v-else class="modern-alert alert-warning mx-4">
        <i class="fas fa-exclamation-triangle me-2"></i>
        Không tìm thấy thông tin khách hàng.
      </div>

      <!-- Edit Modal -->
      <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
          <div class="modal-content modern-modal">
            <div class="modal-header">
              <h5 class="modal-title modern-modal-title" id="editModalLabel">
                <i class="fas fa-edit me-2"></i>
                Chỉnh sửa hồ sơ
              </h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4">
              <form @submit.prevent="updateProfile" class="space-y-6">
                
                <!-- Avatar Upload Section -->
                <div class="modern-card mb-4">
                  <div class="card-body text-center p-6">
                    <div class="relative inline-block">
                      <div class="avatar-container">
                        <img
                          v-if="previewImage"
                          :src="previewImage"
                          alt="Hình đại diện"
                          class="modern-avatar"
                        />
                        <img
                          v-else-if="editProfile.hinh"
                          :src="getImageUrl(editProfile.hinh)"
                          alt="Hình đại diện"
                          class="modern-avatar"
                          @error="handleImageError"
                        />
                        <div v-else class="modern-avatar-placeholder">
                          {{ getInitials(editProfile.hoTen) }}
                        </div>
                        <label class="avatar-upload-btn" for="fileInput">
                          <i class="fas fa-camera"></i>
                          <input
                            id="fileInput"
                            type="file"
                            @change="onFileChange"
                            accept="image/jpeg,image/jpg,image/png"
                            class="d-none"
                          />
                        </label>
                      </div>
                    </div>
                    <p class="text-sm text-muted mt-3">
                      Nhấn để thay đổi ảnh đại diện
                      <br>
                      <small>Hỗ trợ: JPG, PNG (tối đa 5MB)</small>
                    </p>
                  </div>
                </div>
                
                <!-- Personal Information -->
                <div class="modern-card">
                  <div class="card-header p-4">
                    <h6 class="card-title mb-0 flex items-center">
                      <i class="fas fa-user me-2"></i>
                      Thông tin cá nhân
                    </h6>
                  </div>
                  <div class="card-body p-4 space-y-4">
                    <div class="row">
                      <div class="col-md-6">
                        <div class="form-group">
                          <label for="hoTen" class="form-label">
                            Họ và tên <span class="text-danger">*</span>
                          </label>
                          <input
                            v-model="editProfile.hoTen"
                            type="text"
                            class="modern-input"
                            id="hoTen"
                            placeholder="Nhập họ và tên"
                            required
                            maxlength="100"
                          />
                        </div>
                      </div>
                      <div class="col-md-6">
                        <div class="form-group">
                          <label for="gioiTinh" class="form-label">
                            Giới tính <span class="text-danger">*</span>
                          </label>
                          <select
                            v-model="editProfile.gioiTinh"
                            class="modern-input"
                            id="gioiTinh"
                            required
                          >
                            <option value="">Chọn giới tính</option>
                            <option value="Nam">Nam</option>
                            <option value="Nữ">Nữ</option>
                            <option value="Khác">Khác</option>
                          </select>
                        </div>
                      </div>
                    </div>
                    
                    <div class="row">
                      <div class="col-md-6">
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
                            :max="maxDate"
                          />
                        </div>
                      </div>
                      <div class="col-md-6">
                        <div class="form-group">
                          <label for="sdt" class="form-label flex items-center">
                            <i class="fas fa-phone me-2"></i>
                            Số điện thoại <span class="text-danger">*</span>
                          </label>
                          <input
                            v-model="editProfile.sdt"
                            type="text"
                            class="modern-input"
                            id="sdt"
                            placeholder="Nhập số điện thoại"
                            required
                            maxlength="11"
                          />
                        </div>
                      </div>
                    </div>
                    
                    <div class="row">
                      <div class="col-md-6">
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
                            maxlength="100"
                          />
                        </div>
                      </div>
                      <div class="col-md-6">
                        <div class="form-group">
                          <label for="cccd" class="form-label">
                            CCCD <span class="text-danger">*</span>
                          </label>
                          <input
                            v-model="editProfile.cccd"
                            type="text"
                            class="modern-input"
                            id="cccd"
                            placeholder="Nhập số CCCD"
                            required
                            maxlength="12"
                          />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- Address Information -->
                <div class="modern-card">
                  <div class="card-header p-4">
                    <h6 class="card-title mb-0 flex items-center">
                      <i class="fas fa-map-marker-alt me-2"></i>
                      Địa chỉ
                    </h6>
                  </div>
                  <div class="card-body p-4">
                    <div class="form-group">
                      <label for="diaChi" class="form-label">
                        Địa chỉ <span class="text-danger">*</span>
                      </label>
                      <textarea
                        v-model="editProfile.diaChi"
                        class="modern-input modern-textarea"
                        id="diaChi"
                        placeholder="Nhập địa chỉ của bạn"
                        required
                        maxlength="500"
                        rows="3"
                      ></textarea>
                    </div>
                  </div>
                </div>

                <!-- Form Actions -->
                <div class="separator"></div>
                <div class="space-y-3">
                  <button 
                    type="submit" 
                    class="modern-btn modern-btn-primary w-100" 
                    :disabled="isSubmitting"
                  >
                    <i class="fas fa-save me-2"></i>
                    {{ isSubmitting ? 'Đang cập nhật...' : 'Lưu thay đổi' }}
                  </button>
                  <button 
                    type="button" 
                    class="modern-btn modern-btn-outline w-100" 
                    data-bs-dismiss="modal"
                    :disabled="isSubmitting"
                  >
                    <i class="fas fa-times me-2"></i>
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
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { Modal } from 'bootstrap'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../constants/api.js'
import Cookies from 'js-cookie'
import { useRouter } from 'vue-router'

const router = useRouter()

// Reactive data
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
  hinh: null,
})

const loading = ref(true)
const isSubmitting = ref(false)
const error = ref(null)
const selectedFile = ref(null)
const previewImage = ref(null)

// Constants
const getApiUrl = GetApiUrl()

// Computed properties
const maxDate = computed(() => {
  const today = new Date()
  return today.toISOString().split('T')[0]
})

// Utility functions
const getInitials = (name) => {
  if (!name) return 'N/A'
  return name.split(' ').map(word => word.charAt(0)).join('').toUpperCase().slice(0, 2)
}

const getImageUrl = (imagePath) => {
  if (!imagePath) return ''
  return imagePath.startsWith('http') ? imagePath : `${getApiUrl}${imagePath}`
}

const formatDate = (date) => {
  if (!date) return 'Chưa cập nhật'
  try {
    const d = new Date(date)
    return isNaN(d.getTime()) ? 'Chưa cập nhật' : d.toLocaleDateString('vi-VN')
  } catch (e) {
    return 'Chưa cập nhật'
  }
}

const formatDateForInput = (date) => {
  if (!date) return ''
  try {
    const d = new Date(date)
    if (isNaN(d.getTime()) || d.getFullYear() < 1900) {
      return ''
    }
    return d.toISOString().split('T')[0]
  } catch (e) {
    return ''
  }
}

// Validation functions
const validateForm = () => {
  const errors = []

  if (!editProfile.value.hoTen?.trim()) {
    errors.push('Họ tên không được để trống')
  }

  if (!editProfile.value.gioiTinh) {
    errors.push('Giới tính không được để trống')
  }

  if (!editProfile.value.diaChi?.trim()) {
    errors.push('Địa chỉ không được để trống')
  }

  if (!editProfile.value.cccd?.trim()) {
    errors.push('CCCD không được để trống')
  } else if (!/^\d{12}$/.test(editProfile.value.cccd.trim())) {
    errors.push('CCCD phải là 12 chữ số')
  }

  if (!editProfile.value.sdt?.trim()) {
    errors.push('Số điện thoại không được để trống')
  } else if (!/^0\d{9,10}$/.test(editProfile.value.sdt.trim())) {
    errors.push('Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số')
  }

  if (editProfile.value.email?.trim() && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(editProfile.value.email.trim())) {
    errors.push('Email không hợp lệ')
  }

  return errors
}

// API functions
const getAuthHeaders = () => {
  const accessToken = Cookies.get('accessToken')
  console.log('Access Token:', accessToken) // Debug
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {}
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
    return false
  }

  try {
    const response = await axios.post(`${getApiUrl}/api/Account/RenewAccessToken`, {
      Id: profile.value?.maKh,
      HoTen: profile.value?.hoTen,
      SDT: profile.value?.sdt,
      RefreshToken: refreshToken,
    })

    if (response.data.success) {
      Cookies.set('accessToken', response.data.data.accessToken, { 
        expires: 2 / 24, 
        secure: true, 
        sameSite: 'Strict' 
      })
      return true
    } else {
      throw new Error(response.data.message || 'Không thể làm mới token')
    }
  } catch (err) {
    console.error('Token refresh error:', err)
    await Swal.fire({
      icon: 'error',
      title: 'Phiên hết hạn',
      text: 'Vui lòng đăng nhập lại.',
      confirmButtonText: 'OK',
    })
    router.push('/login')
    return false
  }
}

const fetchProfile = async () => {
  loading.value = true
  error.value = null
  
  try {
    const headers = getAuthHeaders()
    if (!headers.Authorization) {
      throw new Error('Vui lòng đăng nhập để xem thông tin cá nhân.')
    }

    const response = await axios.get(`${getApiUrl}/api/Profile/GetProfile`, { headers })

    if (response.data.success) {
      profile.value = response.data.data || {}
      editProfile.value = {
        maKh: response.data.data.maKh,
        hoTen: response.data.data.hoTen || '',
        gioiTinh: response.data.data.gioiTinh || '',
        ngaySinh: formatDateForInput(response.data.data.ngaySinh),
        diaChi: response.data.data.diaChi || '',
        cccd: response.data.data.cccd || '',
        sdt: response.data.data.sdt || '',
        email: response.data.data.email || '',
        hinh: response.data.data.hinh || null,
      }
    } else {
      error.value = response.data.message || 'Không tìm thấy thông tin khách hàng'
    }
  } catch (err) {
    console.error('Fetch profile error:', err)
    
    if (err.response?.status === 401) {
      const refreshSuccess = await handleTokenRefresh()
      if (refreshSuccess) {
        await fetchProfile()
        return
      }
    }
    
    error.value = err.response?.data?.message || err.message || 'Có lỗi xảy ra khi tải thông tin hồ sơ.'
  } finally {
    loading.value = false
  }
}

// Event handlers
const handleImageError = (event) => {
  console.warn('Image load error:', event)
  event.target.style.display = 'none'
}

const onFileChange = (event) => {
  const file = event.target.files[0]
  if (!file) return

  // Validate file size (5MB max)
  if (file.size > 5 * 1024 * 1024) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Kích thước file không được vượt quá 5MB.',
      confirmButtonText: 'OK'
    })
    event.target.value = ''
    return
  }

  // Validate file type
  const validTypes = ['image/jpeg', 'image/jpg', 'image/png']
  if (!validTypes.includes(file.type)) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Chỉ hỗ trợ file JPG, PNG.',
      confirmButtonText: 'OK'
    })
    event.target.value = ''
    return
  }

  selectedFile.value = file
  
  // Create preview
  const reader = new FileReader()
  reader.onload = (e) => {
    previewImage.value = e.target.result
  }
  reader.readAsDataURL(file)
}

const showEditModal = () => {
  // Reset form with only editable fields
  editProfile.value = {
    maKh: profile.value?.maKh,
    hoTen: profile.value?.hoTen || '',
    gioiTinh: profile.value?.gioiTinh || '',
    ngaySinh: formatDateForInput(profile.value?.ngaySinh),
    diaChi: profile.value?.diaChi || '',
    cccd: profile.value?.cccd || '',
    sdt: profile.value?.sdt || '',
    email: profile.value?.email || '',
    hinh: profile.value?.hinh || null,
  }
  selectedFile.value = null
  previewImage.value = null
  
  const modal = new Modal(document.getElementById('editModal'))
  modal.show()
}

const updateProfile = async () => {
  // Validate only editable fields
  const validationErrors = validateForm()
  if (validationErrors.length > 0) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi validation!',
      text: validationErrors.join(', '),
      confirmButtonText: 'OK'
    })
    return
  }

  isSubmitting.value = true
  
  try {
    const formData = new FormData()

    // Chỉ gửi các trường cần chỉnh sửa
    formData.append('MaKH', editProfile.value.maKh || '')
    formData.append('HoTen', editProfile.value.hoTen?.trim() || '')
    formData.append('GioiTinh', editProfile.value.gioiTinh || '')
    formData.append('DiaChi', editProfile.value.diaChi?.trim() || '')
    formData.append('CCCD', editProfile.value.cccd?.trim() || '')
    formData.append('SDT', editProfile.value.sdt?.trim() || '')
    if (editProfile.value.email?.trim()) {
      formData.append('Email', editProfile.value.email.trim())
    }
    if (editProfile.value.ngaySinh) {
      formData.append('NgaySinh', editProfile.value.ngaySinh)
    }
    if (selectedFile.value) {
      formData.append('HinhDaiDien', selectedFile.value)
    }

    // Debug: Log form data
    console.log('Form data being sent:')
    for (let [key, value] of formData.entries()) {
      console.log(`${key}: ${value instanceof File ? '[File]' : value}`)
    }

    const headers = {
      ...getAuthHeaders(),
      'Content-Type': 'multipart/form-data',
    }

    const response = await axios.put(`${getApiUrl}/api/Profile/UpdateProfile`, formData, { headers })

    if (response.data.success) {
      await Swal.fire({
        icon: 'success',
        title: 'Thành công!',
        text: 'Cập nhật thông tin thành công!',
        confirmButtonText: 'OK'
      })
      
      // Close modal
      const modal = Modal.getInstance(document.getElementById('editModal'))
      if (modal) modal.hide()
      
      // Refresh profile data
      await fetchProfile()
    } else {
      throw new Error(response.data.message || 'Cập nhật thất bại')
    }
  } catch (err) {
    console.error('Update profile error:', err.response?.data || err.message)
    
    let errorMessage = 'Có lỗi xảy ra khi cập nhật thông tin.'
    
    if (err.response?.status === 401) {
      const refreshSuccess = await handleTokenRefresh()
      if (refreshSuccess) {
        await updateProfile()
        return
      }
    } else if (err.response?.data?.message) {
      errorMessage = err.response.data.message
    } else if (err.message) {
      errorMessage = err.message
    }
    
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: errorMessage,
      confirmButtonText: 'OK'
    })
  } finally {
    isSubmitting.value = false
  }
}

// Watchers
watch(() => editProfile.value.maKh, (newVal) => {
  if (!newVal && profile.value?.maKh) {
    editProfile.value.maKh = profile.value.maKh
  }
})

// Lifecycle
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
