<script setup>
import { ref, watch, onMounted } from 'vue';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../../../src/constants/api.js';
import Cookies from 'js-cookie';

const getApiUrl = GetApiUrl();
const token = Cookies.get('accessToken');

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
  chitietcombos: [{ maSp: '', soLuongSp: 1 }],
});

const productList = ref([]);
const productMap = ref({});
const search = ref('');
const toTalPages = ref(1);
const pageSelected = ref(1);
const selectedDetailIndex = ref(null); // Index của chi tiết combo đang được chọn

// Lấy danh sách sản phẩm và cache tên
async function fetchAndMapProducts(page = 1) {
  try {
    const response = await fetch(`${getApiUrl}/api/Products?search=${encodeURIComponent(search.value)}&page=${page}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) throw new Error('Lỗi khi lấy dữ liệu sản phẩm.');
    const result = await response.json();
    productList.value = result.data || [];
    toTalPages.value = result.toTalPages || 1;
    pageSelected.value = page;
    productList.value.forEach(p => {
      if (!productMap.value[p.maSp]) productMap.value[p.maSp] = p;
    });
  } catch (error) {
    console.error('Lỗi fetchAndMapProducts:', error);
    Swal.fire('Lỗi', 'Không thể tải danh sách sản phẩm.', 'error');
  }
}

// Toggle hiển thị danh sách sản phẩm
function toggleProductSelection(index) {
  if (selectedDetailIndex.value === index) {
    selectedDetailIndex.value = null; // Đóng nếu đã mở
    productList.value = []; // Xóa danh sách
  } else {
    selectedDetailIndex.value = index;
    search.value = '';
    fetchAndMapProducts(1);
  }
}

// Chọn một sản phẩm
function selectProduct(product) {
  if (selectedDetailIndex.value !== null) {
    const detail = combo.value.chitietcombos[selectedDetailIndex.value];
    detail.maSp = product.maSp;
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product;
    }
    selectedDetailIndex.value = null; // Đóng danh sách sau khi chọn
    productList.value = [];
  }
}

// Lọc sản phẩm
function filterProducts() {
  fetchAndMapProducts(1);
}

// Chuyển trang
function changePage(page) {
  if (page >= 1 && page <= toTalPages.value) {
    fetchAndMapProducts(page);
  }
}

// Thêm/Xóa chi tiết combo
function addDetailCombo() {
  combo.value.chitietcombos.push({ maSp: '', soLuongSp: 1 });
}
function removeDetailCombo(index) {
  if (combo.value.chitietcombos.length > 1) {
    combo.value.chitietcombos.splice(index, 1);
  }
}

// Xử lý file và validate
function handleFileChange(event) {
  const file = event.target.files[0];
  combo.value.hinh = file;
}

watch(combo, (newVal) => {
  if (newVal.soLuong < 1 && newVal.soLuong !== '') newVal.soLuong = 1;
  if (newVal.soTienGiam < 0) newVal.soTienGiam = 0;
  if (newVal.phanTramGiam < 0) newVal.phanTramGiam = 0;
  if (newVal.phanTramGiam >= 100) newVal.phanTramGiam = 99;
  if (newVal.ngayBatDau && newVal.ngayKetThuc) {
    if (new Date(newVal.ngayBatDau) > new Date(newVal.ngayKetThuc)) {
      Swal.fire('Lỗi', 'Ngày bắt đầu không được lớn hơn ngày kết thúc.', 'error');
      newVal.ngayKetThuc = '';
    }
  }
}, { deep: true });

function resetSoTienGiam() { combo.value.soTienGiam = 0; }
function resetPhanTramGiam() { combo.value.phanTramGiam = 0; }
const blockNegativeNumbers = (event) => { if (event.key === '-') event.preventDefault(); };

// Gửi dữ liệu
const addCombo = async () => {
  let isValid = true;
  const errorMessages = [];

  if (!combo.value.tenCombo) {
    isValid = false;
    errorMessages.push('Tên combo không được để trống.');
  }
  if (!combo.value.hinh) {
    isValid = false;
    errorMessages.push('Hình ảnh không được để trống.');
  }
  if (!combo.value.ngayBatDau || !combo.value.ngayKetThuc) {
    isValid = false;
    errorMessages.push('Ngày bắt đầu và kết thúc không được để trống.');
  }
  if (combo.value.chitietcombos.some(d => !d.maSp || d.soLuongSp < 1)) {
    isValid = false;
    errorMessages.push('Mỗi chi tiết combo phải có sản phẩm và số lượng lớn hơn 0.');
  }
  const productIds = combo.value.chitietcombos.map(d => d.maSp);
  if (new Set(productIds).size !== productIds.length) {
      isValid = false;
      errorMessages.push('Không được có sản phẩm trùng lặp trong combo.');
  }

  if (!isValid) {
    Swal.fire('Dữ liệu không hợp lệ', errorMessages.join('<br>'), 'error');
    return;
  }

  const formData = new FormData();
  Object.keys(combo.value).forEach(key => {
    if (key !== 'chitietcombos' && key !== 'hinh') {
      formData.append(key, combo.value[key]);
    }
  });
  if (combo.value.hinh) {
    formData.append('hinh', combo.value.hinh);
  }
  combo.value.chitietcombos.forEach((detail, index) => {
    formData.append(`chitietcombos[${index}].maSp`, detail.maSp);
    formData.append(`chitietcombos[${index}].soLuongSp`, detail.soLuongSp);
  });

  try {
    const response = await fetch(`${getApiUrl}/api/Combos`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` },
      body: formData,
    });
    const result = await response.json();
    if (result.success) {
      Swal.fire('Thành công', 'Đã thêm combo mới thành công!', 'success');
      setTimeout(() => window.location.reload(), 1500);
    } else {
      throw new Error(result.message || 'Không thể thêm combo.');
    }
  } catch (error) {
    console.error('Lỗi khi thêm combo:', error);
    Swal.fire('Lỗi', error.message || 'Có lỗi xảy ra khi thêm combo.', 'error');
  }
};

