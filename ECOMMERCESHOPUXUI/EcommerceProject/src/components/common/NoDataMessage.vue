<template>
  <div class="no-data-message text-center text-muted my-4">
    <i class="fas fa-box-open fa-3x mb-3"></i>
    <p class="h5">Không có dữ liệu để hiển thị.</p>
    <p class="text-sm">Vui lòng kiểm tra lại các bộ lọc hoặc thử lại sau.</p>
  </div>
</template>

<script>
export default {
  name: 'NoDataMessage',
};
</script>

<style scoped>
.no-data-message {
  padding: 20px;
  border-radius: 8px;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
}
</style>
