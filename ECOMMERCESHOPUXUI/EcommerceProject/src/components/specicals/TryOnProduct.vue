<template>
  <div>
    <!-- Nút thử đồ -->
    <button class="btn btn-primary" @click="showModalTryOn = true" title="Thử đồ với AI">
      <i class="bi bi-magic"></i> Thử đồ
    </button>

    <!-- Modal thử đồ -->
    <div v-show="showModalTryOn" class="modal-overlay" @click.self="showModalTryOn = false">
      <div class="tryon-modal">
        <div class="modal-header">
          <div class="header-center">
            <h2>Thử đồ với AI</h2>
          </div>
          <div class="header-right">
            <button class="btn" @click="showApiSettings = true; showModelSelection = false;" title="Cài đặt API Keys">
              <i class="bi bi-gear"></i>
            </button>
            <button class="close-btn" @click="showModalTryOn = false">×</button>
          </div>
        </div>
        <div class="modal-body overflow-auto">
          <div class="tryon-main position-relative">
            <div
              v-if="loading"
              class="loading-overlay"
            >
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <span class="mt-2">AI đang xử lý...</span>
            </div>
            
            <div class="row align-items-stretch" style="min-height: 400px">
              <div class="col-6 d-flex flex-column align-items-center justify-content-center">
                <h4>Sản phẩm của bạn</h4>
                <div class="product-display-card">
                  <img :src="product.image || (product.products && product.products[0]?.image)" alt="Sản phẩm" class="img-fluid" />
                  <h5>{{ product.name || product.comboName }}</h5>
                  <p v-if="product.variant">{{ product.variant.color }} / {{ product.variant.size }}</p>
                </div>
              </div>
              <div class="col-6 d-flex flex-column align-items-center justify-content-center">
                <h4>Kết quả thử đồ</h4>
                <div v-if="tryOnResult" class="result-display-card">
                  <img :src="tryOnResult.image" alt="Kết quả thử đồ" class="img-fluid" />
                  <div class="mt-2">
                    <b>Điểm thẩm mỹ:</b>
                    <span style="font-size: 1.3rem; color: #e67e22">
                      {{ tryOnResult.score }}/10
                    </span>
                  </div>
                  <div class="mt-2">
                    <b>Phong cách:</b>
                    <span> {{ tryOnResult.style }}</span>
                  </div>
                  <div class="mt-2">
                    <b>Giới tính phù hợp:</b>
                    <span> {{ tryOnResult.gender_suitability }}</span>
                  </div>
                  <button class="btn btn-success mt-2" @click="downloadTryOnResult">
                    Tải về kết quả thử đồ
                  </button>
                </div>
                <div v-else class="text-center text-secondary py-5">
                  <i class="bi bi-image" style="font-size: 2rem"></i><br />
                  <span>Chưa có hình ảnh thử đồ.</span>
                </div>
              </div>
            </div>
            <div class="mt-3 text-center">
              <button
                class="btn btn-primary"
                @click="showModelSelection = true"
                :disabled="loading"
              >
                Chọn người mẫu & Thử đồ
              </button>
            </div>
          </div>
        </div>
        <div v-if="showModelSelection" class="model-selection-overlay">
          <div class="model-selection-modal">
            <h4 class="mb-5">Chọn hoặc tải lên ảnh người mẫu</h4>
            <!-- Lựa chọn mẫu có sẵn -->
            <div class="model-list mb-4">
              <!-- Mẫu 1 -->
              <div
                class="model-item"
                :class="{ selected: selectedTryOnModel === models[0] }"
                @click="selectPredefinedModel(models[0])"
              >
                <img :src="models[0].url" :alt="models[0].name" class="model-thumbnail" />
                <span>{{ models[0].name }}</span>
              </div>

              <!-- Ảnh người dùng tải lên -->
              <div v-if="userModelPreviewUrl" class="model-item user-model-preview selected">
                <img :src="userModelPreviewUrl" alt="Mẫu của bạn" class="model-thumbnail" />
                <span>Mẫu của bạn</span>
              </div>

              <!-- Mẫu 2 -->
              <div
                class="model-item"
                :class="{ selected: selectedTryOnModel === models[1] }"
                @click="selectPredefinedModel(models[1])"
              >
                <img :src="models[1].url" :alt="models[1].name" class="model-thumbnail" />
                <span>{{ models[1].name }}</span>
              </div>
            </div>

            <!-- Lựa chọn tải ảnh lên -->
            <div class="model-upload-area">
              <input
                type="file"
                @change="handleModelUpload"
                accept="image/*"
                id="modelUploadInput"
                style="display: none"
              />
              <label for="modelUploadInput" class="btn btn-info m-1"
                >Tải ảnh của bạn (dưới 5MB)</label
              >
            </div>
            <hr />
            <div class="model-selection-actions d-flex flex-row justify-content-between">
              <button class="btn btn-secondary" @click="cancelModelSelection">Hủy</button>
              <button
                class="btn btn-primary"
                @click="confirmModelSelection"
                :disabled="!userModelFile && !selectedTryOnModel"
              >
                Xác nhận
              </button>
            </div>
          </div>
        </div>
        <div v-if="showApiSettings" class="api-settings-overlay">
          <div class="api-settings-modal">
            <h3>Cài đặt API Key</h3>
            <div class="form-group">
              <label for="lightxApiKey" class="d-flex align-items-center">
                LightX API Key
                <i
                  class="bi bi-info-circle ms-2"
                  style="cursor: pointer"
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
    showModal(val) {
      if (val) {
        // Load saved LightX result if any
        const savedResult = localStorage.getItem(`tryon_result_${this.product.id}`);
        if (savedResult) {
          this.tryOnResult = JSON.parse(savedResult);
        }
      } else {
        this.tryOnResult = null; // Clear result when modal closes
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
          confirmButtonText: 'Đã hiểu'
        });
      }
    },
    selectPredefinedModel(model) {
      this.selectedTryOnModel = model
      this.userModelFile = null
      this.userModelPreviewUrl = ''
      document.getElementById('modelUploadInput').value = ''
    },
    handleModelUpload(event) {
      const file = event.target.files[0]
      if (!file) return

      if (!file.type.startsWith('image/')) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng chỉ chọn file hình ảnh.',
        })
        return
      }

      const maxSize = 5 * 1024 * 1024 // 5MB in bytes
      if (file.size > maxSize) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Kích thước file không được vượt quá 5MB.',
        })
        return
      }
      this.userModelFile = file
      const reader = new FileReader()
      reader.onload = (e) => {
        this.userModelPreviewUrl = e.target.result
      }
      reader.readAsDataURL(file)
      this.selectedTryOnModel = null
    },
    cancelApiSettings() {
      this.showApiSettings = false
      this.showModelSelection = false; // Ensure model selection modal is closed
    },
    saveApiSettings() {
      localStorage.setItem('lightxApiKey', this.apiKeys.lightxApiKey)
      this.showApiSettings = false
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'API Key đã được lưu.',
        timer: 1500,
        showConfirmButton: false,
      })
    },
    cancelModelSelection() {
      this.showModelSelection = false
      this.showApiSettings = false; // Ensure API settings modal is closed
      this.userModelFile = null
      this.userModelPreviewUrl = ''
      this.selectedTryOnModel = null
    },
    async tryOnProduct() {
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
            this.showApiSettings = true
          }
        })
        return
      }

      const savedLightXUrl = localStorage.getItem(`lightx_result_url_${this.product.id}`)
      if (savedLightXUrl) {
        Swal.fire({
          title: 'Phân tích chưa hoàn tất',
          text: 'Đã tìm thấy kết quả thử đồ chưa được phân tích. Bạn có muốn tiếp tục phân tích ảnh này không?',
          icon: 'info',
          showCancelButton: true,
          confirmButtonText: 'Tiếp tục phân tích',
          cancelButtonText: 'Bắt đầu lại',
        }).then((result) => {
          if (result.isConfirmed) {
            this.resumeAnalysis(savedLightXUrl)
          } else {
            localStorage.removeItem(`lightx_result_url_${this.product.id}`)
            this.showModelSelection = true
          }
        })
      } else {
        this.showModelSelection = true
        this.showApiSettings = false; // Ensure API settings modal is closed
      }
    },

    async resumeAnalysis(imageUrl) {
      this.loading = true
      let analysisResult = {
          score: 'N/A',
          style: 'Không thể phân tích',
          gender_suitability: 'Không thể phân tích'
      };

      try {
        const geminiResponse = await axiosConfig.postToApi('/TryOn/AnalyzeImage', {
          resultImageUrl: imageUrl,
          productsData: [this.product], // Send single product as array
        })

        if (geminiResponse.success) {
            const result = geminiResponse.data;
            analysisResult = {
                score: result.aesthetic_score,
                style: result.style,
                gender_suitability: result.gender_suitability,
            };
        } else {
            throw new Error(geminiResponse.message || 'Phân tích lại hình ảnh thất bại.');
        }
      } catch (error) {
        console.error('Error during resumed analysis:', error)
        let errorMessage = 'Không thể hoàn tất phân tích. Vui lòng thử lại.';
        const errorText = error.message || '';
        if (errorText.includes('429') || errorText.includes('RESOURCE_EXHAUSTED')) {
            errorMessage = 'Lỗi phân tích: Bạn đã sử dụng hết hạn ngạch API miễn phí cho hôm nay. Vui lòng thử lại sau hoặc nâng cấp gói dịch vụ.';
        }
        Swal.fire({
          icon: 'warning',
          title: 'Phân tích thất bại',
          text: errorMessage,
        })
      } finally {
        this.tryOnResult = {
          model: this.selectedTryOnModel || { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl },
          image: imageUrl,
          ...analysisResult,
          products: [this.product],
          time: new Date().toISOString(),
        }
        localStorage.setItem(`tryon_result_${this.product.id}`, JSON.stringify(this.tryOnResult));
        localStorage.removeItem(`lightx_result_url_${this.product.id}`)
        this.loading = false
      }
    },

    async confirmModelSelection() {
      if (!this.selectedTryOnModel && !this.userModelFile) {
        Swal.fire({
          icon: 'warning',
          title: 'Chưa chọn mẫu',
          text: 'Vui lòng chọn một mẫu có sẵn hoặc tải lên ảnh của bạn.',
        })
        return
      }

      this.loading = true
      this.showModelSelection = false

      const modelInfo = this.selectedTryOnModel
        ? { name: this.selectedTryOnModel.name, url: this.selectedTryOnModel.url }
        : { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl }

      const lightXResultUrlKey = `lightx_result_url_${this.product.id}`

      try {
        // Step 1: Get public URLs for all images
        let modelImageUrl = modelInfo.url;
        if (this.userModelFile) {
            const formData = new FormData();
            formData.append('file', this.userModelFile);
            const response = await axiosConfig.postToApi('/TryOn/UploadImage', formData, { headers: { 'Content-Type': 'multipart/form-data' } });
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

        // Step 2: Process with LightX using the service
        const tryOnImageResultUrl = await LightXService.processWithLightX(
          this.apiKeys.lightxApiKey,
          modelImageUrl,
          [this.product] // Send single product as array
        );
        localStorage.setItem(lightXResultUrlKey, tryOnImageResultUrl);

        // Step 3: Send to Gemini for analysis
        let analysisResult = {
            score: 'N/A',
            style: 'Không thể phân tích',
            gender_suitability: 'Không thể phân tích'
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
                errorMessage = 'Lỗi phân tích: Bạn đã sử dụng hết hạn ngạch API miễn phí cho hôm nay. Vui lòng thử lại sau hoặc nâng cấp gói dịch vụ.';
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
        console.error('Error during try-on process:', error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi xử lý',
          text: error.message || 'Có lỗi xảy ra trong quá trình xử lý. Kết quả trung gian đã được lưu, bạn có thể thử lại.',
        })
      } finally {
        this.loading = false
        this.cancelModelSelection()
      }
    },
    downloadTryOnResult() {
      if (!this.tryOnResult) return
      const data = {
        ...this.tryOnResult,
        image: undefined,
      }
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `tryon_result_${this.product.id}.json`
      a.click()
      URL.revokeObjectURL(url)
    },
  },
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.35);
  z-index: 1002;
  display: flex;
  align-items: center;
  justify-content: center;
}
.tryon-modal {
  background: #fff;
  border-radius: 16px;
  width: 95vw; /* Tăng chiều rộng trên màn hình lớn */
  max-width: 1000px; /* Tăng max-width */
  min-height: 550px; /* Tăng min-height */
  max-height: 95vh; /* Tăng max-height */
  box-shadow: 0 8px 40px rgba(0, 0, 0, 0.2); /* Shadow mạnh hơn */
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden; /* Đảm bảo nội dung không tràn ra ngoài */
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 15px 25px; /* Điều chỉnh padding */
  border-bottom: 1px solid #e0e0e0;
  background-color: #f8f8f8; /* Màu nền cho header */
}