onMounted(() => {
  // Lấy sẵn một số sản phẩm để hiển thị tên ban đầu nếu cần
  fetchAndMapProducts();
});
</script>

<template>
  <div class="modal fade" id="exampleModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
      <div class="modal-content">
        <div class="modal-header" style="background-color: #4C7CF3;">
          <h5 class="modal-title text-white" id="exampleModalLabel">Thêm Combo Mới</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="addCombo">
            <div class="row">
              <!-- Cột trái: Thông tin chung -->
              <div class="col-md-6">
                <div class="mb-3">
                  <label for="tenCombo" class="form-label">Tên combo</label>
                  <input type="text" class="form-control" id="tenCombo" v-model="combo.tenCombo" placeholder="Nhập tên combo" />
                </div>
                <div class="mb-3">
                  <label for="moTa" class="form-label">Mô tả</label>
                  <textarea class="form-control" id="moTa" rows="3" v-model="combo.moTa" placeholder="Nhập mô tả combo"></textarea>
                </div>
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label for="SoLuong" class="form-label">Số lượng</label>
                    <input type="number" class="form-control" id="SoLuong" v-model="combo.soLuong" min="1" @keydown="blockNegativeNumbers" />
                  </div>
                  <div class="col-md-6 mb-3">
                     <label class="form-label">Trạng thái</label>
                      <select class="form-select" v-model="combo.isActive">
                        <option :value="true">Hoạt động</option>
                        <option :value="false">Không hoạt động</option>
                      </select>
                  </div>
                </div>
                <div class="row">
                  <div class="col-md-6 mb-3">
                    <label for="ngayBatDau" class="form-label">Ngày bắt đầu</label>
                    <input type="date" class="form-control" id="ngayBatDau" v-model="combo.ngayBatDau" />
                  </div>
                  <div class="col-md-6 mb-3">
                    <label for="ngayKetThuc" class="form-label">Ngày kết thúc</label>
                    <input type="date" class="form-control" id="ngayKetThuc" v-model="combo.ngayKetThuc" />
                  </div>
                </div>
                 <div class="row">
                    <div class="col-md-6 mb-3">
                        <label for="phantramCombo" class="form-label">Phần trăm giảm (%)</label>
                        <input type="number" class="form-control" id="phantramCombo" v-model="combo.phanTramGiam" min="0" max="99" @input="resetSoTienGiam" @keydown="blockNegativeNumbers"/>
                    </div>
                    <div class="col-md-6 mb-3">
                        <label for="sotienGiam" class="form-label">Số tiền giảm (VND)</label>
                        <input type="number" class="form-control" id="sotienGiam" v-model="combo.soTienGiam" min="0" @input="resetPhanTramGiam" @keydown="blockNegativeNumbers"/>
                    </div>
                </div>
                <div class="mb-3">
                  <label class="form-label">Hình ảnh</label>
                  <input type="file" @change="handleFileChange" class="form-control" accept="image/*" />
                </div>
              </div>

              <!-- Cột phải: Chi tiết combo -->
              <div class="col-md-6">
                <label class="form-label">Chi Tiết Combo</label>
                <div v-for="(detail, index) in combo.chitietcombos" :key="index" class="card mb-3" :class="{ 'highlight-detail': selectedDetailIndex === index }">
                  <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center">
                        <h6 class="card-title mb-0">Sản phẩm {{ index + 1 }}</h6>
                        <button type="button" class="btn btn-danger btn-sm" @click="removeDetailCombo(index)" v-if="combo.chitietcombos.length > 1">Xóa</button>
                    </div>
                    <hr>
                    <div class="row align-items-end">
                      <div class="col-8">
                        <label class="form-label">Sản phẩm đã chọn</label>
                        <div class="selected-product-display">
                          <img v-if="detail.maSp && productMap[detail.maSp]" :src="`${getApiUrl}/HinhAnh/Products/${productMap[detail.maSp].anhDaiDien}`" alt="SP" class="thumbnail" />
                          <span v-if="detail.maSp && productMap[detail.maSp]">{{ productMap[detail.maSp].tenSanPham }}</span>
                          <span v-else class="text-muted">Chưa có sản phẩm nào được chọn</span>
                        </div>
                      </div>
                       <div class="col-4">
                        <label class="form-label">Số lượng</label>
                        <input v-model="detail.soLuongSp" type="number" class="form-control" min="1" @keydown="blockNegativeNumbers" />
                      </div>
                    </div>
                     <button class="btn btn-outline-primary mt-3 w-100" type="button" @click="toggleProductSelection(index)">
                        {{ selectedDetailIndex === index ? 'Đóng' : 'Chọn sản phẩm' }}
                    </button>
                  </div>
                  <!-- Vùng chọn sản phẩm -->
                  <div v-if="selectedDetailIndex === index" class="product-selection-area">
                      <div class="p-3">
                          <input type="text" class="form-control mb-3" placeholder="Tìm kiếm sản phẩm..." v-model="search" @input="filterProducts" />
                          <div v-if="productList.length > 0" class="product-list-scroll">
                              <div v-for="product in productList" :key="product.maSp" class="product-card" @click="selectProduct(product)" :class="{ 'highlight-product': detail.maSp === product.maSp }">
                                  <img :src="`${getApiUrl}/HinhAnh/Products/${product.anhDaiDien}`" alt="Hình sản phẩm" class="product-card-img" />
                                  <div class="product-card-info">
                                      <p class="fw-bold mb-0">{{ product.tenSanPham }}</p>
                                      <p class="text-muted mb-0">Mã: {{ product.maSp }}</p>
                                  </div>
                              </div>
                          </div>
                           <p v-else class="text-center text-muted">Không tìm thấy sản phẩm.</p>
                      </div>
                      <!-- Phân trang -->
                      <nav v-if="toTalPages > 1" class="d-flex justify-content-center mt-2">
                          <ul class="pagination pagination-sm">
                              <li class="page-item" :class="{ disabled: pageSelected === 1 }"><a class="page-link" href="#" @click.prevent="changePage(pageSelected - 1)">Trước</a></li>
                              <li v-for="page in toTalPages" :key="page" class="page-item" :class="{ active: page === pageSelected }">
                                  <a class="page-link" href="#" @click.prevent="changePage(page)">{{ page }}</a>
                              </li>
                              <li class="page-item" :class="{ disabled: pageSelected === toTalPages }"><a class="page-link" href="#" @click.prevent="changePage(pageSelected + 1)">Sau</a></li>
                          </ul>
                      </nav>
                  </div>
                </div>
                <button type="button" @click="addDetailCombo" class="btn btn-secondary w-100" style="background-color: #4C7CF3;">Thêm Sản Phẩm</button>
              </div>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
          <button type="button" @click="addCombo" class="btn btn-primary">Lưu Combo</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-xl {
  max-width: 80%;
}
.highlight-detail {
  border: 2px solid #4C7CF3;
  box-shadow: 0 0 10px rgba(76, 124, 243, 0.5);
}
.selected-product-display {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px;
  background-color: #f8f9fa;
  border-radius: 5px;
  min-height: 50px;
}
.thumbnail {
  width: 40px;
  height: 40px;
  object-fit: cover;
  border-radius: 5px;
}
.product-selection-area {
  background-color: #f8f9fa;
  border-top: 1px solid #dee2e6;
}
.product-list-scroll {
  max-height: 300px;
  overflow-y: auto;
}
.product-card {
  display: flex;
  align-items: center;
  padding: 10px;
  border-radius: 5px;
  cursor: pointer;
  transition: background-color 0.2s;
  border: 2px solid transparent;
}
.product-card:hover {
  background-color: #e9ecef;
}
.product-card-img {
  width: 50px;
  height: 50px;
  object-fit: cover;
  border-radius: 5px;
  margin-right: 15px;
}
.highlight-product {
  background-color: #dbeaff;
  border-color: #4C7CF3;
}
</style>