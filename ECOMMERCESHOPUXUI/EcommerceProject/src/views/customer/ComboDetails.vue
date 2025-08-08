<script setup>
import CompareStorageHelper from '@/models/dtos/expansionModels/compareObject'
import { computed, watch, ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import RecomendationProduct from '@/components/RecommendationProduct/RecomendationProduct.vue'
import Swal from 'sweetalert2'
import ReviewProductCombo from '@/components/pages/customers/reviews/ReviewProductCombo.vue'
import { emitter } from '@/stores/eventBus'
const route = useRoute()
const getUrlAPI = ref(GetApiUrl())
const id = route.params.id
const combo = ref({})
const allImages = ref([])
const currentSlider = ref(1)
const colors = ref([])
const selectedVariants = ref([])
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const router = useRouter()
const quantity = ref('1')
const activeTab = ref('desc')

const readToken = ref({})
const fetchCombo = async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid) {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
  }
  const maKhachHang = readToken.value?.IdUser ?? null
  let url = `${getUrlAPI.value}/api/shop/Combo/${id}`
  if (maKhachHang != null) {
    url += `?maKh=${maKhachHang}`
  }
  const response = await fetch(
    url,
    {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    }
  )
  if (!response.ok) {
    throw new Error('Error fetchAPI Combo')
  }
  const result = await response.json()
  combo.value = {
    id: result.maCombo,
    name: result.tenCombo,
    image: result.hinh,
    quantityCombo: result.soLuong,
    description: result.moTa || 'Chưa có mô tả',
    phanTramGiam: result.phanTramGiam,
    soTienGiam: result.soTienGiam,
    chitietcombos: result.chitietcombos.map((ct) => ({
      id: ct.maSp,
      name: ct.tenSp,
      image: ct.sanPhamCTs[0]?.images[0] || '/images/default-product.jpg',
      quantity: ct.soLuongSp,
      variants: ct.sanPhamCTs,
      colors: [...new Set(ct.sanPhamCTs.map((pd) => pd.mauSac).filter(Boolean))],
      sizes: [...new Set(ct.sanPhamCTs.map((pd) => pd.kichThuoc).filter(Boolean))],
    })),
  }
  combo.value.chitietcombos.forEach((product, index) => {
    selectedVariants.value[index] = {
      color: product.colors[0] || '',
      size: availableSizes.value[index]?.[0] || '',
    }
  })
}
const availableSizes = computed(() => {
  return (
    combo.value.chitietcombos?.map((product, index) => {
      const selectedColor = selectedVariants.value[index]?.color || product.colors[0]
      const sizes = product.variants
        .filter((v) => v.mauSac === selectedColor && v.soLuongTon > 0)
        .map((v) => v.kichThuoc)
      return [...new Set(sizes)]
    }) || []
  )
})
const validateQuantity = () => {
  const value = quantity.value.trim()
  if (value === '') return
  const number = parseInt(quantity.value)
  if (isNaN(number) || number < 1) {
    quantity.value = '1'
  } else if (number > combo.value.quantityCombo) {
    quantity.value = combo.value.quantityCombo.toString()
  } else {
    quantity.value = number.toString()
  }
}
onMounted(async () => {
  await fetchCombo()
})

function selectedvariant(productIndex, type, value) {
  if (!selectedVariants.value[productIndex]) {
    selectedVariants.value[productIndex] = {}
  }
  selectedVariants.value[productIndex][type] = value
  const product = combo.value.chitietcombos[productIndex]
  if (type === 'color') {
    const defaultSize = availableSizes.value[productIndex]?.[0]
    if (defaultSize) {
      selectedVariants.value[productIndex].size = defaultSize
    }
  }
  const variant = product.variants.find(
    (v) =>
      v.mauSac === (selectedVariants.value[productIndex].color || product.colors[0]) &&
      v.kichThuoc ===
        (selectedVariants.value[productIndex].size || availableSizes.value[productIndex]?.[0])
  )
  if (!variant || variant.soLuongTon <= 0) {
    Swal.fire('Lỗi', 'Biến thể này không có sẵn hoặc đã hết hàng!', 'error')
  }
}

const OrginalPriceCombo = computed(() => {
  const chiTietCombos = combo.value?.chitietcombos || []
  var FindPrice = chiTietCombos.map((combo, index) => {
    const selectedColor = selectedVariants.value[index].color
    const selectedSize = selectedVariants.value[index].size
    const variant = combo.variants.find(
      (p) => p.kichThuoc == selectedSize && p.mauSac == selectedColor
    )
    return variant.donGia * combo.quantity
  })
  return FindPrice.reduce((total, num) => total + num, 0)
})

const PriceCombo = computed(() => {
  return (
    OrginalPriceCombo.value -
    ((combo.value.phanTramGiam * OrginalPriceCombo.value) / 100 || combo.value.soTienGiam)
  )
})

