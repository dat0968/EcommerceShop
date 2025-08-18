<template>
  <div>
    <!-- Nút thử đồ -->
    <button class="btn btn-primary try-on-btn" @click="showModalTryOn = true" title="Thử đồ với AI">
      <i class="bi bi-magic"></i> Thử đồ
    </button>

    <!-- Modal thử đồ -->
    <div v-if="showModalTryOn" class="modal-overlay" @click.self="showModalTryOn = false">
      <div class="tryon-modal">
        <div class="modal-header">
          <h2>Thử đồ với AI</h2>
          <div class="header-actions">
            <button class="btn icon-btn" @click="showApiSettings = true" title="Cài đặt API Keys">
              <i class="bi bi-gear"></i>
            </button>
            <button class="close-btn" @click="showModalTryOn = false">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
        </div>
        <div class="modal-body">
          <div v-if="loading" class="loading-overlay">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <span class="mt-2">AI đang xử lý...</span>
          </div>
          <div class="tryon-container">
            <!-- Sidebar for selections -->
            <div class="tryon-sidebar">
              <!-- Model Selection -->
              <div class="selection-section">
                <h5>1. Chọn người mẫu</h5>
                <div class="model-list">
                  <div
                    v-for="(model, index) in models"
                    :key="index"
                    class="item-card model-card"
                    :class="{ selected: selectedTryOnModel === model }"
                    @click="selectPredefinedModel(model)"
                  >
                    <img :src="model.url" :alt="model.name" class="item-thumbnail" />
                    <span>{{ model.name }}</span>
                  </div>
                  <div v-if="userModelPreviewUrl" class="item-card model-card user-model-preview selected">
                    <img :src="userModelPreviewUrl" alt="Mẫu của bạn" class="item-thumbnail" />
                    <span>Mẫu của bạn</span>
                  </div>
                </div>
                <div class="upload-area">
                  <input
                    type="file"
                    @change="handleModelUpload"
                    accept="image/*"
                    id="modelUploadInputSingle"
                    style="display: none"
                  />
                  <label for="modelUploadInputSingle" class="btn btn-outline-info btn-sm">
                    <i class="bi bi-upload"></i> Tải ảnh của bạn
                  </label>
                </div>
              </div>

              <!-- Product Image Selection -->
              <div class="selection-section">
                <h5>2. Chọn ảnh sản phẩm</h5>
                <div class="product-image-list">
                   <div
                    v-for="(image, index) in productImages"
                    :key="index"
                    class="item-card product-image-card"
                    :class="{ selected: selectedProductImage === image }"
                    @click="selectProductImage(image)"
                  >
                    <img :src="image" alt="Ảnh sản phẩm" class="item-thumbnail" />
                  </div>
                </div>
              </div>
                 <button
                class="btn btn-primary try-on-action w-100 mt-3"
                @click="performTryOn"
                :disabled="!selectedProductImage || (!userModelFile && !selectedTryOnModel) || loading"
              >
                <i class="bi bi-magic"></i> Thử đồ ngay
              </button>
            </div>

            <!-- Main content for result -->
            <div class="tryon-main-content">
              <h4>Kết quả</h4>
              <div v-if="tryOnResult" class="display-card result-card">
                <div class="try-on-image-container">
                    <img :src="tryOnResult.image" alt="Kết quả thử đồ" class="img-fluid result-image" />
                    <button class="download-btn" @click="downloadTryOnResult" title="Tải ảnh về">
                        <i class="bi bi-download"></i>
                    </button>
                </div>
                <div class="result-info">
                  <div v-if="tryOnResult.score != null">
                    <b>Điểm thẩm mỹ:</b>
                    <span class="score">{{ tryOnResult.score }}/10</span>
                  </div>
                   <div v-if="tryOnResult.style">
                    <b>Phong cách:</b> {{ tryOnResult.style }}
                  </div>
                  <div v-if="tryOnResult.gender_suitability">
                    <b>Giới tính:</b> {{ tryOnResult.gender_suitability }}
                  </div>
                  <div v-if="tryOnResult.score == null && tryOnResult.style == null" class="mt-2 text-muted">
                    <small>AI không thể phân tích ảnh này.</small>
                  </div>
                </div>
              </div>
              <div v-else class="placeholder">
                <i class="bi bi-image-fill"></i>
                <span>Kết quả thử đồ sẽ xuất hiện ở đây</span>
              </div>
            </div>
          </div>
        </div>

        <!-- API Settings Modal -->
        <div v-if="showApiSettings" class="api-settings-overlay">
          <div class="api-settings-modal">
            <h3>Cài đặt API Key</h3>
            <div class="form-group mb-2">
              <label for="lightxApiKey">
                LightX API Key
                <i
                  class="bi bi-info-circle"
                  @click="showApiKeyHelp('lightx')"
                  title="Làm thế nào để lấy API Key?"
                ></i>
              </label>
              <input
                type="text"
                id="lightxApiKey"
                v-model="apiKeys.lightxApiKey"
                class="form-control"
                placeholder="Nhập LightX API Key"
              />
            </div>
            <div class="api-settings-actions d-flex justify-content-between">
              <button class="btn btn-secondary" @click="cancelApiSettings">Hủy</button>
              <button class="btn btn-primary" @click="saveApiSettings">Lưu</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import Swal from 'sweetalert2'
