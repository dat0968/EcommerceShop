<script setup>
import { ref, watch, onMounted, nextTick } from 'vue'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../../../src/constants/api.js'
import * as bootstrap from 'bootstrap'
import Cookies from 'js-cookie' // Thêm để lấy token

let getApiUrl = GetApiUrl()
const getUrlAPI = ref('https://localhost:7217')

const props = defineProps({
  ListProduct: Object,
  Combo: Object,
})

// State cho modal chọn sản phẩm
const showProductModal = ref(false)
const selectedDetailIndex = ref(null)
const productCurrentPage = ref(1)
const productTotalPages = ref(1)
const toTalPages = ref(1)
const pageSelected = ref(1)
const productList = ref([])
const productMap = ref({})

const initialCombo = ref(null)
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
})

// Watch hiển thị modal sản phẩm
watch(showProductModal, (newValue) => {
  nextTick(() => {
    const modalElement = document.getElementById('productModal')
    if (modalElement) {
      const modal = bootstrap.Modal.getOrCreateInstance(modalElement)
      if (newValue) {
        modal.show()
      } else {
        modal.hide()
      }
    }
  })
})

// Khởi tạo dữ liệu
onMounted(() => {
  console.log('props.Combo:', props.Combo); // Debug dữ liệu props
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
    isActive: true,
    ngayBatDau: formatDateForInput(props.Combo.ngayBatDau),
    ngayKetThuc: formatDateForInput(props.Combo.ngayKetThuc),
    chitietcombos: (props.Combo.chitietcombos || [{ maSp: '', soLuongSp: 1 }]).map((detail) => ({
      ...detail,
    })),
  }
  
  comboEdit.value = {
    ...initialCombo.value,
    chitietcombos: initialCombo.value.chitietcombos.map((detail) => ({ ...detail })),
  }

  // Khởi tạo productMap
  if (props.ListProduct && Array.isArray(props.ListProduct)) {
    props.ListProduct.forEach(product => {
      productMap.value[product.maSp] = product.tenSanPham
    })
  }
  
  // Cập nhật productMap với các sản phẩm đã chọn
  comboEdit.value.chitietcombos.forEach(detail => {
    if (detail.maSp && props.ListProduct) {
      const product = props.ListProduct.find(p => p.maSp === detail.maSp)
      if (product) {
        productMap.value[product.maSp] = product.tenSanPham
      }
    }
  })
})

