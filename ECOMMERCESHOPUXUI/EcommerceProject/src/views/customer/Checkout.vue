<script setup>
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cartStore'
import { ref, computed, onMounted, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { read } from 'xlsx'
import { formatCurrency } from '@/constants/formatCurrency'
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
const couponCode = ref('')
const useSavedAddress = ref(false)
const addresses = ref([])
const getUrlAPI = ref(GetApiUrl())
const payment = ref('COD')
const loading = ref(false)
const selectedAddress = ref('')
const userInfo = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  moTa: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
})
async function fetchMyaddress() {
  try {
    loading.value = true
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (!validatetoken.isValid) {
      router.push('/Login')
      return
    }
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    const response = await fetch(getUrlAPI.value + `/api/Address/${readToken.IdUser}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + accessToken.value,
      },
    })
    if (!response.ok) {
      if (response.status >= 400 && response.status <= 403) {
        router.push('/Login')
        return
      } else {
        throw new Error('HandleGetAddress Failed')
      }
    }
    const result = await response.json()
    addresses.value = result
    addresses.value.forEach(async (element) => {
      if (element.macDinh) {
        userInfo.value.hoTen = element.hoten
        userInfo.value.soDienThoai = element.sdt
        userInfo.value.diaChi = element.diachichitiet
        const province = provinces.value.find(
          (p) => p.ProvinceName.toLowerCase() === element.tinh.toLowerCase()
        )

        userInfo.value.provinceId = province?.ProvinceID || ''
        await fetchDistrict()
        const district = districts.value.find(
          (p) => p.DistrictName.toLowerCase() === element.quanHuyen.toLowerCase()
        )

        userInfo.value.districtId = district?.DistrictID || ''
        await fetchWard()
        const ward = wards.value.find(
          (p) => p.WardName.toLowerCase() === element.xaPhuong.toLowerCase()
        )
        userInfo.value.wardCode = ward?.WardCode || ''
      }
    })
  } catch (error) {
    console.log(error)
  } finally {
    loading.value = false
  }
}

const isOpenModelAddress = ref(false)
const errors = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
  payment: '',
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
    console.log(districts.value)
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
    return
  } else {
    accessToken.value = validatetoken.newAccessToken
    readToken.value = decodeToken(accessToken.value)
    userInfo.value.hoTen = readToken.value.Name
    userInfo.value.soDienThoai = readToken.value.Phone
  }
  await fetchProvince()
  await fetchMyaddress()
})

selectedItems.value = cartStore.selectedItems
if (selectedItems.value.length <= 0) {
  Swal.fire({
    title: `Vui lòng quay lại giỏ hàng chọn sản phẩm bạn muốn đặt hàng!`,
    icon: 'warning',
    timer: 2000,
    showConfirmButton: false,
    timerProgressBar: true,
  })
  router.push('/cart')
}
const tongTien = computed(() => {
  return selectedItems.value.reduce((total, item) => {
    return total + item.donGia * item.soLuong
  }, 0)
})

watch(useSavedAddress, (newValue) => {
  if (newValue) {
    isOpenModelAddress.value = true
  } else {
    isOpenModelAddress.value = false
  }
})
async function saveAddress() {
  try {
    loading.value = true;
    userInfo.value.hoTen = selectedAddress.value.hoten
    userInfo.value.soDienThoai = selectedAddress.value.sdt
    userInfo.value.diaChi = selectedAddress.value.diachichitiet
    const province = provinces.value.find(
      (p) => p.ProvinceName.toLowerCase() === selectedAddress.value.tinh.toLowerCase()
    )

    userInfo.value.provinceId = province?.ProvinceID || ''
    await fetchDistrict()
    const district = districts.value.find(
      (p) => p.DistrictName.toLowerCase() === selectedAddress.value.quanHuyen.toLowerCase()
    )

    userInfo.value.districtId = district?.DistrictID || ''
    await fetchWard()
    const ward = wards.value.find(
      (p) => p.WardName.toLowerCase() === selectedAddress.value.xaPhuong.toLowerCase()
    )
    userInfo.value.wardCode = ward?.WardCode || ''
    useSavedAddress.value = false
  } catch (error) {
    console.log(error)
  }finally{
    loading.value = false;
  }
}
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
        shop_id: 5715364,
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
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (!validatetoken.isValid) {
      router.push('/Login')
      return
    }
    accessToken.value = validatetoken.newAccessToken
    const readToken = decodeToken(accessToken.value)
    if (couponCode.value == '') {
      discount.value = 0
      couponCode.value = ''
      return
    }
    const response = await fetch(
      getUrlAPI.value +
        `/api/Checkout/GetDiscountCoupon?maUser=${readToken.IdUser}&&couponcode=${couponCode.value}&&originalPrice=${tongTien.value}`,
      {
        headers: {
          'Content-type': 'application/json',
          Authorization: `Bearer ${accessToken.value}`,
        },
      }
    )
    if (!response.ok) {
      if (response.status >= 400 && response.status <= 403) {
        router.push('/Login')
        return
      } else {
        const errorMessage = response.message
        throw new Error(errorMessage)
      }
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

async function HandlePayment() {
  // Reset errors
  errors.value = {
    hoTen: '',
    soDienThoai: '',
    diaChi: '',
    provinceId: '',
    districtId: '',
    wardCode: '',
    payment: '',
  }

  // Validate
  let isValid = true

  // Validate họ tên
  if (!userInfo.value.hoTen.trim()) {
    errors.value.hoTen = 'Vui lòng nhập họ tên'
    isValid = false
  }

  // Validate số điện thoại
  const phoneRegex = /^[0-9]{10}$/
  if (!userInfo.value.soDienThoai.trim()) {
    errors.value.soDienThoai = 'Vui lòng nhập số điện thoại'
    isValid = false
  } else if (!phoneRegex.test(userInfo.value.soDienThoai.trim())) {
    errors.value.soDienThoai = 'Số điện thoại không hợp lệ'
    isValid = false
  }

  // Validate địa chỉ
  if (!userInfo.value.diaChi.trim()) {
    errors.value.diaChi = 'Vui lòng nhập địa chỉ chi tiết'
    isValid = false
  }

  // Validate tỉnh/thành phố
  if (!userInfo.value.provinceId) {
    errors.value.provinceId = 'Vui lòng chọn tỉnh/thành phố'
    isValid = false
  }

  // Validate quận/huyện
  if (!userInfo.value.districtId) {
    errors.value.districtId = 'Vui lòng chọn quận/huyện'
    isValid = false
  }

  // Validate phường/xã
  if (!userInfo.value.wardCode) {
    errors.value.wardCode = 'Vui lòng chọn phường/xã'
    isValid = false
  }

  // Validate phương thức thanh toán
  if (!payment.value) {
    errors.value.payment = 'Vui lòng chọn phương thức thanh toán'
    isValid = false
  }

  if (!isValid) {
    Swal.fire({
      title: 'Vui lòng kiểm tra lại thông tin',
      icon: 'warning',
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
  }
  accessToken.value = validatetoken.newAccessToken
  const readToken = decodeToken(accessToken.value)

  if (discount.value == 0 && couponCode.value != '') {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi!',
      text: 'Bạn chưa xác nhận áp dụng mã Coupon, vui lòng bấm nút "ÁP DỤNG" để áp dụng mã.',
    })
    return
  }

  const provinceName = provinces.value.filter((p) => p.ProvinceID == userInfo.value.provinceId)[0]
    .ProvinceName
  const districtName = districts.value.filter((p) => p.DistrictID == userInfo.value.districtId)[0]
    .DistrictName
  const wardName = wards.value.filter((p) => p.WardCode == userInfo.value.wardCode)[0].WardName
  const content = {
    maKh: readToken.IdUser,
    maCode: couponCode.value,
    diaChiNhanHang: userInfo.value.diaChi + ' ' + wardName + ' ' + districtName + provinceName,
    hinhThucTt: payment.value,
    moTa: userInfo.value.moTa,
    hoTen: userInfo.value.hoTen,
    sdt: userInfo.value.soDienThoai,
    phiVanChuyen: shippingFee.value,
    tienGoc: tongTien.value,
    giamGia: discount.value,
    gioHangId: selectedItems.value.map((item) => item.id),
    chitietcombohoadons: selectedItems.value.flatMap((item) =>
      (item.giohangctcombos || []).map((ct) => ({
        maCtsp: ct.maCtsp,
        maCombo: item.maCombo,
        soLuong: ct.soLuong,
        donGia: ct.donGia,
      }))
    ),
    cthoadons: selectedItems.value.map((item) => ({
      maCtsp: item.maCtsp,
      maCombo: item.maCombo,
      soLuong: item.soLuong,
      gia: item.donGia,
      giamGia: item.giamGia,
    })),
  }
  const confirmResult = await Swal.fire({
    title: 'Xác nhận đặt hàng?',
    text: 'Bạn có chắc muốn tiến hành thanh toán đơn hàng này?',
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Đồng ý',
    cancelButtonText: 'Hủy',
    reverseButtons: true,
  })

  if (confirmResult.isConfirmed) {
    try {
      loading.value = true;
      if (payment.value.toLowerCase() == 'cod') {
        const response = await fetch(`${getUrlAPI.value}/api/Checkout`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(content),
        })
        if (!response.ok) {
          if (response.status >= 400 && response.status <= 403) {
            router.push('/Login')
            return
          } else {
            throw new Error('HandlePayment Failed')
          }
        }
        const result = await response.json()
        if (result.success) {
          Swal.fire({
            title: 'Đặt hàng thành công',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
          })
          cartStore.clearCart()
          router.push('/order')
        } else {
          Swal.fire('Thất bại', result.message || 'Không thể đặt hàng', 'error')
        }
      } else if (payment.value.toLowerCase() == 'vnpay') {
        const CreatePaymentUrl = await fetch(getUrlAPI.value + `/api/VNPAY/CreatePaymentUrl`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(content),
        })
        const responseVNPAY = await CreatePaymentUrl.text()
        if (!CreatePaymentUrl.ok) {
          throw new Error(responseVNPAY)
        }
        window.location.href = responseVNPAY
      }
    } catch (error) {
      loading.value = false;
      Swal.fire('Lỗi', error.message || 'Đã xảy ra lỗi khi xử lý thanh toán', 'error')
    }
  } else {
    Swal.fire('Đã hủy', 'Đơn hàng chưa được thanh toán', 'info')
  }
}
const selectedAddressId = ref(null)
async function selectAddress(dc) {
  selectedAddressId.value = dc.id
  selectedAddress.value = dc
}
</script>
<template>
  <div>
    <div v-if="loading" class="spinner-overlay">
      <div class="spinner"></div>
    </div>

    <div
      class="modal show"
      tabindex="-1"
      role="dialog"
      style="
        display: flex;
        align-items: center;
        justify-content: center;
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.5);
        z-index: 1050;
      "
      v-if="isOpenModelAddress"
    >
      <div class="modal-dialog" role="document">
        <div class="modal-content">
          <!-- Header -->
          <div class="modal-header">
            <h6 class="modal-title">Địa chỉ đã lưu</h6>
            <button
              type="button"
              aria-label="Close"
              style="
                background: none;
                border: none;
                font-size: 20px;
                color: #333;
                cursor: pointer;
                margin-left: auto;
              "
              @click="useSavedAddress = false"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>

          <!-- Body -->
          <div class="modal-body">
            <div class="container mt-4">
              <div
                v-for="dc in addresses"
                :key="dc.id"
                class="border rounded p-3 mb-3"
                :style="{
                  backgroundColor: selectedAddressId === dc.id ? '#d4edda' : 'white',
                  borderColor: selectedAddressId === dc.id ? '#28a745' : '#dee2e6',
                  cursor: 'pointer',
                }"
                @click="selectAddress(dc)"
              >
                <div class="d-flex justify-content-between">
                  <div>
                    <strong>{{ dc.hoten }}</strong>
                    <span class="ms-2 text-muted">{{ dc.sdt }}</span>
                  </div>
                </div>

                <div class="mt-2 text-muted">
                  {{ dc.diachichitiet }}
                </div>

                <div class="mt-2">
                  <span v-if="dc.macDinh" class="badge bg-light text-danger border">Mặc định</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" @click="saveAddress()">Lưu</button>
          </div>
        </div>
      </div>
    </div>

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
                    <input
                      v-model="userInfo.hoTen"
                      type="text"
                      :class="{ 'is-invalid': errors.hoTen }"
                    />
                    <div class="invalid-feedback">{{ errors.hoTen }}</div>
                  </div>
                </div>
                <div class="checkout__form__input">
                  <p>Số điện thoại <span>*</span></p>
                  <input
                    v-model="userInfo.soDienThoai"
                    type="text"
                    :class="{ 'is-invalid': errors.soDienThoai }"
                  />
                  <div class="invalid-feedback">{{ errors.soDienThoai }}</div>
                </div>
                <div class="checkout__form__input">
                  <p>Tỉnh/Thành phố <span>*</span></p>
                  <select
                    class="form-control"
                    v-model="userInfo.provinceId"
                    :class="{ 'is-invalid': errors.provinceId }"
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
                  <div class="invalid-feedback">{{ errors.provinceId }}</div>
                </div>
                <div style="margin-top: 20px" class="checkout__form__input">
                  <p>Quận/Huyện<span>*</span></p>
                  <select
                    class="form-control"
                    v-model="userInfo.districtId"
                    :class="{ 'is-invalid': errors.districtId }"
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
                  <div class="invalid-feedback">{{ errors.districtId }}</div>
                </div>
                <div style="margin-top: 20px" class="checkout__form__input">
                  <p>Phường/Xã<span>*</span></p>
                  <select
                    class="form-control"
                    @change="CalculateFee()"
                    :class="{ 'is-invalid': errors.wardCode }"
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
                  <div class="invalid-feedback">{{ errors.wardCode }}</div>
                </div>
                <div style="margin-top: 20px" class="checkout__form__input">
                  <p>Địa chỉ chi tiết <span>*</span></p>
                  <input
                    v-model="userInfo.diaChi"
                    type="text"
                    :class="{ 'is-invalid': errors.diaChi }"
                  />
                  <div class="invalid-feedback">{{ errors.diaChi }}</div>
                </div>
                <label>
                  <input type="checkbox" v-model="useSavedAddress" />
                  Sử dụng địa chỉ được lưu sẵn
                </label>
                <div style="margin-top: 20px" class="checkout__form__input">
                  <p>Mô tả</p>
                  <textarea
                    v-model="userInfo.moTa"
                    style="width: 100%; border: 1px solid #e1e1e1"
                  />
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
                      {{ item.tenSanPham_TenCombo }}
                      <span style="color: #ca1515">{{ formatCurrency(item.donGia * item.soLuong) }} </span>
                    </li>
                  </ul>
                </div>
                <div class="checkout__order__total">
                  <ul>
                    <li>
                      Tạm tính <span> {{ formatCurrency(tongTien) }} </span>
                    </li>
                    <li>
                      Phí ship <span> {{ formatCurrency(shippingFee) }} </span>
                    </li>
                    <li>
                      Giảm giá <span> {{ formatCurrency(discount) }} </span>
                    </li>
                    <li>
                      Tổng tiền <span>{{ formatCurrency(tongTien + shippingFee - discount) }} </span>
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
                        v-model="couponCode"
                      />
                      <button
                        type="button"
                        @click="applyCoupon()"
                        class="site-btn"
                        style="white-space: nowrap"
                      >
                        Áp dụng
                      </button>
                    </div>
                  </div>
                </div>

                <div class="checkout__order__widget" :class="{ 'is-invalid': errors.payment }">
                  <label for="check-payment">
                    COD
                    <input
                      v-model="payment"
                      name="payment-method"
                      type="radio"
                      id="check-payment"
                      value="COD"
                    />
                    <span class="checkmark"></span>
                  </label>
                  <label for="paypal">
                    VNPAY
                    <input
                      v-model="payment"
                      name="payment-method"
                      type="radio"
                      id="paypal"
                      value="VNPAY"
                    />
                    <span class="checkmark"></span>
                  </label>
                  <div class="invalid-feedback">{{ errors.payment }}</div>
                </div>
                <button @click="HandlePayment()" type="button" class="site-btn">Đặt hàng</button>
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
.spinner {
  border: 4px solid #f3f3f3; /* màu viền nhạt */
  border-top: 4px solid #3498db; /* màu viền trên quay vòng */
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 0.8s linear infinite;
  margin: auto;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.spinner-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.8);
  z-index: 9999;
  display: flex;
  justify-content: center;
  align-items: center;
}

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
  padding: 8px 16px;
  white-space: nowrap;
  width: auto !important;
  display: inline-block !important;
}

.is-invalid {
  border-color: #dc3545 !important;
}

.invalid-feedback {
  display: block;
  width: 100%;
  margin-top: 0.25rem;
  font-size: 0.875em;
  color: #dc3545;
}

.checkout__form__input .invalid-feedback {
  margin-top: 5px;
}

.checkout__order__widget.is-invalid {
  border: 1px solid #dc3545;
  padding: 10px;
  border-radius: 4px;
}
</style>