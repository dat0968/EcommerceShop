<template>
  <div v-if="isLoading" class="col-12">
    <h6>Đang tải đánh giá...</h6>
  </div>

  <div v-else class="col-12 mb-3">
    <h6>Nội dung đánh giá</h6>
    <div v-if="!isUserLoggedIn" class="alert alert-warning">
      Vui lòng <router-link to="/login">đăng nhập</router-link> để xem đánh giá của bạn.
    </div>
    <div v-else style="max-height: 80px; overflow-y: auto">
      <code class="language-js line-numbers" data-prismjs-copy="Copy">
        <pre>
          {{ userReview }}
        </pre>
      </code>
    </div>
    <div><strong>Đánh giá trung bình:</strong> {{ averageRating }}</div>
  </div>
</template>

<script>
export default {
  name: 'ReviewTestSub',
  props: {
    reviewProduct: {
      type: Object,
      default: () => ({}),
    },
    currentUserId: {
      type: Number,
      default: null,
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
  },
  data() {
    return {
      isUserLoggedIn: this.currentUserId !== null,
    }
  },
  watch: {
    currentUserId: {
      handler(newVal) {
        this.isUserLoggedIn = newVal !== null
      },
      immediate: true,
    },
  },
  computed: {
    userReview() {
      if (this.isUserLoggedIn) {
        const review = this.reviewProduct.data.find(
          (item) => item.idKhachHang === this.currentUserId,
        )
        return review ? review.noiDung : 'Bạn chưa có đánh giá nào.'
      }
      return 'Bạn chưa đăng nhập.'
    },
    averageRating() {
      const totalStars = this.reviewProduct.data.reduce((sum, item) => sum + item.soSao, 0)
      const numberOfReviews = this.reviewProduct.data.length
      return numberOfReviews ? (totalStars / numberOfReviews).toFixed(1) : 'Chưa có đánh giá'
    },
  },
}
</script>

<style scoped></style>
