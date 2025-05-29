<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../constants/api.js';

const hoTen = ref('');
const tenTaiKhoan = ref('');
const email = ref('');
const matKhau = ref('');
const verificationCode = ref('');
const recaptchaToken = ref('');
const errorMessage = ref('');
const usernameValid = ref(true);
const emailValid = ref(true);
const loading = ref(false);
const router = useRouter();
const getApiUrl = GetApiUrl();

// Hàm kiểm tra định dạng email cơ bản
const isValidEmailFormat = (email) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

// Hàm kiểm tra định dạng mật khẩu
const isValidPassword = (password) => {
  const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{6,}$/;
  return passwordRegex.test(password);
};

// Hàm kiểm tra định dạng tên tài khoản
const isValidUsernameFormat = (username) => {
  const usernameRegex = /^[a-zA-Z0-9_]+$/;
  return usernameRegex.test(username);
};

// Callback khi reCAPTCHA được hoàn thành
const onRecaptchaVerify = (token) => {
  console.log('reCAPTCHA verified, token:', token);
  recaptchaToken.value = token;
};

// Callback khi reCAPTCHA hết hạn
const onRecaptchaExpired = () => {
  console.log('reCAPTCHA expired');
  recaptchaToken.value = '';
  Swal.fire({
    icon: 'warning',
    title: 'Lỗi!',
    text: 'reCAPTCHA đã hết hạn. Vui lòng thử lại.',
    confirmButtonText: 'OK',
  });
};

// Tải script reCAPTCHA động
const loadRecaptchaScript = () => {
  if (document.getElementById('recaptcha-script')) return;

  const script = document.createElement('script');
  script.id = 'recaptcha-script';
  script.src = 'https://www.google.com/recaptcha/api.js';
  script.async = true;
  script.defer = true;
  script.onload = () => {
    console.log('reCAPTCHA script loaded');
  };
  script.onerror = () => {
    console.error('Failed to load reCAPTCHA script');
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Không thể tải reCAPTCHA. Vui lòng kiểm tra kết nối mạng.',
      confirmButtonText: 'OK',
    });
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

// Tải script và gắn các hàm callback vào window
onMounted(() => {
  loadRecaptchaScript();
  window.onRecaptchaVerify = onRecaptchaVerify;
  window.onRecaptchaExpired = onRecaptchaExpired;
});

// Dọn dẹp khi component bị hủy
onUnmounted(() => {
  removeRecaptchaScript();
  delete window.onRecaptchaVerify;
  delete window.onRecaptchaExpired;
  window.grecaptcha?.reset();
});

// Xác minh reCAPTCHA
const verifyRecaptcha = async () => {
  console.log('Verifying reCAPTCHA, token:', recaptchaToken.value);
  if (!recaptchaToken.value) {
    return { success: false, message: 'Vui lòng hoàn thành reCAPTCHA!' };
  }
  try {
    const response = await fetch(`${getApiUrl}/api/Account/VerifyRecaptcha`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ RecaptchaToken: recaptchaToken.value }),
    });

    if (!response.ok) {
      throw new Error(`Lỗi HTTP ${response.status}`);
    }

    const data = await response.json();
    console.log('reCAPTCHA verification result:', data);
    return data;
  } catch (error) {
    console.error('Lỗi khi xác minh reCAPTCHA:', error);
    return { success: false, message: 'Có lỗi xảy ra khi xác minh reCAPTCHA!' };
  }
};

// Kiểm tra tên tài khoản
const checkUsername = async () => {
  if (!tenTaiKhoan.value.trim()) {
    usernameValid.value = false;
    return;
  }
  if (!isValidUsernameFormat(tenTaiKhoan.value.trim())) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Tên tài khoản không được chứa ký tự đặc biệt!',
      confirmButtonText: 'OK',
    });
    usernameValid.value = false;
    return;
  }
  try {
    const response = await fetch(`${getApiUrl}/api/Account/checkUsername?username=${tenTaiKhoan.value.trim()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Lỗi HTTP ${response.status}`);
    }

    const data = await response.json();
    if (!data.success) {
      await Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: data.message || 'Tên tài khoản này đã tồn tại!',
        confirmButtonText: 'OK',
      });
      usernameValid.value = false;
    } else {
      usernameValid.value = true;
    }
  } catch (error) {
    console.error('Lỗi khi kiểm tra tên tài khoản:', error);
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Có lỗi xảy ra khi kiểm tra tên tài khoản!',
      confirmButtonText: 'OK',
    });
    usernameValid.value = false;
  }
};

