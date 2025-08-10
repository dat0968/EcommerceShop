<template>
  <div>
    <!-- Hero Section -->
    <section class="jumbotron jumbotron-fluid bg-light text-center py-5">
      <div class="container">
        <h1 class="display-4">Về chúng tôi</h1>
        <p class="lead">Khám phá câu chuyện đằng sau EcommerceShop - nơi thời trang và công nghệ hội tụ.</p>
      </div>
    </section>

    <!-- Main Content -->
    <div class="container py-5">
      <div class="row mb-5">
        <div class="col-lg-8 mx-auto text-center">
          <p class="lead">Chào mừng bạn đến với EcommerceShop - cửa hàng quần áo trực tuyến hiện đại, nơi phong cách gặp gỡ sự tiện lợi! Chúng tôi tự hào mang đến cho bạn những bộ sưu tập thời trang đa dạng, chất lượng cao, được cập nhật liên tục để bạn luôn dẫn đầu xu hướng.</p>
          <p class="lead">Tại EcommerceShop, chúng tôi không chỉ bán quần áo; chúng tôi mang đến trải nghiệm mua sắm toàn diện với các tính năng nổi bật:</p>
        </div>
      </div>

      <!-- Features Section with Cards -->
      <div class="row text-center mb-5">
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-tshirt fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Thư Viện Sản Phẩm Đa Dạng</h5>
              <p class="card-text">Dễ dàng duyệt tìm sản phẩm theo danh mục, xem mô tả chi tiết và hình ảnh rõ nét.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-lock fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Xác Thực An Toàn</h5>
              <p class="card-text">Đăng nhập, đăng ký an toàn qua JWT và xác thực Google.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-shopping-cart fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Giỏ Hàng & Quản Lý Đơn Hàng</h5>
              <p class="card-text">Thêm, sửa, xóa sản phẩm, đặt hàng, xem lịch sử mua và theo dõi trạng thái đơn hàng một cách thuận tiện.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-credit-card fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Thanh Toán An Toàn</h5>
              <p class="card-text">Tích hợp thanh toán qua VNPAY, đảm bảo giao dịch nhanh chóng và bảo mật.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-lightbulb fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Hệ Thống Gợi Ý Thông Minh</h5>
              <p class="card-text">Đề xuất sản phẩm cá nhân hóa dựa trên sở thích của bạn.</p>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-4">
          <div class="card h-100 shadow-sm">
            <div class="card-body">
              <i class="fas fa-magic fa-3x text-primary mb-3"></i>
              <h5 class="card-title">Tính Năng Nâng Cao</h5>
              <p class="card-text">Trải nghiệm so sánh sản phẩm với thử ảo AI, vòng quay may mắn giảm giá, chatbot hỗ trợ 24/7 và xem lịch sử duyệt gần nhất.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Call to Action -->
      <div class="row text-center">
        <div class="col-12">
          <p class="lead">Sứ mệnh của chúng tôi là giúp bạn thể hiện phong cách độc đáo của mình một cách dễ dàng và thú vị nhất. Hãy khám phá ngay bộ sưu tập của chúng tôi và tìm thấy những món đồ yêu thích của bạn!</p>
          <router-link to="/shop" class="btn btn-primary btn-lg mt-3">Khám phá ngay</router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'AboutUs'
}
</script>

<style scoped>
</style>