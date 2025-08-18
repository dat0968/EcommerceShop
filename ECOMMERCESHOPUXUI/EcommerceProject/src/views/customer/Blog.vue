<template>
  <div class="container py-5">
    <h1 class="mb-4">Blog Thời Trang</h1>
    <p class="lead mb-4">Chào mừng bạn đến với Blog của EcommerceShop! Tại đây, bạn sẽ tìm thấy những bài viết mới nhất về xu hướng thời trang, mẹo phối đồ, tin tức về các bộ sưu tập mới và nhiều hơn nữa. Hãy cùng chúng tôi khám phá thế giới thời trang đầy màu sắc!</p>
    <div class="">
      <div v-for="post in posts" :key="post.id" class="mb-4 pb-4 border-bottom">
        <h2 class="h4">{{ post.title }}</h2>
        <p class="text-muted">{{ post.date }}</p>
        <p class="mt-2">{{ post.excerpt }}</p>
        <a href="#" class="text-primary">Đọc thêm</a>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Blog',
  data() {
    return {
      posts: [
        {
          id: 1,
          title: 'Xu hướng thời trang Thu/Đông 2025: Những gam màu lên ngôi',
          date: 'August 1, 2025',
          excerpt: 'Khám phá những gam màu chủ đạo và phong cách thiết kế sẽ thống trị sàn diễn thời trang trong mùa Thu/Đông năm nay. Từ những tông màu trầm ấm đến các sắc thái nổi bật, hãy cùng EcommerceShop cập nhật tủ đồ của bạn.'
        },
        {
          id: 2,
          title: 'Bí quyết phối đồ công sở thanh lịch và hiện đại',
          date: 'July 25, 2025',
          excerpt: 'Làm thế nào để vừa chuyên nghiệp vừa thể hiện cá tính tại nơi làm việc? Bài viết này sẽ chia sẻ những bí quyết phối đồ công sở giúp bạn tự tin và nổi bật mỗi ngày.'
        },
        {
          id: 3,
          title: 'Chăm sóc trang phục đúng cách: Giữ quần áo luôn bền đẹp',
          date: 'July 18, 2025',
          excerpt: 'Quần áo là khoản đầu tư, và việc chăm sóc đúng cách sẽ giúp chúng bền đẹp theo thời gian. Tìm hiểu các mẹo giặt ủi, bảo quản và sửa chữa trang phục từ chuyên gia của chúng tôi.'
        },
        {
          id: 4,
          title: 'Làm thế nào để chọn size quần áo phù hợp khi mua sắm online?',
          date: 'July 10, 2025',
          excerpt: 'Mua sắm online tiện lợi nhưng chọn size luôn là thách thức. Bài viết này sẽ hướng dẫn bạn cách đo cơ thể và sử dụng bảng size để tìm được trang phục vừa vặn nhất.'
        }
      ]
    }
  }
}
</script>

<style scoped>
</style>