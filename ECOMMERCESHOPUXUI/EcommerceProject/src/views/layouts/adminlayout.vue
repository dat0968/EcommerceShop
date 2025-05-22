<script setup>
import '../../assets/admin/images/favicon.ico'
import '../../assets/admin/plugins/chartist-js/chartist.min.css'
// import 'chartist/dist/chartist.css'
//import '../../assets/admin/plugins/datepicker/datepicker.min.css'
import 'bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css'
//import '../../assets/admin/css/bootstrap.min.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import '../../assets/admin/css/icons.css'
import '../../assets/admin/css/style.css'
// import '../../assets/admin/js/jquery.min.js'
import 'jquery'
import '@popperjs/core'
//import '../../assets/admin/js/bootstrap.min.js'
import 'bootstrap'
import '../../assets/Admin/js/detect.js'
import '../../assets/Admin/js/jquery.slimscroll.js'
import '../../assets/Admin/js/sidebar-menu.js'
import '../../assets/Admin/js/init/to-do-list-init.js'
//import '../../assets/admin/plugins/datepicker/datepicker.min.js'
import 'bootstrap-datepicker'
//import '../../assets/admin/plugins/datepicker/i18n/datepicker.en.js'
import 'bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js'
import '../../assets/Admin/js/main.js'
import statistics from '../admin/statistics/statistics.vue'
import CustomerManagement from '../admin/Customer/CustomerManagement.vue'
import { onMounted } from 'vue'