async function addToCart() {
  const value = quantity.value.trim()
  if (value === '') {
    Swal.fire({
      title: 'Không để trống số lượng',
      icon: 'error',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
    return
  }
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (!validatetoken.isValid) {
    router.push('/Login')
    return
  } else {
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    var content = {
      maKh: readToken.IdUser,
      maCtsp: null,
      maCombo: combo.value.id,
      soLuong: quantity.value,
      donGia: PriceCombo.value,
      giamGia: OrginalPriceCombo.value - PriceCombo.value,
      tenHinhAnh: combo.value.image,
      giohangctcombos: combo.value.chitietcombos.map((product, index) => {
        const selectedColor = selectedVariants.value[index]?.color
        const selectedSize = selectedVariants.value[index]?.size
        const variant = product.variants.find(
          (v) => v.mauSac === selectedColor && v.kichThuoc === selectedSize
        )
        return {
          maCtsp: variant?.maCtsp,
          soLuong: quantity.value * product.quantity,
          donGia: variant?.donGia || 0,
        }
      }),
    }

    const response = await fetch(`${getUrlAPI.value}/api/Cart`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(content),
    })
    const result = await response.json()
    if (!response.ok || !result.success) {
      Swal.fire({
        title: result.error || 'Đã xảy ra lỗi',
        icon: 'error',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
      return
    }
    if (result.success) {
      Swal.fire({
        title: 'Đã thêm sản phẩm vào giỏ hàng.',
        icon: 'success',
        timer: 2000,
        showConfirmButton: false,
        timerProgressBar: true,
      })
      emitter.emit('cart-updated')
    }
    console.log(content)
  }
}

function addComboToCompare() {
  CompareStorageHelper.addComboToCompare(combo.value, selectedVariants.value)
  Swal.fire({
    title: 'Đã thêm combo vào danh sách so sánh!',
    icon: 'success',
    timer: 1500,
    showConfirmButton: false,
    timerProgressBar: true,
  })
}
</script>
<template>
  <div>
    <!-- Combo Details Section Begin -->
    <section class="product-details spad">
      <div class="container">
        <div class="row">
          <div class="col-lg-6">
            <div class="product__details__pic">
              <div style="position: relative" class="product__details__slider__content">
                <div>
                  <img
                    class="product__big__img"
                    :src="`${getUrlAPI}/HinhAnh/AnhCombo/${combo.image}`"
                    alt=""
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="col-lg-6">
            <div class="product__details__text">
              <h3>
                {{ combo.name }} <span>Còn: {{ combo.quantityCombo }} phần</span>
              </h3>
              <div class="product__details__price">
                {{ PriceCombo }} VNĐ<span>{{ OrginalPriceCombo }} VNĐ</span>
              </div>
              <div class="product__details__button">
                <div class="quantity">
                  <span>Số lượng:</span>
                  <div class="pro-qty">
                    <input type="text" v-model="quantity" @input="validateQuantity" />
                  </div>
                </div>
                <div class="button-group">
                  <button style="height: 50px" href="#" class="cart-btn" @click.prevent="addToCart">
                    <span class="icon_bag_alt"></span> Thêm giỏ hàng
                  </button>
                  <button style="margin-bottom: 14px" class="action-buttons">
                    <a
                      href="#"
                      style="border-radius: 50%; width: 50px; height: 50px"
                      class="action-btn"
                      ><span class="icon_heart_alt"></span
                    ></a>
                    <a
                      href="#"
                      style="
                        height: 50px;
                        background: #1976d2;
                        color: #fff;
                        border-radius: 50px;
                        margin-left: 8px;
                      "
                      class="action-btn"
                      @click.prevent="addComboToCompare"
                      title="Thêm vào so sánh Combo"
                      ><span class="icon_adjust-horiz"></span
                    ></a>
                  </button>
                  <div style="margin-bottom: 14px" class="action-buttons">
                    <a
                      href="#"
                      style="border-radius: 50%; width: 50px; height: 50px"
                      class="action-btn"
                      ><span class="icon_heart_alt"></span
                    ></a>
                    <a
                      href="#"
                      style="border-radius: 50%; width: 50px; height: 50px"
                      class="action-btn"
                      ><span class="icon_adjust-horiz"></span
                    ></a>
                  </div>
                </div>
              </div>
              <div class="product__details__widget">
                <ul class="variant-list">
                  <li
                    v-for="(product, index) in combo.chitietcombos"
                    :key="index"
                    class="variant-section"
                  >
                    <div class="variant-title">
                      <h4>{{ product.name }}</h4>
                    </div>
                    <div class="variant-options">
                      <div class="variant-group">
                        <span class="variant-label">Màu sắc:</span>
                        <div class="color__checkbox">
                          <button
                            v-for="(color, colorIndex) in product.colors"
                            :key="colorIndex"
                            :class="[
                              'btn',
                              'btn-light',
                              { active: selectedVariants[index]['color'] === color },
                            ]"
                            @click="selectedvariant(index, 'color', color)"
                          >
                            {{ color }}
                          </button>
                        </div>
                      </div>
                      <div class="variant-group">
                        <span class="variant-label">Kích thước:</span>
                        <div class="size__checkbox">
                          <button
                            v-for="(size, sizeIndex) in availableSizes[index]"
                            :key="sizeIndex"
                            :class="[
                              'btn',
                              'btn-light',
                              { active: selectedVariants[index]['size'] === size },
                            ]"
                            @click="selectedvariant(index, 'size', size)"
                          >
                            {{ size }}
                          </button>
                        </div>
                      </div>
                    </div>
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <div class="col-lg-12">
            <div class="product__details__tab">
              <ul class="nav nav-tabs" role="tablist">
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: activeTab === 'desc' }"
                    href="#"
                    @click.prevent="activeTab = 'desc'"
                    >Mô tả</a
                  >
                </li>
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: activeTab === 'review' }"
                    href="#"
                    @click.prevent="activeTab = 'review'"
                    >Đánh giá</a
                  >
                </li>
              </ul>
              <div class="tab-content vh-100 overflow-auto">
                <div
                  v-show="activeTab == 'desc'"
                  class="tab-pane"
                  :class="{ active: activeTab === 'desc' }"
                  id="tabs-1"
                  role="tabpanel"
                >
                  <p>
                    {{ combo.description }}
                  </p>
                </div>
                <div
                  v-show="activeTab == 'review'"
                  class="tab-pane"
                  :class="{ active: activeTab === 'review' }"
                  id="tabs-2"
                  role="tabpanel"
                >
                  <ReviewProductCombo :objectId="id" :isProduct="false" />
                </div>
              </div>
            </div>
          </div>

          <RecomendationProduct />
        </div>
      </div>
    </section>
    <!-- Combo Details Section End -->
  </div>
