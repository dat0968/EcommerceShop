<template>
  <form @submit.prevent="submitForm" class="customer-form">
    <!-- Form header -->
    <div class="form-header">
      <div class="form-accent-border"></div>
      <h1>{{ isEdit ? 'Cập Nhật Khách Hàng' : 'Thêm Mới Khách Hàng' }}</h1>
    </div>

    <!-- Column layout -->
    <div class="form-columns">
      <!-- Cột trái -->
      <div class="left-column">
        <!-- Hình đại diện -->
        <div class="form-group">
          <label class="section-title" for="hinhDaiDien" style="">
            Hình đại diện {{ isEdit ? '' : `*` }}
          </label>
          <div class="image-container">
            <!-- Input file phủ lên toàn bộ image-preview -->
            <input type="file" id="hinhDaiDien" @change="handleFileUpload" accept="image/*" class="file-input" />
            <div class="image-preview" :class="{ 'empty-preview': !imagePreview }">
              <img v-if="imagePreview" :src="imagePreview" alt="Xem trước hình ảnh" />
              <div v-else class="placeholder-content">
                <div class="placeholder-icon">
                  <span>+</span>
                </div>
                <span class="placeholder-text">Chọn hình ảnh</span>
              </div>
              <div class="image-overlay">
                <span class="change-image">Chọn ảnh mới</span>
              </div>
            </div>
          </div>
          <div class="error-message" v-if="errors.hinh">{{ errors.hinh }}</div>
        </div>

        <!-- Giới tính -->
        <div class="form-group">
          <label class="section-title" for="gioiTinh">Giới tính</label>
          <select id="gioiTinh" v-model="form.gioiTinh" class="info-input">
            <option value="Nam">Nam</option>
            <option value="Nữ">Nữ</option>
            <option value="Khác">Khác</option>
          </select>
        </div>

        <!-- CCCD -->
        <div class="form-group">
          <label class="section-title" for="cccd">
            CCCD <span class="required">*</span>
          </label>
          <input type="text" id="cccd" v-model="form.cccd" class="info-input" maxlength="12" @input="validateCCCD" />
          <div class="error-message" v-if="errors.cccd">{{ errors.cccd }}</div>
        </div>

        <!-- Số điện thoại -->
        <div class="form-group">
          <label class="section-title" for="sdt">
            Số điện thoại <span class="required">*</span>
          </label>
          <input type="text" id="sdt" v-model="form.sdt" @blur="handlePhoneBlur" class="info-input" maxlength="12" />
          <div class="error-message" v-if="errors.sdt">{{ errors.sdt }}</div>
        </div>
      </div>

      <!-- Cột phải -->
      <div class="right-column">
        <!-- Họ tên -->
        <div class="form-group">
          <label class="section-title" for="hoTen">
            Họ tên <span class="required">*</span>
          </label>
          <input type="text" id="hoTen" v-model="form.hoTen" class="info-input" />
          <div class="error-message" v-if="errors.hoTen">{{ errors.hoTen }}</div>
        </div>

        <!-- Ngày sinh -->
        <div class="form-group">
          <label class="section-title" for="ngaySinh">
            Ngày sinh <span class="required">*</span>
          </label>
          <input type="date" id="ngaySinh" v-model="form.ngaySinh" class="info-input date-input" />
          <div class="error-message" v-if="errors.ngaySinh">{{ errors.ngaySinh }}</div>
        </div>

        <!-- Địa chỉ -->
        <div class="form-group">
          <label class="section-title" for="diaChi">Địa chỉ</label>
          <input type="text" id="diaChi" v-model="form.diaChi" class="info-input" />
        </div>

        <!-- Email -->
        <div class="form-group">
          <label class="section-title" for="email">
            Email <span class="required">*</span>
          </label>
          <input type="email" id="email" v-model="form.email" class="info-input" />
          <div class="error-message" v-if="errors.email">{{ errors.email }}</div>
        </div>

        <!-- Tên tài khoản -->
        <div class="form-group" v-if="!isEdit">
          <label class="section-title" for="tenTaiKhoan">
            Tên tài khoản <span class="required">*</span>
          </label>
          <input type="text" id="tenTaiKhoan" v-model="form.tenTaiKhoan" class="info-input" />
          <div class="error-message" v-if="errors.tenTaiKhoan">{{ errors.tenTaiKhoan }}</div>
        </div>

        <!-- Mật khẩu -->
        <div class="form-group" v-if="!isEdit">
          <label class="section-title" for="matKhau">
            Mật khẩu <span class="required">*</span>
          </label>
          <input type="password" id="matKhau" v-model="form.matKhau" class="info-input" />
          <div class="error-message" v-if="errors.matKhau">{{ errors.matKhau }}</div>
        </div>

        <!-- Trạng thái -->
        <div class="form-group" v-if="isEdit">
          <label class="section-title" for="tinhTrang">Tình trạng</label>
          <select id="tinhTrang" v-model="form.tinhTrang" class="info-input">
            <option value="Đang hoạt động">Đang hoạt động</option>
            <option value="Đã tạm khóa">Đã tạm khóa</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Buttons -->
    <div class="buttons">
      <button type="button" class="btn-cancel" @click="cancelForm">Hủy</button>
      <button type="submit" class="btn-submit" :disabled="isSubmitting">
        {{ isSubmitting ? 'Đang xử lý...' : (isEdit ? 'Cập nhật' : 'Thêm mới') }}
      </button>
    </div>
  </form>
