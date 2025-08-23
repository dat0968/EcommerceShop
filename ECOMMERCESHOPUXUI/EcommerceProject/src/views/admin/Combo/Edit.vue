<script setup>
import { ref, watch, onMounted, nextTick } from 'vue';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../../../src/constants/api.js';
import * as bootstrap from 'bootstrap';
import Cookies from 'js-cookie';
import { debounce } from 'lodash';


const getUrlAPI = ref(GetApiUrl());

const props = defineProps({
  Combo: Object,
  ListProduct: Object,
});

const comboEdit = ref({
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

const initialCombo = ref(null);
const selectedDetailIndex = ref(null); // Index của chi tiết combo đang được chọn để thêm sản phẩm
const toTalPages = ref(1);
const pageSelected = ref(1);
const productList = ref([]);
const productMap = ref({});
const search = ref('');
const token = Cookies.get('accessToken');

// Hàm validateDiscount
const validateDiscount = (phanTramGiam, soTienGiam) => {
  if (phanTramGiam !== null && phanTramGiam !== undefined) {
    if (phanTramGiam < 0) {
      Swal.fire({
        title: 'Lỗi',
        text: 'Phần trăm giảm không được nhỏ hơn 0%',
        icon: 'error',
        confirmButtonText: 'OK',
      });
      return false;
    }
    if (phanTramGiam > 100) {
      Swal.fire({
        title: 'Lỗi',
        text: 'Phần trăm giảm không được lớn hơn 100%',
        icon: 'error',
        confirmButtonText: 'OK',
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
        confirmButtonText: 'OK',
      });
      return false;
    }
  }
  return true;
};

// Khởi tạo dữ liệu và map tên sản phẩm
onMounted(() => {

  if (!props.Combo || !props.Combo.maCombo) {
    console.error('Dữ liệu props.Combo không hợp lệ:', props.Combo);
    Swal.fire('Lỗi: Dữ liệu combo không hợp lệ', '', 'error');
    return;
  }

  const formatDateForInput = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  };

  initialCombo.value = {
    tenCombo: props.Combo.tenCombo || '',
    hinh: props.Combo.hinh || null,
    soTienGiam: props.Combo.soTienGiam || 0,
    phanTramGiam: props.Combo.phanTramGiam || 0,
    soLuong: props.Combo.soLuong || 1,
    moTa: props.Combo.moTa || '',
    isActive: props.Combo.isActive ?? true,
    ngayBatDau: formatDateForInput(props.Combo.ngayBatDau),
    ngayKetThuc: formatDateForInput(props.Combo.ngayKetThuc),
    chitietcombos: Array.isArray(props.Combo.chitietcombos) && props.Combo.chitietcombos.length > 0
      ? props.Combo.chitietcombos.map((detail) => ({ ...detail }))
      : [{ maSp: '', soLuongSp: 1 }],
  };

  comboEdit.value = {
    ...initialCombo.value,
    chitietcombos: [...initialCombo.value.chitietcombos],
  };

  comboEdit.value.chitietcombos.forEach(detail => {
    if (detail.maSp) {
      fetchProductName(detail.maSp);
    }
  });
});

// Hàm lấy tên sản phẩm theo maSp
async function fetchProductName(maSp) {
  try {
    if (!token) {
      throw new Error('Không tìm thấy accessToken');
    }
    const response = await fetch(`${getUrlAPI.value}/api/Products/${maSp}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    });
    if (!response.ok) {
      throw new Error(`Lỗi khi lấy sản phẩm ${maSp}: ${response.status}`);
    }
    const product = await response.json();
    if (product && product.data.tenSanPham) {
      productMap.value[maSp] = product.data.tenSanPham;
    }
  } catch (error) {
    console.error(`Lỗi khi lấy tên sản phẩm ${maSp}:`, error.message);
  }
}

// Fetch danh sách sản phẩm
async function fetchProducts(page) {
  try {
    if (!token) {
      throw new Error('Không tìm thấy accessToken. Vui lòng đăng nhập lại.');
    }
    const response = await fetch(
      `${getUrlAPI.value}/api/Products?search=${encodeURIComponent(search.value)}&page=${page}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
      }
    );
    if (!response.ok) {
      const errorText = await response.text();
      if (response.status === 401) {
        throw new Error('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
      } else if (response.status === 429) {
        throw new Error('Quá nhiều yêu cầu. Vui lòng thử lại sau.');
      }
      throw new Error(`Lỗi khi lấy dữ liệu sản phẩm: ${response.status} - ${errorText}`);
    }
    const result = await response.json();
    if (!result.data || !Array.isArray(result.data)) {
      productList.value = [];
      toTalPages.value = 1;
    } else {
      productList.value = result.data;
      toTalPages.value = result.toTalPages || 1;
      pageSelected.value = page;
      productList.value.forEach(product => {
        productMap.value[product.maSp] = product.tenSanPham;
      });
    }
  } catch (error) {
    console.error('Lỗi fetchProducts:', error.message);
    Swal.fire('Không thể tải danh sách sản phẩm', error.message, 'error');
  }
}

// Debounce cho filterProducts
const filterProducts = debounce(() => {
  fetchProducts(1);
}, 500);

// Mở/đóng khu vực chọn sản phẩm
function toggleProductSelection(index) {
  if (selectedDetailIndex.value === index) {
    selectedDetailIndex.value = null; // Đóng nếu đang mở
  }
  else {
    selectedDetailIndex.value = index; // Mở cho mục được chọn
    search.value = ''; // Reset search khi mở
    fetchProducts(1);
    nextTick(() => {
      const container = document.querySelector(`.product-selection-container[data-index="${index}"]`);
      if (container) {
        container.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        const searchInput = container.querySelector('.product-search-input');
        if (searchInput) searchInput.focus();
      }
    });
  }
}

function closeProductSelection() {
  selectedDetailIndex.value = null;
}

function selectProduct(product) {
  if (selectedDetailIndex.value !== null) {
    comboEdit.value.chitietcombos[selectedDetailIndex.value].maSp = product.maSp;
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product.tenSanPham;
    }
    closeProductSelection();
  } else {
    Swal.fire('Lỗi: Không thể chọn sản phẩm', '', 'error');
  }
}

