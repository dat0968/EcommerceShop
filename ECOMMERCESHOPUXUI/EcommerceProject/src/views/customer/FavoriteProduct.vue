<script>
import { ref, onMounted, watch } from 'vue';
import Cookies from 'js-cookie';
import { jwtDecode } from 'jwt-decode';
import Swal from 'sweetalert2';
import { decodeToken, validateToken } from '@/utils/auth';
import { GetApiUrl } from '@/constants/api'
function ReadToken(token) {
  if (token) {
    const decoded = jwtDecode(token);
    return {
      IdUser: decoded.sub,
      Phone: decoded.PhoneNumber,
      Name: decoded.FullName,
      Role: decoded.role,
      Exp: decoded.exp // Đơn vị giây
    };
  }
  return null;
}

export default {
  name: 'FavoriteList',
  setup() {
    const token = Cookies.get('accessToken');
    const decodedToken = ReadToken(token);
    const idKhachHang = decodedToken ? decodedToken.IdUser : null;

    const idKhachHangRef = ref(idKhachHang || 1);
    const favorites = ref({});
    const message = ref('');
    const success = ref(false);
    const getApiUrl = ref(GetApiUrl()); // URL cơ sở cho API

    // Modal state
    const showModal = ref(false);
    const selectedProduct = ref(null);
    const selectedColor = ref('');
    const selectedSize = ref('');
    const quantity = ref('1');
    const allImages = ref([]);
    const accessToken = ref(Cookies.get('accessToken'));
    const refreshToken = ref(Cookies.get('refreshToken'));

    // Fetch product details
    const fetchProductDetails = async (maSp) => {
      try {
        const validatetoken = await validateToken(accessToken.value, refreshToken.value);
        if (validatetoken.isValid) {
          accessToken.value = validatetoken.newAccessToken;
        }
        const maKhachHang = decodedToken ? decodedToken.IdUser : null;
        let url = `${getApiUrl.value}/api/Shop/Product/${maSp}`;
        if (maKhachHang != null) {
          url += `?maKh=${maKhachHang}`;
        }
        const response = await fetch(url, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        });
        if (!response.ok) {
          throw new Error('Failed to fetch product details');
        }
        const result = await response.json();
        selectedProduct.value = result;
        allImages.value = [];
        result.productDetails.forEach((element) => {
          element.images.forEach((image) => {
            allImages.value.push(image);
          });
        });
        const colors = [
          ...new Set(result.productDetails?.map((d) => d?.mauSac || '').filter((color) => color !== '')),
        ];
        selectedColor.value = colors[0] || '';
        selectedSize.value = result.productDetails.find((p) => p.mauSac === selectedColor.value)?.kichThuoc || '';
        quantity.value = '1';
      } catch (error) {
        console.error('Error fetching product details:', error);
        Swal.fire({
          title: 'Lỗi khi lấy chi tiết sản phẩm!',
          text: error.message,
          icon: 'error',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        });
      }
    };

    const fetchFavorites = async () => {
      try {
        if (!idKhachHangRef.value) {
          throw new Error('ID khách hàng không hợp lệ.');
        }
        const response = await fetch(
          `${getApiUrl.value}/api/Favorite/GetFavoriteProducts?idKhachHang=${idKhachHangRef.value}`
        );
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        favorites.value = {
          success: data.success || false,
          message: data.message || 'Không có thông tin.',
          data: Array.isArray(data.data) ? data.data : [],
        };
        message.value = '';
      } catch (error) {
        console.error('Error:', error);
        favorites.value = { success: false, message: 'Lỗi khi lấy danh sách sản phẩm yêu thích.' };
        message.value = error.message || 'Lỗi không xác định.';
        success.value = false;
      }
    };

    const deleteFavorite = async (idSp) => {
      const result = await Swal.fire({
        title: 'Bạn có chắc muốn xóa?',
        text: 'Sản phẩm sẽ bị xóa khỏi danh sách yêu thích.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Xóa',
        cancelButtonText: 'Hủy',
      });

      if (!result.isConfirmed) return;
      try {
        if (!idKhachHangRef.value) {
          throw new Error('ID khách hàng không hợp lệ.');
        }
        const response = await fetch(`${getApiUrl.value}/api/Favorite/DeleteFavoriteProducts`, {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            maKh: idKhachHangRef.value,
            maSp: idSp,
          }),
        });

        const data = await response.json();
        message.value = data.message;
        success.value = true;
        await fetchFavorites();
        Swal.fire({
          icon: 'success',
          title: 'Đã xóa!',
          text: data.message || 'Sản phẩm đã được xóa khỏi danh sách yêu thích.',
          timer: 2000,
          showConfirmButton: false,
        });
      } catch (error) {
        message.value = error.message || 'Lỗi khi xóa sản phẩm yêu thích.';
        success.value = false;
      }
    };

    // Hàm định dạng tiền tệ
    const formatCurrency = (value) => {
      if (!value) return '0 VNĐ';
      return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
      }).format(value);
    };

    // Open modal to select options
    const openModal = async (maSp) => {
      selectedProduct.value = null; // Reset before fetching new data
      await fetchProductDetails(maSp);
      showModal.value = true;
    };

    // Validate quantity
    const validateQuantity = () => {
      let value = quantity.value.toString().trim();
      value = value.replace(/[^\d]/g, '');
      if (value === '' || value === '0') {
        quantity.value = '1';
        return;
      }
      const number = parseInt(value);
      const maxQty = selectedProduct.value.productDetails.find(
        (p) => p.mauSac === selectedColor.value && p.kichThuoc === selectedSize.value
      )?.soLuongTon || 0;
      if (isNaN(number) || number < 1) {
        quantity.value = '1';
      } else if (number > maxQty) {
        quantity.value = maxQty.toString();
        Swal.fire({
          title: `Số lượng tối đa là ${maxQty}`,
          icon: 'warning',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        });
      } else {
        quantity.value = number.toString();
      }
    };

    // Add to cart from modal
    const addToCart = async () => {
      if (!idKhachHangRef.value) {
        Swal.fire({
          title: 'Vui lòng đăng nhập để thêm vào giỏ hàng!',
          icon: 'warning',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        });
        return;
      }

      try {
        const matched = selectedProduct.value.productDetails.find(
          (p) => p.mauSac === selectedColor.value && p.kichThuoc === selectedSize.value
        );
        if (!matched) {
          Swal.fire({
            title: 'Vui lòng chọn màu sắc và kích thước hợp lệ!',
            icon: 'error',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
          });
          return;
        }

        const content = {
          maKh: idKhachHangRef.value,
          maCtsp: matched.maCtsp,
          maCombo: null,
          soLuong: quantity.value,
          donGia: matched.donGia,
          giamGia: 0,
          tenHinhAnh: selectedProduct.value.hinhAnh || '',
          giohangctcombos: [],
        };

        const response = await fetch(`${getApiUrl.value}/api/Cart`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(content),
        });

        const result = await response.json();
        if (!response.ok || !result.success) {
          Swal.fire({
            title: result.error || 'Đã xảy ra lỗi',
            icon: 'error',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
          });
          return;
        }

        if (result.success) {
          showModal.value = false;
          Swal.fire({
            title: 'Đã thêm sản phẩm vào giỏ hàng.',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false,
            timerProgressBar: true,
          });
        }
      } catch (error) {
        Swal.fire({
          title: `${error.message}`,
          icon: 'error',
          timer: 2000,
          showConfirmButton: false,
          timerProgressBar: true,
        });
      }
    };

    // Watch selectedColor to update selectedSize and quantity
    watch(selectedColor, (newColor) => {
      const availableSizes = selectedProduct.value.productDetails
        .filter((p) => p.mauSac === newColor)
        .map((p) => p.kichThuoc);
      selectedSize.value = availableSizes.length > 0 ? availableSizes[0] : '';
      quantity.value = '1'; // Reset quantity when color changes
    });

    // Update color button style based on selected color
    const updateColorButtonStyle = (color) => {
      if (selectedColor.value === color) {
        return {
          backgroundColor: color.toLowerCase(),
          color: '#fff',
          borderColor: color.toLowerCase(),
        };
      }
      return {
        backgroundColor: '',
        color: '',
        borderColor: '#6c757d',
      };
    };

    onMounted(() => {
      fetchFavorites();
    });

    return {
      idKhachHangRef,
      favorites,
      message,
      success,
      deleteFavorite,
      getApiUrl,
      formatCurrency,
      showModal,
      selectedProduct,
      selectedColor,
      selectedSize,
      quantity,
      allImages,
      openModal,
      validateQuantity,
      addToCart,
      updateColorButtonStyle,
    };
  },
};</script>