import LightXService from '@/services/LightXService'

export default {
  name: 'TryOnProduct',
  props: {
    product: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      showModalTryOn: false,
      showApiSettings: false,
      loading: false,
      apiKeys: {
        lightxApiKey: localStorage.getItem('lightxApiKey') || '',
      },
      selectedTryOnModel: null,
      selectedProductImage: null,
      models: [
        {
          name: 'Nam đứng',
          url: 'https://images.pexels.com/photos/532220/pexels-photo-532220.jpeg?w=400',
        },
        {
          name: 'Nữ đứng',
          url: 'https://images.pexels.com/photos/29565763/pexels-photo-29565763.jpeg',
        },
      ],
      userModelFile: null,
      userModelPreviewUrl: '',
      tryOnResult: null,
    }
  },
   computed: {
    productImages() {
      if (!this.product) return [];
      // This handles the case where product details are fetched from an API
      // and might have a nested `images` array or a single `image` property.
      if (this.product.images && this.product.images.length > 0) {
        // Assuming images is an array of strings (URLs)
        return this.product.images;
      }
      // Fallback for single image
      if (this.product.image) {
        return [this.product.image];
      }
      return [];
    }
  },
  watch: {
    showModalTryOn(val) {
      if (val) {
        // Reset state when modal opens
        this.tryOnResult = null;
        this.selectedProductImage = this.productImages[0] || null;
        this.selectPredefinedModel(this.models[0]); // Default to the first model
      }
    },
  },
  methods: {
    selectProductImage(image) {
        this.selectedProductImage = image;
    },
    showApiKeyHelp() {
        Swal.fire({
          title: 'Cách lấy LightX API Key',
          html: `
            <div style="text-align: left; padding: 1em;">
              <ol>
                <li>Đăng ký hoặc đăng nhập vào tài khoản LightX tại <a href="https://www.lightxeditor.com/" target="_blank">trang chủ LightX</a>.</li>
                <li>Điều hướng đến phần API hoặc cài đặt tài khoản của bạn.</li>
                <li>Tạo một API Key mới hoặc sao chép một API Key hiện có.</li>
                <li>Dán API Key của bạn vào đây.</li>
              </ol>
            </div>
          `,
          icon: 'info',
          confirmButtonText: 'Đã hiểu',
        });
    },
    selectPredefinedModel(model) {
      this.selectedTryOnModel = model;
      this.userModelFile = null;
      this.userModelPreviewUrl = '';
      const input = document.getElementById('modelUploadInputSingle');
      if (input) {
        input.value = '';
      }
    },
    handleModelUpload(event) {
      const file = event.target.files[0];
      if (!file) return;

      if (!file.type.startsWith('image/')) {
        Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Vui lòng chọn file hình ảnh.' });
        return;
      }

      const maxSize = 5 * 1024 * 1024;
      if (file.size > maxSize) {
        Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Kích thước file không được vượt quá 5MB.' });
        return;
      }
      this.userModelFile = file;
      const reader = new FileReader();
      reader.onload = (e) => { this.userModelPreviewUrl = e.target.result; };
      reader.readAsDataURL(file);
      this.selectedTryOnModel = null;
    },
    cancelApiSettings() {
      this.showApiSettings = false;
    },
    saveApiSettings() {
      localStorage.setItem('lightxApiKey', this.apiKeys.lightxApiKey);
      this.showApiSettings = false;
      Swal.fire({ icon: 'success', title: 'Thành công', text: 'API Key đã được lưu.', timer: 1500, showConfirmButton: false });
    },
    async performTryOn() {
      // --- Pre-flight checks ---
      if (!this.apiKeys.lightxApiKey) {
        Swal.fire({
          icon: 'warning',
          title: 'Thiếu API Key',
          text: 'Vui lòng cài đặt LightX API Key trước khi thử đồ.',
          showCancelButton: true,
          confirmButtonText: 'Cài đặt ngay',
          cancelButtonText: 'Để sau',
        }).then((result) => { if (result.isConfirmed) { this.showApiSettings = true; } });
        return;
      }
      if (!this.selectedTryOnModel && !this.userModelFile) {
        Swal.fire({ icon: 'warning', title: 'Chưa chọn mẫu', text: 'Vui lòng chọn một mẫu có sẵn hoặc tải lên ảnh của bạn.' });
        return;
      }
       if (!this.selectedProductImage) {
        Swal.fire({ icon: 'warning', title: 'Chưa chọn ảnh', text: 'Vui lòng chọn một ảnh sản phẩm để thử.' });
        return;
      }

      this.loading = true;
      this.tryOnResult = null;

      const modelImageUrl = this.selectedTryOnModel
        ? this.selectedTryOnModel.url
        : this.userModelPreviewUrl;
      
      // The service expects an array of products. We create one with the selected image.
      const productForTryOn = { ...this.product, image: this.selectedProductImage };

      try {
        const { tryOnImageUrl, analysisResult } = await LightXService.performTryOnAndAnalysis(
          this.apiKeys.lightxApiKey,
          modelImageUrl,
          [productForTryOn] // Pass as an array
        );

        // Set the result with whatever we get back
        this.tryOnResult = {
          image: tryOnImageUrl,
          score: analysisResult?.aesthetic_score,
          style: analysisResult?.style,
          gender_suitability: analysisResult?.gender_suitability,
        };

        // If analysis failed, show a non-blocking toast
        if (!analysisResult) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
            });
            Toast.fire({
                icon: 'warning',
                title: 'Phân tích AI thất bại, nhưng ảnh đã được tạo!',
            });
        }

      } catch (error) {
        // This catch block now only handles critical errors (image generation failed)
        console.error('Error during try-on process:', error);
        Swal.fire({
          icon: 'error',
          title: 'Lỗi tạo ảnh',
          text: error.message || 'Có lỗi xảy ra trong quá trình tạo ảnh thử đồ.',
        });
      } finally {
        this.loading = false;
      }
    },
    async downloadTryOnResult() {
      if (!this.tryOnResult || !this.tryOnResult.image) return;

      try {
        const response = await fetch(this.tryOnResult.image);
        if (!response.ok) throw new Error(`Network response was not ok: ${response.statusText}`);
        const blob = await response.blob();
        
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const extension = blob.type.split('/')[1] || 'jpg';
        a.download = `tryon_result_${this.product.id}.${extension}`;
        document.body.appendChild(a);
        a.click();
        
        URL.revokeObjectURL(url);
        document.body.removeChild(a);

      } catch (error) {
        console.error('Error downloading the try-on image:', error);
        Swal.fire({
          icon: 'error',
          title: 'Tải ảnh thất bại',
          text: 'Không thể tải về hình ảnh. Vui lòng kiểm tra kết nối và thử lại.',
        });
      }
    },
  },
}
</script>