// Chuyển trang
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page;
    fetchProducts(page);
  }
}

// Validate số âm
const blockNegativeNumbers = (event) => {
  if (event.key === '-') {
    event.preventDefault();
  }
};

// Validate dữ liệu
watch(
  comboEdit,
  (newcomboEdit) => {
    if (newcomboEdit.soLuong < 1 && newcomboEdit.soLuong !== '') {
      newcomboEdit.soLuong = 1;
    }
    if (!validateDiscount(newcomboEdit.phanTramGiam, newcomboEdit.soTienGiam)) {
      // Xử lý lỗi đã được thực hiện trong validateDiscount
    }
    if (newcomboEdit.ngayBatDau && newcomboEdit.ngayKetThuc) {
      const startDate = new Date(newcomboEdit.ngayBatDau);
      const endDate = new Date(newcomboEdit.ngayKetThuc);
      if (startDate > endDate) {
        Swal.fire('Ngày bắt đầu không được lớn hơn ngày kết thúc', '', 'error');
        newcomboEdit.ngayKetThuc = '';
      }
    }
  },
  { deep: true }
);

// Reset giá trị giảm
function resetSoTienGiam() {
  comboEdit.value.soTienGiam = 0;
}

function resetPhanTramGiam() {
  comboEdit.value.phanTramGiam = 0;
}

// Xử lý file ảnh
function handleFileChange(event) {
  const file = event.target.files[0];
  if (file) {
    comboEdit.value.hinh = file;
  }
}

// Thêm/Xóa chi tiết combo
function addDetailCombo() {
  comboEdit.value.chitietcombos.push({
    maSp: '',
    soLuongSp: 1,
  });
}

function removeDetailCombo(index) {
  if (comboEdit.value.chitietcombos.length > 1) {
    comboEdit.value.chitietcombos.splice(index, 1);
  }
}

// Đóng modal chỉnh sửa
function closeModal() {
  const editModalId = `comboEditModal_${props.Combo.maCombo}`;
  const editModal = document.getElementById(editModalId);
  if (editModal) {
    const modal = bootstrap.Modal.getInstance(editModal);
    if (modal) {
      modal.hide();
      // Re-introducing manual cleanup with a delay
      setTimeout(() => {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach((backdrop) => backdrop.remove());
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
      }, 100); // Small delay to allow Bootstrap's hide animation to start
    } else {
      // Fallback: If Bootstrap instance is not found, force hide and clean up.
      editModal.classList.remove('show');
      editModal.style.display = 'none';
      editModal.setAttribute('aria-hidden', 'true');
      editModal.removeAttribute('aria-modal');
      editModal.removeAttribute('role');

      // Manual cleanup for backdrops and body classes
      const backdrops = document.querySelectorAll('.modal-backdrop');
      backdrops.forEach((backdrop) => backdrop.remove());
      document.body.classList.remove('modal-open');
      document.body.style.removeProperty('overflow');
      document.body.style.removeProperty('padding-right');
    }
  }
}

