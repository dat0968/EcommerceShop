<script setup>
import { onMounted, ref } from 'vue'
import CreateCombo from '../Combo/Create.vue'
import EditCombo from '../Combo/Edit.vue'
import DetailCombo from '../Combo/Details.vue'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../../../src/constants/api.js'
import Cookies from 'js-cookie'
import { useRouter } from 'vue-router'
const router = useRouter()
let getApiUrl = GetApiUrl()
const listCombo = ref([])
const filteredCombos = ref([]) // Danh sách combo đã lọc và sắp xếp
const ListProduct = ref([])
const TotalPages = ref(0)
const CurrentPage = ref(1)
const valueSearch = ref('')
const discountFilter = ref('all') // Bộ lọc mức giảm giá
const sortOrder = ref('asc') // Thứ tự sắp xếp (asc: A-Z, desc: Z-A)
let accesstoken = Cookies.get('accessToken')
let refreshtoken = Cookies.get('refreshToken')
const role = ref('')
const getUrlAPI = ref('https://localhost:7217')
const isActive = (ngayKetThuc) => {
  return ngayKetThuc && new Date(ngayKetThuc) >= new Date();
};
// Hàm định dạng ngày
function formatDate(dateString) {
  if (!dateString) return 'Chưa xác định';
  const date = new Date(dateString);
  return date.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  });
}
const checkToken = () => {
  if (!accesstoken || accesstoken === '' || accesstoken === null) {
    Swal.fire({
      title: 'Bạn chưa đăng nhập!',
      text: 'Vui lòng đăng nhập để tiếp tục.',
      icon: 'warning',
      confirmButtonText: 'Đăng nhập ngay'
    }).then(() => {
      router.push('/LoginStaff')
    })
  }
}
async function fetchCombo() {
  try {
    let url = `${getUrlAPI.value}/api/Combos?page=${CurrentPage.value}&search=${encodeURIComponent(valueSearch.value)}`;
    
    console.log('Đang lấy danh sách combo với URL:', url);
    console.log('Access token:', accesstoken);
    const response = await fetch(url, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accesstoken}`,
      },
    });
    
    console.log('Trạng thái phản hồi:', response.status);
    if (!response.ok) {
      throw new Error(`Lỗi khi lấy dữ liệu: ${response.status} - ${response.statusText}`);
    }
    
    const data = await response.json();
    console.log('Phản hồi API:', data);
    
    if (!data || !data.data) {
      throw new Error('Dữ liệu API không hợp lệ hoặc không có danh sách combo');
    }
    
    listCombo.value = data.data || [];
    TotalPages.value = data.totalPages || 1;
    CurrentPage.value = data.currentPage || 1;
    console.log('Danh sách combo:', listCombo.value);
    
    // Áp dụng bộ lọc và sắp xếp
    applyFilter();
  } catch (error) {
    console.error('Lỗi fetchCombo:', error);
    Swal.fire({
      title: 'Lỗi',
      text: `Không thể tải danh sách combo: ${error.message}`,
      icon: 'error',
      confirmButtonText: 'OK'
    });
  }
}
async function fetchProducts() {
  try {
    const response = await fetch(
      `${getUrlAPI.value}/api/Products`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accesstoken}`,
        },
      }
    );

    if (!response.ok) {
      //throw new Error(`Lỗi khi lấy dữ liệu sản phẩm: ${response.status} - ${response.statusText}`);
    }

    const result = await response.json();
    ListProduct.value = result.data || [];
    console.log('Danh sách sản phẩm:', ListProduct.value);
  } catch (error) {
    // console.error('Lỗi fetchProducts:', error);
    // Swal.fire({
    //   title: 'Lỗi',
    //   text: `Không thể tải danh sách sản phẩm: ${error.message}`,
    //   icon: 'error',
    //   confirmButtonText: 'OK'
    // });
  }
}

