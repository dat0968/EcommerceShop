<template>
  <div>
    <!-- Breadcrumb Begin -->
    <div class="breadcrumb-option">
      <div class="container">
        <div class="row">
          <div class="col-lg-12">
            <div class="breadcrumb__links">
              <a href="./index.html"><i class="fa fa-home"></i> Trang chủ</a>
              <span>Sản phẩm</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Breadcrumb End -->

    <!-- Shop Section Begin -->
    <section class="shop spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-3 col-md-3">
            <div class="shop__sidebar">
              <div class="sidebar__categories">
                <div class="section-title">
                  <h4>Loại sản phẩm</h4>
                </div>
                <div class="categories__accordion">
                  <div class="accordion" id="accordionExample">
                    <div class="card">
                      <div class="card-heading" @click="toggleCategory('collapseOne')">
                        <a href="javascript:void(0)">Women</a>
                      </div>
                      <div
                        id="collapseOne"
                        class="collapse"
                        :class="{ show: activeCategory === 'collapseOne' }"
                      >
                        <div class="card-body">
                          <ul>
                            <li><a href="#">Coats</a></li>
                            <li><a href="#">Jackets</a></li>
                            <li><a href="#">Dresses</a></li>
                            <li><a href="#">Shirts</a></li>
                            <li><a href="#">T-shirts</a></li>
                            <li><a href="#">Jeans</a></li>
                          </ul>
                        </div>
                      </div>
                    </div>
                    <div class="card">
                      <div class="card-heading" @click="toggleCategory('collapseTwo')">
                        <a href="javascript:void(0)">Men</a>
                      </div>
                      <div
                        id="collapseTwo"
                        class="collapse"
                        :class="{ show: activeCategory === 'collapseTwo' }"
                      >
                        <div class="card-body">
                          <ul>
                            <li><a href="#">Coats</a></li>
                            <li><a href="#">Jackets</a></li>
                            <li><a href="#">Dresses</a></li>
                            <li><a href="#">Shirts</a></li>
                            <li><a href="#">T-shirts</a></li>
                            <li><a href="#">Jeans</a></li>
                          </ul>
                        </div>
                      </div>
                    </div>
                    <div class="card">
                      <div class="card-heading" @click="toggleCategory('collapseThree')">
                        <a href="javascript:void(0)">Kids</a>
                      </div>
                      <div
                        id="collapseThree"
                        class="collapse"
                        :class="{ show: activeCategory === 'collapseThree' }"
                      >
                        <div class="card-body">
                          <ul>
                            <li><a href="#">Coats</a></li>
                            <li><a href="#">Jackets</a></li>
                            <li><a href="#">Dresses</a></li>
                            <li><a href="#">Shirts</a></li>
                            <li><a href="#">T-shirts</a></li>
                            <li><a href="#">Jeans</a></li>
                          </ul>
                        </div>
                      </div>
                    </div>
                    <div class="card">
                      <div class="card-heading" @click="toggleCategory('collapseFour')">
                        <a href="javascript:void(0)">Accessories</a>
                      </div>
                      <div
                        id="collapseFour"
                        class="collapse"
                        :class="{ show: activeCategory === 'collapseFour' }"
                      >
                        <div class="card-body">
                          <ul>
                            <li><a href="#">Coats</a></li>
                            <li><a href="#">Jackets</a></li>
                            <li><a href="#">Dresses</a></li>
                            <li><a href="#">Shirts</a></li>
                            <li><a href="#">T-shirts</a></li>
                            <li><a href="#">Jeans</a></li>
                          </ul>
                        </div>
                      </div>
                    </div>
                    <div class="card">
                      <div class="card-heading" @click="toggleCategory('collapseFive')">
                        <a href="javascript:void(0)">Cosmetic</a>
                      </div>
                      <div
                        id="collapseFive"
                        class="collapse"
                        :class="{ show: activeCategory === 'collapseFive' }"
                      >
                        <div class="card-body">
                          <ul>
                            <li><a href="#">Coats</a></li>
                            <li><a href="#">Jackets</a></li>
                            <li><a href="#">Dresses</a></li>
                            <li><a href="#">Shirts</a></li>
                            <li><a href="#">T-shirts</a></li>
                            <li><a href="#">Jeans</a></li>
                          </ul>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="sidebar__filter">
                <div class="section-title">
                  <h4>Khoảng giá</h4>
                </div>
                <div class="price-buttons">
                  <button
                    v-for="range in priceRanges"
                    :key="range.id"
                    :class="['price-btn', { active: selectedPriceRange === range.id }]"
                    @click="selectPriceRange(range.id)"
                  >
                    {{ range.label }}
                  </button>
                </div>
              </div>
              <div class="sidebar__sizes sidebar__box">
                <div class="section-title">
                  <h4>Size</h4>
                </div>
                <div class="size-buttons">
                  <button
                    v-for="size in sizes"
                    :key="size"
                    :class="['size-btn', { active: selectedSize === size }]"
                    @click="selectSize(size)"
                  >
                    {{ size }}
                  </button>
                </div>
              </div>
              <div class="sidebar__color sidebar__box">
                <div class="section-title">
                  <h4>Màu</h4>
                </div>
                <div class="color-buttons">
                  <button
                    v-for="color in colors"
                    :key="color.value"
                    :class="['color-btn', { active: selectedColor === color.value }]"
                    :style="{ background: color.code }"
                    @click="selectColor(color.value)"
                  >
                    <span v-if="selectedColor === color.value" class="color-check">✔</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-9 col-md-9">
            <div class="row">
              <div class="col-lg-3 col-md-4 col-sm-6 mix men">
                <div class="product__item">
                  <div class="product__item__pic">
                    <img
                      src="@/assets/Customer/img/product/product-2.jpg"
                      alt="Flowy striped skirt"
                    />
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-2.jpg" class="image-popup"
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
              <div class="col-lg-3 col-md-4 col-sm-6 mix accessories">
                <div class="product__item">
                  <div class="product__item__pic">
                    <img src="@/assets/Customer/img/product/product-3.jpg" alt="Cotton T-Shirt" />
                    <div class="label stockout">out of stock</div>
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-3.jpg" class="image-popup"
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
              <div class="col-lg-3 col-md-4 col-sm-6 mix cosmetic">
                <div class="product__item">
                  <div class="product__item__pic">
                    <img
                      src="@/assets/Customer/img/product/product-4.jpg"
                      alt="Slim striped pocket shirt"
                    />
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-4.jpg" class="image-popup"
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
              <div class="col-lg-3 col-md-4 col-sm-6 mix kid">
                <div class="product__item">
                  <div class="product__item__pic">
                    <img
                      src="@/assets/Customer/img/product/product-5.jpg"
                      alt="Fit micro corduroy shirt"
                    />
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-5.jpg" class="image-popup"
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
                    <h6><a href="#">Fit micro corduroy shirt</a></h6>
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
              <div class="col-lg-3 col-md-4 col-sm-6 mix women men kid accessories cosmetic">
                <div class="product__item sale">
                  <div class="product__item__pic">
                    <img src="@/assets/Customer/img/product/product-6.jpg" alt="Tropical Kimono" />
                    <div class="label sale">Sale</div>
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-6.jpg" class="image-popup"
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
                    <h6><a href="#">Tropical Kimono</a></h6>
                    <div class="rating">
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                    </div>
                    <div class="product__price">$ 49.0 <span>$ 59.0</span></div>
                  </div>
                </div>
              </div>
              <div class="col-lg-3 col-md-4 col-sm-6 mix women men kid accessories cosmetic">
                <div class="product__item">
                  <div class="product__item__pic">
                    <img
                      src="@/assets/Customer/img/product/product-7.jpg"
                      alt="Contrasting sunglasses"
                    />
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-7.jpg" class="image-popup"
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
                    <h6><a href="#">Contrasting sunglasses</a></h6>
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
              <div class="col-lg-3 col-md-4 col-sm-6 mix women men kid accessories cosmetic">
                <div class="product__item sale">
                  <div class="product__item__pic">
                    <img
                      src="@/assets/Customer/img/product/product-8.jpg"
                      alt="Water resistant backpack"
                    />
                    <div class="label">Sale</div>
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-8.jpg" class="image-popup"
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
                    <h6><a href="#">Water resistant backpack</a></h6>
                    <div class="rating">
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                    </div>
                    <div class="product__price">$ 49.0 <span>$ 59.0</span></div>
                  </div>
                </div>
              </div>
              <div class="col-lg-12 text-center">
                <div class="pagination__option">
                  <a href="#">1</a>
                  <a href="#">2</a>
                  <a href="#">3</a>
                  <a href="#"><i class="fa fa-angle-right"></i></a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Shop Section End -->
  </div>
