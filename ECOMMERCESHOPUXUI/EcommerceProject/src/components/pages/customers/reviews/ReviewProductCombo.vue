<template>
  <div class="review-product-combo">
    <!-- Filter and Search Bar -->
    <div class="row mb-3 align-items-center filter-bar">
      <div class="col-md-3 mb-2">
        <div class="total-reviews-display">Tổng số: {{ reviews.length }} đánh giá</div>
      </div>
      <div class="col-md-3 mb-2">
        <select v-model="filterStar" class="form-select">
          <option value="">Tất cả sao</option>
          <option v-for="n in 5" :key="n" :value="n">
            {{ n }} sao {{ '★'.repeat(n) }}{{ '☆'.repeat(5 - n) }}
          </option>
        </select>
      </div>
      <div class="col-md-3 mb-2">
        <select v-model="filterHasImage" class="form-select">
          <option value="">Có/không ảnh</option>
          <option value="1">Có ảnh</option>
          <option value="0">Không ảnh</option>
        </select>
      </div>
      <div class="col-md-3 mb-2">
        <input v-model="searchText" class="form-control" placeholder="Tìm theo tên, nội dung..." />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <p class="mt-2">Đang tải đánh giá...</p>
    </div>

    <!-- Reviews List -->
    <div v-else>
      <div v-if="paginatedReviews.length" class="reviews-list">
        <ul class="list-group list-group-flush">
          <li
            v-for="review in paginatedReviews"
            :key="review.id"
            class="list-group-item review-item"
          >
            <div class="review-header">
              <img
                :src="pathReplaceImg(undefined, '', review.avatar)"
                alt="avatar"
                class="reviewer-avatar"
                @click="
                  openLightbox(
                    [
                      pathReplaceImg(
                        undefined,
                        `HinhAnh/${isProduct ? 'Products' : 'AnhCombo'}`,
                        review.avatar,
                      ),
                    ],
                    0,
                  )
                "
              />
              <div class="reviewer-info">
                <strong class="reviewer-name">{{ review.tenKhachHang || 'Ẩn danh' }}</strong>
                <span class="review-date">{{ formatDate(review.ngayDanhGia) }}</span>
              </div>
            </div>

            <div class="review-rating">
              <StarRating :rating="review.soSao" />
            </div>

            <div class="review-content">
              <p>{{ review.noiDung }}</p>
            </div>

            <div v-if="getReviewImages(review).length" class="review-images">
              <div
                v-for="(img, idx) in getReviewImages(review)"
                :key="idx"
                class="review-image-container"
                @click="openLightbox(getReviewImagesFullPath(review), idx)"
              >
                <img
                  :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
                  class="review-image"
                  alt="Hình đánh giá"
                />
              </div>
            </div>

            <div v-if="review.shopPhanHoi" class="shop-reply">
              <strong>Phản hồi từ Shop:</strong>
              <p>{{ review.shopPhanHoi }}</p>
            </div>

            <div class="review-product-info">
              <img
                v-if="review.tenHinhAnh"
                :src="pathReplaceImg(undefined, 'HinhAnh/Products', review.tenHinhAnh)"
                alt="Ảnh sản phẩm"
                class="product-thumbnail"
              />
              <div class="product-details">
                <span class="product-name">
                  <strong v-if="review.maSp">Sản phẩm:</strong>
                  <strong v-if="review.maCombo">Combo:</strong>
                  {{ review.tenDoiTuong || 'N/A' }}
                </span>
                <span v-if="review.kichThuoc" class="product-variant"
                  >Size: {{ review.kichThuoc }}</span
                >
                <span v-if="review.mauSac" class="product-variant">Màu: {{ review.mauSac }}</span>
                <span v-if="review.donGia" class="product-price"
                  >Giá: {{ formatCurrency(review.donGia) }}</span
                >
              </div>
            </div>
          </li>
        </ul>
        <!-- Pagination -->
        <nav
          v-if="totalPages > 1"
          aria-label="Page navigation"
          class="mt-4 d-flex justify-content-center"
        >
          <ul class="pagination">
            <li class="page-item" :class="{ disabled: currentPage === 1 }">
              <a class="page-link" href="#" @click.prevent="changePage(currentPage - 1)">&laquo;</a>
            </li>
            <li
              v-for="page in totalPages"
              :key="page"
              class="page-item"
              :class="{ active: currentPage === page }"
            >
              <a class="page-link" href="#" @click.prevent="changePage(page)">{{ page }}</a>
            </li>
            <li class="page-item" :class="{ disabled: currentPage === totalPages }">
              <a class="page-link" href="#" @click.prevent="changePage(currentPage + 1)">&raquo;</a>
            </li>
          </ul>
        </nav>
      </div>
      <!-- Empty/Error State -->
      <div v-else-if="errorMessage" class="empty-state">
        <EmptySuggestBox
          :contentText="errorMessage"
          iconSub="fa fa-star"
          linkNav="/review"
          suggestContent="Đánh giá ngay"
        />
      </div>
      <div v-else class="empty-state">
        <p>Không tìm thấy đánh giá nào phù hợp với tiêu chí của bạn.</p>
      </div>
    </div>
  </div>
  <!-- Lightbox -->
  <VueEasyLightbox
    :visible="isLightboxOpen"
    :imgs="lightboxImages"
    :index="lightboxIndex"
    @hide="closeLightbox"
  />
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import pathReplaceImg from '@/utils/processPathImg'
import { formatDate } from '@/constants/formatDatetime'
import ResponseAPI from '@/models/ResponseAPI'
import VueEasyLightbox from 'vue-easy-lightbox'
import EmptySuggestBox from '@/components/common/EmptySuggestBox.vue'
import StarRating from '@/components/common/StarRating.vue';
import { debounce } from 'lodash'
import { formatCurrency } from '@/constants/formatCurrency';