.header-center {
  flex: 2;
  text-align: center;
}

.header-center h2 {
  margin: 0;
  font-size: 1.8rem; /* Kích thước tiêu đề */
  color: #333;
}

.header-right {
  justify-content: flex-end;
}

.close-btn {
  font-size: 2.2rem; /* Kích thước nút đóng */
  background: none;
  border: none;
  color: #888;
  cursor: pointer;
  transition: color 0.2s ease;
}

.close-btn:hover {
  color: #333;
}

.modal-body {
  display: flex;
  flex: 1;
  padding: 20px; /* Điều chỉnh padding */
  gap: 20px; /* Điều chỉnh khoảng cách giữa các cột */
  min-height: 0;
  max-height: calc(95vh - 70px); /* Tính toán lại max-height */
  overflow-y: auto; /* Chỉ cuộn theo chiều dọc */
  overflow-x: hidden; /* Ẩn thanh cuộn ngang */
}

.tryon-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .tryon-modal {
    width: 98vw;
    max-width: 98vw;
    min-height: unset;
    max-height: 98vh;
  }
  .modal-body {
    flex-direction: column; /* Stack columns on smaller screens */
    padding: 15px;
    gap: 15px;
    max-height: calc(98vh - 60px);
  }
  .header-center h2 {
    font-size: 1.5rem;
  }
  .close-btn {
    font-size: 1.8rem;
  }
}

