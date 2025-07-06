<script setup>
import { ref, watch, nextTick, onMounted, onUnmounted } from 'vue';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../../../src/constants/api.js';
import * as bootstrap from 'bootstrap';
import Cookies from 'js-cookie';
let getApiUrl = GetApiUrl();

const combo = ref({
  tenCombo: '',
  hinh: null,
  soTienGiam: 0,
  phanTramGiam: 0,
  soLuong: 1,
  moTa: '',
  isActive: true,
  ngayBatDau: '',
  ngayKetThuc: '',
  chitietcombos: [
    {
      maSp: '',
      soLuongSp: 1,
    },
  ],
});

// Trạng thái modal chọn sản phẩm
const showProductModal = ref(false);
const selectedDetailIndex = ref(null);
const productCurrentPage = ref(1);
const productTotalPages = ref(1);
const toTalPages = ref(1);
const pageSelected = ref(1);
const productList = ref([]);
const productMap = ref({});
const search = ref('');
const token = Cookies.get('accessToken');
// Kích hoạt/ẩn modal
watch(showProductModal, (newValue) => {
  console.log('showProductModal thay đổi:', newValue);
  nextTick(() => {
    const modalElement = document.getElementById('productModal');
    if (modalElement) {
      const modal = bootstrap.Modal.getOrCreateInstance(modalElement, { backdrop: true });
      if (newValue) {
        console.log('Mở #productModal');
        modal.show();
        setTimeout(() => {
          const searchInput = document.querySelector('.modal-body input[type="text"]');
          if (searchInput) searchInput.focus();
        }, 500);
      } else {
        console.log('Đóng #productModal');
        modal.hide();
      }
    }
  });
});

// Gọi hàm close khi modal đóng (bao gồm nhấn backdrop)
onMounted(() => {
  const modalElement = document.getElementById('productModal');
  if (modalElement) {
    modalElement.addEventListener('hidden.bs.modal', () => {
      console.log('productModal closed (hidden.bs.modal triggered)');
      showProductModal.value = false; // Đảm bảo trạng thái đồng bộ
      close();
    });
  }
});

onUnmounted(() => {
  const modalElement = document.getElementById('productModal');
  if (modalElement) {
    modalElement.removeEventListener('hidden.bs.modal', () => {});
  }
});

// Hàm close để mở lại #exampleModal
function close() {
  nextTick(() => {
    console.log('Gọi hàm close');
    // Dọn dẹp backdrop và lớp modal-open
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach((backdrop) => backdrop.remove());
    document.body.classList.remove('modal-open');
    document.body.style.removeProperty('overflow');
    document.body.style.removeProperty('padding-right');
    // Mở lại #exampleModal
    const exampleModal = document.getElementById('exampleModal');
    if (exampleModal) {
      const modal = bootstrap.Modal.getOrCreateInstance(exampleModal, { backdrop: true });
      modal.show();
    }
  });
}

