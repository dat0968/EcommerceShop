<template>
  <div v-if="isLoading" class="col-12">
    <h6>Đang tải đánh giá...</h6>
  </div>

  <div v-else class="col-12 mb-3">
    <div><strong>Đánh giá trung bình:</strong> {{ averageRating }}</div>

    <div v-if="!isUserLoggedIn" class="alert alert-warning">
      Vui lòng <router-link to="/login">đăng nhập</router-link> để gửi đánh giá.
    </div>
    <div v-else>
      <div class="alert alert-info">
        <h6>{{ userReview ? 'Cập nhật đánh giá của bạn:' : 'Gửi đánh giá của bạn:' }}</h6>
        <form @submit.prevent="submitReview">
          <div class="form-group">
            <label for="reviewContent">Nội dung đánh giá:</label>
            <textarea
              id="reviewContent"
              v-model="userReview.noiDung"
              class="form-control"
              rows="3"
              placeholder="Viết đánh giá của bạn..."
              :readonly="isSubmitting"
              required
            ></textarea>
          </div>

          <div class="form-group">
            <label for="starRating">Đánh giá (sao):</label>
            <select
              v-model="userReview.soSao"
              class="form-control"
              id="starRating"
              :disabled="isSubmitting"
              required
            >
              <option disabled value="">Chọn số sao</option>
              <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
            </select>
          </div>

          <button type="submit" class="btn btn-primary" :disabled="isSubmitting">
            <i class="fas fa-paper-plane"></i> {{ userReview ? 'Cập nhật' : 'Gửi đánh giá' }}
          </button>
          <button
            v-if="userReview"
            type="button"
            class="btn btn-danger"
            style="margin-left: 10px"
            @click="deleteReview"
            :disabled="isSubmitting"
          >
            <i class="fas fa-trash"></i>
          </button>
        </form>
        <div v-if="userReview" class="mt-2">
          <small class="text-muted">Ngày đánh giá: {{ formatDate(userReview.ngayDanhGia) }}</small>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import * as axiosConfig from '@/utils/axiosClient'
import ConfigsRequest from '@/models/ConfigsRequest'
import ResponseAPI from '@/models/ResponseAPI'
import authService from '@/services/authService'
import toastr from 'toastr'
import { formatDate } from '@/constants/formatDatetime'

export default {
  name: 'ReviewTestSub',
  props: {
    productData: {
      type: Object,
      default: () => ({}),
    },
    reviewProduct: {
      type: Object,
      default: () => ({}),
    },
    isUserLoggedIn: {
      type: Boolean,
      default: null,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      isSubmitting: false, // Biến để kiểm soát trạng thái gửi đánh giá
    }
  },
  computed: {
    userReview() {
      if (this.isUserLoggedIn && this.reviewProduct.data) {
        const userReviewData = this.reviewProduct.data.find(
          (item) => item.idKhachHang == authService.getUserId(),
        ) // Sử dụng ID người dùng thực

        if (userReviewData) {
          return userReviewData
        }
      }
      return () => ({
        id: 0,
        idKhachHang: 0,
        idSanPham: 0,
        hoTen: '',
        email: '',
        noiDung: ' ',
        soSao: 0,
        ngayDanhGia: '2025-06-02T13:43:58.268Z',
      })
    },
    averageRating() {
      const totalStars = this.reviewProduct.data.reduce((sum, item) => sum + item.soSao, 0)
      const numberOfReviews = this.reviewProduct.data.length
      return numberOfReviews ? (totalStars / numberOfReviews).toFixed(1) : 'Chưa có đánh giá'
    },
  },
  methods: {
    formatDate,
    async submitReview() {
      if (!this.isUserLoggedIn) {
        alert('Bạn cần đăng nhập để gửi đánh giá.')
        return
      }

      const reviewData = {
        idSanPham: this.productData.maSp,
        noiDung: this.userReview.noiDung,
        soSao: this.userReview.soSao,
      }

      try {
        let response
        this.isSubmitting = true // Bật trạng thái gửi đánh giá

        // Cập nhật đánh giá
        response = await axiosConfig.postToApi(
          `/Review/${this.productData.maSp}`, // Sử dụng ID của đánh giá để cập nhật
          reviewData,
          ConfigsRequest.takeAuth(),
        )

        if (ResponseAPI.handleNotificationAndIsFailResponse(response)) {
          return
        }

        alert('Đánh giá đã được xử lý thành công!')
        // Cập nhật danh sách đánh giá hoặc lấy lại dữ liệu mới tại đây
        this.userReview = response.data
      } catch (error) {
        toastr.error('Đã xảy ra lỗi. Vui lòng thử lại sau.')
        console.error('Có lỗi xảy ra:', error)
      } finally {
        this.isSubmitting = false // Tắt trạng thái gửi đánh giá
        // Emit event to parent to update review data
        this.$emit('review-submitted', authService.getUserId())
      }
    },
    async deleteReview() {
      if (!this.isUserLoggedIn) {
        alert('Bạn cần đăng nhập để xóa đánh giá.')
        return
      }

      if (confirm('Bạn có chắc chắn muốn xóa đánh giá này không?')) {
        try {
          this.isSubmitting = true // Bật trạng thái gửi đánh giá
          const response = await axiosConfig.deleteFromApi(
            `/Review/${this.productData.maSp}`, // Sử dụng ID của đánh giá để xóa
            ConfigsRequest.takeAuth(),
          )

          if (ResponseAPI.handleNotificationAndIsFailResponse(response)) {
            return
          }
          // Emit event to parent to update review data after deletion
          this.$emit('review-deleted', authService.getUserId())

          alert('Đánh giá đã được xóa thành công!')
          // Cập nhật danh sách đánh giá hoặc lấy lại dữ liệu mới tại đây
          this.userReview = () => ({
            id: 0,
            idKhachHang: 0,
            idSanPham: 0,
            hoTen: '',
            email: '',
            noiDung: ' ',
            soSao: 0,
            ngayDanhGia: '2025-06-02T13:43:58.268Z',
          })
        } catch (error) {
          toastr.error('Đã xảy ra lỗi khi xóa đánh giá. Vui lòng thử lại sau.')
          console.error('Có lỗi xảy ra:', error)
        } finally {
          this.isSubmitting = false // Tắt trạng thái gửi đánh giá
        }
      }
    },
  },
}
</script>

<style scoped>
.form-control {
  margin-bottom: 1rem;
}
.card {
  margin-top: 1rem;
}
.btn-danger {
  margin-left: 10px;
}
</style>
