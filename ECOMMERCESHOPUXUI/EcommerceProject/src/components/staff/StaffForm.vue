<template>
    <div class="staff-form-container">
        <div class="modal-overlay" v-if="loading">
            <div class="spinner"></div>
        </div>

        <div class="form-header" style="background-color: #4C7CF3;">
            <h2>{{ isEditing ? 'Cập Nhật Thông Tin Nhân Viên' : 'Thêm Nhân Viên Mới' }}</h2>
        </div>

        <form @submit.prevent="submitForm" class="staff-form">
            <div class="form-columns">
                <!-- Cột trái -->
                <div class="left-column">
                    <!-- Hình ảnh -->
                    <div class="form-group">
                        <label for="hinh">Hình ảnh:</label>
                        <div class="image-upload-container">
                            <img :src="imagePreview || 'https://via.placeholder.com/150'" alt="Hình đại diện"
                                class="image-preview" @error="handleImageError" />
                            <div class="image-upload-controls">
                                <label for="hinh-upload" class="upload-btn" style="color: white;">
                                    <i class="fas fa-upload"></i> Chọn ảnh
                                </label>
                                <input type="file" id="hinh-upload" accept="image/*" @change="onImageChange"
                                    style="display: none" />
                                <button type="button" @click="removeImage" class="remove-btn"
                                    v-if="imagePreview && imagePreview !== 'https://via.placeholder.com/150'">
                                    <i class="fas fa-trash"></i> Xóa
                                </button>
                            </div>
                            <span class="error-message" v-if="errors.hinh">{{ errors.hinh }}</span>
                        </div>
                    </div>

                    <!-- Họ tên -->
                    <div class="form-group">
                        <label for="hoTen">Họ tên:</label>
                        <input type="text" id="hoTen" v-model="formData.hoTen" @input="validateHoTen" required
                            placeholder="Nhập họ tên" />
                        <span class="error-message" v-if="errors.hoTen">{{ errors.hoTen }}</span>
                    </div>

                    <!-- Ngày sinh -->
                    <div class="form-group">
                        <label for="ngaySinh">Ngày sinh:</label>
                        <input type="date" id="ngaySinh" v-model="formData.ngaySinh" @input="validateNgaySinh" required
                            :max="maxDate" />
                        <span class="error-message" v-if="errors.ngaySinh">{{ errors.ngaySinh }}</span>
                    </div>

                    <!-- CCCD -->
                    <div class="form-group">
                        <label for="cccd">CCCD:</label>
                        <input type="text" id="cccd" v-model="formData.cccd" @input="validateCCCD" required
                            placeholder="Nhập số CCCD" />
                        <span class="error-message" v-if="errors.cccd">{{ errors.cccd }}</span>
                    </div>
                    <!-- Giới tính -->
                    <div class="form-group">
                        <label for="gioiTinh">Giới tính:</label>
                        <div class="radio-group">
                            <label class="radio-label">
                                <input type="radio" name="gioiTinh" value="Nam" v-model="formData.gioiTinh" required />
                                Nam
                            </label>
                            <label class="radio-label">
                                <input type="radio" name="gioiTinh" value="Nữ" v-model="formData.gioiTinh" />
                                Nữ
                            </label>
                        </div>
                        <span class="error-message" v-if="errors.gioiTinh">{{ errors.gioiTinh }}</span>
                    </div>

                    <!-- Số điện thoại -->
                    <div class="form-group">
                        <label for="sdt">Số điện thoại:</label>
                        <input type="text" id="sdt" v-model="formData.sdt" @input="validatePhone" required
                            placeholder="Nhập số điện thoại" />
                        <span class="error-message" v-if="errors.sdt">{{ errors.sdt }}</span>
                    </div>
                </div>

                <!-- Cột phải -->
                <div class="right-column">
                    <!-- Ngày vào làm -->
                    <div class="form-group">
                        <label for="ngayVaoLam">Ngày vào làm:</label>
                        <input type="date" id="ngayVaoLam" v-model="formData.ngayVaoLam" @input="validateNgayVaoLam"
                            required :max="currentDate" />
                        <span class="error-message" v-if="errors.ngayVaoLam">{{ errors.ngayVaoLam }}</span>
                    </div>

                    <!-- Địa chỉ -->
                    <div class="form-group">
                        <label for="diaChi">Địa chỉ:</label>
                        <input type="text" id="diaChi" v-model="formData.diaChi" @input="validateDiaChi" required
                            placeholder="Nhập địa chỉ" />
                        <span class="error-message" v-if="errors.diaChi">{{ errors.diaChi }}</span>
                    </div>

                    <!-- Email -->
                    <div class="form-group">
                        <label for="email">Email:</label>
                        <input type="email" id="email" v-model="formData.email" @input="validateEmail" required
                            placeholder="Nhập email" />
                        <span class="error-message" v-if="errors.email">{{ errors.email }}</span>
                    </div>

                    <!-- Tên tài khoản -->
                    <div class="form-group">
                        <label for="tenTaiKhoan">Tên tài khoản:</label>
                        <input type="text" id="tenTaiKhoan" v-model="formData.tenTaiKhoan" @input="validateTenTaiKhoan"
                            :required="!isEditing" placeholder="Nhập tên tài khoản" :readonly="isEditing"
                            :class="{ 'readonly-input': isEditing }" />
                        <span class="error-message" v-if="errors.tenTaiKhoan">{{ errors.tenTaiKhoan }}</span>
                    </div>

                    <!-- Mật khẩu - Chỉ hiển thị khi thêm mới -->
                    <div class="form-group password-group" v-if="!isEditing">
                        <label for="matKhau">Mật khẩu:</label>
                        <div class="password-input-container">
                            <input :type="showPassword ? 'text' : 'password'" id="matKhau" v-model="formData.matKhau"
                                @input="validateMatKhau" required 
                                placeholder="Nhập mật khẩu"
                                class="password-input" />
                            <button type="button" class="toggle-password-btn" @click="showPassword = !showPassword">
                                <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                            </button>
                        </div>
                        <span class="error-message" v-if="errors.matKhau">{{ errors.matKhau }}</span>
                    </div>

                    <div class="form-group">
                        <label for="tinhTrang">Tình trạng:</label>
                        <select id="tinhTrang" v-model="formData.tinhTrang" @change="validateTinhTrang" required
                            :disabled="loading">
                            <option value="">-- Chọn tình trạng --</option>
                            <option value="Đang hoạt động">Đang hoạt động</option>
                            <option v-if="isEditing" value="Đã tạm khóa">Đã tạm khóa</option>
                        </select>
                        <span class="error-message" v-if="errors.tinhTrang">{{ errors.tinhTrang }}</span>
                    </div>
                    <!-- Chức vụ -->
                    <div class="form-group">
                        <label for="chucVu">Chức vụ:</label>
                        <select id="chucVu" v-model="formData.maChucVu" @change="validateChucVu" required
                            :disabled="loading">
                            <option value="">-- Chọn chức vụ --</option>
                            <option v-for="chucVu in chucvus" :key="chucVu.maChucVu" :value="chucVu.maChucVu">
                                {{ chucVu.tenChucVu }}
                            </option>
                        </select>
                        <span class="error-message" v-if="errors.maChucVu">{{ errors.maChucVu }}</span>
                    </div>
                </div>
            </div>

            <!-- Form actions -->
            <div class="form-actions">
                <button type="button" @click="resetForm" class="cancel-btn">Hủy</button>
                <button type="submit" class="submit-btn" :disabled="loading">
                    {{ isEditing ? 'Cập Nhật' : 'Thêm Mới' }}
                </button>
            </div>
        </form>
    </div>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue';
