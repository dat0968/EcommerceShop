<script setup>
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { ref, computed, onMounted } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
const cartStore = useCartStore()
const selectedItems = ref([])
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const readToken = ref({})
const router = useRouter()
const provinces = ref([])
const districts = ref([])
const wards = ref([])
const token = 'eb507c61-0fad-11f0-9aa0-bece206412cb'
const shippingFee = ref(0)
const discount = ref(0)
const couponCode = ref(0)
const userInfo = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  moTa: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
})

async function fetchProvince() {
  const responseProvince = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/master-data/province`,
    {
      method: 'GET',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
    }
  )
  const resultProvince = await responseProvince.json()
  if (resultProvince.code === 200) {
    provinces.value = resultProvince.data
  }
}
async function fetchDistrict() {
  const responseDistrict = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/master-data/district`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
      body: JSON.stringify({
        province_id: userInfo.value.provinceId,
      }),
    }
  )
  const resultDistrict = await responseDistrict.json()
  if (resultDistrict.code === 200) {
    districts.value = resultDistrict.data
    wards.value = []
  }
}
async function fetchWard() {
  const responseWard = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/master-data/ward`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
      body: JSON.stringify({
        district_id: userInfo.value.districtId,
      }),
    }
  )
  const resultWard = await responseWard.json()
  if (resultWard.code === 200) {
    wards.value = resultWard.data
  }
}

onMounted(async () => {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/Login')
  } else {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
  }
  await fetchProvince()
})

selectedItems.value = cartStore.selectedItems
const tongTien = computed(() => {
  return selectedItems.value.reduce((total, item) => {
    return total + item.donGia * item.soLuong
  }, 0)
})

const CalculateFee = async () => {
  let service = 0
  const fetchService = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/available-services`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
      body: JSON.stringify({
        Token: `${token}`,
        from_district: 1552,
        to_district: userInfo.value.districtId,
        shop_id: 5715364
      }),
    }
  )
  const resultService = await fetchService.json()
  if (resultService.code === 200) {
    service = resultService.data[0].service_id
  }

  const content = {
    from_district_id: 1552,
    from_ward_code: '400103', // 400103 là WardCode của phường Tân An - BMT, đây là địa điểm của cửa hàng
    service_id: service,
    service_type_id: null,
    to_district_id: userInfo.value.districtId,
    to_ward_code: userInfo.value.wardCode,
    weight: 200,
    insurance_value: 10000,
    cod_failed_amount: 2000,
  }
  const fetchAPIFee = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
        ShopId: '5715364',
      },
      body: JSON.stringify(content),
    }
  )
  const result = await fetchAPIFee.json()
  if (result.code === 200) {
    shippingFee.value = result.data.total
  }
}

