<template>
  <div>
    <!-- Footer Section Begin -->
    <footer class="footer">
      <div class="container">
        <div class="row justify-content-center">
          <!-- Cột thông tin chính -->
          <div class="col-lg-4 col-md-6 col-sm-7">
            <div class="footer__about text-center text-md-start mb-4 mb-lg-0">
              <div class="footer__logo mb-3">
                <svg
                  viewBox="0 0 700 250"
                  role="img"
                  aria-label="Angel soft curvy logo with wings and animated gradient"
                >
                  <defs>
                    <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                      <stop offset="20%" stop-color="#EC4E79">
                        <animate
                          attributeName="stop-color"
                          values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;"
                          dur="6s"
                          repeatCount="indefinite"
                        />
                      </stop>
                      <stop offset="40%" stop-color="#ABA2B7">
                        <animate
                          attributeName="stop-color"
                          values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;"
                          dur="6s"
                          repeatCount="indefinite"
                        />
                      </stop>
                      <stop offset="55%" stop-color="#5CCAE7">
                        <animate
                          attributeName="stop-color"
                          values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;"
                          dur="6s"
                          repeatCount="indefinite"
                        />
                      </stop>
                    </linearGradient>
                  </defs>

                  <path
                    class="wing left"
                    d="M160 130 C110 90, 90 180, 150 170 C130 150, 140 110, 160 130 Z"
                  />
                  <path
                    class="wing left"
                    d="M150 140 C120 120, 110 170, 150 160 C140 140, 130 120, 150 140 Z"
                    opacity="0.5"
                  />
                  <path
                    class="wing right"
                    d="M540 130 C590 90, 610 180, 550 170 C570 150, 560 110, 540 130 Z"
                  />
                  <path
                    class="wing right"
                    d="M550 140 C580 120, 590 170, 550 160 C560 140, 570 120, 550 140 Z"
                    opacity="0.5"
                  />

                  <RouterLink to="/" style="text-decoration: none">
                    <text
                      x="50%"
                      y="60%"
                      dominant-baseline="middle"
                      text-anchor="middle"
                      class="angel-text"
                    >
                      Angel
                    </text>
                  </RouterLink>
                </svg>
                <div class="footer__payment">
                  <!-- <a href="#"><img src="../../assets/img/payment/payment-1.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-2.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-3.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-4.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-5.png" alt="" /></a> -->
                </div>
                <p class="text-muted">
                  Chúng tôi cam kết mang đến những sản phẩm chất lượng cao và dịch vụ khách hàng
                  tuyệt vời.
                </p>
                <div
                  class="footer__payment d-flex justify-content-center justify-content-md-start gap-2"
                >
                  <a href="#"
                    ><img
                      src="@/assets/Customer/img/payment/payment-1.png"
                      alt="Visa"
                      class="img-fluid"
                      style="height: 25px"
                  /></a>
                  <a href="#"
                    ><img
                      src="@/assets/Customer/img/payment/payment-2.png"
                      alt="Mastercard"
                      class="img-fluid"
                      style="height: 25px"
                  /></a>
                  <a href="#"
                    ><img
                      src="@/assets/Customer/img/payment/payment-3.png"
                      alt="Discover"
                      class="img-fluid"
                      style="height: 25px"
                  /></a>
                  <a href="#"
                    ><img
                      src="@/assets/Customer/img/payment/payment-4.png"
                      alt="Amex"
                      class="img-fluid"
                      style="height: 25px"
                  /></a>
                  <a href="#"
                    ><img
                      src="@/assets/Customer/img/payment/payment-5.png"
                      alt="Paypal"
                      class="img-fluid"
                      style="height: 25px"
                  /></a>
                </div>
              </div>
            </div>

            <!-- Cột liên kết nhanh -->
            <div class="col-lg-2 col-md-3 col-sm-5">
              <div class="footer__widget text-center text-md-start mt-4 mt-lg-0">
                <h6 class="text-white mb-3">Liên kết nhanh</h6>
                <ul class="list-unstyled">
                  <li><a href="#" class="text-muted text-decoration-none">Giới thiệu</a></li>
                  <li><a href="#" class="text-muted text-decoration-none">Blog</a></li>
                  <li>
                    <RouterLink to="/contactus" class="text-muted text-decoration-none"
                      >Liên hệ</RouterLink
                    >
                  </li>
                  <li>
                    <a href="#" class="text-muted text-decoration-none">Câu hỏi thường gặp</a>
                  </li>
                  <TestCodeQr />
                </ul>
              </div>
            </div>

            <!-- Cột tài khoản -->
            <div class="col-lg-2 col-md-3 col-sm-4">
              <div class="footer__widget text-center text-md-start mt-4 mt-lg-0">
                <h6 class="text-white mb-3">Tài khoản</h6>
                <ul class="list-unstyled">
                  <li><a href="#" class="text-muted text-decoration-none">Tài khoản của tôi</a></li>
                  <li>
                    <a href="/FollowOrder" class="text-muted text-decoration-none"
                      >Theo dõi đơn hàng</a
                    >
                  </li>
                  <li><a href="#" class="text-muted text-decoration-none">Thanh toán</a></li>
                  <li>
                    <a href="/favoriteproduct" class="text-muted text-decoration-none"
                      >Danh sách yêu thích</a
                    >
                  </li>
                </ul>
              </div>
            </div>

            <!-- Cột thông tin liên hệ -->
            <div class="col-lg-4 col-md-12 col-sm-12">
              <div class="footer__widget text-center text-md-start mt-4 mt-lg-0">
                <h6 class="text-white mb-3">Thông tin liên hệ</h6>
                <ul class="list-unstyled text-muted">
                  <li><i class="fa fa-map-marker-alt me-2"></i> 123 Đường ABC, Quận XYZ, TP.HCM</li>
                  <li><i class="fa fa-phone me-2"></i> +84 123 456 789</li>
                  <li><i class="fa fa-envelope me-2"></i> info@ecommerceshop.com</li>
                </ul>
                <div class="footer__social mt-3">
                  <a href="#" class="btn btn-outline-light btn-sm rounded-circle me-2"
                    ><i class="fab fa-facebook-f"></i
                  ></a>
                  <a href="#" class="btn btn-outline-light btn-sm rounded-circle me-2"
                    ><i class="fab fa-twitter"></i
                  ></a>
                  <a href="#" class="btn btn-outline-light btn-sm rounded-circle me-2"
                    ><i class="fab fa-instagram"></i
                  ></a>
                  <a href="#" class="btn btn-outline-light btn-sm rounded-circle"
                    ><i class="fab fa-pinterest"></i
                  ></a>
                </div>
              </div>
            </div>
          </div>
          <div class="row mt-4">
            <div class="col-lg-12 text-center">
              <div class="footer__copyright text-muted">
                <p>
                  Bản quyền &copy; {{ new Date().getFullYear() }} Tất cả quyền được bảo lưu | Được
                  thực hiện bởi
                  <a href="#" target="_blank" class="text-primary text-decoration-none"
                    >Nhóm của bạn</a
                  >
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </footer>
    <!-- Footer Section End -->

    <!-- Search Begin -->
    <div class="search-model">
      <div class="h-100 d-flex align-items-center justify-content-center">
        <div class="search-close-switch">+</div>
        <form class="search-model-form">
          <input type="text" id="search-input" placeholder="Search here....." />
        </form>
      </div>
    </div>
    <!-- Search End -->
  </div>
