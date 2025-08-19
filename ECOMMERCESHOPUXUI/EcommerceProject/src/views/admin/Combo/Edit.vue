<script setup>
import { ref, watch, onMounted, nextTick } from 'vue';
import Swal from 'sweetalert2';
import { GetApiUrl } from '../../../../src/constants/api.js';
import * as bootstrap from 'bootstrap';
import Cookies from 'js-cookie';
import { debounce } from 'lodash'; // Cần cài đặt: npm install lodash

let getApiUrl = GetApiUrl();
const getUrlAPI = ref('https://localhost:7217');

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
const selectedDetailIndex = ref(null);
const isProductSidebarOpen = ref(false); // Thay cho modal sản phẩm
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
  // Kiểm tra dữ liệu đầu vào
  if (!props.Combo || !props.Combo.maCombo) {
    console.error('Dữ liệu props.Combo không hợp lệ:', props.Combo);
    Swal.fire('Lỗi: Dữ liệu combo không hợp lệ', '', 'error');
    return;
  }

  // Khởi tạo dữ liệu
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

  // Khởi tạo productMap từ chitietcombos
  comboEdit.value.chitietcombos.forEach(detail => {
    if (detail.maSp) {
      fetchProductName(detail.maSp);
    }
  });
  console.log(comboEdit)
});

