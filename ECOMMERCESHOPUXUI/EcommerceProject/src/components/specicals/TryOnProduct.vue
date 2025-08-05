<template>
  <div>
    <!-- Nút thử đồ -->
    <button class="btn btn-primary try-on-btn" @click="showModalTryOn = true" title="Thử đồ với AI">
      <i class="bi bi-magic"></i> Thử đồ
    </button>

    <!-- Modal thử đồ -->
    <div v-show="showModalTryOn" class="modal-overlay" @click.self="showModalTryOn = false">
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
          <div class="tryon-main position-relative">
            <div v-if="loading" class="loading-overlay">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <span class="mt-2">Đang xử lý...</span>
            </div>

            <div class="content-grid">
              <!-- Sản phẩm -->
              <div class="content-section">
                <h4>Sản phẩm</h4>
                <div class="display-card">
                  <img
                    :src="product.image || (product.products && product.products[0]?.image)"
                    alt="Sản phẩm"
                    class="img-fluid"
                  />
                  <h5>{{ product.name || product.comboName }}</h5>
                  <p v-if="product.variant">{{ product.variant.color }} / {{ product.variant.size }}</p>
                </div>
              </div>

              <!-- Kết quả thử đồ -->
              <div class="content-section">
                <h4>Kết quả thử đồ</h4>
                <div v-if="tryOnResult" class="display-card">
                  <img :src="tryOnResult.image" alt="Kết quả thử đồ" class="img-fluid" />
                  <div class="result-info">
                    <div>
                      <b>Điểm thẩm mỹ:</b>
                      <span class="score">{{ tryOnResult.score }}/10</span>
                    </div>
                    <div>
                      <b>Phong cách:</b> {{ tryOnResult.style }}
                    </div>
                    <div>
                      <b>Giới tính:</b> {{ tryOnResult.gender_suitability }}
                    </div>
                    <button class="btn btn-success mt-2" @click="downloadTryOnResult">
                      <i class="bi bi-download"></i> Tải kết quả
                    </button>
                  </div>
                </div>
                <div v-else class="placeholder">
                  <i class="bi bi-image"></i>
                  <span>Chưa có kết quả thử đồ</span>
                </div>
              </div>
            </div>

            <!-- Chọn người mẫu -->
            <div class="model-selection-section">
              <h4>Chọn người mẫu</h4>
              <div class="model-list">
                <div
                  v-for="(model, index) in models"
                  :key="index"
                  class="model-item"
                  :class="{ selected: selectedTryOnModel === model }"
                  @click="selectPredefinedModel(model)"
                >
                  <img :src="model.url" :alt="model.name" class="model-thumbnail" />
                  <span>{{ model.name }}</span>
                </div>
                <div v-if="userModelPreviewUrl" class="model-item user-model-preview selected">
                  <img :src="userModelPreviewUrl" alt="Mẫu của bạn" class="model-thumbnail" />
                  <span>Mẫu của bạn</span>
                </div>
              </div>
              <div class="model-upload-area">
                <input
                  type="file"
                  @change="handleModelUpload"
                  accept="image/*"
                  id="modelUploadInput"
                  style="display: none"
                />
                <label for="modelUploadInput" class="btn btn-outline-info">
                  <i class="bi bi-upload"></i> Tải ảnh người mẫu
                </label>
              </div>
              <button
                class="btn btn-primary try-on-action"
                @click="confirmModelSelection"
                :disabled="!userModelFile && !selectedTryOnModel || loading"
              >
                <i class="bi bi-magic"></i> Thử đồ ngay
              </button>
            </div>
          </div>
        </div>

        <!-- Modal cài đặt API -->
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
import * as axiosConfig from '@/utils/axiosClient'

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
  watch: {
    showModalTryOn(val) {
      if (val) {
        const savedResult = localStorage.getItem(`tryon_result_${this.product.id}`);
        if (savedResult) {
          this.tryOnResult = JSON.parse(savedResult);
        }
      } else {
        this.tryOnResult = null;
        this.cancelModelSelection();
      }
    },
  },
  methods: {
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
      document.getElementById('modelUploadInput').value = '';
    },
    handleModelUpload(event) {
      const file = event.target.files[0];
      if (!file) return;

      if (!file.type.startsWith('image/')) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng chọn file hình ảnh.',
        });
        return;
      }

      const maxSize = 5 * 1024 * 1024;
      if (file.size > maxSize) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Kích thước file không được vượt quá 5MB.',
        });
        return;
      }
      this.userModelFile = file;
      const reader = new FileReader();
      reader.onload = (e) => {
        this.userModelPreviewUrl = e.target.result;
      };
      reader.readAsDataURL(file);
      this.selectedTryOnModel = null;
    },
    cancelApiSettings() {
      this.showApiSettings = false;
    },
    saveApiSettings() {
      localStorage.setItem('lightxApiKey', this.apiKeys.lightxApiKey);
      this.showApiSettings = false;
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'API Key đã được lưu.',
        timer: 1500,
        showConfirmButton: false,
      });
    },
    cancelModelSelection() {
      this.userModelFile = null;
      this.userModelPreviewUrl = '';
      this.selectedTryOnModel = null;
      document.getElementById('modelUploadInput').value = '';
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
        }).then((result) => {
          if (result.isConfirmed) {
            this.showApiSettings = true;
          }
        });
        return;
      }

      if (!this.selectedTryOnModel && !this.userModelFile) {
        Swal.fire({
          icon: 'warning',
          title: 'Chưa chọn mẫu',
          text: 'Vui lòng chọn một mẫu có sẵn hoặc tải lên ảnh của bạn.',
        });
        return;
      }

      this.loading = true;

      const modelInfo = this.selectedTryOnModel
        ? { name: this.selectedTryOnModel.name, url: this.selectedTryOnModel.url }
        : { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl };

      const lightXResultUrlKey = `lightx_result_url_${this.product.id}`;

      try {
        let modelImageUrl = modelInfo.url;
        if (this.userModelFile) {
          const formData = new FormData();
          formData.append('file', this.userModelFile);
          const response = await axiosConfig.postToApi('/TryOn/UploadImage', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          });
          if (!response.success) throw new Error('Không thể tải ảnh người mẫu của bạn lên máy chủ.');
          modelImageUrl = response.data.imageUrl;
        } else if (modelImageUrl.includes('localhost')) {
          const response = await axiosConfig.postToApi('/TryOn/UploadFromUrl', { imageUrl: modelImageUrl });
          if (!response.success) throw new Error('Không thể tải ảnh người mẫu có sẵn lên máy chủ.');
          modelImageUrl = response.data.imageUrl;
        }

        let productImageUrl = this.product.image || (this.product.products && this.product.products[0]?.image);
        if (!productImageUrl) {
          Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không có hình ảnh sản phẩm hợp lệ để xử lý.' });
          return;
        }

        if (productImageUrl.includes('localhost')) {
          const response = await axiosConfig.postToApi('/TryOn/UploadFromUrl', { imageUrl: productImageUrl });
          if (!response.success) throw new Error(`Không thể tải ảnh sản phẩm "${this.product.name || ''}" lên máy chủ.`);
          productImageUrl = response.data.imageUrl;
        }

        const tryOnImageResultUrl = await LightXService.processWithLightX(
          this.apiKeys.lightxApiKey,
          modelImageUrl,
          [this.product]
        );
        localStorage.setItem(lightXResultUrlKey, tryOnImageResultUrl);

        let analysisResult = {
          score: 'N/A',
          style: 'Không thể phân tích',
          gender_suitability: 'Không thể phân tích',
        };

        try {
          const geminiResponse = await axiosConfig.postToApi('/TryOn/AnalyzeImage', {
            resultImageUrl: tryOnImageResultUrl,
            productsData: [this.product],
          });

          if (geminiResponse.success) {
            const result = geminiResponse.data;
            analysisResult = {
              score: result.aesthetic_score,
              style: result.style,
              gender_suitability: result.gender_suitability,
            };
          } else {
            throw new Error(geminiResponse.message || 'Phân tích hình ảnh thất bại.');
          }
        } catch (error) {
          console.error('Gemini analysis failed:', error);
          let errorMessage = 'Không thể phân tích hình ảnh do lỗi không xác định.';
          const errorText = error.message || '';
          if (errorText.includes('429') || errorText.includes('RESOURCE_EXHAUSTED')) {
            errorMessage = 'Lỗi phân tích: Hạn ngạch API miễn phí đã hết. Vui lòng thử lại sau hoặc nâng cấp gói dịch vụ.';
          }
          Swal.fire({
            icon: 'warning',
            title: 'Phân tích thất bại',
            text: errorMessage,
          });
        }

        this.tryOnResult = {
          model: modelInfo,
          image: tryOnImageResultUrl,
          ...analysisResult,
          products: [this.product],
          time: new Date().toISOString(),
        };
        localStorage.setItem(`tryon_result_${this.product.id}`, JSON.stringify(this.tryOnResult));
        localStorage.removeItem(lightXResultUrlKey);
      } catch (error) {
        console.error('Error during try-on process:', error);
        Swal.fire({
          icon: 'error',
          title: 'Lỗi xử lý',
          text: error.message || 'Có lỗi xảy ra trong quá trình xử lý. Vui lòng thử lại.',
        });
      } finally {
        this.loading = false;
        this.cancelModelSelection();
      }
    },
    downloadTryOnResult() {
      if (!this.tryOnResult) return;
      const data = {
        ...this.tryOnResult,
        image: undefined,
      };
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
/* General styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.3s ease-out;
}

.tryon-modal {
  background: #ffffff;
  border-radius: 12px;
  width: 90vw;
  max-width: 900px;
  min-height: 600px;
  max-height: 95vh;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* Header */
.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2a44;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.icon-btn {
  background: none;
  border: none;
  color: #6b7280;
  font-size: 1.2rem;
  transition: color 0.2s ease;
}

.icon-btn:hover {
  color: #1f2a44;
}

.close-btn {
  background: none;
  border: none;
  color: #6b7280;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.2s ease;
}

.close-btn:hover {
  color: #dc2626;
}

/* Modal Body */
.modal-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

.tryon-main {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.content-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.content-section h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2a44;
  margin-bottom: 8px;
}

/* Display Cards */
.display-card {
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  padding: 16px;
  width: 100%;
  max-width: 300px;
  text-align: center;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.display-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
}

.display-card img {
  max-width: 100%;
  max-height: 200px;
  object-fit: contain;
  border-radius: 8px;
  margin-bottom: 12px;
}

.display-card h5 {
  font-size: 1.1rem;
  color: #1f2a44;
  margin-bottom: 8px;
}

.display-card p {
  font-size: 0.9rem;
  color: #6b7280;
  margin-bottom: 8px;
}

.result-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 0.9rem;
  color: #374151;
}