</template>

<script>
import { ref, reactive, onMounted, watch } from 'vue';
import axios from 'axios';
import Swal from 'sweetalert2';

export default {
  name: 'CustomerForm',
  props: {
    isEdit: {
      type: Boolean,
      default: false,
    },
    customerId: {
      type: Number,
      default: null,
    },
  },
  emits: ['submit-success', 'cancel'],
  setup(props, { emit }) {
    const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7217/api';

    const form = reactive({
      hoTen: '',
      gioiTinh: 'Nam',
      ngaySinh: '',
      diaChi: '',
      cccd: '',
      sdt: '',
      email: '',
      tenTaiKhoan: '',
      matKhau: '',
      tinhTrang: 'Đang hoạt động',
      isActive: true,
    });

    const errors = reactive({});
    const isSubmitting = ref(false);
    const imagePreview = ref('');
    const fileSelected = ref(null);

    const showSuccessMessage = (message) => {
      Swal.fire({
        title: 'Thành công!',
        text: message,
        icon: 'success',
        confirmButtonText: 'OK',
        confirmButtonColor: '#4CAF50',
      });
    };

    const showErrorMessage = (message) => {
      Swal.fire({
        title: 'Lỗi!',
        text: message,
        icon: 'error',
        confirmButtonText: 'OK',
        confirmButtonColor: '#f44336',
      });
    };

    const showWarningMessage = (message) => {
      Swal.fire({
        title: 'Cảnh báo!',
        text: message,
        icon: 'warning',
        confirmButtonText: 'OK',
        confirmButtonColor: '#ff9800',
      });
    };

    const showConfirmDialog = (message, callback) => {
      Swal.fire({
        title: 'Xác nhận',
        text: message,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Đồng ý',
        cancelButtonText: 'Hủy bỏ',
        confirmButtonColor: '#4CAF50',
        cancelButtonColor: '#f44336',
      }).then((result) => {
        if (result.isConfirmed && callback) {
          callback();
        }
      });
    };

    const showLoadingIndicator = (message = 'Đang xử lý...') => {
      Swal.fire({
        title: message,
        allowOutsideClick: false,
        showConfirmButton: false,
        willOpen: () => {
          Swal.showLoading();
        },
      });
    };

    const validateAge = (birthDate) => {
      const today = new Date();
      const birthDateObj = new Date(birthDate);
      let age = today.getFullYear() - birthDateObj.getFullYear();
      const monthDiff = today.getMonth() - birthDateObj.getMonth();

      if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDateObj.getDate())) {
        age--;
      }

      return age >= 10;
    };

    const formatPhoneNumber = (phone) => {
      if (!phone) return '';
      const cleaned = phone.replace(/\D/g, '');
      const truncated = cleaned.substring(0, 10);
      if (truncated.length >= 7) {
        return `${truncated.substring(0, 4)} ${truncated.substring(4, 7)} ${truncated.substring(7)}`.trim();
      } else if (truncated.length >= 4) {
        return `${truncated.substring(0, 4)} ${truncated.substring(4)}`.trim();
      }
      return truncated;
    };

    const validateCCCD = () => {
      const cccd = form.cccd.replace(/\D/g, '');
      form.cccd = cccd;
      if (cccd.length > 12) {
        form.cccd = cccd.substring(0, 12);
      }
    };

    const handleFileUpload = (event) => {
      const file = event.target.files[0];
      if (!file) return;

      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        showWarningMessage('Kích thước hình ảnh không được vượt quá 5MB');
        event.target.value = '';
        return;
      }

      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/jpg'];
      if (!allowedTypes.includes(file.type)) {
        showWarningMessage('Chỉ chấp nhận các định dạng hình ảnh: JPG, JPEG, PNG, GIF');
        event.target.value = '';
        return;
      }

      fileSelected.value = file;
      const reader = new FileReader();
      reader.onload = (e) => {
        imagePreview.value = e.target.result;
      };
      reader.readAsDataURL(file);
    };

    const handlePhoneBlur = () => {
      if (form.sdt) {
        form.sdt = formatPhoneNumber(form.sdt);
      }
    };

    watch(() => form.sdt, (newValue) => {
      if (document.activeElement?.id !== 'sdt') {
        form.sdt = formatPhoneNumber(newValue);
      }
    });

    const validateForm = () => {
      Object.keys(errors).forEach((key) => (errors[key] = ''));

      let isValid = true;

      if (!form.hoTen?.trim()) {
        errors.hoTen = 'Họ tên không được để trống';
        isValid = false;
      }

      if (!form.ngaySinh) {
        errors.ngaySinh = 'Ngày sinh không được để trống';
        isValid = false;
      } else if (!validateAge(form.ngaySinh)) {
        errors.ngaySinh = 'Khách hàng phải từ 10 tuổi trở lên';
        isValid = false;
      }

      if (!form.cccd?.trim()) {
        errors.cccd = 'CCCD không được để trống';
        isValid = false;
      } else if (!/^[0][0-9]{11}$/.test(form.cccd.trim())) {
        errors.cccd = 'CCCD phải là 12 số và bắt đầu bằng 0';
        isValid = false;
      }

      if (!form.sdt?.trim()) {
        errors.sdt = 'Số điện thoại không được để trống';
        isValid = false;
      } else {
        const cleanPhone = form.sdt.replace(/\s+/g, '');
        if (!/^[0-9]{10}$/.test(cleanPhone)) {
          errors.sdt = 'Số điện thoại phải là 10 số';
          isValid = false;
        }
      }

      if (!form.email?.trim()) {
        errors.email = 'Email không được để trống';
        isValid = false;
      } else if (!/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(form.email.trim())) {
        errors.email = 'Email không hợp lệ';
        isValid = false;
      }

      if (!props.isEdit) {
        if (!form.tenTaiKhoan?.trim()) {
          errors.tenTaiKhoan = 'Tên tài khoản không được để trống';
          isValid = false;
        }

        if (!form.matKhau?.trim()) {
          errors.matKhau = 'Mật khẩu không được để trống';
          isValid = false;
        } else if (form.matKhau.trim().length < 6) {
          errors.matKhau = 'Mật khẩu phải có ít nhất 6 ký tự';
          isValid = false;
        }

        if (!fileSelected.value && !imagePreview.value) {
          errors.hinh = 'Vui lòng chọn hình đại diện';
          isValid = false;
        }
      }

      return isValid;
    };

    const submitForm = async () => {
      if (!validateForm()) {
        const firstError = Object.values(errors).find((error) => error);
        showWarningMessage(firstError);
        return;
      }

      isSubmitting.value = true;
      showLoadingIndicator();

      try {
        const formData = new FormData();
        formData.append('hoTen', form.hoTen?.trim() || '');
        formData.append('gioiTinh', form.gioiTinh);
        formData.append('ngaySinh', form.ngaySinh);
        formData.append('diaChi', form.diaChi?.trim() || '');
        formData.append('cccd', form.cccd?.trim() || '');
        formData.append('sdt', form.sdt?.replace(/\s+/g, '') || '');
        formData.append('email', form.email?.trim() || '');

        if (!props.isEdit) {
          formData.append('tenTaiKhoan', form.tenTaiKhoan?.trim() || '');
          formData.append('matKhau', form.matKhau?.trim() || '');
        } else if (form.matKhau) {
          formData.append('matKhau', form.matKhau?.trim() || '');
        }

        if (fileSelected.value) {
          formData.append('hinhDaiDien', fileSelected.value);
        }

        if (props.isEdit) {
          formData.append('tinhTrang', form.tinhTrang);
          formData.append('isActive', form.isActive);
          await axios.put(`${apiUrl}/Customer/${props.customerId}`, formData);
        } else {
          await axios.post(`${apiUrl}/Customer`, formData);
        }

        Swal.close();
        console.log('Attempting to show success message'); // Debug
        await Swal.fire({
          title: 'Thành công!',
          text: props.isEdit ? 'Cập nhật khách hàng thành công!' : 'Thêm mới khách hàng thành công!',
          icon: 'success',
          confirmButtonText: 'OK',
          confirmButtonColor: '#4CAF50',
          customClass: {
            container: 'my-swal-container'
          }
        });
        console.log('Success message shown'); // Debug
        emit('submit-success');
      } catch (error) {
        Swal.close();
        console.error('Error in submitForm:', error); // Debug
        if (error.response?.data) {
          const errorMessage = error.response.data;
          if (errorMessage.includes('CCCD đã tồn tại')) {
            errors.cccd = 'CCCD đã tồn tại';
            showWarningMessage('CCCD đã tồn tại trong hệ thống!');
          } else if (errorMessage.includes('SĐT đã tồn tại')) {
            errors.sdt = 'Số điện thoại đã tồn tại';
            showWarningMessage('Số điện thoại đã tồn tại trong hệ thống!');
          } else if (errorMessage.includes('Email đã tồn tại')) {
            errors.email = 'Email đã tồn tại';
            showWarningMessage('Email đã tồn tại trong hệ thống!');
          } else if (errorMessage.includes('Tên tài khoản đã tồn tại')) {
            errors.tenTaiKhoan = 'Tên tài khoản đã tồn tại';
            showWarningMessage('Tên tài khoản đã tồn tại trong hệ thống!');
          } else {
            showErrorMessage(errorMessage || 'Đã xảy ra lỗi khi xử lý yêu cầu!');
          }
        } else {
          showErrorMessage('Không thể kết nối đến server. Vui lòng thử lại sau!');
        }
      } finally {
        isSubmitting.value = false;
      }
    };
    const cancelForm = () => {
      showConfirmDialog('Bạn có muốn hủy? Các thay đổi sẽ không được lưu!', () => {
        emit('cancel');
      });
    };

    const fetchCustomerData = async () => {
      if (props.isEdit && props.customerId) {
        try {
          showLoadingIndicator('Đang tải thông tin khách hàng...');
          const response = await axios.get(`${apiUrl}/Customer/${props.customerId}`);
          const customerData = response.data;

          form.hoTen = customerData.hoTen?.trim() || '';
          form.gioiTinh = customerData.gioiTinh || 'Nam';
          form.ngaySinh = customerData.ngaySinh
            ? new Date(customerData.ngaySinh).toISOString().split('T')[0]
            : '';
          form.diaChi = customerData.diaChi?.trim() || '';
          form.cccd = customerData.cccd?.trim() || '';
          form.sdt = formatPhoneNumber(customerData.sdt?.trim() || '');
          form.email = customerData.email?.trim() || '';
          form.tinhTrang = customerData.tinhTrang || 'Đang hoạt động';
          form.isActive = customerData.isActive ?? true;

          const imagePath = customerData.hinhDaiDien || customerData.hinh || '';
          if (imagePath) {
            if (imagePath.includes('AnhKhachHang')) {
              const fileName = imagePath.split('/').pop();
              imagePreview.value = `${apiUrl}/Customer/image/${fileName}`;
            } else {
              imagePreview.value = `${apiUrl}${imagePath.startsWith('/') ? '' : '/'}${imagePath}`;
            }
          } else {
            imagePreview.value = '';
          }

          Swal.close();
        } catch (error) {
          Swal.close();
          showErrorMessage('Không thể tải thông tin khách hàng. Vui lòng thử lại sau!');
        }
      }
    };

    onMounted(fetchCustomerData);

    return {
      form,
      errors,
      isSubmitting,
      imagePreview,
      handleFileUpload,
      handlePhoneBlur,
      submitForm,
      cancelForm,
      validateCCCD,
    };
  },
};
</script>

