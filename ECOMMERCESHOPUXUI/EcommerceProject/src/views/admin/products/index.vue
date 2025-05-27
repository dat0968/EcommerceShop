<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import CreateProductModal from '../products/create.vue'
import EditProductModel from '../products/edit.vue'
import DetailProductModel from '../products/details.vue'
import Swal from 'sweetalert2'
const search = ref('')
const selectedCategory = ref('')
const sortByPrice = ref('')
const getUrlAPI = ref('https://localhost:7217/api')
const products = ref([])
const toTalPages = ref(1)
const pageSelected = ref(1)
const listBigCategories = ref([])
const listSmallCategories = ref([])
const fetchAPICategories = async () => {
  try {
    const response = await fetch(`${getUrlAPI.value}/Categories/GetAllCategories`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) throw new Error('Lỗi khi gọi API')

    const result = await response.json()
    listBigCategories.value = result.listBigCategories
    listSmallCategories.value = result.listSmallCategories
  } catch (error) {
    console.error('Lỗi fetchAPICategories:', error)
  }
}
const fetchAPIProducts = async () => {
  try {
    const response = await fetch(
      `${getUrlAPI.value}/Products?search=${search.value}&selectedCategory=${selectedCategory.value}&sortByPrice=${sortByPrice.value}&page=${pageSelected.value}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    )

    if (!response.ok) throw new Error('Lỗi khi gọi API')

    const result = await response.json()
    products.value = result.data
    toTalPages.value = result.toTalPages
  } catch (error) {
    console.error('Lỗi fetchAPIProducts:', error)
  }
}
onMounted(() => {
  fetchAPIProducts()
  fetchAPICategories()
})
watch(pageSelected.value, () => {
  fetchAPIProducts()
})

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page
  }
}

// Tìm kiếm
function filterProducts() {
  fetchAPIProducts()
}

async function RemoveProducts(productid) {
  try {
    Swal.fire({
      title: 'Bạn có muốn xóa sản phẩm này không ?',
      showDenyButton: true,
      showCancelButton: false,
      confirmButtonText: 'Có',
      denyButtonText: `Không`,
    }).then(async (result) => {
      if (result.isConfirmed) {
        const response = await fetch(`${getUrlAPI.value}/Products/${productid}/Cancel`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
        })
        const result = await response.json()
        if (result.success) {
          Swal.fire({
            title: 'Đã xóa thông tin sản phẩm',
            icon: 'success',
            timer: 1500, // 2000 ms = 2 giây
            showConfirmButton: false, // ẩn nút OK
            timerProgressBar: true, // hiển thị thanh tiến trình
          })
          fetchAPIProducts()
        }
      } else if (result.isDenied) {
        Swal.clickCancel()
      }
    })
  } catch (error) {
    console.log(error)
  }
}
</script>
<template>
  <div class="container mt-4">
    <!-- Tiêu đề chính -->
    <div style="margin-top: 90px" class="mb-4 text-center">
      <h1 class="fw-bold text-uppercase text-dark">Quản lý sản phẩm</h1>
    </div>
    <!-- Bộ lọc và tìm kiếm -->
    <div class="row g-3 mb-3">
      <div class="col-md-4">
        <input
          style="background-color: white"
          v-model="search"
          @input="filterProducts()"
          type="text"
          class="form-control"
          placeholder="Tìm kiếm sản phẩm..."
        />
      </div>
      <div class="col-md-4">
        <select @change="filterProducts()" v-model="selectedCategory" class="form-select">
          <option value="">Tất cả danh mục</option>
          <option
            v-for="category in listBigCategories"
            :key="category.maDanhMucCha"
            :value="category.maDanhMucCha"
          >
            {{ category.tenDanhMucCha }}
          </option>
        </select>
      </div>
      <div class="col-md-4">
        <select @change="filterProducts()" v-model="sortByPrice" class="form-select">
          <option value="">Sắp xếp theo...</option>
          <option value="desc">Khoảng giá (giảm dần)</option>
          <option value="asc">Khoảng giá (tăng dần)</option>
        </select>
      </div>
    </div>
    <!-- Tiêu đề phụ và nút thêm -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <button
        type="button"
        data-bs-toggle="modal"
        data-bs-target="#productModal"
        class="btn btn-primary"
      >
        + Thêm sản phẩm
      </button>
    </div>
    <CreateProductModal
      :listBigCategories="listBigCategories"
      :listSmallCategories="listSmallCategories"
      @update-success="fetchAPIProducts"
    />
    <!-- Bảng sản phẩm -->
    <div class="table-responsive">
      <table class="table table-bordered table-hover" style="text-align: center">
        <thead class="table-light">
          <tr>
            <th>Mã sản phẩm</th>
            <th>Tên sản phẩm</th>
            <th>Hình ảnh</th>
            <th>Khoảng giá</th>
            <th>Số lượng</th>
            <th>Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in products" :key="product.id">
            <td>{{ product.maSp }}</td>
            <td>{{ product.tenSanPham }}</td>
            <td>
              <img
                :src="`${getUrlAPI.replace('/api', '')}/HinhAnh/Products/${
                  product.productDetails[0].images[0].tenHinhAnh
                }`"
                alt="Hình ảnh sản phẩm"
                style="width: 60px; height: 60px; object-fit: cover"
                v-if="
                  product.productDetails.length > 0 &&
                  product.productDetails[0].images &&
                  product.productDetails[0].images.length > 0
                "
              />
              <span v-else class="text-muted"> Không có ảnh </span>
            </td>
            <td>{{ product.khoangGia }}</td>
            <td>{{ product.soLuong }}</td>
            <td>
              <button
                type="button"
                data-bs-toggle="modal"
                :data-bs-target="`#productModal_${product.maSp}`"
                class="btn btn-sm btn-warning me-1"
              >
                Sửa
              </button>
              <EditProductModel
                :productinformation="product"
                :listBigCategories="listBigCategories"
                :listSmallCategories="listSmallCategories"
                @update-success="fetchAPIProducts"
              />
              <button
                type="button"
                data-bs-toggle="modal"
                :data-bs-target="`#productDetailsModal_${product.maSp}`"
                class="btn btn-sm btn-info me-1"
              >
                Chi tiết
              </button>
              <DetailProductModel
                :productinformation="product"
                :listBigCategories="listBigCategories"
                :listSmallCategories="listSmallCategories"
              />
              <button @click="RemoveProducts(product.maSp)" class="btn btn-sm btn-danger">
                Xóa
              </button>
            </td>
          </tr>
          <tr v-if="products.length === 0">
            <td colspan="6" class="text-center text-muted">Không có sản phẩm nào.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Phân trang -->
    <nav style="margin-bottom: 60px" class="d-flex justify-content-center mt-3">
      <ul class="pagination">
        <li @click="ChangePage(1)" class="page-item"><a class="page-link" href="#">Đầu</a></li>
        <li
          @click="ChangePage(page)"
          v-for="page in toTalPages"
          :key="page"
          :class="['page-item', { active: page == pageSelected }]"
        >
          <a class="page-link" href="#">{{ page }}</a>
        </li>
        <li @click="ChangePage(toTalPages)" class="page-item">
          <a class="page-link" href="#">Cuối</a>
        </li>
      </ul>
    </nav>
  </div>
</template>

<style scoped>
.table td,
.table th {
  vertical-align: middle;
}
</style>