<style scoped>
/* General Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.3s ease-out;
}

.tryon-modal {
  background: #f4f7fc;
  border-radius: 16px;
  width: 95vw;
  max-width: 1200px;
  height: 90vh;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

/* Header */
.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  border-bottom: 1px solid #e5e7eb;
  background: #ffffff;
}
.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2a44;
}
.header-actions { display: flex; gap: 8px; }
.icon-btn, .close-btn {
  background: none;
  border: none;
  color: #6b7280;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.2s ease;
}
.icon-btn:hover, .close-btn:hover { color: #1f2a44; }

/* Body */
.modal-body {
  padding: 0;
  overflow: hidden;
  flex: 1;
}

.tryon-container {
  display: flex;
  height: 100%;
}

/* Sidebar */
.tryon-sidebar {
  width: 320px;
  background: #ffffff;
  padding: 20px;
  border-right: 1px solid #e5e7eb;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.selection-section h5 {
  font-size: 1.1rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid #eee;
}

/* Item Lists (Model & Product) */
.model-list, .product-image-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 12px;
}
.item-card {
  cursor: pointer;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  transition: all 0.3s ease;
  background: #ffffff;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px;
  text-align: center;
}
.item-card:hover {
  border-color: #3b82f6;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.2);
}
.item-card.selected {
  border-color: #2563eb;
  background: #eff6ff;
  box-shadow: 0 0 12px rgba(37, 99, 235, 0.3);
  transform: scale(1.05);
}
.item-thumbnail {
  width: 100%;
  height: 120px;
  object-fit: cover;
  border-radius: 8px;
  margin-bottom: 8px;
}
.item-card span {
  font-size: 0.85rem;
  color: #4b5563;
  font-weight: 500;
}
.upload-area { margin-top: 12px; text-align: center; }