// Kiểm tra email
const checkEmail = async () => {
  if (!email.value.trim()) {
    emailValid.value = false;
    return;
  }
  if (!isValidEmailFormat(email.value.trim())) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Định dạng email không hợp lệ!',
      confirmButtonText: 'OK',
    });
    emailValid.value = false;
    return;
  }
  try {
    const response = await fetch(`${getApiUrl}/api/Account/checkEmail?email=${email.value.trim()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error(`Lỗi HTTP ${response.status}`);
    }

    const data = await response.json();
    if (!data.success) {
      await Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: data.message || 'Email này đã tồn tại!',
        confirmButtonText: 'OK',
      });
      emailValid.value = false;
    } else {
      emailValid.value = true;
    }
  } catch (error) {
    console.error('Lỗi khi kiểm tra email:', error);
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Có lỗi xảy ra khi kiểm tra email!',
      confirmButtonText: 'OK',
    });
    emailValid.value = false;
  }
};

// Gửi mã xác minh
const sendVerificationCode = async () => {
  if (!email.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Vui lòng nhập email trước khi gửi mã xác minh!',
      confirmButtonText: 'OK',
    });
    return;
  }

  if (!isValidEmailFormat(email.value.trim())) {
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Định dạng email không hợp lệ!',
      confirmButtonText: 'OK',
    });
    return;
  }

  loading.value = true;
  await checkEmail();
  if (!emailValid.value) {
    loading.value = false;
    return;
  }

  try {
    const response = await fetch(`${getApiUrl}/api/Account/SendVerificationCode?email=${email.value.trim()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();
    if (data.success) {
      await Swal.fire({
        icon: 'success',
        title: 'Thành công!',
        text: data.message || 'Mã xác minh đã được gửi tới email của bạn.',
        confirmButtonText: 'OK',
      });
    } else {
      await Swal.fire({
        icon: 'error',
        title: 'Lỗi!',
        text: data.message || 'Không thể gửi mã xác minh!',
        confirmButtonText: 'OK',
      });
    }
  } catch (error) {
    console.error('Lỗi khi gửi mã xác minh:', error);
    await Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: `Có lỗi xảy ra khi gửi mã xác minh: ${error.message}`,
      confirmButtonText: 'OK',
    });
  } finally {
    loading.value = false;
  }
};

