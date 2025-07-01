<template>
  <div class="star-rating">
    <span
      v-for="n in 5"
      :key="n"
      :class="['star', { 'filled': n <= rating }]"
    >★</span>
  </div>
</template>

<script>
export default {
  name: 'StarRating',
  props: {
    rating: {
      type: Number,
      required: true,
      validator: (value) => value >= 0 && value <= 5,
    },
  },
};
</script>

<style scoped>
.star-rating {
  display: inline-block;
}
.star {
  color: #e4e5e9; /* Màu sao chưa được tô */
  font-size: 1em;
}
.star.filled {
  color: #ffc107; /* Màu sao đã được tô */
}
</style>
