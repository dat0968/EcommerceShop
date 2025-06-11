<template>
  <div>
    <br /><br /><br />
    <div class="container mt-5">
      <h2 class="mb-4 text-primary" style="color: black; font-size: 50px">Thông Tin Cá Nhân</h2>
      <div v-if="loading" class="text-center">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Đang tải...</span>
        </div>
      </div>
      <div v-else-if="error" class="alert alert-danger">
        {{ error }}
      </div>
      <div v-else-if="profile" class="card custom-card">
        <div class="card-body">
          <div class="row">
            <div class="col-md-6 text-center">
              <img
                v-if="editProfile.hinhDaiDien && !editProfile.anh"
                :src="`${getApiUrl}${editProfile.hinhDaiDien}?t=${new Date().getTime()}`"
                alt="Hình đại diện"
                class="avatar-img"
                @error="imageError"
              />
              <img
                v-else
                src="data:image/jpeg;base64,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"
                alt="Hình đại diện mặc định"
                class="avatar-img mb-3"
              />
            </div>
            <div class="col-md-6">
              <p><strong>Mã khách hàng:</strong> {{ profile.maKh }}</p>
              <p><strong>Họ tên:</strong> {{ profile.hoTen || 'Chưa cập nhật' }}</p>
              <p><strong>Giới tính:</strong> {{ profile.gioiTinh || 'Chưa cập nhật' }}</p>
              <p><strong>Ngày sinh:</strong> {{ formatDate(profile.ngaySinh) }}</p>
              <p><strong>Địa chỉ:</strong> {{ profile.diaChi || 'Chưa cập nhật' }}</p>
              <p><strong>CCCD:</strong> {{ profile.cccd || 'Chưa cập nhật' }}</p>
              <p><strong>Số điện thoại:</strong> {{ profile.sdt || 'Chưa cập nhật' }}</p>
              <p><strong>Email:</strong> {{ profile.email || 'Chưa có' }}</p>
              <p><strong>Tên tài khoản:</strong> {{ profile.tenTaiKhoan || 'Chưa cập nhật' }}</p>
              <p>
                <strong>Trạng thái:</strong>
                <span
                  :class="{
                    'badge bg-success': profile.tinhTrang === 'Đang hoạt động',
                    'badge bg-warning': profile.tinhTrang === 'Đã tạm khóa',
                  }"
                >
                  {{ profile.tinhTrang || 'Chưa cập nhật' }}
                </span>
              </p>
              <button class="btn btn-primary custom-btn" @click="showEditModal">Sửa thông tin</button>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="alert alert-warning">Không tìm thấy thông tin khách hàng.</div>

      <!-- Modal chỉnh sửa -->
      <div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
          <div class="modal-content custom-modal-content">
            <div class="modal-header">
              <h5 class="modal-title" id="editModalLabel" style="font-size: 50px">Sửa Thông Tin Cá Nhân</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <form @submit.prevent="updateProfile" enctype="multipart/form-data">
                <div class="row">
                  <div class="col-md-6 text-center avatar-container">
                   <img
                      v-if="editProfile.hinhDaiDien && !editProfile.anh"
                      :src="`${getApiUrl}${editProfile.hinhDaiDien}?t=${new Date().getTime()}`"
                      alt="Hình đại diện"
                      class="avatar-img"
                      @error="imageError"
                    />
                    <img
                      v-else-if="editProfile.anh"
                      :src="URL.createObjectURL(editProfile.anh)"
                      alt="Hình đại diện mới"
                      class="avatar-img"
                    />
                    <img
                      v-else
                      src="https://via.placeholder.com/200x200?text=No+Image"
                      alt="Hình đại diện mặc định"
                      class="avatar-img"
                    />
                    <div class="mb-3 mt-3">
                      <label for="anh" class="form-label">Hình Đại Diện</label>
                      <input
                        type="file"
                        class="form-control custom-input"
                        id="anh"
                        @change="onFileChange"
                        accept="image/jpeg,image/jpg,image/png"
                      />
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="mb-3">
                      <label for="hoTen" class="form-label">Họ tên</label>
                      <input
                        v-model="editProfile.hoTen"
                        type="text"
                        class="form-control custom-input"
                        id="hoTen"
                        required
                      />
                    </div>
                    <div class="mb-3">
                      <label for="gioiTinh" class="form-label">Giới tính</label>
                      <select
                        v-model="editProfile.gioiTinh"
                        class="form-select custom-input"
                        id="gioiTinh"
                        required
                      >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>
                    <div class="mb-3">
                      <label for="ngaySinh" class="form-label">Ngày sinh</label>
                      <input
                        v-model="editProfile.ngaySinh"
                        type="date"
                        class="form-control custom-input"
                        id="ngaySinh"
                      />
                    </div>
                    <div class="mb-3">
                      <label for="diaChi" class="form-label">Địa chỉ</label>
                      <input
                        v-model="editProfile.diaChi"
                        type="text"
                        class="form-control custom-input"
                        id="diaChi"
                        required
                      />
                    </div>
                    <div class="mb-3">
                      <label for="cccd" class="form-label">CCCD</label>
                      <input
                        v-model="editProfile.cccd"
                        type="text"
                        class="form-control custom-input"
                        id="cccd"
                        required
                      />
                    </div>
                    <div class="mb-3">
                      <label for="sdt" class="form-label">Số điện thoại</label>
                      <input
                        v-model="editProfile.sdt"
                        type="text"
                        class="form-control custom-input"
                        id="sdt"
                        required
                      />
                    </div>
                    <div class="mb-3">
                      <label for="email" class="form-label">Email</label>
                      <input
                        v-model="editProfile.email"
                        type="email"
                        class="form-control custom-input"
                        id="email"
                      />
                    </div>
                  </div>
                </div>
                <div class="text-end mt-3">
                  <button type="submit" class="btn btn-success custom-submit-btn" :disabled="loading">
                    {{ loading ? 'Đang cập nhật...' : 'Cập nhật' }}
                  </button>
                  <button type="button" class="btn btn-secondary ms-2" data-bs-dismiss="modal">Hủy</button>
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
import { ref, onMounted, watch } from 'vue'
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

