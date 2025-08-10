<template>
  <div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h4>Địa chỉ của tôi</h4>
      <!-- Button mở modal -->
      <button
        type="button"
        class="btn btn-primary"
        data-bs-toggle="modal"
        data-bs-target="#addAddressModal"
      >
        Thêm địa chỉ mới
      </button>
      <!-- Modal -->
      <createAddressModal />
    </div>

    <div
      v-for="dc in diachis"
      :key="dc.id"
      class="border rounded p-3 mb-3"
      style="background-color: white"
    >
      <div class="d-flex justify-content-between">
        <div>
          <strong>{{ dc.hoten }}</strong>
          <span class="ms-2 text-muted">{{ dc.sdt }}</span>
        </div>
        <div>
          <a href="#" @click.prevent="edit(dc)">Cập nhật</a>
          <span class="mx-1">|</span>
          <b href="#" class="text-danger" @click.prevent="remove(dc.id)">Xóa</b>
        </div>
      </div>

      <div class="mt-2 text-muted">
        {{ dc.diachichitiet }}
      </div>

      <div class="mt-2">
        <span v-if="dc.macDinh" class="badge bg-light text-danger border">Mặc định</span>
        <!-- <button v-else class="btn btn-outline-secondary btn-sm" @click="setDefault(dc.id)">
          Thiết lập mặc định
        </button> -->
      </div>
    </div>
    <editAddressModal @edit="edit" v-if="isOpenEdit" :selectedAddress="selectedAddress" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import createAddressModal from '@/components/address/createAddressModal.vue'
import editAddressModal from '@/components/address/editAddressModal.vue'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { useRouter } from 'vue-router'
const readToken = ref({})
const isOpenEdit = ref(false)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const provinces = ref([])
const districts = ref([])
const wards = ref([])
const token = 'eb507c61-0fad-11f0-9aa0-bece206412cb'
const diachis = ref([])
const getUrlAPI = ref(GetApiUrl())
const selectedAddress = ref({})
async function fetchAdrress() {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/Login')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  readToken.value = decodeToken(accessToken.value)
  const response = await fetch(getUrlAPI.value + '/api/Address/' + readToken.value.IdUser, {
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
      const errorMessage = response.message
      throw new Error(errorMessage)
    }
  }
  const result = await response.json()
  diachis.value = result
}
onMounted(async () => {
  fetchAdrress()
})
function edit(dc) {
  isOpenEdit.value = !isOpenEdit.value
  selectedAddress.value = dc
}

async function remove(id) {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (!validatetoken.isValid) {
    router.push('/Login')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  const confirmResult = await Swal.fire({
    title: 'Xác nhận xóa địa chỉ này?',
    text: 'Bạn có chắc muốn xóa địa chỉ này?',
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: 'Đồng ý',
    cancelButtonText: 'Hủy',
    reverseButtons: true,
  })

  if (confirmResult.isConfirmed) {
    try {
      const response = await fetch(`${getUrlAPI.value}/api/Address?id=${id}`, {
        method: 'DELETE',
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
          throw new Error('HandleDELETEAddress Failed')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Đã xóa địa chỉ',
          icon: 'success',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        })
        window.location.reload()
      } else {
        Swal.fire('Thất bại', result.message || 'Không thể xóa địa chỉ', 'error')
      }
    } catch (error) {
      Swal.fire('Lỗi', error.message || 'Đã xảy ra lỗi khi xử lý xóa địa chỉ', 'error')
    }
  } else {
    Swal.fire('Đã hủy', 'Địa chỉ chưa được xóa', 'info')
  }
}

function setDefault(id) {
  diachis.value.forEach((dc) => (dc.macDinh = false))
  const target = diachis.value.find((dc) => dc.id === id)
  if (target) target.macDinh = true
}
</script>
