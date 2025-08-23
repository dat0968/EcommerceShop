<script setup>
import CompareStorageHelper from '@/models/dtos/expansionModels/compareObject'
import ReviewProductCombo from '@/components/pages/customers/reviews/ReviewProductCombo.vue'
import $ from 'jquery'
import RecomendationProduct from '@/components/RecommendationProduct/RecomendationProduct.vue'
import { ref, onMounted, computed, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { jwtDecode } from 'jwt-decode'
import { emitter } from '@/stores/eventBus'
import TryOnProduct from '@/components/specicals/TryOnProduct.vue' // Import TryOnProduct
import CommentSection from '@/components/comments/CommentSection.vue'
import { formatCurrency } from '@/constants/formatCurrency';


const route = useRoute()
const getUrlAPI = ref(GetApiUrl())
const id = route.params.id
const product = ref({})
const productExists = ref(true) // State for error handling
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

// Add a new ref for overall loading state
const overallLoading = ref(true)
const readToken = ref({})
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

const token = Cookies.get('accessToken')
const decodedToken = ReadToken(token)
const idKhachHang = decodedToken ? decodedToken.IdUser : null
const isFavorited = ref(false)
const checkFavoriteProduct = async () => {
  if (!idKhachHang) return
}
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
    const response = await fetch(getUrlAPI.value + '/api/Favorite/CheckFavoriteProduct', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        maSp: id,
        maKh: idKhachHang,
      }),
    })
    const data = await response.json()
    isFavorited.value = data.isFavorited
  } catch (error) {
    console.error('Lỗi khi kiểm tra sản phẩm yêu thích:', error)
  }
}

const toggleFavoriteProduct = async () => {
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
    // Store original state for rollback if needed
    const originalState = isFavorited.value

    if (isFavorited.value) {
      // Remove from favorites
      const response = await fetch(getUrlAPI.value + '/api/Favorite/DeleteFavoriteProducts', {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          maKh: idKhachHang,
          maSp: id,
        }),
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      if (data.success !== false) {
        isFavorited.value = false
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
        throw new Error(data.message || 'Failed to remove from favorites')
      }
    } else {
      // Add to favorites
      const response = await fetch(getUrlAPI.value + '/api/Favorite/AddFavoriteProduct', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          maSp: id,
          maKh: idKhachHang,
        }),
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      if (data.success !== false) {
        isFavorited.value = true
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
        throw new Error(data.message || 'Failed to add to favorites')
      }
    }
  } catch (error) {
    console.error('Error toggling favorite:', error)
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

// Call Api ProductDetails
const fetchAPI = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid) {
      accessToken.value = validatetoken.newAccessToken
      readToken.value = decodeToken(accessToken.value)
    }
    const maKhachHang = readToken.value?.IdUser ?? null
    let url = `${getUrlAPI.value}/api/Shop/Product/${id}`
    if (maKhachHang != null) {
      url += `?maKh=${maKhachHang}`
    }
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) {
      productExists.value = false
      return
    }
    const result = await response.json()
    if (!result || !result.maSp) {
      productExists.value = false
      return
    }

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
        product.value.productDetails?.map((d) => d?.mauSac || '').filter((color) => color !== ''),
      ),
    ]

    selectedColor.value = colors.value[0] || ''
  } catch (error) {
    console.error('Error fetching product:', error)
    productExists.value = false
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
        },
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
    selectedSize.value = newSizes[0] || ''
  }
})

const originalPrice = computed(() => {
  if (!product.value || !product.value.productDetails) return 0
  var match = product.value.productDetails.find(
    (p) =>
      (p?.mauSac || '').toLowerCase() === (selectedColor.value || '').toLowerCase() &&
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase(),
  )
  return match ? match.donGia : 0
})

const maxQuantity = computed(() => {
  if (!product.value || !product.value.productDetails) return 0
  var match = product.value.productDetails.find(
    (p) =>
      (p?.mauSac || '').toLowerCase() === (selectedColor.value || '').toLowerCase() &&
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase(),
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
      (p?.kichThuoc || '').toLowerCase() === (selectedSize.value || '').toLowerCase(),
  )
  var maCtsp = match.maCtsp
  return allImages.value.findIndex((p) => p.maCtsp == maCtsp) + 1
})

