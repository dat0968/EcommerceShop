<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../constants/api.js';

const email = ref('');
const verificationCode = ref('');
const showVerification = ref(false);
const message = ref('');
const success = ref(false);
const router = useRouter();
const getApiUrl = GetApiUrl();

// Biến cho bộ đếm ngược
const countdown = ref(60); // 60 giây
const isCounting = ref(false);
let countdownInterval = null;

// Biến cho reCAPTCHA
const recaptchaToken = ref('');
const isRecaptchaVerified = ref(false);
const loading = ref(false);

// Hàm bắt đầu bộ đếm ngược
const startCountdown = () => {
  countdown.value = 60;
  isCounting.value = true;
  countdownInterval = setInterval(() => {
    if (countdown.value > 0) {
      countdown.value--;
    } else {
      clearInterval(countdownInterval);
      isCounting.value = false;
    }
  }, 1000);
};

// Dừng bộ đếm ngược khi component bị hủy
onUnmounted(() => {
  if (countdownInterval) {
    clearInterval(countdownInterval);
  }
});

// Định dạng thời gian đếm ngược
const formattedCountdown = computed(() => {
  const minutes = Math.floor(countdown.value / 60);
  const seconds = countdown.value % 60;
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
});

// Tải script reCAPTCHA động
const loadRecaptchaScript = () => {
  if (document.getElementById('recaptcha-script')) return; // Tránh tải lại script nếu đã tồn tại

  const script = document.createElement('script');
  script.id = 'recaptcha-script';
  script.src = 'https://www.google.com/recaptcha/api.js';
  script.async = true;
  script.defer = true;
  script.onload = () => {
    console.log('reCAPTCHA script loaded');
  };
  document.head.appendChild(script);
};

// Xóa script khi component bị hủy
const removeRecaptchaScript = () => {
  const script = document.getElementById('recaptcha-script');
  if (script) {
    script.remove();
  }
};

// Callback khi reCAPTCHA được hoàn thành
const onRecaptchaVerify = (token) => {
  recaptchaToken.value = token;
};

// Callback khi reCAPTCHA hết hạn
const onRecaptchaExpired = () => {
  recaptchaToken.value = '';
  isRecaptchaVerified.value = false;
  Swal.fire({
    icon: 'warning',
    title: 'Lỗi!',
    text: 'reCAPTCHA đã hết hạn. Vui lòng thử lại.',
    confirmButtonText: 'OK',
  });
};

// Xác minh reCAPTCHA
const verifyRecaptcha = async () => {
  if (!recaptchaToken.value) {
    isRecaptchaVerified.value = false;
    return { success: false, message: 'Vui lòng hoàn thành reCAPTCHA!' };
  }
  loading.value = true;
  try {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 10000); // Timeout 10 giây

    const response = await fetch(`${getApiUrl}/api/Account/VerifyRecaptcha`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ RecaptchaToken: recaptchaToken.value }),
      signal: controller.signal,
    });

    clearTimeout(timeoutId);

    if (!response.ok) {
      throw new Error(`Lỗi HTTP ${response.status}`);
    }

    const data = await response.json();
    isRecaptchaVerified.value = data.success;
    if (!data.success) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: data.message || 'Xác minh reCAPTCHA thất bại.',
        confirmButtonText: 'OK',
      });
    }
    return data;
  } catch (error) {
    console.error('Lỗi khi xác minh reCAPTCHA:', error);
    isRecaptchaVerified.value = false;
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: error.name === 'AbortError' ? 'Xác minh reCAPTCHA timeout!' : error.message || 'Có lỗi xảy ra khi xác minh reCAPTCHA!',
      confirmButtonText: 'OK',
    });
    return { success: false, message: error.message };
  } finally {
    loading.value = false;
  }
};

// Tải script và gắn callback khi component được mounted
onMounted(() => {
  loadRecaptchaScript();
  window.onRecaptchaVerify = onRecaptchaVerify;
  window.onRecaptchaExpired = onRecaptchaExpired;
});

// Xóa script và callback khi component bị unmounted
onUnmounted(() => {
  removeRecaptchaScript();
  delete window.onRecaptchaVerify;
  delete window.onRecaptchaExpired;
  window.grecaptcha?.reset();
});