// Hàm lấy tên sản phẩm theo maSp
async function fetchProductName(maSp) {
  try {
    if (!token) {
      throw new Error('Không tìm thấy accessToken');
    }
    const response = await fetch(`${getApiUrl}/api/Products/${maSp}`, {
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
    // console.log(product)
    if (product && product.data.tenSanPham) {
      productMap.value[maSp] = product.data.tenSanPham;
    }
    // console.log(productMap)
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
      `${getApiUrl}/api/Products?search=${encodeURIComponent(search.value)}&page=${page}`,
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
      throw new Error(`Lỗi khi lấy dữ liệu sản ph���m: ${response.status} - ${errorText}`);
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

// Sidebar chọn sản phẩm (thay vì modal phụ)
function openProductSidebar(index) {
  if (!Array.isArray(comboEdit.value.chitietcombos) || comboEdit.value.chitietcombos.length === 0) {
    comboEdit.value.chitietcombos = [{ maSp: '', soLuongSp: 1 }];
  }
  if (index >= 0 && index < comboEdit.value.chitietcombos.length) {
    selectedDetailIndex.value = index;
    isProductSidebarOpen.value = true;
    fetchProducts(1);
    nextTick(() => {
      const cards = document.querySelectorAll('.left-pane .card');
      const el = cards[index];
      if (el && el.scrollIntoView) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
      const container = document.querySelector('#product-sidebar-search');
      if (container) container.focus();
    });
  } else {
    Swal.fire('Lỗi: Chỉ số chi tiết combo không hợp lệ', '', 'error');
  }
}

function closeProductSidebar() {
  isProductSidebarOpen.value = false;
  selectedDetailIndex.value = null;
}

function selectProduct(product) {
  if (selectedDetailIndex.value != null && selectedDetailIndex.value < comboEdit.value.chitietcombos.length) {
    comboEdit.value.chitietcombos[selectedDetailIndex.value].maSp = product.maSp;
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product.tenSanPham;
    }
    closeProductSidebar();
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
      // Không cần gán lại giá trị vì validateDiscount đã xử lý
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
function handleFileChange(comboEdit, event) {
  const file = event.target.files[0];
  if (file) {
    comboEdit.hinh = file;
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
      setTimeout(() => {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach((backdrop) => backdrop.remove());
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
      }, 100);
    } else {
      Swal.fire('Lỗi: Không thể đóng modal', '', 'error');
    }
  } else {
    Swal.fire('Lỗi: Không tìm thấy modal chỉnh sửa', '', 'error');
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
      confirmButtonText: 'Có',
      denyButtonText: 'Tiếp tục chỉnh sửa',
      cancelButtonText: 'Hủy',
    }).then((result) => {
      if (result.isConfirmed) {
        UpdateCombo();
      } else if (result.isDenied) {
        // Tiếp tục chỉnh sửa, không làm gì
      } else {
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

    const form_input_combo = document.querySelectorAll('.data-editCombo .mb-3');
    form_input_combo.forEach((element) => {
      const messageErrorCombo = element.querySelector('.error-message');
      if (messageErrorCombo) {
        messageErrorCombo.textContent = '';
      }
    });

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
    }, 2000);
  } catch (error) {
    console.error('Lỗi trong UpdateCombo:', error);
    Swal.fire('Lỗi khi cập nhật combo', error.message, 'error');
  }
}
</script>

<template>
  <div class="modal fade" :id="`comboEditModal_${props.Combo.maCombo}`" tabindex="-1" data-bs-backdrop="static"
    data-bs-keyboard="false" aria-labelledby="comboEditModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl text-start">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title" id="comboEditModalLabel">Sửa thông tin combo</h5>
          <button type="button" class="btn-close" @click="cancelEdit()" aria-label="Close"></button>
        </div>

        <div class="modal-body p-4 data-editCombo">
          <div class="row g-4 edit-layout">
            <!-- Bên trái: Form chỉnh sửa -->
            <div :class="['left-pane', isProductSidebarOpen ? 'col-lg-7 col-md-12' : 'col-12']">
              <form @submit.prevent>
                <div class="mb-3">
                  <label class="form-label">Tên combo</label>
                  <input type="text" class="form-control" v-model="comboEdit.tenCombo" placeholder="Nhập tên combo" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label for="moTa" class="form-label">Mô tả</label>
                  <textarea v-model="comboEdit.moTa" class="form-control" id="moTa" rows="3"
                    placeholder="Nhập mô tả combo"></textarea>
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Số lượng</label>
                  <input @keydown="blockNegativeNumbers" v-model="comboEdit.soLuong" type="number" class="form-control"
                    min="1" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Ngày bắt đầu</label>
                  <input type="date" class="form-control" v-model="comboEdit.ngayBatDau" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Ngày kết thúc</label>
                  <input type="date" class="form-control" v-model="comboEdit.ngayKetThuc" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Chi tiết combo</label>
                  <div class="card mb-3" :class="{ 'selected-card': selectedDetailIndex === index && isProductSidebarOpen }" v-for="(detail, index) in comboEdit.chitietcombos" :key="index">
                    <div class="card-body">
                      <div class="row">
                        <div class="col-md-6">
                          <label class="form-label">Sản phẩm</label>
                          <div class="input-group">
                            <input type="text" class="form-control" :value="productMap[detail.maSp] || 'Chọn sản phẩm'"
                              readonly />
                            <button class="btn btn-outline-primary" type="button" @click="openProductSidebar(index)">
                              Chọn
                            </button>
                          </div>
                        </div>
                        <div class="col-md-4">
                          <label class="form-label">Số lượng</label>
                          <input type="number" class="form-control" v-model="detail.soLuongSp" min="1"
                            @keydown="blockNegativeNumbers" />
                        </div>
                        <div class="col-md-2 d-flex align-items-end">
                          <button @click="removeDetailCombo(index)" type="button" class="btn btn-danger btn-sm">
                            Xóa
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                  <button @click="addDetailCombo()" type="button" class="btn btn-secondary"
                    style="background-color: #4C7CF3; margin-bottom: 10px;">
                    Thêm chi tiết combo
                  </button>
                </div>

                <div class="mb-3">
                  <label class="form-label">Phần trăm giảm</label>
                  <input type="number" class="form-control" v-model="comboEdit.phanTramGiam" min="0"
                    @input="resetSoTienGiam" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Số tiền giảm</label>
                  <input type="number" class="form-control" v-model="comboEdit.soTienGiam" min="0"
                    @input="resetPhanTramGiam" />
                  <label style="color: red" class="error-message"></label>
                </div>

                <div class="mb-3">
                  <label class="form-label">Hình ảnh</label>
                  <input @change="handleFileChange(comboEdit, $event)" type="file" class="form-control" accept="image/*" />
                  <img v-if="comboEdit.hinh && typeof comboEdit.hinh == 'string'"
                    :src="`${getUrlAPI}/HinhAnh/AnhCombo/${comboEdit.hinh}`" alt="Ảnh combo" class="img-fluid mt-2"
                    style="max-width: 100px; height: auto" @error="comboEdit.hinh = null" />
                  <span v-else>Không có ảnh</span>
                  <label style="color: red" class="error-message imageMessage"></label>
                </div>

                <div class="modal-footer p-0 pt-3">
                  <button type="button" class="btn btn-secondary" @click="cancelEdit()">Hủy</button>
                  <button type="button" @click="UpdateCombo()" class="btn btn-primary">Lưu thay đổi</button>
                </div>
              </form>
            </div>

            <!-- Bên phải: Sidebar chọn sản phẩm -->
            <div class="col-lg-5 col-md-12 right-pane" v-if="isProductSidebarOpen">
              <div class="d-flex align-items-center justify-content-between mb-3">
                <h5 class="mb-0">Chọn sản phẩm</h5>
                <button class="btn-close" @click="closeProductSidebar()" aria-label="Close"></button>
              </div>

              <div class="row g-3 mb-3">
                <div class="col-md">
                  <input id="product-sidebar-search" style="background-color: white" v-model="search" @click.stop @keydown.stop
                    @input="filterProducts" type="text" class="form-control" placeholder="Tìm kiếm sản phẩm..." />
                </div>
              </div>

              <div class="table-responsive sidebar-table">
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
                        <img :src="`${getUrlAPI}/HinhAnh/Products/${product.anhDaiDien || 'default.png'}`"
                          alt="Product Image" width="50" height="50" style="object-fit: cover; border-radius: 5px"
                          @error="product.anhDaiDien = null" />
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

              <nav class="d-flex justify-content-center mt-3">
                <ul class="pagination mb-0">
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
          </div>
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

.btn-danger {
  font-size: 12px;
  padding: 2px 6px;
}

/* Bố cục 2 cột trong modal */
.edit-layout {
  min-height: 60vh;
}
.left-pane {
  max-height: 70vh;
  overflow-y: auto;
  padding-right: 8px;
}
.right-pane {
  border-left: 1px solid #e5e5e5;
  max-height: 70vh;
  overflow-y: auto;
}
.sidebar-table {
  max-height: calc(70vh - 160px);
  overflow-y: auto;
}

.modal-body input.form-control {
  pointer-events: auto;
  user-select: auto;
  z-index: 50;
}

/* Highlight card được chọn khi đang mở sidebar chọn sản phẩm */
.selected-card {
  border-color: #4C7CF3 !important;
  box-shadow: 0 0 0 2px rgba(76, 124, 243, 0.2);
  background: #f8faff;
}
</style>
