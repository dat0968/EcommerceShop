<script setup>
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../constants/api.js';
import Cookies from 'js-cookie';

const route = useRoute();
const router = useRouter();
// Gán email từ query parameter khi component mount
const email = ref(route.query.email || '');
const newPassword = ref('');
const confirmPassword = ref('');
const loginAfterReset = ref(false);
const message = ref('');
const success = ref(false);
const getApiUrl = GetApiUrl();

// Log để debug
onMounted(() => {
  console.log('Email from query:', route.query.email);
  if (!email.value) {
    console.warn('Email is empty, redirecting to forgot password');
    router.push('/ForgotPasswordCustomer');
  }
});

// Hàm kiểm tra mật khẩu trùng với mật khẩu cũ
const checkPassword = async () => {
  try {
    const response = await fetch(`${getApiUrl}/api/Account/CheckPassword?email=${encodeURIComponent(email.value.trim())}&password=${encodeURIComponent(newPassword.value)}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Lỗi trong checkPassword:', {
      message: error.message,
      name: error.name,
      stack: error.stack,
    });
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: error.message || 'Có lỗi xảy ra khi kiểm tra mật khẩu!',
      confirmButtonText: 'OK',
    });
    return { success: false, message: error.message };
  }
};

const handleResetPassword = async () => {
  message.value = '';
  success.value = false;

  // Kiểm tra email có giá trị không
  if (!email.value.trim()) {
    success.value = false;
    message.value = 'Email không được để trống!';
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: message.value,
      confirmButtonText: 'OK',
    });
    return;
  }

  // Kiểm tra mật khẩu xác nhận
  if (newPassword.value !== confirmPassword.value) {
    success.value = false;
    message.value = 'Mật khẩu xác nhận không khớp!';
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: message.value,
      confirmButtonText: 'OK',
    });
    return;
  }

  // Kiểm tra mật khẩu trùng với mật khẩu cũ
  const passwordCheckResult = await checkPassword();
  if (!passwordCheckResult.success) {
    success.value = false;
    message.value = passwordCheckResult.message || 'Mật khẩu trùng với mật khẩu cũ!';
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: message.value,
      confirmButtonText: 'OK',
    });
    return;
  }

  // Tiếp tục đặt lại mật khẩu nếu mật khẩu hợp lệ
  try {
    console.log('Sending reset password request with loginAfterReset:', loginAfterReset.value);
    const response = await fetch(`${getApiUrl}/api/Account/ResetPasswordCustomer`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: email.value.trim(),
        newPassword: newPassword.value,
        loginAfterReset: loginAfterReset.value,
      }),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();
    console.log('API response:', data);

    if (data.success) {
      success.value = true;
      message.value = data.message || 'Đặt lại mật khẩu thành công!';
      if (loginAfterReset.value) {
        // Lưu token và chuyển hướng về '/' nếu chọn đăng nhập ngay
        if (data.data) {
          console.log('Login after reset is true, saving tokens and redirecting to /');
          Cookies.set('accessToken', data.data.accessToken, { expires: 3 / 24 });
          Cookies.set('refreshToken', data.data.refreshToken, { expires: 3 / 24 });
          await Swal.fire({
            icon: 'success',
            title: 'Thành công!',
            text: 'Đăng nhập thành công!',
            confirmButtonText: 'OK',
          });
          router.push('/');
        } else {
          console.warn('No data returned for loginAfterReset true');
          await Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: 'Không thể đăng nhập ngay, vui lòng thử lại.',
            confirmButtonText: 'OK',
          });
          router.push('/Login');
        }
      } else {
        // Chuyển hướng về '/Login' nếu không chọn đăng nhập ngay
        console.log('Login after reset is false, redirecting to /Login');
        await Swal.fire({
          icon: 'success',
          title: 'Thành công!',
          text: message.value,
          confirmButtonText: 'OK',
        });
        router.push('/Login');
      }
    } else {
      success.value = false;
      message.value = data.message || 'Có lỗi xảy ra, vui lòng thử lại!';
      await Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: message.value,
        confirmButtonText: 'OK',
      });
    }
  } catch (error) {
    console.error('Lỗi trong handleResetPassword:', {
      message: error.message,
      name: error.name,
      stack: error.stack,
    });
    success.value = false;
    message.value = error.message || 'Có lỗi xảy ra, vui lòng thử lại!';
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: message.value,
      confirmButtonText: 'OK',
    });
  }
};
</script>

<template>
  <div>
    <div class="xp-authenticate-bg"></div>
    <div id="xp-container" class="xp-container">
      <div class="container">
        <div class="row vh-100 align-items-center">
          <div class="col-lg-12">
            <div class="xp-auth-box">
              <div class="card">
                <div class="card-body">
                  <!-- <h3 class="text-center mt-0 m-b-15">
                    <a href="index.html" class="xp-web-logo">
                      <img src="../../assets/admin/images/logo.svg" height="40" alt="logo" />
                    </a>
                  </h3> -->
                  <div class="col-xl-3 col-lg-2"
                    style="width: 300px; margin-right: 50px;  display:block;margin:0 auto;">
                    <svg viewBox="0 0 700 250" role="img"
                      aria-label="Angel soft curvy logo with wings and animated gradient">
                      <defs>
                        <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                          <stop offset="20%" stop-color="#EC4E79">
                            <animate attributeName="stop-color" values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;"
                              dur="6s" repeatCount="indefinite" />
                          </stop>
                          <stop offset="40%" stop-color="#ABA2B7">
                            <animate attributeName="stop-color" values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;"
                              dur="6s" repeatCount="indefinite" />
                          </stop>
                          <stop offset="55%" stop-color="#5CCAE7">
                            <animate attributeName="stop-color" values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;"
                              dur="6s" repeatCount="indefinite" />
                          </stop>
                        </linearGradient>
                      </defs>

                      <!-- Left wing - smooth curves -->
                      <path class="wing left" d="M160 130 C110 90, 90 180, 150 170 C130 150, 140 110, 160 130 Z" />
                      <path class="wing left" d="M150 140 C120 120, 110 170, 150 160 C140 140, 130 120, 150 140 Z"
                        opacity="0.5" />

                      <!-- Right wing - smooth curves -->
                      <path class="wing right" d="M540 130 C590 90, 610 180, 550 170 C570 150, 560 110, 540 130 Z" />
                      <path class="wing right" d="M550 140 C580 120, 590 170, 550 160 C560 140, 570 120, 550 140 Z"
                        opacity="0.5" />

                      <!-- Angel text with soft cursive font -->
                        <RouterLink to="/" style="text-decoration: none;">
                      <text x="50%" y="60%" dominant-baseline="middle" text-anchor="middle" class="angel-text">
                        Angel
                      </text>
                      </RouterLink>
                    </svg>
                  </div>
                  <div class="p-3">
                    <form @submit.prevent="handleResetPassword">
                      <div class="text-center mb-3">
                        <h4 class="text-black">Đổi mật khẩu</h4>
                        <p class="text-muted">
                          Bạn đã nhớ lại mật khẩu?
                          <router-link to="/Login" class="text-primary">Đăng nhập</router-link>
                        </p>
                      </div>
                      <p class="text-muted text-center m-b-30">
                        Nhập mật khẩu mới cho tài khoản của bạn
                      </p>
                      <div v-if="message" :class="['alert', success ? 'alert-success' : 'alert-danger', 'text-center']">
                        {{ message }}
                      </div>
                      <div class="form-group"      style="margin-bottom: 10px;">
                        <input
                          v-model="email"
                          type="email"
                          class="form-control"
                          id="email"
                          placeholder="Email"
                          required
                          disabled
                          hidden
                        />
                      </div>
                      <div class="form-group"     style="margin-bottom: 10px;">
                        <input
                          v-model="newPassword"
                          type="password"
                          class="form-control"
                          id="newPassword"
                          placeholder="Nhập mật khẩu mới"
                          required
                        />
                      </div>
                      <div class="form-group"     style="margin-bottom: 10px;">
                        <input
                          v-model="confirmPassword"
                          type="password"
                          class="form-control"
                          id="confirmPassword"
                          placeholder="Xác nhận mật khẩu mới"
                          required
                        />
                      </div>
                      <div class="form-group form-check"     style="margin-bottom: 10px;">
                        <input
                          v-model="loginAfterReset"
                          type="checkbox"
                          class="form-check-input"
                          id="loginAfterReset"
                        />
                        <label class="form-check-label" for="loginAfterReset">Đăng nhập ngay sau khi đổi mật khẩu</label>
                      </div>
                      <button type="submit" class="btn btn-primary btn-rounded btn-lg btn-block" style="width: 440px;">
                        Đổi mật khẩu
                      </button>
                    </form>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.alert-success {
  margin-bottom: 15px;
  color: #155724;
  background-color: #d4edda;
  border-color: #c3e6cb;
}
.alert-danger {
  margin-bottom: 15px;
  color: #721c24;
  background-color: #f8d7da;
  border-color: #f5c6cb;
}
.text-primary {
  color: #007bff;
  text-decoration: none;
}
.text-primary:hover {
  text-decoration: underline;
}
</style>