// Fetch danh sách sản phẩm
async function fetchProducts(page) {
  try {
    console.log('Fetching products for page:', page);
    const response = await fetch(
      `${getApiUrl}/api/Products?page=${page}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          //'Authorization': `Bearer ${Cookies.get('accessToken') || ''}`, // Thêm token
        },
      }
    )

    if (!response.ok) {
      throw new Error('Lỗi khi lấy dữ liệu sản phẩm: ' + response.status)
    }

    const result = await response.json()
    productList.value = result.data || []
    productTotalPages.value = result.toTalPages || 1
    toTalPages.value = result.toTalPages || 1
    pageSelected.value = page

    productList.value.forEach(product => {
      productMap.value[product.maSp] = product.tenSanPham
    })
  } catch (error) {
    console.error('Lỗi fetchProducts:', error)
    Swal.fire('Không thể tải danh sách sản phẩm', error.message, 'error')
  }
}

// Mở modal chọn sản phẩm
function openProductModal(index) {
  selectedDetailIndex.value = index
  showProductModal.value = true
  productCurrentPage.value = 1
  fetchProducts(1)
}

// Chọn sản phẩm từ modal
function selectProduct(product) {
  if (selectedDetailIndex.value !== null) {
    comboEdit.value.chitietcombos[selectedDetailIndex.value].maSp = product.maSp
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product.tenSanPham
    }
  }
  showProductModal.value = false
}

// Chuyển trang sản phẩm
function ChangePage(page) {
  if (page !== pageSelected.value && page >= 1 && page <= toTalPages.value) {
    pageSelected.value = page
    fetchProducts(page)
  }
}

// Validate số âm
const blockNegativeNumbers = (event) => {
  if (event.key === '-') {
    event.preventDefault()
  }
}

// Validate dữ liệu
watch(
  comboEdit,
  (newcomboEdit) => {
    if (newcomboEdit.soLuong < 1 && newcomboEdit.soLuong !== '') {
      newcomboEdit.soLuong = 1
    }
    if (newcomboEdit.soTienGiam < 0) {
      newcomboEdit.soTienGiam = 0
    }
    if (newcomboEdit.phanTramGiam < 0) {
      newcomboEdit.phanTramGiam = 0
    }
    if (newcomboEdit.ngayBatDau && newcomboEdit.ngayKetThuc) {
      const startDate = new Date(newcomboEdit.ngayBatDau)
      const endDate = new Date(newcomboEdit.ngayKetThuc)
      if (startDate > endDate) {
        Swal.fire('Ngày bắt đầu không được lớn hơn ngày kết thúc', '', 'error')
        newcomboEdit.ngayKetThuc = ''
      }
    }
  },
  { deep: true }
)

// Reset giá trị giảm
function resetSoTienGiam() {
  comboEdit.value.soTienGiam = 0
}

function resetPhanTramGiam() {
  comboEdit.value.phanTramGiam = 0
}

// Xử lý file ảnh
function handleFileChange(comboEdit, event) {
  const file = event.target.files[0]
  if (file) {
    comboEdit.hinh = file
  }
}

// Thêm/Xóa chi tiết combo
function addDetailCombo() {
  comboEdit.value.chitietcombos.push({
    maSp: '',
    soLuongSp: 1,
  })
}

function removeDetailCombo(index) {
  if (comboEdit.value.chitietcombos.length > 1) {
    comboEdit.value.chitietcombos.splice(index, 1)
  }
}

// Hủy thay đổi
const cancelEdit = () => {
  const comboChanged = JSON.stringify(comboEdit.value) !== JSON.stringify(initialCombo.value)
  if (comboChanged) {
    Swal.fire({
      title: 'Bạn có muốn lưu các thay đổi này không ?',
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: 'Có',
      denyButtonText: `Tiếp tục chỉnh sửa`,
      cancelButtonText: 'Hủy',
    }).then((result) => {
      if (result.isConfirmed) {
        UpdateCombo()
      } else if (result.isDenied) {
        Swal.clickCancel()
      } else {
        comboEdit.value = {
          ...initialCombo.value,
          chitietcombos: initialCombo.value.chitietcombos.map((detail) => ({ ...detail })),
        }
        const instanceModal = document.getElementById(`comboEditModal_${props.Combo.maCombo}`)
        const closeButton = instanceModal.querySelector('[data-bs-dismiss="modal"]')
        if (closeButton) {
          closeButton.click()
        }
      }
    })
  } else {
    const instanceModal = document.getElementById(`comboEditModal_${props.Combo.maCombo}`)
    const closeButton = instanceModal.querySelector('[data-bs-dismiss="modal"]')
    if (closeButton) {
      closeButton.click()
    }
  }
}

// Cập nhật combo
async function UpdateCombo() {
  try {
    let isValid = true;

    console.log('URL PUT:', `${getUrlAPI.value}/api/Combos/${props.Combo.maCombo}`); // Debug URL
    console.log('comboEdit:', comboEdit.value); // Debug dữ liệu gửi đi

    // Kiểm tra trùng lặp chi tiết combo
    const hasDuplicates = comboEdit.value.chitietcombos.some(
      (item, index, arr) => arr.findIndex((obj) => obj.maSp === item.maSp) !== index
    );
    if (hasDuplicates) {
      Swal.fire('Vui lòng không để hai sản phẩm trùng lặp trong combo', '', 'error');
      isValid = false;
    }

    // Kiểm tra mã combo
    if (!props.Combo.maCombo) {
      Swal.fire('Mã combo không hợp lệ', '', 'error');
      isValid = false;
    }

    // Kiểm tra các trường bắt buộc
    const form_input_combo = document.querySelectorAll('.data-editCombo .mb-3')
    form_input_combo.forEach((element) => {
      const inputValueCombo = element.querySelector('.form-control')
      const messageErrorCombo = element.querySelector('.error-message')
      if (messageErrorCombo) {
        messageErrorCombo.textContent = ''
      }
      
    })

    // Kiểm tra chi tiết combo
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

    // Kiểm tra ngày bắt đầu và ngày kết thúc
    if (!comboEdit.value.ngayBatDau || !comboEdit.value.ngayKetThuc) {
      Swal.fire('Ngày bắt đầu và ngày kết thúc không được để trống', '', 'error')
      isValid = false
    }
console.log(isValid)
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
    console.log('Sending data:', {
      tenCombo: comboEdit.value.tenCombo,
      ngayBatDau: formatDateForAPI(comboEdit.value.ngayBatDau),
      ngayKetThuc: formatDateForAPI(comboEdit.value.ngayKetThuc),
      // ... các trường khác
    });
console.log("sdhsajh")
    const response = await fetch(`https://localhost:7217/api/Combos/${props.Combo.maCombo}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${Cookies.get('accessToken') || ''}`, // Thêm token
      },
      body: formData,
    });
    console.log("shdgsj");
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
  <div
    class="modal fade"
    :id="`comboEditModal_${props.Combo.maCombo}`"
    tabindex="-1"
    data-bs-backdrop="static"
    data-bs-keyboard="false"
  >
    <button class="btn-close" data-bs-dismiss="modal"></button>
    <div class="modal-dialog modal-xl text-start">
      <div class="modal-content">
        <div class="modal-header bg-primary text-white">
          <h5 class="modal-title">Sửa thông tin combo</h5>
          <button @click="cancelEdit()" type="button" style="background: none; border: 0px; margin-left: 660px;">
            X
          </button>
        </div>

        <div class="modal-body p-4 data-editCombo">
          <form @submit.prevent>
            <!-- Tên combo -->
            <div class="mb-3">
              <label class="form-label">Tên combo</label>
              <input type="text" class="form-control" v-model="comboEdit.tenCombo" />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Mô tả -->
            <div class="mb-3">
              <label for="moTa" class="form-label">Mô tả</label>
              <textarea
                v-model="comboEdit.moTa"
                class="form-control"
                id="moTa"
                rows="3"
                placeholder="Nhập mô tả combo"
              ></textarea>
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Số lượng combo -->
            <div class="mb-3">
              <label class="form-label">Số lượng</label>
              <input
                @keydown="blockNegativeNumbers"
                v-model="comboEdit.soLuong"
                type="number"
                class="form-control"
                min="1"
              />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Ngày bắt đầu -->
            <div class="mb-3">
              <label class="form-label">Ngày bắt đầu</label>
              <input
                type="date"
                class="form-control"
                v-model="comboEdit.ngayBatDau"
              />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Ngày kết thúc -->
            <div class="mb-3">
              <label class="form-label">Ngày kết thúc</label>
              <input
                type="date"
                class="form-control"
                v-model="comboEdit.ngayKetThuc"
              />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Chi tiết combo -->
            <div class="mb-3">
              <label class="form-label">Chi tiết combo</label>
              <div
                class="card mb-3"
                v-for="(detail, index) in comboEdit.chitietcombos"
                :key="index"
              >
                <div class="card-body">
                  <div class="row">
                    <div class="col-md-6">
                      <label class="form-label">Sản phẩm</label>
                      <div class="input-group">
                        <input
                          type="text"
                          class="form-control"
                          :value="productMap[detail.maSp] || 'Chọn sản phẩm'"
                          readonly
                        />
                        <button
                          class="btn btn-outline-primary"
                          type="button"
                          @click="openProductModal(index)"
                        >
                          Chọn
                        </button>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <label class="form-label">Số lượng</label>
                      <input
                        type="number"
                        class="form-control"
                        v-model="detail.soLuongSp"
                        min="1"
                        @keydown="blockNegativeNumbers"
                      />
                    </div>
                    <div class="col-md-2 d-flex align-items-end">
                      <button
                        @click="removeDetailCombo(index)"
                        type="button"
                        class="btn btn-danger btn-sm"
                      >
                        Xóa
                      </button>
                    </div>
                  </div>
                </div>
              </div>
              <button @click="addDetailCombo()" type="button" class="btn btn-secondary">
                Thêm chi tiết combo
              </button>
            </div>

            <!-- Phần trăm giảm -->
            <div class="mb-3">
              <label class="form-label">Phần trăm giảm</label>
              <input
                type="number"
                class="form-control"
                v-model="comboEdit.phanTramGiam"
                min="0"
                @input="resetSoTienGiam"
              />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Số tiền giảm -->
            <div class="mb-3">
              <label class="form-label">Số tiền giảm</label>
              <input
                type="number"
                class="form-control"
                v-model="comboEdit.soTienGiam"
                min="0"
                @input="resetPhanTramGiam"
              />
              <label style="color: red" class="error-message"></label>
            </div>

            <!-- Hình ảnh -->
            <div>
              <label class="form-label">Hình ảnh</label>
              <input
                @change="handleFileChange(comboEdit, $event)"
                type="file"
                class="form-control"
                accept="image/*"
              />
              <img
                v-if="comboEdit.hinh && typeof comboEdit.hinh === 'string'"
                :src="getApiUrl+'/HinhAnh/AnhCombo/'+comboEdit.hinh"
                alt="Ảnh combo"
                class="img-fluid mt-2"
                style="max-width: 100px; height: auto"
                @error="comboEdit.hinh = null"
              />
              <span v-else>Không có ảnh</span>
              <label style="color: red" class="error-message imageMessage"></label>
            </div>

            <!-- Nút lưu -->
            <div class="text-end">
              <button type="button" @click="UpdateCombo()" class="btn btn-primary">
                Lưu thay đổi
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>

  <!-- Modal Chọn Sản Phẩm -->
  <div
    class="modal fade"
    id="productModal"
    tabindex="-1"
    aria-labelledby="productModalLabel"
    aria-hidden="true"
    v-if="showProductModal"
  >
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="productModalLabel">Chọn sản phẩm</h5>
          <button
            type="button"
            class="btn-close"
            @click="showProductModal = false"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body">
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
                <tr v-for="product in productList" :key="product.maSp">
                  <td>{{ product.maSp }}</td>
                  <td>{{ product.tenSanPham }}</td>
                  <td>
                    <img
                      :src="product.hinh ? `${getApiUrl}/HinhAnh/AnhSanPham/${product.hinh}` : '/placeholder-image.jpg'"
                      alt="Product Image"
                      width="50"
                      height="50"
                      style="object-fit: cover; border-radius: 5px"
                    />
                  </td>
                  <td>
                    <button
                      class="btn btn-primary btn-sm"
                      @click="selectProduct(product)"
                    >
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
        <div class="modal-footer">
          <button
            type="button"
            class="btn btn-secondary"
            @click="showProductModal = false"
          >
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
.btn-danger {
  font-size: 12px;
  padding: 2px 6px;
}
</style>