// Hủy thay đổi
const cancelEdit = () => {
  const comboChanged = JSON.stringify(comboEdit.value) !== JSON.stringify(initialCombo.value);
  if (comboChanged) {
    Swal.fire({
      title: 'Bạn có muốn lưu các thay đổi này không?',
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: 'Lưu',
      denyButtonText: 'Không lưu',
      cancelButtonText: 'Tiếp tục sửa',
    }).then((result) => {
      if (result.isConfirmed) {
        UpdateCombo();
      } else if (result.isDenied) {
        closeModal();
      }
    });
  } else {
    closeModal();
  }
};

// Cập nhật combo
async function UpdateCombo() {
  try {
    let isValid = true;

    const hasDuplicates = comboEdit.value.chitietcombos.some(
      (item, index, arr) => arr.findIndex((obj) => obj.maSp === item.maSp && obj.maSp !== '') !== index
    );
    if (hasDuplicates) {
      Swal.fire('Vui lòng không để hai sản phẩm trùng lặp trong combo', '', 'error');
      isValid = false;
    }

    if (!props.Combo.maCombo) {
      Swal.fire('Mã combo không hợp lệ', '', 'error');
      isValid = false;
    }

    comboEdit.value.chitietcombos.forEach((detail) => {
      if (!detail.maSp) {
        Swal.fire('Vui lòng chọn sản phẩm cho tất cả chi tiết combo', '', 'error');
        isValid = false;
      }
      if (detail.soLuongSp < 1 || detail.soLuongSp === '') {
        Swal.fire('Số lượng sản phẩm trong chi tiết combo phải lớn hơn 0', '', 'error');
        isValid = false;
      }
    });

    if (!comboEdit.value.tenCombo) {
      isValid = false;
      Swal.fire('Tên combo không được để trống', '', 'error');
    }

    if (!comboEdit.value.ngayBatDau || !comboEdit.value.ngayKetThuc) {
      Swal.fire('Ngày bắt đầu và ngày kết thúc không được để trống', '', 'error');
      isValid = false;
    }

    if (!validateDiscount(comboEdit.value.phanTramGiam, comboEdit.value.soTienGiam)) {
      isValid = false;
    }

    if (!isValid) {
      return;
    }

    const formatDateForAPI = (dateString) => {
      if (!dateString) return '';
      return new Date(dateString).toISOString();
    };

    const formData = new FormData();
    formData.append('tenCombo', comboEdit.value.tenCombo);
    if (comboEdit.value.hinh && typeof comboEdit.value.hinh !== 'string') {
      formData.append('hinh', comboEdit.value.hinh);
    }
    formData.append('soLuong', comboEdit.value.soLuong);
    formData.append('soTienGiam', comboEdit.value.soTienGiam);
    formData.append('phanTramGiam', comboEdit.value.phanTramGiam);
    formData.append('moTa', comboEdit.value.moTa);
    formData.append('isActive', comboEdit.value.isActive.toString());
    formData.append('ngayBatDau', formatDateForAPI(comboEdit.value.ngayBatDau));
    formData.append('ngayKetThuc', formatDateForAPI(comboEdit.value.ngayKetThuc));

    comboEdit.value.chitietcombos.forEach((detail, index) => {
      formData.append(`chitietcombos[${index}].maSp`, detail.maSp);
      formData.append(`chitietcombos[${index}].soLuongSp`, detail.soLuongSp);
    });

    const response = await fetch(`${getUrlAPI.value}/api/Combos/${props.Combo.maCombo}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${Cookies.get('accessToken') || ''}`,
      },
      body: formData,
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`Lỗi khi cập nhật combo: ${response.status} - ${errorText}`);
    }

    Swal.fire('Đã cập nhật thông tin combo sản phẩm', '', 'success');
    setTimeout(() => {
      window.location.reload();
    }, 1500);
  } catch (error) {
    console.error('Lỗi trong UpdateCombo:', error);
    Swal.fire('Lỗi khi cập nhật combo', error.message, 'error');
  }
}
</script>

