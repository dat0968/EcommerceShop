<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import CreateProductModal from '../products/create.vue'
const search = ref('')
const selectedCategory = ref('')
const sortBy = ref('')
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
    const response = await fetch(`${getUrlAPI.value}/Products?page=${pageSelected.value}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    })

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
watch(pageSelected, () => {
  fetchAPIProducts()
})
const categoryMap = {
  tui: 'Túi xách',
  balo: 'Ba lô',
  vi: 'Ví',
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page
  }
}

// Lọc và sắp xếp
const filteredAndSortedProducts = computed(() => {
  let filtered = products.value.filter((p) => {
    return (
      (!search.value || p.name.toLowerCase().includes(search.value.toLowerCase())) &&
      (!selectedCategory.value || p.category === selectedCategory.value)
    )
  })

  switch (sortBy.value) {
    case 'name':
      filtered.sort((a, b) => a.name.localeCompare(b.name))
      break
    case 'price':
      filtered.sort((a, b) => a.price - b.price)
      break
    case 'quantity':
      filtered.sort((a, b) => b.quantity - a.quantity)
      break
  }

  return filtered
})
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
          type="text"
          class="form-control"
          placeholder="Tìm kiếm sản phẩm..."
        />
      </div>
      <div class="col-md-4">
        <select v-model="selectedCategory" class="form-select">
          <option value="">Tất cả danh mục</option>
          <option value="tui">Túi xách</option>
          <option value="balo">Ba lô</option>
          <option value="vi">Ví</option>
        </select>
      </div>
      <div class="col-md-4">
        <select v-model="sortBy" class="form-select">
          <option value="">Sắp xếp theo...</option>
          <option value="name">Tên sản phẩm (A-Z)</option>
          <option value="price">Giá (thấp - cao)</option>
          <option value="quantity">Số lượng (cao - thấp)</option>
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
    <CreateProductModal :listBigCategories="listBigCategories" :listSmallCategories="listSmallCategories" />
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
          <tr v-for="product in filteredAndSortedProducts" :key="product.id">
            <td>{{ product.maSp }}</td>
            <td>{{ product.tenSanPham }}</td>
            <td>trống</td>
            <td>{{ product.khoangGia }}</td>
            <td>{{ product.soLuong }}</td>
            <td>
              <button class="btn btn-sm btn-warning me-1">Sửa</button>
              <button class="btn btn-sm btn-info me-1">Chi tiết</button>
              <button class="btn btn-sm btn-danger">Xóa</button>
            </td>
          </tr>
          <tr v-if="filteredAndSortedProducts.length === 0">
            <td colspan="6" class="text-center text-muted">Không có sản phẩm nào.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Phân trang -->
    <nav class="d-flex justify-content-center mt-3">
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
