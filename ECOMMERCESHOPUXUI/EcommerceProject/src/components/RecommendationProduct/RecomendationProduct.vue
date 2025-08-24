
<script setup>
import { ref, onMounted, computed } from 'vue'
import $ from 'jquery'
import { decodeToken, validateToken } from '@/utils/auth'
import { GetApiUrl } from '@/constants/api'
import Cookies from 'js-cookie'
import { useRoute } from 'vue-router'
const route = useRoute()
const recommendationProduct = ref([])
const id = route.params.id
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const getUrlAPI = ref(GetApiUrl())
const isFavorited = ref(false)
const isLoading = ref(false)
// Add to favorites function for recommendation products
// const addToFavorites = async (productId) => {
//   try {
//     const validatetoken = await validateToken(accessToken.value, refreshToken.value)
//     if (!validatetoken.isValid) {
//       return;
//     }
//     accessToken.value = validatetoken.newAccessToken
//     const readToken = decodeToken(accessToken.value)
//     const response = await fetch(`${getUrlAPI.value}/api/Favorite/CheckFavoriteProduct`, {
//       method: 'POST',
//       headers: {
//         'Content-Type': 'application/json',
//       },
//       body: JSON.stringify({
//         maSp: productId,
//         maKh: readToken.IdUser,
//       }),
//     })
//     const data = await response.json()
//     isFavorited.value = data.isFavorited
//   } catch (error) {
//     console.error('Lỗi khi kiểm tra sản phẩm yêu thích:', error)
//   }
// }
const fetchRcmProduct = async () => {
  try {
    isLoading.value = true
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
    console.log(error)
  } finally {
    isLoading.value = false
  }
}
const isLogin = computed(() => {
  if (accessToken.value != undefined && accessToken.value != '') {
    return true
  }
  return false
})
// Format price function
const formatPrice = (price) => {
  if (typeof price === 'string') {
    return price
  }
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(price)
}
onMounted(async () => {
  await fetchRcmProduct()
})
</script>
<template>
  <!-- Recommendation Section with Smart Spacing -->
  <div
    v-if="isLogin"
    class="recommendation-section"
    :class="{ 'close-spacing': isShortDescription }"
  >
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
    <div
      v-if="isLoading"
      class="d-flex justify-content-center align-items-center"
      style="height: 300px"
    >
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <!-- Products Grid -->
    <div v-else-if="recommendationProduct.length > 0 && !isLoading" class="products-container">
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
                :src="`${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${
                  item.productDetails[0].images[0].tenHinhAnh
                }`"
                :alt="item.tenSanPham"
                class="product-img"
              />
              <!-- <div class="image-overlay">
                <div class="overlay-content">
                  <button
                    class="action-btn favorite-btn"
                    @click="addToFavorites(item.maSp)"
                    title="Thêm vào yêu thích"
                  >
                    <svg viewBox="0 0 24 24" width="18" height="18">
                      <path
                        fill="currentColor"
                        d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
                      />
                    </svg>
                  </button>
                  <button
                    class="action-btn cart-btn"
                    @click="addToCartRecommendation(item.maSp)"
                    title="Thêm vào giỏ hàng"
                  >
                    <svg viewBox="0 0 24 24" width="18" height="18">
                      <path
                        fill="currentColor"
                        d="M19 7h-3V6a4 4 0 0 0-8 0v1H5a1 1 0 0 0-1 1v11a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3V8a1 1 0 0 0-1-1zM10 6a2 2 0 0 1 4 0v1h-4V6zm8 15a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V9h2v1a1 1 0 0 0 2 0V9h4v1a1 1 0 0 0 2 0V9h2v12z"
                      />
                    </svg>
                  </button>
                </div>
              </div> -->
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
                <span v-for="n in 5" :key="n" class="star" :class="{ filled: n <= 4 }">
                  <svg viewBox="0 0 24 24" width="14" height="14">
                    <path
                      fill="currentColor"
                      d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"
                    />
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