import axios from 'axios';
import Swal from 'sweetalert2';
import { useRouter } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
export default {
    name: 'StaffForm',
    props: {
        staffId: {
            type: [Number, String],
            default: null,
        },
    },
    emits: ['submit-success', 'cancel'],
    setup(props, { emit }) {
        const apiUrl = ref(GetApiUrl());
        const loading = ref(false);
        const imagePreview = ref(null);
        const imageFile = ref(null);
        const showPassword = ref(false);
        const originalStaffData = ref(null);
        const chucvus = ref([]);
        const accessToken = ref(Cookies.get('accessToken'))
        const refreshToken = ref(Cookies.get('refreshToken'))
        const router = useRouter()
        const readToken = ref({})
        
        // Initialize form data
        const formData = ref({
            maNV: null,
            hoTen: '',
            gioiTinh: 'Nam',
            ngaySinh: '',
            cccd: '',
            diaChi: '',
            sdt: '',
            email: '',
            maChucVu: '',
            ngayVaoLam: '',
            tinhTrang: 'Đang hoạt động',
            matKhau: '',
            hinh: null,
            tenTaiKhoan: '',
        });

        // Initialize validation errors object
        const errors = ref({});

        // Check if editing or adding new
        const isEditing = computed(() => props.staffId !== null);

        // Set maximum date for birth date (18+ years old) and join date (not in future)
        const maxDate = computed(() => {
            const date = new Date();
            date.setFullYear(date.getFullYear() - 18);
            return date.toISOString().split('T')[0];
        });

        const currentDate = computed(() => {
            return new Date().toISOString().split('T')[0];
        });

        // Show loading indicator
        const showLoadingIndicator = (message = 'Đang xử lý...') => {
            Swal.fire({
                title: message,
                allowOutsideClick: false,
                showConfirmButton: false,
                customClass: {
                    container: 'my-swal-container'
                },
                willOpen: () => {
                    Swal.showLoading();
                },
            });
        };

        // Fetch list of positions
        const fetchChucvus = async () => {
            try {
                const validatetoken = await validateToken(accessToken.value, refreshToken.value)
                if (validatetoken.isValid == false) {
                    router.push('/Login')
                    return
                }
                accessToken.value = validatetoken.newAccessToken
                const response = await axios.get(`${apiUrl.value}/api/Staff/chucvus`, {
                    headers: { 'Authorization': 'Bearer ' + accessToken.value }
                });
                chucvus.value = response.data;
            } catch (error) {
                console.error('Error fetching positions:', error);
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Không thể lấy danh sách chức vụ. Vui lòng thử lại sau.',
                    icon: 'error',
                    confirmButtonColor: '#f44336',
                    customClass: {
                        container: 'my-swal-container'
                    }
                });
            }
        };

        // FIXED: Date formatting function to handle timezone properly
        const formatDateForInput = (dateString) => {
            if (!dateString) return '';
            try {
                // Create date object and format it properly
                const date = new Date(dateString);
                // Extract year, month, day without timezone conversion
                const year = date.getFullYear();
                const month = String(date.getMonth() + 1).padStart(2, '0');
                const day = String(date.getDate()).padStart(2, '0');
                return `${year}-${month}-${day}`;
            } catch (error) {
                console.error('Date formatting error:', error);
                return '';
            }
        };

        // Fetch staff data if editing
        const fetchStaffData = async () => {
            if (!props.staffId) return;
            try {
                const validatetoken = await validateToken(accessToken.value, refreshToken.value)
                if (validatetoken.isValid == false) {
                    router.push('/Login')
                    return
                }
                accessToken.value = validatetoken.newAccessToken
                loading.value = true;
                showLoadingIndicator('Đang tải thông tin nhân viên...');
                const response = await axios.get(`${apiUrl.value}/api/Staff/${props.staffId}`, {
                    headers: {'Authorization': 'Bearer ' + accessToken.value}
                });
                const staffData = response.data;

                // Save original data without modification
                originalStaffData.value = {
                    ...staffData,
                    hoTen: staffData.hoTen || '',
                    cccd: staffData.cccd || '',
                    diaChi: staffData.diaChi || '',
                    sdt: staffData.sdt || '',
                    email: staffData.email || '',
                    tenTaiKhoan: staffData.tenTaiKhoan || '',
                    ngaySinh: staffData.ngaySinh,
                    ngayVaoLam: staffData.ngayVaoLam
                };

                // FIXED: Proper date handling - use the fixed formatting function
                formData.value = {
                    maNV: staffData.maNV,
                    hoTen: staffData.hoTen || '',
                    gioiTinh: staffData.gioiTinh || 'Nam',
                    ngaySinh: formatDateForInput(staffData.ngaySinh),
                    cccd: staffData.cccd || '',
                    diaChi: staffData.diaChi || '',
                    sdt: staffData.sdt || '',
                    email: staffData.email || '',
                    maChucVu: staffData.maChucVu || '',
                    ngayVaoLam: formatDateForInput(staffData.ngayVaoLam),
                    tinhTrang: staffData.tinhTrang || 'Đang hoạt động',
                    matKhau: '', // Empty password for editing
                    hinh: staffData.hinh || null,
                    tenTaiKhoan: staffData.tenTaiKhoan || ''
                };

                if (staffData.hinh) {
                    imagePreview.value = getImageUrl(staffData.hinh);
                }
                Swal.close();
            } catch (error) {
                console.error('Error fetching staff data:', error);
                Swal.close();
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Không thể lấy thông tin nhân viên. Vui lòng thử lại sau.',
                    icon: 'error',
                    confirmButtonColor: '#f44336',
                    customClass: {
                        container: 'my-swal-container'
                    }
                });
            } finally {
                loading.value = false;
            }
        };

        const handleImageError = (event) => {
            event.target.src = 'https://via.placeholder.com/150';
        };

        const onImageChange = (event) => {
            const file = event.target.files[0];
            if (!file) return;
            const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
            const maxSize = 5 * 1024 * 1024;
            if (!allowedTypes.includes(file.type)) {
                errors.value.hinh = 'Vui lòng chọn file ảnh (JPEG, PNG, GIF, WEBP).';
                return;
            }
            if (file.size > maxSize) {
                errors.value.hinh = 'Kích thước ảnh không được vượt quá 5MB.';
                return;
            }
            delete errors.value.hinh;
            imageFile.value = file;
            imagePreview.value = URL.createObjectURL(file);
        };

        const removeImage = () => {
            imageFile.value = null;
            imagePreview.value = null;
            formData.value.hinh = null;
            if (!isEditing.value) {
                errors.value.hinh = 'Hình ảnh là bắt buộc';
            }
        };

        const submitForm = async () => {
            const isValid = await validateForm();
            
            if (!isValid) {
                const firstError = document.querySelector('.error-message');
                if (firstError) {
                    firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    Swal.fire({
                        title: 'Cảnh báo!',
                        text: firstError.textContent,
                        icon: 'warning',
                        confirmButtonColor: '#ff9800',
                        customClass: {
                            container: 'my-swal-container'
                        }
                    });
                }
                return;
            }

            loading.value = true;
            showLoadingIndicator();

            try {
                const formDataToSend = new FormData();

                // Add all form data to formDataToSend
                for (const key in formData.value) {
                    if (key !== 'hinh' && formData.value[key] !== null && formData.value[key] !== '') {
                        // Skip username when updating
                        if (isEditing.value && key === 'tenTaiKhoan') continue;
                        
                        // FIXED: For password - only send if not empty (for editing)
                        if (key === 'matKhau') {
                            const trimmedPassword = formData.value[key].trim();
                            if (trimmedPassword !== '') {
                                formDataToSend.append(key, trimmedPassword);
                            }
                            // If password is empty when editing, don't send it (keep existing password)
                        } else {
                            formDataToSend.append(key, formData.value[key]);
                        }
                    }
                }

                // Only add new image if user has selected a new image
                if (imageFile.value) {
                    formDataToSend.append('hinhDaiDien', imageFile.value);
                }
                
                const validatetoken = await validateToken(accessToken.value, refreshToken.value);
                if (validatetoken.isValid == false) {
                    router.push('/Login');
                    return;
                }
                accessToken.value = validatetoken.newAccessToken;
                
                let response;
                if (isEditing.value) {
                    response = await axios.put(`${apiUrl.value}/api/Staff/${props.staffId}`, formDataToSend, {
                        headers: { 
                            'Content-Type': 'multipart/form-data', 
                            'Authorization': 'Bearer ' + accessToken.value 
                        },
                    });
                } else {
                    response = await axios.post(`${apiUrl.value}/api/Staff`, formDataToSend, {
                        headers: { 
                            'Content-Type': 'multipart/form-data', 
                            'Authorization': 'Bearer ' + accessToken.value 
                        },
                    });
                }

                Swal.close();
                await Swal.fire({
                    title: 'Thành công!',
                    text: response.data.message || (isEditing.value ? 'Cập nhật thông tin nhân viên thành công.' : 'Thêm nhân viên mới thành công.'),
                    icon: 'success',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#4CAF50',
                    customClass: {
                        container: 'my-swal-container'
                    }
                });
                
                emit('submit-success', response.data);
                if (!isEditing.value) resetForm();
            } catch (error) {
                Swal.close();
                console.error('Error saving staff data:', error);
                let errorMessage = error.response?.data?.message || 'Không thể lưu thông tin nhân viên. Vui lòng thử lại sau.';

                // Handle specific API errors
                if (errorMessage.includes('CCCD đã tồn tại')) {
                    errors.value.cccd = 'CCCD đã tồn tại';
                    errorMessage = 'CCCD đã tồn tại trong hệ thống!';
                } else if (errorMessage.includes('SĐT đã tồn tại')) {
                    errors.value.sdt = 'Số điện thoại đã tồn tại';
                    errorMessage = 'Số điện thoại đã tồn tại trong hệ thống!';
                } else if (errorMessage.includes('Email đã tồn tại')) {
                    errors.value.email = 'Email đã tồn tại';
                    errorMessage = 'Email đã tồn tại trong hệ thống!';
                }

                Swal.fire({
                    title: 'Lỗi!',
                    text: errorMessage,
                    icon: 'error',
                    confirmButtonColor: '#f44336',
                    customClass: {
                        container: 'my-swal-container'
                    }
                });
            } finally {
                loading.value = false;
            }
        };

        const resetForm = () => {
            formData.value = {
                maNV: null,
                hoTen: '',
                gioiTinh: 'Nam',
                ngaySinh: '',
                cccd: '',
                diaChi: '',
                sdt: '',
                email: '',
                maChucVu: '',
                ngayVaoLam: '',
                tinhTrang: 'Đang hoạt động',
                matKhau: '',
                hinh: null,
                tenTaiKhoan: '',
            };
            imageFile.value = null;
            imagePreview.value = null;
            errors.value = {};
            originalStaffData.value = null;
            emit('cancel');
        };

        const validateHoTen = () => {
            // Don't trim to preserve spaces in names
            if (!formData.value.hoTen) {
                errors.value.hoTen = 'Họ tên không được để trống';
            } else {
                delete errors.value.hoTen;
            }
        };

        const validateNgaySinh = () => {
            if (!formData.value.ngaySinh) {
                errors.value.ngaySinh = 'Ngày sinh không được để trống';
            } else {
                const birthDate = new Date(formData.value.ngaySinh);
                const today = new Date();
                let age = today.getFullYear() - birthDate.getFullYear();
                const m = today.getMonth() - birthDate.getMonth();
                if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
                    age--;
                }
                if (age < 18) {
                    errors.value.ngaySinh = 'Nhân viên phải đủ 18 tuổi trở lên';
                } else {
                    delete errors.value.ngaySinh;
                }
            }
        };

        const validateCCCD = async () => {
            const cccdRegex = /^[0][0-9]{11}$/;
            const trimmedCCCD = (formData.value.cccd || '').trim();
            formData.value.cccd = trimmedCCCD;

            if (!formData.value.cccd) {
                errors.value.cccd = 'CCCD không được để trống';
                return false;
            } else if (!cccdRegex.test(formData.value.cccd)) {
                errors.value.cccd = 'CCCD phải có đúng 12 số và bắt đầu bằng 0';
                return false;
            } else if (!isEditing.value || (isEditing.value && trimmedCCCD !== originalStaffData.value?.cccd?.trim())) {
                try {
                    const validatetoken = await validateToken(accessToken.value, refreshToken.value);
                    if (validatetoken.isValid == false) {
                        router.push('/Login');
                        return false;
                    }
                    accessToken.value = validatetoken.newAccessToken;

                    const response = await axios.get(`${apiUrl.value}/api/Staff/check-cccd`, {
                        params: {
                            cccd: trimmedCCCD,
                            maNV: isEditing.value ? props.staffId : null
                        },
                        headers: { 'Authorization': 'Bearer ' + accessToken.value }
                    });
                    if (response.data) {
                        errors.value.cccd = 'CCCD đã tồn tại';
                        return false;
                    } else {
                        delete errors.value.cccd;
                        return true;
                    }
                } catch (error) {
                    console.error('Error checking CCCD:', error);
                    errors.value.cccd = 'Không thể kiểm tra CCCD. Vui lòng thử lại sau.';
                    return false;
                }
            } else {
                delete errors.value.cccd;
                return true;
            }
        };

        const validatePhone = async () => {
            // Phone must start with 0 and have exactly 10 digits
            const phoneRegex = /^0[0-9]{9}$/;
            const trimmedSDT = (formData.value.sdt || '').trim();
            formData.value.sdt = trimmedSDT;

            if (!formData.value.sdt) {
                errors.value.sdt = 'Số điện thoại không được để trống';
                return false;
            } else if (!phoneRegex.test(formData.value.sdt)) {
                errors.value.sdt = 'Số điện thoại phải có đúng 10 số và bắt đầu bằng số 0';
                return false;
            } else if (!isEditing.value || (isEditing.value && trimmedSDT !== originalStaffData.value?.sdt?.trim())) {
                try {
                    const validatetoken = await validateToken(accessToken.value, refreshToken.value);
                    if (validatetoken.isValid == false) {
                        router.push('/Login');
                        return false;
                    }
                    accessToken.value = validatetoken.newAccessToken;

                    const response = await axios.get(`${apiUrl.value}/api/Staff/check-sdt`, {
                        params: {
                            sdt: trimmedSDT,
                            maNV: isEditing.value ? props.staffId : null
                        },
                        headers: { 'Authorization': 'Bearer ' + accessToken.value }
                    });
                    if (response.data) {
                        errors.value.sdt = 'Số điện thoại đã tồn tại';
                        return false;
                    } else {
                        delete errors.value.sdt;
                        return true;
                    }
                } catch (error) {
                    console.error('Error checking phone:', error);
                    errors.value.sdt = 'Không thể kiểm tra SĐT. Vui lòng thử lại sau.';
                    return false;
                }
            } else {
                delete errors.value.sdt;
                return true;
            }
        };

        const validateEmail = async () => {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            const trimmedEmail = (formData.value.email || '').trim();
            formData.value.email = trimmedEmail;

            if (!formData.value.email) {
                errors.value.email = 'Email không được để trống';
                return false;
            } else if (!emailRegex.test(formData.value.email)) {
                errors.value.email = 'Email không hợp lệ';
                return false;
            } else if (!isEditing.value || (isEditing.value && trimmedEmail !== originalStaffData.value?.email?.trim())) {
                try {
                    const validatetoken = await validateToken(accessToken.value, refreshToken.value);
                    if (validatetoken.isValid == false) {
                        router.push('/Login');
                        return false;
                    }
                    accessToken.value = validatetoken.newAccessToken;

                    const response = await axios.get(`${apiUrl.value}/api/Staff/check-email`, {
                        params: {
                            email: trimmedEmail,
                            maNV: isEditing.value ? props.staffId : null
                        },
                        headers: { 'Authorization': 'Bearer ' + accessToken.value }
                    });
                    if (response.data) {
                        errors.value.email = 'Email đã tồn tại';
                        return false;
                    } else {
                        delete errors.value.email;
                        return true;
                    }
                } catch (error) {
                    console.error('Error checking email:', error);
                    errors.value.email = 'Không thể kiểm tra email. Vui lòng thử lại sau.';
                    return false;
                }
            } else {
                delete errors.value.email;
                return true;
            }
        };

        const validateDiaChi = () => {
            // Don't trim to preserve spaces in addresses
            if (!formData.value.diaChi) {
                errors.value.diaChi = 'Địa chỉ không được để trống';
            } else {
                delete errors.value.diaChi;
            }
        };

        const validateChucVu = () => {
            if (!formData.value.maChucVu) {
                errors.value.maChucVu = 'Vui lòng chọn chức vụ';
            } else {
                delete errors.value.maChucVu;
            }
        };

        const validateNgayVaoLam = () => {
            if (!formData.value.ngayVaoLam) {
                errors.value.ngayVaoLam = 'Ngày vào làm không được để trống';
            } else {
                const joinDate = new Date(formData.value.ngayVaoLam);
                const today = new Date();
                if (joinDate > today) {
                    errors.value.ngayVaoLam = 'Ngày vào làm không được ở tương lai';
                } else {
                    delete errors.value.ngayVaoLam;
                }
            }
        };

        const validateTinhTrang = () => {
            if (!formData.value.tinhTrang) {
                errors.value.tinhTrang = 'Vui lòng chọn tình trạng';
            } else if (!['Đang hoạt động', 'Đã tạm khóa'].includes(formData.value.tinhTrang)) {
                errors.value.tinhTrang = 'Tình trạng không hợp lệ';
            } else {
                delete errors.value.tinhTrang;
            }
        };

        const validateMatKhau = () => {
            // Only validate password if it's provided (not empty)
            if (formData.value.matKhau && formData.value.matKhau.trim() !== '') {
                const trimmedPassword = formData.value.matKhau.trim();
                if (trimmedPassword.length < 6) {
                    errors.value.matKhau = 'Mật khẩu phải có ít nhất 6 ký tự';
                } else {
                    delete errors.value.matKhau;
                }
            } else if (!isEditing.value) {
                // Only require password for new staff
                errors.value.matKhau = 'Mật khẩu không được để trống';
            } else {
                // For editing, if password is empty, don't validate (keep existing password)
                delete errors.value.matKhau;
            }
        };

        const validateHinh = () => {
            if (!isEditing.value && !imageFile.value && !formData.value.hinh) {
                errors.value.hinh = 'Hình ảnh là bắt buộc';
            } else {
                delete errors.value.hinh;
            }
        };

        const validateTenTaiKhoan = async () => {
            const trimmedTenTaiKhoan = (formData.value.tenTaiKhoan || '').trim();
            formData.value.tenTaiKhoan = trimmedTenTaiKhoan;

            if (!isEditing.value) {
                // Only validate when adding new staff
                if (!formData.value.tenTaiKhoan || formData.value.tenTaiKhoan.length < 4) {
                    errors.value.tenTaiKhoan = 'Tên tài khoản không được để trống và phải có ít nhất 4 ký tự';
                    return false;
                } else {
                    try {
                        const validatetoken = await validateToken(accessToken.value, refreshToken.value);
                        if (validatetoken.isValid == false) {
                            router.push('/Login');
                            return false;
                        }
                        accessToken.value = validatetoken.newAccessToken;

                        const response = await axios.get(`${apiUrl.value}/api/Staff/check-ten-tai-khoan`, {
                            params: {
                                tenTaiKhoan: trimmedTenTaiKhoan,
                                maNV: null // No maNV needed when adding new
                            },
                            headers: { 'Authorization': 'Bearer ' + accessToken.value }
                        });
                        if (response.data) {
                            errors.value.tenTaiKhoan = 'Tên tài khoản đã tồn tại';
                            return false;
                        } else {
                            delete errors.value.tenTaiKhoan;
                            return true;
                        }
                    } catch (error) {
                        console.error('Error checking username:', error);
                        errors.value.tenTaiKhoan = 'Không thể kiểm tra tên tài khoản. Vui lòng thử lại sau.';
                        return false;
                    }
                }
            } else {
                // When editing, don't validate username duplication
                delete errors.value.tenTaiKhoan;
                return true;
            }
        };

        const validateForm = async () => {
            // Clear all errors first
            errors.value = {};

            // Validate synchronous fields first
            validateHoTen();
            validateNgaySinh();
            validateDiaChi();
            validateChucVu();
            validateNgayVaoLam();
            validateTinhTrang();
            validateMatKhau();
            validateHinh();

            if (!formData.value.gioiTinh) {
                errors.value.gioiTinh = 'Vui lòng chọn giới tính';
            }

            // Validate asynchronous fields
            const cccdValid = await validateCCCD();
            const phoneValid = await validatePhone();
            const emailValid = await validateEmail();
            const usernameValid = await validateTenTaiKhoan();

            // Return true only if all validations pass
            return Object.keys(errors.value).length === 0 &&
                cccdValid && phoneValid && emailValid && usernameValid;
        };

        const getImageUrl = (relativePath) => {
            if (!relativePath) return 'https://via.placeholder.com/150';
            return `${apiUrl.value}${relativePath}`;
        };

        watch(() => props.staffId, (newValue) => {
            if (newValue) {
                fetchStaffData();
            } else {
                resetForm();
            }
        });

        onMounted(() => {
            fetchChucvus();
            if (props.staffId) {
                fetchStaffData();
            }
        });

        return {
            formData,
            errors,
            loading,
            isEditing,
            imagePreview,
            maxDate,
            currentDate,
            showPassword,
            chucvus,
            onImageChange,
            removeImage,
            submitForm,
            resetForm,
            validateEmail,
            validatePhone,
            validateCCCD,
            validateHoTen,
            validateNgaySinh,
            validateDiaChi,
            validateChucVu,
            validateNgayVaoLam,
            validateTinhTrang,
            validateMatKhau,
            validateTenTaiKhoan,
            handleImageError,
        };
    },
};
</script>