.result-info .score {
  color: #d97706;
  font-weight: 600;
}

/* Placeholder */
.placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #9ca3af;
  font-size: 0.9rem;
}

.placeholder i {
  font-size: 2rem;
  margin-bottom: 8px;
}

/* Model Selection */
.model-selection-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 16px 0;
}

.model-selection-section h4 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2a44;
}

.model-list {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
  justify-content: center;
}

.model-item {
  cursor: pointer;
  padding: 8px;
  border: 2px solid #e5e7eb;
  border-radius: 10px;
  transition: all 0.3s ease;
  background: #ffffff;
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 140px;
}

.model-item:hover {
  border-color: #3b82f6;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.2);
}

.model-item.selected {
  border-color: #2563eb;
  background: #eff6ff;
  box-shadow: 0 0 12px rgba(37, 99, 235, 0.3);
}

.user-model-preview {
  border-color: #2563eb;
}

.model-thumbnail {
  width: 100px;
  height: 150px;
  object-fit: cover;
  border-radius: 8px;
  margin-bottom: 8px;
}

.model-upload-area {
  margin: 16px 0;
}

.btn-outline-info {
  border: 1px solid #22d3ee;
  color: #22d3ee;
  transition: all 0.3s ease;
}

.btn-outline-info:hover {
  background: #22d3ee;
  color: #ffffff;
}

