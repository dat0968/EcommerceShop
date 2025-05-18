<script setup>
import { onMounted, ref, watch } from 'vue'
import Swal from 'sweetalert2'
const getUrlAPI = ref('https://localhost:7217/api')
const listBigCategories = ref([])
const listSmallCategories = ref([])
const hasVariants = ref(false)
const mainImages = ref([])
const mainImagePreviews = ref([])

const props = defineProps({
  listBigCategories: Object,
  listSmallCategories: Object,
})
listBigCategories.value = props.listBigCategories
listSmallCategories.value = props.listSmallCategories

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
watch(
  () => hasVariants.value,
  (newVal) => {
    if (newVal == true) {
      Swal.fire({
        title:
          'Hành động này sẽ khiến thông tin các mục dành cho sản phẩm đơn lẻ (không có biến thể) sẽ không được lưu. Xác nhận chứ ?',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(async (result) => {
        if (result.isConfirmed) {
          hasVariants.value = true
        } else {
          hasVariants.value = false
        }
      })
    }
  }
)

const product = ref({
  tenSanPham: '',
  isActive: true,
  categoryDetails: [{ maDanhMucCha: 0, maDanhMucCon: 0 }],
  productDetails: [
    {
      kichThuoc: '',
      mauSac: '',
      soLuongTon: 0,
      donGia: 0,
      images: [],
    },
  ],
})

const detailsproductSingle = ref({
  productDetails: [
    {
      kichThuoc: '',
      mauSac: '',
      soLuongTon: 0,
      donGia: 0,
      images: [],
    },
  ],
})

const detailsproductHasVariants = ref({
  productDetails: [
    {
      kichThuoc: '',
      mauSac: '',
      soLuongTon: 0,
      donGia: 0,
      images: [],
    },
  ],
})

function addCategoryDetail() {
  product.value.categoryDetails.push({ maDanhMucCha: 0, maDanhMucCon: 0 })
}

function addProductDetail() {
  detailsproductHasVariants.value.productDetails.push({
    kichThuoc: '',
    mauSac: '',
    soLuongTon: 0,
    donGia: 0,
    isActive: true,
    images: [],
  })
}

async function submitProduct() {
  try {
    // Validate cho sản phẩm không có biến thể
    if (!hasVariants.value) {
      if (!product.value.tenSanPham.trim()) {
        Swal.fire('Vui lòng nhập tên sản phẩm', '', 'error')
        return
      }
      if (
        !detailsproductSingle.value.productDetails[0].donGia ||
        detailsproductSingle.value.productDetails[0].donGia <= 0
      ) {
        Swal.fire('Vui lòng nhập đơn giá là một giá trị lớn hơn 0', '', 'error')
        return
      }
      if (detailsproductSingle.value.productDetails[0].length === 0) {
        Swal.fire('Vui lòng chọn ít nhất một ảnh cho sản phẩm', '', 'error')
        return
      }

      product.value.productDetails = detailsproductSingle.value.productDetails.map((detail) => ({
        ...detail,
        images: [...detail.images],
      }))
    }
    // Validate cho sản phẩm có biến thể
    else {
      if (!product.value.tenSanPham.trim()) {
        Swal.fire('Vui lòng nhập tên sản phẩm', '', 'error')
        return
      }
      // Kiểm tra kích thước và màu sắc của từng biến thể
      for (let i = 0; i < detailsproductHasVariants.value.productDetails.length; i++) {
        const detail = detailsproductHasVariants.value.productDetails[i]
        if (!detail.kichThuoc.trim() && !detail.mauSac.trim()) {
          Swal.fire(`Vui lòng nhập kích thước hoặc màu sắc cho biến thể thứ ${i + 1}`, '', 'error')
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
      product.value.productDetails = detailsproductHasVariants.value.productDetails.map(
        (detail) => ({
          ...detail,
          images: detail.images.map((image) => ({
            tenHinhAnh: image.tenHinhAnh,
          })),
        })
      )
      // Kiểm tra trùng lặp chi tiết sản phẩm
      var checkVarianrs = hasDuplicatesByColorAndSize(product.value.productDetails)
      if (checkVarianrs == true) {
        Swal.fire(`Đã xuất hiện nhiều biến thể giống nhau, vui lòng kiểm tra lại`, '', 'error')
        return
      }
    }

    // Kiểm tra ảnh cho từng chi tiết sản phẩm
    product.value.productDetails.forEach((p, index) => {
      if (p.images.length == 0) {
        Swal.fire(`Biến thể số ${index + 1} thiếu hình ảnh`, '', 'error')
        return
      }
    })

    // Kiểm tra danh mục có bị trống không
    for (let i = 0; i < product.value.categoryDetails; i++) {
      const detailCategories = product.value.categoryDetails[i]
      if (detailCategories.maDanhMucCha == 0) {
        Swal.fire(`Vui lòng chọn danh mục cha cặp danh mục thứ ${i + 1}`, '', 'error')
        return
      }
      if (detailCategories.maDanhMucCon == 0) {
        Swal.fire(`Vui lòng chọn danh mục cha cặp danh mục thứ ${i + 1}`, '', 'error')
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

    // Upload file lên server
    const formData = new FormData()
    formData.append('file', file)
    const responseImage = await fetch(getApiUrl + '/UploadImage', {
      method: 'POST',
      body: formData,
    })
    if (!responseImage.ok) {
      throw new Error(`Lỗi khi upload ảnh: ${response.status} ${response.statusText}`)
    }

    const response = await fetch(`${getUrlAPI.value}/Products`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(product.value),
    })
    const result = await response.json()
    if (result.success) {
      Swal.fire('Thêm sản phẩm thành công', '', 'success')
    }
    console.log(product.value)
  } catch (error) {
    console.log(error)
  }
}

function handleMultipleImages(event, index) {
  const files = event.target.files
  if (!files.length) return

  Array.from(files).forEach((file) => {
    const preview = URL.createObjectURL(file)
    // product.value.productDetails[index].images.push({ preview, file })
    detailsproductHasVariants.value.productDetails[index].images.push({
      preview,
      tenHinhAnh: file.name,
    })
  })

  // Reset input file để chọn lại cùng file nếu cần
  event.target.value = ''
}

function removeDetail(index) {
  if (detailsproductHasVariants.value.productDetails.length > 1) {
    detailsproductHasVariants.value.productDetails.splice(index, 1)
  } else {
    Swal.fire('Tối thiểu cần giữ lại một chi tiết sản phẩm', '', 'error')
  }
}
function removeImage(detailIndex, imageIndex) {
  // product.value.productDetails[detailIndex].images.splice(imageIndex, 1)
  detailsproductHasVariants.value.productDetails[detailIndex].images.splice(imageIndex, 1)
}

function handleMainImages(event) {
  const files = event.target.files
  if (!files.length) return

  Array.from(files).forEach((file) => {
    const preview = URL.createObjectURL(file)
    mainImages.value.push(file)
    mainImagePreviews.value.push(preview)
    detailsproductSingle.value.productDetails[0].images.push({ tenHinhAnh: file.name })
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
  detailsproductSingle.value.productDetails[0].images.splice(index, 1)
}

function removeCategoryDetail(index) {
  if (product.value.categoryDetails.length > 1) {
    product.value.categoryDetails.splice(index, 1)
  } else {
    Swal.fire('Tối thiểu cần giữ lại một cặp danh mục', '', 'error')
  }
}
</script>
<template>
  <!-- Modal -->
  <div
    class="modal fade"
    id="productModal"
    tabindex="-1"
    aria-labelledby="productModalLabel"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-xl modal-dialog-scrollable">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title">Thêm Sản Phẩm Mới</h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
        </div>

        <div class="modal-body">
          <!-- Tên sản phẩm -->
          <div class="mb-3">
            <label class="form-label">Tên sản phẩm</label>
            <input v-model="product.tenSanPham" type="text" class="form-control" />
          </div>

          <!-- Giá sản phẩm - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!hasVariants">
            <label class="form-label"
              >Đơn giá
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input
              v-model="detailsproductSingle.productDetails[0].donGia"
              type="number"
              class="form-control"
            />
          </div>

          <!-- Ảnh sản phẩm chính - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!hasVariants">
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
                v-for="(preview, index) in mainImagePreviews"
                :key="index"
                class="position-relative"
              >
                <img
                  :src="preview"
                  alt="Ảnh sản phẩm"
                  class="img-thumbnail"
                  style="max-width: 150px; max-height: 150px"
                />
                <button
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
                v-model="hasVariants"
                class="form-check-input"
                type="checkbox"
                id="hasVariants"
              />
              <label class="form-check-label" for="hasVariants"> Sản phẩm có biến thể </label>
            </div>
          </div>
          <!-- Chi tiết sản phẩm - chỉ hiển thị khi có biến thể -->
          <div class="mb-3" v-if="hasVariants">
            <label class="form-label fw-bold">Chi tiết sản phẩm</label>
            <div
              v-for="(detail, index) in detailsproductHasVariants.productDetails"
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
          <button class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
          <button class="btn btn-primary" @click="submitProduct">Lưu</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style></style>