/* Main Content */
.tryon-main-content {
  flex: 1;
  padding: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start; /* Align to top */
  overflow-y: auto;
}
.tryon-main-content h4 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2a44;
  margin-bottom: 16px;
}
.display-card.result-card {
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 20px;
  width: 100%;
  max-width: 500px;
  text-align: center;
}
.result-image {
  max-width: 100%;
  border-radius: 8px;
  margin-bottom: 16px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}
.result-info {
  display: flex;
  flex-direction: column;
  gap: 10px;
  font-size: 1rem;
  color: #374151;
}
.result-info .score { color: #d97706; font-weight: 700; font-size: 1.1rem; }

/* Placeholder */
.placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  width: 100%;
  color: #9ca3af;
  background-color: #f9fafb;
  border-radius: 12px;
  border: 2px dashed #e5e7eb;
}
.placeholder i { font-size: 4rem; margin-bottom: 16px; }
.placeholder span { font-size: 1.1rem; font-weight: 500; }

/* Buttons & Actions */
.try-on-action {
  font-size: 1rem;
  font-weight: 500;
  padding: 12px 24px;
}
.btn-primary:disabled {
    background-color: #9ca3af;
    cursor: not-allowed;
}

/* Loading & API Settings */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.9);
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #1f2a44;
}
.api-settings-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1001;
  display: flex;
  align-items: center;
  justify-content: center;
}
.api-settings-modal {
  background: #ffffff;
  border-radius: 12px;
  padding: 24px;
  width: 90%;
  max-width: 450px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
}

/* Download Button Styles */
.try-on-image-container {
  position: relative;
  display: inline-block;
}

.download-btn {
  position: absolute;
  bottom: 8px;
  right: 8px;
  background: rgba(0, 0, 0, 0.6);
  color: white;
  border: none;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 1rem;
}

.download-btn:hover {
  background: rgba(0, 0, 0, 0.8);
}
</style>
