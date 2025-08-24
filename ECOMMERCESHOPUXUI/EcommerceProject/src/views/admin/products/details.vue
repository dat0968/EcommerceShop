<script setup>
import { onMounted, ref, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
import { jwtDecode } from 'jwt-decode'
import { useRouter } from 'vue-router'
const getUrlAPI = ref(GetApiUrl())
const listBigCategories = ref([])
const listSmallCategories = ref([])
const openModalProductDetails = ref(false)
const props = defineProps({
  listBigCategories: Object,
  listSmallCategories: Object,
  maSp: Number,
  openModalProductDetails: Boolean,
})
const router = useRouter()
const isLoading = ref(false)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const maSp = ref(props.maSp)
const product = ref({})
async function fetchAPIProductsDetails() {
  try {
    if(maSp.value == null){
      return;
    }
    isLoading.value = true
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/LoginStaff')
      return
    }
    accessToken.value = validatetoken.newAccessToken
    const response = await fetch(`${getUrlAPI.value}/api/Products/${maSp.value}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + accessToken.value,
      },
    })
    if (!response.ok) {
      if (response.status >= 400 && response.status <= 403) {
        router.push('/LoginStaff')
        return
      } else {
        throw new Error('Failed to FetchAPI')
      }
    }
    const result = await response.json()
    product.value = result.data
  } catch (error) {
    console.error('Error fetching product:', error)
  } finally {
    isLoading.value = false
  }
}
onMounted(async () => {
  await fetchAPIProductsDetails()
})

// Định nghĩa emit
const emit = defineEmits(['update-success', 'update:openModalProductDetails'])
watch(
  () => props.openModalProductDetails,
  (newVal) => {
    openModalProductDetails.value = newVal
  }
)
// Lấy data Categories từ props cha truyền vào
watch(
  () => props.listSmallCategories,
  (newVal) => {
    listSmallCategories.value = newVal
  },
  { immediate: true }
)
watch(
  () => props.listBigCategories,
  (newVal) => {
    listBigCategories.value = newVal
  },
  { immediate: true }
)
listBigCategories.value = props.listBigCategories
listSmallCategories.value = props.listSmallCategories

// Lấy data từ props product và productdetails (2 case sản phẩm đơn và đa biến thể)
// const product = ref({
//   maSp: props.productinformation.maSp,
//   tenSanPham: props.productinformation.tenSanPham,
//   moTa: props.productinformation.moTa,
//   isActive: true,
//   hasVariants: props.productinformation.hasVariants,
//   confirmhasVariants: props.productinformation.hasVariants,
//   categoryDetails: props.productinformation.categoryDetails,
//   productDetails: props.productinformation.productDetails ? props.productinformation.productDetails.map((detail) => ({
//     ...detail,
//     images: [...detail.images],
//   })) : [],
// })

// Đa biến thể
// const detailsproductHasVariants = ref({
//   productDetails: product.value.productDetails ? product.value.productDetails.map((detail) => ({
//     ...detail,
//     images: [...detail.images],
//   })) : [],
// })

// Đơn biến thể
// const detailsproductSingle = ref({
//   productDetails:
//     product.value.hasVariants == false
//       ? product.value.productDetails.map((detail) => ({
//           ...detail,
//           images: [...detail.images],
//         }))
//       : [
//           {
//             kichThuoc: '',
//             mauSac: '',
//             soLuongTon: 0,
//             donGia: 0,
//             images: [],
//           },
//         ],
// })
function closeModal() {
  emit('update:openModalProductDetails', false)
}

watch(
  () => props.maSp,
  (newVal) => {
    maSp.value = newVal
    fetchAPIProductsDetails()
  }
)

// Chuyển đổi cập nhật biến thể đơn thành đa biến thể và ngược lại
watch(
  () => product.value.hasVariants,
  (newVal) => {
    if (newVal == true && product.value.hasVariants == false) {
      Swal.fire({
        title: 'Xác nhận chuyển đổi sản phẩm này thành sản phẩm đa biến thể chứ ?',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(async (result) => {
        if (result.isConfirmed) {
          product.value.hasVariants = true
        } else {
          product.value.hasVariants = false
        }
      })
    } else if (newVal == false && product.value.hasVariants == true) {
      Swal.fire({
        title: 'Xác nhận chuyển đổi sản phẩm này thành sản phẩm đơn lẻ chứ ?',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(async (result) => {
        if (result.isConfirmed) {
          product.value.hasVariants = false
        } else {
          product.value.hasVariants = true
        }
      })
    }
  }
)
</script>
<template>
  <div
    style="text-align: left"
    v-if="openModalProductDetails"
    class="custom-modal-overlay"
    @click.self="closeModal"
  >
    <div class="custom-modal-content">
      <!-- Modal Header -->
      <div class="modal-header">
        <h3>Chi tiết Thông Tin Sản Phẩm ({{ product.maSp }})</h3>
        <button class="close-button" @click="closeModal">×</button>
      </div>
      <div class="loading-overlay" style="text-align: center;" v-if="isLoading">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="text-white mt-2">Đang xử lý...</p>
      </div>
      <!-- Modal Body -->
      <div v-else class="modal-body">
        <!-- Tên sản phẩm -->
        <div class="mb-3">
          <label class="form-label">Tên sản phẩm</label>
          <input readonly v-model="product.tenSanPham" type="text" class="form-control" />
        </div>

        <!-- Mô tả sản phẩm -->
        <div class="mb-3">
          <label class="form-label">Mô tả</label>
          <textarea
            readonly
            v-model="product.moTa"
            class="form-control"
            style="height: 150px"
          ></textarea>
        </div>

        <!-- Đơn giá và số lượng tồn (nếu không có biến thể) -->
        <div class="mb-3" v-if="!product.hasVariants">
          <label>Đơn giá</label>
          <input
            readonly
            v-model="product.productDetails[0].donGia"
            type="number"
            class="form-control"
          />
        </div>
        <div class="mb-3" v-if="!product.hasVariants">
          <label>Số lượng tồn</label>
          <input
            readonly
            v-model="product.productDetails[0].soLuongTon"
            type="number"
            class="form-control"
          />
        </div>

        <!-- Ảnh sản phẩm -->
        <div class="mb-3" v-if="!product.hasVariants">
          <label>Ảnh sản phẩm</label>
          <div class="image-grid">
            <img
              v-for="(image, index) in product.productDetails[0].images"
              :key="index"
              :src="
                image.preview
                  ? image.preview
                  : getUrlAPI.replace('api/', '') + '/HinhAnh/Products/' + image.tenHinhAnh
              "
              class="image-thumb"
              alt="Ảnh sản phẩm"
            />
          </div>
        </div>

        <!-- Danh mục -->
        <div class="mb-3">
          <div v-for="(cat, index) in product.categoryDetails" :key="index" class="row">
            <div class="col">
              <label>Danh mục cha</label>
              <select disabled v-model="cat.maDanhMucCha" class="form-control">
                <option value="">-- Chọn --</option>
                <option
                  v-for="item in listBigCategories"
                  :key="item.maDanhMucCha"
                  :value="item.maDanhMucCha"
                >
                  {{ item.tenDanhMucCha }}
                </option>
              </select>
            </div>
            <div class="col">
              <label>Danh mục con</label>
              <select disabled v-model="cat.maDanhMucCon" class="form-control">
                <option value="">-- Chọn --</option>
                <option
                  v-for="item in listSmallCategories"
                  :key="item.maDanhMucCon"
                  :value="item.maDanhMucCon"
                >
                  {{ item.tenDanhMucCon }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <!-- Biến thể -->
        <div class="mb-3" v-if="product.hasVariants">
          <label class="form-label fw-bold">Chi tiết sản phẩm</label>
          <div v-for="(detail, index) in product.productDetails" :key="index" class="variant-card">
            <div class="row">
              <div class="col">
                <label>Kích thước</label>
                <input readonly v-model="detail.kichThuoc" class="form-control" />
              </div>
              <div class="col">
                <label>Màu sắc</label>
                <input readonly v-model="detail.mauSac" class="form-control" />
              </div>
              <div class="col">
                <label>Số lượng tồn</label>
                <input readonly v-model="detail.soLuongTon" class="form-control" type="number" />
              </div>
              <div class="col">
                <label>Đơn giá</label>
                <input readonly v-model="detail.donGia" class="form-control" type="number" />
              </div>
            </div>
            <div class="image-grid mt-2">
              <img
                v-for="(img, i) in detail.images"
                :key="i"
                :src="
                  img.preview
                    ? img.preview
                    : getUrlAPI.replace('api/', '') + '/HinhAnh/Products/' + img.tenHinhAnh
                "
                class="image-thumb"
                alt="Ảnh biến thể"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.6);
  z-index: 1050;
  display: flex;
  align-items: center;
  justify-content: center;
}

.custom-modal-content {
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid #007bff;
  padding-bottom: 10px;
  margin-bottom: 20px;
}

.close-button {
  background: transparent;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: #333;
}

.image-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.image-thumb {
  width: 120px;
  height: 120px;
  object-fit: cover;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.variant-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 15px;
  background-color: #f9f9f9;
}
</style>