// Fetch profile data
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
      // Đảm bảo các trường không null
      editProfile.value.hoTen = editProfile.value.hoTen || ''
      editProfile.value.gioiTinh = editProfile.value.gioiTinh || ''
      editProfile.value.diaChi = editProfile.value.diaChi || ''
      editProfile.value.cccd = editProfile.value.cccd || ''
      editProfile.value.sdt = editProfile.value.sdt || ''
      editProfile.value.email = editProfile.value.email || ''
      editProfile.value.tenTaiKhoan = editProfile.value.tenTaiKhoan || ''
    } else {
      error.value = response.data.message || 'Không tìm thấy thông tin khách hàng'
    }
  } catch (err) {
    console.error('Lỗi khi lấy thông tin hồ sơ:', err)
    error.value = err.response?.data?.message || err.message || 'Có lỗi xảy ra khi tải thông tin hồ sơ.'
    if (err.response?.status === 401) {
      await handleTokenRefresh()
      await fetchProfile() // Thử lại sau khi làm mới token
    }
  } finally {
    loading.value = false
  }
}

// Handle token refresh
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

// Show edit modal
const showEditModal = () => {
  const modal = new Modal(document.getElementById('editModal'))
  modal.show()
}

// Handle file change
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

// Update profile
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
      await fetchProfile() // Lấy lại dữ liệu mới
      editProfile.value.anh = null
      imagePreview.value = ''
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

// Format date
const formatDate = (date) => {
  if (!date) return 'Chưa cập nhật'
  const d = new Date(date)
  return isNaN(d) ? 'Chưa cập nhật' : d.toLocaleDateString('vi-VN')
}

