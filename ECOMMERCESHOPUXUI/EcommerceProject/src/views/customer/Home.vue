<script setup>
import { onMounted } from 'vue'
import category1 from '@/assets/Customer/img/categories/category-1.jpg'
import category2 from '@/assets/Customer/img/categories/category-2.jpg'
import category3 from '@/assets/Customer/img/categories/category-3.jpg'
import category4 from '@/assets/Customer/img/categories/category-4.jpg'
import category5 from '@/assets/Customer/img/categories/category-5.jpg'
import banner1 from '@/assets/Customer/img/banner/banner-1.jpg'
import insta1 from '@/assets/Customer/img/instagram/insta-1.jpg'
import insta2 from '@/assets/Customer/img/instagram/insta-2.jpg'
import insta3 from '@/assets/Customer/img/instagram/insta-3.jpg'
import insta4 from '@/assets/Customer/img/instagram/insta-4.jpg'
import insta5 from '@/assets/Customer/img/instagram/insta-5.jpg'
import insta6 from '@/assets/Customer/img/instagram/insta-6.jpg'
import { ref } from 'vue'
import TestCodeQr from '@/components/tests/TestCodeQr.vue'
const setBackgroundImages = () => {
  const elements = document.querySelectorAll('[data-setbg]')
  elements.forEach((element) => {
    const bgImage = element.getAttribute('data-setbg')
    if (bgImage) {
      const imagePath = bgImage.replace('../../assets/img/', '')
      let imageUrl = ''

      switch (imagePath) {
        case 'categories/category-1.jpg':
          imageUrl = category1
          break
        case 'categories/category-2.jpg':
          imageUrl = category2
          break
        case 'categories/category-3.jpg':
          imageUrl = category3
          break
        case 'categories/category-4.jpg':
          imageUrl = category4
          break
        case 'categories/category-5.jpg':
          imageUrl = category5
          break
        case 'banner/banner-1.jpg':
          imageUrl = banner1
          break
        case 'instagram/insta-1.jpg':
          imageUrl = insta1
          break
        case 'instagram/insta-2.jpg':
          imageUrl = insta2
          break
        case 'instagram/insta-3.jpg':
          imageUrl = insta3
          break
        case 'instagram/insta-4.jpg':
          imageUrl = insta4
          break
        case 'instagram/insta-5.jpg':
          imageUrl = insta5
          break
        case 'instagram/insta-6.jpg':
          imageUrl = insta6
          break
      }

      if (imageUrl) {
        element.style.backgroundImage = `url(${imageUrl})`
      }
    }
  })
}
const getUrlAPI = ref(`https://localhost:7217/api`)
const ListNewProducts = ref([])
const ListBestSellerProducts = ref([])
const ListBestHotProducts = ref([])
const fetchAPINewProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/Home/GetNewProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListNewProducts.value = result
}

const fetchAPIBestSellerProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/Home/GetBestSellerProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListBestSellerProducts.value = result
}

const fetchAPIHotProduts = async () => {
  const response = await fetch(`${getUrlAPI.value}/Home/GetHotProduct`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to fetch')
  }
  const result = await response.json()
  ListBestHotProducts.value = result
}