<style scoped>
/* Form container */
.customer-form {
  position: relative;
  width: 100%;
  max-width: 1000px;
  /* Giảm từ 1200px để phù hợp với StaffForm */
  margin: 1.5rem auto;
  background: #f9fafb;
  /* Sử dụng màu nền giống StaffForm */
  border-radius: 1rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  /* Shadow nhẹ hơn */
  max-height: 90vh;
  /* Giới hạn chiều cao để tránh tràn */
  overflow-y: auto;
  /* Thêm cuộn dọc */
  animation: formExpand 0.5s ease-out;
}

/* Thanh cuộn tùy chỉnh */
.customer-form::-webkit-scrollbar {
  width: 8px;
}

.customer-form::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 4px;
}

.customer-form::-webkit-scrollbar-thumb {
  background: #3b82f6;
  border-radius: 4px;
}

.customer-form::-webkit-scrollbar-thumb:hover {
  background: #2563eb;
}

@keyframes formExpand {
  from {
    transform: scale(0.95);
    opacity: 0;
  }

  to {
    transform: scale(1);
    opacity: 1;
  }
}

/* Form header */
.form-header {
  position: sticky;
  /* Cố định header khi cuộn */
  top: 0;
  z-index: 10;
  padding: 1rem 1.5rem;
  /* Giảm padding */
  background-color: rgb(194, 221, 245);
  /* Sử dụng gradient giống StaffForm */
  border-radius: 1rem 1rem 0 0;
  /* Bo góc chỉ phía trên */
}

