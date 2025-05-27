
<template>
  <div>
    <!-- Product Details Section Begin -->
    <section class="product-details spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-6">
            <div class="product__details__pic">
              <div style="position: relative" class="product__details__slider__content">
                <div class="product__details__pic__slider owl-carousel">
                  <div>
                    <img
                      data-hash="product-1"
                      class="product__big__img"
                      src="@/assets/Customer/img/product/details/thumb-1.jpg"
                      alt=""
                    />
                  </div>
                  <div>
                    <img
                      data-hash="product-2"
                      class="product__big__img"
                      src="@/assets/Customer/img/product/details/thumb-2.jpg"
                      alt=""
                    />
                  </div>
                  <div>
                    <img
                      data-hash="product-3"
                      class="product__big__img"
                      src="@/assets/Customer/img/product/details/thumb-3.jpg"
                      alt=""
                    />
                  </div>
                  <div>
                    <img
                      data-hash="product-4"
                      class="product__big__img"
                      src="@/assets/Customer/img/product/details/thumb-4.jpg"
                      alt=""
                    />
                  </div>
                </div>
              </div>
              <!-- Thumbnail ảnh nhỏ nằm dưới ảnh lớn -->
                <div
                  class="product__details__thumbnails d-flex justify-content-center col-lg-6"
                  style="max-width: 100%; display: flex; justify-content: center"
                >
                  <div class="carousel slide w-100">
                    <div class="carousel-inner">
                      <div :class="['carousel-item', { active: currentSlider === 1 }]">
                        <div class="d-flex gap-2 justify-content-center" style="width: 100%;">
                          <img
                            src="@/assets/Customer/img/product/details/thumb-1.jpg"
                            class="img-fluid w-25"
                            alt=""
                            @click.prevent="changeImage(1)"
                          />
                          <img
                            src="@/assets/Customer/img/product/details/thumb-2.jpg"
                            class="img-fluid w-25"
                            alt=""
                            @click.prevent="changeImage(2)"
                          />
                          <img
                            src="@/assets/Customer/img/product/details/thumb-3.jpg"
                            class="img-fluid w-25"
                            alt=""
                            @click.prevent="changeImage(3)"
                          />
                          <img
                            src="@/assets/Customer/img/product/details/thumb-4.jpg"
                            class="img-fluid w-25"
                            alt=""
                            @click.prevent="changeImage(4)"
                          />
                        </div>
                      </div>
                    </div>

                    <button @click="prevImage" class="carousel-control-prev" type="button" style="width: 40px; height: 40px; top: 50%; transform: translateY(-50%);">
                      <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                    </button>
                    <button @click="nextImage" class="carousel-control-next" type="button" style="width: 40px; height: 40px; top: 50%; transform: translateY(-50%);">
                      <span class="carousel-control-next-icon" aria-hidden="true"></span>
                    </button>
                  </div>
                </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="product__details__text">
              <h3>
                Essential structured blazer <span>Brand: SKMEIMore Men Watches from SKMEI</span>
              </h3>
              <div class="product__details__price">$ 75.0 <span>$ 83.0</span></div>
              <div class="product__details__button">
                <div class="quantity">
                  <span>Quantity:</span>
                  <div class="pro-qty">
                    <input type="text" value="1" />
                  </div>
                </div>
                <a href="#" class="cart-btn"><span class="icon_bag_alt"></span> Add to cart</a>
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
                  <li style="display: flex; align-items: center">
                    <span style="min-width: 120px">Available color:</span>
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
                  <li style="display: flex; align-items: center">
                    <span style="min-width: 120px">Available size:</span>
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
                  <a class="nav-link active" data-toggle="tab" href="#tabs-1" role="tab"
                    >Description</a
                  >
                </li>
              </ul>
              <div class="tab-content">
                <div class="tab-pane active" id="tabs-1" role="tabpanel">
                  <p>
                    Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut loret
                    fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi
                    nesciunt loret. Neque porro lorem quisquam est, qui dolorem ipsum quia dolor si.
                    Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut loret
                    fugit, sed quia ipsu consequuntur magni dolores eos qui ratione voluptatem sequi
                    nesciunt. Nulla consequat massa quis enim.
                  </p>
                  <p>
                    Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula
                    eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient
                    montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque
                    eu, pretium quis, sem.
                  </p>
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
  
<script setup>
import { ref, onMounted } from 'vue'
const currentSlider = ref(1)
const maxSlide = 1
const prevImage = () => {
  currentSlider.value = currentSlider.value === 1 ? maxSlide : currentSlider.value - 1
}

const nextImage = () => {
  currentSlider.value = currentSlider.value === maxSlide ? 1 : currentSlider.value + 1
}
const colors = ['Đỏ', 'Xanh', 'Vàng'] // Mảng các màu sắc
const selectedColor = ref('Đỏ') // Màu được chọn mặc định
const selectedSize = ref('X') // Màu được chọn mặc định
const selectColor = (color) => {
  selectedColor.value = color // Cập nhật màu được chọn
}
const sizes = ['X', 'M', 'L'] // Mảng các màu sắc

const selectSize = (size) => {
  selectedSize.value = size // Cập nhật màu được chọn
}
const currentImage = ref(1)

onMounted(() => {
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