@media (max-width: 480px) {
  .modal-header {
    padding: 10px 15px;
  }
  .header-center h2 {
    font-size: 1.3rem;
  }
  .close-btn {
    font-size: 1.5rem;
  }
  .modal-body {
    padding: 10px;
    gap: 10px;
  }
}


.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.product-display-card, .result-display-card {
  background: #f8f9fa; /* Light background */
  border: 1px solid #e0e0e0;
  border-radius: 12px; /* More rounded corners */
  padding: 20px; /* Increased padding */
  text-align: center;
  margin-bottom: 20px;
  width: 100%;
  max-width: 350px; /* Slightly wider cards */
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08); /* Subtle shadow */
  transition: all 0.3s ease;
}

.product-display-card:hover, .result-display-card:hover {
  transform: translateY(-5px); /* Lift effect on hover */
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.12);
}

.product-display-card img, .result-display-card img {
  max-width: 100%;
  height: auto;
  max-height: 250px; /* Max height for images */
  object-fit: contain; /* Ensure image fits */
  border-radius: 8px; /* Rounded image corners */
  margin-bottom: 15px; /* Increased margin */
  border: 1px solid #f0f0f0;
}

.product-display-card h4, .result-display-card h4 {
  font-size: 1.4rem;
  color: #333;
  margin-bottom: 10px;
}

