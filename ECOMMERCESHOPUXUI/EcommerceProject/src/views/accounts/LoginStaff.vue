<script setup>
import { ref, onMounted } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import Cookies from 'js-cookie';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../constants/api.js'
const emailOrUsername = ref('');
const password = ref('');
const errorMessage = ref('');
const router = useRouter();
let getApiUrl = GetApiUrl()
const showPassword = ref(false);
// Hàm kiểm tra định dạng email
const isValidEmail = (input) => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  // Nếu input chứa @, kiểm tra định dạng email
  if (input.includes('@')) {
    return emailRegex.test(input);
  }
  // Nếu không chứa @, chấp nhận như tên tài khoản (chỉ kiểm tra không rỗng)
  return input.trim().length > 0;
};
const togglePasswordVisibility = () => {
  showPassword.value = !showPassword.value;
};
// Hàm kiểm tra nếu đã đăng nhập thì chuyển hướng về trang chủ
const checkIfLoggedIn = () => {
  const accessToken = Cookies.get('accessToken');
  if (accessToken) {
    // Nếu đã đăng nhập (có accessToken), chuyển hướng về trang chủ
    router.push('/admin');
  }
};

// Gọi hàm kiểm tra khi component được mounted
onMounted(() => {
  checkIfLoggedIn();
});
const handleLogin = async () => {
  // Kiểm tra input email hoặc tên tài khoản
  if (!emailOrUsername.value.trim()) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng nhập thất bại!',
      text: 'Vui lòng nhập tài khoản',
      confirmButtonText: 'OK',
    });
    return;
  }
  
  // Kiểm tra định dạng email nếu người dùng nhập email
  if (!isValidEmail(emailOrUsername.value)) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng nhập thất bại!',
      text: 'Email không đúng định dạng.',
      confirmButtonText: 'OK',
    });
    return;
  }
  
  // Kiểm tra mật khẩu
  if (!password.value) {
    await Swal.fire({
      icon: 'error',
      title: 'Đăng nhập thất bại!',
      text: 'Vui lòng nhập mật khẩu.',
      confirmButtonText: 'OK',
    });
    return;
  }
  errorMessage.value = ''
  try {
    const payload = {
      Email_TenTaiKhoan: emailOrUsername.value.trim(),
      MatKhau: password.value,
    };
    const response = await fetch(getApiUrl+'/api/Account/LoginStaff', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      const errorText = await response.text()
      console.log('Phản hồi lỗi từ server:', errorText)
      throw new Error(`Lỗi HTTP ${response.status}: ${errorText || 'Không có chi tiết'}`)
    }
    const data = await response.json()
    if(data.message == 'Tài khoản đang bị tạm khóa' && data.success == false){
      await Swal.fire({
        icon: 'error',
        title: 'Đăng nhập thất bại!',
        text: 'Tài khoản đang bị tạm khóa, vui lòng liên hệ với cửa hàng.',
        confirmButtonText: 'OK',
      });
      return;
    }
    if(data.message == 'false' && data.success == false){
      await Swal.fire({
        icon: 'error',
        title: 'Đăng nhập thất bại!',
        text: 'Tài khoản không tồn tại.',
        confirmButtonText: 'OK',
      });
      return;
    }
    if (data.success) {
      Cookies.set('accessToken', data.data.accessToken, { expires: 3 / 24 }) 
      Cookies.set('refreshToken', data.data.refreshToken, { expires: 3 / 24 })
      await Swal.fire({
        icon: 'success',
        title: 'Đăng nhập thành công!',
        text: 'Chào mừng bạn trở lại.',
        confirmButtonText: 'OK',
      })
      // ? Kiểm tra có link web không có đăng nhập trước đó để dùng thì điều hướng lại
      if (router.currentRoute.query && router.currentRoute.query.redirect) {
        router.push(router.currentRoute.query.redirect)
      } else {
        router.push('/admin')
      }
    } else {
      await Swal.fire({
        icon: 'error',
        title: 'Đăng nhập thất bại!',
        text: 'Tên tài khoản/email hoặc mật khẩu không chính xác.',
        confirmButtonText: 'OK',
      });
      return;
    }
  } catch (error) {
     await Swal.fire({
        icon: 'error',
        title: 'Đăng nhập thất bại!',
        text: 'Tên tài khoản/email hoặc mật khẩu không chính xác.',
        confirmButtonText: 'OK',
      });
     
  }
}
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
                      <img src="../../assets/Customer/img/logo.png" height="150" alt="logo" />
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
                    <form @submit.prevent="handleLogin">
                      <div class="text-center mb-3">
                        <h4 class="text-black">Đăng nhập</h4>
                      </div>
                      
                      <div class="form-group" style="margin-bottom: 20px;">
                        <input
                          v-model="emailOrUsername"
                          type="text"
                          class="form-control"
                          id="username"
                          placeholder="Tên tài khoản hoặc Email"
                          required
                        />
                      </div>
                      <div class="form-group position-relative"style="margin-bottom: 20px;">
                        <input
                          v-model="password"
                          :type="showPassword ? 'text' : 'password'"
                          class="form-control"
                          id="password"
                          placeholder="Mật khẩu"
                          required
                        />
                        <span
                          class="password-toggle-icon"
                          @click="togglePasswordVisibility"
                        >
                          <i :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                        </span>
                      </div>
                     <div class="form-row">
                        <div class="form-group col-12 text-center"style="margin-bottom: 10px;">
                          <label class="forgot-psw">
                            <router-link to="/ForgotPasswordStaff">Quên mật khẩu</router-link>
                          </label>
                        </div>
                      </div>
                      <button type="submit" class="btn btn-primary btn-rounded btn-lg btn-block" style="width: 440px;">
                        Đăng nhập
                      </button>
                      <p class="text-center mt-3 mb-0">
                        <router-link to="/Login" class="text-link text-decoration-none"
                          >Bạn là khách hàng?</router-link
                        >
                      </p>
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
.alert-danger {
  margin-bottom: 15px;
}
/* CSS cho icon mắt */
.password-toggle-icon {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  cursor: pointer;
  font-size: 1.2rem;
  color: #6c757d;
}

.password-toggle-icon:hover {
  color: #343a40;
}

.form-group.position-relative {
  position: relative;
}

.form-control {
  padding-right: 40px; /* Tạo khoảng trống cho icon */
}
</style>
