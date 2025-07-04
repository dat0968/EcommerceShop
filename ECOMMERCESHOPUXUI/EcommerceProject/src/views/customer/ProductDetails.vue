<script setup>
import ReviewProductCombo from '@/components/reviews/ReviewProductCombo.vue'
import $ from 'jquery'

import { ref, onMounted, computed, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { jwtDecode } from 'jwt-decode'

const route = useRoute()
const getUrlAPI = ref(GetApiUrl())
const id = route.params.id
const product = ref({})
const allImages = ref([])
const currentSlider = ref(1)
const colors = ref([])
const selectedColor = ref('')
const selectedSize = ref('')
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const router = useRouter()
const quantity = ref('1')
const activeTab = ref('desc')
const recommendationProduct = ref([])
const isLoading = ref(true)

function ReadToken(token) {
  if (token) {
    const decoded = jwtDecode(token);
    return {
      IdUser: decoded.sub,
      Phone: decoded.PhoneNumber,
      Name: decoded.FullName,
      Role: decoded.role,
      Exp: decoded.exp
    };
  }
  return null;
}

const token = Cookies.get('accessToken');
const decodedToken = ReadToken(token);
const idKhachHang = decodedToken ? decodedToken.IdUser : null;
const isFavorited = ref(false)

const isLogin = computed(() => {
  return accessToken.value && accessToken.value !== ''
})

// Check if description content is short
const isShortDescription = computed(() => {
  if (!product.value.moTa) return true
  const textContent = product.value.moTa.replace(/<[^>]*>/g, '').trim()
  return textContent.length < 300 // If less than 300 characters, consider it short
})

const addFavoriteProduct = async () => {
  try {
    const response = await fetch('https://localhost:7217/api/Favorite/AddFavoriteProduct', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        maSp: id,
        maKh: idKhachHang
      })
    })

    const data = await response.json()

    if (data.message === 'Sản phẩm yêu thích đã tồn tại') {
      Swal.fire({
        title: 'Sản phẩm đã nằm trong danh sách yêu thích.',
        icon: 'info',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
    } else {
      Swal.fire({
        title: 'Đã thêm vào yêu thích!',
        icon: 'success',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
    }

    isFavorited.value = true
  } catch (error) {
    Swal.fire({
      title: 'Lỗi khi thêm vào yêu thích!',
      text: error.message,
      icon: 'error',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
  }
}

// Call Api ProductDetails
const fetchAPI = async () => {
  try {
    const response = await fetch(`${getUrlAPI.value}/api/Shop/Product/${id}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) {
      throw new Error('Failed to FetchAPI')
    }
    const result = await response.json()
    product.value = result
    product.value.productDetails.forEach((element) => {
      element.images.forEach((image) => {
        allImages.value.push(image)
      })
    })

    // Process description with formatting
    if (product.value.moTa) {
      product.value.moTa = product.value.moTa
        .replace(/\*\*([^*]+)\*\*/g, '<br><strong>$1</strong><br>')
        .replace(/\n/g, '<br>')
    }
    
    colors.value = [
      ...new Set(
        product.value.productDetails?.map((d) => d?.mauSac || '').filter((color) => color !== '')
      ),
    ]

    selectedColor.value = colors.value[0]
  } catch (error) {
    console.error('Error fetching product:', error)
  }
}

const fetchRcmProduct = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid) {
      accessToken.value = validatetoken.newAccessToken
      const readToken = decodeToken(accessToken.value)
      const response = await fetch(
        `${getUrlAPI.value}/api/Home/RecommendationProduct?UserId=${readToken.IdUser}&maSp=${id}&numberOfRecommendations=8`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      )

      if (!response.ok) {
        throw new Error('Error to fetchRecommendationProducts')
      }
      const result = await response.json()
      recommendationProduct.value = result
    }
  } catch (error) {
    console.error('Error fetching recommendations:', error)
  } finally {
    isLoading.value = false
  }
}

const sizes = computed(() => {
  if (!product.value || !product.value.productDetails) return []

  const filtered = product.value.productDetails
    .filter((p) => p.mauSac && p.mauSac.toLowerCase() === selectedColor.value.toLowerCase())
    .map((p) => p.kichThuoc)
  return [...new Set(filtered)]
})

watch(sizes, (newSizes) => {
  if (newSizes.length > 0) {
    selectedSize.value = newSizes[0]
  }
})

const originalPrice = computed(() => {
  if (!product.value || !product.value.productDetails) return 0
  var match = product.value.productDetails.find(
    (p) =>
      (p?.mauSac || '').toLowerCase() === (selectedColor.value || '').toLowerCase() &&
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase()
  )
  return match ? match.donGia : 0
})

const maxQuantity = computed(() => {
  if (!product.value || !product.value.productDetails) return 0
  var match = product.value.productDetails.find(
    (p) =>
      (p?.mauSac || '').toLowerCase() === (selectedColor.value || '').toLowerCase() &&
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase()
  )
  quantity.value = '1'
  return match ? match.soLuongTon : 'Hết hàng'
})

const chunkSize = 4
const slideChunks = computed(() => {
  const chunks = []
  for (let i = 0; i < allImages.value.length; i += chunkSize) {
    chunks.push(allImages.value.slice(i, i + chunkSize))
  }
  return chunks
})

const maxSlide = computed(() => slideChunks.value.length || 1)
const prevImage = () => {
  currentSlider.value = currentSlider.value === 1 ? maxSlide.value : currentSlider.value - 1
}

const nextImage = () => {
  currentSlider.value = currentSlider.value === maxSlide.value ? 1 : currentSlider.value + 1
}

const selectColor = (color) => {
  selectedColor.value = color
}

const selectSize = (size) => {
  selectedSize.value = size
}
const currentImage = ref(1)
const showMainImage = computed(() => {
  if (!product.value || !product.value.productDetails) return 0
  var match = product.value.productDetails.find(
    (p) =>
      (p?.mauSac || '').toLowerCase() === (selectedColor.value || '').toLowerCase() &&
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase()
  )
  var maCtsp = match.maCtsp
  return allImages.value.findIndex((p) => p.maCtsp == maCtsp) + 1
})

watch(showMainImage, (newIndex) => {
  currentImage.value = newIndex
})

onMounted(async () => {
  isLoading.value = true
  await Promise.all([fetchAPI(), fetchRcmProduct()])
  
  // Scroll to top
  window.scrollTo({
    top: 0,
    behavior: 'smooth',
  })

  // Initialize carousel
  nextTick(() => {
    const $carousel = $('.product__details__pic__slider')

    if ($carousel.length === 0) {
      console.warn('Không tìm thấy .product__details__pic__slider trong DOM')
      return
    }

    if (typeof $carousel.owlCarousel !== 'function') {
      console.error('owlCarousel is not a function. OwlCarousel chưa được attach vào jQuery')
      return
    }

    const owl = $carousel.owlCarousel({
      items: 1,
      loop: true,
      autoplay: false,
      nav: false,
      dots: true,
      animateOut: 'fadeOut',
      animateIn: 'fadeIn',
    })

    $('.pt').on('click', function () {
      const index = $(this).index()
      owl.trigger('to.owl.carousel', [index, 300])
      currentImage.value = index + 1
    })

    owl.on('changed.owl.carousel', function (event) {
      currentImage.value = event.item.index + 1 - event.item.count
      if (currentImage.value < 1) currentImage.value += event.item.count
    })
  })
})

const changeImage = (index) => {
  currentImage.value = index
  $('.product__details__pic__slider').trigger('to.owl.carousel', [index - 1, 300])
}

const validateQuantity = () => {
  const value = quantity.value.trim()
  if (value === '') return
  const number = parseInt(quantity.value)
  if (isNaN(number) || number < 1) {
    quantity.value = '1'
  } else if (number > maxQuantity.value) {
    quantity.value = maxQuantity.value.toString()
  } else {
    quantity.value = number.toString()
  }
}

const addToCart = async () => {
  try {
    const value = quantity.value.trim()
    if (value === '') {
      Swal.fire({
        title: 'Không để trống số lượng',
        icon: 'error',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
      return
    }
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (!validatetoken.isValid) {
      router.push('/Login')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      const readToken = decodeToken(accessToken.value)
      const matched = product.value.productDetails.find(
        (p) =>
          p.mauSac?.toLowerCase() === selectedColor.value?.toLowerCase() &&
          p.kichThuoc?.toLowerCase() === selectedSize.value?.toLowerCase()
      )

      const content = {
        maKh: readToken.IdUser,
        maCtsp: matched.maCtsp,
        maCombo: null,
        soLuong: quantity.value,
        donGia: matched.donGia,
        giamGia: 0,
        tenHinhAnh: allImages.value[currentImage.value - 1]?.tenHinhAnh || '',
        giohangctcombos: [],
      }
      const response = await fetch(`${getUrlAPI.value}/api/Cart`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(content),
      })
      const result = await response.json()
      if (!response.ok || !result.success) {
        Swal.fire({
          title: result.error || 'Đã xảy ra lỗi',
          icon: 'error',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        return
      }

      if (result.success) {
        Swal.fire({
          title: 'Đã thêm sản phẩm vào giỏ hàng.',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
      }
    }
  } catch (error) {
    Swal.fire({
      title: `${error.message}`,
      icon: 'error',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
  }
}

// Format price function
const formatPrice = (price) => {
  if (typeof price === 'string') {
    return price
  }
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(price)
}

// Add to favorites function for recommendation products
const addToFavorites = (productId) => {
  console.log('Add to favorites:', productId)
  // Add your logic here
}

// Add to cart function for recommendation products
const addToCartRecommendation = (productId) => {
  console.log('Add to cart:', productId)
  // Add your logic here
}

watch(
  () => route.params.id,
  async () => {
    allImages.value = []
    selectedColor.value = ''
    selectedSize.value = ''
    quantity.value = 1
    currentSlider.value = 1
    currentImage.value = 1
    isLoading.value = true
    await Promise.all([fetchAPI(), fetchRcmProduct()])
  }
)
</script>

<template>
  <div class="product-page-container">
    <!-- Product Details Section Begin -->
    <section class="product-details spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-6">
            <div class="product__details__pic">
              <div style="position: relative; margin-bottom: 20px" class="product__details__slider__content">
                <div class="product__details__pic__slider owl-carousel">
                  <div v-for="(image, index) in allImages" :key="index">
                    <img v-if="index + 1 == currentImage" :data-hash="`product-${index}`" class="product__big__img"
                      :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${image.tenHinhAnh}`" alt="" />
                  </div>
                </div>
              </div>
              <!-- Thumbnail ảnh nhỏ nằm dưới ảnh lớn -->
              <div class="product__details__thumbnails d-flex justify-content-center col-lg-6"
                style="max-width: 100%; display: flex; justify-content: center; margin: 20px">
                <div class="carousel slide w-100">
                  <div class="carousel-inner">
                    <div v-for="(imageGroup, index) in slideChunks" :key="index"
                      :class="['carousel-item', { active: currentSlider === index + 1 }]">
                      <div class="d-flex gap-2 justify-content-center" style="width: 100%">
                        <img v-for="(image, imageindex) in imageGroup" :key="imageindex" :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${image.tenHinhAnh
                          }`" class="img-fluid" :style="{ width: `${100 / imageGroup.length}%`, height: '100px' }"
                          alt="" @click.prevent="changeImage(index * chunkSize + imageindex + 1)" />
                      </div>
                    </div>
                  </div>

                  <button @click="prevImage" class="carousel-control-prev" type="button" style="
                      width: 40px;
                      height: 40px;
                      top: 50%;
                      transform: translateY(-50%);
                      background-color: gray;
                    ">
                    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                  </button>
                  <button @click="nextImage" class="carousel-control-next" type="button" style="
                      width: 40px;
                      height: 40px;
                      top: 50%;
                      transform: translateY(-50%);
                      background-color: gray;
                    ">
                    <span class="carousel-control-next-icon" aria-hidden="true"></span>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="product__details__text">
              <h3>
                {{ product.tenSanPham }} <span>Còn: {{ maxQuantity }} sản phẩm</span>
              </h3>
              <div class="product__details__price">{{ originalPrice }}</div>
              <div class="product__details__button">
                <div class="quantity">
                  <span>Số lượng:</span>
                  <div class="pro-qty">
                    <input v-model="quantity" @input="validateQuantity" type="text" value="1" />
                  </div>
                </div>
                <button @click="addToCart" class="cart-btn">
                  <span class="icon_bag_alt"></span> Thêm giỏ hàng
                </button>
                <ul>
                  <li>
                    <a href="#" @click.prevent="addFavoriteProduct">
                      <span
                        :class="[isFavorited ? 'icon_heart' : 'icon_heart_alt']"
                        style="color: red; font-size: 20px; transition: 0.3s"
                      ></span>
                    </a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_adjust-horiz"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__details__widget">
                <ul>
                  <li style="display: flex; align-items: center" v-if="colors.length > 0">
                    <span style="min-width: 120px">Màu:</span>
                    <div class="color__checkbox" style="display: flex; gap: 8px">
                      <button v-for="(color, index) in colors" :key="index"
                        :class="['btn', 'btn-light', { active: selectedColor === color }]" @click="selectColor(color)"
                        style="background-color: #e0e0e0; border: 1px solid #ccc; font-weight: 500">
                        {{ color }}
                      </button>
                    </div>
                  </li>
                  <li style="display: flex; align-items: center" v-if="sizes.length > 0">
                    <span style="min-width: 120px">Kích thước:</span>
                    <div class="size__checkbox" style="display: flex; gap: 8px">
                      <button v-for="(size, index) in sizes" :key="index"
                        :class="['btn', 'btn-light', { active: selectedSize === size }]" @click="selectSize(size)"
                        style="background-color: #e0e0e0; border: 1px solid #ccc; font-weight: 500">
                        {{ size }}
                      </button>
                    </div>
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>

        <!-- Optimized Tab Section with Dynamic Spacing -->
        <div class="row" :class="{ 'compact-spacing': isShortDescription }">
          <div class="col-lg-12">
            <div class="product-tabs-container">
              <!-- Tab Navigation -->
              <ul class="nav nav-tabs custom-tabs" role="tablist">
                <li class="nav-item">
                  <a
                    class="nav-link custom-tab-link"
                    :class="{ active: activeTab === 'desc' }"
                    href="#"
                    @click.prevent="activeTab = 'desc'"
                  >
                    <span class="tab-icon">📝</span>
                    Mô tả
                  </a>
                </li>
                <li class="nav-item">
                  <a
                    class="nav-link custom-tab-link"
                    :class="{ active: activeTab === 'review' }"
                    href="#"
                    @click.prevent="activeTab = 'review'"
                  >
                    <span class="tab-icon">⭐</span>
                    Đánh giá
                  </a>
                </li>
              </ul>

              <!-- Tab Content -->
              <div class="tab-content custom-tab-content">
                <div
                  v-show="activeTab == 'desc'"
                  class="tab-pane custom-tab-pane"
                  :class="[
                    activeTab == 'desc' ? 'active' : '',
                    { 'short-content': isShortDescription }
                  ]"
                  id="tabs-1"
                  role="tabpanel"
                >
                  <div class="description-content">
                    <p v-html="product.moTa" class="description-text"></p>
                    <div v-if="isShortDescription" class="content-spacer"></div>
                  </div>
                </div>
                <div
                  v-show="activeTab == 'review'"
                  class="tab-pane custom-tab-pane"
                  :class="[activeTab == 'review' ? 'active' : '']"
                  id="tabs-2"
                  role="tabpanel"
                >
                  <div class="review-content">
                    <ReviewProductCombo :objectId="id" :isProduct="true" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recommendation Section with Smart Spacing -->
        <div v-if="isLogin" class="recommendation-section" :class="{ 'close-spacing': isShortDescription }">
          <!-- Section Header -->
          <div class="section-header">
            <div class="header-content">
              <h2 class="section-title">
                <span class="title-icon">✨</span>
                Gợi ý cho bạn
              </h2>
              <p class="section-subtitle">Những sản phẩm được chọn riêng cho bạn</p>
            </div>
            <div class="header-decoration">
              <div class="decoration-line"></div>
            </div>
          </div>

          <!-- Loading State -->
          <div v-if="isLoading" class="loading-container">
            <div class="loading-grid">
              <div v-for="n in 8" :key="n" class="loading-card">
                <div class="loading-image"></div>
                <div class="loading-content">
                  <div class="loading-line"></div>
                  <div class="loading-line short"></div>
                  <div class="loading-line price"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Products Grid -->
          <div v-else-if="recommendationProduct.length > 0" class="products-container">
            <div class="products-grid">
              <div 
                v-for="(item, index) in recommendationProduct" 
                :key="item.maSp" 
                class="product-card"
                :style="{ 'animation-delay': `${index * 0.1}s` }"
              >
                <!-- Product Image -->
                <div class="product-image-container">
                  <div class="product-image">
                    <img
                      :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                        item.productDetails[0].images[0].tenHinhAnh
                      }`"
                      :alt="item.tenSanPham"
                      class="product-img"
                    />
                    <div class="image-overlay">
                      <div class="overlay-content">
                        <button 
                          class="action-btn favorite-btn"
                          @click="addToFavorites(item.maSp)"
                          title="Thêm vào yêu thích"
                        >
                          <svg viewBox="0 0 24 24" width="18" height="18">
                            <path fill="currentColor" d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                          </svg>
                        </button>
                        <button 
                          class="action-btn cart-btn"
                          @click="addToCartRecommendation(item.maSp)"
                          title="Thêm vào giỏ hàng"
                        >
                          <svg viewBox="0 0 24 24" width="18" height="18">
                            <path fill="currentColor" d="M19 7h-3V6a4 4 0 0 0-8 0v1H5a1 1 0 0 0-1 1v11a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3V8a1 1 0 0 0-1-1zM10 6a2 2 0 0 1 4 0v1h-4V6zm8 15a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V9h2v1a1 1 0 0 0 2 0V9h4v1a1 1 0 0 0 2 0V9h2v12z"/>
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Product Info -->
                <div class="product-info">
                  <h3 class="product-title">
                    <router-link :to="`/product/${item.maSp}`" class="product-link">
                      {{ item.tenSanPham }}
                    </router-link>
                  </h3>
                  
                  <!-- Rating -->
                  <div class="product-rating">
                    <div class="stars">
                      <span v-for="n in 5" :key="n" class="star" :class="{ 'filled': n <= 4 }">
                        <svg viewBox="0 0 24 24" width="14" height="14">
                          <path fill="currentColor" d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/>
                        </svg>
                      </span>
                    </div>
                    <span class="rating-count">(4.0)</span>
                  </div>

                  <!-- Price -->
                  <div class="product-price">
                    <span class="current-price">{{ formatPrice(item.khoangGia) }}</span>
                  </div>

                  <!-- Quick Actions -->
                  <div class="quick-actions">
                    <router-link :to="`/product/${item.maSp}`" class="view-btn">
                      Xem chi tiết
                    </router-link>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="empty-state">
            <div class="empty-icon">🛍️</div>
            <h3>Không có gợi ý nào</h3>
            <p>Hãy thử xem các sản phẩm khác để nhận được gợi ý phù hợp</p>
          </div>
        </div>
      </div>
    </section>
    <!-- Product Details Section End -->
  </div>
</template>

<style scoped>
.product-page-container {
  background: #f8fafc;
}

/* Responsive spacing based on content length */
.compact-spacing {
  margin-top: 30px !important;
}

.compact-spacing .product-tabs-container {
  margin-bottom: 20px;
}

.close-spacing {
  margin-top: -40px !important;
  padding-top: 40px !important;
}

/* Enhanced Tab Styling */
.product-tabs-container {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  margin-bottom: 40px;
  border: 1px solid #e2e8f0;
}

.custom-tabs {
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  border-bottom: 2px solid #e2e8f0;
  padding: 0 20px;
  margin: 0;
}

.custom-tab-link {
  color: #64748b !important;
  font-weight: 600;
  padding: 16px 24px;
  border: none !important;
  background: none !important;
  transition: all 0.3s ease;
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
  text-decoration: none;
}

.custom-tab-link:hover {
  color: #667eea !important;
  background: rgba(102, 126, 234, 0.1) !important;
}

.custom-tab-link.active {
  color: #667eea !important;
  background: white !important;
  border-radius: 8px 8px 0 0 !important;
  box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.1);
}

.custom-tab-link.active::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  right: 0;
  height: 2px;
  background: #667eea;
}

.tab-icon {
  font-size: 16px;
}

.custom-tab-content {
  padding: 0;
  border: none;
}

.custom-tab-pane {
  min-height: 200px;
  padding: 30px;
  background: white;
}

.custom-tab-pane.short-content {
  min-height: 150px;
  padding: 20px 30px;
}

.description-content {
  line-height: 1.8;
  color: #4a5568;
}

.description-text {
  font-size: 16px;
  margin-bottom: 0;
}

.description-text strong {
  color: #2d3748;
  font-weight: 600;
}

.content-spacer {
  height: 20px;
}

.review-content {
  background: #fff;
}

/* Recommendation Section Styling */
.recommendation-section {
  margin-top: 60px;
  padding: 50px 0;
  background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
  border-radius: 24px 24px 0 0;
  position: relative;
  overflow: hidden;
}

.recommendation-section.close-spacing {
  margin-top: 20px !important;
  padding-top: 30px !important;
}

.recommendation-section::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: linear-gradient(90deg, transparent, #cbd5e0, transparent);
}

/* Header Styles */
.section-header {
  text-align: center;
  margin-bottom: 40px;
  position: relative;
}

.header-content {
  position: relative;
  z-index: 2;
}

.section-title {
  font-size: 2.25rem;
  font-weight: 700;
  color: #1a202c;
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  animation: slideInDown 0.8s ease-out;
}

.title-icon {
  font-size: 1.75rem;
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% {
    transform: translateY(0);
  }
  40% {
    transform: translateY(-8px);
  }
  60% {
    transform: translateY(-4px);
  }
}

.section-subtitle {
  font-size: 1rem;
  color: #64748b;
  margin: 0;
  animation: slideInUp 0.8s ease-out 0.2s both;
}

.header-decoration {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 200px;
  height: 200px;
  z-index: 1;
}

.decoration-line {
  width: 100%;
  height: 2px;
  background: linear-gradient(90deg, transparent, #667eea, transparent);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 0.5;
  }
  50% {
    opacity: 1;
  }
}

/* Loading Styles */
.loading-container {
  padding: 0 20px;
}

.loading-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.loading-card {
  background: white;
  border-radius: 16px;
  padding: 16px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.loading-image {
  width: 100%;
  height: 200px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  border-radius: 12px;
  animation: shimmer 1.5s infinite;
}

.loading-content {
  margin-top: 16px;
}

.loading-line {
  height: 12px;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  border-radius: 6px;
  margin-bottom: 12px;
  animation: shimmer 1.5s infinite;
}

.loading-line.short {
  width: 60%;
}

.loading-line.price {
  width: 40%;
  height: 16px;
}

@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

/* Products Grid */
.products-container {
  padding: 0 20px;
  opacity: 1;
  transform: translateY(0);
  transition: all 0.8s ease-out;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

/* Product Card */
.product-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transition: all 0.3s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  opacity: 1;
  transform: translateY(0);
  animation: slideInUp 0.6s ease-out forwards;
  border: 1px solid #e2e8f0;
}

@keyframes slideInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideInDown {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.product-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
  border-color: #667eea;
}

/* Product Image */
.product-image-container {
  position: relative;
  overflow: hidden;
}

.product-image {
  position: relative;
  height: 220px;
  overflow: hidden;
}

.product-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}

.product-card:hover .product-img {
  transform: scale(1.05);
}

.image-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  opacity: 0;
  transition: opacity 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.product-card:hover .image-overlay {
  opacity: 1;
}

.overlay-content {
  display: flex;
  gap: 12px;
  transform: translateY(20px);
  transition: transform 0.3s ease 0.1s;
}

.product-card:hover .overlay-content {
  transform: translateY(0);
}

.action-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: white;
  color: #4a5568;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.action-btn:hover {
  transform: scale(1.1);
  color: #667eea;
}

.favorite-btn:hover {
  color: #e53e3e;
}

.cart-btn:hover {
  color: #38a169;
}

/* Product Info */
.product-info {
  padding: 18px;
}

.product-title {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 8px;
  line-height: 1.4;
  height: 2.8em;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.product-link {
  color: #2d3748;
  text-decoration: none;
  transition: color 0.3s ease;
}

.product-link:hover {
  color: #667eea;
}

/* Rating */
.product-rating {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 10px;
}

.stars {
  display: flex;
  gap: 2px;
}

.star {
  color: #e2e8f0;
  transition: color 0.2s ease;
}

.star.filled {
  color: #fbbf24;
}

.rating-count {
  font-size: 0.8rem;
  color: #64748b;
  font-weight: 500;
}

/* Price */
.product-price {
  margin-bottom: 14px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.current-price {
  font-size: 1.125rem;
  font-weight: 700;
  color: #e53e3e;
}

/* Quick Actions */
.quick-actions {
  display: flex;
  gap: 8px;
}

.view-btn {
  flex: 1;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  text-decoration: none;
  padding: 10px 16px;
  border-radius: 8px;
  text-align: center;
  font-weight: 500;
  font-size: 0.875rem;
  transition: all 0.3s ease;
  border: none;
  cursor: pointer;
}

.view-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
  color: white;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 50px 20px;
  color: #64748b;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 16px;
}

.empty-state h3 {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 8px;
  color: #2d3748;
}

.empty-state p {
  font-size: 1rem;
  max-width: 400px;
  margin: 0 auto;
  line-height: 1.6;
}

/* Original carousel styles */
.carousel-item img {
  object-fit: cover;
  max-height: 150px;
}

.product__details__pic__slider {
  position: relative;
}

.slider-navigation {
  position: absolute;
  top: 50%;
  width: 100%;
  display: flex;
  justify-content: space-between;
  transform: translateY(-50%);
  z-index: 10;
}

.slider-prev,
.slider-next {
  color: white;
  background-color: rgba(0, 0, 0, 0.6);
  padding: 12px 15px;
  border: none;
  cursor: pointer;
  font-size: 18px;
  transition: background-color 0.3s ease;
}

.slider-prev:hover,
.slider-next:hover {
  background-color: rgba(0, 0, 0, 0.8);
}

.slider-prev {
  margin-left: 10px;
}

.slider-next {
  margin-right: 10px;
}

.pt {
  display: block;
  margin-bottom: 10px;
  opacity: 0.6;
  transition: opacity 0.3s ease;
}

.pt.active {
  opacity: 1;
  border: 2px solid #e7ab3c;
}

.pt img {
  width: 100%;
  height: 100%;
}

.product__big__img {
  width: 100%;
  height: 500px;
}

.product__details__pic__left .pt img {
  width: 100px;
  height: 100px;
  object-fit: cover;
  border-radius: 5px;
  margin-bottom: 10px;
}

.btn.active {
  background-color: #4a90e2 !important;
  color: white !important;
  border-color: #357ab8 !important;
}

/* Responsive Design */
@media (max-width: 768px) {
  .recommendation-section {
    margin-top: 30px;
    padding: 30px 0;
    border-radius: 16px 16px 0 0;
  }
  
  .recommendation-section.close-spacing {
    margin-top: 15px !important;
    padding-top: 20px !important;
  }
  
  .section-title {
    font-size: 1.75rem;
  }
  
  .products-grid {
    grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
    gap: 16px;
  }
  
  .product-image {
    height: 180px;
  }
  
  .product-info {
    padding: 14px;
  }
  
  .product-title {
    font-size: 0.9rem;
  }
  
  .current-price {
    font-size: 1rem;
  }

  .custom-tab-pane {
    padding: 20px 16px;
  }

  .custom-tab-pane.short-content {
    padding: 16px;
  }

  .custom-tabs {
    padding: 0 10px;
  }

  .custom-tab-link {
    padding: 12px 16px;
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .products-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }
  
  .section-title {
    font-size: 1.5rem;
    flex-direction: column;
    gap: 8px;
  }
  
  .title-icon {
    font-size: 1.25rem;
  }
  
  .product-image {
    height: 160px;
  }
  
  .overlay-content {
    gap: 8px;
  }
  
  .action-btn {
    width: 36px;
    height: 36px;
  }

  .custom-tab-link {
    padding: 10px 12px;
    font-size: 13px;
  }

  .tab-icon {
    font-size: 14px;
  }
}

/* Animation delays for staggered effect */
.product-card:nth-child(1) { animation-delay: 0.1s; }
.product-card:nth-child(2) { animation-delay: 0.2s; }
.product-card:nth-child(3) { animation-delay: 0.3s; }
.product-card:nth-child(4) { animation-delay: 0.4s; }
.product-card:nth-child(5) { animation-delay: 0.5s; }
.product-card:nth-child(6) { animation-delay: 0.6s; }
.product-card:nth-child(7) { animation-delay: 0.7s; }
.product-card:nth-child(8) { animation-delay: 0.8s; }
</style>