// Hàm lọc và sắp xếp combo
function applyFilter() {
  let combos = [...listCombo.value];

  // Lọc theo mức giảm giá
  if (discountFilter.value !== 'all') {
    combos = combos.filter(combo => {
      if (discountFilter.value === 'percent') {
        return combo.phanTramGiam && combo.phanTramGiam > 0;
      } else if (discountFilter.value === 'fixed') {
        return combo.soTienGiam && combo.soTienGiam > 0;
      } else if (discountFilter.value === 'none') {
        return (!combo.phanTramGiam || combo.phanTramGiam === 0) && (!combo.soTienGiam || combo.soTienGiam === 0);
      }
      return true;
    });
  }

  // Sắp xếp theo maCombo (giảm dần để combo mới lên đầu) rồi theo tenCombo (A-Z hoặc Z-A)
  combos.sort((a, b) => {
    // Sắp xếp theo maCombo giảm dần (combo mới có maCombo lớn hơn)
    if (a.maCombo !== b.maCombo) {
      return b.maCombo - a.maCombo;
    }
    // Sắp xếp theo tenCombo dựa trên sortOrder
    return sortOrder.value === 'asc'
      ? a.tenCombo.localeCompare(b.tenCombo, 'vi', { sensitivity: 'base' })
      : b.tenCombo.localeCompare(a.tenCombo, 'vi', { sensitivity: 'base' });
  });

  filteredCombos.value = combos;

  // if (filteredCombos.value.length === 0) {
  //   Swal.fire({
  //     title: 'Không tìm thấy combo',
  //     text: 'Không có combo nào phù hợp với bộ lọc hiện tại.',
  //     icon: 'info',
  //     confirmButtonText: 'OK'
  //   });
  // }
}

const ChangePage = (page) => {
  if (page >= 1 && page <= TotalPages.value) {
    console.log("Chuyển trang " + page);
    CurrentPage.value = page;
    fetchCombo();
  }
}

const ReturnCombo = () => {
  CurrentPage.value = 1; // Reset về trang 1 khi thay đổi bộ lọc hoặc sắp xếp
  fetchCombo();
}

