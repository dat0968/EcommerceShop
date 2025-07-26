<template>
  <div class="loading-spinner">
    <div class="spinner-border text-primary" role="status">
      <span class="visually-hidden">Loading...</span>
    </div>
    <p class="mt-2 text-muted">Đang tải dữ liệu...</p>
  </div>
</template>

<script>
export default {
  name: 'LoadingSpinner',
};
</script>

<style scoped>
.loading-spinner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
}
.spinner-border {
  width: 3rem;
  height: 3rem;
}
</style>
