<template>
  <div>
    <!-- Footer Section Begin -->
    <footer class="footer">
      <div class="container">
        <div class="row gy-4">
          <!-- About Section -->
          <div class="col-lg-4 col-md-12 text-center text-lg-start">
            <div class="footer__about">
              <div class="footer__logo mb-3">
                <RouterLink to="/">
                  <svg
                    viewBox="0 0 700 250"
                    role="img"
                    aria-label="Angel soft curvy logo with wings and animated gradient"
                    style="width: 150px; height: auto"
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
                    <text
                      x="50%"
                      y="60%"
                      dominant-baseline="middle"
                      text-anchor="middle"
                      class="angel-text"
                    >
                      Angel
                    </text>
                  </svg>
                </RouterLink>
              </div>
              <p class="text-muted px-3 px-lg-0">
                Chúng tôi cam kết mang đến những sản phẩm chất lượng cao và dịch vụ khách hàng tuyệt
                vời.
              </p>
              <div class="footer__payment justify-content-center justify-content-lg-start">
                <img src="@/assets/Customer/img/payment/payment-1.png" alt="Visa" />
                <img src="@/assets/Customer/img/payment/payment-2.png" alt="Mastercard" />
                <img src="@/assets/Customer/img/payment/payment-3.png" alt="Discover" />
                <img src="@/assets/Customer/img/payment/payment-4.png" alt="Amex" />
                <img src="@/assets/Customer/img/payment/payment-5.png" alt="Paypal" />
              </div>
            </div>
          </div>

          <!-- Links Section -->
          <div class="col-lg-2 col-md-4 col-sm-6">
            <div class="footer__widget">
              <h6>Liên kết nhanh</h6>
              <ul>
                <li><RouterLink to="/about-us">Giới thiệu</RouterLink></li>
                <li><RouterLink to="/blog">Blog</RouterLink></li>
                <li><RouterLink to="/contact">Liên hệ</RouterLink></li>
                <li><RouterLink to="/faq">Câu hỏi thường gặp</RouterLink></li>
              </ul>
            </div>
          </div>

          <!-- Account Section -->
          <div class="col-lg-2 col-md-4 col-sm-6">
            <div class="footer__widget">
              <h6>Tài khoản</h6>
              <ul>
                <li><RouterLink to="/Profile">Tài khoản của tôi</RouterLink></li>
                <li><RouterLink to="/order">Theo dõi đơn hàng</RouterLink></li>
                <li><RouterLink to="/cart">Thanh toán</RouterLink></li>
                <li><RouterLink to="/favoriteproduct">Danh sách yêu thích</RouterLink></li>
              </ul>
            </div> 
          </div>

          <!-- Contact Section -->
          <div class="col-lg-4 col-md-4 col-sm-12">
            <div class="footer__widget">
              <h6>Thông tin liên hệ</h6>
              <ul class="contact-info">
                <li><i class="fa fa-map-marker-alt"></i> 300, 06 đường Hà Huy Tập, tổ dân phố 08, BMT, Đắk Lắk</li>
                <li><i class="fa fa-phone"></i> 024 7300 1955</li>
                <li title="No email"><i class="fa fa-envelope"></i> info@ecommerceshop.com</li>
              </ul>
              <div class="footer__social">
                <a href="#" aria-label="Facebook"><i class="fab fa-facebook-f"></i></a>
                <a href="#" aria-label="Twitter"><i class="fab fa-twitter"></i></a>
                <a href="#" aria-label="Instagram"><i class="fab fa-instagram"></i></a>
                <a href="#" aria-label="Pinterest"><i class="fab fa-pinterest"></i></a>
              </div>
            </div>
          </div>
        </div>

        <!-- Copyright -->
        <div class="row mt-5">
          <div class="col-12">
            <div class="footer__copyright text-center">
              <p>
                Bản quyền &copy; {{ new Date().getFullYear() }} Tất cả quyền được bảo lưu | Được thực
                hiện bởi
                <a href="#" target="_blank" class="text-primary">Silent Stack Team</a>
              </p>
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

