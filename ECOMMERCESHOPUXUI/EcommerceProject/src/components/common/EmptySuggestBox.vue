<template>
  <div class="empty-suggest-box text-center py-5">
    <div class="mb-3">
      <slot name="icon">
        <span style="font-size: 3rem; color: #bbb">
          <i :class="iconSub"></i>
        </span>
      </slot>
    </div>
    <div class="mb-2 text-muted" style="font-size: 1.1rem">
      {{ contentText }}
    </div>
    <RouterLink v-if="linkNav" :to="linkNav" class="btn btn-outline-primary mt-2">
      {{ suggestContent }}
    </RouterLink>
  </div>
</template>

<script>
export default {
  name: 'EmptySuggestBox',
  props: {
    iconSub: {
      type: String,
      default: 'fa fa-box-open',
    },
    contentText: {
      type: String,
      required: true,
    },
    linkNav: {
      type: String,
      default: '',
    },
    suggestContent: {
      type: String,
      default: 'Khám phá ngay',
    },
  },
}
</script>

<style scoped>
.empty-suggest-box {
  border: 1px dashed #ddd;
  border-radius: 12px;
  background: #fafbfc;
}
</style>
