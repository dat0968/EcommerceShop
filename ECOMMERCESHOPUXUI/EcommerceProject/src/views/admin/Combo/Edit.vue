<script setup>
import { ref, watch, onMounted, onUnmounted, nextTick } from 'vue'
import Swal from 'sweetalert2'
import { GetApiUrl } from '../../../../src/constants/api.js'
import * as bootstrap from 'bootstrap'
import Cookies from 'js-cookie'

let getApiUrl = GetApiUrl()
const getUrlAPI = ref('https://localhost:7217')

const props = defineProps({
  ListProduct: Object,
  Combo: Object,
})

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

const initialCombo = ref(null)
const showProductModal = ref(false)
const selectedDetailIndex = ref(null)
const productCurrentPage = ref(1)
const productTotalPages = ref(1)
const toTalPages = ref(1)
const pageSelected = ref(1)
const productList = ref([])
const productMap = ref({})
const search = ref('')
const token = Cookies.get('accessToken')

// Kích hoạt/ẩn modal chọn sản phẩm
watch(showProductModal, (newValue) => {
  console.log('showProductModal thay đổi:', newValue)
  nextTick(() => {
    const modalElement = document.getElementById('productModalEdit')
    if (modalElement) {
      const modal = bootstrap.Modal.getOrCreateInstance(modalElement, { backdrop: true })
      if (newValue) {
        console.log('Mở #productModalEdit')
        modal.show()
        setTimeout(() => {
          const searchInput = document.querySelector('#productModalEdit .modal-body input[type="text"]')
          if (searchInput) searchInput.focus()
        }, 500)
      } else {
        console.log('Đóng #productModalEdit')
        modal.hide()
      }
    } else {
      console.error('Không tìm thấy #productModalEdit')
      Swal.fire('Lỗi: Không tìm thấy modal chọn sản phẩm', '', 'error')
    }
  })
})

// Xử lý sự kiện đóng modal
onMounted(() => {
  const modalElement = document.getElementById('productModalEdit')
  if (modalElement) {
    modalElement.addEventListener('hidden.bs.modal', () => {
      console.log('productModalEdit closed (hidden.bs.modal triggered)')
      showProductModal.value = false
      close()
    })
  }

  // Kiểm tra dữ liệu đầu vào
  if (!props.Combo || !props.Combo.maCombo) {
    console.error('Dữ liệu props.Combo không hợp lệ:', props.Combo)
    Swal.fire('Lỗi: Dữ liệu combo không hợp lệ', '', 'error')
    return
  }

  // Khởi tạo dữ liệu
  const formatDateForInput = (dateString) => {
    if (!dateString) return ''
    const date = new Date(dateString)
    return date.toISOString().split('T')[0]
  }

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
    chitietcombos: (props.Combo.chitietcombos && props.Combo.chitietcombos.length > 0
      ? props.Combo.chitietcombos
      : [{ maSp: '', soLuongSp: 1 }]).map((detail) => ({
        ...detail,
      })),
  }

  comboEdit.value = {
    ...initialCombo.value,
    chitietcombos: initialCombo.value.chitietcombos.map((detail) => ({ ...detail })),
  }

  // Khởi tạo productMap từ props.ListProduct
  if (props.ListProduct && Array.isArray(props.ListProduct)) {
    console.log('Khởi tạo productMap từ props.ListProduct:', props.ListProduct)
    props.ListProduct.forEach(product => {
      productMap.value[product.maSp] = product.tenSanPham
    })
  } else {
    console.warn('props.ListProduct không hợp lệ hoặc rỗng:', props.ListProduct)
    fetchProducts(1) // Gọi API để lấy danh sách sản phẩm nếu props.ListProduct rỗng
  }

  comboEdit.value.chitietcombos.forEach(detail => {
    if (detail.maSp && props.ListProduct && Array.isArray(props.ListProduct)) {
      const product = props.ListProduct.find(p => p.maSp === detail.maSp)
      if (product) {
        productMap.value[product.maSp] = product.tenSanPham
      }
    }
  })
})

