<template>
  <div>
    <!-- Combo Details Section Begin -->
    <section class="product-details spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-6">
            <div class="product__details__pic">
              <div style="position: relative" class="product__details__slider__content">
                <div class="product__details__pic__slider owl-carousel">
                  <div>
                    <img data-hash="combo-1" class="product__big__img" :src="combo.image" alt="" />
                  </div>
                  <div v-for="(product, index) in combo.products" :key="index">
                    <img
                      :data-hash="'combo-' + (index + 2)"
                      class="product__big__img"
                      :src="product.image"
                      alt=""
                    />
                  </div>
                </div>
              </div>
              <!-- Thumbnail ảnh nhỏ -->
              <div
                class="product__details__thumbnails d-flex justify-content-center col-lg-6"
                style="max-width: 100%; display: flex; justify-content: center"
              >
                <div class="carousel slide w-100">
                  <div class="carousel-inner">
                    <div :class="['carousel-item', { active: currentSlider === 1 }]">
                      <div class="d-flex gap-2 justify-content-center" style="width: 100%">
                        <img
                          :src="combo.image"
                          class="img-fluid w-25"
                          alt=""
                          @click.prevent="changeImage(1)"
                        />
                        <img
                          v-for="(product, index) in combo.products"
                          :key="index"
                          :src="product.image"
                          class="img-fluid w-25"
                          alt=""
                          @click.prevent="changeImage(index + 2)"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="product__details__text">
              <h3>{{ combo.name }}</h3>
              <div class="product__details__price">
                {{ formatPrice(combo.price) }}đ <span>{{ formatPrice(combo.originalPrice) }}đ</span>
              </div>
              <div class="product__details__button">
                <div class="quantity">
                  <span>Số lượng:</span>
                  <div class="pro-qty">
                    <input type="text" v-model="quantity" />
                  </div>
                </div>
                <div class="button-group">
                  <a style="height: 50px;" href="#" class="cart-btn" @click.prevent="addToCart">
                    <span class="icon_bag_alt"></span> Thêm giỏ hàng
                  </a>
                  <div style="margin-bottom: 14px;" class="action-buttons">
                    <a href="#" style="border-radius: 50%; width: 50px; height: 50px;" class="action-btn"><span class="icon_heart_alt"></span></a>
                    <a href="#" style="border-radius: 50%; width: 50px; height: 50px;" class="action-btn"><span class="icon_adjust-horiz"></span></a>
                  </div>
                </div>
              </div>
              <div class="product__details__widget">
                <ul class="variant-list">
                  <li
                    v-for="(product, index) in combo.products"
                    :key="index"
                    class="variant-section"
                  >
                    <div class="variant-title">
                      <h4>{{ product.name }}</h4>
                    </div>
                    <div class="variant-options">
                      <div class="variant-group">
                        <span class="variant-label">Màu sắc:</span>
                        <div class="color__checkbox">
                          <button
                            v-for="(color, colorIndex) in product.colors"
                            :key="colorIndex"
                            :class="[
                              'btn',
                              'btn-light',
                              { active: selectedVariants[index]?.color === color },
                            ]"
                            @click="selectVariant(index, 'color', color)"
                          >
                            {{ color }}
                          </button>
                        </div>
                      </div>
                      <div class="variant-group">
                        <span class="variant-label">Kích thước:</span>
                        <div class="size__checkbox">
                          <button
                            v-for="(size, sizeIndex) in product.sizes"
                            :key="sizeIndex"
                            :class="[
                              'btn',
                              'btn-light',
                              { active: selectedVariants[index]?.size === size },
                            ]"
                            @click="selectVariant(index, 'size', size)"
                          >
                            {{ size }}
                          </button>
                        </div>
                      </div>
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
      </div>
    </section>
    <!-- Combo Details Section End -->
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'

