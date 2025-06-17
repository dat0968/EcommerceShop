<script setup>
import ReviewProductCombo from '@/components/reviews/ReviewProductCombo.vue'
import $ from 'jquery'

import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import recommendationview from '@/components/RecommendationProduct/RecomendationProduct.vue'
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

// Call Api ProductDetails
const fetchAPI = async () => {
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

  // Xử lý moTa để thêm xuống dòng và định dạng
  if (product.value.moTa) {
    product.value.moTa = product.value.moTa
      .replace(/\*\*([^*]+)\*\*/g, '<br><strong>$1</strong><br>') // Chuyển **...** thành <strong> và thêm <br> trước/sau
      .replace(/\n/g, '<br>') // Chuyển các ký tự xuống dòng \n thành <br>
  }
  colors.value = [
    ...new Set(
      product.value.productDetails?.map((d) => d?.mauSac || '').filter((color) => color !== ''),
    ),
  ]

  selectedColor.value = colors.value[0]
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
// Đồng bộ giá trị mỗi khi showMainImage thay đổi
watch(showMainImage, (newIndex) => {
  currentImage.value = newIndex
})
const isLogin = computed(() => {
  if (accessToken.value != undefined && accessToken.value != '') {
    return true
  }
  return false
})
console.log(accessToken.value)
onMounted(() => {
  fetchAPI()
  fetchRcmProduct()
  // Cuộn lên đầu trang
  window.scrollTo({
    top: 0,
    behavior: 'smooth', // Cuộn mượt mà
  })

  // Initialize Owl Carousel
  /* const owl = $('.product__details__pic__slider').owlCarousel({
    items: 1,
    loop: true,
    autoplay: false,
    nav: false,
    dots: true,
    animateOut: 'fadeOut',
    animateIn: 'fadeIn',
  })

  // Sync thumbnail clicks with large image
  $('.pt').on('click', function () {
    const index = $(this).index()
    owl.trigger('to.owl.carousel', [index, 300])
    currentImage.value = index + 1
  })

  // Update active thumbnail when carousel changes
  owl.on('changed.owl.carousel', function (event) {
    currentImage.value = event.item.index + 1 - event.item.count
    if (currentImage.value < 1) currentImage.value += event.item.count
  }) */
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
          p.kichThuoc?.toLowerCase() === selectedSize.value?.toLowerCase(),
      )

      const content = {
        maKh: readToken.IdUser,
        maCtsp: matched.maCtsp,
        maCombo: null,
        soLuong: quantity.value,
        donGia: matched.donGia,
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

const recommendationProduct = ref([])
const fetchRcmProduct = async () => {
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
    console.log(recommendationProduct.value)
  }
}
const activeTab = ref('desc')

watch(
  () => route.params.id,
  async () => {
    allImages.value = []
    selectedColor.value = ''
    selectedSize.value = ''
    quantity.value = 1
    currentSlider.value = 1
    currentImage.value = 1
    await fetchAPI()
  },
)
</script>
<template>
  <div>
    <!-- Product Details Section Begin -->
    <section class="product-details spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-6">
            <div class="product__details__pic">
              <div
                style="position: relative; margin-bottom: 20px"
                class="product__details__slider__content"
              >
                <div class="product__details__pic__slider owl-carousel">
                  <div v-for="(image, index) in allImages" :key="index">
                    <img
                      v-if="index + 1 == currentImage"
                      :data-hash="`product-${index}`"
                      class="product__big__img"
                      :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${image.tenHinhAnh}`"
                      alt=""
                    />
                  </div>
                </div>
              </div>
              <!-- Thumbnail ảnh nhỏ nằm dưới ảnh lớn -->
              <div
                class="product__details__thumbnails d-flex justify-content-center col-lg-6"
                style="max-width: 100%; display: flex; justify-content: center; margin: 20px"
              >
                <div class="carousel slide w-100">
                  <div class="carousel-inner">
                    <div
                      v-for="(imageGroup, index) in slideChunks"
                      :key="index"
                      :class="['carousel-item', { active: currentSlider === index + 1 }]"
                    >
                      <div class="d-flex gap-2 justify-content-center" style="width: 100%">
                        <img
                          v-for="(image, imageindex) in imageGroup"
                          :key="imageindex"
                          :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                            image.tenHinhAnh
                          }`"
                          class="img-fluid"
                          :style="{ width: `${100 / imageGroup.length}%`, height: '100px' }"
                          alt=""
                          @click.prevent="changeImage(index * chunkSize + imageindex + 1)"
                        />
                      </div>
                    </div>
                  </div>

                  <button
                    @click="prevImage"
                    class="carousel-control-prev"
                    type="button"
                    style="
                      width: 40px;
                      height: 40px;
                      top: 50%;
                      transform: translateY(-50%);
                      background-color: gray;
                    "
                  >
                    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                  </button>
                  <button
                    @click="nextImage"
                    class="carousel-control-next"
                    type="button"
                    style="
                      width: 40px;
                      height: 40px;
                      top: 50%;
                      transform: translateY(-50%);
                      background-color: gray;
                    "
                  >
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
                    <a href="#"><span class="icon_heart_alt"></span></a>
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
                      <button
                        v-for="(color, index) in colors"
                        :key="index"
                        :class="['btn', 'btn-light', { active: selectedColor === color }]"
                        @click="selectColor(color)"
                        style="background-color: #e0e0e0; border: 1px solid #ccc; font-weight: 500"
                      >
                        {{ color }}
                      </button>
                    </div>
                  </li>
                  <li style="display: flex; align-items: center" v-if="sizes.length > 0">
                    <span style="min-width: 120px">Kích thước:</span>
                    <div class="size__checkbox" style="display: flex; gap: 8px">
                      <button
                        v-for="(size, index) in sizes"
                        :key="index"
                        :class="['btn', 'btn-light', { active: selectedSize === size }]"
                        @click="selectSize(size)"
                        style="background-color: #e0e0e0; border: 1px solid #ccc; font-weight: 500"
                      >
                        {{ size }}
                      </button>
                    </div>
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <div class="col-lg-12">
            <!-- Thay thế phần tab hiện tại bằng đoạn này -->
            <ul class="nav nav-tabs" role="tablist">
              <li class="nav-item">
                <a
                  class="nav-link"
                  :class="{ active: activeTab === 'desc' }"
                  href="#"
                  @click.prevent="activeTab = 'desc'"
                  >Mô tả</a
                >
              </li>
              <li class="nav-item">
                <a
                  class="nav-link"
                  :class="{ active: activeTab === 'review' }"
                  href="#"
                  @click.prevent="activeTab = 'review'"
                  >Đánh giá</a
                >
              </li>
            </ul>
            <div class="tab-content vh-100 overflow-auto">
              <div
                v-show="activeTab == 'desc'"
                class="tab-pane"
                :class="[activeTab == 'desc' ? 'active' : '']"
                id="tabs-1"
                role="tabpanel"
              >
                <p v-html="product.moTa"></p>
              </div>
              <div
                v-show="activeTab == 'review'"
                class="tab-pane"
                :class="[activeTab == 'review' ? 'active' : '']"
                id="tabs-2"
                role="tabpanel"
              >
                <ReviewProductCombo :objectId="id" :isProduct="true" />
              </div>
            </div>
          </div>
        </div>
        <!-- <div v-if="isLogin" class="row">
          <div class="col-lg-12 text-center">
            <div class="related__title">
              <h5>GỢI Ý CHO BẠN</h5>
            </div>
          </div>
          <div
            v-for="item in recommendationProduct"
            :key="item.maSp"
            class="col-lg-3 col-md-4 col-sm-6"
          >
            <div class="product__item">
              <div
                class="product__item__pic set-bg"
                :data-setbg="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                  item.productDetails[0].images[0].tenHinhAnh
                }`"
              >
                <img
                  :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                    item.productDetails[0].images[0].tenHinhAnh
                  }`"
                  class="image-popup"
                  style="width: 100%; height: 300px; object-fit: cover; border-radius: 8px"
                /><span class="arrow_expand"></span>
                <ul class="product__hover">
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6>
                  <router-link :to="`/product/${item.maSp}`" style="text-decoration-line: none">{{
                    item.tenSanPham
                  }}</router-link>
                </h6>
               
                <div style="color: red" class="product__price">{{ item.khoangGia }}</div>
              </div>
            </div>
          </div>
        </div> -->
        <recommendationview></recommendationview>
      </div>
    </section>
    <!-- Product Details Section End -->
  </div>
</template>

<style scoped>
.carousel-item img {
  object-fit: cover;
  max-height: 150px;
}
/* Slider container */
.product__details__pic__slider {
  position: relative;
}

/* Navigation buttons */
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

/* Thumbnail styles */
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

/* Ensure images are responsive */
.product__big__img {
  width: 100%;
  height: 500px;
}
.product__details__pic__left .pt img {
  width: 100px; /* Hoặc điều chỉnh kích thước phù hợp */
  height: 100px;
  object-fit: cover; /* Đảm bảo ảnh không bị méo */
  border-radius: 5px;
  margin-bottom: 10px;
}
.btn.active {
  background-color: #4a90e2 !important;
  color: white !important;
  border-color: #357ab8 !important;
}
</style>