.form-header h1 {
  font-family: 'Roboto', sans-serif;
  font-weight: 700;
  font-size: 1.5rem;
  /* Giảm kích thước font */
  color: white;
  margin: 0;
  text-align: center;
}

.form-accent-border {
  display: none;
  /* Loại bỏ form-accent-border để đơn giản hóa */
}

/* Form columns */
.form-columns {
  display: flex;
  gap: 1.5rem;
  /* Giảm gap */
  padding: 1.5rem;
  /* Giảm padding */
}

.left-column,
.right-column {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  /* Giảm khoảng cách giữa các form-group */
}

.left-column {
  border-right: none;
  /* Loại bỏ border-right để giống StaffForm */
}

/* Form group */
.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.section-title {
  font-weight: 600;
  font-size: 0.85rem;
  /* Giảm kích thước font */
  color: #374151;
  margin-bottom: 0.4rem;
}

.section-title::before {
  content: none;
  /* Loại bỏ thanh màu trước để giống StaffForm */
}

.required {
  color: #ef4444;
}

/* Input styling */
.info-input {
  padding: 0.6rem 0.8rem;
  /* Giảm padding */
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  /* Giảm border-radius */
  font-size: 0.9rem;
  /* Giảm kích thước font */
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.info-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.25);
  outline: none;
}

