<template>
  <div>
    <!-- Offcanvas Menu Begin -->
    <div class="offcanvas-menu-overlay"></div>
    <!-- Offcanvas Menu End -->
    <!-- Header Section Begin -->
    <header class="header">
      <div class="container-fluid">
        <div class="row">
          <div class="col-xl-3 col-lg-2" style="width: 300px; margin-right: 50px; padding-bottom: 20px">
            <svg viewBox="0 0 700 250" role="img" aria-label="Angel soft curvy logo with wings and animated gradient">
              <defs>
                <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                  <stop offset="20%" stop-color="#EC4E79">
                    <animate attributeName="stop-color" values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;" dur="6s"
                      repeatCount="indefinite" />
                  </stop>
                  <stop offset="40%" stop-color="#ABA2B7">
                    <animate attributeName="stop-color" values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;" dur="6s"
                      repeatCount="indefinite" />
                  </stop>
                  <stop offset="55%" stop-color="#5CCAE7">
                    <animate attributeName="stop-color" values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;" dur="6s"
                      repeatCount="indefinite" />
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
          <div class="col-xl-6 col-lg-7">
            <nav class="header__menu">
              <ul>
                <li>
                  <RouterLink to="/">Trang Chủ</RouterLink>
                </li>
                <li>
                  <RouterLink to="/Shop">Cửa Hàng</RouterLink>
                </li>
                <li>
                  <RouterLink to="/chat">Liên Hệ</RouterLink>
                </li>
              </ul>
            </nav>
          </div>
          <div class="col-lg-3">
            <div class="header__right d-flex align-items-center justify-content-end gap-3">
              <ul class="header__right__widget d-flex align-items-center gap-3 list-unstyled mb-0">
                <li>
                  <router-link to="/favoriteproduct" class="position-relative">
                    <i class="fa fa-heart fs-5"></i>
                    <span
                      class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                    >
                      2
                      <span class="visually-hidden">sản phẩm yêu thích</span>
                    </span>
                  </router-link>
                </li>
                <li>
                  <router-link to="/Cart" class="position-relative">
                    <i class="fa fa-shopping-bag fs-5"></i>
                    <span
                      class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
                    >
                      2
                      <span class="visually-hidden">sản phẩm trong giỏ hàng</span>
                    </span>
                  </router-link>
                </li>
                <li v-if="isLoggedIn">
                  <NavigationUserReview />
                </li>
                <li>
                  <WheelRandomCode />
                </li>
              </ul>
              <div class="dropdown">
                <button
                  class="btn btn-light dropdown-toggle"
                  type="button"
                  id="userDropdown"
                  data-bs-toggle="dropdown"
                  aria-expanded="false"
                >
                  <i class="fa fa-user"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="userDropdown">
                  <template v-if="!isLoggedIn">
                    <li><router-link class="dropdown-item" to="/Login">Đăng nhập</router-link></li>
                    <li><router-link class="dropdown-item" to="/Register">Đăng ký</router-link></li>
                  </template>
                  <template v-else>
                    <li>
                      <router-link class="dropdown-item" to="/Profile"
                        >Thông tin cá nhân</router-link
                      >
                    </li>
                    <li><hr class="dropdown-divider" /></li>
                    <li>
                      <a class="dropdown-item" href="#" @click.prevent="handleLogout">Đăng xuất</a>
                    </li>
                  </template>
                </ul>
              </div>
            </div>
          </div>
        </div>
        <div id="mobile-menu-wrap"></div>
      </div>
    </header>
    <!-- Header Section End -->
  </div>

</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import Cookies from 'js-cookie'
import { validateToken } from '@/utils/auth'
import NavigationUserReview from './ui/navigationUserReview.vue'
import WheelRandomCode from './specicals/WheelRandomCode.vue'

const router = useRouter()
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const isLoggedIn = ref(false)

const checkLogin = async () => {
  if (accessToken.value && refreshToken.value) {
    const result = await validateToken(accessToken.value, refreshToken.value)
    isLoggedIn.value = result.isValid
    if (result.isValid) {
      Cookies.set('accessToken', result.newAccessToken)
    } else {
      Cookies.remove('accessToken')
      Cookies.remove('refreshToken')
    }
  } else {
    isLoggedIn.value = false
  }
}

const handleLogout = () => {
  Cookies.remove('accessToken')
  Cookies.remove('refreshToken')
  isLoggedIn.value = false
  router.push('/Login')
}

onMounted(() => {
  checkLogin()
})
</script>
<style>
.header__menu {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
}

.header__menu ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
}

.header__menu li {
  margin: 0 15px;
}

.header__menu li a {
  text-decoration: none;
  color: #333;
}
</style>