// Initialize
onMounted(() => {
  fetchProfile()
})
</script>

<style scoped>
.container { 
  max-width: 800px; 
  background: linear-gradient(135deg, #e0f7fa 0%, #f8bbd0 100%);
  border-radius: 15px;
  padding: 20px;
  box-shadow: 0 8px 16px rgba(33, 150, 243, 0.15);
}

h2.mb-4.text-primary {
  color: #0288d1 !important;
  font-size: 50px;
  text-shadow: 1px 1px 2px rgba(255, 255, 255, 0.8);
}

.custom-card {
  border-radius: 15px;
  box-shadow: 0 4px 12px rgba(3, 169, 244, 0.2);
  background: rgba(255, 255, 255, 0.85);
  border: 2px solid #81d4fa;
  transition: all 0.3s ease;
}

.custom-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 16px rgba(33, 150, 243, 0.3);
}

.card-body { 
  padding: 30px; 
  background: transparent;
}

.avatar-img {
  width: 300px;
  height: 400px;
  object-fit: cover;
  border: 4px solid #81d4fa;
  box-shadow: 0 4px 8px rgba(3, 169, 244, 0.2);
  transition: all 0.4s ease;
  border-radius: 10px;
}

.avatar-img:hover {
  border-color: #f48fb1;
  transform: scale(1.03);
}

.badge.bg-success {
  background: linear-gradient(45deg, #4fc3f7, #29b6f6) !important;
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 14px;
}

.badge.bg-warning {
  background: linear-gradient(45deg, #f48fb1, #f06292) !important;
  color: #fff !important;
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 14px;
}

.custom-btn {
  background: linear-gradient(45deg, #29b6f6, #4fc3f7);
  border: none;
  border-radius: 8px;
  padding: 12px 24px;
  font-weight: 600;
  color: white;
  box-shadow: 0 4px 8px rgba(41, 182, 246, 0.3);
  transition: all 0.3s ease;
}

.custom-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 12px rgba(41, 182, 246, 0.4);
  background: linear-gradient(45deg, #039be5, #29b6f6);
}

.custom-modal-content {
  border-radius: 15px;
  box-shadow: 0 8px 24px rgba(3, 169, 244, 0.25);
  border: 2px solid #81d4fa;
  overflow: hidden;
}

.modal-header {
  background: linear-gradient(45deg, #4fc3f7, #29b6f6);
  color: white !important;
  border-bottom: 2px solid #0288d1;
}

.modal-title {
  color: white !important;
  text-shadow: 1px 1px 3px rgba(0, 0, 0, 0.2);
}

.modal-body {
  padding: 30px;
  background: linear-gradient(135deg, #f8bbd0 0%, #e1f5fe 100%);
}

.form-label {
  font-weight: 600;
  color: #0277bd;
}

.custom-input {
  border: 2px solid #81d4fa;
  border-radius: 8px;
  padding: 12px;
  background: rgba(255, 255, 255, 0.8);
  transition: all 0.3s ease;
}

.custom-input:focus {
  border-color: #f48fb1;
  box-shadow: 0 0 8px rgba(244, 143, 177, 0.4);
  background: white;
}

.custom-submit-btn {
  background: linear-gradient(45deg, #f48fb1, #f06292);
  border: none;
  border-radius: 8px;
  padding: 12px 32px;
  font-size: 16px;
  font-weight: 600;
  color: white;
  box-shadow: 0 4px 8px rgba(244, 143, 177, 0.3);
  transition: all 0.3s ease;
}

.custom-submit-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 12px rgba(244, 143, 177, 0.4);
  background: linear-gradient(45deg, #ec407a, #f06292);
}

.avatar-container {
  position: relative;
  padding: 15px;
  background: rgba(129, 212, 250, 0.15);
  border-radius: 12px;
}
</style>
