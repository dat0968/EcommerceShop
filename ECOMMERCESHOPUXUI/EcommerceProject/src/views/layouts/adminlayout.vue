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
            <a href="index.html" class="xp-logo"
              ><img src="../../assets/admin/images/logo.svg" class="img-fluid" alt="logo"
            /></a>
          </div>
          <!-- End XP Logobar -->

          <!-- Start XP Navigationbar -->
          <div class="xp-navigationbar">
            <ul class="xp-vertical-menu">
              <li>
                <RouterLink to="/Admin">
                  <i class="icon-speedometer"></i><span>Thống kê</span>
                </RouterLink>
              </li>
            </ul>
            <ul class="xp-vertical-menu">
              <li>
                <RouterLink>
                  <i class="icon-speedometer"></i><span>Quản lý sản phẩm</span>
                </RouterLink>
              </li>
            </ul>
            <ul class="xp-vertical-menu">
              <li>
                <RouterLink>
                  <i class="icon-speedometer"></i><span>Quản lý combo</span>
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
        <div class="xp-topbar">
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
                            <img
                              class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-1.jpg"
                              alt="Generic placeholder image"
                            />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Ariel Blue<span class="font-12 f-w-4 float-right">3 min ago</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Thank you for attending...<span
                                    class="badge badge-pill badge-success float-right"
                                    >2</span
                                  >
                                </p>
                              </a>
                            </div>
                          </li>
                          <li class="media xp-msg">
                            <img
                              class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-2.jpg"
                              alt="Generic placeholder image"
                            />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Jammy Moon<span class="font-12 f-w-4 float-right">5 min ago</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Hey no worries! Trust me...<span
                                    class="badge badge-pill badge-success float-right"
                                    >3</span
                                  >
                                </p>
                              </a>
                            </div>
                          </li>
                          <li class="media xp-msg">
                            <img
                              class="mr-3 align-self-center rounded-circle"
                              src="../../assets/admin/images/topbar/user-message-3.jpg"
                              alt="Generic placeholder image"
                            />
                            <div class="media-body">
                              <a href="#">
                                <h5 class="mt-0 mb-1 font-14">
                                  Lisa Ross<span class="font-12 f-w-4 float-right">5:25 PM</span>
                                </h5>
                                <p class="mb-0 font-13">
                                  Remedies for colic? i don't...<span
                                    class="badge badge-pill badge-success float-right"
                                    >5</span
                                  >
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
                      <a
                        class="dropdown-toggle"
                        href="#"
                        role="button"
                        id="xp-userprofile"
                        data-toggle="dropdown"
                        aria-haspopup="true"
                        aria-expanded="false"
                        ><img
                          src="../../assets/admin/images/topbar/user.jpg"
                          alt="user-profile"
                          class="rounded-circle img-fluid" /><span class="xp-user-live"></span
                      ></a>

                      <div
                        class="dropdown-menu dropdown-menu-right"
                        aria-labelledby="xp-userprofile"
                      >
                        <a class="dropdown-item py-3 text-white text-center font-16" href="#"
                          >Welcome, John Doe</a
                        >
                        <a class="dropdown-item" href="#"
                          ><i class="icon-user text-primary mr-2"></i> Profile</a
                        >
                        <a class="dropdown-item" href="#"
                          ><i class="icon-wallet text-success mr-2"></i> Billing</a
                        >
                        <a class="dropdown-item" href="#"
                          ><i class="icon-settings text-warning mr-2"></i> Setting</a
                        >
                        <a class="dropdown-item" href="#"
                          ><i class="icon-lock text-info mr-2"></i> Lock Screen</a
                        >
                        <a class="dropdown-item" href="#"
                          ><i class="icon-power text-danger mr-2"></i> Logout</a
                        >
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

        <statistics />

        <!-- Start XP Footerbar -->
        <div class="xp-footerbar">
          <footer class="footer">
            <p class="mb-0">© 2020 Neon - All Rights Reserved.</p>
          </footer>
        </div>
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
  border-color: #e94560;
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
  box-shadow: 0 0 20px rgba(0,0,0,0.1);
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
  background: #e94560;
  color: #fff;
  font-weight: 500;
  border-radius: 8px 8px 0 0;
}

.xp-userprofile .dropdown-item:last-child {
  border-bottom: none;
}

.xp-userprofile .dropdown-item:hover {
  background: #f8f9fa;
  color: #e94560;
}

.xp-userprofile .dropdown-item i {
  width: 20px;
  text-align: center;
  margin-right: 10px;
}

.xp-userprofile .dropdown-item:last-child {
  color: #dc3545;
}

.xp-userprofile .dropdown-item:last-child:hover {
  background: #dc3545;
  color: #fff;
}
</style>