const handleForgotPassword = async () => {
  message.value = '';
  success.value = false;

  const recaptchaResult = await verifyRecaptcha();
  if (!recaptchaResult.success) {
    return;
  }

  try {
    const response = await fetch(`${getApiUrl}/api/Account/ForgotPasswordCustomer?email=${encodeURIComponent(email.value.trim())}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();

    if (data.success) {
      success.value = true;
      message.value = data.message || 'Mã xác minh đã được gửi tới email của bạn!';
      showVerification.value = true;
      startCountdown(); // Bắt đầu đếm ngược khi gửi mã thành công
      await Swal.fire({
        icon: 'success',
        title: 'Thành công!',
        text: message.value,
        confirmButtonText: 'OK',
      });
      // Reset reCAPTCHA sau khi thành công
      recaptchaToken.value = '';
      isRecaptchaVerified.value = false;
      window.grecaptcha?.reset();
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
    console.error('Lỗi trong handleForgotPassword:', {
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

const handleVerifyCode = async () => {
  message.value = '';
  success.value = false;

  const recaptchaResult = await verifyRecaptcha();
  if (!recaptchaResult.success) {
    return;
  }

  try {
    const response = await fetch(`${getApiUrl}/api/Account/VerifyResetPasswordCode?email=${encodeURIComponent(email.value.trim())}&code=${encodeURIComponent(verificationCode.value.trim())}`, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();

    if (data.success) {
      success.value = true;
      message.value = data.message || 'Xác minh thành công!';
      await Swal.fire({
        icon: 'success',
        title: 'Thành công!',
        text: message.value,
        confirmButtonText: 'OK',
      });
      router.push({ name: 'ResetPasswordCustomer', query: { email: email.value } });
      // Reset reCAPTCHA sau khi thành công
      recaptchaToken.value = '';
      isRecaptchaVerified.value = false;
      window.grecaptcha?.reset();
    } else {
      success.value = false;
      message.value = data.message || 'Mã xác minh không đúng hoặc đã hết hạn!';
      await Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: message.value,
        confirmButtonText: 'OK',
      });
    }
  } catch (error) {
    console.error('Lỗi trong handleVerifyCode:', {
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

// Hàm gửi lại mã xác minh
const handleResendCode = async () => {
  if (isCounting.value) {
    await Swal.fire({
      icon: 'info',
      title: 'Vui lòng chờ!',
      text: 'Vui lòng đợi hết thời gian đếm ngược trước khi gửi lại mã.',
      confirmButtonText: 'OK',
    });
    return;
  }
  await handleForgotPassword(); // Gọi lại hàm gửi mã
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
                    <form @submit.prevent="showVerification ? handleVerifyCode() : handleForgotPassword()">
                      <div class="text-center mb-3">
                        <h4 class="text-black">Quên mật khẩu</h4>
                        <p class="text-muted">
                          Bạn đã nhớ lại mật khẩu?
                          <router-link to="/Login" class="text-primary">Đăng nhập</router-link>
                        </p>
                      </div>
                      <p class="text-muted text-center m-b-30"     style="margin-bottom: 10px;">
                        {{ showVerification ? 'Nhập mã xác minh đã gửi tới email của bạn' : 'Nhập email để nhận mã xác minh' }}
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
                          placeholder="Nhập email đăng ký tài khoản"
                          required
                          :disabled="showVerification"
                        />
                      </div>
                      <div v-if="showVerification" class="form-group">
                        <input
                          v-model="verificationCode"
                          type="text"
                          class="form-control"
                          id="verificationCode"
                          placeholder="Nhập mã xác minh"
                          required
                        />
                      </div>
                      <div v-if="showVerification" class="text-center mb-3">
                        <p class="text-muted">
                          Mã xác minh có hiệu lực trong: <strong>{{ formattedCountdown }}</strong>
                        </p>
                        <button
                          type="button"
                          class="btn btn-link"
                          :disabled="isCounting"
                          @click="handleResendCode"
                        >
                          Gửi lại mã xác minh
                        </button>
                      </div>
                      <div class="form-group">
                        <div
                          class="g-recaptcha"
                          data-sitekey="6LdIA0orAAAAAB-3smkOKHc3MRmSw85-XrOMnST3"
                          data-callback="onRecaptchaVerify"
                          data-expired-callback="onRecaptchaExpired"
                   
                        ></div>
                      </div>
                      <button
                        type="submit"
                        class="btn btn-primary btn-rounded btn-lg btn-block"
                        :disabled="loading"
                        style="width: 440px;"
                      >
                        {{ showVerification ? 'Xác minh mã' : 'Gửi mã xác minh' }}
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
.btn-link {
  color: #007bff;
}
.btn-link:disabled {
  color: #6c757d;
  cursor: not-allowed;
}
.g-recaptcha {
  margin-bottom: 15px;
}
</style>