const applyCoupon = async () => {
  try {
    const validateToken = await validateToken(accessToken.value, refreshToken.value)
    if(!validateToken){
      router.push('/Login')
      return;
    }
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    if(couponCode.value == ''){
      discount.value = 0
      couponCode.value = ''
      return;
    }
    const response = await fetch(
      GetApiUrl+`/api/Checkout/GetDiscountCoupon?maUser=${readToken.IdUser}&&couponcode=${couponCode.value}&&originalPrice=${tongTien}`,
      {
        headers: {
          'Content-type': 'application/json',
          Authorization: `Bearer ${accessToken.value}`,
        },
      }
    )
    if (response.status == 401) {
      Swal.fire({
        icon: 'error',
        title: 'Phiên của bạn đã hết hoặc bạn chưa đăng nhập, vui lòng đăng nhập lại!',
        timer: 2000,
        showConfirmButton: false,
      })
      router.push('/Login')
      return
    }
    if (!response.ok) {
      const errorMessage = response.message
      throw new Error(errorMessage)
    }
    const result = await response.json()
    if (result.success) {
      discount.value = result.discount
      Swal.fire(result.message, '', 'success')
    } else {
      Swal.fire(result.message, '', 'error')
      discount.value = 0
      couponCode.value = ''
    }
  } catch (error) {
    console.error('Lỗi khi áp dụng mã coupon:', error)
    couponCode.value = ''
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Đã xảy ra lỗi. Vui lòng thử lại.',
    })
  }
}
</script>
<template>
  <div>
    <!-- Checkout Section Begin -->
    <section class="checkout spad">
      <div class="container">
        <form action="#" class="checkout__form">
          <div class="row">
            <div class="col-lg-8">
              <h5>Thông tin đơn hàng</h5>
              <div class="row">
                <div class="col-lg-12">
                  <div class="checkout__form__input">
                    <p>Họ và tên <span>*</span></p>
                    <input :value="readToken.Name" type="text" />
                  </div>
                </div>
                <div class="checkout__form__input">
                  <p>Số điện thoại <span>*</span></p>
                  <input :value="readToken.Phone" type="text" />
                </div>
                <div class="checkout__form__input">
                  <p>Tỉnh/Thành phố <span>*</span></p>
                  <select
                    class="form-control"
                    v-model="userInfo.provinceId"
                    style="
                      background-color: white;
                      height: 50px;
                      border: 1px solid #e1e1e1;
                      border-radius: 2px;
                    "
                    @change="fetchDistrict()"
                  >
                    <option value="" disabled>Chọn tỉnh/thành phố</option>
                    <option
                      v-for="province in provinces"
                      :key="province.ProvinceID"
                      :value="province.ProvinceID"
                    >
                      {{ province.ProvinceName }}
                    </option>
                  </select>
                </div>
                <div style="margin-top: 20px;" class="checkout__form__input">
                  <p>Quận/Huyện<span>*</span></p>
                  <select
                    class="form-control"
                    v-model="userInfo.districtId"
                    style="
                      background-color: white;
                      height: 50px;
                      border: 1px solid #e1e1e1;
                      border-radius: 2px;
                    "
                    @change="fetchWard()"
                  >
                    <option value="" disabled>Chọn quận/huyện</option>
                    <option
                      v-for="district in districts"
                      :key="district.DistrictID"
                      :value="district.DistrictID"
                    >
                      {{ district.DistrictName }}
                    </option>
                  </select>
                </div>
                <div style="margin-top: 20px;" class="checkout__form__input">
                  <p>Phường/Xã<span>*</span></p>
                  <select
                    class="form-control"
                    @change="CalculateFee()"
                    style="
                      background-color: white;
                      height: 50px;
                      border: 1px solid #e1e1e1;
                      border-radius: 2px;
                    "
                    v-model="userInfo.wardCode"
                  >
                    <option value="" disabled>Chọn phường/xã</option>
                    <option v-for="ward in wards" :key="ward.WardCode" :value="ward.WardCode">
                      {{ ward.WardName }}
                    </option>
                  </select>
                </div>
                <div style="margin-top: 20px;" class="checkout__form__input">
                  <p>Mô tả</p>
                  <textarea style="width: 100%; border: 1px solid #e1e1e1" />
                </div>
              </div>
            </div>
            <div class="col-lg-4">
              <div class="checkout__order">
                <h5>Đơn hàng của bạn</h5>
                <div class="checkout__order__product">
                  <ul>
                    <li>
                      <span class="top__text">SẢN PHẨM</span>
                      <span class="top__text__right">TỔNG GIÁ TRỊ</span>
                    </li>
                    <li v-for="item in selectedItems" :key="item.id">
                      {{ item.tenSanPham }}
                      <span style="color: #ca1515">{{ item.donGia * item.soLuong }} VNĐ</span>
                    </li>
                  </ul>
                </div>
                <div class="checkout__order__total">
                  <ul>
                    <li>
                      Tạm tính <span> {{ tongTien }} VNĐ</span>
                    </li>
                    <li>
                      Phí ship <span> {{ shippingFee }} VNĐ</span>
                    </li>
                    <li>
                      Giảm giá <span> {{ 0 }} VNĐ</span>
                    </li>
                    <li>
                      Tổng tiền <span>{{ tongTien + shippingFee }} VNĐ</span>
                    </li>
                  </ul>
                </div>
                <div class="col-lg-12" style="margin-bottom: 30px">
                  <div class="discount__content">
                    <h6>Nhập mã coupon (nếu có)</h6>
                    <div style="display: flex; gap: 10px">
                      <input
                        type="text"
                        placeholder="Nhập mã coupon"
                        style="flex: 1; min-width: 0; border: 1px solid #e1e1e1"
                      />
                      <button type="button" @click="applyCoupon()" class="site-btn" style="white-space: nowrap">
                        Áp dụng
                      </button>
                    </div>
                  </div>
                </div>

                <div class="checkout__order__widget">
                  <label for="check-payment">
                    COD
                    <input type="checkbox" id="check-payment" />
                    <span class="checkmark"></span>
                  </label>
                  <label for="paypal">
                    VNPAY
                    <input type="checkbox" id="paypal" />
                    <span class="checkmark"></span>
                  </label>
                </div>
                <button type="submit" class="site-btn">Đặt hàng</button>
              </div>
            </div>
          </div>
        </form>
      </div>
    </section>
    <!-- Checkout Section End -->
  </div>
</template>


<style scoped>
.discount__content > div {
  display: flex;
  gap: 10px;
  align-items: center;
}

.discount__content input {
  flex: 2;
  min-width: 0;
}

.discount__content .site-btn {
  flex: 0 0 auto;
  padding: 8px 16px; /* tuỳ chỉnh độ lớn nút */
  white-space: nowrap;
  width: auto !important;
  display: inline-block !important;
}
.discount__content > div {
  display: flex;
  gap: 10px;
  align-items: center;
}

.discount__content input {
  flex: 2;
  min-width: 0;
  border-radius: 8px; /* Bo góc mượt */
  padding: 8px 12px;
  border: 1px solid #ccc;
}

.discount__content .site-btn {
  flex: 0 0 auto;
  padding: 8px 16px;
  white-space: nowrap;
  border-radius: 8px; /* Bo góc cho nút */
}
</style>