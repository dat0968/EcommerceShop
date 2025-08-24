<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { useCartStore } from '@/stores/cartStore'
const cartStore = useCartStore()
import { emitter } from '@/stores/eventBus'
import { formatCurrency } from '@/constants/formatCurrency'
const listCart = ref([])
const router = useRouter()
const getUrlAPI = ref(GetApiUrl())
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
async function fetchCart() {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    console.log(accessToken.value)
    console.log(refreshToken.value)
    router.push('/Login')
  } else {
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    const response = await fetch(`${getUrlAPI.value}/api/Cart/${readToken.IdUser}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })
    if (!response.ok) {
      throw new Error('Failed to fetchCart')
    }
    const result = await response.json()
    listCart.value = result
  }
}

onMounted(async () => {
  await fetchCart()
})

const tongTien = computed(() => {
  const selectedCartItems = listCart.value.filter((item) => selectedItems.value.includes(item.id))
  return selectedCartItems.reduce((total, item) => {
    return total + item.donGia * item.soLuong
  }, 0)
})

function validateQuantity(item) {
  let number = parseInt(item.soLuong)

  if (isNaN(number) || number < 1) {
    item.soLuong = 1
  } else if (number > item.soLuongToiDa) {
    item.soLuong = item.soLuongToiDa

    Swal.fire({
      title: `Chỉ còn lại ${item.soLuongToiDa} sản phẩm`,
      icon: 'warning',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
  } else {
    item.soLuong = number
  }
}

async function removeCart(id) {
  const response = await fetch(`${getUrlAPI.value}/api/Cart/${id}`, {
    method: 'DELETE',
    headers: {
      'Content-Type': 'application/json',
    },
  })
  if (!response.ok) {
    throw new Error('Failed to deleteCart')
  }
  const result = await response.json()
  if (result.success) {
    Swal.fire({
      title: `Đã xóa sản phẩm ra khỏi giỏ hàng`,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
    cartStore.deleteItemsCart(id)
    emitter.emit('cart-updated')
    await fetchCart()
  }
}
const selectedItems = ref([])

function toggleSelection(id) {
  const index = selectedItems.value.indexOf(id)
  if (index === -1) {
    selectedItems.value.push(id)
  } else {
    selectedItems.value.splice(index, 1)
  }
}

function confirmCart() {
  if (selectedItems.value.length === 0) {
    Swal.fire({
      title: `Vui lòng chọn sản phẩm bạn muốn đặt hàng!`,
      icon: 'warning',
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
    })
    return
  }
  const selectedCartItems = listCart.value.filter((item) => selectedItems.value.includes(item.id))

  cartStore.setSelectedItems(selectedCartItems)
  router.push('/checkout')
}
</script>
<template>
  <div>
    <!-- Shop Cart Section Begin -->
    <section class="shop-cart spad" style="margin-bottom: 200px">
      <div class="container">
        <div class="row">
          <div class="col-lg-9">
            <div class="shop__cart__table">
              <table>
                <thead>
                  <tr>
                    <th>Sản phẩm</th>
                    <th>Giá</th>
                    <th>Số lượng</th>
                    <th>Tổng tiền</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    :class="{ 'selected-row': selectedItems.includes(item.id) }"
                    @click="toggleSelection(item.id)"
                    style="cursor: pointer"
                    v-for="item in listCart"
                    :key="item.id"
                  >
                    <td class="cart__product__item">
                      <div class="tick-icon" v-if="selectedItems.includes(item.id)">✔</div>
                      <img
                        :src="
                          item.maCombo == null
                            ? `${getUrlAPI.replace('api/', '')}/HinhAnh/Products/${item.tenHinhAnh}`
                            : `${getUrlAPI.replace('api/', '')}/HinhAnh/AnhCombo/${item.tenHinhAnh}`
                        "
                        alt=""
                        class="cart-product-image"
                        @click="toggleSelection(item.id)"
                      />
                      <div class="cart__product__item__title">
                        <h6>{{ item.tenSanPham_TenCombo }}</h6>
                        <div v-if="!item.maCombo" class="product-options">
                          <span v-if="item.mau" class="option-label">Màu: {{ item.mau }}</span>
                          <span v-if="item.kichThuoc" class="option-label"
                            >Kích thước: {{ item.kichThuoc }}</span
                          >
                        </div>
                        <div v-else class="product-options">
                          <span
                            v-for="detailsCombo in item.giohangctcombos"
                            :key="detailsCombo.id"
                            class="option-label"
                          >
                            {{ detailsCombo.tenSanPham }} ({{ detailsCombo.mauSac }},
                            {{ detailsCombo.kichThuoc }})
                          </span>
                        </div>
                      </div>
                    </td>
                    <td class="cart__price">{{ formatCurrency(item.donGia) }}</td>
                    <td class="cart__quantity">
                      <div class="pro-qty">
                        <input
                          @input="() => validateQuantity(item)"
                          type="text"
                          v-model="item.soLuong"
                        />
                      </div>
                    </td>
                    <td class="cart__total">{{ formatCurrency(item.donGia * item.soLuong) }}</td>
                    <td class="cart__close">
                      <span @click="removeCart(item.id)" class="icon_close"></span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="col-lg-3">
            <div class="cart__total__procced">
              <h6>Tổng giá trị</h6>
              <ul>
                <li>
                  Tổng <span>{{ formatCurrency(tongTien) }}</span>
                </li>
              </ul>
              <button
                @click="confirmCart()"
                style="width: 100%; text-decoration-line: none"
                class="primary-btn"
              >
                Đặt hàng
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
  <!-- Shop Cart Section End -->
</template>



<style>
.cart-product-image {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 8px;
  margin-right: 10px;
}
.product-options {
  font-size: 14px;
  color: #666;
  margin-top: 4px;
}

.option-label {
  display: inline-block;
  margin-right: 10px;
}
.selected-row {
  border: 2px solid #28a745;
  background-color: #f0fff4;
}
.selected-row {
  position: relative;
}

.tick-icon {
  position: absolute;
  top: 2px;
  right: 2px;
  background-color: #28a745;
  color: white;
  font-weight: bold;
  border-radius: 50%;
  width: 20px;
  height: 20px;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
}
</style>
