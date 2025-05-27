<script setup>
import { onMounted, ref, watch } from 'vue'
import Swal from 'sweetalert2'
const getUrlAPI = ref('https://localhost:7217/api')
const listBigCategories = ref([])
const listSmallCategories = ref([])
const props = defineProps({
  listBigCategories: Object,
  listSmallCategories: Object,
  productinformation: Object,
})
// Định nghĩa emit
const emit = defineEmits(['update-success'])

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
const product = ref({
  maSp: props.productinformation.maSp,
  tenSanPham: props.productinformation.tenSanPham,
  moTa: props.productinformation.moTa,
  isActive: true,
  hasVariants: props.productinformation.hasVariants,
  confirmhasVariants: props.productinformation.hasVariants,
  categoryDetails: props.productinformation.categoryDetails,
  productDetails: props.productinformation.productDetails.map((detail) => ({
    ...detail,
    images: [...detail.images],
  })),
})

// Đa biến thể
const detailsproductHasVariants = ref({
  productDetails: product.value.productDetails.map((detail) => ({
    ...detail,
    images: [...detail.images],
  })),
})

// Đơn biến thể
const detailsproductSingle = ref({
  productDetails:
    product.value.hasVariants == false
      ? product.value.productDetails.map((detail) => ({
          ...detail,
          images: [...detail.images],
        }))
      : [
          {
            kichThuoc: '',
            mauSac: '',
            soLuongTon: 0,
            donGia: 0,
            images: [],
          },
        ],
})
watch(
  () => props.productinformation,
  (newVal) => {
    product.value = {
      ...newVal,
      confirmhasVariants: newVal.hasVariants,
    }
    detailsproductHasVariants.value.productDetails = newVal.productDetails.map((detail) => ({
      ...detail,
      images: [...detail.images],
    }))
    detailsproductSingle.value.productDetails =
      product.value.hasVariants == false || product.value.confirmhasVariants == false
        ? product.value.productDetails.map((detail) => ({
            ...detail,
            images: [...detail.images],
          }))
        : [
            {
              kichThuoc: '',
              mauSac: '',
              soLuongTon: 0,
              donGia: 0,
              images: [],
            },
          ]
  }
)

// Chuyển đổi cập nhật biến thể đơn thành đa biến thể và ngược lại
watch(
  () => product.value.confirmhasVariants,
  (newVal) => {
    if (newVal == true && product.value.hasVariants == false) {
      Swal.fire({
        title: 'Xác nhận chuyển đổi sản phẩm này thành sản phẩm đa biến thể chứ ?',
        showCancelButton: true,
        confirmButtonText: 'Xác nhận',
        cancelButtonText: 'Hủy',
      }).then(async (result) => {
        if (result.isConfirmed) {
          product.value.confirmhasVariants = true
        } else {
          product.value.confirmhasVariants = false
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
          product.value.confirmhasVariants = false
        } else {
          product.value.confirmhasVariants = true
        }
      })
    }
  }
)

</script>
<template>
  <!-- Modal -->
  <div
    class="modal fade"
    :id="`productDetailsModal_${productinformation.maSp}`"
    tabindex="-1"
    aria-labelledby="productModalLabel"
    aria-hidden="true"
    style="text-align: left"
  >
    <div class="modal-dialog modal-xl modal-dialog-scrollable">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title">Sửa Thông Tin Sản Phẩm ({{ productinformation.maSp }})</h5>
          <button
            type="button"
            :class="['btn-close', 'btn-close-white', 'close_modal_' + productinformation.maSp]"
            data-bs-dismiss="modal"
          ></button>
        </div>

        <div class="modal-body">
          <!-- Tên sản phẩm -->
          <div class="mb-3">
            <label class="form-label">Tên sản phẩm</label>
            <input readonly v-model="product.tenSanPham" type="text" class="form-control" />
          </div>
          <!-- Mô tả sản phẩm -->
          <div class="mb-3">
            <label class="form-label">Mô tả </label>
            <textarea
                disabled
              style="height: 200px"
              v-model="product.moTa"
              type="number"
              class="form-control"
            >
            </textarea>
          </div>
          <!-- Giá sản phẩm - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.confirmhasVariants">
            <label class="form-label"
              >Đơn giá
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input
            readonly
              v-model="detailsproductSingle.productDetails[0].donGia"
              type="number"
              class="form-control"
            />
          </div>
          <!-- Số lượng sản phẩm - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.confirmhasVariants">
            <label class="form-label"
              >Số lượng tồn
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <input
            readonly
              v-model="detailsproductSingle.productDetails[0].soLuongTon"
              type="number"
              class="form-control"
            />
          </div>
          <!-- Ảnh sản phẩm chính - chỉ hiển thị khi không có biến thể -->
          <div class="mb-3" v-if="!product.confirmhasVariants">
            <label class="form-label"
              >Ảnh sản phẩm
              <span style="color: red; font-style: italic"
                >(dành cho sản phẩm không có biến thể)</span
              ></label
            >
            <div class="d-flex flex-wrap gap-3 mt-3">
              <div
                v-for="(image, index) in detailsproductSingle.productDetails[0].images"
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
                <img
                  v-if="!image.preview"
                  :src="getUrlAPI.replace('api', '') + '/HinhAnh/Products/' + image.tenHinhAnh"
                  alt="Ảnh sản phẩm"
                  class="img-thumbnail"
                  style="max-width: 150px; max-height: 150px"
                />
              </div>
            </div>
          </div>

          <!-- Danh mục cha - con -->
          <div class="mb-4">
            <div v-for="(cat, index) in product.categoryDetails" :key="index" class="row g-2 mb-2">
              <div class="col">
                <label class="form-label">Danh mục cha</label>
                <select disabled v-model="cat.maDanhMucCha" class="form-select">
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
                <select disabled v-model="cat.maDanhMucCon" class="form-select">
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
            </div>
          </div>
          <!-- Chi tiết sản phẩm - chỉ hiển thị khi có biến thể -->
          <div class="mb-3" v-if="product.confirmhasVariants">
            <label class="form-label fw-bold">Chi tiết sản phẩm</label>
            <div
              v-for="(detail, index) in detailsproductHasVariants.productDetails"
              :key="index"
              class="border rounded p-3 mb-3"
            >
              <div class="row g-3">
                <div class="col-md-3">
                  <label class="form-label">Kích thước</label>
                  <input readonly v-model="detail.kichThuoc" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Màu sắc</label>
                  <input readonly v-model="detail.mauSac" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Số lượng tồn</label>
                  <input readonly v-model="detail.soLuongTon" type="number" class="form-control" />
                </div>
                <div class="col-md-3">
                  <label class="form-label">Đơn giá</label>
                  <input readonly v-model="detail.donGia" type="number" class="form-control" />
                </div>
              </div>

              <!-- Hình ảnh -->
              <div class="mt-3">
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
                    <img
                      v-if="!img.preview"
                      :src="getUrlAPI.replace('api', '') + `/HinhAnh/Products/${img.tenHinhAnh}`"
                      alt="Ảnh đã chọn"
                      style="max-width: 150px; max-height: 150px"
                      class="img-thumbnail rounded border"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style></style>