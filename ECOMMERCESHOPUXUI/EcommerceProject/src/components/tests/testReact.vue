<template>
  <div>
    <!-- Nút mở modal, đặt absolute góc trái -->
    <button
      class="btn btn-primary position-fixed"
      style="bottom: 30px; left: 20px; z-index: 1050"
      @click="showModal = true"
    >
      Mở đánh giá
    </button>

    <!-- Modal overlay -->
    <div v-if="showModal" class="modal-backdrop fade show" style="z-index: 1040"></div>
    <!-- Modal card -->
    <div
      v-if="showModal"
      class="position-fixed top-50 start-50 translate-middle"
      style="z-index: 1051; min-width: 600px; max-width: 90vw; max-height: 90vh; overflow: auto"
    >
      <div class="card shadow">
        <div class="card-header d-flex justify-content-between align-items-center">
          <span>Đánh giá sản phẩm</span>
          <button class="btn-close" @click="showModal = false"></button>
        </div>
        <div class="card-body">
          <!-- Form nhập số -->
          <form @submit.prevent="onSubmit" class="mb-3">
            <div class="input-group">
              <input
                type="number"
                class="form-control"
                v-model.number="inputValue"
                placeholder="Nhập số lượng"
                min="1"
                required
              />
              <button class="btn btn-success" type="submit">Gửi</button>
            </div>
          </form>
          <!-- Nội dung chia 2 cột -->
          <div class="row" style="overflow-x: auto">
            <!-- Cột nội dung chính -->
            <div class="col-md-7 mb-3" style="min-width: 250px">
              <h5>Nội dung chính</h5>
              <div style="max-height: 200px; overflow-y: auto">
                <code>
                  <pre>{{ productData }} </pre>
                </code>
              </div>
            </div>
            <!-- Cột phụ -->
            <div class="col-md-5" style="min-width: 200px">
              <div class="row">
                <!-- Nội dung phụ -->
                <ReviewTestSub
                  :productData="productData"
                  :reviewProduct="reviewProduct"
                  :is-loading="isLoading"
                  :is-user-logged-in="isUserLoggedIn"
                />
                <!-- Đánh giá -->
                <CommentTestSub
                  :reviewProduct="reviewProduct"
                  :commentsProduct="commentsProduct"
                  :is-loading="isLoading"
                  :is-user-logged-in="isUserLoggedIn"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ReviewTestSub from './subTestReact/ReviewTestSub.vue'
import CommentTestSub from './subTestReact/CommentTestSub.vue'
import authService from '@/services/authService'
export default {
  name: 'TestReaction',
  components: { ReviewTestSub, CommentTestSub },
  data() {
    return {
      showModal: false,
      isLoading: true,
      inputValue: 1,
      productData: {},
      reviewProduct: {},
      commentsProduct: {},
      isUserLoggedIn: !authService.isExpiredSessionAccess(),
    }
  },
  computed: {},
  methods: {
    async onSubmit() {
      try {
        this.isLoading = true
        // Xử lý submit ở đây, ví dụ alert hoặc emit
        await this.loadProductData()
        await this.loadReviewProduct()
        await this.loadCommentsProduct()
      } catch (error) {
        console.error('Error during submit:', error)
      } finally {
        this.showModal = true
        this.isLoading = false
      }
    },
    async loadProductData() {
      const res = await axiosConfig
        .getFromApi(`/Products/${this.inputValue}`, ConfigsRequest.getSkipAuthConfig())
        .then((response) => {
          if (response) {
            return response
          }
          return response
        })
        .catch((error) => {
          console.error('Error fetching product data:', error)
          return {}
        })
      if (res && res.data) {
        this.productData = res.data
      } else {
        this.productData = res
      }
    },
    async loadReviewProduct() {
      const res = await axiosConfig.getFromApi(
        `/Review/${this.inputValue}`,
        ConfigsRequest.getSkipAuthConfig(),
      )
      if (res && res.data) {
        this.reviewProduct = res
      } else {
        this.reviewProduct = res
      }
    },
    async loadCommentsProduct() {
      const res = await axiosConfig
        .getFromApi(`/Comment/${this.inputValue}`, ConfigsRequest.getSkipAuthConfig())
        .then((response) => {
          if (response) {
            return response
          }
          return response
        })
        .catch((error) => {
          console.error('Error fetching product data:', error)
          return {}
        })
      if (res && res.data) {
        this.commentsProduct = res.data
      } else {
        this.commentsProduct = res
      }
    },
  },
}
</script>

<style scoped>
/* Đảm bảo modal overlay phủ toàn màn hình */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.3);
}
</style>
