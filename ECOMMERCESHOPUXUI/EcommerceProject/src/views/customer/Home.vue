
<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { jwtDecode } from 'jwt-decode'
import category1 from '@/assets/Customer/img/categories/category-1.jpg'
import category2 from '@/assets/Customer/img/categories/category-2.jpg'
import category3 from '@/assets/Customer/img/categories/category-3.jpg'
import category4 from '@/assets/Customer/img/categories/category-4.jpg'
import category5 from '@/assets/Customer/img/categories/category-5.jpg'
import banner1 from '@/assets/Customer/img/banner/banner-1.jpg'
import insta1 from '@/assets/Customer/img/instagram/insta-1.jpg'
import insta2 from '@/assets/Customer/img/instagram/insta-2.jpg'
import insta3 from '@/assets/Customer/img/instagram/insta-3.jpg'
import insta4 from '@/assets/Customer/img/instagram/insta-4.jpg'
import insta5 from '@/assets/Customer/img/instagram/insta-5.jpg'
import insta6 from '@/assets/Customer/img/instagram/insta-6.jpg'
import { GetApiUrl } from '@/constants/api'

const router = useRouter()
const getUrlAPI = ref(GetApiUrl())
const favoriteStatus = ref({})
const token = Cookies.get('accessToken')
const decodedToken = ReadToken(token)
const idKhachHang = decodedToken ? decodedToken.IdUser : null

function ReadToken(token) {
  if (token) {
    const decoded = jwtDecode(token)
    return {
      IdUser: decoded.sub,
      Phone: decoded.PhoneNumber,
      Name: decoded.FullName,
      Role: decoded.role,
      Exp: decoded.exp,
    }
  }
  return null
}

const setBackgroundImages = () => {
  const elements = document.querySelectorAll('[data-setbg]')
  elements.forEach((element) => {
    const bgImage = element.getAttribute('data-setbg')
    if (bgImage) {
      const imagePath = bgImage.replace('../../assets/img/', '')
      let imageUrl = ''
      switch (imagePath) {
        case 'categories/category-1.jpg': imageUrl = category1; break
        case 'categories/category-2.jpg': imageUrl = category2; break
        case 'categories/category-3.jpg': imageUrl = category3; break
        case 'categories/category-4.jpg': imageUrl = category4; break
        case 'categories/category-5.jpg': imageUrl = category5; break
        case 'banner/banner-1.jpg': imageUrl = banner1; break
        case 'instagram/insta-1.jpg': imageUrl = insta1; break
        case 'instagram/insta-2.jpg': imageUrl = insta2; break
        case 'instagram/insta-3.jpg': imageUrl = insta3; break
        case 'instagram/insta-4.jpg': imageUrl = insta4; break
        case 'instagram/insta-5.jpg': imageUrl = insta5; break
        case 'instagram/insta-6.jpg': imageUrl = insta6; break
      }
      if (imageUrl) {
        element.style.backgroundImage = `url(${imageUrl})`
      }
    }
  })
}

const checkFavoriteProduct = async (maSp) => {
  if (!idKhachHang) return
  try {
    const response = await fetch(`${getUrlAPI.value}/api/Favorite/CheckFavoriteProduct`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        maSp: maSp,
        maKh: idKhachHang,
      }),
    })
    const data = await response.json()
    favoriteStatus.value[maSp] = data.isFavorited || false
  } catch (error) {
    console.error('Lỗi khi kiểm tra sản phẩm yêu thích:', error)
    favoriteStatus.value[maSp] = false
  }
}