.product-display-card h5 {
  font-size: 1.1rem;
  color: #555;
  margin-bottom: 5px;
}

.product-display-card p {
  font-size: 0.9rem;
  color: #777;
}

.result-display-card .mt-2 {
  margin-top: 10px; /* Adjust margin for result details */
}

.result-display-card b {
  color: #444;
}

.result-display-card span {
  font-weight: 600;
}

/* Responsive adjustments for cards */
@media (max-width: 768px) {
  .product-display-card, .result-display-card {
    max-width: 90%; /* Adjust width on smaller screens */
    padding: 15px;
  }
  .product-display-card img, .result-display-card img {
    max-height: 200px;
  }
}

@media (max-width: 480px) {
  .product-display-card, .result-display-card {
    max-width: 100%;
    margin-bottom: 15px;
  }
  .product-display-card h4, .result-display-card h4 {
    font-size: 1.2rem;
  }
  .product-display-card h5 {
    font-size: 1rem;
  }
}

.model-selection-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6); /* Darker overlay */
  z-index: 1003;
  display: flex;
  align-items: center;
  justify-content: center;
}

.model-selection-modal {
  background: #fff;
  border-radius: 16px; /* More rounded corners */
  padding: 30px; /* Increased padding */
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25); /* Stronger shadow */
  width: 90%;
  max-width: 550px; /* Slightly wider modal */
  text-align: center;
  animation: fadeInScale 0.3s ease-out; /* Add animation */
}