<template>
  <div style="margin-top: 20px; margin-left: 120px; margin-right: 120px;">
    <nav aria-label="breadcrumb" class="mb-4">
      <ol class="breadcrumb">
        <li class="breadcrumb-item"><router-link to="/" class="text-decoration-none text-muted">Trang chủ</router-link></li>
        <li class="breadcrumb-item"><a href="/Shop" class="text-decoration-none text-muted">Sản phẩm</a></li>
        <li class="breadcrumb-item active text-muted" aria-current="page">Sản phẩm yêu thích</li>
      </ol>
    </nav>
    <h2 style="text-align: center; padding-bottom: 20px;">Sản phẩm yêu thích</h2>

    <!-- Danh sách sản phẩm yêu thích -->
    
      <div
        v-for="item in favorites.data"
        :key="item.maSp"
        class="favorite-item"
        style="display: flex; align-items: center; margin-bottom: 20px; padding: 15px; background-color: #FFFFFF; border-radius: 10px; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); position: relative;"
      >
        <div class="item-image" style="width: 200px; height: 200px;">
          <img
            :src="getApiUrl + '/HinhAnh/Products/' + item.hinhAnh"
            :alt="item.tenSanPham"
            style="width: 100%; height: 100%; object-fit: cover; border-radius: 5px;"
          />
        </div>
        <div class="item-details" style="margin-left: 50px; flex: 1;">
          <p class="item-name" style="font-size: 1.2rem; margin: 0;">{{ item.tenSanPham }}</p>
          <p class="item-price" style="font-size: 1.2rem; margin: 10px 0;">{{ formatCurrency(item.khoangGia) }}</p>
          <div class="d-grid gap-2 mb-2">
            <router-link :to="`/product/${item.maSp}`" class="detail-btn btn btn-outline-primary" style="text-decoration: none; width: 100%;">
              Xem chi tiết
            </router-link>
            <button @click="openModal(item.maSp)" class="btn btn-outline-danger" style="width: 100%;">
              <i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ
            </button>
          </div>
        </div>
        <div class="item-actions" style="position: absolute; top: 10px; right: 10px;">
          <button @click="deleteFavorite(item.maSp)" class="delete-btn" style="padding: 5px; background: none; border: none; cursor: pointer;">
            <i class="fa fa-times" style="font-size: 1.5rem; color: #f44336;"></i>
          </button>
        </div>
      </div>
    

    <!-- Thông báo khi không có sản phẩm -->
    <p v-if="favorites.data && favorites.data.length === 0" class="no-data" style="text-align: center; color: #f44336;">
      Không có sản phẩm yêu thích.
    </p>
    <!-- <p v-else-if="favorites.message && !favorites.success" class="error-message" style="text-align: center; color: #f44336;">
      {{ favorites.message }}
    </p> -->

    <!-- Thông báo -->
    <p v-if="message" :class="{ 'success': success, 'error': !success }" style="text-align: center;">
      {{ message }}
    </p>

    <!-- Modal -->
    <div v-if="showModal" class="modal" style="display: block; background-color: rgba(0, 0, 0, 0.5);" @click.self="showModal = false">
      <div class="modal-dialog" style="max-width: 500px; margin: 50px auto;">
        <div class="modal-content" style="border-radius: 10px;">
          <div class="modal-header">
            <h5 class="modal-title">Chọn tùy chọn</h5>
            <button type="button" class="btn-close" @click="showModal = false"></button>
          </div>
          <div class="modal-body">
            <div class="mb-3">
              <label class="form-label fw-bold">Màu sắc:</label>
              <div class="d-flex gap-2 flex-wrap">
                <button
                  v-for="color in selectedProduct.productDetails.map((p) => p.mauSac)"
                  :key="color"
                  :style="updateColorButtonStyle(color)"
                  :class="['btn', 'btn-outline-secondary', 'btn-sm', { 'active': selectedColor === color }]"
                  @click="selectedColor = color"
                  :disabled="!selectedProduct.productDetails.some((p) => p.mauSac === color && p.soLuongTon > 0)"
                >
                  {{ color }}
                </button>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label fw-bold">Kích thước:</label>
              <div class="d-flex gap-2 flex-wrap">
                <button
                  v-for="size in selectedProduct.productDetails
                    .filter((p) => p.mauSac === selectedColor)
                    .map((p) => p.kichThuoc)"
                  :key="size"
                  :class="['btn', 'btn-outline-secondary', 'btn-sm', { 'active': selectedSize === size }]"
                  @click="selectedSize = size"
                  :disabled="!selectedProduct.productDetails.some((p) => p.mauSac === selectedColor && p.kichThuoc === size && p.soLuongTon > 0)"
                >
                  {{ size }}
                </button>
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label fw-bold">Số lượng:</label>
              <div class="d-flex align-items-center">
                <div class="input-group" style="width: 140px;">
                  <button class="btn btn-outline-secondary" type="button" @click="quantity = Math.max(1, parseInt(quantity) - 1).toString()">-</button>
                  <input
                    type="number"
                    class="form-control text-center"
                    v-model="quantity"
                    @input="validateQuantity"
                    min="1"
                    :max="selectedProduct.productDetails.find((p) => p.mauSac === selectedColor && p.kichThuoc === selectedSize)?.soLuongTon || 0"
                  >
                  <button class="btn btn-outline-secondary" type="button" @click="quantity = Math.min(selectedProduct.productDetails.find((p) => p.mauSac === selectedColor && p.kichThuoc === selectedSize)?.soLuongTon || 0, parseInt(quantity) + 1).toString()">+</button>
                </div>
                <span class="ms-3 text-muted small">Còn {{ selectedProduct.productDetails.find((p) => p.mauSac === selectedColor && p.kichThuoc === selectedSize)?.soLuongTon || 0 }} sản phẩm</span>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showModal = false">Hủy</button>
            <button type="button" class="btn btn-danger" @click="addToCart" :disabled="!selectedColor || !selectedSize || parseInt(quantity) <= 0">
              <i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Banner
    <div class="row mb-5">
      <div class="col-12">
        <div class="row g-4">
          <div class="col-md-6">
            <div class="position-relative overflow-hidden rounded-4 angel-banner">
              <img
                src="https://i.pinimg.com/736x/1e/6b/26/1e6b26db806e77ae28f29ea52310746d.jpg"
                alt="Angel Fashion Banner"
                class="w-100"
                style="height: 350px; object-fit: cover;"
              />
            </div>
          </div>
          <div class="col-md-6">
            <div class="position-relative overflow-hidden rounded-4 angel-banner">
              <img
                src="https://i.pinimg.com/1200x/39/4b/4f/394b4f714fada2935ce2d63d867aca8d.jpg"
                alt="Angel Fashion Trends"
                class="w-100"
                style="height: 350px; object-fit: cover;"
              />
            </div>
          </div>
        </div>
      </div>
    </div> -->
  </div>