// Xử lý đăng ký
const handleRegister = async () => {
  errorMessage.value = '';

  // Kiểm tra các trường bắt buộc
  if (!hoTen.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng nhập họ và tên!',
      confirmButtonText: 'OK',
    });
    return;
  }
  if (!tenTaiKhoan.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng nhập tên tài khoản!',
      confirmButtonText: 'OK',
    });
    return;
  }
  if (!email.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng nhập email!',
      confirmButtonText: 'OK',
    });
    return;
  }
  if (!matKhau.value) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng nhập mật khẩu!',
      confirmButtonText: 'OK',
    });
    return;
  }
  if (!verificationCode.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng nhập mã xác minh!',
      confirmButtonText: 'OK',
    });
    return;
  }
  if (!recaptchaToken.value) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Vui lòng hoàn thành reCAPTCHA!',
      confirmButtonText: 'OK',
    });
    return;
  }

  // Kiểm tra định dạng mật khẩu
  if (!isValidPassword(matKhau.value)) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: 'Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt!',
      confirmButtonText: 'OK',
    });
    return;
  }

  // Kiểm tra trùng lặp tên tài khoản và email
  await checkUsername();
  await checkEmail();

  // Nếu có lỗi trùng lặp, dừng xử lý
  if (!usernameValid.value || !emailValid.value) {
    return;
  }

  // Xác minh mã trước khi đăng ký
  loading.value = true;
  try {
    const response = await fetch(`${getApiUrl}/api/Account/VerifyEmail?email=${email.value.trim()}&code=${verificationCode.value.trim()}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const data = await response.json();
    if (!data.success) {
      await Swal.fire({
        icon: 'error',
        title: 'Đăng ký thất bại!',
        text: data.message || 'Mã xác minh không đúng!',
        confirmButtonText: 'OK',
      });
      return;
    }

    // Xác minh reCAPTCHA
    const recaptchaResult = await verifyRecaptcha();
    if (!recaptchaResult.success) {
      await Swal.fire({
        icon: 'error',
        title: 'Đăng ký thất bại!',
        text: recaptchaResult.message || 'Xác minh reCAPTCHA thất bại!',
        confirmButtonText: 'OK',
      });
      return;
    }

    // Gửi yêu cầu đăng ký nếu tất cả đều hợp lệ
    const payload = {
      HoTen: hoTen.value.trim(),
      TenTaiKhoan: tenTaiKhoan.value.trim(),
      Email: email.value.trim(),
      MatKhau: matKhau.value,
      RecaptchaToken: recaptchaToken.value,
    };

    const registerResponse = await fetch(`${getApiUrl}/api/Account/Register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (!registerResponse.ok) {
      const errorText = await registerResponse.text();
      throw new Error(`Lỗi HTTP ${registerResponse.status}: ${errorText || 'Không có chi tiết'}`);
    }

    const registerData = await registerResponse.json();

    if (registerData.success) {
      await Swal.fire({
        icon: 'success',
        title: 'Đăng ký thành công!',
        text: 'Vui lòng đăng nhập để tiếp tục.',
        confirmButtonText: 'OK',
      });
      router.push('/Login');
    } else {
      await Swal.fire({
        icon: 'error',
        title: 'Đăng ký thất bại!',
        text: registerData.message || 'Đăng ký thất bại!',
        confirmButtonText: 'OK',
      });
    }
  } catch (error) {
    console.error('Lỗi trong handleRegister:', {
      message: error.message,
      name: error.name,
      stack: error.stack,
    });
    await Swal.fire({
      icon: 'error',
      title: 'Đăng ký thất bại!',
      text: `Có lỗi xảy ra khi đăng ký: ${error.message}`,
      confirmButtonText: 'OK',
    });
  } finally {
    loading.value = false;
    window.grecaptcha?.reset();
    recaptchaToken.value = '';
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
                  <h3 class="text-center mt-0 m-b-15">
                    <a href="index.html" class="xp-web-logo">
                      <img src="../../assets/client/img/logo.png" height="150" alt="logo" />
                    </a>
                  </h3>
                  <div class="p-3">
                    <form @submit.prevent="handleRegister">
                      <div class="text-center mb-3">
                        <h4 class="text-black">Tạo tài khoản</h4>
                        <p class="text-muted">
                          Bạn đã có tài khoản ?
                          <router-link to="/Login" class="text-primary">Đăng nhập</router-link>
                        </p>
                      </div>
                      <div class="login-or">
                        <h6 class="text-muted">Hoặc</h6>
                      </div>
                      <div class="form-group">
                        <input
                          v-model="hoTen"
                          type="text"
                          class="form-control"
                          id="hoTen"
                          placeholder="Họ và tên"
                          required
                        />
                      </div>
                      <div class="form-group">
                        <input
                          v-model="tenTaiKhoan"
                          type="text"
                          class="form-control"
                          id="username"
                          placeholder="Tên tài khoản"
                          required
                          @blur="checkUsername"
                        />
                      </div>
                      <div class="form-group">
                        <input
                          v-model="email"
                          type="email"
                          class="form-control"
                          id="email"
                          placeholder="Email"
                          required
                          @blur="checkEmail"
                        />
                        <button
                          type="button"
                          class="btn btn-secondary btn-sm mt-2"
                          @click="sendVerificationCode"
                          :disabled="loading"
                        >
                          {{ loading ? 'Đang gửi...' : 'Gửi mã xác minh' }}
                        </button>
                      </div>
                      <div class="form-group">
                        <input
                          v-model="verificationCode"
                          type="text"
                          class="form-control"
                          id="verificationCode"
                          placeholder="Nhập mã xác minh"
                          required
                        />
                      </div>
                      <div class="form-group">
                        <input
                          v-model="matKhau"
                          type="password"
                          class="form-control"
                          id="password"
                          placeholder="Mật khẩu"
                          required
                        />
                        <small class="form-text text-muted">
                          Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt (!@#$%^&*).
                        </small>
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
                      >
                        {{ loading ? 'Đang đăng ký...' : 'Tạo tài khoản' }}
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
.text-primary {
  color: #007bff;
  text-decoration: none;
}
.text-primary:hover {
  text-decoration: underline;
}
.form-text {
  font-size: 0.875rem;
}
.btn-secondary {
  margin-left: 10px;
}
.g-recaptcha {
  margin-bottom: 15px;
}
</style>