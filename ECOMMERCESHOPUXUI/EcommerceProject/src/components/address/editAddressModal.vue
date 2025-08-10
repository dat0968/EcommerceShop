<template>
  <!-- Modal Edit -->
  <div class="modal-overlay">
    <div class="modal-box">
      <h5 class="mb-3">Cập nhật địa chỉ</h5>
      <div v-if="loading" class="loading-overlay">
        <div class="spinner"></div>
      </div>
      <form>
        <div class="mb-2">
          <label>Họ tên</label>
          <input
            v-model="editForm.hoten"
            type="text"
            class="form-control"
            :class="{ 'is-invalid': errors.hoTen }"
          />
          <div class="invalid-feedback">{{ errors.hoTen }}</div>
        </div>
        <div class="mb-2">
          <label>Số điện thoại</label>
          <input
            v-model="editForm.sdt"
            :class="{ 'is-invalid': errors.soDienThoai }"
            type="text"
            class="form-control"
            required
          />
          <div class="invalid-feedback">{{ errors.soDienThoai }}</div>
        </div>
        <div class="mb-2">
          <label>Tỉnh/Thành phố</label>
          <select
            class="form-control"
            :class="{ 'is-invalid': errors.provinceId }"
            @change="fetchDistrict()"
            v-model="editForm.tinh"
          >
            <option value="" disabled>Chọn tỉnh/thành phố</option>
            <option
              v-for="province in provinces"
              :key="province.ProvinceID"
              :value="province.ProvinceName"
            >
              {{ province.ProvinceName }}
            </option>
          </select>

          <div class="invalid-feedback">{{ errors.provinceId }}</div>
        </div>
        <div class="mb-2">
          <label>Quận/Huyện</label>
          <select
            class="form-control"
            :class="{ 'is-invalid': errors.districtId }"
            @change="fetchWard()"
            v-model="editForm.quanHuyen"
          >
            <option value="" disabled>Chọn quận/huyện</option>
            <option
              v-for="district in districts"
              :key="district.DistrictID"
              :value="district.DistrictName"
            >
              {{ district.DistrictName }}
            </option>
          </select>
          <div class="invalid-feedback">{{ errors.districtId }}</div>
        </div>
        <div class="mb-2">
          <label>Xã/Phường</label>
          <select
            class="form-control"
            @change="updateDetailsAddress()"
            :class="{ 'is-invalid': errors.wardCode }"
            v-model="editForm.xaPhuong"
          >
            <option value="" disabled>Chọn phường/xã</option>
            <option v-for="ward in wards" :key="ward.WardCode" :value="ward.WardName">
              {{ ward.WardName }}
            </option>
          </select>
          <div class="invalid-feedback">{{ errors.wardCode }}</div>
        </div>
        <div class="mb-2">
          <label>Địa chỉ chi tiết</label>
          <input
            v-model="editForm.diachichitiet"
            :class="{ 'is-invalid': errors.diaChi }"
            class="form-control"
            rows="2"
            required
          />
          <div class="invalid-feedback">{{ errors.diaChi }}</div>
        </div>
        <div class="form-check mb-2">
          <input
            v-model="editForm.macDinh"
            type="checkbox"
            class="form-check-input"
            id="editMacDinh"
          />
          <label class="form-check-label" for="editMacDinh">Đặt làm mặc định</label>
        </div>
        <div class="d-flex justify-content-end gap-2 mt-3">
          <button type="button" @click="submitEdit" class="btn btn-success">Cập nhật</button>
          <button type="button" class="btn btn-secondary" @click="closeModal()">Hủy</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, defineEmits, defineProps } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { useRouter } from 'vue-router'
const emit = defineEmits(['edit'])
const props = defineProps({
  selectedAddress: Object,
})
const getUrlAPI = ref(GetApiUrl())
const router = useRouter()
const provinces = ref([])
const districts = ref([])
const wards = ref([])
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const token = 'eb507c61-0fad-11f0-9aa0-bece206412cb'
const loading = ref(false)
const errors = ref({
  hoTen: '',
  soDienThoai: '',
  diaChi: '',
  provinceId: '',
  districtId: '',
  wardCode: '',
  payment: '',
})
watch(
  () => props.selectedAddress,
  (newVal) => {
    editForm.value = newVal
  }
)