</template>
<style scoped>
.favorite-items {
  max-width: 100%;
  margin: 0 auto;
  padding: 20px 0;
  font-family: Arial, sans-serif;
  background-color: #F5F1E9;
  border-radius: 10px;
  padding: 15px;
}

.favorite-item {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding: 15px;
  background-color: #FFFFFF;
  border-radius: 10px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  position: relative;
}

.item-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 5px;
}

.item-details {
  margin-left: 50px;
  flex: 1;
}

.item-name {
  font-size: 1.2rem;
  font-weight: bold;
  margin: 0;
}

.item-price {
  font-size: 1.2rem;
  color: #333;
  margin: 10px 0;
}

.detail-btn {
  padding: 8px 16px;
  background-color: #2196F3;
  color: white;
  border: none;
  border-radius: 3px;
  text-decoration: none;
  display: inline-block;
  text-align: center;
}

.detail-btn:hover {
  background-color: #1976D2;
}

.item-actions {
  position: absolute;
  top: 10px;
  right: 10px;
}

.delete-btn {
  padding: 5px;
  background: none;
  border: none;
  cursor: pointer;
}

.delete-btn .fa-times {
  font-size: 1.5rem;
  color: #f44336;
}

.delete-btn:hover .fa-times {
  color: #d32f2f;
}

.no-data,
.error-message {
  color: #f44336;
  text-align: center;
  font-size: 1.1rem;
}

.success {
  color: #4CAF50;
  text-align: center;
}

.error {
  color: #f44336;
  text-align: center;
}

/* Modal Styles */
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1050;
}

.modal-dialog {
  max-width: 500px;
}

.modal-content {
  border: none;
  border-radius: 10px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.modal-header {
  border-bottom: 1px solid #eee;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 500;
}

.modal-body .btn {
  margin-bottom: 5px;
}

.modal-footer {
  border-top: 1px solid #eee;
}
</style>