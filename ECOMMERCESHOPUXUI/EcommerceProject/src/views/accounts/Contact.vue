<template>
  <br>
  <br>
  <div class="container p-4">
    <div class="modern-card">
      <div class="card-header p-4">
        <h5 class="card-title mb-0 flex items-center">
          <i class="fas fa-envelope me-2"></i>
          Liên hệ với chúng tôi
        </h5>
      </div>
      <div class="card-body p-4 space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <form @submit.prevent="handleSubmit" class="space-y-4">
              <div class="form-group">
                <label for="name" class="form-label">
                  Họ và tên <span class="text-danger">*</span>
                </label>
                <input
                  v-model="form.name"
                  type="text"
                  id="name"
                  class="modern-input"
                  placeholder="Nhập họ và tên"
                  required
                  maxlength="100"
                />
              </div>

              <div class="form-group">
                <label for="subject" class="form-label">
                  Tiêu đề <span class="text-danger">*</span>
                </label>
                <input
                  v-model="form.subject"
                  type="text"
                  id="subject"
                  class="modern-input"
                  placeholder="Nhập tiêu đề"
                  required
                  maxlength="200"
                />
              </div>

              <div class="form-group">
                <label for="message" class="form-label">
                  Nội dung <span class="text-danger">*</span>
                </label>
                <textarea
                  v-model="form.message"
                  id="message"
                  class="modern-input modern-textarea"
                  placeholder="Nhập nội dung liên hệ"
                  required
                  maxlength="1000"
                  rows="5"
                ></textarea>
              </div>

              <div class="separator"></div>

              <div class="space-y-3">
                <button
                  type="submit"
                  class="modern-btn modern-btn-primary w-100"
                  :disabled="isSubmitting"
                >
                  <i class="fas fa-paper-plane me-2"></i>
                  {{ isSubmitting ? 'Đang gửi...' : 'Gửi liên hệ' }}
                </button>
              </div>
            </form>
          </div>
          <div class="store-info">
            <div class="info-row">
             <div class="info-item">
                <i class="fas fa-map-marker-alt"></i>
              <h5>Trụ sở chính:</h5>
              </div>
              
                <span style="margin-left: 30px;  font-size: 18px;"> 300, 06 đường Hà Huy Tập, tổ dân phố 08, BMT, Đắk Lắk</span>
                 
              <div class="info-item">
                <i class="fas fa-phone"></i>
                <h5>Điện thoại:</h5>
                <span style=" margin-left: 10px; margin-top: 3px;   font-size: 18px;"> 0775 444 205</span>
              </div>
              
                
            </div>
            <div class="info-row">
              <div class="info-item">
                 <i class="fas fa-envelope"></i>
                 <h5>Email:</h5>
                <span style=" margin-left: 10px; margin-top: 3px;   font-size: 18px;">datntpk03691@gmail.com</span>
              </div>
             
              <div class="info-item">
                <i class="fas fa-globe"></i>
                <h5>Cửa hàng bán thời trang Angel:</h5>
                             </div>
                             <span style=" margin-left: 30px; margin-top: 3px;   font-size: 15px;"> www.angelshop.com (Lưu ý: Đây là website chuyên bán các dòng sản phẩm thời trang: Quần áo nam nữ, giày dép, túi xách, phụ kiện, váy đầm, áo khoác, áo sơ mi, vest, quần jean, quần tây, quần kaki và nhiều sản phẩm khác liên quan đến thời trang)</span>

            </div>
          </div>
        </div>

        <div v-if="loading" class="text-center py-8">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Đang tải...</span>
          </div>
        </div>

        <div v-else-if="error" class="modern-alert alert-danger">
          <i class="fas fa-exclamation-triangle me-2"></i>
          {{ error }}
        </div>
      </div>
    </div>
  </div>

  <br><br>
  
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../constants/api.js';
import Cookies from 'js-cookie';
import { useRouter } from 'vue-router';

const router = useRouter();

// Reactive data
const form = ref({
  name: '',
  subject: '',
  message: '',
});
const loading = ref(true);
const isSubmitting = ref(false);
const error = ref(null);

// Constants
const getApiUrl = GetApiUrl();

// API functions
const getAuthHeaders = () => {
  const accessToken = Cookies.get('accessToken');
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
};

