<script setup>
import { ref, onMounted, nextTick, watch } from 'vue'
import $ from 'jquery'
import 'jquery-ui-dist/jquery-ui'
import { RouterLink } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { formatCurrency } from '@/constants/formatCurrency';

const activeCategory = ref('collapseOne')
const selectedPriceRange = ref(null)
const listCategories = ref([])
const getUrlAPI = ref(GetApiUrl())
const products = ref([])
const search = ref('')
const searchInput = ref('') // New search input for the search bar
const toTalPages = ref(1)
const pageSelected = ref(1)
const categoryBigSelected = ref('')
const categorySmallSelected = ref('')
const sortByPrice = ref('')
const filterPrice = ref('')
const isSearching = ref(false) // Loading state for search
const isCombo = ref(false)
const fetchBigCategories = async () => {
  const fetchAPI = await fetch(`${getUrlAPI.value}/api/Categories/GetCategoriesforShop`, {
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
    isSearching.value = true
    const response = await fetch(
      `${getUrlAPI.value}/api/Shop?search=${search.value}&selectedBigCategory=${categoryBigSelected.value}&selectedSmallCategory=${categorySmallSelected.value}&Category&sortByPrice=${sortByPrice.value}&filterPrice=${filterPrice.value}&isCombo=${isCombo.value}&page=${pageSelected.value}`,
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
    console.log(result)
  } catch (error) {
    console.error('Lỗi fetchAPIProducts:', error)
  } finally {
    isSearching.value = false
  }
}

// New search functionality
const handleSearch = () => {
  isCombo.value = false;
  search.value = searchInput.value.trim()
  pageSelected.value = 1 // Reset to first page when searching
  fetchAPIProducts()
}

const clearSearch = () => {
  isCombo.value = false;
  searchInput.value = ''
  search.value = ''
  pageSelected.value = 1
  fetchAPIProducts()
}

// Auto search with debounce
let searchTimeout = null
const handleSearchInput = () => {
  clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    if (searchInput.value.trim() !== search.value) {
      handleSearch()
    }
  }, 500) // 500ms debounce
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
    isCombo.value = false;
    categoryBigSelected.value = maDanhMucCha
    categorySmallSelected.value = maDanhMucCon
    pageSelected.value = 1 // Reset to first page when filtering
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
  isCombo.value = false;
  selectedPriceRange.value = rangeId
  const range = priceRanges.find((r) => r.id === rangeId)
  filterPrice.value = range.label
  pageSelected.value = 1 // Reset to first page when filtering
  fetchAPIProducts()
}

// New sorting functionality
const sortOptions = [
  { value: '', label: 'Sắp xếp mặc định' },
  { value: 'asc', label: 'Giá: Thấp đến cao' },
  { value: 'desc', label: 'Giá: Cao đến thấp' },
]

const handleSortChange = (event) => {
  sortByPrice.value = event.target.value
  pageSelected.value = 1 // Reset to first page when sorting
  fetchAPIProducts()
}
function selectedCombo(){
  isCombo.value = true;
  fetchAPIProducts()
}
onMounted(async () => {
  await fetchBigCategories()
  await fetchAPIProducts()
})

const formatRating = (rating) => {
  if (rating === null || rating === undefined) {
    return '5.0'; // Default to 5 if no rating
  }
  // Round to nearest 0.5
  const rounded = Math.round(rating * 2) / 2;
  return rounded.toFixed(1);
};
</script>