async function removeCombo(id) {
  try {
    Swal.fire({
      title: 'Bạn có muốn xóa combo này không?',
      showDenyButton: true,
      showCancelButton: false,
      confirmButtonText: 'Có',
      denyButtonText: `Không`,
    }).then(async (result) => {
      if (result.isConfirmed) {
        const response = await fetch(`${getUrlAPI.value}/api/Combos/${id}/Cancel`, {
method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accesstoken}`,
          },
        });

        if (!response.ok) {
          throw new Error(`Lỗi khi xóa combo: ${response.status} - ${response.statusText}`);
        }
        Swal.fire({
          title: 'Đã xóa thông tin combo!',
          icon: 'success',
          draggable: true,
        });
        fetchCombo(); // Tải lại danh sách sau khi xóa
      }
    });
  } catch (error) {
    console.error('Lỗi removeCombo:', error);
    Swal.fire({
      title: 'Lỗi',
      text: `Không thể xóa combo: ${error.message}`,
      icon: 'error',
      confirmButtonText: 'OK'
    });
  }
}

onMounted(() => {
  fetchCombo();
  fetchProducts();
  checkToken();
});
</script>

<template>
  <div class="container mt-4">
    <div style="margin-top: 110px" class="mb-4 text-center">
      <h1 class="fw-bold text-uppercase text-dark" style="font-size: 3rem;">Quản lý Combo</h1>
    </div>
    <!-- Thanh tìm kiếm, bộ lọc và sắp xếp -->
    <div class="row g-3 mb-3 align-items-center">
      <div class="col-md-3">
        <input
          type="text"
          class="form-control shadow-sm border-primary bg-white"
          placeholder="🔍 Nhập tên combo..."
          v-model="valueSearch"
          @input="ReturnCombo()"
        />
      </div>
      <div class="col-md-3">
        <select
          class="form-select shadow-sm border-primary"
          v-model="discountFilter"
          @change="ReturnCombo()"
        >
          <option value="all">Tất cả mức giảm</option>
          <option value="percent">Giảm theo phần trăm</option>
          <option value="fixed">Giảm theo số tiền</option>
          <option value="none">Không giảm giá</option>
        </select>
      </div>
      <div class="col-md-3">
        <select
          class="form-select shadow-sm border-primary"
          v-model="sortOrder"
          @change="ReturnCombo()"
        >
          <option value="asc">Sắp xếp: A đến Z</option>
          <option value="desc">Sắp xếp: Z đến A</option>
        </select>
      </div>
    </div>

    <!-- Nút thêm combo -->
    <div class="mb-4">
      <button
        type="button"
        class="btn btn-primary"
        data-bs-toggle="modal"
        data-bs-target="#exampleModal"
      >
        ➕ Thêm combo
      </button>
    </div>
    <CreateCombo />
    
    <!-- Bảng dữ liệu -->
    <div class="table-responsive">
      <table class="table table-bordered table-hover" style="text-align: center">
        <thead class="table-light">
          <tr>
            <th>Mã combo</th>
            <th>Tên combo</th>
            <th>Hình</th>
            <th>Số lượng</th>
            <th>Mức giảm</th>
            <th>Ngày bắt đầu</th>
            <th>Ngày kết thúc</th>
            <th>Tình trạng</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
<tr v-for="combo in filteredCombos" :key="combo.maCombo">
            <td class="text-center">{{ combo.maCombo }}</td>
            <td class="text-center">{{ combo.tenCombo }}</td>
            <td class="text-center">
              <img
                :src="getApiUrl + '/HinhAnh/AnhCombo/' + combo.hinh"
                alt="Combo Image"
                width="50"
                height="50"
                style="object-fit: cover; border-radius: 5px"
              />
            </td>
            <td class="text-center">{{ combo.soLuong }}</td>
            <td class="text-center">
              {{
                combo.soTienGiam == null || combo.soTienGiam == 0
                  ? (combo.phanTramGiam ? '-' + combo.phanTramGiam + '%' : 'Không giảm')
                  : '-' + combo.soTienGiam + ' VNĐ'
              }}
            </td>
            <td class="text-center">{{ formatDate(combo.ngayBatDau) }}</td>
            <td class="text-center">{{ formatDate(combo.ngayKetThuc) }}</td>
            <td class="text-center">
              {{ new Date(combo.ngayKetThuc) < new Date() ? 'Hết hạn' : 'Đang hoạt động' }}
            </td>
            <td class="text-center">
              <div class="action-buttons">
                <div v-if="combo.ngayKetThuc && new Date(combo.ngayKetThuc) >= new Date()">
                  <button
                    type="button"
                    data-bs-toggle="modal"
                    :data-bs-target="`#comboEditModal_${combo.maCombo}`"
                    class="btn btn-sm btn-warning me-1"
                  >
                    Sửa
                  </button>
                  <EditCombo :Combo="combo" :ListProduct="ListProduct" />
                  <button
                    @click="removeCombo(combo.maCombo)"
                    class="btn btn-danger btn-sm me-1"
                  >
                    Xóa
                  </button>
                </div>
                <button
                  type="button"
                  data-bs-toggle="modal"
                  :data-bs-target="`#comboDetailModal_${combo.maCombo}`"
                  class="btn btn-sm btn-info me-1"
                >
                  Chi tiết
                </button>
                <DetailCombo :Combo="combo" :ListProduct="ListProduct" />
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Phân trang -->
    <div class="d-flex justify-content-center mt-4">
      <nav>
        <ul class="pagination">
          <li class="page-item" :class="{ disabled: CurrentPage === 1 }">
            <a class="page-link" @click="ChangePage(CurrentPage - 1)" tabindex="-1">«</a>
          </li>
          <li
            v-for="page in TotalPages"
            :key="page"
            :class="{ active: page === CurrentPage }"
            class="page-item"
          >
            <a class="page-link" @click="ChangePage(page)"> {{ page }} </a>
          </li>
<li class="page-item" :class="{ disabled: CurrentPage === TotalPages }">
            <a class="page-link" @click="ChangePage(CurrentPage + 1)">»</a>
          </li>
        </ul>
      </nav>
    </div>
  </div>
</template>

<style>
.sortable {
  cursor: pointer;
  user-select: none;
}
.sortable:hover {
  color: #f8d210;
}
/* CSS cho cột Thao tác */
.action-buttons {
  display: flex;
  gap: 5px;
  align-items: center;
  justify-content: center;
}

.action-buttons .btn {
  margin: 0;
}
</style>
