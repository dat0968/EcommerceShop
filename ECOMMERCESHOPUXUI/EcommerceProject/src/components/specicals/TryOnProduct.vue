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
                    id="modelUploadInput"
                    style="display: none"
                  />
                  <label for="modelUploadInput" class="btn btn-outline-info btn-sm">
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
                @click="confirmModelSelection"
                :disabled="!selectedProductImage || (!userModelFile && !selectedTryOnModel) || loading"
              >
                <i class="bi bi-magic"></i> Thử đồ ngay
              </button>
            </div>

            <!-- Main content for result -->
            <div class="tryon-main-content">
              <h4>Kết quả</h4>
              <div v-if="tryOnResult" class="display-card result-card">
                <img :src="tryOnResult.image" alt="Kết quả thử đồ" class="img-fluid result-image" />
                <div class="result-info">
                  <div v-if="tryOnResult.score && tryOnResult.score !== 'N/A'">
                    <b>Điểm thẩm mỹ:</b>
                    <span class="score">{{ tryOnResult.score }}/10</span>
                  </div>
                   <div v-if="tryOnResult.style && tryOnResult.style !== 'N/A'">
                    <b>Phong cách:</b> {{ tryOnResult.style }}
                  </div>
                  <div v-if="tryOnResult.gender_suitability && tryOnResult.gender_suitability !== 'N/A'">
                    <b>Giới tính:</b> {{ tryOnResult.gender_suitability }}
                  </div>
                  <div v-if="analysisError" class="alert alert-warning mt-2 small">
                    {{ analysisError }}
                  </div>
                  <button class="btn btn-success mt-2" @click="downloadTryOnResult">
                    <i class="bi bi-download"></i> Tải kết quả
                  </button>
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
            <div class="form-group">
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
            <div class="api-settings-actions">
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
      analysisError: null,
    }
  },
   computed: {
    productImages() {
      if (!this.product) return [];
      if (this.product.type === 'combo' && this.product.products) {
        return this.product.products.map(p => p.image).flat();
      }
      if (this.product.images && this.product.images.length > 0) {
        return this.product.images;
      }
      if (this.product.image) {
        return [this.product.image];
      }
      return [];
    }
  },
  watch: {
    showModalTryOn(val) {
      if (val) {
        this.tryOnResult = null;
        this.analysisError = null;
        this.selectedProductImage = this.productImages[0] || null;
        this.cancelModelSelection();
        
        const savedResult = localStorage.getItem(`tryon_result_${this.product.id}`);
        if (savedResult) {
          this.tryOnResult = JSON.parse(savedResult);
        }
      }
    },
  },
  methods: {
    selectProductImage(image) {
        this.selectedProductImage = image;
    },
    showApiKeyHelp(keyType) {
      if (keyType === 'lightx') {
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
      }
    },
    selectPredefinedModel(model) {
      this.selectedTryOnModel = model;
      this.userModelFile = null;
      this.userModelPreviewUrl = '';
      if (document.getElementById('modelUploadInput')) {
        document.getElementById('modelUploadInput').value = '';
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
    cancelModelSelection() {
      this.userModelFile = null;
      this.userModelPreviewUrl = '';
      this.selectedTryOnModel = null;
       if (document.getElementById('modelUploadInput')) {
        document.getElementById('modelUploadInput').value = '';
      }
    },
    async confirmModelSelection() {
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
      this.analysisError = null;

      const modelInfo = this.selectedTryOnModel
        ? { name: this.selectedTryOnModel.name, url: this.selectedTryOnModel.url }
        : { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl };

      let tryOnImageUrl = '';

      try {
        const productForTryOn = { ...this.product, image: this.selectedProductImage };
        
        // Step 1: Generate the try-on image
        tryOnImageUrl = await LightXService.generateTryOnImage(
          this.apiKeys.lightxApiKey,
          modelInfo.url,
          [productForTryOn],
          this.userModelFile
        );

        // Show the image immediately
        this.tryOnResult = {
          model: modelInfo,
          image: tryOnImageUrl,
          score: 'Đang phân tích...',
          style: 'Đang phân tích...',
          gender_suitability: 'Đang phân tích...',
          products: [this.product],
          time: new Date().toISOString(),
        };

        // Step 2: Analyze the image
        try {
          const analysisResult = await LightXService.analyzeTryOnImage(tryOnImageUrl, [productForTryOn]);
          this.tryOnResult.score = analysisResult.aesthetic_score;
          this.tryOnResult.style = analysisResult.style;
          this.tryOnResult.gender_suitability = analysisResult.gender_suitability;

        } catch (analysisError) {
            console.error('Image analysis failed:', analysisError);
            this.analysisError = 'Không thể phân tích hình ảnh. Dịch vụ AI có thể đang gặp sự cố.';
            this.tryOnResult.score = 'N/A';
            this.tryOnResult.style = 'N/A';
            this.tryOnResult.gender_suitability = 'N/A';
        }

        localStorage.setItem(`tryon_result_${this.product.id}`, JSON.stringify(this.tryOnResult));

      } catch (error) {
        console.error('Error during try-on process:', error);
        Swal.fire({
          icon: 'error',
          title: 'Lỗi xử lý ảnh',
          text: error.message || 'Có lỗi xảy ra trong quá trình ghép ảnh. Vui lòng thử lại.',
        });
      } finally {
        this.loading = false;
      }
    },
    downloadTryOnResult() {
      if (!this.tryOnResult) return;
      const data = { ...this.tryOnResult, image: undefined };
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `tryon_result_${this.product.id}.json`;
      a.click();
      URL.revokeObjectURL(url);
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
  justify-content: center;
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
/* Other utility styles as needed */
</style>