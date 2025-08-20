<script setup>
import { ref, onMounted, computed } from 'vue';
import { GetApiUrl } from '../../../../src/constants/api.js';
import Cookies from 'js-cookie';

const props = defineProps({
  Combo: Object,
});

const getApiUrl = GetApiUrl();
const token = Cookies.get('accessToken');

const comboDetails = ref(null);
const productMap = ref({});

// Helper to format date for display
const formatDateForInput = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toISOString().split('T')[0]; // Formats to YYYY-MM-DD
};

// Fetch product details if they are not already in the combo object
async function fetchProductDetails(maSp) {
  if (productMap.value[maSp]) return; // Already fetched

  try {
    const response = await fetch(`${getApiUrl}/api/Products/${maSp}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    if (!response.ok) throw new Error('Lỗi khi lấy dữ liệu sản phẩm.');
    const result = await response.json();
    if (result.success && result.data) {
      productMap.value[maSp] = result.data;
    }
  } catch (error) {
    console.error(`Lỗi fetchProductDetails cho ${maSp}:`, error);
    // Set a default to avoid breaking the template
    productMap.value[maSp] = { tenSanPham: 'Không thể tải', anhDaiDien: 'default.png' };
  }
}

onMounted(() => {
  if (props.Combo) {
    comboDetails.value = { ...props.Combo };

    // Pre-fetch all product details for the combo
    if (comboDetails.value.chitietcombos && comboDetails.value.chitietcombos.length > 0) {
      const productFetches = comboDetails.value.chitietcombos.map(detail => {
        // If product name or image is missing, fetch the full product details
        if (!detail.tenSp || !detail.hinh) {
           return fetchProductDetails(detail.maSp);
        } else {
           // If data is already present, just populate the map
           productMap.value[detail.maSp] = {
               tenSanPham: detail.tenSp,
               anhDaiDien: detail.hinh // Assuming the image field is named 'hinh'
           };
           return Promise.resolve();
        }
      });
      Promise.all(productFetches);
    }
  }
});

const getProductImage = (maSp) => {
    const product = productMap.value[maSp];
    if (product && product.productDetails[0].images[0].tenHinhAnh) {
        return `${getApiUrl}/HinhAnh/Products/${product.productDetails[0].images[0].tenHinhAnh}`;
    }
    // Fallback or placeholder image
    return 'https://via.placeholder.com/80';
};

const getProductName = (maSp) => {
    const product = productMap.value[maSp];
    return product ? product.tenSanPham : 'Đang tải...';
}

const statusText = computed(() => {
    if (!comboDetails.value) return '';
    return comboDetails.value.isActive ? 'Hoạt động' : 'Không hoạt động';
});

</script>

<template>
  <div class="modal fade" :id="`comboDetailModal_${props.Combo.maCombo}`" tabindex="-1" aria-labelledby="comboDetailModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl text-start">
      <div class="modal-content">
        <div class="modal-header" style="background-color: #4C7CF3;">
          <h5 class="modal-title text-white" id="comboDetailModalLabel">Chi Tiết Combo</h5>
          <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
        </div>

        <div v-if="comboDetails" class="modal-body p-4">
          <!-- General Info Box -->
          <div class="p-3 mb-4 bg-light border rounded">
              <div class="row">
                <!-- Cột trái: Thông tin chung -->
                <div class="col-md-6">
                  <div class="mb-4">
                    <label class="form-label fw-bold">Tên combo</label>
                    <input type="text" class="form-control" :value="comboDetails.tenCombo" readonly />
                  </div>
                  <div class="mb-4">
                    <label class="form-label fw-bold">Mô tả</label>
                    <textarea class="form-control" :value="comboDetails.moTa || 'Không có mô tả'" rows="3" readonly></textarea>
                  </div>
                  <div class="row">
                    <div class="col-md-6 mb-4">
                      <label class="form-label fw-bold">Số lượng</label>
                      <input type="number" class="form-control" :value="comboDetails.soLuong" readonly />
                    </div>
                    <div class="col-md-6 mb-4">
                      <label class="form-label fw-bold">Trạng thái</label>
                      <input type="text" class="form-control" :value="statusText" readonly />
                    </div>
                  </div>
                  <div class="row">
                    <div class="col-md-6 mb-md-0 mb-4">
                      <label class="form-label fw-bold">Ngày bắt đầu</label>
                      <input type="date" class="form-control" :value="formatDateForInput(comboDetails.ngayBatDau)" readonly />
                    </div>
                    <div class="col-md-6 mb-0">
                      <label class="form-label fw-bold">Ngày kết thúc</label>
                      <input type="date" class="form-control" :value="formatDateForInput(comboDetails.ngayKetThuc)" readonly />
                    </div>
                  </div>
                </div>

                <!-- Cột phải: Giảm giá & Hình ảnh -->
                <div class="col-md-6">
                  <div class="row">
                    <div class="col-md-6 mb-4">
                      <label class="form-label fw-bold">Phần trăm giảm (%)</label>
                      <input type="number" class="form-control" :value="comboDetails.phanTramGiam || 0" readonly />
                    </div>
                    <div class="col-md-6 mb-4">
                      <label class="form-label fw-bold">Số tiền giảm (VND)</label>
                      <input type="number" class="form-control" :value="comboDetails.soTienGiam || 0" readonly />
                    </div>
                  </div>
                  <div class="mb-4">
                    <label class="form-label fw-bold">Hình ảnh combo</label>
                    <div>
                      <img v-if="comboDetails.hinh" :src="`${getApiUrl}/HinhAnh/AnhCombo/${comboDetails.hinh}`" alt="Ảnh combo" class="img-thumbnail" />
                      <p v-else class="text-muted mt-2">Không có hình ảnh</p>
                    </div>
                  </div>
                </div>
              </div>
          </div>

          <!-- Chi tiết sản phẩm trong combo -->
          <div>
            <h5 class="mb-3">Các Sản Phẩm Trong Combo</h5>
            <div v-if="comboDetails.chitietcombos && comboDetails.chitietcombos.length > 0">
              <div v-for="(detail, index) in comboDetails.chitietcombos" :key="index" class="card product-card-horizontal mb-3">
                <div class="row g-0">
                  <div class="col-auto">
                     <img :src="getProductImage(detail.maSp)" class="img-fluid rounded-start product-card-img-horizontal" alt="Product Image" />
                  </div>
                  <div class="col">
                    <div class="card-body d-flex justify-content-between align-items-center">
                      <div>
                        <h6 class="card-title mb-1">{{ getProductName(detail.maSp) }}</h6>
                        <p class="card-text text-muted mb-0">Mã SP: {{ detail.maSp }}</p>
                      </div>
                      <div class="text-end">
                        <p class="mb-0">Số lượng: <span class="fw-bold">{{ detail.soLuongSp }}</span></p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div v-else>
              <p class="text-muted">Combo này không có sản phẩm nào.</p>
            </div>
          </div>
        </div>
        <div v-else class="modal-body p-5 text-center">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Đang tải dữ liệu...</p>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.modal-xl {
  max-width: 80%;
}
.img-thumbnail {
  max-width: 150px;
  height: auto;
  border-radius: 0.375rem;
}
.form-control[readonly] {
    background-color: #e9ecef;
    opacity: 1;
}
.bg-light {
    background-color: #f8f9fa !important;
}
.product-card-horizontal {
  border: 1px solid #e0e0e0;
  background-color: #fff;
}
.product-card-img-horizontal {
  width: 80px;
  height: 80px;
  object-fit: cover;
}
.product-card-horizontal .card-body {
  padding: 1rem;
}
.card-title {
    font-weight: 600;
}
</style>