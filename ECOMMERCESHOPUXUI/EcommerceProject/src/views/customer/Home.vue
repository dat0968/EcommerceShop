<script setup>
import { ref, onMounted, computed, watch } from 'vue'
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
const getUrlAPI = ref(GetApiUrl())
const favoriteStatus = ref({})
const setBackgroundImages = () => {
  const elements = document.querySelectorAll('[data-setbg]')
  elements.forEach((element) => {
    const bgImage = element.getAttribute('data-setbg')
    if (bgImage) {
      const imagePath = bgImage.replace('../../assets/img/', '')
      let imageUrl = ''

      switch (imagePath) {
        case 'categories/category-1.jpg':
          imageUrl = category1
          break
        case 'categories/category-2.jpg':
          imageUrl = category2
          break
        case 'categories/category-3.jpg':
          imageUrl = category3
          break
        case 'categories/category-4.jpg':
          imageUrl = category4
          break
        case 'categories/category-5.jpg':
          imageUrl = category5
          break
        case 'banner/banner-1.jpg':
          imageUrl = banner1
          break
        case 'instagram/insta-1.jpg':
          imageUrl = insta1
          break
        case 'instagram/insta-2.jpg':
          imageUrl = insta2
          break
        case 'instagram/insta-3.jpg':
          imageUrl = insta3
          break
        case 'instagram/insta-4.jpg':
          imageUrl = insta4
          break
        case 'instagram/insta-5.jpg':
          imageUrl = insta5
          break
        case 'instagram/insta-6.jpg':
          imageUrl = insta6
          break
      }

      if (imageUrl) {
        element.style.backgroundImage = `url(${imageUrl})`
      }
    }
  })
}
function ReadToken(token) {
  if (token) {
    const decoded = jwtDecode(token)
    return {
      IdUser: decoded.sub,
      Phone: decoded.PhoneNumber,
      Name: decoded.FullName,
      Role: decoded.role,
      Exp: decoded.exp, // Đơn vị giây
    }
  }
  return null
}
const token = Cookies.get('accessToken')
const decodedToken = ReadToken(token)
const idKhachHang = decodedToken ? decodedToken.IdUser : null
// Countdown timer state
const countdown = ref({
  days: 3,
  hours: 23,
  minutes: 19,
  seconds: 56,
})

// Initialize countdown timer
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