export default {
  name: 'ReviewProductCombo',
  props: {
    objectId: {
      type: [String, Number],
      required: true,
    },
    isProduct: {
      type: Boolean,
      default: true,
    },
  },
  components: { VueEasyLightbox, EmptySuggestBox, StarRating },
  data() {
    return {
      reviews: [],
      loading: false,
      errorMessage: null,
      filterStar: '',
      filterHasImage: '',
      searchText: '',
      debouncedSearchText: '',
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
      currentPage: 1,
      itemsPerPage: 5,
    };
  },
  computed: {
    filteredReviews() {
      return this.reviews.filter((r) => {
        // Filter by star rating
        if (this.filterStar && r.soSao != this.filterStar) return false

        // Filter by presence of images
        const hasImg = r.hinhAnhs && r.hinhAnhs.length > 0
        if (this.filterHasImage === '1' && !hasImg) return false
        if (this.filterHasImage === '0' && hasImg) return false

        // Filter by search text
        const text = this.debouncedSearchText.trim().toLowerCase()
        if (text) {
          const inContent =
            (r.noiDung && r.noiDung.toLowerCase().includes(text)) ||
            (r.tenKhachHang && r.tenKhachHang.toLowerCase().includes(text)) ||
            (r.shopPhanHoi && r.shopPhanHoi.toLowerCase().includes(text))
          if (!inContent) return false
        }
        return true
      })
    },
    totalPages() {
      return Math.ceil(this.filteredReviews.length / this.itemsPerPage)
    },
    paginatedReviews() {
      const start = (this.currentPage - 1) * this.itemsPerPage
      const end = start + this.itemsPerPage
      return this.filteredReviews.slice(start, end)
    },
  },
  watch: {
    objectId(newId) {
      if (this.isValidId(newId)) this.fetchReviews()
      else this.reviews = []
    },
    isProduct() {
      if (this.isValidId(this.objectId)) this.fetchReviews()
      else this.reviews = []
    },
    searchText: debounce(function (newValue) {
      this.debouncedSearchText = newValue
      this.currentPage = 1 // Reset page when search text changes
    }, 300),
  },
  methods: {
    formatCurrency,
    pathReplaceImg,
    formatDate,
    isValidId(id) {
      return id !== null && id !== undefined && id !== 0 && !isNaN(id)
    },
    async fetchReviews() {
      if (!this.isValidId(this.objectId)) return
      this.loading = true
      this.reviews = []
      this.errorMessage = null
      try {
        const url = this.isProduct
          ? `/review/products/${this.objectId}`
          : `/review/combos/${this.objectId}`
        const res = await axiosConfig.getFromApi(url, ConfigsRequest.getSkipAuthConfig())

        if (ResponseAPI.handleNotificationAndIsFailResponse(res, false)) {
          this.errorMessage = res.data?.message || 'Không thể có đánh giá.'
          this.reviews = []
          return
        }
        this.reviews = res?.data || []
      } catch (e) {
        this.reviews = []
        this.errorMessage = 'Đã xảy ra lỗi khi tải danh sách đánh giá.'
        console.error(e)
      } finally {
        this.loading = false
      }
    },
    changePage(page) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page
      }
    },
    openLightbox(imgs, idx = 0) {
      this.lightboxImages = imgs
      this.lightboxIndex = idx
      this.isLightboxOpen = true
    },
    closeLightbox() {
      this.isLightboxOpen = false
    },
    getReviewImages(item) {
      if (!item.hinhAnhs) return []
      return Array.isArray(item.hinhAnhs)
        ? item.hinhAnhs
        : item.hinhAnhs.split(',').filter((img) => img)
    },
    getReviewImagesFullPath(item) {
      return this.getReviewImages(item).map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img))
    },
  },
  mounted() {
    if (this.isValidId(this.objectId)) {
      this.fetchReviews()
    }
  },
}
</script>