</template>

<script>
export default {
  name: 'ComboDetails',
}
</script>

<style scoped>
.product__details__pic {
  position: relative;
}

.product__big__img {
  width: 100%;
  height: 500px;
  object-fit: cover;
}

.carousel-item img {
  object-fit: cover;
  max-height: 150px;
}

.product__details__text h3 {
  font-size: 24px;
  margin-bottom: 20px;
}

.product__details__price {
  font-size: 30px;
  color: #e94560;
  margin-bottom: 20px;
}

.product__details__price span {
  font-size: 20px;
  color: #999;
  text-decoration: line-through;
  margin-left: 10px;
}

.product__details__button {
  display: flex;
  align-items: center;
  margin-bottom: 30px;
  gap: 1.5rem;
}

.quantity {
  display: flex;
  align-items: center;
}

.quantity span {
  margin-right: 10px;
}

.button-group {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 6px;
  color: #666;
  transition: all 0.3s ease;
}

.action-btn:hover {
  background-color: #e9ecef;
  color: #333;
}

.cart-btn {
  padding: 12px 25px;
  background-color: #ca1515;
  color: white;
  border-radius: 6px;
  text-decoration: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  border-radius: 50px;
}

.cart-btn:hover {
  background-color: #ca1515;
  color: white;
}

.product__details__widget {
  margin-top: 2rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.product__details__widget ul {
  margin: 0;
  padding: 0;
}

.variant-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.variant-section {
  margin-bottom: 2.5rem;
  padding: 1.5rem;
  border: 1px solid #eee;
  border-radius: 8px;
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.variant-title {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.variant-title h4 {
  font-size: 1.2rem;
  color: #333;
  margin: 0;
  font-weight: 600;
}

.variant-options {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.variant-group {
  display: flex;
  align-items: flex-start;
  gap: 1.5rem;
}

.variant-label {
  min-width: 120px;
  font-weight: 500;
  color: #666;
  padding-top: 0.5rem;
}

.color__checkbox,
.size__checkbox {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  flex: 1;
}

.btn {
  padding: 6px 12px;
  transition: all 0.3s ease;
  background-color: #f8f9fa;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  font-weight: 500;
  font-size: 0.9rem;
  min-width: 60px;
}

.btn:hover {
  background-color: #e9ecef;
  border-color: #ced4da;
}

.btn.active {
  background-color: #4a90e2 !important;
  color: white !important;
  border-color: #357ab8 !important;
}

.product__details__tab {
  margin-top: 50px;
}

.nav-tabs {
  border-bottom: 1px solid #ddd;
}

.nav-tabs .nav-link {
  border: none;
  color: #999;
  padding: 10px 20px;
}

.nav-tabs .nav-link.active {
  color: #ca1515;
  border-bottom: 2px solid #ca1515;
}

.tab-content {
  padding: 20px 0;
}
</style>