// Trong onMounted thêm:
onMounted(() => {
  // ... các code khác
  startCountdown()
})
const checkFavoriteProduct = async (maSp) => {
  if (!idKhachHang) return
  try {
    const response = await fetch(getUrlAPI.value + `/api/Favorite/CheckFavoriteProduct`, {
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
    favoriteStatus.value[maSp] = data
  } catch (error) {
    console.error('Lỗi khi kiểm tra sản phẩm yêu thích:', error)
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
    if (favoriteStatus.value[maSp] == true) {
      const response = await fetch(getUrlAPI.value + '/api/Favorite/DeleteFavoriteProducts', {
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
      if (response.ok) {
        favoriteStatus.value[maSp] = !favoriteStatus.value[maSp]

        Swal.fire({
          title: 'Đã xóa khỏi danh sách yêu thích!',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      } else {
        Swal.fire({
          title: data.message || 'Đã xảy ra lỗi!',
          icon: 'error',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      }
    } else if (favoriteStatus.value[maSp] == false) {
      const response = await fetch(getUrlAPI.value + '/api/Favorite/AddFavoriteProduct', {
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
      if (response.ok) {
        favoriteStatus.value[maSp] = !favoriteStatus.value[maSp]

        Swal.fire({
          title: 'Đã thêm vào danh sách yêu thích!',

          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      } else {
        Swal.fire({
          title: data.message || 'Đã xảy ra lỗi!',
          icon: 'error',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      }
    }
  } catch (error) {
    Swal.fire({
      title: 'Lỗi khi xử lý yêu thích!',
      text: error.message,
      icon: 'error',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
  }
}

const ListNewProducts = ref([])
const ListBestSellerProducts = ref([])
const ListBestHotProducts = ref([])
const fetchAPINewProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/api/Home/GetNewProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListNewProducts.value = result
}

const fetchAPIBestSellerProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/api/Home/GetBestSellerProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListBestSellerProducts.value = result
}

const fetchAPIHotProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/api/Home/GetHotProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListBestHotProducts.value = result
}
// Thêm vào script setup
const productDiscounts = ref({}) // Store fixed discount percentages for each product

// Calculate original price and discount percentage (fixed per product)
// const calculatePriceInfo = (currentPrice, productId) => {
//   if (productDiscounts.value[productId]) {
//     return productDiscounts.value[productId]
//   }

//   const discountPercentages = [30, 10, 25]
//   const randomDiscount = discountPercentages[Math.floor(Math.random() * discountPercentages.length)]
//   const originalPrice = Math.round(currentPrice * (1 + randomDiscount / 100))
//   productDiscounts.value[productId] = {
//     originalPrice,
//     discountPercentage: randomDiscount,
//   }

//   return productDiscounts.value[productId]
// }

// Parse price from string
const parsePrice = (priceString) => {
  if (!priceString) return 0
  // Extract number from price string like "100.000vnđ - 200.000vnđ"
  const match = priceString.match(/(\d+(?:\.\d+)*)/g)
  if (match && match.length > 0) {
    return parseInt(match[0].replace(/\./g, ''))
  }
  return 0
}
// Thêm vào script setup
const currentSlide = ref(0)
const slideWidth = 300 // Width của mỗi slide + margin
const itemsPerView = 4 // Số items hiển thị cùng lúc

const maxSlides = computed(() => {
  return Math.max(0, Math.ceil(ListNewProducts.value.length / itemsPerView) - 1)
})

const totalSlides = computed(() => {
  return Math.ceil(ListNewProducts.value.length / itemsPerView)
})

const slideLeft = () => {
  if (currentSlide.value > 0) {
    currentSlide.value--
  }
}

const slideRight = () => {
  if (currentSlide.value < maxSlides.value) {
    currentSlide.value++
  }
}

const goToSlide = (index) => {
  currentSlide.value = index
}

// Auto-slide functionality (optional)
const startAutoSlide = () => {
  setInterval(() => {
    if (currentSlide.value >= maxSlides.value) {
      currentSlide.value = 0
    } else {
      currentSlide.value++
    }
  }, 5000) // Change slide every 5 seconds
}

// Thêm vào onMounted
onMounted(() => {
  // ... existing code
  startAutoSlide() // Uncomment if you want auto-sliding
})
// Format price to Vietnamese format
const formatPrice = (price) => {
  return price.toLocaleString('vi-VN') + 'vnđ'
}
onMounted(() => {
  setBackgroundImages(), fetchAPINewProduts()
  fetchAPIBestSellerProduts()
  fetchAPIHotProduts()
  setTimeout(() => {
    ListNewProducts.value.forEach((item) => {
      // calculatePriceInfo(parsePrice(item.khoangGia), item.maSp)
      checkFavoriteProduct(item.maSp)
    })
    ListBestHotProducts.value.forEach((item) => {
      checkFavoriteProduct(item.maSp)
    })
    ListBestSellerProducts.value.forEach((item) => {
      checkFavoriteProduct(item.maSp)
    })
  }, 100)
})
</script>
<template>
  <div>
    <!-- Categories Section Begin -->
    <section class="categories" style="font-family: Arial, Helvetica, sans-serif">
      <div class="container-fluid">
        <div class="row">
          <div class="col-lg-6 p-0">
            <div
              class="categories__item categories__large__item set-bg animated-category"
              data-setbg="../../assets/img/categories/category-1.jpg"
            >
              <div class="categories__text">
                <h1 style="font-family: Arial, Helvetica, sans-serif; font-size: 36px">
                  Thời trang nữ
                </h1>
                <p style="color: #000; font-size: 28px">
                  Khám phá phong cách thời trang dành riêng cho phái đẹp.
                </p>
                <router-link
                  style="text-decoration-line: none; font-size: 18px; font-weight: bold"
                  to="/Shop"
                >
                  Mua ngay
                </router-link>
              </div>

              <div class="category-icon"><i class="fas fa-female fa-3x"></i></div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="row">
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-2.jpg"
                >
                  <div class="categories__text">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">
                      Thời trang nam
                    </h4>
                    <router-link
                      to="/Shop"
                      style="text-decoration: none; font-size: 15px; font-weight: bold"
                    >
                      Mua ngay
                    </router-link>
                  </div>
                  <div class="category-icon">
                    <i class="fas fa-male fa-3x"></i>
                  </div>
                </div>
              </div>

              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-3.jpg"
                >
                  <div class="categories__text">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">Thời trang trẻ em</h4>
                    <!-- <p>Phong cách năng động, dễ thương cho bé yêu</p> -->
                    <router-link style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                  <div class="category-icon"><i class="fas fa-child fa-3x"></i></div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0" style="height: 327px">
                <div
                  class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-4.jpg"
                >
                  <div class="categories__text" style="margin-top: 100px">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">Giày dép</h4>
                    <!-- <p>Bước đi phong cách, vững vàng mỗi ngày</p> -->
                    <router-link style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                  <div class="category-icon"><i class="fas fa-shoe-prints fa-3x"></i></div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg animated-category"
                  data-setbg="../../assets/img/categories/category-5.jpg"
                >
                  <div class="categories__text" style="margin-top: 100px">
                    <h4 style="font-size: 25px; font-family: Arial, Helvetica, sans-serif">Phụ kiện</h4>
                    <!-- <p>Hoàn thiện phong cách với hàng trăm phụ kiện hot</p> -->
                    <router-link  style="text-decoration: none; font-size: 15px; font-weight: bold" to="/Shop"
                      >Mua ngay</router-link
                    >
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
          <!-- Flash Sale Badge -->
          <div class="col-md-6">
            <div class="d-flex align-items-center">
              <h3 class="mb-0 fw-bold angel-text-gradient">
                <i class="fa fa-heart"></i>Được yêu thích nhiều nhất
              </h3>
            </div>
          </div>
          <!-- Navigation Buttons -->
          <div class="col-md-2 text-end">
            <button
              class="btn btn-outline-angel rounded-circle me-2"
              style="width: 45px; height: 45px"
              @click="slideLeft"
              :disabled="currentSlide === 0"
            >
              <i class="fas fa-chevron-left"></i>
            </button>
            <button
              class="btn btn-outline-angel rounded-circle"
              style="width: 45px; height: 45px"
              @click="slideRight"
              :disabled="currentSlide >= maxSlides"
            >
              <i class="fas fa-chevron-right"></i>
            </button>
          </div>
        </div>

        <!-- Products Slider Container -->
        <div class="product-slider-container position-relative">
          <div class="product-slider-wrapper overflow-hidden">
            <div
              class="product-slider d-flex transition-all"
              :style="{ transform: `translateX(-${currentSlide * slideWidth}px)` }"
            >
              <div
                class="product-slide flex-shrink-0 me-3"
                v-for="item in ListBestHotProducts"
                :key="item.maSp"
                style="width: 280px"
              >
                <div class="product__item" style="background-color: #ffffff; border-radius: 12px">
                  <div
                    class="product__item__pic position-relative animated-product"
                    style="height: 320px"
                  >
                    <img
                      :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                        item.productDetails[0].images[0].tenHinhAnh
                      }`"
                      :alt="item.tenSanPham"
                      class="w-100 h-100"
                      style="object-fit: cover; border-radius: 12px"
                    />

                    <!-- Discount Badge on Image -->
                    <div class="discount-badge position-absolute top-0 start-0 m-2">
                      <div
                        class="bg-danger text-white px-2 py-1 rounded-2 d-flex align-items-center shadow-sm"
                      >
                        <i class="fas fa-heart"></i>
                      </div>
                    </div>

                    <!-- Hover Icons -->
                    <ul class="product__hover">
                      <!-- <li>
                        <a href="#" class="image-popup">
                          <span class="arrow_expand"></span>
                        </a>
                      </li> -->
                      <li>
                        <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                          <span
                            :class="[favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt']"
                            style="color: #ec4e79; font-size: 20px; transition: 0.3s"
                          ></span>
                        </a>
                      </li>
                      <!-- <li>
                        <a href="#"><span class="icon_bag_alt"></span></a>
                      </li> -->
                    </ul>
                  </div>

                  <div class="product__item__text text-center pt-3">
                    <h6 class="mb-2">
                      <router-link
                        :to="`/product/${item.maSp}`"
                        style="
                          text-decoration-line: none;
                          color: #333;
                          font-size: 1.1rem;
                          font-weight: 600;
                        "
                      >
                        {{ item.tenSanPham }}
                      </router-link>
                    </h6>

                    <!-- Price Section -->
                    <div class="d-flex align-items-center justify-content-center gap-2 mb-2">
                      <!-- Current Price -->
                      <div class="product__price text-danger fw-bold fs-6">
                        {{ formatPrice(parsePrice(item.khoangGia)) }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Slider Indicators -->
        </div>
      </div>
    </section>
    <!-- Product Section End -->

    <!-- Banner Section Begin -->
    <section class="set-bg" style="position: relative; margin-bottom: 50px">
      <img
        src="../../assets/Customer/img/banner/banner-1.jpg"
        class="animated-banner"
        style="width: 100%; height: 400px"
      />
      <div class="banner-icon"><i class="fas fa-tags fa-3x"></i></div>
      <div
        class="container"
        style="
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          text-align: center;
          color: white;
        "
      >
        <div class="row">
          <div class="col-12">
            <div class="banner__slider owl-carousel">
              <div class="banner__item" style="margin-bottom: 300px">
                <div class="banner__text">
                  <span>Bộ Sưu Tập</span>
                  <div
                    class="col-xl-3 col-lg-2"
                    style="width: 300px; margin-left: 430px; padding-bottom: 20px"
                  >
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

                      <!-- Angel text with soft cursive font -->
                      <RouterLink to="/" style="text-decoration: none">
                        <text
                          x="50%"
                          y="60%"
                          dominant-baseline="middle"
                          text-anchor="middle"
                          class="angel-text"
                        >
                          Angel Fashion
                        </text>
                      </RouterLink>
                    </svg>
                  </div>
                  <a href="/Shop" style="color: black; text-decoration: none">Mua ngay</a>
                </div>
              </div>
              <!-- <div class="banner__item">
            <div class="banner__text">
              <span>The Chloe Collection</span>
              <h1>The Project Jacket</h1>
              <a href="#" style="color: white; text-decoration: none;">Mua ngay</a>
            </div>
          </div> -->
              <!-- <div class="banner__item" >
            <div class="banner__text">
              <span>The Chloe Collection</span>
              <h1>The Project Jacket</h1>
              <a href="#" style="color: white; text-decoration: none;">Mua ngay</a>
            </div>
          </div> -->
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
            <!-- Left Side - Advertisement Banner -->
            <div class="col-md-2">
              <div class="banner-container h-100">
                <img
                  src="/src/assets/images/Beige Minimalist Fashion Business Banner (1).png"
                  alt="Fashion Banner"
                  class="w-100 h-100 object-fit-cover"
                  style="min-height: 580px;"
                />
                <div class="banner-icon"><i class="fas fa-ad fa-3x"></i></div>
              </div>
            </div>

            <!-- Right Side - Product Grid (8 products: 4 top + 4 bottom) -->
            <div class="col-md-10">
              <!-- First Row - 4 products -->
              <div class="row g-3 mb-4">
                <div class="col-md-3" v-for="item in ListNewProducts.slice(0, 4)" :key="item.maSp">
                  <div
                    class="hot-product-item"
                    style="background-color: #ffffff; border-radius: 12px"
                  >
                    <!-- Product Image with Hover Effects -->
                    <div class="text-center position-relative mb-3">
                      <div
                        class="product__item__pic position-relative animated-product"
                        style="height: 300px; overflow: hidden; border-radius: 12px"
                      >
                        <img
                          :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                            item.productDetails[0].images[0].tenHinhAnh
                          }`"
                          :alt="item.tenSanPham"
                          class="img-fluid w-100 h-100"
                          style="object-fit: cover"
                        />
                        <div class="product-icon position-absolute top-0 start-0 m-2">
                          <i class="fas fa-clock text-danger fa-2x"></i>
                        </div>
                        <!-- Hover Icons -->
                        <ul class="product__hover">
                          <li>
                            <a href="#" class="image-popup">
                              <span class="arrow_expand"></span>
                            </a>
                          </li>
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span
                                :class="[
                                  favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt',
                                ]"
                                style="color: #ec4e79; font-size: 18px; transition: 0.3s"
                              ></span>
                            </a>
                          </li>
                          <li>
                            <a href="#"><span class="icon_bag_alt"></span></a>
                          </li>
                        </ul>
                      </div>
                    </div>

                    <!-- Product Info -->
                    <div class="product-info text-center">
                      <h6
                        class="product-title mb-2"
                        style="font-size: 1rem; line-height: 1.3; font-weight: 600"
                      >
                        <router-link
                          :to="`/product/${item.maSp}`"
                          style="text-decoration-line: none; color: #333"
                        >
                          {{ item.tenSanPham }}
                        </router-link>
                      </h6>

                      <!-- Price Section -->
                      <div class="d-flex align-items-center justify-content-center gap-3 mb-2">
                        <span class="angel-price fw-bold" style="font-size: 1.1rem; color: #ec4e79">
                          {{ formatPrice(parsePrice(item.khoangGia)) }}
                        </span>
                        <div
                          class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center animated-icon"
                          style="
                            width: 32px;
                            height: 32px;
                            cursor: pointer;
                            box-shadow: 0 2px 8px rgba(236, 78, 121, 0.3);
                          "
                          @click="toggleFavoriteProduct(item.maSp)"
                        >
                          <i class="fas fa-heart" style="font-size: 0.8rem; color: white"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Divider -->
              <div class="text-center mb-4">
                <hr style="border: none; border-top: 2px dashed #000" />
              </div>

              <!-- Second Row - 4 more products -->
              <div class="row g-3">
                <div class="col-md-3" v-for="item in ListNewProducts.slice(4, 8)" :key="item.maSp">
                  <div
                    class="hot-product-item"
                    style="background-color: #ffffff; border-radius: 12px"
                  >
                    <!-- Product Image with Hover Effects -->
                    <div class="text-center position-relative mb-3">
                      <div
                        class="product__item__pic position-relative animated-product"
                        style="height: 300px; overflow: hidden; border-radius: 12px"
                      >
                        <img
                          :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                            item.productDetails[0].images[0].tenHinhAnh
                          }`"
                          :alt="item.tenSanPham"
                          class="img-fluid w-100 h-100"
                          style="object-fit: cover"
                        />
                        <div class="product-icon position-absolute top-0 start-0 m-2">
                          <i class="fas fa-clock text-danger fa-2x"></i>
                        </div>

                        <!-- Hover Icons -->
                        <ul class="product__hover">
                          <li>
                            <a href="#" class="image-popup">
                              <span class="arrow_expand"></span>
                            </a>
                          </li>
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span
                                :class="[
                                  favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt',
                                ]"
                                style="color: #ec4e79; font-size: 18px; transition: 0.3s"
                              ></span>
                            </a>
                          </li>
                          <li>
                            <a href="#"><span class="icon_bag_alt"></span></a>
                          </li>
                        </ul>
                      </div>
                    </div>

                    <!-- Product Info -->
                    <div class="product-info text-center">
                      <h6
                        class="product-title mb-2"
                        style="font-size: 1rem; line-height: 1.3; font-weight: 600"
                      >
                        <router-link
                          :to="`/product/${item.maSp}`"
                          style="text-decoration-line: none; color: #333"
                        >
                          {{ item.tenSanPham }}
                        </router-link>
                      </h6>

                      <!-- Price Section -->
                      <div class="d-flex align-items-center justify-content-center gap-3 mb-2">
                        <span class="angel-price fw-bold" style="font-size: 1.1rem; color: #ec4e79">
                          {{ formatPrice(parsePrice(item.khoangGia)) }}
                        </span>
                        <div
                          class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center animated-icon"
                          style="
                            width: 32px;
                            height: 32px;
                            cursor: pointer;
                            box-shadow: 0 2px 8px rgba(236, 78, 121, 0.3);
                          "
                          @click="toggleFavoriteProduct(item.maSp)"
                        >
                          <i class="fas fa-heart" style="font-size: 0.8rem; color: white"></i>
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
              <!-- Left Banner -->
              <!-- <div class="col-md-6">
                <div
                  class="position-relative overflow-hidden rounded-4 angel-banner animated-banner"
                >
                  <img
                    src="/src/assets/images/banner ngang2.jpg"
                    alt="Angel Fashion Banner"
                    class="w-100"
                    style="height: 350px; object-fit: cover"
                  />
                  <div class="banner-icon"><i class="fas fa-female fa-3x"></i></div>
                </div>
              </div> -->

              <!-- Right Banner -->
              <div >
                <div
                  class="position-relative overflow-hidden rounded-4 angel-banner animated-banner"
                >
                  <img
                    src="/src/assets/images/banner ngang.jpg"
                    alt="Angel Fashion Trends"
                    class="w-100"
                    style="height: 350px; object-fit: cover"
                  />
                  <div class="banner-icon"><i class="fas fa-star fa-3x"></i></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Sản phẩm bán chạy Section -->
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
            <!-- Left Side - Advertisement Banner -->
            <div class="col-md-2">
              <div class="position-relative overflow-hidden rounded-3 h-100 animated-banner">
                <img
                  src="https://i.pinimg.com/1200x/17/12/e0/1712e06978a02432d83d400d6bded81a.jpg"
                  alt="Best Seller Banner"
                  class="w-100 h-100 object-fit-cover"
                  style="min-height: 580px"
                />
                <div class="banner-icon"><i class="fas fa-ad fa-3x"></i></div>
              </div>
            </div>

            <!-- Right Side - Product Grid (8 products: 4 top + 4 bottom) -->
            <div class="col-md-10">
              <!-- First Row - 4 products -->
              <div class="row g-3 mb-4">
                <div
                  class="col-md-3"
                  v-for="item in ListBestSellerProducts.slice(0, 4)"
                  :key="item.maSp"
                >
                  <div
                    class="hot-product-item"
                    style="background-color: #ffffff; border-radius: 12px"
                  >
                    <!-- Product Image with Hover Effects -->
                    <div class="text-center position-relative mb-3">
                      <div
                        class="product__item__pic position-relative animated-product"
                        style="height: 300px; overflow: hidden; border-radius: 12px"
                      >
                        <img
                          :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                            item.productDetails[0].images[0].tenHinhAnh
                          }`"
                          :alt="item.tenSanPham"
                          class="img-fluid w-100 h-100"
                          style="object-fit: cover"
                        />
                        <div class="product-icon position-absolute top-0 start-0 m-2">
                          <i class="fas fa-chart-line text-warning fa-2x"></i>
                        </div>

                        <!-- Hover Icons -->
                        <ul class="product__hover">
                          <li>
                            <a href="#" class="image-popup">
                              <span class="arrow_expand"></span>
                            </a>
                          </li>
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span
                                :class="[
                                  favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt',
                                ]"
                                style="color: #ec4e79; font-size: 18px; transition: 0.3s"
                              ></span>
                            </a>
                          </li>
                          <li>
                            <a href="#"><span class="icon_bag_alt"></span></a>
                          </li>
                        </ul>
                      </div>
                    </div>

                    <!-- Product Info -->
                    <div class="product-info text-center">
                      <h6
                        class="product-title mb-2"
                        style="font-size: 1rem; line-height: 1.3; font-weight: 600"
                      >
                        <router-link
                          :to="`/product/${item.maSp}`"
                          style="text-decoration-line: none; color: #333"
                        >
                          {{ item.tenSanPham }}
                        </router-link>
                      </h6>

                      <!-- Price Section -->
                      <div class="d-flex align-items-center justify-content-center gap-3 mb-2">
                        <span class="angel-price fw-bold" style="font-size: 1.1rem; color: #ec4e79">
                          {{ formatPrice(parsePrice(item.khoangGia)) }}
                        </span>
                        <div
                          class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center animated-icon"
                          style="
                            width: 32px;
                            height: 32px;
                            cursor: pointer;
                            box-shadow: 0 2px 8px rgba(236, 78, 121, 0.3);
                          "
                          @click="toggleFavoriteProduct(item.maSp)"
                        >
                          <i class="fas fa-heart" style="font-size: 0.8rem; color: white"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Divider -->
              <div class="text-center mb-4">
                <hr style="border: none; border-top: 2px dashed #000" />
              </div>

              <!-- Second Row - 4 more products -->
              <div class="row g-3">
                <div
                  class="col-md-3"
                  v-for="item in ListBestSellerProducts.slice(4, 8)"
                  :key="item.maSp"
                >
                  <div
                    class="hot-product-item"
                    style="background-color: #ffffff; border-radius: 12px"
                  >
                    <!-- Product Image with Hover Effects -->
                    <div class="text-center position-relative mb-3">
                      <div
                        class="product__item__pic position-relative animated-product"
                        style="height: 300px; overflow: hidden; border-radius: 12px"
                      >
                        <img
                          :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                            item.productDetails[0].images[0].tenHinhAnh
                          }`"
                          :alt="item.tenSanPham"
                          class="img-fluid w-100 h-100"
                          style="object-fit: cover"
                        />
                        <div class="product-icon position-absolute top-0 left-0 m-2">
                          <i class="fas fa-chart-line text-warning fa-2x"></i>
                        </div>

                        <!-- Hover Icons -->
                        <ul class="product__hover">
                          <li>
                            <a href="#" class="image-popup">
                              <span class="arrow_expand"></span>
                            </a>
                          </li>
                          <li>
                            <a href="#" @click.prevent="toggleFavoriteProduct(item.maSp)">
                              <span
                                :class="[
                                  favoriteStatus[item.maSp] ? 'icon_heart' : 'icon_heart_alt',
                                ]"
                                style="color: #ec4e79; font-size: 18px; transition: 0.3s"
                              ></span>
                            </a>
                          </li>
                          <li>
                            <a href="#"><span class="icon_bag_alt"></span></a>
                          </li>
                        </ul>
                      </div>
                    </div>

                    <!-- Product Info -->
                    <div class="product-info text-center">
                      <h6
                        class="product-title mb-2"
                        style="font-size: 1rem; line-height: 1.3; font-weight: 600"
                      >
                        <router-link
                          :to="`/product/${item.maSp}`"
                          style="text-decoration-line: none; color: #333"
                        >
                          {{ item.tenSanPham }}
                        </router-link>
                      </h6>

                      <!-- Price Section -->
                      <div class="d-flex align-items-center justify-content-center gap-3 mb-2">
                        <span class="angel-price fw-bold" style="font-size: 1.1rem; color: #ec4e79">
                          {{ formatPrice(parsePrice(item.khoangGia)) }}
                        </span>
                        <div
                          class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center animated-icon"
                          style="
                            width: 32px;
                            height: 32px;
                            cursor: pointer;
                            box-shadow: 0 2px 8px rgba(236, 78, 121, 0.3);
                          "
                          @click="toggleFavoriteProduct(item.maSp)"
                        >
                          <i class="fas fa-heart" style="font-size: 0.8rem; color: white"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Sản phẩm nổi bật Section -->
        <!-- <div class="mb-5">
          <div class="row align-items-center mb-4">
            <div class="col">
              <h3 class="fw-bold mb-0 angel-text-gradient">Sản phẩm nổi bật</h3>
            </div>
            <div class="col-auto">
              <button class="btn btn-outline-angel rounded-circle me-2" style="width: 40px; height: 40px;">
                <i class="fas fa-chevron-left"></i>
              </button>
              <button class="btn btn-outline-angel rounded-circle" style="width: 40px; height: 40px;">
                <i class="fas fa-chevron-right"></i>
              </button>
            </div>
          </div>

          <div class="row">
          
            <div class="col-md-2">
              <div class="position-relative overflow-hidden rounded-3 h-100">
                <img
                  src="https://inkythuatso.com/uploads/thumbnails/800/2023/03/1-hinh-anh-ngay-moi-hanh-phuc-sieu-cute-inkythuatso-09-13-35-50.jpg"
                  alt="Featured Banner" class="w-100 h-100 object-fit-cover" style="min-height: 520px;">
              </div>
            </div>

       
            <div class="col-md-10">
       
              <div class="row g-3 mb-3">
                <div class="col-md-3">
                  <div class="card border-0 shadow-sm h-100 angel-card-small">
                    <div class="d-flex justify-content-between p-2">
                      <div class="bg-danger text-white rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 22px; height: 22px;">
                        <i class="fas fa-star" style="font-size: 0.7rem;"></i>
                      </div>
                      <div class="bg-gradient-angel text-white rounded d-flex align-items-center px-2"
                        style="font-size: 0.7rem;">
                        <i class="fas fa-check me-1"></i>Nổi bật
                      </div>
                      <div
                        class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 22px; height: 22px;">
                        <i class="fas fa-crown" style="font-size: 0.7rem;"></i>
                      </div>
                    </div>
                    <div class="text-center p-3">
                      <img
                        src="https://inkythuatso.com/uploads/thumbnails/800/2023/03/1-hinh-anh-ngay-moi-hanh-phuc-sieu-cute-inkythuatso-09-13-35-50.jpg"
                        alt="Bow wrap skirt" class="img-fluid" style="max-width: 120px; height: auto;">
                    </div>
                    <div class="card-body pt-0">
                      <h6 class="card-title mb-2" style="font-size: 0.9rem;">Bow wrap skirt</h6>
                      <div class="d-flex align-items-center gap-2 mb-2">
                        <span class="angel-price fw-bold" style="font-size: 1rem;">1.400.000vnđ</span>
                        <small class="text-muted text-decoration-line-through"
                          style="font-size: 0.8rem;">1.500.000vnđ</small>
                      </div>
                      <div class="d-flex align-items-center justify-content-between">
                        <small class="angel-discount" style="font-size: 0.8rem;">-7%</small>
                        <div
                          class="bg-gradient-angel text-white rounded-circle d-flex align-items-center justify-content-center angel-add-btn"
                          style="width: 25px; height: 25px; cursor: pointer;">
                          <i class="fas fa-plus" style="font-size: 0.7rem;"></i>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
    
              </div>


              <div class="row g-3">
     
              </div>
            </div>
          </div>
        </div> -->
      </div>
    </section>
    <!-- Trend Section End -->
  </div>
  <!-- <CompareProduct /> -->
</template>

<style>
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

/* --- SỬA ĐỂ CÂN ĐỐI CÁC MỤC ĐANG HOT, BÁN CHẠY, NỔI BẬT --- */
.trend__content {
  min-height: 520px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  /* background: #fff; */
  /* border-radius: 12px; */
  /* box-shadow: 0 2px 16px rgba(0, 0, 0, 0.04); */
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

/* Angel Theme Colors and Styles */
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

/* New Styles for Icons and Animations */
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
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.1);
  }
  100% {
    transform: scale(1);
  }
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