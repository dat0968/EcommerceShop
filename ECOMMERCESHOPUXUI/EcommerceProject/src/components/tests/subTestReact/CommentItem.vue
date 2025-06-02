<template>
  <div class="comment">
    <div :style="{ marginLeft: comment.parentId ? '20px' : '0' }">
      <strong>{{ comment.hoTen }}</strong> - {{ new Date(comment.ngayBinhLuan).toLocaleString() }}
      <p>{{ comment.noiDung }}</p>
    </div>
    <div v-if="comment.childComments.length" class="child-comments">
      <CommentItem
        v-for="child in comment.childComments"
        :key="child.id"
        :comment="child"
        :isUserLoggedIn="isUserLoggedIn"
      />
    </div>
  </div>
</template>

<script>
export default {
  name: 'CommentItem',
  props: {
    comment: {
      type: Object,
      required: true,
    },
    isUserLoggedIn: {
      type: Boolean,
      default: false,
    },
  },
}
</script>

<style scoped>
.comment {
  margin-bottom: 1rem;
}
.child-comments {
  margin-left: 20px;
}
</style>