onUnmounted(() => {
  const modalElement = document.getElementById('productModalEdit')
  if (modalElement) {
    modalElement.removeEventListener('hidden.bs.modal', () => {})
  }
})

// Hàm close để mở lại modal chỉnh sửa
function close() {
  nextTick(() => {
    console.log('Gọi hàm close trong EditCombo')
    const backdrops = document.querySelectorAll('.modal-backdrop')
    backdrops.forEach((backdrop) => backdrop.remove())
    document.body.classList.remove('modal-open')
    document.body.style.removeProperty('overflow')
    document.body.style.removeProperty('padding-right')
    const editModalId = `comboEditModal_${props.Combo.maCombo}`
    const editModal = document.getElementById(editModalId)
    if (editModal) {
      const modal = bootstrap.Modal.getOrCreateInstance(editModal, { backdrop: 'static', keyboard: false })
      modal.show()
    } else {
      console.error(`Không tìm thấy modal với ID: ${editModalId}`)
      Swal.fire('Lỗi: Không tìm thấy modal chỉnh sửa', '', 'error')
    }
  })
}

// Fetch danh sách sản phẩm
async function fetchProducts(page) {
  try {
    console.log('Đang lấy sản phẩm cho trang:', page, 'với tìm kiếm:', search.value)
    if (!token) {
      throw new Error('Không tìm thấy accessToken')
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
    )
    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Lỗi khi lấy dữ liệu sản phẩm: ${response.status} - ${errorText}`)
    }
    const result = await response.json()
    console.log('Phản hồi API:', result)
    productList.value = result.data && Array.isArray(result.data) ? result.data : []
    productTotalPages.value = result.toTalPages || 1
    toTalPages.value = result.toTalPages || 1
    pageSelected.value = page
    productList.value.forEach(product => {
      productMap.value[product.maSp] = product.tenSanPham
    })
    console.log('Sản phẩm đã lấy:', productList.value)
    if (productList.value.length === 0) {
      console.warn('Danh sách sản phẩm rỗng')
      Swal.fire('Không tìm thấy sản phẩm nào', '', 'warning')
    }
  } catch (error) {
    console.error('Lỗi fetchProducts:', error.message)
    Swal.fire('Không thể tải danh sách sản phẩm', error.message, 'error')
  }
}

// Mở modal chọn sản phẩm
function openProductModal(index) {
  if (index >= 0 && index < comboEdit.value.chitietcombos.length) {
    selectedDetailIndex.value = index
    showProductModal.value = true
    productCurrentPage.value = 1
    fetchProducts(1)
    nextTick(() => {
      const editModalId = `comboEditModal_${props.Combo.maCombo}`
      const editModal = document.getElementById(editModalId)
      if (editModal) {
        const modal = bootstrap.Modal.getInstance(editModal)
        if (modal) modal.hide()
      } else {
        console.error(`Không tìm thấy modal chỉnh sửa với ID: ${editModalId}`)
        Swal.fire('Lỗi: Không tìm thấy modal chỉnh sửa', '', 'error')
      }
    })
  } else {
    console.error('Chỉ số chi tiết combo không hợp lệ:', index)
    Swal.fire('Lỗi: Chỉ số chi tiết combo không hợp lệ', '', 'error')
  }
}

// Lọc sản phẩm theo tìm kiếm
function filterProducts() {
  fetchProducts(1)
}

// Chọn sản phẩm
function selectProduct(product) {
  if (selectedDetailIndex.value !== null && selectedDetailIndex.value < comboEdit.value.chitietcombos.length) {
    comboEdit.value.chitietcombos[selectedDetailIndex.value].maSp = product.maSp
    if (!productMap.value[product.maSp]) {
      productMap.value[product.maSp] = product.tenSanPham
    }
    showProductModal.value = false
  } else {
    console.error('Không thể chọn sản phẩm: selectedDetailIndex không hợp lệ')
    Swal.fire('Lỗi: Không thể chọn sản phẩm', '', 'error')
  }
}

// Chuyển trang
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
      title: 'Bạn có muốn lưu các thay đổi này không?',
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: 'Có',
      denyButtonText: 'Tiếp tục chỉnh sửa',
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
    let isValid = true

    const hasDuplicates = comboEdit.value.chitietcombos.some(
      (item, index, arr) => arr.findIndex((obj) => obj.maSp === item.maSp) !== index
    )
    if (hasDuplicates) {
      Swal.fire('Vui lòng không để hai sản phẩm trùng lặp trong combo', '', 'error')
      isValid = false
    }

    if (!props.Combo.maCombo) {
      Swal.fire('Mã combo không hợp lệ', '', 'error')
      isValid = false
    }

    const form_input_combo = document.querySelectorAll('.data-editCombo .mb-3')
    form_input_combo.forEach((element) => {
      const inputValueCombo = element.querySelector('.form-control')
      const messageErrorCombo = element.querySelector('.error-message')
      if (messageErrorCombo) {
        messageErrorCombo.textContent = ''
      }
    })

    comboEdit.value.chitietcombos.forEach((detail) => {
      if (!detail.maSp) {
        Swal.fire('Vui lòng chọn sản phẩm cho tất cả chi tiết combo', '', 'error')
        isValid = false
      }
      if (detail.soLuongSp < 1 || detail.soLuongSp === '') {
        Swal.fire('Số lượng sản phẩm trong chi tiết combo phải lớn hơn 0', '', 'error')
        isValid = false
      }
    })

    if (!comboEdit.value.tenCombo) {
      isValid = false
      Swal.fire('Tên combo không được để trống', '', 'error')
    }

    if (!comboEdit.value.ngayBatDau || !comboEdit.value.ngayKetThuc) {
      Swal.fire('Ngày bắt đầu và ngày kết thúc không được để trống', '', 'error')
      isValid = false
    }

    if (!isValid) {
      return
    }

    const formatDateForAPI = (dateString) => {
      if (!dateString) return ''
      return new Date(dateString).toISOString()
    }

    const formData = new FormData()
    formData.append('tenCombo', comboEdit.value.tenCombo)
    if (comboEdit.value.hinh && typeof comboEdit.value.hinh !== 'string') {
      formData.append('hinh', comboEdit.value.hinh)
    }
    formData.append('soLuong', comboEdit.value.soLuong)
    formData.append('soTienGiam', comboEdit.value.soTienGiam)
    formData.append('phanTramGiam', comboEdit.value.phanTramGiam)
    formData.append('moTa', comboEdit.value.moTa)
    formData.append('isActive', comboEdit.value.isActive.toString())
    formData.append('ngayBatDau', formatDateForAPI(comboEdit.value.ngayBatDau))
    formData.append('ngayKetThuc', formatDateForAPI(comboEdit.value.ngayKetThuc))

    comboEdit.value.chitietcombos.forEach((detail, index) => {
      formData.append(`chitietcombos[${index}].maSp`, detail.maSp)
      formData.append(`chitietcombos[${index}].soLuongSp`, detail.soLuongSp)
    })

    const response = await fetch(`${getUrlAPI.value}/api/Combos/${props.Combo.maCombo}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${Cookies.get('accessToken') || ''}`,
      },
      body: formData,
    })

    if (!response.ok) {
      const errorText = await response.text()
      throw new Error(`Lỗi khi cập nhật combo: ${response.status} - ${errorText}`)
    }

    Swal.fire('Đã cập nhật thông tin combo sản phẩm', '', 'success')
    setTimeout(() => {
      window.location.reload()
    }, 2000)
  } catch (error) {
    console.error('Lỗi trong UpdateCombo:', error)
    Swal.fire('Lỗi khi cập nhật combo', error.message, 'error')
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
          <form @submit.prevent>
            <div class="mb-3">
              <label class="form-label">Tên combo</label>
              <input type="text" class="form-control" v-model="comboEdit.tenCombo" placeholder="Nhập tên combo" />
              <label style="color: red" class="error-message"></label>
            </div>

            <div class="mb-3">
              <label for="moTa" class="form-label">Mô tả</label>
              <textarea v-model="comboEdit.moTa" class="form-control" id="moTa" rows="3" placeholder="Nhập mô tả combo"></textarea>
              <label style="color: red" class="error-message"></label>
            </div>

            <div class="mb-3">
              <label class="form-label">Số lượng</label>
              <input @keydown="blockNegativeNumbers" v-model="comboEdit.soLuong" type="number" class="form-control" min="1" />
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
              <div class="card mb-3" v-for="(detail, index) in comboEdit.chitietcombos" :key="index">
                <div class="card-body">
                  <div class="row">
                    <div class="col-md-6">
                      <label class="form-label">Sản phẩm</label>
                      <div class="input-group">
                        <input type="text" class="form-control" :value="productMap[detail.maSp] || 'Chọn sản phẩm'" readonly />
                        <button class="btn btn-outline-primary" type="button" @click="openProductModal(index)">
                          Chọn
                        </button>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <label class="form-label">Số lượng</label>
                      <input type="number" class="form-control" v-model="detail.soLuongSp" min="1" @keydown="blockNegativeNumbers" />
                    </div>
                    <div class="col-md-2 d-flex align-items-end">
                      <button @click="removeDetailCombo(index)" type="button" class="btn btn-danger btn-sm">
                        Xóa
                      </button>
                    </div>
                  </div>
                </div>
              </div>
              <button @click="addDetailCombo()" type="button" class="btn btn-secondary" style="background-color: #4C7CF3; margin-bottom: 10px;">
                Thêm chi tiết combo
              </button>
            </div>

            <div class="mb-3">
              <label class="form-label">Phần trăm giảm</label>
              <input type="number" class="form-control" v-model="comboEdit.phanTramGiam" min="0" @input="resetSoTienGiam" />
              <label style="color: red" class="error-message"></label>
            </div>

            <div class="mb-3">
              <label class="form-label">Số tiền giảm</label>
              <input type="number" class="form-control" v-model="comboEdit.soTienGiam" min="0" @input="resetPhanTramGiam" />
              <label style="color: red" class="error-message"></label>
            </div>

            <div class="mb-3">
              <label class="form-label">Hình ảnh</label>
              <input @change="handleFileChange(comboEdit, $event)" type="file" class="form-control" accept="image/*" />
              <img v-if="comboEdit.hinh && typeof comboEdit.hinh == 'string'"
                :src="getApiUrl + '/HinhAnh/AnhCombo/' + comboEdit.hinh" alt="Ảnh combo" class="img-fluid mt-2"
                style="max-width: 100px; height: auto" @error="comboEdit.hinh = null" />
              <span v-else>Không có ảnh</span>
              <label style="color: red" class="error-message imageMessage"></label>
            </div>

            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="cancelEdit()">Hủy</button>
              <button type="button" @click="UpdateCombo()" class="btn btn-primary">Lưu thay đổi</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>

  <div class="modal fade" id="productModalEdit" tabindex="-1" aria-labelledby="productModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="productModalLabel">Chọn sản phẩm</h5>
          <button type="button" class="btn-close" @click="showProductModal = false; close()" aria-label="Close"></button>
        </div>
        <div class="modal-body">
          <div class="row g-3 mb-3">
            <div class="col-md">
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
                <tr v-if="productList.length == 0">
                  <td colspan="4" class="text-center">Không có sản phẩm nào</td>
                </tr>
                <tr v-else v-for="product in productList" :key="product.maSp">
                  <td>{{ product.maSp }}</td>
                  <td>{{ product.tenSanPham }}</td>
                  <td>
                    <img
                      :src="`${getUrlAPI.value}/HinhAnh/Products/${product.hinh}`"
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

.btn-danger {
  font-size: 12px;
  padding: 2px 6px;
}

.modal-body input.form-control {
  pointer-events: auto;
  user-select: auto;
  z-index: 50;
}
</style>