onMounted(() => {
  setBackgroundImages(), fetchAPINewProduts()
  fetchAPIBestSellerProduts()
  fetchAPIHotProduts()
})
</script>
<template>
  <div>
    <!-- Categories Section Begin -->
    <section class="categories">
      <div class="container-fluid">
        <div class="row">
          <div class="col-lg-6 p-0">
            <div
              class="categories__item categories__large__item set-bg"
              data-setbg="../../assets/img/categories/category-1.jpg"
            >
              <div class="categories__text">
                <h1>Thời trang nữ</h1>
                <p>Khám phá phong cách thời trang dành riêng cho phái đẹp.</p>
                <router-link style="text-decoration-line: none" to="/Shop">Mua ngay</router-link>
              </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="row">
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg"
                  data-setbg="../../assets/img/categories/category-2.jpg"
                >
                  <div class="categories__text">
                    <h4>Thời trang nam</h4>
                    <p>Đậm chất nam tính, phong cách lịch lãm</p>
                    <router-link style="text-decoration-line: none" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg"
                  data-setbg="../../assets/img/categories/category-3.jpg"
                >
                  <div class="categories__text">
                    <h4>Thời trang trẻ em</h4>
                    <p>Phong cách năng động, dễ thương cho bé yêu</p>
                    <router-link style="text-decoration-line: none" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg"
                  data-setbg="../../assets/img/categories/category-4.jpg"
                >
                  <div class="categories__text">
                    <h4>Giày dép</h4>
                    <p>Bước đi phong cách, vững vàng mỗi ngày</p>
                    <router-link style="text-decoration-line: none" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                </div>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-6 p-0">
                <div
                  class="categories__item set-bg"
                  data-setbg="../../assets/img/categories/category-5.jpg"
                >
                  <div class="categories__text">
                    <h4>Phụ kiện</h4>
                    <p>Hoàn thiện phong cách với hàng trăm phụ kiện hot</p>
                    <router-link style="text-decoration-line: none" to="/Shop"
                      >Mua ngay</router-link
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Categories Section End -->

    <!-- Product Section Begin -->
    <section class="product spad">
      <div class="container">
        <div class="row" style="text-align: center">
          <div class="col-lg-4 col-md-4">
            <div
              style="
                border-bottom: 2px solid #e7ab3c;
                display: inline-block;
                padding-bottom: 5px;
                margin-bottom: 50px;
              "
            >
              <h4>Sản phẩm mới nhất</h4>
            </div>
          </div>
        </div>
        <div class="row property__gallery">
          <div
            class="col-lg-3 col-md-4 col-sm-6 mix"
            v-for="item in ListNewProducts"
            :key="item.maSp"
          >
            <div class="product__item">
              <div class="product__item__pic">
                <img
                  :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                    item.productDetails[0].images[0].tenHinhAnh
                  }`"
                  alt="Buttons tweed blazer"
                />
                <!-- <div class="label new">New</div> -->
                <ul class="product__hover">
                  <li>
                    <a href="@/assets/Customer/img/product/product-1.jpg" class="image-popup"
                      ><span class="arrow_expand"></span
                    ></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_heart_alt"></span></a>
                  </li>
                  <li>
                    <a href="#"><span class="icon_bag_alt"></span></a>
                  </li>
                </ul>
              </div>
              <div class="product__item__text">
                <h6>
                  <router-link :to="`/product/${item.maSp}`" style="text-decoration-line: none">
                    {{ item.tenSanPham }}
                    <div class="product__price text-muted fw-semibold fs-6 text-danger">
                      {{ item.khoangGia }}
                    </div>
                  </router-link>
                </h6>
                <!-- <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div> -->
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Product Section End -->

    <!-- Banner Section Begin -->
    <section class="banner set-bg" data-setbg="../../assets/img/banner/banner-1.jpg">
      <div class="container">
        <div class="row">
          <div class="col-xl-7 col-lg-8 m-auto">
            <div class="banner__slider owl-carousel">
              <div class="banner__item">
                <div class="banner__text">
                  <span>The Chloe Collection</span>
                  <h1>The Project Jacket</h1>
                  <a href="#">Mua ngay</a>
                </div>
              </div>
              <div class="banner__item">
                <div class="banner__text">
                  <span>The Chloe Collection</span>
                  <h1>The Project Jacket</h1>
                  <a href="#">Mua ngay</a>
                </div>
              </div>
              <div class="banner__item">
                <div class="banner__text">
                  <span>The Chloe Collection</span>
                  <h1>The Project Jacket</h1>
                  <a href="#">Mua ngay</a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Banner Section End -->

    <!-- Trend Section Begin -->
    <section class="trend spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-4 col-md-4 col-sm-6">
            <div class="trend__content">
              <div class="section-title">
                <h4>Đang hot</h4>
              </div>
              <div class="trend__item" v-for="item in ListBestHotProducts" :key="item.maSp">
                <div class="trend__item__pic">
                  <img
                    :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                      item.productDetails[0].images[0].tenHinhAnh
                    }`"
                    alt=""
                    class="img-fluid"
                  />
                </div>
                <div class="trend__item__text text-center mt-2">
                  <h6>
                    <router-link
                      :to="`/product/${item.maSp}`"
                      style="text-decoration-line: none; color: black"
                    >
                      {{ item.tenSanPham }}
                      <div class="product__price text-danger fw-semibold fs-7">
                        {{ item.khoangGia }}
                      </div>
                    </router-link>
                  </h6>
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-4 col-md-4 col-sm-6">
            <div class="trend__content">
              <div class="section-title">
                <h4>Sản phẩm bán chạy</h4>
              </div>

              <div class="trend__item mb-3" v-for="item in ListBestSellerProducts" :key="item.maSp">
                <div class="trend__item__pic">
                  <img
                    :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                      item.productDetails[0].images[0].tenHinhAnh
                    }`"
                    alt=""
                    class="img-fluid"
                  />
                </div>
                <div class="trend__item__text text-center mt-2">
                  <h6>
                    <router-link
                      :to="`/product/${item.maSp}`"
                      style="text-decoration-line: none; color: black"
                    >
                      {{ item.tenSanPham }}
                      <div class="product__price text-danger fw-semibold fs-7">
                        {{ item.khoangGia }}
                      </div>
                    </router-link>
                  </h6>
                </div>
              </div>
            </div>
          </div>

          <div class="col-lg-4 col-md-4 col-sm-6">
            <div class="trend__content">
              <div class="section-title">
                <h4>Sản phẩm nổi bật</h4>
              </div>
              <div class="trend__item">
                <div class="trend__item__pic">
                  <img src="../../assets/img/trend/f-1.jpg" alt="" class="img-fluid" />
                </div>
                <div class="trend__item__text text-center mt-2">
                  <h6 class="mb-1">Bow wrap skirt</h6>
                  <!-- <div class="rating">
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                  </div> -->
                  <div class="product__price">$ 59.0</div>
                </div>
              </div>
              <div class="trend__item">
                <div class="trend__item__pic">
                  <img src="../../assets/img/trend/f-2.jpg" alt="" class="img-fluid" />
                </div>
                <div class="trend__item__text text-center">
                  <h6 class="mb-1">Metallic earrings</h6>
                  <!-- <div class="rating">
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                  </div> -->
                  <div class="product__price">$ 59.0</div>
                </div>
              </div>
              <div class="trend__item">
                <div class="trend__item__pic">
                  <img src="../../assets/img/trend/f-3.jpg" alt="" class="img-fluid" />
                </div>
                <div class="trend__item__text text-center">
                  <h6 class="mb-1">Flap cross-body bag</h6>
                  <!-- <div class="rating">
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                    <i class="fa fa-star"></i>
                  </div> -->
                  <div class="product__price">$ 59.0</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Trend Section End -->
  </div>
  <TestCodeQr />
</template>

<style>
.product__item__pic {
  height: 300px;
  position: relative;
  overflow: hidden;
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

.product__item__pic img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.categories__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 300px;
  position: relative;
}

.categories__large__item {
  min-height: 600px;
}

.banner {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 500px;
  position: relative;
}

.instagram__item {
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
  height: 100%;
  min-height: 200px;
  position: relative;
}

/* --- SỬA ĐỂ CÂN ĐỐI CÁC MỤC ĐANG HOT, BÁN CHẠY, NỔI BẬT --- */
.trend__content {
  min-height: 520px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  /* background: #fff; */
  /* border-radius: 12px; */
  /* box-shadow: 0 2px 16px rgba(0, 0, 0, 0.04); */
  padding: 24px 12px 18px 12px;
  margin-bottom: 24px;
}

.trend__item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  margin-bottom: 18px;
  min-height: 220px;
}

.trend__item__pic {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 120px;
  height: 180px;
  overflow: hidden;
  background: #f8f8f8;
  border-radius: 8px;
  margin-bottom: 10px;
}

.trend__item__pic img {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  display: block;
  margin: 0 auto;
}

.trend__item__text {
  text-align: center;
  width: 100%;
}

.section-title h4 {
  text-align: center;
  width: 100%;
}

@media (max-width: 991px) {
  .trend__content {
    min-height: 0;
    padding: 18px 6px 12px 6px;
  }
}
</style>