watch(showMainImage, (newIndex) => {
  currentImage.value = newIndex
})
watch(maxQuantity, (newMaxQuantity) => {
  if (newMaxQuantity > 0) {
    const currentQty = parseInt(quantity.value)
    if (currentQty > newMaxQuantity) {
      quantity.value = newMaxQuantity.toString()
    }
  } else {
    quantity.value = '0'
  }
})
onMounted(async () => {
  try {
    await fetchAPI()
    await Promise.all([
      fetchRcmProduct(),
      checkFavoriteStatus(), // Use the renamed function
    ])
  } catch (error) {
    console.error('Error during component initialization:', error)
  } finally {
    overallLoading.value = false // Set overall loading to false after all fetches
    // Initialize carousel
    nextTick(() => {
      const $carousel = $(".product__details__pic__slider")

      if ($carousel.length === 0) {
        console.warn('Carousel element not found in DOM')
        return
      }

      if (typeof $carousel.owlCarousel !== 'function') {
        console.error('owlCarousel is not a function. OwlCarousel not loaded')
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
  }
})

const changeImage = (index) => {
  currentImage.value = index
  $('.product__details__pic__slider').trigger('to.owl.carousel', [index - 1, 300])
}

const validateQuantity = () => {
  let value = quantity.value.toString().trim()

  // Remove any non-digit characters
  value = value.replace(/[^\d]/g, '')

  if (value === '' || value === '0') {
    quantity.value = '1'
    return
  }

  const number = parseInt(value)
  if (isNaN(number) || number < 1) {
    quantity.value = '1'
  } else if (number > maxQuantity.value) {
    quantity.value = maxQuantity.value.toString()
    // Show warning message when user tries to exceed max quantity
    Swal.fire({
      title: `Số lượng tối đa là ${maxQuantity.value}`,
      icon: 'warning',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
  } else {
    quantity.value = number.toString()
  }
}
const handleQuantityInput = (event) => {
  // Allow only numbers and prevent negative/decimal values
  const value = event.target.value
  const sanitized = value.replace(/[^\d]/g, '')

  if (sanitized !== value) {
    event.target.value = sanitized
    quantity.value = sanitized
  }

  validateQuantity()
}
const addToCompare = () => {
  const productToAdd = {
    id: product.value.maSp,
    name: product.value.tenSanPham,
    image: `${getUrlAPI.value.replace('/api', '')}/HinhAnh/Products/${allImages.value[currentImage.value - 1]?.tenHinhAnh}`,
    type: 'single',
    category: product.value.tenLoai,
    description: product.value.moTa,
    rating: product.value.danhGia,
    info: product.value.thongTin,
    variant: {
      color: selectedColor.value,
      size: selectedSize.value,
      price: originalPrice.value,
    },
    variants: product.value.productDetails.map((d) => ({
      color: d.mauSac,
      size: d.kichThuoc,
      price: d.donGia,
    })),
  }
  CompareStorageHelper.addProductToCompare(productToAdd)
  Swal.fire({
    title: 'Đã thêm vào danh sách so sánh',
    icon: 'success',
    timer: 1500,
    showConfirmButton: false,
  })
}
const checkFavoriteStatus = async () => {
  if (!idKhachHang) {
    isFavorited.value = false
    return
  }

  try {
    const response = await fetch(getUrlAPI + '/api/Favorite/CheckFavoriteProduct', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        maSp: id,
        maKh: idKhachHang,
      }),
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    const data = await response.json()
    isFavorited.value = data.isFavorited || false
  } catch (error) {
    console.error('Error checking favorite status:', error)
    isFavorited.value = false
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
          (p.mauSac != null ? p.mauSac?.toLowerCase() : '') ===
          selectedColor.value?.toLowerCase() &&
          (p.kichThuoc != null ? p.kichThuoc?.toLowerCase() : '') ===
          selectedSize.value?.toLowerCase(),
      )
      console.log(product.value.productDetails)
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
        if (response.status >= 400 && response.status <= 403) {
          router.push('/Login')
          return
        } else {
          Swal.fire({
            title: result.error || 'Đã xảy ra lỗi',
            icon: 'error',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
          })
          return
        }
      }

      if (result.success) {
        Swal.fire({
          title: 'Đã thêm sản phẩm vào giỏ hàng.',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })

        emitter.emit('cart-updated')
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



const formatRating = (rating) => {
  if (rating === null || rating === undefined) {
    return '5.0'; // Default to 5 if no rating
  }
  // Round to nearest 0.5
  const rounded = Math.round(rating * 2) / 2;
  return rounded.toFixed(1);
};

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

const productForTryOn = computed(() => {
  if (!product.value || !product.value.maSp) return null

  const imageUrls = allImages.value.map(
    (img) => `${getUrlAPI.value.replace('/api', '')}/HinhAnh/Products/${img.tenHinhAnh}`,
  )

  return {
    ...product.value,
    images: imageUrls,
    id: product.value.maSp,
    name: product.value.tenSanPham,
    type: 'single',
    category: product.value.tenLoai,
    products: product.value.productDetails.map((detail) => ({
      ...detail,
      image:
        detail.images.length > 0
          ? `${getUrlAPI.value.replace('/api', '')}/HinhAnh/Products/${detail.images[0].tenHinhAnh}`
          : '',
    })),
  }
})

watch(
  () => route.params.id,
  async () => {
    allImages.value = []
    selectedColor.value = ''
    selectedSize.value = ''
    quantity.value = '1'
    currentSlider.value = 1
    currentImage.value = 1
    isLoading.value = true
    isFavorited.value = false // Reset favorite status

    await Promise.all([
      fetchAPI(),
      fetchRcmProduct(),
      checkFavoriteStatus(), // Check favorite status for new product
    ])
  },
)
</script>

<template>
  <div class="">
    <div v-if="overallLoading" class="loading-spinner-overlay">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2 text-muted">Đang tải dữ liệu sản phẩm...</p>
    </div>
    <div v-else>
      <div v-if="productExists">
        <!-- Product Details Section Begin -->
        <section class="product-details spad">
          <div class="" style="margin-left: 100px; margin-right: 100px; margin-top: -50px">
            <nav aria-label="breadcrumb" class="mb-4">
              <ol class="breadcrumb">
                <li class="breadcrumb-item">
                  <router-link to="/" class="text-decoration-none text-muted">Trang chủ</router-link>
                </li>
                <li class="breadcrumb-item">
                  <a href="#" class="text-decoration-none text-muted">Sản phẩm</a>
                </li>
                <li class="breadcrumb-item active text-muted" aria-current="page">
                  {{ product.tenSanPham }}
                </li>
              </ol>
            </nav>
            <div class="row">
              <!-- Left Column - Product Images -->
              <div class="col-md-6">
                <!-- Main Product Image -->
                <div class="mb-3 text-center">
                  <img v-if="allImages.length > 0 && currentImage > 0"
                    :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${allImages[currentImage - 1]?.tenHinhAnh}`"
                    :alt="product.tenSanPham" class="img-fluid"
                    style="max-height: 500px; object-fit: contain; border-radius: 12px" />
                </div>

                <!-- Thumbnail Images -->
                <div class="row g-2" v-if="allImages.length > 0">
                  <div class="col-3" v-for="(image, index) in allImages.slice(0, 4)" :key="index">
                    <img :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${image.tenHinhAnh}`"
                      :alt="`Thumbnail ${index + 1}`" class="img-fluid w-100 rounded-2 border"
                      :class="{ 'border-2 border-danger': currentImage === index + 1 }"
                      style="height: 80px; object-fit: contain; cursor: pointer" @click="changeImage(index + 1)" />
                  </div>
                </div>
              </div>

              <!-- Middle Column - Product Info -->
              <div class="col-md-4">
                <!-- Product Title -->
                <h1 class="h2 fw-bold mb-3" style="color: black">{{ product.tenSanPham }}</h1>

                <!-- Rating -->
                <div class="d-flex align-items-center mb-3">
                  <div class="product__rating">
                    <i class="fa fa-star" style="color: #ffc107;"></i>
                    <span>{{ formatRating(product.averageRating) }}</span>
                  </div>
                  <span class="text-muted ms-2">({{ product.reviewCount }} đánh giá)</span>
                </div>

                <!-- Product Status and Brand -->
                <div class="mb-3">
                  <div class="d-flex align-items-center mb-2">
                    <span class="me-3">Tình trạng:</span>
                    <span class="text-success fw-bold">{{
                      maxQuantity > 0 ? 'Còn hàng' : 'Hết hàng'
                      }}</span>
                  </div>
                  <div class="d-flex align-items-center">
                    <span class="me-3">Mã sản phẩm:</span>
                    <span class="text-success fw-bold">{{ product.maSp || 'N/A' }}</span>
                  </div>
                </div>

                <!-- Price -->
                <div class="mb-4">
                  <div class="h4 text-danger fw-bold mb-1">{{ formatCurrency(originalPrice) }}</div>
                </div>

                <!-- Color Selection -->
                <div class="mb-3" v-if="colors.length > 0">
                  <span class="fw-bold mb-2 d-block">Màu sắc:</span>
                  <div class="d-flex gap-2 flex-wrap">
                    <button v-for="color in colors" :key="color" :class="[
                      'btn',
                      'btn-outline-secondary',
                      'btn-sm',
                      { active: selectedColor === color },
                    ]" @click="selectColor(color)">
                      {{ color }}
                    </button>
                  </div>
                </div>

                <!-- Size Selection -->
                <div class="mb-3" v-if="sizes.length > 0">
                  <span class="fw-bold mb-2 d-block">Kích thước:</span>
                  <div class="d-flex gap-2 flex-wrap">
                    <button v-for="size in sizes" :key="size" :class="[
                      'btn',
                      'btn-outline-secondary',
                      'btn-sm',
                      { active: selectedSize === size },
                    ]" @click="selectSize(size)">
                      {{ size }}
                    </button>
                  </div>
                </div>

                <!-- Quantity -->
                <div class="mb-4">
                  <span class="fw-bold mb-2 d-block">Số lượng:</span>
                  <div class="d-flex align-items-center">
                    <div class="input-group" style="width: 140px">
                      <button class="btn btn-outline-secondary" type="button"
                        @click="quantity = Math.max(1, parseInt(quantity) - 1).toString()">
                        -
                      </button>
                      <input type="number" class="form-control text-center" v-model="quantity" @input="validateQuantity"
                        min="1" :max="maxQuantity" />
                      <button class="btn btn-outline-secondary" type="button"
                        @click="quantity = Math.min(maxQuantity, parseInt(quantity) + 1).toString()">
                        +
                      </button>
                    </div>
                    <span class="ms-3 text-muted small">Còn {{ maxQuantity }} sản phẩm</span>
                  </div>
                </div>

                <!-- Action Buttons -->
                <div style="display: grid; gap: 8px; margin-bottom: 1.5rem">
                  <!-- Nút Thêm vào giỏ -->
                  <button @click="addToCart" :disabled="maxQuantity <= 0" style="
                    background-color: red;
                    color: white;
                    border: 1px solid red;
                    padding: 12px 16px;
                    border-radius: 6px;
                    font-weight: 600;
                    cursor: pointer;
                    transition: all 0.3s ease;
                    font-size: 16px;
                  " @mouseover="
                    $event.target.style.backgroundColor = '#dc3545';
                  $event.target.style.borderColor = '#dc3545'
                    " @mouseout="
                    $event.target.style.backgroundColor = 'red';
                  $event.target.style.borderColor = 'red'
                    ">
                    <i class="fas fa-shopping-cart" style="margin-right: 8px"></i>THÊM VÀO GIỎ
                  </button>

                  <!-- Hàng nút Yêu thích và So sánh -->
                  <div style="display: flex; gap: 8px">
                    <!-- Nút Yêu thích -->
                    <button @click="toggleFavoriteProduct" :style="isFavorited
                        ? 'background-color: transparent; color: #007bff; border: 1px solid #007bff; padding: 8px 12px; border-radius: 6px; font-size: 14px; cursor: pointer; flex: 1; min-height: 38px; display: flex; align-items: center; justify-content: center; transition: all 0.3s ease;'
                        : 'background-color: transparent; color: red; border: 1px solid red; padding: 8px 12px; border-radius: 6px; font-size: 14px; cursor: pointer; flex: 1; min-height: 38px; display: flex; align-items: center; justify-content: center; transition: all 0.3s ease;'
                      " @mouseover="
                      isFavorited
                        ? (($event.target.style.backgroundColor = '#007bff'),
                          ($event.target.style.color = 'white'))
                        : (($event.target.style.backgroundColor = 'red'),
                          ($event.target.style.color = 'white'))
                      " @mouseout="
                      isFavorited
                        ? (($event.target.style.backgroundColor = 'transparent'),
                          ($event.target.style.color = '#007bff'))
                        : (($event.target.style.backgroundColor = 'transparent'),
                          ($event.target.style.color = 'red'))
                      ">
                      <i class="fas fa-heart" style="margin-right: 4px; color: red"></i>
                      {{ isFavorited ? 'Đã thích' : 'Yêu thích' }} ({{ favoriteCount || 0 }})
                    </button>

                    <!-- Nút So sánh -->
                    <button @click="addToCompare" style="
                      background-color: transparent;
                      color: #007bff;
                      border: 1px solid #007bff;
                      padding: 8px 12px;
                      border-radius: 6px;
                      font-size: 14px;
                      cursor: pointer;
                      flex: 1;
                      min-height: 38px;
                      display: flex;
                      align-items: center;
                      justify-content: center;
                      transition: all 0.3s ease;
                    " @mouseover="
                      $event.target.style.backgroundColor = '#007bff';
                    $event.target.style.color = 'white'
                      " @mouseout="
                      $event.target.style.backgroundColor = 'transparent';
                    $event.target.style.color = '#007bff'
                      ">
                      <i class="bi bi-arrow-left-right" style="margin-right: 4px"></i> So sánh
                    </button>
                  </div>
                  <TryOnProduct :product="productForTryOn" v-if="productForTryOn" />
                </div>

                <!-- Product Features -->
                <div class="mb-4">
                  <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-check-circle text-success me-2"></i>
                    <small>Cam kết 100% chính hãng</small>
                  </div>
                  <!-- <div class="d-flex align-items-center mb-2">
                          <i class="fas fa-shipping-fast text-success me-2"></i>
                          <small>Miễn phí giao hàng</small>
                      </div> -->
                  <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-headset text-success me-2"></i>
                    <small>Hỗ trợ 24/7</small>
                  </div>
                  <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-undo text-success me-2"></i>
                    <small>Hoàn tiền 200% nếu hàng giả</small>
                  </div>
                  <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-shield-alt text-success me-2"></i>
                    <small>Mô hình kiểm tra nhãn hàng</small>
                  </div>
                  <div class="d-flex align-items-center mb-2">
                    <i class="fas fa-clock text-success me-2"></i>
                    <small>Đổi/trả trong 3 ngày</small>
                  </div>
                </div>
              </div>

              <!-- Right Column - Related Products -->
              <div class="col-md-2" v-if="isLogin && recommendationProduct.length > 0">
                <div class="mb-4">
                  <!-- Section Header -->
                  <div class="d-flex justify-content-between align-items-center mb-3">
                    <h5 class="fw-bold mb-0">Sản phẩm gợi ý</h5>
                  </div>

                  <!-- Related Products Grid - Vertical Layout -->
                  <div class="d-flex flex-column gap-3">
                    <!-- Product Item -->
                    <div v-for="item in recommendationProduct.slice(0, 5)" :key="item.maSp" class="">
                      <div class="row g-0">
                        <div class="col-4">
                          <img
                            :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${item.productDetails[0].images[0].tenHinhAnh}`"
                            :alt="item.tenSanPham" class="img-fluid rounded-start"
                            style="height: 90px; width: 100%; object-fit: contain;">
                        </div>
                        <div class="col-8">
                          <div class="card-body p-2">
                            <h4 class="card-title mb-1" style="font-size: 0.8rem; padding-bottom: 5px;">
                              <router-link :to="`/product/${item.maSp}`" class="text-decoration-none text-dark">
                                {{ item.tenSanPham }}
                              </router-link>

                            </h4>
                            <h6 style="padding-bottom: 5px;"> </h6>
                            <div class="d-flex align-items-center justify-content-between">
                              <span class="text-danger fw-bold" style="font-size: 1rem;">{{ formatCurrency(item.khoangGia) }}</span>

                            </div>
                          </div>
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
            <!-- Optimized Tab Section with Dynamic Spacing -->
            <div class="row" :class="{ 'compact-spacing': isShortDescription }">
              <div class="col-lg-12">
                <div class="product-tabs-container">
                  <!-- Tab Navigation -->
                  <ul class="nav nav-tabs custom-tabs" role="tablist">
                    <li class="nav-item">
                      <a class="nav-link custom-tab-link" :class="{ active: activeTab === 'desc' }" href="#"
                        @click.prevent="activeTab = 'desc'">
                        <span class="tab-icon">📝</span>
                        Mô tả
                      </a>
                    </li>
                    <li class="nav-item">
                      <a class="nav-link custom-tab-link" :class="{ active: activeTab === 'review' }" href="#"
                        @click.prevent="activeTab = 'review'">
                        <span class="tab-icon">⭐</span>
                        Đánh giá
                      </a>
                    </li>
                    <li class="nav-item">
                      <a class="nav-link custom-tab-link" :class="{ active: activeTab === 'comment' }" href="#"
                        @click.prevent="activeTab = 'comment'">
                        <span class="tab-icon">💬</span>
                        Bình luận
                      </a>
                    </li>
                  </ul>

                  <!-- Tab Content -->
                  <div class="tab-content custom-tab-content">
                    <div v-show="activeTab == 'desc'" class="tab-pane custom-tab-pane" :class="[
                      activeTab == 'desc' ? 'active' : '',
                      { 'short-content': isShortDescription },
                    ]" id="tabs-1" role="tabpanel">
                      <div class="description-content">
                        <p v-html="product.moTa" class="description-text"></p>
                        <div v-if="isShortDescription" class="content-spacer"></div>
                      </div>
                    </div>
                    <div v-show="activeTab == 'review'" class="tab-pane custom-tab-pane"
                      :class="[activeTab == 'review' ? 'active' : '']" id="tabs-2" role="tabpanel">
                      <div class="review-content">
                        <ReviewProductCombo :objectId="id" :isProduct="true" />
                      </div>
                    </div>
                    <div v-show="activeTab == 'comment'" class="tab-pane custom-tab-pane"
                      :class="[activeTab == 'comment' ? 'active' : '']" id="tabs-3" role="tabpanel">
                      <div class="comment-content">
                        <CommentSection v-if="product.maSp" :objectId="product.maSp" objectType="product" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Recommendation Section with Smart Spacing -->
          </div>
        </section>
        <!-- Product Details Section End -->
      </div>
      <div v-else class="text-center py-5 ">
        <div class="row justify-content-center align-items-center" style="height: 50vh;">          
          <div class="col-12">
            <i class="fas fa-box-open fa-4x text-muted mb-3"></i>
            <h3 class="text-muted">Sản phẩm không tồn tại</h3>
            <p class="text-muted">Sản phẩm bạn tìm không tồn tại trong hệ thống hoặc hiện không được cửa hàng phục vụ.
            </p>
            <router-link to="/shop" class="btn btn-primary mt-3">Quay lại cửa hàng</router-link>
          </div>
        </div>
      </div>
    </div>
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

  0%,
  20%,
  50%,
  80%,
  100% {
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

  0%,
  100% {
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
.product-card:nth-child(1) {
  animation-delay: 0.1s;
}

.product-card:nth-child(2) {
  animation-delay: 0.2s;
}

.product-card:nth-child(3) {
  animation-delay: 0.3s;
}

.product-card:nth-child(4) {
  animation-delay: 0.4s;
}

.product-card:nth-child(5) {
  animation-delay: 0.5s;
}

.product-card:nth-child(6) {
  animation-delay: 0.6s;
}

.product-card:nth-child(7) {
  animation-delay: 0.7s;
}

.product-card:nth-child(8) {
  animation-delay: 0.8s;
}
</style>