export default {
  name: 'ComboDetails',
  setup() {
    const currentSlider = ref(1)
    const quantity = ref(1)
    const selectedVariants = ref([])

    const combo = ref({
      id: 1,
      name: 'Combo Sản Phẩm Tốt',
      image: '/images/combo.jpg',
      price: 1500000,
      originalPrice: 2000000,
      description: 'Combo bao gồm các sản phẩm chất lượng cao với giá ưu đãi',
      products: [
        {
          id: 1,
          name: 'Sản phẩm 1',
          image: '/images/product1.jpg',
          price: 500000,
          colors: ['Đỏ', 'Xanh', 'Vàng'],
          sizes: ['S', 'M', 'L'],
        },
        {
          id: 2,
          name: 'Sản phẩm 2',
          image: '/images/product2.jpg',
          price: 700000,
          colors: ['Đen', 'Trắng', 'Xám'],
          sizes: ['M', 'L', 'XL'],
        },
        {
          id: 3,
          name: 'Sản phẩm 3',
          image: '/images/product3.jpg',
          price: 800000,
          colors: ['Hồng', 'Tím', 'Cam'],
          sizes: ['S', 'M', 'L'],
        },
      ],
      details: '<p>Chi tiết về combo sản phẩm...</p>',
    })

    const formatPrice = (price) => {
      return price.toLocaleString('vi-VN')
    }

    const changeImage = (index) => {
      currentSlider.value = index
      $('.product__details__pic__slider').trigger('to.owl.carousel', [index - 1, 300])
    }

    const selectVariant = (productIndex, type, value) => {
      if (!selectedVariants.value[productIndex]) {
        selectedVariants.value[productIndex] = {}
      }
      selectedVariants.value[productIndex][type] = value
    }

    const addToCart = () => {
      console.log('Thêm vào giỏ hàng:', {
        comboId: combo.value.id,
        quantity: quantity.value,
        variants: selectedVariants.value,
      })
    }

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

      // Khởi tạo biến thể mặc định
      combo.value.products.forEach((product, index) => {
        selectedVariants.value[index] = {
          color: product.colors[0],
          size: product.sizes[0],
        }
      })
    })

    return {
      combo,
      currentSlider,
      quantity,
      selectedVariants,
      formatPrice,
      changeImage,
      selectVariant,
      addToCart,
    }
  },
}
</script>

<style scoped>
.product__details__pic {
  position: relative;
}

.product__big__img {
  width: 100%;
  height: 500px;
  object-fit: cover;
}

.carousel-item img {
  object-fit: cover;
  max-height: 150px;
}

.product__details__text h3 {
  font-size: 24px;
  margin-bottom: 20px;
}

.product__details__price {
  font-size: 30px;
  color: #e94560;
  margin-bottom: 20px;
}

.product__details__price span {
  font-size: 20px;
  color: #999;
  text-decoration: line-through;
  margin-left: 10px;
}

.product__details__button {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
  gap: 1.5rem;
}

.quantity {
  display: flex;
  align-items: center;
}

.quantity span {
  margin-right: 10px;
}

.button-group {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  color: #666;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background-color: #e9ecef;
  color: #333;
}

.cart-btn {
  padding: 12px 25px;
  background-color: #ca1515;
  color: white;
  border-radius: 6px;
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  border-radius: 50px
}

.cart-btn:hover {
  background-color: #ca1515;
  color: white;
}

.product__details__widget {
  margin-top: 2rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.product__details__widget ul {
  margin: 0;
  padding: 0;
}

.variant-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.variant-section {
  margin-bottom: 2.5rem;
  padding: 1.5rem;
  border: 1px solid #eee;
  border-radius: 8px;
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.variant-title {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.variant-title h4 {
  font-size: 1.2rem;
  color: #333;
  margin: 0;
  font-weight: 600;
}

.variant-options {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.variant-group {
  display: flex;
  align-items: flex-start;
  gap: 1.5rem;
}

.variant-label {
  min-width: 120px;
  font-weight: 500;
  color: #666;
  padding-top: 0.5rem;
}

.color__checkbox,
.size__checkbox {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  flex: 1;
}

.btn {
  padding: 6px 12px;
  transition: all 0.3s ease;
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  font-weight: 500;
  font-size: 0.9rem;
  min-width: 60px;
}

.btn:hover {
  background-color: #e9ecef;
  border-color: #ced4da;
}

.btn.active {
  background-color: #4a90e2 !important;
  color: white !important;
  border-color: #357ab8 !important;
}

.product__details__tab {
  margin-top: 50px;
}

.nav-tabs {
  border-bottom: 1px solid #ddd;
}

.nav-tabs .nav-link {
  border: none;
  color: #999;
  padding: 10px 20px;
}

.nav-tabs .nav-link.active {
  color: #ca1515;
  border-bottom: 2px solid #ca1515;
}

.tab-content {
  padding: 20px 0;
}
</style>