onMounted(() => {
  // Khởi tạo dropdown menu
  const dropdownToggle = document.querySelector('.xp-userprofile .dropdown-toggle')
  const dropdownMenu = document.querySelector('.xp-userprofile .dropdown-menu')

  if (dropdownToggle && dropdownMenu) {
    dropdownToggle.addEventListener('click', (e) => {
      e.preventDefault()
      dropdownMenu.classList.toggle('show')
    })

    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', (e) => {
      if (!dropdownToggle.contains(e.target) && !dropdownMenu.contains(e.target)) {
        dropdownMenu.classList.remove('show')
      }
    })
  }
})
</script>
<template>
  <div>
    <!-- Start XP Container -->
    <div id="xp-container">
      <!-- Start XP Leftbar -->
      <div class="xp-leftbar">
        <!-- Start XP Sidebar -->
        <div class="xp-sidebar">
          <!-- Start XP Logobar -->
          <div class="xp-logobar text-center">
            <svg viewBox="0 0 700 250" role="img" aria-label="Angel soft curvy logo with wings and animated gradient">
              <defs>
                <linearGradient id="start" x1="0%" y1="0%" x2="0%" y2="100%">
                  <stop offset="20%" stop-color="#EC4E79">
                    <animate attributeName="stop-color" values="#EC4E79; #ABA2B7; #5CCAE7; #ABA2B7; #EC4E79;" dur="6s"
                      repeatCount="indefinite" />
                  </stop>
                  <stop offset="40%" stop-color="#ABA2B7">
                    <animate attributeName="stop-color" values="#ABA2B7; #5CCAE7; #EC4E79; #5CCAE7; #ABA2B7;" dur="6s"
                      repeatCount="indefinite" />
                  </stop>
                  <stop offset="55%" stop-color="#5CCAE7">
                    <animate attributeName="stop-color" values="#5CCAE7; #ABA2B7; #EC4E79; #ABA2B7; #5CCAE7;" dur="6s"
                      repeatCount="indefinite" />
                  </stop>
                </linearGradient>
              </defs>

              <!-- Left wing - smooth curves -->
              <path class="wing left" d="M160 130 C110 90, 90 180, 150 170 C130 150, 140 110, 160 130 Z" />
              <path class="wing left" d="M150 140 C120 120, 110 170, 150 160 C140 140, 130 120, 150 140 Z"
                opacity="0.5" />

              <!-- Right wing - smooth curves -->
              <path class="wing right" d="M540 130 C590 90, 610 180, 550 170 C570 150, 560 110, 540 130 Z" />
              <path class="wing right" d="M550 140 C580 120, 590 170, 550 160 C560 140, 570 120, 550 140 Z"
                opacity="0.5" />

              <!-- Angel text with soft cursive font -->
              <text x="50%" y="60%" dominant-baseline="middle" text-anchor="middle" class="angel-text">
                Angel
              </text>
            </svg>
          </div>
          <!-- End XP Logobar -->

          <!-- Start XP Navigationbar -->
          <div class="xp-navigationbar"  >
            <ul class="xp-vertical-menu"  style="color: black;">
              <li>
                <RouterLink to="/Admin" class="menu-link" active-class="menu-active">
                  <i class="icon-speedometer"></i><span class="font-color" >Thống kê</span>
                </RouterLink>
              </li>
              <li>
                <RouterLink to="/admin/product">
                  <i class="icon-bag"></i><span class="font-color" >Quản lý sản phẩm</span>
                </RouterLink>
              </li>
              <li>
                <RouterLink to="/admin/combo">
                  <i class="icon-basket"></i><span class="font-color" >Quản lý combo</span>
                </RouterLink>
              </li>
              <li>
                <RouterLink to="/admin/customer">
                  <i class="icon-people"></i><span class="font-color" >Khách hàng</span>
                </RouterLink>
              </li>
              <li>
                <RouterLink to="/admin/staff">
                  <i class="icon-people"></i><span class="font-color" >Nhân Viên</span>
                </RouterLink>
              </li>
            </ul>
          </div>
          <!-- End XP Navigationbar -->
        </div>
        <!-- End XP Sidebar -->
      </div>
      <!-- End XP Leftbar -->

      <!-- Start XP Rightbar -->
      <div class="xp-rightbar">
        <!-- Start XP Topbar -->
        <div class="xp-topbar" style="background-color: rgb(169, 162, 221);">
          <!-- Start XP Row -->
          <div class="row">
            <!-- Start XP Col -->
            <div class="col-2 col-md-1 col-lg-1 order-2 order-md-1 align-self-center">
              <div class="xp-menubar">
                <a class="xp-menu-hamburger" href="javascript:void();">
                  <i class="icon-menu font-20 text-white"></i>
                </a>
              </div>
            </div>
            <!-- End XP Col -->

            <!-- Start XP Col -->
            <div class="col-10 col-md-11 col-lg-11 order-1 order-md-2">
              <div class="xp-profilebar text-right">
                <ul class="list-inline mb-0">
                  <li class="list-inline-item">
                    <div class="dropdown xp-message">
                      <div class="dropdown-menu dropdown-menu-right" aria-labelledby="xp-message">
                        <ul class="list-unstyled">
                          <li class="media">
                            <div class="media-body">
                              <h5 class="mt-0 mb-0 py-3 text-white text-center font-16">
                                8 New Messages
                              </h5>
                            </div>
                          </li>
                          <li class="media xp-msg">
                            <img class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-1.jpg"
                              alt="Generic placeholder image" />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Ariel Blue<span class="font-12 f-w-4 float-right">3 min ago</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Thank you for attending...<span
                                    class="badge badge-pill badge-success float-right">2</span>
                                </p>
                              </a>
                            </div>
                          </li>
                          <li class="media xp-msg">
                            <img class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-2.jpg"
                              alt="Generic placeholder image" />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Jammy Moon<span class="font-12 f-w-4 float-right">5 min ago</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Hey no worries! Trust me...<span
                                    class="badge badge-pill badge-success float-right">3</span>
                                </p>
                              </a>
                            </div>
                          </li>
                          <li class="media xp-msg">
                            <img class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-3.jpg"
                              alt="Generic placeholder image" />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Lisa Ross<span class="font-12 f-w-4 float-right">5:25 PM</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Remedies for colic? i don't...<span
                                    class="badge badge-pill badge-success float-right">5</span>
                                </p>
                              </a>
                            </div>
                          </li>
                          <li class="media">
                            <div class="media-body">
                              <h5 class="mt-0 mb-0 py-3 text-black text-center font-14">
                                <a href="#" class="text-primary">View all</a>
                              </h5>
                            </div>
                          </li>
                        </ul>
                      </div>
                    </div>
                  </li>

                  <li class="list-inline-item mr-0">
                    <div class="dropdown xp-userprofile">
                      <a class="dropdown-toggle" href="#" role="button" id="xp-userprofile" data-toggle="dropdown"
                        aria-haspopup="true" aria-expanded="false"><img src="../../assets/admin/images/topbar/user.jpg"
                          alt="user-profile" class="rounded-circle img-fluid" /><span class="xp-user-live"></span></a>

                      <div class="dropdown-menu dropdown-menu-right" aria-labelledby="xp-userprofile">
                        <a class="dropdown-item py-3 text-white text-center font-16" href="#">Welcome, John Doe</a>
                        <a class="dropdown-item" href="#"><i class="icon-user text-primary mr-2"></i> Profile</a>
                        <a class="dropdown-item" href="#"><i class="icon-wallet text-success mr-2"></i> Billing</a>
                        <a class="dropdown-item" href="#"><i class="icon-settings text-warning mr-2"></i> Setting</a>
                        <a class="dropdown-item" href="#"><i class="icon-lock text-info mr-2"></i> Lock Screen</a>
                        <a class="dropdown-item" href="#"><i class="icon-power text-danger mr-2"></i> Logout</a>
                      </div>
                    </div>
                  </li>
                </ul>
              </div>
            </div>
            <!-- End XP Col -->
          </div>
          <!-- End XP Row -->
        </div>
        <!-- End XP Topbar -->
        <!-- 
        <statistics /> -->

        <RouterView />
        <!-- Start XP Footerbar -->
        <!-- <div class="xp-footerbar">
          <footer class="footer">
            <p class="mb-0">© 2020 Neon - All Rights Reserved.</p>
          </footer>
        </div> -->
        <!-- End XP Footerbar -->
      </div>
      <!-- End XP Rightbar -->
    </div>
    <!-- End XP Container -->
  </div>