@keyframes fadeInScale {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.model-selection-modal h4 {
  margin-bottom: 25px; /* Adjusted margin */
  color: #333;
  font-size: 1.6rem;
}

.model-list {
  display: flex;
  justify-content: center;
  gap: 25px; /* Increased gap */
  flex-wrap: wrap;
  margin-bottom: 30px; /* Increased margin */
}

.model-item {
  cursor: pointer;
  padding: 12px; /* Increased padding */
  border: 2px solid #e0e0e0;
  border-radius: 12px; /* More rounded corners */
  transition: all 0.3s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  background-color: #fdfdfd;
}

.model-item:hover {
  border-color: #667eea; /* Highlight color */
  box-shadow: 0 6px 18px rgba(102, 126, 234, 0.2); /* Subtle hover shadow */
  transform: translateY(-3px); /* Slight lift on hover */
}

.model-item.selected {
  border-color: #1976d2;
  background-color: #e3f2fd;
  box-shadow: 0 0 15px rgba(25, 118, 210, 0.6); /* Stronger selected shadow */
  transform: scale(1.05); /* Slightly larger when selected */
}

.user-model-preview {
  border-width: 3px;
  border-color: #1976d2;
  box-shadow: 0 4px 15px rgba(25, 118, 210, 0.4);
}

.model-thumbnail {
  width: 120px; /* Larger thumbnails */
  height: 180px; /* Larger thumbnails */
  object-fit: cover;
  border-radius: 8px; /* More rounded image corners */
  margin-bottom: 10px;
}

.model-selection-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px; /* Increased gap */
  margin-top: 20px;
}

.model-selection-actions .btn-primary:hover {
  background-color: #1565c0;
  transform: translateY(-2px);
}

.model-selection-actions .btn-secondary {
  background-color: #9e9e9e;
  color: #fff;
  border: none;
}

.model-selection-actions .btn-secondary:hover {
  background-color: #757575;
  transform: translateY(-2px);
}

.api-settings-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  z-index: 1003;
  display: flex;
  align-items: center;
  justify-content: center;
}

.api-settings-modal {
  background: #fff;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
  width: 90%;
  max-width: 500px;
  text-align: center;
  animation: fadeInScale 0.3s ease-out;
}

.api-settings-modal h3 {
  margin-bottom: 25px;
  color: #333;
  font-size: 1.6rem;
}

.form-group {
  margin-bottom: 20px; /* Increased margin */
  text-align: left;
}

.form-group label {
  display: block;
  margin-bottom: 8px; /* Increased margin */
  font-weight: bold;
  color: #555;
}

.form-control {
  width: 100%;
  padding: 10px 15px; /* Increased padding */
  border: 1px solid #ccc;
  border-radius: 8px; /* More rounded corners */
  font-size: 1rem;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-control:focus {
  border-color: #1976d2;
  box-shadow: 0 0 0 3px rgba(25, 118, 210, 0.25);
  outline: none;
}

.api-settings-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 30px; /* Increased margin */
}

.api-settings-actions .btn {
  padding: 10px 25px;
  border-radius: 8px;
  font-size: 1.05rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.api-settings-actions .btn-primary {
  background-color: #1976d2;
  color: #fff;
  border: none;
}

.api-settings-actions .btn-primary:hover {
  background-color: #1565c0;
  transform: translateY(-2px);
}

.api-settings-actions .btn-secondary {
  background-color: #9e9e9e;
  color: #fff;
  border: none;
}

.api-settings-actions .btn-secondary:hover {
  background-color: #757575;
  transform: translateY(-2px);
}

.btn-primary:hover {
  background-color: #1565c0;
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(25, 118, 210, 0.3);
}

.btn-success {
  background-color: #28a745;
  color: #fff;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-success:hover {
  background-color: #218838;
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(40, 167, 69, 0.3);
}
</style>
