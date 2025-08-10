<template>
  <div
    class="modal fade"
    id="addAddressModal"
    tabindex="-1"
    aria-labelledby="addAddressModalLabel"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <form id="addressForm">
          <div class="modal-header">
            <h5 class="modal-title" id="addAddressModalLabel">Thêm địa chỉ mới</h5>
            <button
              type="button"
              class="btn-close"
              data-bs-dismiss="modal"
              aria-label="Đóng"
            ></button>
          </div>
          <div class="modal-body">
            <div class="row g-3">
              <div class="col-md-6">
                <label for="hoten" class="form-label">Họ tên</label>
                <input
                  v-model="userInfo.hoTen"
                  type="text"
                  class="form-control"
                  id="hoten"
                  required
                  :class="{ 'is-invalid': errors.hoTen }"
                />
                <div class="invalid-feedback">{{ errors.hoTen }}</div>
              </div>
              <div class="col-md-6">
                <label for="sdt" class="form-label">Số điện thoại</label>
                <input
                  v-model="userInfo.soDienThoai"
                  type="text"
                  class="form-control"
                  id="sdt"
                  required
                  :class="{ 'is-invalid': errors.soDienThoai }"
                />
                <div class="invalid-feedback">{{ errors.soDienThoai }}</div>
              </div>
              <div class="col-md-4">
                <label for="tinh" class="form-label">Tỉnh/Thành phố</label>
                <select
                  class="form-control"
                  :class="{ 'is-invalid': errors.provinceId }"
                  @change="fetchDistrict()"
                  v-model="userInfo.provinceId"
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
              <div class="col-md-4">
                <label for="quanHuyen" class="form-label">Quận/Huyện</label>
                <select
                  class="form-control"
                  :class="{ 'is-invalid': errors.districtId }"
                  @change="fetchWard()"
                  v-model="userInfo.districtId"
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
              <div class="col-md-4">
                <label for="xaPhuong" class="form-label">Xã/Phường</label>
                <select
                  class="form-control"
                  @change="CalculateFee()"
                  :class="{ 'is-invalid': errors.wardCode }"
                  v-model="userInfo.wardCode"
                >
                  <option value="" disabled>Chọn phường/xã</option>
                  <option v-for="ward in wards" :key="ward.WardCode" :value="ward.WardCode">
                    {{ ward.WardName }}
                  </option>
                </select>
                <div class="invalid-feedback">{{ errors.wardCode }}</div>
              </div>
              <div class="col-12">
                <label for="diachichitiet" class="form-label">Địa chỉ chi tiết</label>
                <input
                  v-model="userInfo.diaChi"
                  :class="{ 'is-invalid': errors.diaChi }"
                  class="form-control"
                  id="diachichitiet"
                  rows="2"
                  required
                />
                <div class="invalid-feedback">{{ errors.diaChi }}</div>
              </div>
              <div class="col-12">
                <div class="form-check">
                  <input
                    v-model="userInfo.defaultPick"
                    class="form-check-input"
                    type="checkbox"
                    id="macDinh"
                  />
                  <label class="form-check-label" for="macDinh"> Đặt làm địa chỉ mặc định </label>
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" @click="handleCreateAddress()" class="btn btn-success">
              Lưu địa chỉ
            </button>
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Huỷ</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { useRouter } from 'vue-router'
const router = useRouter()
const provinces = ref([])
const districts = ref([])
const wards = ref([])
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const getUrlAPI = ref(GetApiUrl())
const token = 'eb507c61-0fad-11f0-9aa0-bece206412cb'
const errors = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
  payment: '',
})
const userInfo = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
  defaultPick: false,
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
  await fetchProvince()
})

async function handleCreateAddress() {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (!validatetoken.isValid) {
    router.push('/Login')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  const readToken = decodeToken(accessToken.value)
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
  const provinceName = provinces.value.filter((p) => p.ProvinceID == userInfo.value.provinceId)[0]
    .ProvinceName
  const districtName = districts.value.filter((p) => p.DistrictID == userInfo.value.districtId)[0]
    .DistrictName
  const wardName = wards.value.filter((p) => p.WardCode == userInfo.value.wardCode)[0].WardName
  const content = {
    tinh: provinceName,
    quanHuyen: districtName,
    xaPhuong: wardName,
    diachichitiet: userInfo.value.diaChi + ' ' + wardName + ' ' + districtName + ' ' + provinceName,
    macDinh: userInfo.value.defaultPick,
    hoten: userInfo.value.hoTen,
    sdt: userInfo.value.soDienThoai,
    maKh: readToken.IdUser,
  }
  const confirmResult = await Swal.fire({
    title: 'Xác nhận thêm địa chỉ mới này?',
    text: 'Bạn có chắc muốn thêm địa chỉ này?',
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Đồng ý',
    cancelButtonText: 'Hủy',
    reverseButtons: true,
  })

  if (confirmResult.isConfirmed) {
    try {
      const response = await fetch(`${getUrlAPI.value}/api/Address`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + accessToken.value,
        },
        body: JSON.stringify(content),
      })
      if (!response.ok) {
        if (response.status >= 400 && response.status <= 403) {
          router.push('/Login')
          return
        } else {
          throw new Error('HandleCreateAddress Failed')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Thêm địa chỉ mới thành công',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        window.location.reload()
      } else {
        Swal.fire('Thất bại', result.message || 'Không thể thêm địa chỉ', 'error')
      }
    } catch (error) {
      Swal.fire('Lỗi', error.message || 'Đã xảy ra lỗi khi xử lý thêm địa chỉ', 'error')
    }
  } else {
    Swal.fire('Đã hủy', 'Địa chỉ chưa được thêm', 'info')
  }
}
</script>

<style scoped>
.invalid-feedback {
  display: block;
  width: 100%;
  margin-top: 0.25rem;
  font-size: 0.875em;
  color: #dc3545;
}
</style>