.date-input {
  cursor: pointer;
}

/* Image upload */
.image-container {
  position: relative;
  display: flex;
  justify-content: center;
}

.file-input {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  cursor: pointer;
  z-index: 10;
}

.image-preview {
  position: relative;
  width: 120px;
  /* Giảm kích thước ảnh để giống StaffForm */
  height: 160px;
  /* Điều chỉnh tỷ lệ */
  border-radius: 0.5rem;
  /* Giảm border-radius */
  border: 2px solid #3b82f6;
  overflow: hidden;
  transition: transform 0.2s ease;
  z-index: 5;
}

.image-preview:hover {
  transform: scale(1.05);
}

.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-preview.empty-preview {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #f3f4f6;
}

.placeholder-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.placeholder-icon {
  width: 40px;
  /* Giảm kích thước icon */
  height: 40px;
  background: #3b82f6;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 0.4rem;
}

.placeholder-icon span {
  color: white;
  font-size: 1.5rem;
}

.placeholder-text {
  color: #4b5563;
  font-size: 0.85rem;
}

.image-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background: rgba(59, 130, 246, 0.7);
  color: white;
  text-align: center;
  padding: 0.4rem;
  opacity: 0;
  transition: opacity 0.2s ease;
}

.image-preview:hover .image-overlay {
  opacity: 1;
}