/* Buttons */
.btn {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.btn-primary {
  background: #2563eb;
  color: #ffffff;
  border: none;
}

.btn-primary:hover {
  background: #1d4ed8;
  transform: translateY(-2px);
}

.btn-success {
  background: #16a34a;
  color: #ffffff;
  border: none;
}

.btn-success:hover {
  background: #15803d;
  transform: translateY(-2px);
}

.btn-secondary {
  background: #6b7280;
  color: #ffffff;
  border: none;
}

.btn-secondary:hover {
  background: #4b5563;
  transform: translateY(-2px);
}

.try-on-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.try-on-action {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  font-size: 1rem;
  font-weight: 500;
}

/* Loading Overlay */
.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.85);
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #1f2a44;
}

/* API Settings Modal */
.api-settings-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
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
  animation: fadeIn 0.3s ease-out;
}

.api-settings-modal h3 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1f2a44;
  margin-bottom: 16px;
}

.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
  color: #374151;
  margin-bottom: 8px;
}

.form-group i {
  cursor: pointer;
  color: #6b7280;
}

.form-group i:hover {
  color: #2563eb;
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  font-size: 0.9rem;
}

.form-control:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
  outline: none;
}

.api-settings-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 16px;
}

/* Responsive Design */
@media (max-width: 768px) {
  .tryon-modal {
    width: 95vw;
    min-height: unset;
  }
  .content-grid {
    grid-template-columns: 1fr;
  }
  .display-card {
    max-width: 100%;
  }
  .model-list {
    gap: 12px;
  }
  .model-item {
    width: 120px;
  }
  .model-thumbnail {
    width: 80px;
    height: 120px;
  }
}

@media (max-width: 480px) {
  .modal-header {
    padding: 12px 16px;
  }
  .modal-header h2 {
    font-size: 1.25rem;
  }
  .modal-body {
    padding: 16px;
  }
  .content-section h4 {
    font-size: 1.1rem;
  }
  .display-card img {
    max-height: 150px;
  }
}
</style>