export default {
  name: 'AppFooter',
}
</script>

<style scoped>
/* General Footer Styles */
.footer {
  background: #111;
  padding: 60px 0;
  color: #b7b7b7;
  position: relative;
}

.footer::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, #ca1515, #e94560);
}

/* Logo and About */
.angel-text {
  font-family: 'Great Vibes', cursive;
  font-size: 120px;
  fill: url(#start);
  filter: drop-shadow(2px 2px 3px rgba(0, 0, 0, 0.4));
}

.wing {
  fill: #e9ecef;
  opacity: 0.8;
  filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.2));
}

.footer__about p {
  line-height: 1.7;
  font-size: 14px;
  margin-top: 1.5rem;
}

.footer__payment {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

.footer__payment img {
  height: 25px;
  opacity: 0.8;
  transition:
    transform 0.3s ease,
    opacity 0.3s ease;
}

.footer__payment img:hover {
  opacity: 1;
  transform: scale(1.1);
}

/* Footer Widgets */
.footer__widget {
  text-align: center;
}

.footer__widget h6 {
  color: #fff;
  font-weight: 600;
  margin-bottom: 20px;
  font-size: 16px;
  text-transform: uppercase;
  position: relative;
  padding-bottom: 10px;
}

.footer__widget h6::after {
  content: '';
  position: absolute;
  left: 50%;
  bottom: 0;
  transform: translateX(-50%);
  width: 40px;
  height: 2px;
  background: #ca1515;
}

.footer__widget ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.footer__widget ul li {
  margin-bottom: 10px;
}

.footer__widget ul li a {
  color: #b7b7b7;
  text-decoration: none;
  transition: all 0.3s ease;
  font-size: 14px;
}

.footer__widget ul li a:hover {
  color: #ca1515;
  text-decoration: underline;
}

/* Contact Info & Social */
.contact-info li {
  display: flex;
  align-items: center;
  justify-content: center;
}

.contact-info li i {
  color: #ca1515;
  margin-right: 10px;
  width: 20px;
  text-align: center;
}

.footer__social {
  margin-top: 20px;
  display: flex;
  gap: 10px;
  justify-content: center;
}

.footer__social a {
  color: #fff;
  background: #333;
  width: 40px;
  height: 40px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  text-decoration: none;
  transition: all 0.3s ease;
}

.footer__social a:hover {
  background: #ca1515;
  color: #fff;
  transform: translateY(-3px);
}

/* Copyright */
.footer__copyright {
  border-top: 1px solid #333;
  padding-top: 20px;
  margin-top: 20px;
}

.footer__copyright p {
  font-size: 14px;
  color: #888;
}

.footer__copyright a {
  color: #ca1515;
  text-decoration: none;
  font-weight: 500;
}
.footer__copyright a:hover {
  text-decoration: underline;
}

/* Responsive Styles */
@media (min-width: 992px) {
  .footer__widget,
  .footer__widget .contact-info li {
    text-align: left;
    justify-content: flex-start;
  }
  .footer__widget h6::after {
    left: 0;
    transform: translateX(0);
  }
  .footer__social {
    justify-content: flex-start;
  }
}

@media (max-width: 767px) {
  .footer {
    padding: 40px 0;
  }
  .footer__widget {
    margin-top: 30px;
  }
}

/* Search Model - Unchanged from original */
.search-model {
  display: none;
  position: fixed;
  width: 100%;
  height: 100%;
  left: 0;
  top: 0;
  background: #000;
  z-index: 99999;
}

.search-close-switch {
  position: absolute;
  width: 50px;
  height: 50px;
  background: #333;
  color: #fff;
  text-align: center;
  line-height: 50px;
  font-size: 24px;
  right: 50px;
  top: 50px;
  cursor: pointer;
}

.search-model-form {
  width: 600px;
}

.search-model-form input {
  width: 100%;
  font-size: 24px;
  border: none;
  border-bottom: 2px solid #999;
  background: 0 0;
  color: #999;
}
</style>