<style scoped>
.review-product-combo {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  padding: 1rem;
  background-color: #f9f9f9;
}

.filter-bar {
  background-color: #fff;
  padding: 1rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.total-reviews-display {
  font-weight: 500;
  color: #333;
  display: flex;
  align-items: center;
  height: 100%;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: #6c757d;
}

.reviews-list {
  margin-top: 1rem;
}

.review-item {
  background-color: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  margin-bottom: 1rem;
  padding: 1.5rem;
  transition: box-shadow 0.3s ease;
}

.review-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.review-header {
  display: flex;
  align-items: center;
  margin-bottom: 0.75rem;
}

.reviewer-avatar {
  width: 45px;
  height: 45px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 12px;
  border: 2px solid #eee;
}

.reviewer-info {
  display: flex;
  flex-direction: column;
}

.reviewer-name {
  font-weight: 600;
  color: #212529;
}

.review-date {
  font-size: 0.85em;
  color: #6c757d;
}

.review-rating {
  margin-bottom: 0.75rem;
  font-size: 1.2em;
}

.star-filled {
  color: #ffc107;
}

.star-empty {
  color: #e0e0e0;
}

.review-content {
  margin-bottom: 1rem;
  color: #343a40;
  line-height: 1.6;
}

.review-images {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 1rem;
}

.review-image-container {
  cursor: pointer;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #eee;
}

.review-image {
  width: 80px;
  height: 80px;
  object-fit: cover;
  transition: transform 0.2s ease;
}

.review-image:hover {
  transform: scale(1.05);
}

.shop-reply {
  background-color: #f8f9fa;
  border-left: 4px solid #0d6efd;
  padding: 1rem;
  margin: 1rem 0;
  border-radius: 0 8px 8px 0;
}

.shop-reply p {
  margin-bottom: 0;
}

.review-product-info {
  background-color: #f8f9fa;
  padding: 1rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 15px;
  margin-top: 1rem;
}

.product-thumbnail {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
  border: 1px solid #ddd;
}

.product-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
  font-size: 0.9em;
}

.product-name {
  font-weight: 500;
  color: #333;
}

.product-variant {
  color: #555;
}

.product-price {
  font-weight: bold;
  color: #d9534f;
}

.form-select,
.form-control {
  font-size: 0.95rem;
}

.pagination .page-link {
  color: #0d6efd;
}

.pagination .page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
  color: #fff;
}

.pagination .page-item.disabled .page-link {
  color: #6c757d;
}
</style>