</template>

<script>
export default {}
</script>

<style>
.xp-profilebar {
  display: flex;
  justify-content: flex-end;
  align-items: center;
}

.xp-profilebar ul {
  display: flex;
  align-items: center;
  margin: 0;
  padding: 0;
}

.xp-profilebar .list-inline-item {
  margin-left: 15px;
}

.xp-userprofile img {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border: 2px solid #fff;
  transition: all 0.3s ease;
}

.xp-userprofile img:hover {
  border-color: #6c00bd;
}

.xp-userprofile .dropdown-toggle {
  display: flex;
  align-items: center;
  text-decoration: none;
}

.xp-userprofile .dropdown-toggle::after {
  display: none;
}

.xp-userprofile {
  position: relative;
}

.xp-userprofile .dropdown-menu {
  min-width: 200px;
  padding: 0;
  border: none;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
  border-radius: 8px;
  margin-top: 10px;
  display: none;
  position: absolute;
  right: 0;
  left: auto;
  transform: translateX(0);
}

.xp-userprofile .dropdown-menu.show {
  display: block;
}

.xp-userprofile .dropdown-item {
  padding: 12px 20px;
  font-size: 14px;
  color: #333;
  transition: all 0.3s ease;
  border-bottom: 1px solid #f5f5f5;
}

.xp-userprofile .dropdown-item:first-child {
  background: #6c00bd;
  color: #fff;
  font-weight: 500;
  border-radius: 8px 8px 0 0;
}

.xp-userprofile .dropdown-item:last-child {
  border-bottom: none;
}

.xp-userprofile .dropdown-item:hover {
  background: #f8f9fa;
  color: #6c00bd;
}

.xp-userprofile .dropdown-item i {
  width: 20px;
  text-align: center;
  margin-right: 10px;
}

.xp-userprofile .dropdown-item:last-child {
  color: #6c00bd;
}

.xp-userprofile .dropdown-item:last-child:hover {
  background: #6c00bd;
  color: #fff;
}

svg {
  max-width: 700px;
  width: 100%;
  height: auto;
  overflow: visible;
}

.angel-text {
  fill: url(#start);
  stroke: #a78bfa;
  stroke-width: 1.3;
  filter: drop-shadow(2px 2px 4px rgba(167, 139, 250, 0.6));
  font-family: 'Allura', cursive;
  font-weight: 400;
  font-size: 140px;
  letter-spacing: 6px;
  paint-order: stroke fill;
  user-select: none;
}

.wing {
  fill: url(#start);
  opacity: 0.7;
  filter: drop-shadow(1px 1px 3px rgba(167, 139, 250, 0.3));
  transform-origin: center;
  animation: gentleFloat 4.5s ease-in-out infinite;
  shape-rendering: geometricPrecision;
}

.wing.right {
  animation-delay: 0.25s;
}

@keyframes gentleFloat {

  0%,
  100% {
    transform: translateY(0) rotate(0deg);
    opacity: 0.7;
  }

  50% {
    transform: translateY(-7px) rotate(4deg);
    opacity: 0.85;
  }
}

/* Thay thế CSS hiện tại với đoạn này */

/* Định dạng menu không được chọn */
.xp-vertical-menu li a {
  color: #8a98ac;
  display: block;
  padding: 12px 15px;
  transition: all 0.3s ease;
  font-size: 14px;
  position: relative;
  text-decoration: none;
}

/* Định dạng khi hover menu */
.xp-vertical-menu li a:hover {
  color: #ffffff;
  background-color: rgba(255, 255, 255, 0.07);
}

/* Định dạng menu đang được chọn (active) */
.xp-vertical-menu li a.router-link-active,
.xp-vertical-menu li a.router-link-exact-active {
  color: #ffffff;
  background-image: linear-gradient(to right,#5E72EB,
  #FF9190);
  border-left: 4px solid #fff;
}

/* Hiệu ứng khi menu active */
.xp-vertical-menu li a.router-link-active i,
.xp-vertical-menu li a.router-link-exact-active i {
  color: #ffffff;
}

/* Điều chỉnh icon và text trong menu */
.xp-vertical-menu li a i {
  margin-right: 10px;
  font-size: 16px;
  vertical-align: middle;
  width: 20px;
  display: inline-block;
  text-align: center;
}
.font-color {
  color: rgb(70, 12, 12);
  font-weight: bold;
  font-size: 1rem;
}
</style>