<template>
  <div v-show="isEditModalVisible" class="modal fade" :id="`comboEditModal_${props.Combo.maCombo}`" tabindex="-1" data-bs-backdrop="static"
    data-bs-keyboard="false" aria-labelledby="comboEditModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl text-start">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title" id="comboEditModalLabel">Sửa thông tin combo</h5>
          <button type="button" class="btn-close btn-close-white" @click="cancelEdit()" aria-label="Close"></button>
        </div>

        <div class="modal-body p-4 data-editCombo">
          <div class="form-container">
            <form @submit.prevent>
              <div class="row">
                <!-- Cột 1: Tên, Mô tả, Số lượng -->
                <div class="col-md-6">
                  <div class="mb-3">
                    <label class="form-label">Tên combo</label>
                    <input type="text" class="form-control" v-model="comboEdit.tenCombo" placeholder="Nhập tên combo" />
                  </div>
                  <div class="mb-3">
                    <label for="moTa" class="form-label">Mô tả</label>
                    <textarea v-model="comboEdit.moTa" class="form-control" id="moTa" rows="3"
                      placeholder="Nhập mô tả combo"></textarea>
                  </div>
                   <div class="mb-3">
                    <label class="form-label">Số lượng</label>
                    <input @keydown="blockNegativeNumbers" v-model="comboEdit.soLuong" type="number" class="form-control"
                      min="1" />
                  </div>
                </div>

                <!-- Cột 2: Ngày, Giảm giá, Hình ảnh -->
                <div class="col-md-6">
                  <div class="row">
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Ngày bắt đầu</label>
                      <input type="date" class="form-control" v-model="comboEdit.ngayBatDau" />
                    </div>
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Ngày kết thúc</label>
                      <input type="date" class="form-control" v-model="comboEdit.ngayKetThuc" />
                    </div>
                  </div>
                  <div class="row">
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Phần trăm giảm (%)</label>
                      <input type="number" class="form-control" v-model="comboEdit.phanTramGiam" min="0" max="100"
                        @input="resetSoTienGiam" />
                    </div>
                    <div class="col-md-6 mb-3">
                      <label class="form-label">Số tiền giảm (VND)</label>
                      <input type="number" class="form-control" v-model="comboEdit.soTienGiam" min="0"
                        @input="resetPhanTramGiam" />
                    </div>
                  </div>
                   <div class="mb-3">
                    <label class="form-label">Hình ảnh</label>
                    <input @change="handleFileChange" type="file" class="form-control" accept="image/*" />
                    <div class="mt-2">
                      <img v-if="comboEdit.hinh && typeof comboEdit.hinh === 'string'"
                        :src="`${getUrlAPI}/HinhAnh/AnhCombo/${comboEdit.hinh}`" alt="Ảnh combo" class="img-thumbnail"
                        @error="comboEdit.hinh = null" />
                      <span v-else-if="!comboEdit.hinh">Không có ảnh</span>
                    </div>
                  </div>
                </div>
              </div>

              <hr class="my-4" />

              <!-- Chi tiết combo -->
              <div class="mb-3">
                <h5 class="mb-3">Chi tiết combo</h5>
                <div class="card mb-3 detail-card" v-for="(detail, index) in comboEdit.chitietcombos" :key="index"
                  :class="{ 'highlighted-detail-card': selectedDetailIndex === index }">
                  <div class="card-body">
                    <div class="row align-items-end">
                      <div class="col-md-6">
                        <label class="form-label">Sản phẩm</label>
                        <div class="input-group">
                          <input type="text" class="form-control" :value="productMap[detail.maSp] || 'Chưa chọn sản phẩm'"
                            readonly />
                          <button class="btn btn-outline-primary" type="button" @click="toggleProductSelection(index)">
                            {{ selectedDetailIndex === index ? 'Đóng' : 'Chọn' }}
                          </button>
                        </div>
                      </div>
                      <div class="col-md-4">
                        <label class="form-label">Số lượng</label>
                        <input type="number" class="form-control" v-model="detail.soLuongSp" min="1"
                          @keydown="blockNegativeNumbers" />
                      </div>
                      <div class="col-md-2">
                        <button @click="removeDetailCombo(index)" type="button" class="btn btn-danger w-100">
                          Xóa
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Product Selection Area -->
                  <div v-if="selectedDetailIndex === index" class="product-selection-container" :data-index="index">
                    <div class="p-3 border-top">
                       <div class="row g-3 mb-3">
                          <div class="col-md">
                            <input v-model="search" @input="filterProducts" type="text" class="form-control product-search-input" placeholder="Tìm kiếm sản phẩm theo tên..." />
                          </div>
                        </div>

                      <div v-if="productList.length === 0" class="text-center p-3">
                        Không tìm thấy sản phẩm nào.
                      </div>
                      <div v-else class="row row-cols-1 g-3 product-list-cards">
                        <div class="col" v-for="product in productList" :key="product.maSp">
                          <div class="card product-card-horizontal"
                            :class="{ 'selected-product-card': selectedDetailIndex !== null && product.maSp === comboEdit.chitietcombos[selectedDetailIndex].maSp }">
                            <div class="row g-0">
                              <div class="col-auto">
                                <img :src="`${getUrlAPI}/HinhAnh/Products/${product.anhDaiDien || 'default.png'}`"
                                  class="img-fluid rounded-start product-card-img-horizontal" alt="Product Image" @error="product.anhDaiDien = null" />
                              </div>
                              <div class="col">
                                <div class="card-body d-flex justify-content-between align-items-center">
                                  <h6 class="card-title mb-0">{{ product.tenSanPham }}</h6>
                                  <button class="btn btn-primary btn-sm" @click="selectProduct(product)">
                                    Chọn
                                  </button>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>

                      <nav v-if="toTalPages > 1" class="d-flex justify-content-center mt-4">
                        <ul class="pagination mb-0">
                          <li @click="ChangePage(1)" class="page-item" :class="{ 'disabled': pageSelected === 1 }"><a class="page-link" href="#">Đầu</a></li>
                          <li @click="ChangePage(page)" v-for="page in toTalPages" :key="page"
                            :class="['page-item', { active: page == pageSelected }]">
                            <a class="page-link" href="#">{{ page }}</a>
                          </li>
                          <li @click="ChangePage(toTalPages)" class="page-item" :class="{ 'disabled': pageSelected === toTalPages }">
                            <a class="page-link" href="#">Cuối</a>
                          </li>
                        </ul>
                      </nav>
                    </div>
                  </div>
                </div>
                <button @click="addDetailCombo()" type="button" class="btn btn-secondary" style="background-color: #4C7CF3;">
                  Thêm sản phẩm vào combo
                </button>
              </div>

              <div class="modal-footer p-0 pt-4">
                <button type="button" class="btn btn-secondary" @click="cancelEdit()">Hủy</button>
                <button type="button" @click="UpdateCombo()" class="btn btn-primary">Lưu thay đổi</button>
              </div>
            </form>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-xl {
  max-width: 80%;
}