</template>

<script>
import TestCodeQr from './tests/TestCodeQr.vue'

export default {
  name: 'AppFooter',
  components: { TestCodeQr },
}
</script>

<style>
.footer {
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
  padding: 40px 0;
  color: #fff;
  position: relative;
  overflow: hidden;
}

.footer::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #ca1515, #e94560);
}

.footer__logo img {
  max-width: 120px;
}

.footer__about p {
  color: #b7b7b7;
  margin-bottom: 20px;
  line-height: 1.6;
  font-size: 14px;
}

.footer__payment {
  display: flex;
  justify-content: center;
  gap: 10px;
  margin-top: 15px;
}

.footer__payment img {
  height: 25px;
  transition: transform 0.3s ease;
}

.footer__payment img:hover {
  transform: scale(1.1);
}

.footer__widget h6 {
  color: #fff;
  font-weight: 600;
  margin-bottom: 15px;
  font-size: 16px;
  position: relative;
  padding-bottom: 8px;
}

.footer__widget h6::after {
  content: '';
  position: absolute;
  left: 50%;
  bottom: 0;
  transform: translateX(-50%);
  width: 50px;
  height: 2px;
  background: #ca1515;
}

.footer__widget ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.footer__widget ul li {
  margin-bottom: 8px;
}

.footer__widget ul li a {
  color: #b7b7b7;
  text-decoration: none;
  transition: all 0.3s ease;
  display: inline-block;
  font-size: 14px;
}

.footer__widget ul li a:hover {
  color: #ca1515;
  transform: translateX(5px);
}

/* Responsive styles */
@media (max-width: 768px) {
  .footer__widget {
    margin-top: 30px;
  }
}
</style>