// Lấy danh sách sản phẩm
async function fetchProducts(page) {
  try {
console.log('Đang lấy sản phẩm cho trang:', page);
    const response = await fetch(
      `${getApiUrl}/api/Products?search=${encodeURIComponent(search.value)}&page=${page}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      }
    );
    if (!response.ok) {
      throw new Error('Lỗi khi lấy dữ liệu sản phẩm: ' + response.status);
    }
    const result = await response.json();
    productList.value = result.data || [];
    productTotalPages.value = result.toTalPages || 1;
    toTalPages.value = result.toTalPages || 1;
    pageSelected.value = page;
    productList.value.forEach((product) => {
      productMap.value[product.maSp] = product.tenSanPham;
    });
    console.log('Sản phẩm đã lấy:', productList.value);
  } catch (error) {
    console.error('Lỗi fetchProducts:', error);
    Swal.fire('Không thể tải danh sách sản phẩm', '', 'error');
  }
}

// Mở modal chọn sản phẩm
function openProductModal(index) {
  selectedDetailIndex.value = index;
  showProductModal.value = true;
  productCurrentPage.value = 1;
  fetchProducts(1);
  // Đóng #exampleModal tạm thời
  nextTick(() => {
    const exampleModal = document.getElementById('exampleModal');
    if (exampleModal) {
      const modal = bootstrap.Modal.getInstance(exampleModal);
      if (modal) modal.hide();
    }
  });
}

function filterProducts() {
  fetchProducts(1);
}

// Chọn sản phẩm
function selectProduct(product) {
  if (selectedDetailIndex.value !== null) {
    combo.value.chitietcombos[selectedDetailIndex.value].maSp = product.maSp;
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product.tenSanPham;
    }
  }
  showProductModal.value = false; // Đóng #productModal, hidden.bs.modal sẽ gọi close
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page;
    fetchProducts(page);
  }
}

// Thêm chi tiết combo
function addDetailCombo() {
  combo.value.chitietcombos.push({
    maSp: '',
    soLuongSp: 1,
  });
}

// Validate SoLuong, PhanTramGiam, SoTienGiam, NgayBatDau, NgayKetThuc
watch(
  combo,
  (newcombo) => {
    if (newcombo.soLuong < 1 && newcombo.soLuong !== '') {
      newcombo.soLuong = 1;
    }
    if (newcombo.soTienGiam < 0) {
      newcombo.soTienGiam = 0;
    }
    if (newcombo.phanTramGiam < 0) {
      newcombo.phanTramGiam = 0;
    }
    if (newcombo.ngayBatDau && newcombo.ngayKetThuc) {
      const startDate = new Date(newcombo.ngayBatDau);
      const endDate = new Date(newcombo.ngayKetThuc);
      if (startDate > endDate) {
        Swal.fire('Ngày bắt đầu không được lớn hơn ngày kết thúc', '', 'error');
        newcombo.ngayKetThuc = '';
      }
    }
  },
  { deep: true }
);

// Reset soTienGiam khi nhập phanTramGiam
function resetSoTienGiam() {
combo.value.soTienGiam = 0;
}

// Reset phanTramGiam khi nhập soTienGiam
function resetPhanTramGiam() {
  combo.value.phanTramGiam = 0;
}

const blockNegativeNumbers = (event) => {
  if (event.key === '-') {
    event.preventDefault();
  }
};

// Xử lý chọn hình ảnh
function handleFileChange(combo, event) {
  const file = event.target.files[0];
  combo.hinh = file;
}

// Xóa chi tiết combo
function removeDetailCombo(index) {
  if (combo.value.chitietcombos.length > 1) {
    combo.value.chitietcombos.splice(index, 1);
  }
}

// Gửi dữ liệu combo lên server
const addCombo = async () => {
  console.log('Dữ liệu combo:', combo.value);
  try {
    let isValid = true;

    // Kiểm tra trùng lặp chi tiết combo
    const hasDuplicates = combo.value.chitietcombos.some(
      (detail, index, arr) => arr.findIndex((d) => d.maSp === detail.maSp) !== index
    );
    if (hasDuplicates) {
      isValid = false;
      Swal.fire('Không được để trùng lặp sản phẩm trong chi tiết combo!', '', 'error');
    }

    // Kiểm tra hình ảnh
    if (combo.value.hinh == null) {
      document.querySelector('.imageMessage').textContent = `Không được để trống hình ảnh`;
      isValid = false;
    }

    // Kiểm tra chi tiết combo
    combo.value.chitietcombos.forEach((p) => {
      if (p.soLuongSp === '' || p.soLuongSp < 1) {
        isValid = false;
        Swal.fire('Số lượng sản phẩm trong chi tiết combo không được để trống và phải lớn hơn 0', '', 'error');
      }
      if (p.maSp === '') {
        isValid = false;
        Swal.fire('Combo phải chứa tối thiểu một sản phẩm!', '', 'error');
      }
    });

    // Kiểm tra ngày bắt đầu và ngày kết thúc
    if (!combo.value.ngayBatDau || !combo.value.ngayKetThuc) {
      isValid = false;
      Swal.fire('Ngày bắt đầu và ngày kết thúc không được để trống', '', 'error');
    }

    if (!isValid) {
      return;
    }

    // Tạo FormData
    const formData = new FormData();
    formData.append('tenCombo', combo.value.tenCombo);
    if (combo.value.hinh) {
      formData.append('hinh', combo.value.hinh);
    }
    formData.append('soLuong', combo.value.soLuong);
    formData.append('soTienGiam', combo.value.soTienGiam);
    formData.append('phanTramGiam', combo.value.phanTramGiam);
    formData.append('moTa', combo.value.moTa);
    formData.append('isActive', combo.value.isActive);
    formData.append('ngayBatDau', combo.value.ngayBatDau);
    formData.append('ngayKetThuc', combo.value.ngayKetThuc);
    combo.value.chitietcombos.forEach((detail, index) => {
      formData.append(`chitietcombos[${index}].maSp`, detail.maSp);
      formData.append(`chitietcombos[${index}].soLuongSp`, detail.soLuongSp);
    });

    // Gửi request
    const response = await fetch(`${getApiUrl}/api/Combos`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('Không thể thêm combo');
    }
const result = await response.json();
    if (result.success) {
      Swal.fire('Đã thêm combo mới thành công', '', 'success');
      setTimeout(() => {
        window.location.reload();
      }, 2000);
    } else {
      throw new Error('Không thể thêm combo');
    }
  } catch (error) {
    console.error('Lỗi:', error.message);
    Swal.fire('Có lỗi xảy ra khi thêm combo', '', 'error');
  }
};
</script>

<template>
  <!-- Modal Thêm Combo -->
  <div class="modal fade" id="exampleModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
      <div class="modal-content">
        <div class="modal-header" style="background-color: #4C7CF3;">
          <h5 class="modal-title" id="exampleModalLabel" style="color: white;">Thêm combo</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body data-createCombo">
          <form>
            <!-- Tên combo -->
            <div class="mb-3">
              <label for="tenCombo" class="form-label">Tên combo</label>
              <input type="text" class="form-control" id="tenCombo" v-model="combo.tenCombo"
                placeholder="Nhập tên combo" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Mô tả -->
            <div class="mb-3">
              <label for="moTa" class="form-label">Mô tả</label>
              <textarea class="form-control" id="moTa" rows="3" v-model="combo.moTa"
                placeholder="Nhập mô tả combo"></textarea>
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Số lượng combo -->
            <div class="mb-3">
              <label for="SoLuong" class="form-label">Số lượng</label>
              <input type="number" class="form-control" id="SoLuong" v-model="combo.soLuong" min="1"
                @keydown="blockNegativeNumbers" />
              <label style="color: red" class="error-message soluongMessage"></label>
            </div>

            <!-- Ngày bắt đầu -->
            <div class="mb-3">
              <label for="ngayBatDau" class="form-label">Ngày bắt đầu</label>
              <input type="date" class="form-control" id="ngayBatDau" v-model="combo.ngayBatDau" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Ngày kết thúc -->
            <div class="mb-3">
              <label for="ngayKetThuc" class="form-label">Ngày kết thúc</label>
              <input type="date" class="form-control" id="ngayKetThuc" v-model="combo.ngayKetThuc" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Chi tiết combo -->
            <div>
              <label class="form-label">Chi tiết combo</label>
              <div v-for="(detail, index) in combo.chitietcombos" :key="index" class="card mb-3">
<div class="card-body">
                  <div class="row">
                    <div class="col-md-6">
                      <label class="form-label">Sản phẩm</label>
                      <div class="input-group">
                        <input type="text" class="form-control" :value="productMap[detail.maSp] || 'Chọn sản phẩm'"
                          readonly />
                        <button class="btn btn-outline-primary" type="button" @click="openProductModal(index)">
                          Chọn
                        </button>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <label class="form-label">Số lượng</label>
                      <input v-model="detail.soLuongSp" type="number" class="form-control" min="1"
                        @keydown="blockNegativeNumbers" />
                    </div>
                    <div class="col-md-2 d-flex align-items-end">
                      <button type="button" class="btn btn-danger btn-sm" @click="removeDetailCombo(index)">
                        Xóa
                      </button>
                    </div>
                  </div>
                </div>
              </div>
              <button type="button" @click="addDetailCombo()" class="btn btn-secondary" style="background-color: #4C7CF3; margin-bottom: 10px;">
                Thêm chi tiết combo
              </button>
            </div>

            <!-- Phần trăm giảm combo -->
            <div class="mb-3">
              <label for="phantramCombo" class="form-label">Phần trăm giảm</label>
              <input type="number" class="form-control" id="phantramCombo" v-model="combo.phanTramGiam" min="0"
                @input="resetSoTienGiam" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Số tiền giảm combo -->
            <div class="mb-3">
              <label for="sotienGiam" class="form-label">Số tiền giảm</label>
              <input type="number" class="form-control" id="sotienGiam" v-model="combo.soTienGiam" min="0"
                @input="resetPhanTramGiam" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Hình ảnh -->
            <div class="mb-3">
              <label class="form-label">Hình ảnh</label>
              <input type="file" @change="handleFileChange(combo, $event)" class="form-control" accept="image/*" />
              <label style="color: red" class="error-message imageMessage"></label>
            </div>
          </form>
        </div>
        <div class="modal-footer" >
          <button type="button" @click="addCombo()" class="btn btn-primary" style="width: 170px;">Xác nhận</button>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal Chọn Sản Phẩm -->
  <div class="modal fade" id="productModal" tabindex="-1" aria-labelledby="productModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
<div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="productModalLabel">Chọn sản phẩm</h5>
          <button type="button" class="btn-close" @click="showProductModal = false; close()" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <!-- Bộ lọc và tìm kiếm -->
          <div class="row g-3 mb-3">
            <div class="col-md-4">
              <input style="background-color: white" v-model="search" @click.stop @keydown.stop
                @input="filterProducts()" type="text" class="form-control" placeholder="Tìm kiếm sản phẩm..." />
            </div>
          </div>
          <div class="table-responsive">
            <table class="table table-bordered table-hover">
              <thead class="table-light">
                <tr>
                  <th>Mã sản phẩm</th>
                  <th>Tên sản phẩm</th>
                  <th>Hình ảnh</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="productList.length === 0">
                  <td colspan="4" class="text-center">Không có sản phẩm nào</td>
                </tr>
                <tr v-else v-for="product in productList" :key="product.maSp">
                  <td>{{ product.maSp }}</td>
                  <td>{{ product.tenSanPham }}</td>
                  <td>
                    <img
                      :src="product.hinh ? `${getApiUrl}/HinhAnh/AnhSanPham/${product.hinh}` : '/placeholder-image.jpg'"
                      alt="Product Image" width="50" height="50" style="object-fit: cover; border-radius: 5px" />
                  </td>
                  <td>
                    <button class="btn btn-primary btn-sm" @click="selectProduct(product)">
                      Chọn
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Phân trang -->
          <nav style="margin-bottom: 60px" class="d-flex justify-content-center mt-3">
            <ul class="pagination">
              <li @click="ChangePage(1)" class="page-item"><a class="page-link" href="#">Đầu</a></li>
              <li @click="ChangePage(page)" v-for="page in toTalPages" :key="page"
                :class="['page-item', { active: page == pageSelected }]">
                <a class="page-link" href="#">{{ page }}</a>
              </li>
              <li @click="ChangePage(toTalPages)" class="page-item">
                <a class="page-link" href="#">Cuối</a>
              </li>
            </ul>
          </nav>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" @click="showProductModal = false; close()">
            Đóng
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card {
  border: 1px solid #ddd;
}
.modal-xl {
  max-width: 60%;
}
.modal-body input.form-control {
  pointer-events: auto;
  user-select: auto;
  z-index: 50;
}
</style>