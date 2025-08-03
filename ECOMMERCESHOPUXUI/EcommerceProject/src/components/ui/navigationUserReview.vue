<template>
  <RouterLink to="/review" class="dropdown-item d-flex">
    <div class="position-relative">
      <i class="fa fa-star me-2"></i>
      <div class="position-absolute top-0 start-50 translate-middle badge rounded-pill bg-danger" v-if="totalReviewNeedSubmit > 0">{{ totalReviewNeedSubmit }}</div>
    </div>
    Đánh giá
  </RouterLink>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'
import authService from '@/services/authService'

export default {
  name: 'NavigationUserReview',
  data() {
    return {
      userReviews: {},
      totalReviewNeedSubmit: 0,
      isLogged: authService.isAuthenticated(),
    }
  },
  mounted() {
    this.loadUserReviews()
  },
  methods: {
    async loadUserReviews() {
      try {
        if (this.isLogged) {
          this.totalReviewNeedSubmit = 0
          this.userReviews = {}
          document.cookie = 'userReviews=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
          return
        }
        const res = await axiosConfig.getFromApi(
          '/Review/users',
          ConfigsRequest.takeAuth({ 'Skip-Navigation': true }),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, false)) {
          this.totalReviewNeedSubmit = 0
          this.userReviews = {}
          document.cookie = 'userReviews=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
          return
        }
        // Lưu vào cookies (hoặc localStorage)
        this.userReviews = res.data || {}
        document.cookie =
          'userReviews=' + encodeURIComponent(JSON.stringify(this.userReviews)) + '; path=/;'
        // Tính tổng số đánh giá cần thực hiện (ví dụ: số lượng notReviewIn7days)
        this.totalReviewNeedSubmit = Array.isArray(this.userReviews.notReviewIn7days)
          ? this.userReviews.notReviewIn7days.length
          : 0
      } catch (e) {
        this.totalReviewNeedSubmit = 0
        this.userReviews = {}
        document.cookie = 'userReviews=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
        console.log('Lỗi tải: ' + e)
      }
    },
  },
}
</script>