</template>

<script>
import $ from 'jquery'
import 'jquery-ui-dist/jquery-ui'

export default {
  data() {
    return {
      activeCategory: 'collapseOne',
      selectedPriceRange: null,
      priceRanges: [
        { id: 1, label: 'Dưới 500K', min: 0, max: 500000 },
        { id: 2, label: '500K - 1 triệu', min: 500000, max: 1000000 },
        { id: 3, label: '1 triệu - 2 triệu', min: 1000000, max: 2000000 },
        { id: 4, label: 'Trên 2 triệu', min: 2000000, max: 100000000 },
      ],
      sizes: ['XXS', 'XS', 'XS-S', 'S', 'M', 'M-L', 'L', 'XL'],
      selectedSize: null,
      colors: [
        { value: 'black', code: '#222' },
        { value: 'white', code: '#fff' },
        { value: 'red', code: '#e74c3c' },
        { value: 'grey', code: '#888' },
        { value: 'blue', code: '#3498db' },
        { value: 'beige', code: '#f5e6ca' },
        { value: 'green', code: '#27ae60' },
        { value: 'yellow', code: '#f1c40f' },
      ],
      selectedColor: null,
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.initPriceRange()
    })
  },
  methods: {
    toggleCategory(categoryId) {
      this.activeCategory = this.activeCategory === categoryId ? null : categoryId
    },
    initPriceRange() {
      const self = this
      if (this.$refs.priceRange) {
        $(this.$refs.priceRange).slider({
          range: true,
          min: 33,
          max: 99,
          values: [33, 99],
          slide: function (event, ui) {
            self.minPrice = ui.values[0]
            self.maxPrice = ui.values[1]
          },
        })
      }
    },
    selectPriceRange(rangeId) {
      this.selectedPriceRange = rangeId
      const range = this.priceRanges.find((r) => r.id === rangeId)
      console.log('Khoảng giá đã chọn:', range)
    },
    selectSize(size) {
      this.selectedSize = size
    },
    selectColor(color) {
      this.selectedColor = color
    },
  },
}
</script>

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
}

