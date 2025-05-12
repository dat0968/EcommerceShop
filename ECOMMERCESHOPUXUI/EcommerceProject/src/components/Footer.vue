<template>
  <div>
    <!-- Footer Section Begin -->
    <footer class="footer">
      <div class="container">
        <div class="row justify-content-center">
          <div class="col-lg-4 col-md-6 col-sm-7">
            <div class="footer__about text-center">
              <div class="footer__logo">
                <a href="./index.html"><img src="../../assets/img/logo.png" alt="" /></a>
              </div>
              <p>
                Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
                incididunt cilisis.
              </p>
              <div class="footer__payment">
                <a href="#"><img src="../../assets/img/payment/payment-1.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-2.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-3.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-4.png" alt="" /></a>
                <a href="#"><img src="../../assets/img/payment/payment-5.png" alt="" /></a>
              </div>
            </div>
          </div>
          <div class="col-lg-2 col-md-3 col-sm-5">
            <div class="footer__widget text-center">
              <h6>Quick links</h6>
              <ul>
                <li><a href="#">About</a></li>
                <li><a href="#">Blogs</a></li>
                <li><a href="#">Contact</a></li>
                <li><a href="#">FAQ</a></li>
              </ul>
            </div>
          </div>
          <div class="col-lg-2 col-md-3 col-sm-4">
            <div class="footer__widget text-center">
              <h6>Account</h6>
              <ul>
                <li><a href="#">My Account</a></li>
                <li><a href="#">Orders Tracking</a></li>
                <li><a href="#">Checkout</a></li>
                <li><a href="#">Wishlist</a></li>
              </ul>
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
export default {}
</script>

<style scoped>
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

.footer__logo {
  margin-bottom: 15px;
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

.footer__widget {
  margin-bottom: 20px;
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
  .footer {
    padding: 40px 0;
  }

  .footer__widget {
    margin-top: 30px;
  }
}
</style>