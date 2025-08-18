<template>
  <div class="star-rating">
    <span v-for="n in fullStars" :key="'full-' + n" class="star star-filled" @click="handleClick(n)">★</span>
    <span v-if="halfStar" class="star-half" @click="handleClick(fullStars + 0.5)">★</span>
    <span v-for="n in emptyStars" :key="'empty-' + n" class="star star-empty" @click="handleClick(fullStars + n)">☆</span>
  </div>
</template>

<script>
import { computed } from 'vue';

export default {
  name: 'StarRating',
  props: {
    rating: {
      type: Number,
      required: true,
      default: 0
    },
    maxStars: {
      type: Number,
      default: 5
    },
    readOnly: {
      type: Boolean,
      default: true
    }
  },
  emits: ['update:rating'],
  setup(props, { emit }) {
    const fullStars = computed(() => Math.floor(props.rating));
    const halfStar = computed(() => props.rating % 1 !== 0);
    const emptyStars = computed(() => props.maxStars - Math.ceil(props.rating));

    const handleClick = (star) => {
      if (!props.readOnly) {
        emit('update:rating', star);
      }
    };

    return {
      fullStars,
      halfStar,
      emptyStars,
      handleClick
    };
  }
};
</script>

<style scoped>
.star-rating[read-only="false"] {
  cursor: pointer;
}
.star-filled {
  color: #ffc107;
}
.star-half {
  color: #ffc107;
  position: relative;
}
.star-half::before {
  content: '★';
  position: absolute;
  left: 0;
  width: 50%;
  overflow: hidden;
}
.star-empty {
  color: #e0e0e0;
}
.star-rating .star {
  font-size: 1.75rem;
  cursor: pointer;
  transition: color 0.2s;
}
.star-rating .star.filled,
.star-rating .star:hover,
.star-rating .star:hover ~ .star {
  color: #ffc107;
}
.star-rating:hover .star {
  color: #ffc107;
}
.star-rating .star:hover ~ .star {
  color: #e4e5e9;
}
</style>