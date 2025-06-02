<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
const getUrlAPI = ref('https://localhost:7217/api')
const id = route.params.id
const product = ref({})
const allImages = ref([])
const currentSlider = ref(1)
const colors = ref([])
const selectedColor = ref('')
const selectedSize = ref('')
// Call Api ProductDetails
const fetchAPI = async () => {
  const response = await fetch(`${getUrlAPI.value}/Shop/Product/${id}`, {
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
      product.value.productDetails?.map((d) => d?.mauSac || '').filter((color) => color !== '')
    ),
  ]

  selectedColor.value = colors.value[0]
  console.log(product.value)
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

onMounted(() => {
  fetchAPI()
  // Cuộn lên đầu trang
  window.scrollTo({
    top: 0,
    behavior: 'smooth', // Cuộn mượt mà
  })
  // Initialize Owl Carousel
  const owl = $('.product__details__pic__slider').owlCarousel({
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
  })
})

const changeImage = (index) => {
  currentImage.value = index
  $('.product__details__pic__slider').trigger('to.owl.carousel', [index - 1, 300])
}
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
                    <input type="text" value="1" />
                  </div>
                </div>
                <a style="text-decoration-line: none" href="#" class="cart-btn"
                  ><span class="icon_bag_alt"></span> Thêm giỏ hàng</a
                >
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
            <div class="product__details__tab">
              <ul class="nav nav-tabs" role="tablist">
                <li class="nav-item">
                  <a class="nav-link active" data-toggle="tab" href="#tabs-1" role="tab">Mô tả</a>
                </li>
              </ul>
              <div class="tab-content">
                <div class="tab-pane active" id="tabs-1" role="tabpanel">
                  <p v-html="product.moTa"></p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="row">
          <div class="col-lg-12 text-center">
            <div class="related__title">
              <h5>RELATED PRODUCTS</h5>
            </div>
          </div>
          <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="product__item">
              <div class="product__item__pic set-bg" data-setbg="img/product/related/rp-1.jpg">
                <div class="label new">New</div>
                <ul class="product__hover">
                  <li>
                    <a href="img/product/related/rp-1.jpg" class="image-popup"
                      ><span class="arrow_expand"></span
                    ></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6><a href="#">Buttons tweed blazer</a></h6>
                <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div>
                <div class="product__price">$ 59.0</div>
              </div>
            </div>
          </div>
          <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="product__item">
              <div class="product__item__pic set-bg" data-setbg="img/product/related/rp-2.jpg">
                <ul class="product__hover">
                  <li>
                    <a href="img/product/related/rp-2.jpg" class="image-popup"
                      ><span class="arrow_expand"></span
                    ></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6><a href="#">Flowy striped skirt</a></h6>
                <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div>
                <div class="product__price">$ 49.0</div>
              </div>
            </div>
          </div>
          <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="product__item">
              <div class="product__item__pic set-bg" data-setbg="img/product/related/rp-3.jpg">
                <div class="label stockout">out of stock</div>
                <ul class="product__hover">
                  <li>
                    <a href="img/product/related/rp-3.jpg" class="image-popup"
                      ><span class="arrow_expand"></span
                    ></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6><a href="#">Cotton T-Shirt</a></h6>
                <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div>
                <div class="product__price">$ 59.0</div>
              </div>
            </div>
          </div>
          <div class="col-lg-3 col-md-4 col-sm-6">
            <div class="product__item">
              <div class="product__item__pic set-bg" data-setbg="img/product/related/rp-4.jpg">
                <ul class="product__hover">
                  <li>
                    <a href="img/product/related/rp-4.jpg" class="image-popup"
                      ><span class="arrow_expand"></span
                    ></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6><a href="#">Slim striped pocket shirt</a></h6>
                <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div>
                <div class="product__price">$ 59.0</div>
              </div>
            </div>
          </div>
        </div>
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