.form-container {
  max-height: 80vh;
  overflow-y: auto;
  padding: 5px;
}

.detail-card {
  border: 1px solid #ddd;
  transition: all 0.3s ease;
}

.product-selection-container {
  background-color: #f8f9fa;
}

.product-list-cards {
  max-height: 400px;
  overflow-y: auto;
  padding: 5px;
}

.product-card-horizontal {
  margin-bottom: 10px;
  border: 1px solid #e0e0e0; /* Default border */
  transition: border-color 0.3s ease;
}

.product-card-img-horizontal {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 0.25rem;
}

.product-card-horizontal .card-body {
  padding: 10px 15px;
}

.product-card-horizontal .card-title {
  font-size: 1rem;
  margin-right: 10px;
  flex-grow: 1;
}

.img-thumbnail {
  max-width: 120px;
  height: auto;
}

.page-item {
  cursor: pointer;
}

/* Highlight for selected product card in the selection list */
.selected-product-card {
  border-color: #007bff; /* Blue border for highlight */
  box-shadow: 0 0 0 0.25rem rgba(0, 123, 255, 0.25); /* Optional: add a subtle shadow */
}

/* Highlight for the selected combo detail card */
.highlighted-detail-card {
  border-color: #0056b3; /* Darker blue border */
  box-shadow: 0 0 0 0.25rem rgba(0, 86, 179, 0.25); /* Matching shadow */
}
</style>