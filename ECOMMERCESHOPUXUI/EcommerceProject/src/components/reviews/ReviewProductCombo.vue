<template>
  <div>
    <!-- Thanh lọc và tìm kiếm -->
    <div class="row mb-3">
      <div class="col-md-3 mb-2">
        <select v-model="filterStar" class="form-control">
          <option value="">Tất cả sao</option>
          <option v-for="n in 5" :key="n" :value="n">{{ n }} sao</option>
        </select>
      </div>
      <div class="col-md-3 mb-2">
        <select v-model="filterHasImage" class="form-control">
          <option value="">Có/không ảnh</option>
          <option value="1">Có ảnh</option>
          <option value="0">Không ảnh</option>
        </select>
      </div>
      <div class="col-md-6 mb-2">
        <input
          v-model="searchText"
          class="form-control"
          placeholder="Tìm theo nội dung, tên khách, phản hồi..."
        />
      </div>
    </div>

    <div v-if="loading" class="text-muted">Đang tải...</div>
    <div v-else>
      <div v-if="filteredReviews.length">
        <ul class="list-group">
          <li
            v-for="review in filteredReviews"
            :key="review.id"
            class="list-group-item"
            style="margin-bottom: 12px"
          >
            <!-- ...phần hiển thị đánh giá giữ nguyên... -->
            <div class="d-flex align-items-center mb-2">
              <img
                v-if="review.avatar"
                :src="pathReplaceImg(undefined, '', review.avatar)"
                alt="avatar"
                style="
                  width: 40px;
                  height: 40px;
                  border-radius: 50%;
                  object-fit: cover;
                  margin-right: 10px;
                "
              />
              <div>
                <strong>{{ review.tenKhachHang || 'Ẩn danh' }}</strong>
                <span class="text-muted ms-2" style="font-size: 13px">
                  {{ formatDate(review.ngayDanhGia) }}
                </span>
              </div>
            </div>
            <div class="mb-1">
              <span>
                <span v-for="n in review.soSao" :key="n" style="color: #ffc107">★</span>
                <span v-for="n in 5 - review.soSao" :key="'empty' + n" style="color: #e4e5e9"
                  >★</span
                >
              </span>
            </div>
            <div class="mb-2">
              <strong>Nội dung:</strong>
              <span>{{ review.noiDung }}</span>
            </div>
            <div v-if="review.hinhAnhs && review.hinhAnhs.length" class="d-flex flex-wrap mb-2">
              <img
                v-for="(img, idx) in Array.isArray(review.hinhAnhs)
                  ? review.hinhAnhs
                  : review.hinhAnhs.split(',')"
                :key="idx"
                :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
                style="
                  max-width: 80px;
                  max-height: 80px;
                  margin-right: 8px;
                  margin-bottom: 8px;
                  border: 1px solid #ccc;
                  object-fit: cover;
                "
                alt="Hình đánh giá"
              />
            </div>
            <blockquote
              v-if="review.shopPhanHoi"
              class="col-12"
              style="border-left: 2px solid #ccc; padding-left: 10px; margin: 10px 0"
            >
              <strong>Phản hồi của shop:</strong>
              {{ review.shopPhanHoi ? review.shopPhanHoi : 'Chưa có phản hồi' }}
            </blockquote>
          </li>
        </ul>
      </div>
      <div v-else-if="errorMessage" class="">
        {{ errorMessage }}
      </div>
      <div v-else class="text-muted">Không tìm thấy đánh giá phù hợp.</div>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import pathReplaceImg from '@/utils/processPathImg'
import { formatDate } from '@/constants/formatDatetime'
import ResponseAPI from '@/models/ResponseAPI'

export default {
  name: 'ReviewProductCombo',
  props: {
    objectId: Number,
    isProduct: Boolean,
  },
  data() {
    return {
      objectIdLocal: this.objectId || null,
      reviews: [],
      loading: false,
      pathReplaceImg,
      formatDate,
      errorMessage: null,
      filterStar: '',
      filterHasImage: '',
      searchText: '',
    }
  },
  watch: {
    objectId(val) {
      this.objectIdLocal = val
      if (this.isValidId(val)) this.fetchReviews()
      else this.reviews = []
    },
    isProduct() {
      this.reviews = []
    },
  },
  computed: {
    filteredReviews() {
      return this.reviews.filter((r) => {
        // Lọc theo số sao
        if (this.filterStar && r.soSao != this.filterStar) return false
        // Lọc theo có ảnh/không ảnh
        const hasImg = r.hinhAnhs && r.hinhAnhs.length > 0
        if (this.filterHasImage === '1' && !hasImg) return false
        if (this.filterHasImage === '0' && hasImg) return false
        // Lọc theo nội dung tìm kiếm
        const text = this.searchText.trim().toLowerCase()
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
  },
  methods: {
    isValidId(id) {
      return id !== null && id !== undefined && id !== 0 && !isNaN(id)
    },
    async fetchReviews() {
      if (!this.isValidId(this.objectIdLocal)) return
      this.loading = true
      this.reviews = []
      try {
        const url = this.isProduct
          ? `/review/products/${this.objectIdLocal}`
          : `/review/combos/${this.objectIdLocal}`
        const res = await axiosConfig.getFromApi(url, ConfigsRequest.getSkipAuthConfig())
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) {
          this.errorMessage = res.data.message
          this.reviews = []
          return
        }
        this.reviews = res?.data || []
      } catch (e) {
        this.reviews = []
        this.errorMessage = 'Hiện tại không thể tải nội dung đánh giá'
        console.warn(e)
      } finally {
        this.loading = false
      }
    },
    onInputId() {
      if (this.isValidId(this.objectIdLocal)) {
        this.fetchReviews()
      } else {
        this.reviews = []
      }
    },
  },
  mounted() {
    if (this.isValidId(this.objectIdLocal)) this.fetchReviews()
  },
}
</script>
