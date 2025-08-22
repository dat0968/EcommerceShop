<template>
  <div class="container-fluid review-order-container py-3">
    <h4 class="mb-3">Đánh giá đơn hàng</h4>

    <!-- Loading State -->
    <div v-if="loading" class="d-flex justify-content-center align-items-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <span class="ms-3">Đang tải chi tiết đơn hàng...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <!-- Main Content -->
    <div v-else-if="orderDetail">
      <!-- Order Information -->
      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Thông tin hóa đơn</h5>
        </div>
        <div class="card-body">
          <ul class="list-group list-group-flush">
            <li class="list-group-item d-flex justify-content-between">
              <strong>Mã hóa đơn:</strong>
              <span>{{ orderDetail.maHd }}</span>
            </li>
            <li class="list-group-item d-flex justify-content-between">
              <strong>Ngày tạo:</strong>
              <span>{{ formatDate(orderDetail.ngayTao, 'L LT') }}</span>
            </li>
            <li class="list-group-item d-flex justify-content-between">
              <strong>Khách hàng:</strong>
              <span>{{ orderDetail.hoTen }}</span>
            </li>
            <li class="list-group-item d-flex justify-content-between">
              <strong>Địa chỉ nhận hàng:</strong>
              <span class="text-end">{{ orderDetail.diaChiNhanHang }}</span>
            </li>
            <li class="list-group-item d-flex justify-content-between">
              <strong>Tình trạng:</strong>
              <span class="badge bg-info text-dark">{{ orderDetail.tinhTrang }}</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Reviewable Items -->
      <div v-for="item in orderItems" :key="item.uniqueId" class="card mb-3 review-item-card">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start mb-3">
            <div>
              <span class="badge me-2" :class="item.isProduct ? 'bg-primary' : 'bg-success'">
                {{ item.isProduct ? 'Sản phẩm' : 'Combo' }}
              </span>
              <strong class="me-2">{{ item.name }}</strong>
            </div>
            <span class="text-muted">Số lượng: {{ item.soLuong }}</span>
          </div>

          <!-- Star Rating -->
          <div class="mb-3">
            <label class="form-label fw-bold">Đánh giá của bạn:</label>
            <div class="d-flex align-items-center">
              <StarRating :rating="item.rating" :read-only="false" @update:rating="item.rating = $event" />
            </div>
          </div>

          <!-- Review Content -->
          <div class="mb-3">
            <label class="form-label fw-bold">Nội dung:</label>
            <textarea
              v-model.trim="item.content"
              class="form-control"
              rows="3"
              placeholder="Chia sẻ cảm nhận của bạn..."
            ></textarea>
          </div>

          <!-- Image Upload -->
          <div class="mb-3">
            <label class="form-label fw-bold">Hình ảnh (tùy chọn):</label>
            <input
              type="file"
              multiple
              accept="image/*"
              class="form-control"
              @change="handleImageSelection($event, item)"
              :disabled="!!item.reviewId"
            />
            <div v-if="item.imagePreviews.length" class="mt-2 d-flex flex-wrap gap-2">
              <img
                v-for="(img, idx) in item.imagePreviews"
                :key="idx"
                :src="img"
                class="rounded border"
                style="width: 80px; height: 80px; object-fit: cover"
              />
            </div>
          </div>

          <!-- Shop Reply -->
          <div v-if="item.shopReply" class="alert alert-secondary mt-3">
            <strong class="d-block mb-1">Phản hồi từ Shop:</strong>
            {{ item.shopReply }}
          </div>

          <!-- Action Buttons -->
          <div class="d-flex justify-content-end gap-2 mt-3">
            <button class="btn btn-sm btn-danger" v-if="item.reviewId" @click="deleteReview(item)">
              <i class="fas fa-trash me-1"></i> Xóa
            </button>
            <button
              class="btn btn-sm btn-primary"
              @click="submitReview(item)"
              :disabled="item.isSubmitting"
            >
              <span
                v-if="item.isSubmitting"
                class="spinner-border spinner-border-sm"
                role="status"
                aria-hidden="true"
              ></span>
              <i v-else class="fas fa-save me-1"></i>
              {{ item.reviewId ? 'Cập nhật' : 'Gửi đánh giá' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Initial state / No order selected -->
    <div v-else class="text-center text-muted py-5">
      <p>Vui lòng chọn một đơn hàng để xem và gửi đánh giá.</p>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'
import StarRating from '@/components/common/StarRating.vue';
import { formatDate } from '@/constants/formatDatetime'

export default {
  name: 'ReviewOrder',
  props: {
    orderId: {
      type: Number,
      default: null,
    },
  },
  components: { StarRating },
  data() {
    return {
      orderDetail: null,
      loading: false,
      error: null,
    };
  },
  computed: {
    orderItems() {
      if (!this.orderDetail) return []

      const products = (this.orderDetail.products || []).map((p) => ({
        ...p,
        uniqueId: `prod-${p.id}`,
        isProduct: true,
        name: `SP: ${p.maSp}`,
        rating: p.soSao || 5,
        content: p.noiDung || '',
        reviewId: p.maDanhGia,
        shopReply: p.shopPhanHoi,
        imageFiles: [],
        imagePreviews: (p.hinhAnhs
          ? Array.isArray(p.hinhAnhs)
            ? p.hinhAnhs
            : p.hinhAnhs.split(',')
          : []
        ).map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img)),
        isSubmitting: false,
      }))

      const combos = (this.orderDetail.combos || []).map((c) => ({
        ...c,
        uniqueId: `combo-${c.id}`,
        isProduct: false,
        name: `Combo: ${c.maCombo}`,
        rating: c.soSao || 5,
        content: c.noiDung || '',
        reviewId: c.maDanhGia,
        shopReply: c.shopPhanHoi,
        imageFiles: [],
        imagePreviews: (c.hinhAnhs
          ? Array.isArray(c.hinhAnhs)
            ? c.hinhAnhs
            : c.hinhAnhs.split(',')
          : []
        ).map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img)),
        isSubmitting: false,
      }))

      return [...products, ...combos]
    },
  },
  watch: {
    orderId: {
      handler(newId) {
        if (this.isValidId(newId)) {
          this.getOrderDetail()
        }
      },
      immediate: true,
    },
  },
  methods: {
    formatDate,
    isValidId(id) {
      return id !== null && id !== undefined && id > 0 && !isNaN(id)
    },
    async getOrderDetail() {
      if (!this.isValidId(this.orderId)) {
        this.orderDetail = null
        return
      }

      this.loading = true
      this.error = null
      this.orderDetail = null

      try {
        const res = await axiosConfig.getFromApi(
          `/Review/orders/${this.orderId}`,
          ConfigsRequest.takeAuth(),
        )
        if (res.success) {
          this.orderDetail = res.data
        } else {
          this.error = res.message || 'Không thể tải thông tin đơn hàng.'
        }
      } catch (e) {
        console.error(e)
        this.error = 'Đã xảy ra lỗi khi tải chi tiết đơn hàng.'
      } finally {
        this.loading = false
      }
    },
    handleImageSelection(event, item) {
      item.imageFiles = Array.from(event.target.files)
      item.imagePreviews = item.imageFiles.map((file) => URL.createObjectURL(file))
    },
    async submitReview(item) {
      item.isSubmitting = true
      try {
        const isUpdate = !!item.reviewId
        const url = `/Review?isProduct=${item.isProduct}`
        let res

        if (isUpdate) {
          const body = {
            id: item.reviewId,
            noiDung: item.content,
            soSao: item.rating,
            maSp: item.maSp,
            maCtsp: item.maCtsp,
            maCombo: item.maCombo,
            maCtHd: item.id,
          }
          res = await axiosConfig.putToApi(url, body, ConfigsRequest.takeAuth())
        } else {
          const formData = new FormData()
          formData.append('noiDung', item.content)
          formData.append('soSao', item.rating)
          formData.append('maCtHd', item.id)
          if (item.isProduct) {
            formData.append('maSp', item.maSp)
            formData.append('maCtsp', item.maCtsp)
          } else {
            formData.append('maCombo', item.maCombo)
          }
          item.imageFiles.forEach((file) => formData.append('hinhAnhs', file))

          res = await axiosConfig.postToApi(url, formData, ConfigsRequest.takeAuth(true))
        }

        if (!ResponseAPI.handleNotificationAndIsFailResponse(res, true)) {
          await this.getOrderDetail() // Refresh data on success
        }
      } catch (e) {
        alert(`Lỗi: ${e.message}`)
      } finally {
        item.isSubmitting = false
      }
    },
    async deleteReview(item) {
      if (!confirm('Bạn có chắc chắn muốn xóa đánh giá này không?')) return

      item.isSubmitting = true
      try {
        const url = `/Review/${item.reviewId}?isProduct=${item.isProduct}`
        const res = await axiosConfig.deleteFromApi(url, ConfigsRequest.takeAuth())

        if (!ResponseAPI.handleNotificationAndIsFailResponse(res)) {
          alert('Đã xóa đánh giá!')
          await this.getOrderDetail() // Refresh data
        }
      } catch (e) {
        alert(`Lỗi: ${e.message}`)
      } finally {
        item.isSubmitting = false
      }
    },
  },
  mounted() {
    if (this.isValidId(this.orderId)) {
      this.getOrderDetail()
    }
  },
}
</script>

<style scoped>
.review-order-container {
  background-color: #f8f9fa;
}
.review-item-card {
  transition: box-shadow 0.2s ease-in-out;
}
.review-item-card:hover {
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}
.gap-2 {
  gap: 0.5rem;
}
</style>