<style scoped>
.staff-form-container {
    background-color: #f9fafb;
    border-radius: 1rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    margin: 1.5rem auto;
    max-width: 1000px;
    max-height: 90vh;
    overflow-y: auto;
    position: relative;
    padding: 0;
}

/* Custom scrollbar styling */
.staff-form-container::-webkit-scrollbar {
    width: 8px;
}

.staff-form-container::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

.staff-form-container::-webkit-scrollbar-thumb {
    background: #f1f1f1;
    border-radius: 4px;
}

.staff-form-container::-webkit-scrollbar-thumb:hover {
    background: #f1f1f1;
}

.form-header {
    background-color: rgb(194, 221, 245);
    color: white;
    padding: 1rem 1.5rem;
    box-shadow: 0 4px 6px rgba(59, 130, 246, 0.3);
    position: sticky;
    top: 0;
    z-index: 10;
}

.form-header h2 {
    margin: 0;
    font-size: 1.5rem;
    font-weight: 700;
    color: white;
}

.staff-form {
    padding: 1.5rem;
}

.form-columns {
    display: flex;
    gap: 1.5rem;
    margin-bottom: 1.5rem;
}

.left-column,
.right-column {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
}

.form-group label {
    font-weight: 600;
    color: #374151;
    font-size: 0.85rem;
}

