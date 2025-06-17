
<script setup>
import { ref, onMounted, computed } from 'vue'
import { decodeToken, validateToken } from '@/utils/auth'
import { GetApiUrl } from '@/constants/api'
import Cookies from 'js-cookie'
import { useRoute } from 'vue-router'
const route = useRoute()
const recommendationProduct = ref([])
const id = route.params.id
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const getUrlAPI = ref(GetApiUrl())
const fetchRcmProduct = async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid) {
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    const response = await fetch(
      `${getUrlAPI.value}/api/Home/RecommendationProduct?UserId=${readToken.IdUser}&maSp=${id}&numberOfRecommendations=8`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    )

    if (!response.ok) {
      throw new Error('Error to fetchRecommendationProducts')
    }
    const result = await response.json()
    recommendationProduct.value = result
    console.log(recommendationProduct.value)
  }
}
const isLogin = computed(() => {
  if (accessToken.value != undefined && accessToken.value != '') {
    return true
  }
  return false
})
onMounted(async () => {
  await fetchRcmProduct()
})
</script>
<template>
  <div v-if="isLogin" class="row">
    <div class="col-lg-12 text-center">
      <div class="related__title">
        <h5>GỢI Ý CHO BẠN</h5>
      </div>
    </div>
    <div v-for="item in recommendationProduct" :key="item.maSp" class="col-lg-3 col-md-4 col-sm-6">
      <div class="product__item">
        <div
          class="product__item__pic set-bg"
          :data-setbg="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
            item.productDetails[0].images[0].tenHinhAnh
          }`"
        >
          <img
            :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
              item.productDetails[0].images[0].tenHinhAnh
            }`"
            class="image-popup"
            style="width: 100%; height: 300px; object-fit: cover; border-radius: 8px"
          /><span class="arrow_expand"></span>
          <ul class="product__hover">
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
            <router-link :to="`/product/${item.maSp}`" style="text-decoration-line: none">{{
              item.tenSanPham
            }}</router-link>
          </h6>
          <!-- <div class="rating">
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                  <i class="fa fa-star"></i>
                </div> -->
          <div style="color: red" class="product__price">{{ item.khoangGia }}</div>
        </div>
      </div>
    </div>
  </div>
</template>


<style>
</style>