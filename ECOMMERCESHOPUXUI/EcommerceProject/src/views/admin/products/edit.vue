<script setup>
import { useRouter } from 'vue-router'
import { onMounted, ref, watch } from 'vue'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import Cookies from 'js-cookie'
import Swal from 'sweetalert2'
const getUrlAPI = ref(GetApiUrl())
const listBigCategories = ref([])
const listSmallCategories = ref([])
const mainImages = ref([])
const multiImages = ref([])
const mainImagePreviews = ref([])
const isSubmitting = ref(false)
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const router = useRouter()
const readToken = ref({})
const isLoading = ref(false)
const openModalProductUpdate = ref(false)
const props = defineProps({
  listBigCategories: Object,
  listSmallCategories: Object,
  maSp: Number,
  openModalProductUpdate: Boolean,
})
const maSp = ref(props.maSp)
const product = ref({})
async function fetchAPIProductsDetails() {
  try {
    if (maSp.value == null) {
      return
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
    console.log(product.value)
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
const emit = defineEmits(['update-success', 'update:openModalProductUpdate'])
watch(
  () => props.openModalProductUpdate,
  (newVal) => {
    openModalProductUpdate.value = newVal
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
openModalProductUpdate.value = props.openModalProductUpdate
// Lấy data từ props product và productdetails (2 case sản phẩm đơn và đa biến thể)
// const product = ref({
//   maSp: props.productinformation.maSp,
//   tenSanPham: props.productinformation.tenSanPham,
//   moTa: props.productinformation.moTa,
//   isActive: true,
//   hasVariants: props.productinformation.hasVariants,
//   confirmhasVariants: props.productinformation.hasVariants,
//   categoryDetails: props.productinformation.categoryDetails,
//   productDetails: props.productinformation.productDetails
//     ? props.productinformation.productDetails.map((detail) => ({
//         ...detail,
//         images: [...detail.images],
//       }))
//     : [],
// })

// Đa biến thể
// const detailsproductHasVariants = ref({
//   productDetails: product.value.productDetails.map((detail) => ({
//     ...detail,
//     images: [...detail.images],
//   })),
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
watch(
  () => props.maSp,
  async (newVal) => {
    maSp.value = newVal
    await fetchAPIProductsDetails()
  }
)

// Chuyển đổi cập nhật biến thể đơn thành đa biến thể và ngược lại
watch(
  () => product.value.hasVariants,
  (newVal, oldVal) => {
    if (newVal === oldVal || isLoading.value == false) return
    if (newVal == true && oldVal == false) {
      Swal.fire({
        title: 'Xác nhận chuyển đổi sản phẩm này thành sản phẩm đa biến thể chứ ?',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(async (result) => {
        if (!result.isConfirmed) {
          product.value.hasVariants = true
        } else {
          product.value.hasVariants = false
        }
      })
    } else if (newVal == false && oldVal == true) {
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

function addCategoryDetail() {
  product.value.categoryDetails.push({ maDanhMucCha: 0, maDanhMucCon: 0 })
}

function addProductDetail() {
  product.value.productDetails.push({
    kichThuoc: '',
    mauSac: '',
    soLuongTon: 0,
    donGia: 0,
    isActive: true,
    images: [],
  })
}
async function updateProduct() {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/LoginStaff')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      readToken.value = decodeToken(accessToken.value)
      isSubmitting.value = true
      // Validate cho sản phẩm không có biến thể
      if (product.value.hasVariants == false) {
        if (!product.value.tenSanPham.trim()) {
          Swal.fire('Vui lòng nhập tên sản phẩm', '', 'error')
          return
        }
        if (
          !product.value.productDetails[0].donGia ||
          product.value.productDetails[0].donGia <= 0
        ) {
          Swal.fire('Vui lòng nhập đơn giá là một giá trị lớn hơn 0', '', 'error')
          return
        }
        if (
          !product.value.productDetails[0].soLuongTon ||
          product.value.productDetails[0].soLuongTon <= 0
        ) {
          Swal.fire('Vui lòng nhập số lượng tồn là một giá trị lớn hơn 0', '', 'error')
          return
        }
        if (product.value.productDetails[0].images.length === 0) {
          Swal.fire('Vui lòng chọn ít nhất một ảnh cho sản phẩm', '', 'error')
          return
        }
        const firstDetail = product.value.productDetails[0]
        product.value.productDetails = firstDetail
          ? [
              {
                kichThuoc: '',
                mauSac: '',
                soLuongTon: firstDetail.soLuongTon,
                donGia: firstDetail.donGia,
                images:
                  firstDetail.images && firstDetail.images.length > 0
                    ? [{ tenHinhAnh: firstDetail.images[0].tenHinhAnh }]
                    : [],
              },
            ]
          : []
        for (const file of mainImages.value) {
          if (!file || file.size === 0) {
            Swal.fire('Tệp ảnh không hợp lệ hoặc đã bị xóa', '', 'error')
            return
          }
          const formData = new FormData()
          formData.append('file', file)
          const responseImage = await fetch(getUrlAPI.value + '/api/UploadImages', {
            method: 'POST',
            body: formData,
          })
          if (!responseImage.ok) {
            throw new Error(
              `Lỗi khi upload ảnh: ${responseImage.status} ${responseImage.statusText}`
            )
          }
          mainImages.value = []
        }
      }
      // Validate cho sản phẩm có biến thể
      else {
        if (!product.value.tenSanPham.trim()) {
          Swal.fire('Vui lòng nhập tên sản phẩm', '', 'error')
          return
        }
        // Kiểm tra kích thước và màu sắc của từng biến thể
        for (let i = 0; i < product.value.productDetails.length; i++) {
          const detail = product.value.productDetails[i]
          if (detail.kichThuoc.trim() == '' && detail.mauSac.trim() == '') {
            Swal.fire(
              `Vui lòng nhập kích thước hoặc màu sắc cho biến thể thứ ${i + 1}`,
              '',
              'error'
            )
            return
          }
          if (!detail.donGia || detail.donGia <= 0) {
            Swal.fire(
              `Vui lòng nhập đơn giá với giá trị lớn hơn 0 cho biến thể thứ ${i + 1}`,
              '',
              'error'
            )
            return
          }
          if (!detail.soLuongTon || detail.soLuongTon <= 0) {
            Swal.fire(
              `Vui lòng nhập số lượng tồn với giá trị lớn hơn 0 cho biến thể thứ ${i + 1}`,
              '',
              'error'
            )
            return
          }
        }
        // Cập nhật productDetails này vô product
        product.value.productDetails = product.value.productDetails.map((detail) => ({
          ...detail,
          images: detail.images.map((image) => ({
            tenHinhAnh: image.tenHinhAnh,
          })),
        }))
        // Kiểm tra trùng lặp chi tiết sản phẩm
        var checkVarianrs = hasDuplicatesByColorAndSize(product.value.productDetails)
        if (checkVarianrs == true) {
          Swal.fire(`Đã xuất hiện nhiều biến thể giống nhau, vui lòng kiểm tra lại`, '', 'error')
          return
        }

        for (const file of multiImages.value) {
          const formData = new FormData()
          formData.append('file', file)
          const responseImage = await fetch(getUrlAPI.value + '/api/UploadImages', {
            method: 'POST',
            body: formData,
          })
          if (!responseImage.ok) {
            throw new Error(
              `Lỗi khi upload ảnh: ${responseImage.status} ${responseImage.statusText}`
            )
          }
        }
      }
      let hasError = false
      // Kiểm tra ảnh cho từng chi tiết sản phẩm
      product.value.productDetails.forEach((p, index) => {
        if (p.images.length == 0) {
          Swal.fire(`Biến thể số ${index + 1} thiếu hình ảnh`, '', 'error')
          hasError = true
        }
      })
      if (hasError) {
        return // Ngăn chặn cập nhật nếu có lỗi
      }
      // Kiểm tra danh mục có bị trống không
      for (let i = 0; i < product.value.categoryDetails.length; i++) {
        const detailCategories = product.value.categoryDetails[i]
        if (detailCategories.maDanhMucCha == 0) {
          Swal.fire(`Vui lòng chọn danh mục cha cặp danh mục thứ ${i + 1}`, '', 'error')
          return
        }
        if (detailCategories.maDanhMucCon == 0) {
          Swal.fire(`Vui lòng chọn danh mục con cặp danh mục thứ ${i + 1}`, '', 'error')
          return
        }
      }

      // Kiểm tra trùng lặp danh mục
      var checkCategories = hasDuplicateByCategory(product.value.categoryDetails)
      if (checkCategories == true) {
        Swal.fire(
          `Đã xuất hiện nhiều cặp danh mục cha-con giống nhau, vui lòng kiểm tra lại`,
          '',
          'error'
        )
        return
      }

      const response = await fetch(`${getUrlAPI.value}/api/Products/${product.value.maSp}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer ' + accessToken.value,
        },
        body: JSON.stringify(product.value),
      })
      if (!response.ok) {
        if (response.status >= 400 && response.status <= 403) {
          router.push('/LoginStaff')
          return
        } else {
          throw new Error('Lỗi khi gọi API')
        }
      }
      const result = await response.json()
      if (result.success) {
        Swal.fire({
          title: 'Đã cập nhật thông tin sản phẩm',
          icon: 'success',
          timer: 1500, // 2000 ms = 2 giây
          showConfirmButton: false, // ẩn nút OK
          timerProgressBar: true, // hiển thị thanh tiến trình
        })
        setTimeout(() => {
          const modalElement = document.querySelector(`.close_modal_${product.value.maSp}`)
          modalElement.click()
          // Emit event khi cập nhật thành công
          emit('update-success')
        }, 1500)
      }
      //console.log(product.value)
    }
  } catch (error) {
    console.log(error)
  } finally {
    isSubmitting.value = false
  }
}

function handleMultipleImages(event, index) {
  const files = event.target.files
  if (!files.length) return

  Array.from(files).forEach((file) => {
    const preview = URL.createObjectURL(file)
    multiImages.value.push(file)
    // product.value.productDetails[index].images.push({ preview, file })
    product.value.productDetails[index].images.push({
      preview,
      tenHinhAnh: file.name,
    })
  })

  // Reset input file để chọn lại cùng file nếu cần
  event.target.value = ''
}

function removeDetail(index) {
  if (product.value.productDetails.length > 1) {
    product.value.productDetails.splice(index, 1)
  } else {
    Swal.fire('Tối thiểu cần giữ lại một chi tiết sản phẩm', '', 'error')
  }
}
function removeImage(detailIndex, imageIndex) {
  // product.value.productDetails[detailIndex].images.splice(imageIndex, 1)
  product.value.productDetails[detailIndex].images.splice(imageIndex, 1)
}

function handleMainImages(event) {
  const files = event.target.files
  if (!files.length) return

  Array.from(files).forEach((file) => {
    const preview = URL.createObjectURL(file)
    mainImages.value.push(file)
    mainImagePreviews.value.push(preview)
    product.value.productDetails[0].images.push({ preview, tenHinhAnh: file.name })
  })
  event.target.value = ''
}
function hasDuplicatesByColorAndSize(details) {
  const seen = new Set()
  for (let item of details) {
    const key = `${item.mauSac}_${item.kichThuoc}` // Tạo key từ cặp color và size
    if (seen.has(key)) return true // Nếu key đã tồn tại, có trùng lặp
    seen.add(key)
  }
  return false
}

function hasDuplicateByCategory(categories) {
  const seen = new Set()
  for (let item of categories) {
    const key = `${item.maDanhMucCha}_${item.maDanhMucCon}`
    if (seen.has(key)) return true
    seen.add(key)
  }
  return false
}

function removeMainImage(index) {
  mainImages.value.splice(index, 1)
  mainImagePreviews.value.splice(index, 1)
  product.value.productDetails[0].images.splice(index, 1)
}

function removeCategoryDetail(index) {
  if (product.value.categoryDetails.length > 1) {
    product.value.categoryDetails.splice(index, 1)
  } else {
    Swal.fire('Tối thiểu cần giữ lại một cặp danh mục', '', 'error')
  }
}

function closeModal() {
  // openModalProductDetails.value = !openModalProductDetails.value
  emit('update:openModalProductUpdate', false)
}
</script>
<template>
  <!-- Modal -->
  <div
    class="modal fade show custom-modal-overlay"
    tabindex="-1"
    aria-labelledby="productModalLabel"
    aria-hidden="true"
    style="text-align: left"
    v-if="openModalProductUpdate"
  >
    <div class="modal-dialog modal-xl modal-dialog-scrollable">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title">Sửa Thông Tin Sản Phẩm ({{ product.maSp }})</h5>
          <button
            type="button"
            :class="['btn-close', 'btn-close-white', 'close_modal_' + product.maSp]"
            @click="closeModal"
          ></button>
        </div>
        <div class="loading-overlay" style="text-align: center" v-if="isLoading">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="text-white mt-2">Đang xử lý...</p>
        </div>
        <div class="modal-body" v-else>
          <!-- Tên sản phẩm -->
          <div class="mb-3">
            <label class="form-label">Tên sản phẩm</label>
            <input v-model="product.tenSanPham" type="text" class="form-control" />
          </div>
          <!-- Mô tả sản phẩm -->
          <div class="mb-3">
            <label class="form-label">Mô tả </label>
            <textarea
              style="height: 200px"
              v-model="product.moTa"
              type="number"
              class="form-control"
            >
            </textarea>
          </div>
          <!-- Giá sản phẩm - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.hasVariants">
            <label class="form-label"
              >Đơn giá
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input v-model="product.productDetails[0].donGia" type="number" class="form-control" />
          </div>
          <!-- Số lượng sản phẩm - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.hasVariants">
            <label class="form-label"
              >Số lượng tồn
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input
              v-model="product.productDetails[0].soLuongTon"
              type="number"
              class="form-control"
            />
          </div>
          <!-- Ảnh sản phẩm chính - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.hasVariants">
            <label class="form-label"
              >Ảnh sản phẩm
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input
              type="file"
              class="form-control"
              @change="handleMainImages"
              accept="image/*"
              multiple
            />
            <div class="d-flex flex-wrap gap-3 mt-3">
              <div
                v-for="(image, index) in product.productDetails[0].images"
                :key="index"
                class="position-relative"
              >
                <img
                  v-if="image.preview"
                  :src="image.preview"
                  alt="Ảnh sản phẩm"
                  class="img-thumbnail"
                  style="max-width: 150px; max-height: 150px"
                />
                <button
                  v-if="image.preview"
                  @click="removeMainImage(index)"
                  class="btn btn-sm btn-danger position-absolute"
                  style="top: -10px; right: -10px"
                >
                  <i class="fas fa-times"></i>
                </button>
                <img
                  v-if="!image.preview"
                  :src="getUrlAPI.replace('api', '') + '/HinhAnh/Products/' + image.tenHinhAnh"
                  alt="Ảnh sản phẩm"
                  class="img-thumbnail"
                  style="max-width: 150px; max-height: 150px"
                />
                <button
                  v-if="!image.preview"
                  @click="removeMainImage(index)"
                  class="btn btn-sm btn-danger position-absolute"
                  style="top: -10px; right: -10px"
                >
                  <i class="fas fa-times"></i>
                </button>
              </div>
            </div>
          </div>

          <!-- Danh mục cha - con -->
          <div class="mb-4">
            <div v-for="(cat, index) in product.categoryDetails" :key="index" class="row g-2 mb-2">
              <div class="col">
                <label class="form-label">Danh mục cha</label>
                <select v-model="cat.maDanhMucCha" class="form-select">
                  <option disabled value="">-- Chọn danh mục cha --</option>
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
                <label class="form-label">Danh mục con</label>
                <select v-model="cat.maDanhMucCon" class="form-select">
                  <option disabled value="">-- Chọn danh mục con --</option>
                  <option
                    v-for="item in listSmallCategories"
                    :key="item.maDanhMucCon"
                    :value="item.maDanhMucCon"
                  >
                    {{ item.tenDanhMucCon }}
                  </option>
                </select>
              </div>
              <div class="col-auto d-flex align-items-end">
                <button
                  @click="removeCategoryDetail(index)"
                  class="btn btn-sm btn-outline-danger mb-1"
                  :disabled="product.categoryDetails.length <= 1"
                >
                  <i class="fas fa-times"></i>
                </button>
              </div>
            </div>
            <button type="button" class="btn btn-sm btn-outline-primary" @click="addCategoryDetail">
              + Thêm danh mục
            </button>
          </div>
          <!-- Checkbox xác nhận có biến thể -->
          <div class="mb-3">
            <div class="form-check">
              <input
                v-model="product.hasVariants"
                class="form-check-input"
                type="checkbox"
                :id="`hasVariants_${product.maSp}`"
              />
              <label class="form-check-label"> Sản phẩm có biến thể </label>
            </div>
          </div>
          <!-- Chi tiết sản phẩm - chỉ hiển thị khi có biến thể -->
          <div class="mb-3" v-if="product.hasVariants">
            <label class="form-label fw-bold">Chi tiết sản phẩm</label>
            <div
              v-for="(detail, index) in product.productDetails"
              :key="index"
              class="border rounded p-3 mb-3"
            >
              <div class="row g-3">
                <div class="col-md-3">
                  <label class="form-label">Kích thước</label>
                  <input v-model="detail.kichThuoc" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Màu sắc</label>
                  <input v-model="detail.mauSac" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Số lượng tồn</label>
                  <input v-model="detail.soLuongTon" type="number" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Đơn giá</label>
                  <input v-model="detail.donGia" type="number" class="form-control" />
                </div>
              </div>

              <!-- Hình ảnh -->
              <div class="mt-3">
                <input
                  type="file"
                  class="form-control"
                  multiple
                  @change="handleMultipleImages($event, index)"
                  accept="image/*"
                />

                <!-- Hiển thị preview từng ảnh -->
                <div class="d-flex flex-wrap gap-3 mt-3">
                  <div v-for="(img, i) in detail.images" :key="i" class="position-relative">
                    <img
                      v-if="img.preview"
                      :src="img.preview"
                      alt="Ảnh đã chọn"
                      style="max-width: 150px; max-height: 150px"
                      class="img-thumbnail rounded border"
                    />
                    <button
                      v-if="img.preview"
                      @click="removeImage(index, i)"
                      class="btn btn-sm btn-danger position-absolute"
                      style="top: -10px; right: -10px"
                    >
                      <i class="fas fa-times"></i>
                    </button>
                    <img
                      v-if="!img.preview"
                      :src="getUrlAPI.replace('api', '') + `/HinhAnh/Products/${img.tenHinhAnh}`"
                      alt="Ảnh đã chọn"
                      style="max-width: 150px; max-height: 150px"
                      class="img-thumbnail rounded border"
                    />
                    <button
                      v-if="!img.preview"
                      @click="removeImage(index, i)"
                      class="btn btn-sm btn-danger position-absolute"
                      style="top: -10px; right: -10px"
                    >
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                </div>
              </div>
              <!-- Xóa chi tiết sản phẩm -->
              <div class="mt-4">
                <button @click="removeDetail(index)" class="btn btn-sm btn-outline-danger">
                  Xóa chi tiết sản phẩm
                </button>
              </div>
            </div>

            <button type="button" class="btn btn-sm btn-outline-success" @click="addProductDetail">
              + Thêm chi tiết sản phẩm
            </button>
          </div>
        </div>

        <div class="modal-footer">
          <button @click="updateProduct" :disabled="isSubmitting" class="btn btn-primary">
            <span
              v-if="isSubmitting"
              class="spinner-border spinner-border-sm"
              role="status"
              aria-hidden="true"
            ></span>
            {{ isSubmitting ? 'Đang xử lý...' : 'Cập nhật' }}
          </button>
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
</style>