.form-group input[type="text"],
.form-group input[type="email"],
.form-group input[type="date"],
.form-group select {
    padding: 0.6rem 0.8rem;
    border: 1px solid #d1d5db;
    border-radius: 0.375rem;
    font-size: 0.9rem;
    transition: all 0.2s ease;
}

.form-group input:focus,
.form-group select:focus {
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.25);
    outline: none;
}

.error-message {
    color: #ef4444;
    font-size: 0.75rem;
    margin-top: 0.2rem;
}

.radio-group {
    display: flex;
    gap: 1.5rem;
}

.radio-label {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    cursor: pointer;
    font-weight: normal;
    font-size: 0.9rem;
}

.radio-label input[type="radio"] {
    margin: 0;
    cursor: pointer;
}

.image-upload-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.8rem;
}

.image-preview {
    width: 120px;
    height: 120px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid #3b82f6;
    box-shadow: 0 3px 5px rgba(0, 0, 0, 0.1);
}

.image-upload-controls {
    display: flex;
    gap: 0.8rem;
}

.upload-btn {
    background-color: #3b82f6;
    color: white;
    padding: 0.4rem 0.8rem;
    border-radius: 0.375rem;
    cursor: pointer;
    font-weight: 500;
    font-size: 0.85rem;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
}

.upload-btn:hover {
    background-color: #2563eb;
}