const editForm = ref({
  id: props.selectedAddress.id,
  hoten: props.selectedAddress.hoten,
  sdt: props.selectedAddress.sdt,
  tinh: props.selectedAddress.tinh,
  quanHuyen: props.selectedAddress.quanHuyen,
  xaPhuong: props.selectedAddress.xaPhuong,
  diachichitiet: props.selectedAddress.diachichitiet,
  macDinh: props.selectedAddress.macDinh,
  provinceId: '',
  districtId: '',
  wardCode: '',
})
async function fetchProvince() {
  try {
    loading.value = true
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
      console.log(
        provinces.value.filter(
          (p) => p.ProvinceName.toLowerCase() == editForm.value.tinh.toLowerCase()
        )[0]
      )
      const responseDistrict = await fetch(
        `https://online-gateway.ghn.vn/shiip/public-api/master-data/district`,
        {
          method: 'POST',
          headers: {
            'Content-type': 'application/json',
            Token: `${token}`,
          },
          body: JSON.stringify({
            province_id: provinces.value.filter(
              (p) => p.ProvinceName.toLowerCase() == editForm.value.tinh.toLowerCase()
            )[0].ProvinceID,
          }),
        }
      )
      const resultDistrict = await responseDistrict.json()
      if (resultDistrict.code === 200) {
        districts.value = resultDistrict.data
        wards.value = []

        const responseWard = await fetch(
          `https://online-gateway.ghn.vn/shiip/public-api/master-data/ward`,
          {
            method: 'POST',
            headers: {
              'Content-type': 'application/json',
              Token: `${token}`,
            },
            body: JSON.stringify({
              district_id: districts.value.filter(
                (p) => p.DistrictName.toLowerCase() == editForm.value.quanHuyen.toLowerCase()
              )[0].DistrictID,
            }),
          }
        )
        const resultWard = await responseWard.json()
        if (resultWard.code === 200) {
          wards.value = resultWard.data
        }
      }
    }
  } catch (error) {
    console.log(error)
  } finally {
    loading.value = false
  }
}
async function fetchDistrict() {
  editForm.value.diachichitiet = ''
  editForm.value.quanHuyen = ''
  editForm.value.xaPhuong = ''
  const responseDistrict = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/master-data/district`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
      body: JSON.stringify({
        province_id: provinces.value.filter(
          (p) => p.ProvinceName.toLowerCase() == editForm.value.tinh.toLowerCase()
        )[0].ProvinceID,
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
  editForm.value.diachichitiet = ''
  editForm.value.xaPhuong = ''
  const responseWard = await fetch(
    `https://online-gateway.ghn.vn/shiip/public-api/master-data/ward`,
    {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
        Token: `${token}`,
      },
      body: JSON.stringify({
        district_id: districts.value.filter(
          (p) => p.DistrictName.toLowerCase() == editForm.value.quanHuyen.toLowerCase()
        )[0].DistrictID,
      }),
    }
  )
  const resultWard = await responseWard.json()
  if (resultWard.code === 200) {
    wards.value = resultWard.data
  }
}

function updateDetailsAddress() {
  editForm.value.diachichitiet = ''
}

onMounted(async () => {
  await fetchProvince()
})
function closeModal() {
  emit('edit')
}
async function submitEdit() {
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
  if (editForm.value.hoten == '' || !editForm.value.hoten.trim()) {
    errors.value.hoTen = 'Vui lòng nhập họ tên'
    isValid = false
  }
  // Validate số điện thoại
  const phoneRegex = /^[0-9]{10}$/
  if (editForm.value.sdt == '' || !editForm.value.sdt.trim()) {
    errors.value.soDienThoai = 'Vui lòng nhập số điện thoại'
    isValid = false
  } else if (!phoneRegex.test(editForm.value.sdt.trim())) {
    errors.value.soDienThoai = 'Số điện thoại không hợp lệ'
    isValid = false
  }
  // Validate địa chỉ
  if (editForm.value.diachichitiet == '' || !editForm.value.diachichitiet.trim()) {
    errors.value.diaChi = 'Vui lòng nhập địa chỉ chi tiết'
    isValid = false
  }
  // Validate tỉnh/thành phố
  if (!editForm.value.tinh) {
    errors.value.provinceId = 'Vui lòng chọn tỉnh/thành phố'
    isValid = false
  }
  console.log(editForm.value.quanHuyen)
  // Validate quận/huyện
  if (!editForm.value.quanHuyen || editForm.value.quanHuyen == '') {
    errors.value.districtId = 'Vui lòng chọn quận/huyện'
    isValid = false
  }

  // Validate phường/xã
  if (!editForm.value.xaPhuong || editForm.value.xaPhuong == '') {
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
  const content = {
    tinh: editForm.value.tinh,
    quanHuyen: editForm.value.quanHuyen,
    xaPhuong: editForm.value.xaPhuong,
    diachichitiet: editForm.value.diachichitiet + ' ' + editForm.value.xaPhuong + ' ' + editForm.value.quanHuyen + ' ' + editForm.value.tinh,
    macDinh: editForm.value.macDinh,
    hoten: editForm.value.hoten,
    sdt: editForm.value.sdt,
    maKh: readToken.IdUser,
  }
  const confirmResult = await Swal.fire({
    title: 'Xác nhận cập nhật địa chỉ mới này?',
    text: 'Bạn có chắc muốn cập nhật địa chỉ này?',
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Đồng ý',
    cancelButtonText: 'Hủy',
    reverseButtons: true,
  })

  if (confirmResult.isConfirmed) {
    try {
      const response = await fetch(`${getUrlAPI.value}/api/Address/${editForm.value.id}`, {
        method: 'PUT',
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
          throw new Error('HandleEditAddress Failed')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Cập nhật địa chỉ thành công',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        window.location.reload()
      } else {
        Swal.fire('Thất bại', result.message || 'Không thể cập nhật địa chỉ', 'error')
      }
    } catch (error) {
      Swal.fire('Lỗi', error.message || 'Đã xảy ra lỗi khi xử lý cập nhật địa chỉ', 'error')
    }
  } else {
    Swal.fire('Đã hủy', 'Địa chỉ chưa được cập nhật', 'info')
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1050;
}

.modal-box {
  background: white;
  padding: 20px;
  width: 500px;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.3);
}
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(255, 255, 255, 0.6);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1060; /* cao hơn modal-box */
}

.spinner {
  border: 4px solid #f3f3f3; /* Light grey */
  border-top: 4px solid #3498db; /* Blue */
  border-radius: 50%;
  width: 36px;
  height: 36px;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>