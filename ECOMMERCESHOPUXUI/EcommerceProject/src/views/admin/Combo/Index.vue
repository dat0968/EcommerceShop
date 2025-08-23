<script setup>
import { onMounted, ref, watch } from 'vue'
import CreateCombo from '../Combo/Create.vue'
import EditCombo from '../Combo/Edit.vue'
import DetailCombo from '../Combo/Details.vue'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../../../src/constants/api.js'
import Cookies from 'js-cookie'
import { useRouter } from 'vue-router'
import { decodeToken, validateToken } from '@/utils/auth'
const router = useRouter()
const listCombo = ref([])
const filteredCombos = ref([]) // Danh sách combo đã lọc và sắp xếp
const ListProduct = ref([])
const TotalPages = ref(0)
const CurrentPage = ref(1)
const valueSearch = ref('')
const discountFilter = ref('all') // Bộ lọc mức giảm giá
const sortOrder = ref('default') // Thứ tự sắp xếp (asc: A-Z, desc: Z-A)
let accesstoken = Cookies.get('accessToken')
let refreshtoken = Cookies.get('refreshToken')
const readToken = ref({})
const roleUser = ref('')
onMounted(async () => {
  const validatetoken = await validateToken(accesstoken, refreshtoken)
    if (validatetoken.isValid == false) {
      router.push('/LoginStaff')
      return
    }
    accesstoken = validatetoken.newAccessToken
    readToken.value = decodeToken(accesstoken)
    roleUser.value = readToken.value.Role
})


const role = ref('')
const getUrlAPI = ref(GetApiUrl())
const activeTab = ref('all') // Tab mặc định là 'all'

// Hàm kiểm tra validation
const validateDiscount = (phanTramGiam, soTienGiam) => {
  if (phanTramGiam !== null && phanTramGiam !== undefined) {
    if (phanTramGiam < 0) {
      Swal.fire({
        title: 'Lỗi',
        text: 'Phần trăm giảm không được nhỏ hơn 0%',
        icon: 'error',
        confirmButtonText: 'OK'
      });
      return false;
    }
    if (phanTramGiam > 100) {
      Swal.fire({
        title: 'Lỗi',
        text: 'Phần trăm giảm không được lớn hơn 100%',
        icon: 'error',
        confirmButtonText: 'OK'
      });
      return false;
    }
  }
  if (soTienGiam !== null && soTienGiam !== undefined) {
    if (soTienGiam < 0) {
      Swal.fire({
        title: 'Lỗi',
        text: 'Số tiền giảm không được nhỏ hơn 0 VNĐ',
        icon: 'error',
        confirmButtonText: 'OK'
      });
      return false;
    }
  }
  return true;
};

// Hàm gọi từ component con để validate trước khi lưu
const handleSave = async (comboData) => {
  if (!validateDiscount(comboData.phanTramGiam, comboData.soTienGiam)) {
    return false;
  }
  // Logic lưu dữ liệu (giả định gọi API hoặc xử lý trong component con)
  return true;
};

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


// Biến để kiểm soát sắp xếp mặc định
const defaultSortByDate = ref(true);

// Hàm lọc và sắp xếp combo
function applyFilter() {
  let combos = [...listCombo.value];

  // Lọc theo tab tình trạng
  if (activeTab.value === 'active') {
    combos = combos.filter(combo => isActive(combo.ngayKetThuc));
  } else if (activeTab.value === 'expired') {
    combos = combos.filter(combo => !isActive(combo.ngayKetThuc));
  }

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

  // Sắp xếp theo ngày kết thúc (mới nhất lên đầu) làm mặc định
  if (defaultSortByDate.value) {
    combos.sort((a, b) => {
      const dateA = new Date(a.ngayKetThuc || a.ngayBatDau || '1970-01-01');
      const dateB = new Date(b.ngayKetThuc || b.ngayBatDau || '1970-01-01');
      return dateB - dateA; // Sắp xếp giảm dần theo ngày
    });
  }

  // Sắp xếp theo tenCombo dựa trên sortOrder nếu có, chỉ khi người dùng chọn
  if (sortOrder.value == 'asc' && !defaultSortByDate.value) {
    combos.sort((a, b) => a.tenCombo.localeCompare(b.tenCombo, 'vi', { sensitivity: 'base' }));
  } else if (sortOrder.value === 'desc' && !defaultSortByDate.value) {
    combos.sort((a, b) => b.tenCombo.localeCompare(a.tenCombo, 'vi', { sensitivity: 'base' }));
  }
  else if (sortOrder.value === 'default' && !defaultSortByDate.value) {
    combos.sort((a, b) => b.tenCombo.localeCompare(a.tenCombo, 'vi', { sensitivity: 'base' }));
  }

  filteredCombos.value = combos;
}

