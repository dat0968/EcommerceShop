<template>
  <div v-if="isLoading" class="col-12">
    <h6>Đang tải bình luận...</h6>
  </div>

  <div v-else class="col-12">
    <h6>Bình luận</h6>
    <div v-if="!isUserLoggedIn" class="alert alert-warning">
      Vui lòng <router-link to="/login">đăng nhập</router-link> để có thể bình luận.
    </div>
    <div class="comments-list">
      <CommentItem
        v-for="comment in sortedComments"
        :key="comment.id"
        :comment="comment"
        :isUserLoggedIn="isUserLoggedIn"
      />
    </div>
  </div>
</template>

<script>
import CommentItem from './CommentItem.vue' // Nhập component hiển thị bình luận

export default {
  name: 'CommentTestSub',
  components: {
    CommentItem,
  },
  props: {
    commentsProduct: {
      type: Object,
      default: () => ({}),
    },
    isLoading: {
      type: Boolean,
      default: true,
    },
    isUserLoggedIn: {
      type: Boolean,
      default: null,
    },
  },
  data() {
    return {}
  },
  computed: {
    sortedComments() {
      const comments = this.commentsProduct.data || []
      const commentsMap = {}
      comments.forEach((comment) => {
        comment.childComments = []
        commentsMap[comment.id] = comment
      })

      const roots = []
      comments.forEach((comment) => {
        if (comment.parentId === 0) {
          roots.push(comment)
        } else {
          const parentComment = commentsMap[comment.parentId]
          if (parentComment) {
            parentComment.childComments.push(comment)
          }
        }
      })

      return roots.sort((a, b) => new Date(b.ngayBinhLuan) - new Date(a.ngayBinhLuan))
    },
  },
}
</script>

<style scoped>
.comments-list {
  margin-left: 0;
}
</style>
