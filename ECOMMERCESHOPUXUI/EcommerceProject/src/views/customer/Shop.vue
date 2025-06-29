<script setup>
import { ref, onMounted, nextTick } from 'vue'
import $ from 'jquery'
import 'jquery-ui-dist/jquery-ui'
import { RouterLink } from 'vue-router'

const activeCategory = ref('collapseOne')
const selectedPriceRange = ref(null)
const listCategories = ref([])
const getUrlAPI = ref('https://localhost:7217/api')
const products = ref([])
const search = ref('')
const toTalPages = ref(1)
const pageSelected = ref(1)
const categoryBigSelected = ref('')
const categorySmallSelected = ref('')
const sortByPrice = ref('')
const filterPrice = ref('')
const fetchBigCategories = async () => {
  const fetchAPI = await fetch(`${getUrlAPI.value}/Categories/GetCategoriesforShop`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!fetchAPI.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await fetchAPI.json()
  listCategories.value = result.listBigCategory
}
const fetchAPIProducts = async () => {
  try {
    const response = await fetch(
      `${getUrlAPI.value}/Shop?search=${search.value}&selectedBigCategory=${categoryBigSelected.value}&selectedSmallCategory=${categorySmallSelected.value}&Category&sortByPrice=${sortByPrice.value}&filterPrice=${filterPrice.value}&page=${pageSelected.value}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    )

    if (!response.ok) throw new Error('Lỗi khi gọi API')

    const result = await response.json()
    products.value = result.data
    toTalPages.value = result.toTalPages
  } catch (error) {
    console.error('Lỗi fetchAPIProducts:', error)
  }
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page
    fetchAPIProducts()
  }
}

// Lọc danh mục
function selectedCategory(maDanhMucCon, maDanhMucCha) {
  if (maDanhMucCon !== categorySmallSelected.value) {
    categoryBigSelected.value = maDanhMucCha
    categorySmallSelected.value = maDanhMucCon
    fetchAPIProducts()
  }
}

const priceRanges = [
  { id: 0, label: 'Tất cả khoảng giá' },
  { id: 1, label: 'Dưới 300K' },
  { id: 2, label: '300K - 1 triệu' },
  { id: 3, label: '1 triệu - 2 triệu' },
  { id: 4, label: 'Trên 2 triệu' },
]

const toggleCategory = (categoryId) => {
  activeCategory.value = activeCategory.value === categoryId ? null : categoryId
}

const selectPriceRange = (rangeId) => {
  selectedPriceRange.value = rangeId
  const range = priceRanges.find((r) => r.id === rangeId)
  filterPrice.value = range.label
  fetchAPIProducts()
}

onMounted(() => {
  fetchBigCategories()
  fetchAPIProducts()
})
</script>

<template>
  <div>
    <!-- Breadcrumb Begin -->
    <div class="breadcrumb-option">
      <div class="container">
        <div class="row">
          <div class="col-lg-12">
            <div class="breadcrumb__links">
              <RouterLink style="text-decoration-line: none" to="/"
                ><i class="fa fa-home"></i> Trang chủ</RouterLink
              >
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
                <div
                  style="
                    border-bottom: 2px solid #e7ab3c;
                    display: inline-block;
                    padding-bottom: 5px;
                    margin-bottom: 20px;
                    text-align: center;
                  "
                >
                  <h4 style="display: inline-block; margin: 0; font-weight: 600">Loại sản phẩm</h4>
                </div>
                <div class="categories__accordion">
                  <div class="accordion" id="accordionExample">
                    <div
                      class="card"
                      v-for="category in listCategories"
                      :key="category.maDanhMucCha"
                    >
                      <div class="card-heading" @click="toggleCategory(category.maDanhMucCha)">
                        <a href="javascript:void(0)">{{ category.tenDanhMucCha }}</a>
                      </div>
                      <div
                        :id="category.maDanhMucCha"
                        class="collapse"
                        :class="{ show: activeCategory === category.maDanhMucCha }"
                      >
                        <div class="card-body">
                          <ul
                            v-for="smallcategory in category.chitietdanhmucs"
                            :key="smallcategory.maDanhMucCon"
                          >
                            <li
                              @click="
                                selectedCategory(smallcategory.maDanhMucCon, category.maDanhMucCha)
                              "
                            >
                              <a href="#">{{ smallcategory.tenDanhMucCon }}</a>
                            </li>
                          </ul>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div class="sidebar__filter">
                <div
                  style="
                    border-bottom: 2px solid #e7ab3c;
                    display: inline-block;
                    padding-bottom: 5px;
                    margin-bottom: 20px;
                    text-align: center;
                  "
                >
                  <h4 style="display: inline-block; margin: 0; font-weight: 600">Khoảng giá</h4>
                </div>
                <div class="price-buttons">
                  <button
                    v-for="range in priceRanges"
                    :key="range.id"
                    :class="['price-btn', { active: selectedPriceRange === range.id }]"
                    @click="selectPriceRange(range.id, range.label)"
                  >
                    {{ range.label }}
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-9 col-md-9">
            <div class="row">
              <div
                class="col-lg-3 col-md-4 col-sm-6 mix"
                v-for="product in products"
                :key="product.id"
              >
                <div class="product__item">
                  <div class="product__item__pic">
                    <img
                      :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${product.image}`"
                      alt="Hình ảnh sản phẩm"
                      v-if="product.image != undefined"
                    />
                    <span v-else class="text-muted"> Không có ảnh </span>
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
                    <h6>
                      <RouterLink
                        :to="product.type.toLowerCase() === 'product' ? `/product/${product.id}` : `/combo/${product.id}`"
                        style="text-decoration-line: none"
                      >
                        {{ product.name }}
                        <div class="product__price text-muted fw-semibold fs-7 text-danger">
                          {{
                            product.type.toLowerCase() == 'product'
                              ? product.priceRange
                              : product.discountPercentage != undefined &&
                                product.discountPercentage > 0
                              ? 'Giảm ' + product.discountPercentage + '%'
                              : 'Giảm ' + product.discountAmount + 'VNĐ'
                          }}
                        </div>
                      </RouterLink>
                    </h6>
                    <!-- <div class="rating">
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                      <i class="fa fa-star"></i>
                    </div> -->
                  </div>
                </div>
              </div>
              <div v-if="products.length === 0">
                <p style="text-align: center">Không có sản phẩm</p>
              </div>
              <div class="col-lg-12 text-center">
                <div class="pagination__option">
                  <a @click="ChangePage(1)" href="#"><i class="fa fa-angle-left"></i></a>
                  <a @click="ChangePage(page)" v-for="page in toTalPages" :key="page" href="#">{{
                    page
                  }}</a>
                  <a @click="ChangePage(toTalPages)" href="#"><i class="fa fa-angle-right"></i></a>
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