/* Error message */
.error-message {
  color: #ef4444;
  font-size: 0.75rem;
  /* Giảm kích thước font */
  margin-top: 0.2rem;
}

/* Buttons */
.buttons {
  display: flex;
  justify-content: flex-end;
  gap: 0.8rem;
  padding: 1.5rem;
  border-top: 1px solid #e5e7eb;
  /* Sử dụng border-top giống StaffForm */
}

.btn-cancel,
.btn-submit {
  padding: 0.6rem 1.2rem;
  /* Giảm padding */
  border-radius: 0.375rem;
  font-size: 0.9rem;
  /* Giảm kích thước font */
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.2s ease;
}

.btn-cancel {
  background: #f3f4f6;
  border: 1px solid #d1d5db;
  color: #4b5563;
}

.btn-cancel:hover {
  background: #e5e7eb;
  transform: translateY(-2px);
}

.btn-submit {
  background: #3b82f6;
  color: white;
  border: none;
}

.btn-submit:hover {
  background: #2563eb;
  transform: translateY(-2px);
}

.btn-submit:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}

/* Responsive */
@media (max-width: 1024px) {
  .customer-form {
    max-width: 90%;
    margin: 1rem auto;
  }

  .form-columns {
    flex-direction: column;
    gap: 1rem;
  }

  .left-column {
    border-bottom: 1px solid #e5e7eb;
    /* Thay border-right bằng border-bottom */
    padding-right: 0;
    padding-bottom: 1rem;
  }

  .form-header h1 {
    font-size: 1.25rem;
  }
}

@media (max-width: 768px) {
  .customer-form {
    max-height: 85vh;
    /* Giảm chiều cao tối đa trên mobile */
  }

  .image-preview {
    width: 100px;
    /* Giảm kích thước ảnh */
    height: 130px;
  }

  .info-input {
    font-size: 0.85rem;
    padding: 0.5rem 0.7rem;
  }

  .section-title {
    font-size: 0.8rem;
  }

  .error-message {
    font-size: 0.7rem;
  }

  .placeholder-icon {
    width: 35px;
    height: 35px;
  }

  .placeholder-icon span {
    font-size: 1.2rem;
  }

  .placeholder-text {
    font-size: 0.8rem;
  }

  .buttons {
    flex-direction: column;
    gap: 0.5rem;
  }

  .btn-cancel,
  .btn-submit {
    width: 100%;
    font-size: 0.85rem;
    padding: 0.5rem;
  }
}

@media (max-width: 480px) {
  .customer-form {
    max-width: 95%;
    max-height: 80vh;
  }

  .form-header h1 {
    font-size: 1.1rem;
  }

  .form-columns {
    padding: 0.8rem;
  }
}

:global(.my-swal-container) {
  z-index: 10000 !important;
  /* Tăng z-index để khớp với StaffForm */
}

.swal2-container {
  display: block !important;
  visibility: visible !important;
  opacity: 1 !important;
}
</style>