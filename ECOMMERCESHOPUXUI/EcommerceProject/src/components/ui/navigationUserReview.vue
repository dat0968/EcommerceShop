<template>
  <RouterLink to="/review">
    <span class="icon_star"></span>
    <div class="tip">{{ totalReviewNeedSubmit }}</div>
  </RouterLink>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'

export default {
  name: 'NavigationUserReview',
  data() {
    return {
      userReviews: {},
      totalReviewNeedSubmit: 0,
    }
  },
  mounted() {
    this.loadUserReviews()
  },
  methods: {
    async loadUserReviews() {
      try {
        const res = await axiosConfig.getFromApi(
          '/Review/users',
          ConfigsRequest.takeAuth({ 'Skip-Navigation': true }),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) {
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
      }
    },
  },
}
</script>
