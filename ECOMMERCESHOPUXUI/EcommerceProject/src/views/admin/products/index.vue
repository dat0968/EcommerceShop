<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { GetApiUrl } from '@/constants/api'
import { decodeToken, validateToken } from '@/utils/auth'
import CreateProductModal from '../products/create.vue'
import EditProductModel from '../products/edit.vue'
import DetailProductModel from '../products/details.vue'
import Swal from 'sweetalert2'
import Cookies from 'js-cookie'
import axios from 'axios'
const search = ref('')
const categoryBigSelected = ref('')
const categorySmallSelected = ref('')
const sortByPrice = ref('')
const getUrlAPI = ref(GetApiUrl())
const products = ref([])
const toTalPages = ref(1)
const pageSelected = ref(1)
const listBigCategories = ref([])
const listSmallCategories = ref([])
const accessToken = ref(Cookies.get('accessToken'))
const refreshToken = ref(Cookies.get('refreshToken'))
const loading = ref(false)
const readToken = ref({})
const roleUser = ref('')
const router = useRouter()
const fetchAPICategories = async () => {
  try {
    loading.value = true
    const response = await fetch(`${getUrlAPI.value}/api/Categories/GetCategoriesforShop`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    if (!response.ok) throw new Error('Lỗi khi gọi API')

    const result = await response.json()
    listBigCategories.value = result.listBigCategory.map((p) => {
      return {
        maDanhMucCha: p.maDanhMucCha,
        tenDanhMucCha: p.tenDanhMucCha,
      }
    })
    listSmallCategories.value = result.listSmallCategory.map((p) => {
      return {
        maDanhMucCon: p.maDanhMucCon,
        tenDanhMucCon: p.tenDanhMucCon,
      }
    })
  } catch (error) {
    console.error('Lỗi fetchAPICategories:', error)
  } finally {
    loading.value = false
  }
}
const fetchAPIProducts = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value)
    if (validatetoken.isValid == false) {
      router.push('/Login')
      return
    } else {
      accessToken.value = validatetoken.newAccessToken
      readToken.value = decodeToken(accessToken.value)
      loading.value = true
      roleUser.value = readToken.value.Role
      const response = await fetch(
        `${getUrlAPI.value}/api/Products?search=${search.value}&selectedBigCategory=${categoryBigSelected.value}&selectedSmallCategory=${categorySmallSelected.value}&sortByPrice=${sortByPrice.value}&page=${pageSelected.value}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            Authorization: 'Bearer ' + accessToken.value,
          },
        }
      )

      if (!response.ok) throw new Error('Lỗi khi gọi API')

      const result = await response.json()
      products.value = result.data
      toTalPages.value = result.toTalPages
    }
  } catch (error) {
    console.error('Lỗi fetchAPIProducts:', error)
  } finally {
    loading.value = false
  }
}
onMounted(async () => {
  fetchAPIProducts()
  fetchAPICategories()
})

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page
    fetchAPIProducts()
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
        const validatetoken = await validateToken(accessToken.value, refreshToken.value)
        if (validatetoken.isValid == false) {
          router.push('/Login')
          return
        } else {
          accessToken.value = validatetoken.newAccessToken
          readToken.value = decodeToken(accessToken.value)
          roleUser.value = readToken.value.Role
          const response = await fetch(`${getUrlAPI.value}/api/Products/${productid}/Cancel`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
              Authorization: 'Bearer ' + accessToken.value,
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
        }
      } else if (result.isDenied) {
        Swal.clickCancel()
      }
    })
  } catch (error) {
    console.log(error)
  }
}

async function ExportExcel() {
  const validatetoken = await validateToken(accessToken.value, refreshToken.value)
  if (validatetoken.isValid == false) {
    router.push('/Login')
    return
  }
  accessToken.value = validatetoken.newAccessToken
  axios
    .get(getUrlAPI.value + '/api/Products/xuat-excel', {
      headers: {
        Authorization: `Bearer ${accessToken.value}`,
      },
      responseType: 'blob',
    })
    .then((response) => {
      //Chuyển Blod file thành URL để trình duyệt tải về
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      //Khi người dùng tải về, trình duyệt sẽ lưu file với tên này.
      link.setAttribute('download', 'DanhSachSanPham.xlsx')
      //Thêm thẻ vào trang để có thể gọi click().
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    })
    .catch((error) => {
      console.error('Tải file thất bại:', error)
    })
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
      <div class="col-md-3">
        <input
          style="background-color: white"
          v-model="search"
          @input="filterProducts()"
          type="text"
          class="form-control"
          placeholder="Tìm kiếm sản phẩm..."
        />
      </div>
      <div class="col-md-3">
        <select @change="filterProducts()" v-model="categoryBigSelected" class="form-select">
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
      <div class="col-md-3">
        <select @change="filterProducts()" v-model="sortByPrice" class="form-select">
          <option value="">Sắp xếp theo...</option>
          <option value="desc">Khoảng giá (giảm dần)</option>
          <option value="asc">Khoảng giá (tăng dần)</option>
        </select>
      </div>
      <div class="col-md-3">
        <button type="button" class="btn btn-danger" @click="ExportExcel()">Xuất Excel</button>
      </div>
    </div>
    <!-- Tiêu đề phụ và nút thêm -->
    <div class="d-flex justify-content-between align-items-center mb-3">
      <button
        v-if="roleUser.toLowerCase() == 'admin'"
        type="button"
        data-bs-toggle="modal"
        data-bs-target="#productModal"
        class="btn btn-primary"
      >
        + Thêm sản phẩm
      </button>
    </div>
    <CreateProductModal
      v-if="roleUser.toLowerCase() == 'admin'"
      :listBigCategories="listBigCategories"
      :listSmallCategories="listSmallCategories"
      @update-success="fetchAPIProducts"
    />
    <div v-if="loading" class="my-loading-spinner text-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <div class="fw-semibold text-primary mt-2">Đang tải dữ liệu...</div>
    </div>
    <!-- Bảng sản phẩm -->
    <div v-else class="table-responsive">
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
                v-if="roleUser.toLowerCase() == 'admin'"
                type="button"
                data-bs-toggle="modal"
                :data-bs-target="`#productModal_${product.maSp}`"
                class="btn btn-sm btn-warning me-1"
              >
                Sửa
              </button>
              <EditProductModel
                v-if="roleUser.toLowerCase() == 'admin'"
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
              <button
                v-if="roleUser.toLowerCase() == 'admin'"
                @click="RemoveProducts(product.maSp)"
                class="btn btn-sm btn-danger"
              >
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