/* Categories Styling */
.sidebar__categories {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.section-title h4 {
  color: #111111;
  font-weight: 600;
  margin-bottom: 20px;
  position: relative;
  padding-bottom: 10px;
}

.section-title h4:after {
  content: '';
  position: absolute;
  left: 0;
  bottom: 0;
  width: 50px;
  height: 2px;
  background: #e7ab3c;
}

.categories__accordion .card {
  border: none;
  margin-bottom: 5px;
}

.categories__accordion .card-heading {
  background: #f5f5f5;
  padding: 12px 20px;
  border-radius: 4px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.categories__accordion .card-heading:hover {
  background: #e7ab3c;
}

.categories__accordion .card-heading a {
  color: #111111;
  font-weight: 500;
  display: block;
  text-decoration: none;
  transition: all 0.3s ease;
}

.categories__accordion .card-heading:hover a {
  color: #ffffff;
}

.categories__accordion .card-body {
  padding: 15px 20px;
}

.categories__accordion .card-body ul li {
  list-style: none;
  margin-bottom: 8px;
}

.categories__accordion .card-body ul li a {
  color: #666666;
  font-size: 14px;
  text-decoration: none;
  transition: all 0.3s ease;
  display: block;
  padding: 5px 0;
}

.categories__accordion .card-body ul li a:hover {
  color: #e7ab3c;
  padding-left: 5px;
}

.categories__accordion .card-body ul li a:hover {
  color: #e7ab3c;
  padding-left: 5px;
}

.categories__accordion .collapse {
  transition: all 0.3s ease;
}

.categories__accordion .collapse.show {
  display: block;
}

/* Shop by Price Styling */
.sidebar__filter {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  margin-top: 30px;
}

.price-buttons {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 20px 0;
}

.price-btn {
  padding: 10px 15px;
  border: 1px solid #e1e1e1;
  background: #ffffff;
  color: #111111;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: left;
  font-size: 14px;
}

.price-btn:hover {
  background: #f5f5f5;
  border-color: #e7ab3c;
}

.price-btn.active {
  background: #e7ab3c;
  color: #ffffff;
  border-color: #e7ab3c;
}

.categories__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 300px;
  position: relative;
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

.instagram__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 200px;
  position: relative;
}

/* Price Range Styling */
.ui-slider {
  position: relative;
  text-align: left;
  background: #e1e1e1;
  border: none;
  border-radius: 2px;
  height: 4px;
}

.ui-slider .ui-slider-handle {
  position: absolute;
  z-index: 2;
  width: 16px;
  height: 16px;
  background: #e7ab3c;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  top: -6px;
  margin-left: -8px;
}

.ui-slider .ui-slider-range {
  position: absolute;
  z-index: 1;
  display: block;
  border: 0;
  background: #e7ab3c;
  height: 100%;
}

.ui-slider-horizontal .ui-slider-range {
  top: 0;
  height: 100%;
}
.categories__accordion .card-heading a:after,
.categories__accordion .card-heading > a.active[aria-expanded='false']:after {
  content: '\f107';
  font-size: 14px;
  font-family: 'FontAwesome';
  color: #666666;
  position: absolute;
  right: 30px;
  top: 10px;
}

.categories__accordion .card-heading.active a:after {
  content: '\f106';
  font-size: 14px;
  font-family: 'FontAwesome';
  color: #666666;
  position: absolute;
  right: 30px;
  top: -1px;
}

.categories__accordion .card-heading a[aria-expanded='true']:after,
.categories__accordion .card-heading > a.active:after {
  content: '\f106';
  font-size: 14px;
  font-family: 'FontAwesome';
  color: #666666;
  position: absolute;
  right: 30px;
  top: -1px;
}

.size-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin: 20px 0;
}
.size-btn {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: 1.5px solid #e1e1e1;
  background: #fff;
  color: #111;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.size-btn:hover {
  border-color: #e7ab3c;
  background: #f5f5f5;
}
.size-btn.active {
  background: #e7ab3c;
  color: #fff;
  border-color: #e7ab3c;
}

.color-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin: 20px 0;
}
.color-btn {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid #e1e1e1;
  cursor: pointer;
  transition: border 0.2s;
  position: relative;
  outline: none;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  display: flex;
  align-items: center;
  justify-content: center;
}
.color-btn.active {
  border: 2.5px solid #e7ab3c;
}
.color-btn .color-check {
  color: #111;
  font-size: 16px;
  font-weight: bold;
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  pointer-events: none;
}
.color-btn[style*='#fff'] .color-check {
  color: #222;
}
.color-btn:hover {
  border: 2px solid #e7ab3c;
}

.sidebar__box {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  margin-top: 30px;
}
</style>