const toggleFavoriteProduct = async (maSp) => {
  if (!idKhachHang) {
    Swal.fire({
      title: 'Vui lòng đăng nhập để thêm sản phẩm yêu thích!',
      icon: 'warning',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
    router.push('/Login')
    return
  }

  try {
    const originalState = favoriteStatus.value[maSp] || false
    if (favoriteStatus.value[maSp]) {
      // Remove from favorites
      const response = await fetch(`${getUrlAPI.value}/api/Favorite/DeleteFavoriteProducts`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          maKh: idKhachHang,
          maSp: maSp,
        }),
      })
      const data = await response.json()
      if (response.ok && data.success !== false) {
        favoriteStatus.value[maSp] = false
        Swal.fire({
          title: 'Đã xóa khỏi danh sách yêu thích!',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      } else {
        throw new Error(data.message || 'Failed to remove from favorites')
      }
    } else {
      // Add to favorites
      const response = await fetch(`${getUrlAPI.value}/api/Favorite/AddFavoriteProduct`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          maSp: maSp,
          maKh: idKhachHang,
        }),
      })
      const data = await response.json()
      if (response.ok && data.success !== false) {
        favoriteStatus.value[maSp] = true
        Swal.fire({
          title: 'Đã thêm vào danh sách yêu thích!',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      } else {
        throw new Error(data.message || 'Failed to add to favorites')
      }
    }
  } catch (error) {
    console.error('Error toggling favorite:', error)
    Swal.fire({
      title: 'Sản phẩm đã có trong danh sách yêu thích',
      text: 'Cảm ơn bạn đã quan tâm đến sản phẩm này',
      icon: 'info',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
    favoriteStatus.value[maSp] = originalState // Rollback on error
  }
}

const ListNewProducts = ref([])
const ListBestSellerProducts = ref([])
const ListBestHotProducts = ref([])

const fetchAPINewProduts = async () => {
  try {
    const response = await fetch(`${getUrlAPI.value}/api/Home/GetNewProduct`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) throw new Error('Failed to fetch new products')
    ListNewProducts.value = await response.json()
  } catch (error) {
    console.error('Error fetching new products:', error)
  }
}

const fetchAPIBestSellerProduts = async () => {
  try {
    const response = await fetch(`${getUrlAPI.value}/api/Home/GetBestSellerProduct`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) throw new Error('Failed to fetch best seller products')
    ListBestSellerProducts.value = await response.json()
  } catch (error) {
    console.error('Error fetching best seller products:', error)
  }
}

const fetchAPIFavoriteProduts = async () => {
  try {
    const response = await fetch(`${getUrlAPI.value}/api/Home/GetFavoriteProduct`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) throw new Error('Failed to fetch favorite products')
    ListBestHotProducts.value = await response.json()
  } catch (error) {
    console.error('Error fetching favorite products:', error)
  }
}

const parsePrice = (priceString) => {
  if (!priceString) return 0
  const match = priceString.match(/(\d+(?:\.\d+)*)/g)
  return match && match.length > 0 ? parseInt(match[0].replace(/\./g, '')) : 0
}

const formatPrice = (price) => {
  return price.toLocaleString('vi-VN') + ' VNĐ'
}

const formatRating = (rating) => {
  if (rating === null || rating === undefined) {
    return '5.0'; // Default to 5 if no rating
  }
  // Round to nearest 0.5
  const rounded = Math.round(rating * 2) / 2;
  return rounded.toFixed(1);
};

const currentSlide = ref(0)
const slideWidth = 300
const itemsPerView = 4

const maxSlides = computed(() => {
  return Math.max(0, Math.ceil(ListBestHotProducts.value.length / itemsPerView) - 1)
})

const slideLeft = () => {
  if (currentSlide.value > 0) currentSlide.value--
}

const slideRight = () => {
  if (currentSlide.value < maxSlides.value) currentSlide.value++
}

const startAutoSlide = () => {
  setInterval(() => {
    if (currentSlide.value >= maxSlides.value) {
      currentSlide.value = 0
    } else {
      currentSlide.value++
    }
  }, 5000)
}

const countdown = ref({
  days: 3,
  hours: 23,
  minutes: 19,
  seconds: 56,
})

const startCountdown = () => {
  setInterval(() => {
    if (countdown.value.seconds > 0) {
      countdown.value.seconds--
    } else if (countdown.value.minutes > 0) {
      countdown.value.minutes--
      countdown.value.seconds = 59
    } else if (countdown.value.hours > 0) {
      countdown.value.hours--
      countdown.value.minutes = 59
      countdown.value.seconds = 59
    } else if (countdown.value.days > 0) {
      countdown.value.days--
      countdown.value.hours = 23
      countdown.value.minutes = 59
      countdown.value.seconds = 59
    }
  }, 1000)
}

onMounted(() => {
  setBackgroundImages()
  startCountdown()
  startAutoSlide()
  Promise.all([
    fetchAPINewProduts(),
    fetchAPIBestSellerProduts(),
    fetchAPIFavoriteProduts()
  ]).then(() => {
    setTimeout(() => {
      ListNewProducts.value.forEach((item) => checkFavoriteProduct(item.maSp))
      ListBestHotProducts.value.forEach((item) => checkFavoriteProduct(item.maSp))
      ListBestSellerProducts.value.forEach((item) => checkFavoriteProduct(item.maSp))
    }, 100)
  })
})
</script>

<template>
  <div>
    <!-- Categories Section Begin -->
    <section class="categories" style="font-family: Arial, Helvetica, sans-serif">
      <div class="container-fluid">
        <div class="row">
          <div class="col-lg-6 p-0">
            <div class="categories__item categories__large__item set-bg animated-category"
              data-setbg="../../assets/img/categories/category-1.jpg">
              <div class="categories__text">
                <h1 style="font-family: Arial, Helvetica, sans-serif; font-size: 36px">
                  Thời trang nữ
                </h1>
                <p style="color: #000; font-size: 28px">
                  Khám phá phong cách thời trang dành riêng cho phái đẹp.
                </p>
                <router-link style="text-decoration-line: none; font-size: 18px; font-weight: bold" to="/Shop">
                  Mua ngay
                </router-link>
              </div>
              <div class="category-icon"><i class="fas fa-female fa-3x"></i></div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="row">
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-2.jpg">
                  <div class="categories__text">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">
                      Thời trang nam
                    </h4>
                    <router-link to="/Shop" style="text-decoration: none; font-size: 15px; font-weight: bold">
                      Mua ngay
                    </router-link>
                  </div>
                  <div class="category-icon">
                    <i class="fas fa-male fa-3x"></i>
                  </div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-3.jpg">
                  <div class="categories__text">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">
                      Thời trang trẻ em
                    </h4>
                    <router-link style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop">Mua ngay</router-link>
                  </div>
                  <div class="category-icon"><i class="fas fa-child fa-3x"></i></div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0" style="height: 327px">
                <div class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-4.jpg">
                  <div class="categories__text" style="margin-top: 100px">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">
                      Giày dép
                    </h4>
                    <router-link style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop">Mua ngay</router-link>
                  </div>
                  <div class="category-icon"><i class="fas fa-shoe-prints fa-3x"></i></div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-5.jpg">
                  <div class="categories__text" style="margin-top: 100px">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">
                      Phụ kiện
                    </h4>
                    <router-link style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop">Mua ngay</router-link>
                  </div>
                  <div class="category-icon"><i class="fas fa-glasses fa-3x"></i></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Categories Section End -->

    <!-- Product Section Begin -->
    <section class="product spad">
      <div class="" style="margin-left: 100px; margin-right: 100px">
        <!-- Flash Sale Header -->
        <div class="row align-items-center mb-4">
          <div class="col-md-6">
            <div class="d-flex align-items-center">
              <h3 class="mb-0 fw-bold angel-text-gradient">
                <i class="fa fa-heart"></i> Được yêu thích nhiều nhất
              </h3>
            </div>
          </div>
          <div class="col-md-2 text-end">
            <button class="btn btn-outline-angel rounded-circle me-2" style="width: 45px; height: 45px"
              @click="slideLeft" :disabled="currentSlide === 0">
              <i class="fas fa-chevron-left"></i>
            </button>
            <button class="btn btn-outline-angel rounded-circle" style="width: 45px; height: 45px" @click="slideRight"
              :disabled="currentSlide >= maxSlides">
              <i class="fas fa-chevron-right"></i>
            </button>
          </div>
        </div>

        <!-- Products Slider Container -->
        <div class="product-slider-container position-relative">
          <div class="product-slider-wrapper overflow-hidden">
            <div class="product-slider d-flex transition-all"
              :style="{ transform: `translateX(-${currentSlide * slideWidth}px)` }">
              <div class="product-slide flex-shrink-0 me-3" v-for="item in ListBestHotProducts" :key="item.maSp" style="
                  width: 280px;
                  height: 470px;
                  margin-bottom: 10px;
                  background-color: #ffffff;
                  border-radius: 12px;
                  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                  overflow: hidden;
                ">
                <div class="product__item" style="border-radius: 12px; position: relative">
                  <div class="product__item__pic position-relative animated-product" style="height: 320px">
                    <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                      :alt="item.tenSanPham" class="w-100 h-100"
                      style="object-fit: cover; border-radius: 12px 12px 0 0" />
                    <div class="discount-badge position-absolute top-0 start-0 m-2">
                      <div style="
                          background-color: #dc3545;
                          color: white;
                          padding: 4px 8px;
                          border-radius: 6px;
                          display: flex;
                          align-items: center;
                          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                          font-size: 12px;
                          font-weight: 500;
                        ">
                        <i class="fa fa-heart" style="margin-right: 4px"></i>
                        Yêu Thích
                      </div>
                    </div>
                    <div style="
                        position: absolute;
                        bottom: 8px;
                        right: 8px;
                        background: linear-gradient(45deg, #28a745, #20c997);
                        color: white;
                        padding: 3px 6px;
                        border-radius: 4px;
                        font-size: 10px;
                        font-weight: bold;
                      ">
                      Còn {{ item.soLuong || 23 }} sản phẩm
                    </div>
                    <ul class="product__hover">
                      <li>
                        <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                          <span :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                            style="color: #ec4e79; font-size: 20px; transition: 0.3s"></span>
                        </a>
                      </li>
                    </ul>
                  </div>
                  <div class="product__item__text text-center pt-3" style="padding: 16px">
                    <h6 class="mb-2">
                      <router-link :to="`/product/${item.maSp}`" style="
                          text-decoration-line: none;
                          color: #333;
                          font-size: 1.1rem;
                          font-weight: 600;
                        ">
                        {{ item.tenSanPham }}
                      </router-link>
                    </h6>
                    <div class="d-flex align-items-center justify-content-center gap-2 mb-3">
                      <div class="product__price text-danger fw-bold fs-5">
                        {{ formatPrice(parsePrice(item.khoangGia)) }}
                      </div>
                    </div>
                    <div style="
                        display: flex;
                        justify-content: space-between;
                        margin-bottom: 12px;
                        padding: 8px 12px;
                        background-color: #f8f9fa;
                        border-radius: 6px;
                        font-size: 12px;
                      ">
                      <div style="display: flex; align-items: center; color: #666">
                        <i class="fas fa-star" style="color: #ffc107; margin-right: 4px"></i>
                        {{ formatRating(item.averageRating) }} ({{ item.reviewCount }})
                      </div>
                      <div style="color: #28a745; font-weight: 600">
                        Đã bán {{ item.soLuongBan }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Product Section End -->

    <!-- Banner Section Begin -->
    <section class="set-bg" style="position: relative; margin-bottom: 50px">
      <img src="../../assets/Customer/img/banner/banner-1.jpg" class="animated-banner"
        style="width: 100%; height: 370px" />
      <div class="banner-icon"><i class="fas fa-tags fa-3x"></i></div>
      <div class="container" style="
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          text-align: center;
          color: white;
        ">
        <div class="row">
          <div class="col-12">
            <div class="banner__slider owl-carousel">
              <div class="banner__item" style="margin-bottom: 160px">
                <div class="banner__text">
                  <span>Bộ Sưu Tập</span>
                  <div class="col-xl-3 col-lg-2" style="width: 300px; margin-left: 430px">
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
                      <RouterLink to="/" style="text-decoration: none">
                        <text x="50%" y="60%" dominant-baseline="middle" text-anchor="middle" class="angel-text">
                          Angel Fashion
                        </text>
                      </RouterLink>
                    </svg>
                  </div>
                  <a href="/Shop" style="color: black; text-decoration: none">Mua ngay</a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Banner Section End -->

    <!-- Trend Section Begin -->
    <section class="trend spad" style="margin-top: -100px">
      <div class="container-fluid px-5">
        <!-- Đang hot Section -->
        <div class="mb-5">
          <div class="row align-items-center mb-4">
            <div class="col">
              <h3 class="fw-bold mb-0 angel-text-gradient">
                <i class="fas fa-clock"></i> Sản phẩm mới nhất
              </h3>
            </div>
          </div>
          <div class="row">
            <div class="col-md-2">
              <div class="banner-container h-100">
                <img src="/src/assets/images/Beige Minimalist Fashion Business Banner (1).png" alt="Fashion Banner"
                  class="w-100 h-100 object-fit-cover" style="min-height: 580px" />
                <div class="banner-icon"><i class="fas fa-ad fa-3x"></i></div>
              </div>
            </div>
            <div class="col-md-10">
              <div class="row g-3 mb-4">
                <div class="col-md-3" v-for="item in ListNewProducts.slice(0, 4)" :key="item.maSp">
                  <div class="hot-product-item" style="
                      width: 280px;
                      height: 470px;
                      margin-bottom: 10px;
                      background-color: #ffffff;
                      border-radius: 12px;
                      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                      overflow: hidden;
                    ">
                    <div class="product__item" style="border-radius: 12px; position: relative">
                      <div class="product__item__pic position-relative animated-product" style="height: 320px">
                        <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                          :alt="item.tenSanPham" class="w-100 h-100"
                          style="object-fit: cover; border-radius: 12px 12px 0 0" />
                        <div class="discount-badge position-absolute top-0 start-0 m-2">
                          <div style="
                              background-color: #dc3545;
                              color: white;
                              padding: 4px 8px;
                              border-radius: 6px;
                              display: flex;
                              align-items: center;
                              box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                              font-size: 12px;
                              font-weight: bold;
                              letter-spacing: 0.5px;
                            ">
                            NEW
                          </div>
                        </div>
                        <div style="
                            position: absolute;
                            bottom: 8px;
                            right: 8px;
                            background: linear-gradient(45deg, #28a745, #20c997);
                            color: white;
                            padding: 3px 6px;
                            border-radius: 4px;
                            font-size: 10px;
                            font-weight: bold;
                          ">
                          Còn {{ item.soLuong || 23 }} sản phẩm
                        </div>
                        <ul class="product__hover">
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                                style="color: #ec4e79; font-size: 20px; transition: 0.3s"></span>
                            </a>
                          </li>
                        </ul>
                      </div>
                      <div class="product__item__text text-center pt-3" style="padding: 10px">
                        <h6 class="mb-2">
                          <router-link :to="`/product/${item.maSp}`" style="
                              text-decoration-line: none;
                              color: #333;
                              font-size: 1.1rem;
                              font-weight: 600;
                            ">
                            {{ item.tenSanPham }}
                          </router-link>
                        </h6>
                        <div class="d-flex align-items-center justify-content-center gap-2 mb-3">
                          <div class="product__price text-danger fw-bold fs-5">
                            {{ formatPrice(parsePrice(item.khoangGia)) }}
                          </div>
                        </div>
                        <div style="
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 12px;
                            padding: 8px 12px;
                            background-color: #f8f9fa;
                            border-radius: 6px;
                            font-size: 12px;
                          ">
                          <div style="display: flex; align-items: center; color: #666">
                            <i class="fas fa-star" style="color: #ffc107; margin-right: 4px"></i>
                            {{ formatRating(item.averageRating) }} ({{ item.reviewCount }})
                          </div>
                          <div style="color: #28a745; font-weight: 600">
                            Đã bán {{ item.soLuongBan }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="text-center mb-4">
                <hr style="border: none; border-top: 2px dashed #000" />
              </div>
              <div class="row g-3">
                <div class="col-md-3" v-for="item in ListNewProducts.slice(4, 8)" :key="item.maSp">
                  <div class="hot-product-item" style="
                      width: 280px;
                      height: 470px;
                      margin-bottom: 10px;
                      background-color: #ffffff;
                      border-radius: 12px;
                      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                      overflow: hidden;
                    ">
                    <div class="product__item" style="border-radius: 12px; position: relative">
                      <div class="product__item__pic position-relative animated-product" style="height: 320px">
                        <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                          :alt="item.tenSanPham" class="w-100 h-100"
                          style="object-fit: cover; border-radius: 12px 12px 0 0" />
                        <div class="discount-badge position-absolute top-0 start-0 m-2">
                          <div style="
                              background-color: #dc3545;
                              color: white;
                              padding: 4px 8px;
                              border-radius: 6px;
                              display: flex;
                              align-items: center;
                              box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                              font-size: 12px;
                              font-weight: bold;
                              letter-spacing: 0.5px;
                            ">
                            NEW
                          </div>
                        </div>
                        <div style="
                            position: absolute;
                            bottom: 8px;
                            right: 8px;
                            background: linear-gradient(45deg, #28a745, #20c997);
                            color: white;
                            padding: 3px 6px;
                            border-radius: 4px;
                            font-size: 10px;
                            font-weight: bold;
                          ">
                          Còn {{ item.soLuong || 23 }} sản phẩm
                        </div>
                        <ul class="product__hover">
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                                style="color: #ec4e79; font-size: 20px; transition: 0.3s"></span>
                            </a>
                          </li>
                        </ul>
                      </div>
                      <div class="product__item__text text-center pt-3" style="padding: 16px">
                        <h6 class="mb-2">
                          <router-link :to="`/product/${item.maSp}`" style="
                              text-decoration-line: none;
                              color: #333;
                              font-size: 1.1rem;
                              font-weight: 600;
                            ">
                            {{ item.tenSanPham }}
                          </router-link>
                        </h6>
                        <div class="d-flex align-items-center justify-content-center gap-2 mb-3">
                          <div class="product__price text-danger fw-bold fs-5">
                            {{ formatPrice(parsePrice(item.khoangGia)) }}
                          </div>
                        </div>
                        <div style="
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 12px;
                            padding: 8px 12px;
                            background-color: #f8f9fa;
                            border-radius: 6px;
                            font-size: 12px;
                          ">
                          <div style="display: flex; align-items: center; color: #666">
                            <i class="fas fa-star" style="color: #ffc107; margin-right: 4px"></i>
                            {{ formatRating(item.averageRating) }} ({{ item.reviewCount }})
                          </div>
                          <div style="color: #28a745; font-weight: 600">
                            Đã bán {{ item.soLuongBan }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Banner Section -->
        <div class="row mb-5">
          <div class="col-12">
            <div class="row g-4">
              <div>
                <div class="position-relative overflow-hidden rounded-4 angel-banner animated-banner">
                  <img src="/src/assets/images/banner ngang.jpg" alt="Angel Fashion Trends" class="w-100"
                    style="height: 350px; object-fit: cover" />
                  <div class="banner-icon"><i class="fas fa-star fa-3x"></i></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Sản phẩm bán chạy Section -->
        <div class="mb-5">
          <div class="row align-items-center mb-4">
            <div class="col">
              <h3 class="fw-bold mb-0 angel-text-gradient">
                <i class="fas fa-chart-line text-warning"></i> Sản phẩm bán chạy
              </h3>
            </div>
          </div>
          <div class="row">
            <div class="col-md-2">
              <div class="position-relative overflow-hidden rounded-3 h-100 animated-banner">
                <img src="https://i.pinimg.com/1200x/17/12/e0/1712e06978a02432d83d400d6bded81a.jpg"
                  alt="Best Seller Banner" class="w-100 h-100 object-fit-cover" style="min-height: 580px" />
                <div class="banner-icon"><i class="fas fa-ad fa-3x"></i></div>
              </div>
            </div>
            <div class="col-md-10">
              <div class="row g-3 mb-4">
                <div class="col-md-3" v-for="(item, index) in ListBestSellerProducts.slice(0, 4)" :key="item.maSp">
                  <div class="hot-product-item" style="
                      width: 280px;
                      height: 470px;
                      margin-bottom: 10px;
                      background-color: #ffffff;
                      border-radius: 12px;
                      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                      overflow: hidden;
                    ">
                    <div class="product__item" style="border-radius: 12px; position: relative">
                      <div class="product__item__pic position-relative animated-product" style="height: 320px">
                        <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                          :alt="item.tenSanPham" class="w-100 h-100"
                          style="object-fit: cover; border-radius: 12px 12px 0 0" />
                        <div v-if="index === 0" style="
                            position: absolute;
                            top: 8px;
                            left: 8px;
                            background-color: #ff6b35;
                            color: white;
                            padding: 4px 8px;
                            border-radius: 0 0 8px 0;
                            font-weight: bold;
                            font-size: 12px;
                            z-index: 10;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                          ">
                          <div style="font-size: 10px; line-height: 1">TOP</div>
                          <div style="font-size: 14px; line-height: 1; font-weight: 900">1</div>
                        </div>
                        <div v-else-if="index === 1" style="
                            position: absolute;
                            top: 8px;
                            left: 8px;
                            background-color: #ff6b35;
                            color: white;
                            padding: 4px 8px;
                            border-radius: 0 0 8px 0;
                            font-weight: bold;
                            font-size: 12px;
                            z-index: 10;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                          ">
                          <div style="font-size: 10px; line-height: 1">TOP</div>
                          <div style="font-size: 14px; line-height: 1; font-weight: 900">2</div>
                        </div>
                        <div v-else-if="index === 2" style="
                            position: absolute;
                            top: 8px;
                            left: 8px;
                            background-color: #ff6b35;
                            color: white;
                            padding: 4px 8px;
                            border-radius: 0 0 8px 0;
                            font-weight: bold;
                            font-size: 12px;
                            z-index: 10;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                          ">
                          <div style="font-size: 10px; line-height: 1">TOP</div>
                          <div style="font-size: 14px; line-height: 1; font-weight: 900">3</div>
                        </div>
                        <div v-else-if="index === 3" style="
                            position: absolute;
                            top: 8px;
                            left: 8px;
                            background-color: #ff6b35;
                            color: white;
                            padding: 4px 8px;
                            border-radius: 0 0 8px 0;
                            font-weight: bold;
                            font-size: 12px;
                            z-index: 10;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                          ">
                          <div style="font-size: 10px; line-height: 1">TOP</div>
                          <div style="font-size: 14px; line-height: 1; font-weight: 900">4</div>
                        </div>
                        <div style="
                            position: absolute;
                            bottom: 8px;
                            right: 8px;
                            background: linear-gradient(45deg, #28a745, #20c997);
                            color: white;
                            padding: 3px 6px;
                            border-radius: 4px;
                            font-size: 10px;
                            font-weight: bold;
                          ">
                          Còn {{ item.soLuong || 23 }} sản phẩm
                        </div>
                        <ul class="product__hover">
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                                style="color: #ec4e79; font-size: 20px; transition: 0.3s"></span>
                            </a>
                          </li>
                        </ul>
                      </div>
                      <div class="product__item__text text-center pt-3" style="padding: 16px">
                        <h6 class="mb-2">
                          <router-link :to="`/product/${item.maSp}`" style="
                              text-decoration-line: none;
                              color: #333;
                              font-size: 1.1rem;
                              font-weight: 600;
                            ">
                            {{ item.tenSanPham }}
                          </router-link>
                        </h6>
                        <div class="d-flex align-items-center justify-content-center gap-2 mb-3">
                          <div class="product__price text-danger fw-bold fs-5">
                            {{ formatPrice(parsePrice(item.khoangGia)) }}
                          </div>
                        </div>
                        <div style="
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 12px;
                            padding: 8px 12px;
                            background-color: #f8f9fa;
                            border-radius: 6px;
                            font-size: 12px;
                          ">
                          <div style="display: flex; align-items: center; color: #666">
                            <i class="fas fa-star" style="color: #ffc107; margin-right: 4px"></i>
                            {{ formatRating(item.averageRating) }} ({{ item.reviewCount }})
                          </div>
                          <div style="color: #28a745; font-weight: 600">
                            Đã bán {{ item.soLuongBan }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="text-center mb-4">
                <hr style="border: none; border-top: 2px dashed #000" />
              </div>
              <div class="row g-3">
                <div class="col-md-3" v-for="(item, index) in ListBestSellerProducts.slice(4, 8)" :key="item.maSp">
                  <div class="hot-product-item" style="
                      width: 280px;
                      height: 470px;
                      margin-bottom: 10px;
                      background-color: #ffffff;
                      border-radius: 12px;
                      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                      overflow: hidden;
                    ">
                    <div class="product__item" style="border-radius: 12px; position: relative">
                      <div class="product__item__pic position-relative animated-product" style="height: 320px">
                        <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                          :alt="item.tenSanPham" class="w-100 h-100"
                          style="object-fit: cover; border-radius: 12px 12px 0 0" />
                        <div v-if="index === 0" style="
                            position: absolute;
                            top: 8px;
                            left: 8px;
                            background-color: #ff6b35;
                            color: white;
                            padding: 4px 8px;
                            border-radius: 0 0 8px 0;
                            font-weight: bold;
                            font-size: 12px;
                            z-index: 10;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
                          ">
                          <div style="font-size: 10px; line-height: 1">TOP</div>
                          <div style="font-size: 14px; line-height: 1; font-weight: 900">5</div>
                        </div>
                        <div style="
                            position: absolute;
                            bottom: 8px;
                            right: 8px;
                            background: linear-gradient(45deg, #28a745, #20c997);
                            color: white;
                            padding: 3px 6px;
                            border-radius: 4px;
                            font-size: 10px;
                            font-weight: bold;
                          ">
                          Còn {{ item.soLuong || 23 }} sản phẩm
                        </div>
                        <ul class="product__hover">
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                                style="color: #ec4e79; font-size: 20px; transition: 0.3s"></span>
                            </a>
                          </li>
                        </ul>
                      </div>
                      <div class="product__item__text text-center pt-3" style="padding: 16px">
                        <h6 class="mb-2">
                          <router-link :to="`/product/${item.maSp}`" style="
                              text-decoration-line: none;
                              color: #333;
                              font-size: 1.1rem;
                              font-weight: 600;
                            ">
                            {{ item.tenSanPham }}
                          </router-link>
                        </h6>
                        <div class="d-flex align-items-center justify-content-center gap-2 mb-3">
                          <div class="product__price text-danger fw-bold fs-5">
                            {{ formatPrice(parsePrice(item.khoangGia)) }}
                          </div>
                        </div>
                        <div style="
                            display: flex;
                            justify-content: space-between;
                            margin-bottom: 12px;
                            padding: 8px 12px;
                            background-color: #f8f9fa;
                            border-radius: 6px;
                            font-size: 12px;
                          ">
                          <div style="display: flex; align-items: center; color: #666">
                            <i class="fas fa-star" style="color: #ffc107; margin-right: 4px"></i>
                            {{ formatRating(item.averageRating) }} ({{ item.reviewCount }})
                          </div>
                          <div style="color: #28a745; font-weight: 600">
                            Đã bán {{ item.soLuongBan }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Trend Section End -->
  </div>

</template>

<style>
.ranking-badge {
  transition: all 0.3s ease;
  font-family: 'Arial', sans-serif;
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.ranking-badge:hover {
  transform: scale(1.1);
}

.animated-product:hover .ranking-badge {
  transform: scale(1.05);
}

.product__item__pic {
  height: 300px;
  position: relative;
  overflow: hidden;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.product__item__pic img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.product__item__pic:hover img {
  transform: scale(1.05);
}

.categories__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 300px;
  position: relative;
  transition: opacity 0.3s ease;
}

.categories__item:hover {
  opacity: 0.9;
}

.categories__large__item {
  min-height: 600px;
}

.banner {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 500px;
  position: relative;
}

.banner img {
  transition: transform 0.5s ease;
}

.banner:hover img {
  transform: rotate(2deg) scale(1.02);
}

.instagram__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 200px;
  position: relative;
}

.trend__content {
  min-height: 520px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  padding: 24px 12px 18px 12px;
  margin-bottom: 24px;
}

.trend__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  margin-bottom: 18px;
  min-height: 220px;
}

.trend__item__pic {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 120px;
  height: 180px;
  overflow: hidden;
  background: #f8f8f8;
  border-radius: 8px;
  margin-bottom: 10px;
}

.trend__item__pic img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  display: block;
  margin: 0 auto;
  transition: transform 0.3s ease;
}

.trend__item__pic:hover img {
  transform: scale(1.1);
}

.trend__item__text {
  text-align: center;
  width: 100%;
}

.section-title h4 {
  text-align: center;
  width: 100%;
}

@media (max-width: 991px) {
  .trend__content {
    min-height: 0;
    padding: 18px 6px 12px 6px;
  }
}

.bg-gradient-angel {
  background: linear-gradient(135deg, #ec4e79, #aba2b7, #5ccae7) !important;
}

.angel-text-gradient {
  background: linear-gradient(135deg, #ec4e79, #aba2b7);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.angel-accent {
  color: #ec4e79 !important;
}

.btn-outline-angel {
  border-color: #ec4e79;
  color: #ec4e79;
}

.btn-outline-angel:hover {
  background-color: #ec4e79;
  border-color: #ec4e79;
  color: white;
}

.animated-category {
  position: relative;
}

.category-icon {
  position: absolute;
  bottom: 20px;
  right: 20px;
  color: white;
  opacity: 0.7;
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.categories__item:hover .category-icon {
  transform: scale(1.2);
  opacity: 1;
}

.animated-product img {
  transition: transform 0.3s ease, filter 0.3s ease;
}

.animated-product:hover img {
  transform: scale(1.05);
  filter: brightness(1.1);
}

.animated-banner img {
  transition: transform 0.5s ease;
}

.animated-banner:hover img {
  transform: scale(1.03);
}

.animated-icon {
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

.banner-icon {
  position: absolute;
  bottom: 20px;
  right: 20px;
  color: white;
  opacity: 0.7;
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.position-relative:hover .banner-icon {
  transform: scale(1.2);
  opacity: 1;
}

.product-icon {
  opacity: 0.8;
  transition: transform 0.3s ease;
}

.product__item__pic:hover .product-icon {
  transform: scale(1.2);
}
</style>
