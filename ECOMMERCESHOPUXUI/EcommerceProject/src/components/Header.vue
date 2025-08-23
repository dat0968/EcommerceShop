
<script setup>
import { useRouter, RouterLink } from 'vue-router'
import Cookies from 'js-cookie'
import NavigationUserReview from '@/components/ui/NavigationUserReview.vue'
import WheelRandomCode from '@/components/specicals/WheelRandomCode.vue'
import { ref, computed, onMounted, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Swal from 'sweetalert2'
import { emitter } from '@/stores/eventBus'
const router = useRouter()
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const isLoggedIn = ref(false)
const numberCart = ref(0)
const getUrlAPI = ref(GetApiUrl())
const checkLogin = async () => {
  const result = await validateToken(accessToken.value, refreshToken.value)
  isLoggedIn.value = result.isValid
  if (result.isValid) {
    accessToken.value = result.newAccessToken
  }
}
async function fetchCart() {
  // accessToken.value = Cookies.get('accessToken')
  // refreshToken.value = Cookies.get('refreshToken')
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid) {
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    const response = await fetch(`${getUrlAPI.value}/api/Cart/${readToken.IdUser}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) {
      throw new Error('Failed to fetchCart')
    }
    const result = await response.json()
    numberCart.value = result.length
  }
}

const handleLogout = () => {
  Swal.fire({
    title: 'Bạn có chắc chắn muốn đăng xuất?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#3085d6',
    confirmButtonText: 'Đăng xuất',
    cancelButtonText: 'Hủy',
  }).then((result) => {
    if (result.isConfirmed) {
      Cookies.remove('accessToken')
      Cookies.remove('refreshToken')
      isLoggedIn.value = false
      Swal.fire({
        title: 'Đăng xuất thành công!',
        icon: 'success',
        confirmButtonColor: '#3085d6',
        timer: 1500,
      }).then(() => {
        router.push('/Login')
      })
    }
  })
}
onMounted(async () => {
  checkLogin()
  await fetchCart()
  emitter.on('cart-updated', fetchCart)
})
</script>
<template>
  <div>
    <!-- Offcanvas Menu Begin -->
    <div class="offcanvas-menu-overlay"></div>
    <!-- Header Section Begin -->
    <header class="header">
      <div class="container-fluid">
        <div class="row justify-content-between align-items-center">
          <div
            class="col-xl-3 col-lg-2"
            style="width: 300px; margin-right: 50px; padding-bottom: 20px"
          >
            <!-- Logo SVG -->
            <svg
              viewBox="0 0 700 250"
              role="img"
              aria-label="Angel soft curvy logo with wings and animated gradient"
            >
              <defs>
                <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                  <stop offset="20%" stop-color="#EC4E79">
                    <animate
                      attributeName="stop-color"
                      values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;"
                      dur="6s"
                      repeatCount="indefinite"
                    />
                  </stop>
                  <stop offset="40%" stop-color="#ABA2B7">
                    <animate
                      attributeName="stop-color"
                      values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;"
                      dur="6s"
                      repeatCount="indefinite"
                    />
                  </stop>
                  <stop offset="55%" stop-color="#5CCAE7">
                    <animate
                      attributeName="stop-color"
                      values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;"
                      dur="6s"
                      repeatCount="indefinite"
                    />
                  </stop>
                </linearGradient>
              </defs>
              <path
                class="wing left"
                d="M160 130 C110 90, 90 180, 150 170 C130 150, 140 110, 160 130 Z"
              />
              <path
                class="wing left"
                d="M150 140 C120 120, 110 170, 150 160 C140 140, 130 120, 150 140 Z"
                opacity="0.5"
              />
              <path
                class="wing right"
                d="M540 130 C590 90, 610 180, 550 170 C570 150, 560 110, 540 130 Z"
              />
              <path
                class="wing right"
                d="M550 140 C580 120, 590 170, 550 160 C560 140, 570 120, 550 140 Z"
                opacity="0.5"
              />
              <RouterLink to="/" style="text-decoration: none">
                <text
                  x="50%"
                  y="60%"
                  dominant-baseline="middle"
                  text-anchor="middle"
                  class="angel-text"
                >
                  Angel
                </text>
              </RouterLink>
            </svg>
          </div>

          <div class="col-xl-6 col-lg-7">
            <nav class="header__menu">
              <ul>
                <li><RouterLink to="/">Trang Chủ</RouterLink></li>
                <li><RouterLink to="/Shop">Cửa Hàng</RouterLink></li>
                <li><RouterLink to="/Contact">Liên Hệ</RouterLink></li>
                <li><RouterLink to="/Chat">Hỗ trợ</RouterLink></li>
              </ul>
            </nav>
          </div>

          <div class="col-lg-3">
            <div class="header__right" style="margin-top: 10px">
              <div class="header__right__auth">
                <!-- Chưa đăng nhập -->
                <template v-if="!isLoggedIn">
                  <div class="dropdown">
                    <button
                      class="dropdown-toggle text-primary"
                      id="loginDropdown"
                      data-bs-toggle="dropdown"
                      aria-expanded="false"
                    >
                      Tài khoản
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="loginDropdown">
                      <li>
                        <router-link to="/Login" class="dropdown-item"
                          ><i class="fa fa-user me-2"></i>Đăng nhập</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/Register" class="dropdown-item"
                          ><i class="fa fa-user-plus me-2"></i>Đăng ký</router-link
                        >
                      </li>
                      <!-- <li><hr class="dropdown-divider" /></li> -->
                      <!-- <li>
                        <router-link to="/favoriteproduct" class="dropdown-item">
                          <i class="fa fa-heart me-2"></i>Yêu thích
                        </router-link>
                      </li>
                      <li>
                        <router-link to="/Cart" class="dropdown-item">
                          <i class="fa fa-shopping-bag me-2"></i>Giỏ hàng
                        </router-link>
                      </li>
                      <li><WheelRandomCode class="dropdown-item" /></li> -->
                    </ul>
                  </div>
                </template>

                <!-- Đã đăng nhập -->
                <template v-else>
                  <div class="dropdown">
                    <button
                      class="dropdown-toggle d-flex align-items-center"
                      id="accountDropdown"
                      data-bs-toggle="dropdown"
                      aria-expanded="false"
                    >
                      <img
                        src="/src/assets/default/Avtdf.jpg"
                        alt="Avtdf"
                        class="avatar-img"
                        style="width: 40px; height: 40px; border-radius: 50%; margin-right: 8px"
                      />
                      <span></span>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="accountDropdown">
                      <li>
                        <router-link to="/profile" class="dropdown-item"
                          ><i class="fa fa-user me-2"></i>Cập nhật thông tin</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/MyAddresses" class="dropdown-item">
                          <i class="fa fa-map-marker-alt me-2"></i>Địa chỉ</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/favoriteproduct" class="dropdown-item"
                          ><i class="fa fa-heart me-2"></i>Yêu thích</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/Cart" class="dropdown-item"
                          ><i class="fa fa-shopping-bag me-2"></i>Giỏ hàng</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/Order" class="dropdown-item"
                          ><i class="fa fa-box me-2"></i>Đơn hàng</router-link
                        >
                      </li>
                      <li>
                        <router-link to="/my-comments" class="dropdown-item"
                          ><i class="fa fa-comments me-2"></i>Bình luận của tôi</router-link
                        >
                      </li>
                      <li><NavigationUserReview class="dropdown-item" /></li>
                      <WheelRandomCode class="dropdown-item" />
                      <li><hr class="dropdown-divider" /></li>
                      <li>
                        <a href="#" @click.prevent="handleLogout" class="dropdown-item text-danger"
                          ><i class="fa fa-sign-out me-2"></i>Đăng xuất</a
                        >
                      </li>
                    </ul>
                  </div>
                </template>
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

<script>
import { ref, onMounted } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import Cookies from 'js-cookie'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Swal from 'sweetalert2'
import { emitter } from '@/stores/eventBus'

export default {
  components: {
    NavigationUserReview,
    WheelRandomCode,
    RouterLink,
  },
  setup() {
    const router = useRouter()
    const accessToken = ref(Cookies.get('accessToken'))
    const refreshToken = ref(Cookies.get('refreshToken'))
    const isLoggedIn = ref(false)
    const numberCart = ref(0)
    const getUrlAPI = ref(GetApiUrl())

    const checkLogin = async () => {
      if (accessToken.value && refreshToken.value) {
        const result = await validateToken(accessToken.value, refreshToken.value)
        isLoggedIn.value = result.isValid
        if (result.isValid) {
          accessToken.value = result.newAccessToken
          Cookies.set('accessToken', accessToken.value)
        } else {
          Cookies.remove('accessToken')
          Cookies.remove('refreshToken')
        }
      } else {
        isLoggedIn.value = false
      }
    }

    const fetchCart = async () => {
      const validatetoken = await validateToken(accessToken.value, refreshToken.value)
      if (validatetoken.isValid) {
        accessToken.value = validatetoken.newAccessToken
        const readToken = decodeToken(accessToken.value)
        const response = await fetch(`${getUrlAPI.value}/api/Cart/${readToken.IdUser}`, {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
        })
        if (!response.ok) throw new Error('Failed to fetchCart')
        const result = await response.json()
        numberCart.value = result.length
      }
    }

    const handleLogout = () => {
      Swal.fire({
        title: 'Bạn có chắc chắn muốn đăng xuất?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Đăng xuất',
        cancelButtonText: 'Hủy',
      }).then((result) => {
        if (result.isConfirmed) {
          Cookies.remove('accessToken')
          Cookies.remove('refreshToken')
          isLoggedIn.value = false
          Swal.fire({
            title: 'Đăng xuất thành công!',
            icon: 'success',
            confirmButtonColor: '#3085d6',
            timer: 1500,
          }).then(() => {
            router.push('/Login')
          })
        }
      })
    }

    onMounted(() => {
      checkLogin()
      fetchCart()
      emitter.on('cart-updated', fetchCart)
    })

    return {
      isLoggedIn,
      numberCart,
      handleLogout,
    }
  },
}
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

.header__right__auth .dropdown-toggle {
  text-decoration: none;
  color: #333;
  display: flex;
  align-items: center;
  border: none;
  background: none;
  padding: 0;
  cursor: pointer;
}

.header__right__auth .dropdown-toggle::after {
  margin-left: 5px;
}

.header__right__auth .dropdown-menu {
  min-width: 150px;
  border-radius: 5px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.header__right__auth .dropdown-item {
  padding: 8px 16px;
  font-size: 14px;
}

.header__right__auth .dropdown-item:hover {
  background-color: #f8f9fa;
}

.avatar-img {
  vertical-align: middle;
}

.header__right {
  display: flex;
  align-items: center;
  justify-content: space-evenly;
}
/* Cho phép dropdown con hiện ra bên phải */
.dropdown-submenu {
  position: relative;
}

.dropdown-submenu > .dropdown-menu {
  top: 0;
  left: 100%;
  margin-top: -0.1rem;
  display: none;
}

.dropdown-submenu:hover > .dropdown-menu {
  display: block;
}
.dropdown-item::after {
  content: none !important;
}
</style>