.remove-btn {
    background-color: #ef4444;
    color: white;
    padding: 0.4rem 0.8rem;
    border-radius: 0.375rem;
    border: none;
    cursor: pointer;
    font-weight: 500;
    font-size: 0.85rem;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
}

.remove-btn:hover {
    background-color: #dc2626;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 0.8rem;
    margin-top: 1.5rem;
    border-top: 1px solid #e5e7eb;
    padding-top: 1.5rem;
}

.cancel-btn {
    background-color: #f3f4f6;
    color: #4b5563;
    border: 1px solid #d1d5db;
    padding: 0.6rem 1.2rem;
    border-radius: 0.375rem;
    font-weight: 600;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.2s ease;
}

.cancel-btn:hover {
    background-color: #e5e7eb;
}

.submit-btn {
    background-color: #3b82f6;
    color: white;
    border: none;
    padding: 0.6rem 1.2rem;
    border-radius: 0.375rem;
    font-weight: 600;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.2s ease;
}

.submit-btn:hover {
    background-color: #2563eb;
}

.submit-btn:disabled {
    background-color: #93c5fd;
    cursor: not-allowed;
}

.modal-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(255, 255, 255, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 10;
}

.spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #3b82f6;
    border-radius: 50%;
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}

.password-group {
    position: relative;
}