<template>
  <div>
    <!-- Breadcrumb Begin -->
    <div class="breadcrumb-option" style="margin-left: -200px;">
      <div class="container">
        <div class="row">
          <div class="col-lg-12">
            <div class="breadcrumb__links">
              <RouterLink style="text-decoration-line: none" to="/"><i class="fa fa-home"></i> Trang chủ</RouterLink>
              <span>Sản phẩm</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Breadcrumb End -->

    <!-- Shop Section Begin -->
    <section class="shop spad" style="margin-top: -50px;">
      <div class="" style="margin-left: 80px; margin-right: 80px;">
        <!-- Search Bar Section -->
        <div class="row">
              <div class="col-lg-12" style="margin-left: 60px;">
                <div class="">
                  <div class="search-container">
                    <div class="search-box">
                      <div class="search-input-wrapper">
                        <input v-model="searchInput" @input="handleSearchInput" @keyup.enter="handleSearch" type="text"
                          class="search-input" placeholder="Tìm kiếm sản phẩm..."  style="width: 1030px;" />
                        <button v-if="searchInput" @click="clearSearch" class="clear-search-btn" type="button">
                          <i class="fa fa-times"></i>
                        </button>
                      </div>
                      <!-- <button @click="handleSearch" class="search-btn" type="button">
                    <i class="fa fa-search"></i>
                    <span>Tìm kiếm</span>
                  </button> -->
                    </div>

                    <!-- Search Results Info -->
                    <div v-if="search" class="search-info" style="margin-bottom: 20px;">
                      <span class="search-term">
                        <i class="fa fa-search"></i>
                        Kết quả tìm kiếm cho: "<strong>{{ search }}</strong>"
                      </span>
                      <span class="results-count">
                        {{ products.length }} sản phẩm được tìm thấy
                      </span>
                    </div>
                  </div>

                  <!-- Sorting and Filter Bar -->
                  <!-- <div class="filter-bar">
                <div class="filter-left">
                  <div class="sort-dropdown">
                    <select v-model="sortByPrice" @change="handleSortChange" class="sort-select">
                      <option v-for="option in sortOptions" :key="option.value" :value="option.value">
                        {{ option.label }}
                      </option>
                    </select>
                  </div>
                </div>
                <div class="filter-right">
                  <div class="view-options">
                    <span class="view-text">Hiển thị:</span>
                    <button class="view-btn active">
                      <i class="fa fa-th-large"></i>
                    </button>
                    <button class="view-btn">
                      <i class="fa fa-list"></i>
                    </button>
                  </div>
                </div>
              </div> -->
                </div>
              </div>
            </div>

        <div class="row">
          <div class="col-lg-3 col-md-3">
            <div class="shop__sidebar">
              <div class="sidebar__categories">
                <div style="
                    border-bottom: 2px solid #e7ab3c;
                    display: inline-block;
                    padding-bottom: 5px;
                    margin-bottom: 20px;
                    text-align: center;
                  ">
                  <h4 style="display: inline-block; margin: 0; font-weight: 600">Loại sản phẩm</h4>
                </div>
                <div class="categories__accordion">
                  <div class="accordion" id="accordionExample">
                    <div class="card-heading" style="margin-bottom: 10px;">
                      <a href="/shop">Tất cả</a>
                    </div>
                    <div class="card" v-for="category in listCategories" :key="category.maDanhMucCha">
                      <div class="card-heading" @click="toggleCategory(category.maDanhMucCha)">
                        <a href="javascript:void(0)">{{ category.tenDanhMucCha }}</a>
                      </div>

                      <div :id="category.maDanhMucCha" class="collapse"
                        :class="{ show: activeCategory === category.maDanhMucCha }">
                        <div class="card-body">
                          <ul v-for="smallcategory in category.chitietdanhmucs" :key="smallcategory.maDanhMucCon">
                            <li @click="
                              selectedCategory(smallcategory.maDanhMucCon, category.maDanhMucCha)
                              ">
                              <a href="#">{{ smallcategory.tenDanhMucCon }}</a>
                            </li>

                          </ul>
                        </div>
                      </div>
                    </div>
                    <div class="card-heading" style="margin-bottom: 10px">
                      <li @click="selectedCombo()" style="list-style: none;">Combo</li>
                    </div>
                  </div>
                </div>
              </div>
              <div class="sidebar__filter">
                <div style="
                    border-bottom: 2px solid #e7ab3c;
                    display: inline-block;
                    padding-bottom: 5px;
                    margin-bottom: 20px;
                    text-align: center;
                  ">
                  <h4 style="display: inline-block; margin: 0; font-weight: 600">Khoảng giá</h4>
                </div>
                <div class="price-buttons">
                  <button v-for="range in priceRanges" :key="range.id"
                    :class="['price-btn', { active: selectedPriceRange === range.id }]"
                    @click="selectPriceRange(range.id, range.label)">
                    {{ range.label }}
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-9 col-md-9">

            <!-- Loading State -->
            <div v-if="isSearching" class="loading-container">
              <div class="loading-spinner">
                <i class="fa fa-spinner fa-spin"></i>
                <span>Đang tìm kiếm...</span>
              </div>
            </div>

            <div v-else class="row">
              <div class="col-lg-3 col-md-4 col-sm-6 mix" v-for="product in products" :key="product.id">
                <div class="product__item" style="background-color: #ffffff; border-radius: 12px">
                  <div class="product__item__pic">
                    <img :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${product.image}`"
                      alt="Hình ảnh sản phẩm" v-if="product.image != undefined" />
                    <span v-else class="text-muted"> Không có ảnh </span>
                    <ul class="product__hover">
                      <li>
                        <a href="@/assets/Customer/img/product/product-2.jpg" class="image-popup"><span
                            class="arrow_expand"></span></a>
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
                        style="text-decoration-line: none">
                        {{ product.name }}
                        <div class="product__price text-muted fw-semibold fs-7 text-danger">
                          {{
                            product.type.toLowerCase() == 'product'
                              ? (product.priceRange)
                              : product.discountPercentage != undefined &&
                                product.discountPercentage > 0
                                ? 'Giảm ' + product.discountPercentage + '%'
                                : 'Giảm ' + product.discountAmount + 'VNĐ'
                          }}
                        </div>
                      </RouterLink>
                    </h6>
                    <div class="d-flex justify-content-between align-items-center mt-2">
                        <div class="product__rating">
                            <i class="fa fa-star" style="color: #ffc107;"></i>
                            <span>{{ formatRating(product.averageRating) }}</span>
                            <span class="text-muted">({{ product.reviewCount }})</span>
                        </div>
                    </div>
                  </div>
                </div>
              </div>
              <div v-if="products.length === 0 && !isSearching">
                <div class="no-products">
                  <div class="no-products-icon">
                    <i class="fa fa-search"></i>
                  </div>
                  <h3>Không tìm thấy sản phẩm</h3>
                  <p v-if="search">
                    Không có sản phẩm nào phù hợp với từ khóa "<strong>{{ search }}</strong
                    >"
                  </p>
                  <p v-else>Không có sản phẩm nào trong danh mục này</p>
                  <button @click="clearSearch" v-if="search" class="btn-clear-search">
                    Xóa bộ lọc
                  </button>
                </div>
              </div>
              <div class="col-lg-12 text-center" v-if="products.length > 0">
                <div class="pagination__option">
                  <a @click="ChangePage(1)" href="#" :class="{ disabled: pageSelected === 1 }">
                    <i class="fa fa-angle-left"></i>
                  </a>
                  <a @click="ChangePage(page)" v-for="page in toTalPages" :key="page" href="#"
                    :class="{ active: pageSelected === page }">
                    {{ page }}
                  </a>
                  <a
                    @click="ChangePage(toTalPages)"
                    href="#"
                    :class="{ disabled: pageSelected === toTalPages }"
                  >
                    <i class="fa fa-angle-right"></i>
                  </a>
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
/* Original styles remain unchanged */
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

/* NEW SEARCH BAR STYLES */
.search-section {
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  border-radius: 16px;
  padding: 30px;
  margin-bottom: 40px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  border: 1px solid #e2e8f0;
}

.search-container {
  max-width: 800px;
  margin: 0 auto;
}

.search-box {
  display: flex;
  gap: 12px;
  margin-bottom: 20px;
  align-items: center;
}

.search-input-wrapper {
  flex: 1;
  position: relative;
}

.search-input {
  width: 100%;
  padding: 16px 20px;
  padding-right: 50px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  font-size: 16px;
  background: white;
  transition: all 0.3s ease;
  outline: none;
}

.search-input:focus {
  border-color: #e7ab3c;
  box-shadow: 0 0 0 3px rgba(231, 171, 60, 0.1);
  transform: translateY(-1px);
}

.clear-search-btn {
  position: absolute;
  right: 15px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #64748b;
  font-size: 16px;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.clear-search-btn:hover {
  background: #f1f5f9;
  color: #e53e3e;
}

.search-btn {
  background: linear-gradient(135deg, #e7ab3c 0%, #d4941a 100%);
  color: white;
  border: none;
  padding: 16px 24px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
}

.search-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(231, 171, 60, 0.4);
}

.search-btn:active {
  transform: translateY(0);
}

.search-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 20px;
  background: white;
  border-radius: 8px;
  border-left: 4px solid #e7ab3c;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.search-term {
  color: #2d3748;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 8px;
}

.search-term i {
  color: #e7ab3c;
}

.results-count {
  color: #64748b;
  font-size: 14px;
  background: #f8fafc;
  padding: 4px 12px;
  border-radius: 6px;
}

/* Filter Bar */
.filter-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #e2e8f0;
}

.filter-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.sort-select {
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  color: #2d3748;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 180px;
}

.sort-select:focus {
  outline: none;
  border-color: #e7ab3c;
  box-shadow: 0 0 0 3px rgba(231, 171, 60, 0.1);
}

.filter-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.view-options {
  display: flex;
  align-items: center;
  gap: 8px;
}

.view-text {
  color: #64748b;
  font-size: 14px;
  font-weight: 500;
}

.view-btn {
  width: 36px;
  height: 36px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #64748b;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.view-btn:hover {
  border-color: #e7ab3c;
  color: #e7ab3c;
}

.view-btn.active {
  background: #e7ab3c;
  color: white;
  border-color: #e7ab3c;
}

/* Loading State */
.loading-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 300px;
}

.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  color: #64748b;
}

.loading-spinner i {
  font-size: 2rem;
  color: #e7ab3c;
}

.loading-spinner span {
  font-size: 16px;
  font-weight: 500;
}

/* No Products State */
.no-products {
  text-align: center;
  padding: 60px 20px;
  color: #64748b;
  background: white;
  border-radius: 12px;
  margin: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.no-products-icon {
  font-size: 4rem;
  color: #cbd5e0;
  margin-bottom: 20px;
}

.no-products h3 {
  color: #2d3748;
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 12px;
}

.no-products p {
  font-size: 1rem;
  line-height: 1.6;
  margin-bottom: 24px;
}

.btn-clear-search {
  background: #e7ab3c;
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-clear-search:hover {
  background: #d4941a;
  transform: translateY(-2px);
}

/* Enhanced Pagination */
.pagination__option a {
  transition: all 0.3s ease;
}

.pagination__option a.active {
  background: #e7ab3c;
  color: white;
  border-color: #e7ab3c;
}

.pagination__option a.disabled {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}

/* Responsive Design */
@media (max-width: 768px) {
  .search-section {
    padding: 20px;
    margin-bottom: 30px;
  }

  .search-box {
    flex-direction: column;
    gap: 12px;
  }

  .search-btn {
    width: 100%;
    justify-content: center;
  }

  .search-info {
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }

  .filter-bar {
    flex-direction: column;
    gap: 16px;
    align-items: stretch;
  }

  .filter-left,
  .filter-right {
    justify-content: center;
  }

  .sort-select {
    min-width: auto;
    width: 100%;
  }
}

@media (max-width: 480px) {
  .search-input {
    padding: 14px 16px;
    font-size: 14px;
  }

  .search-btn {
    padding: 14px 20px;
    font-size: 14px;
  }

  .search-btn span {
    display: none;
  }

  .view-options {
    display: none;
  }
}

/* Existing styles continue... */
.categories__accordion .card-heading a:after,
.categories__accordion .card-heading>a.active[aria-expanded='false']:after {
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
.categories__accordion .card-heading>a.active:after {
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
</style>