const handleTokenRefresh = async () => {
  const refreshToken = Cookies.get('refreshToken');
  if (!refreshToken) {
    await Swal.fire({
      icon: 'error',
      title: 'Phiên hết hạn',
      text: 'Vui lòng đăng nhập lại.',
      confirmButtonText: 'OK',
    });
    router.push('/login');
    return false;
  }

  try {
    const response = await axios.post(`${getApiUrl}/api/Account/RenewAccessToken`, {
      Id: null,
      HoTen: form.value.name || '',
      SDT: null,
      RefreshToken: refreshToken,
    });

    if (response.data.success) {
      Cookies.set('accessToken', response.data.data.accessToken, {
        expires: 2 / 24, // 2h
        secure: true,
        sameSite: 'Strict',
      });
      return true;
    } else {
      throw new Error(response.data.message || 'Không thể làm mới token');
    }
  } catch (err) {
    await Swal.fire({
      icon: 'error',
      title: 'Phiên hết hạn',
      text: 'Vui lòng đăng nhập lại.',
      confirmButtonText: 'OK',
    });
    router.push('/login');
    return false;
  }
};

const handleSubmit = async () => {
  // Validate form
  const validationErrors = validateForm();
  if (validationErrors.length > 0) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: validationErrors.join(', '),
      confirmButtonText: 'OK',
    });
    return;
  }

  isSubmitting.value = true;
  error.value = null;

  try {
    const headers = getAuthHeaders();
    if (!headers.Authorization) {
      throw new Error('Vui lòng đăng nhập để gửi liên hệ.');
    }

    const response = await axios.post(
      `${getApiUrl}/api/Contact`,
      {
        name: form.value.name.trim(),
        subject: form.value.subject.trim(),
        message: form.value.message.trim(),
      },
      { headers }
    );

    if (response.data.success) {
      await Swal.fire({
        icon: 'success',
        title: 'Thành công!',
        text: response.data.message || 'Gửi liên hệ thành công!',
        confirmButtonText: 'OK',
      });
      form.value.name = '';
      form.value.subject = '';
      form.value.message = '';
    } else {
      throw new Error(response.data.message || 'Gửi liên hệ thất bại');
    }
  } catch (err) {
    if (err.response?.status === 401) {
      const refreshSuccess = await handleTokenRefresh();
      if (refreshSuccess) {
        await handleSubmit();
        return;
      }
    }

    let errorMessage = 'Có lỗi xảy ra khi gửi liên hệ.';
    if (err.response?.data?.message) {
      errorMessage = err.response.data.message;
    } else if (err.message) {
      errorMessage = err.message;
    }

    error.value = errorMessage;
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: errorMessage,
      confirmButtonText: 'OK',
    });
  } finally {
    isSubmitting.value = false;
  }
};

const handleLogout = async () => {
  Cookies.remove('accessToken');
  Cookies.remove('refreshToken');
  await Swal.fire({
    icon: 'info',
    title: 'Đã đăng xuất',
    text: 'Bạn đã đăng xuất thành công.',
    confirmButtonText: 'OK',
  });
  router.push('/login');
};

// Validation
const validateForm = () => {
  const errors = [];

  if (!form.value.name?.trim()) {
    errors.push('Họ tên không được để trống');
  }
  if (!form.value.subject?.trim()) {
    errors.push('Tiêu đề không được để trống');
  }
  if (!form.value.message?.trim()) {
    errors.push('Nội dung không được để trống');
  }

  return errors;
};

// Lifecycle
onMounted(() => {
  const accessToken = Cookies.get('accessToken');
  if (!accessToken) {
    error.value = 'Vui lòng đăng nhập để gửi liên hệ.';
    router.push('/login');
  } else {
    loading.value = false;
  }
});
</script>

<style scoped>
.container {
  max-width: 1500px;
  margin: 0 auto;
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
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
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
}

.card-body {
  background: white;
}

.grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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

.text-center {
  text-align: center;
}

.me-2 {
  margin-right: 0.5rem;
}

.mb-0 {
  margin-bottom: 0;
}

.w-100 {
  width: 100%;
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

.separator {
  height: 1px;
  background-color: #e2e8f0;
  margin: 1.5rem 0;
}

.modern-alert {
  padding: 1rem;
  border-radius: 0.375rem;
}

.alert-danger {
  background-color: #fee2e2;
  color: #991b1b;
}

.spinner-border {
  display: inline-block;
  width: 2rem;
  height: 2rem;
  vertical-align: text-bottom;
  border: 0.25rem solid #3b82f6;
  border-right-color: transparent;
  border-radius: 50%;
  animation: spinner-border 0.75s linear infinite;
}

@keyframes spinner-border {
  to {
    transform: rotate(360deg);
  }
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  border: 0;
}

/* Store Information Styles */
.store-info {
  padding: 1rem;
  margin-top: 20px;
}

.info-row {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.info-item {
  display: flex;
  align-items: center;
}

.info-item i {
  margin-right: 0.5rem;
  color: #3b82f6;
  font-size: 1rem;
}

.info-item span {
  font-size: 0.875rem;
  color: #374151;
}
</style>