// Cập nhật khi sortOrder thay đổi
watch(sortOrder, (newVal) => {
  defaultSortByDate.value = false; // Tắt sắp xếp mặc định theo ngày khi người dùng chọn sắp xếp tên
  applyFilter();
});


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
  defaultSortByDate.value = true; // Đảm bảo sắp xếp theo ngày khi vào trang
});

// Watch activeTab to reapply filter when changed
watch(activeTab, () => {
  ReturnCombo();
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
        <input type="text" class="form-control shadow-sm border-primary bg-white" placeholder="🔍 Nhập tên combo..."
          v-model="valueSearch" @input="ReturnCombo()" />
      </div>
      <div class="col-md-3">
        <select class="form-select shadow-sm border-primary" v-model="discountFilter" @change="ReturnCombo()">
          <option value="all">Tất cả mức giảm</option>
          <option value="percent">Giảm theo phần trăm</option>
          <option value="fixed">Giảm theo số tiền</option>
          <option value="none">Không giảm giá</option>
        </select>
      </div>
      <div class="col-md-3">
        <select class="form-select shadow-sm border-primary" v-model="sortOrder" @change="ReturnCombo()">
          <option value="default">Sắp xếp</option> <!-- Thêm tùy chọn mặc định -->
          <option value="asc">Sắp xếp: A đến Z</option>
          <option value="desc">Sắp xếp: Z đến A</option>
        </select>
      </div>
    </div>

    <!-- Nút thêm combo -->
    <div class="mb-4">
      <button v-if="roleUser.toLowerCase() == 'admin'" type="button" class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#exampleModal">
        ➕ Thêm combo
      </button>
    </div>
    <CreateCombo :validateDiscount="validateDiscount" :handleSave="handleSave" />

    <!-- Tabs cho tình trạng -->
    <div class="status-tabs mb-4">
      <button
        v-for="tab in [
          { value: 'all', label: 'Tất cả' },
          { value: 'active', label: 'Đang hoạt động' },
          { value: 'expired', label: 'Hết hạn' }
        ]"
        :key="tab.value"
        :class="['tab-button', { active: activeTab === tab.value }]"
        @click="activeTab = tab.value; ReturnCombo()"
      >
        {{ tab.label }}
      </button>
    </div>

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
              <img :src="getUrlAPI + '/HinhAnh/AnhCombo/' + combo.hinh" alt="Combo Image" width="50" height="50"
                style="object-fit: cover; border-radius: 5px" />
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
              {{ new Date(combo.ngayKetThuc) < new Date() ? 'Hết hạn' : 'Đang hoạt động' }} </td>
            <td class="text-center">
              <div class="d-flex justify-content-center align-items-center flex-wrap gap-2">
                <template v-if="combo.ngayKetThuc && new Date(combo.ngayKetThuc) >= new Date()">
                  <button type="button" data-bs-toggle="modal" :data-bs-target="`#comboEditModal_${combo.maCombo}`"
                    class="btn btn-sm btn-warning" v-if="roleUser.toLowerCase() == 'admin'">
                    Sửa
                  </button>
                  <EditCombo :Combo="combo" :ListProduct="ListProduct" />

                  <button v-if="roleUser.toLowerCase() == 'admin'" @click="removeCombo(combo.maCombo)" class="btn btn-danger btn-sm">
                    Xóa
                  </button>
                </template>

                <button type="button" data-bs-toggle="modal" :data-bs-target="`#comboDetailModal_${combo.maCombo}`"
                  class="btn btn-sm btn-info text-white">
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
          <li v-for="page in TotalPages" :key="page" :class="{ active: page === CurrentPage }" class="page-item">
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

/* Thêm hiệu ứng hover cho tất cả các nút */
.btn-warning:hover {
  color: #fff !important;
  background-color: #be9629 !important;
}

.btn-danger:hover {
  color: #fff !important;
  background-color: #dc3545 !important;
}

.btn-info:hover {
  color: #fff !important;
  background-color: #17a2b8 !important;
}

/* CSS cho tab giống MyOrders.vue */
.status-tabs {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.25rem;
  background: white;
  padding: 0.25rem;
  border-radius: 0.75rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-button {
  padding: 0.75rem 0.5rem;
  border: none;
  background: transparent;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
}

.tab-button:hover {
  background-color: #f1f5f9;
  color: #334155;
}

.tab-button.active {
  background-color: #3b82f6;
  color: white;
}
</style>