.password-input-container {
    display: flex;
    align-items: center;
    gap: 0.4rem;
}

.password-input {
    padding: 0.6rem 0.8rem;
    border: 1px solid #d1d5db;
    border-radius: 0.375rem;
    font-size: 0.9rem;
    flex: 1;
    transition: all 0.2s ease;
}

.password-input:focus {
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.25);
    outline: none;
}

.toggle-password-btn {
    background-color: #3b82f6;
    color: white;
    padding: 0.4rem 0.8rem;
    border: none;
    border-radius: 0.375rem;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.2rem;
    transition: all 0.2s ease;
}

.toggle-password-btn:hover {
    background-color: #2563eb;
}

.readonly-input {
    background-color: #e5e7eb;
    cursor: not-allowed;
    color: #6b7280;
}

/* Responsive design */
@media (max-width: 1024px) {
    .staff-form-container {
        max-width: 90%;
        margin: 1rem auto;
    }

    .form-columns {
        flex-direction: column;
        gap: 1rem;
    }

    .form-header h2 {
        font-size: 1.25rem;
    }

    .staff-form {
        padding: 1rem;
    }
}

@media (max-width: 768px) {
    .staff-form-container {
        max-height: 85vh;
    }

    .image-preview {
        width: 100px;
        height: 100px;
    }

    .form-group input[type="text"],
    .form-group input[type="email"],
    .form-group input[type="date"],
    .form-group select {
        font-size: 0.85rem;
        padding: 0.5rem 0.7rem;
    }

    .form-group label {
        font-size: 0.8rem;
    }

    .error-message {
        font-size: 0.7rem;
    }

    .upload-btn,
    .remove-btn {
        font-size: 0.8rem;
        padding: 0.35rem 0.7rem;
    }

    .password-input-container {
        flex-direction: column;
        gap: 0.3rem;
    }

    .toggle-password-btn {
        width: 100%;
        font-size: 0.85rem;
        padding: 0.35rem;
    }

    .form-actions {
        flex-direction: column;
        gap: 0.5rem;
    }

    .cancel-btn,
    .submit-btn {
        width: 100%;
        font-size: 0.85rem;
        padding: 0.5rem;
    }
}

@media (max-width: 480px) {
    .staff-form-container {
        max-width: 95%;
        max-height: 80vh;
    }

    .form-header h2 {
        font-size: 1.1rem;
    }

    .staff-form {
        padding: 0.8rem;
    }

    .radio-group {
        flex-direction: column;
        gap: 0.5rem;
    }
}

:global(.my-swal-container) {
    z-index: 10000 !important;
}
</style>