<template>
  <div>
    <!-- Nút so sánh sản phẩm cố định giữa dưới -->
    <button class="compare-btn-fixed" @click="showModal = true" title="So sánh sản phẩm">
      <i class="bi bi-arrow-left-right"></i>
    </button>

    <!-- Modal so sánh sản phẩm -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="compare-modal">
        <div class="modal-header">
          <div class="header-left">
            <button class="btn btn-light" @click="showSidebarModal = !showSidebarModal" title="Danh sách sản phẩm">
              <i :class="showSidebarModal ? 'bi bi-chevron-left' : 'bi bi-list-ul'"></i>
            </button>
          </div>
          <div class="header-center">
            <h2>So sánh sản phẩm</h2>
          </div>
          <div class="header-right">
            <button class="btn" @click="showApiSettings = true" title="Cài đặt API Keys">
              <i class="bi bi-gear"></i>
            </button>
            <button class="close-btn" @click="showModal = false">×</button>
          </div>
        </div>
        <div class="modal-body overflow-auto">
          <!-- Cột phải: danh sách sản phẩm đã chọn để so sánh -->
          <div v-if="showSidebarModal" class="compare-sidebar">
            <h5>Danh sách đã chọn</h5>
            <hr />
            <div class="sidebar-list">
              <template v-if="selectedProducts.length === 0">
                <div class="text-center py-5 w-100">
                  <i class="bi bi-search" style="font-size: 2.5rem; color: #1976d2"></i><br />
                  <div style="font-size: 1.2rem; color: #888; margin: 12px 0">
                    Chưa có sản phẩm nào trong danh sách so sánh.<br />
                    <router-link
                      to="/shop"
                      style="color: #1976d2; text-decoration: underline; font-weight: 500"
                      >Khám phá sản phẩm ngay!</router-link
                    >
                  </div>
                </div>
              </template>
              <template v-else>
                <div
                  v-for="(item, idx) in selectedProducts"
                  :key="item.id || idx"
                  :draggable="true"
                  @dragstart="onDragStart($event, item)"
                  class="sidebar-draggable"
                  @dblclick="onDoubleClickSidebar(item)"
                >
                  <div v-if="item.type === 'combo'" class="combo-card sidebar">
                    <div class="combo-header">
                      <div class="combo-title">{{ item.comboName }}</div>
                      <button class="delete-sidebar-btn" @click="removeFromSidebar(item)">×</button>
                    </div>
                    <div class="combo-products">
                      <div v-for="(prod, pidx) in item.products" :key="pidx">
                        <div class="product-card mini">
                          <img :src="prod.image" alt="" />
                          <div>{{ prod.name }}</div>
                          <div class="variant">
                            {{ prod.variant.color }} / {{ prod.variant.size }}
                          </div>
                          <div class="price">{{ formatCurrency(prod.variant.price) }}</div>
                        </div>
                        <div v-if="pidx < item.products.length - 1" class="combo-divider"></div>
                      </div>
                    </div>
                    <div class="mt-2 d-flex justify-content-between w-100 align-items-center">
                      <button class="btn btn-sm btn-primary" @click="addComboToCompare(item)">
                        <i class="bi bi-plus-circle"></i> Thêm vào so sánh
                      </button>
                      <RouterLink
                        :to="'/combo/' + item.id"
                        class="btn btn-sm btn-outline-info"
                        title="Xem chi tiết combo"
                      >
                        <i class="bi bi-box-arrow-up-right"></i>
                      </RouterLink>
                    </div>
                  </div>
                  <div v-else class="product-card sidebar">
                    <div class="row justify-content-evenly p-2">
                      <div
                        class="col-4 d-flex justify-content-center align-items-center border-end"
                      >
                        <img :src="item.image" alt="" class="img-fluid" />
                      </div>
                      <div class="col-8 d-flex flex-column align-items-start grap-2">
                        <div>{{ item.name }}</div>
                        <div class="variant">
                          Loại: {{ item.variant.color }} / {{ item.variant.size }}
                        </div>
                        <div class="price">Giá: {{ formatCurrency(item.variant.price) }}</div>
                      </div>
                      <div class="mt-2 d-flex justify-content-between w-100 align-items-center">
                        <button class="btn btn-sm btn-primary" @click="addToCompare(item)">
                          <i class="bi bi-plus-circle"></i> Thêm vào so sánh
                        </button>
                        <RouterLink
                          :to="'/product/' + item.id"
                          class="btn btn-sm btn-outline-info"
                          title="Xem chi tiết sản phẩm"
                        >
                          <i class="bi bi-box-arrow-up-right"></i>
                        </RouterLink>
                      </div>
                    </div>
                    <button class="delete-sidebar-btn" @click="removeFromSidebar(item)">×</button>
                  </div>
                </div>
              </template>
            </div>
          </div>
          <!-- Vùng so sánh chính -->
          <div class="compare-main position-relative">
            <div
              v-if="loadingGroup !== null"
              class="loading-overlay-group"
              :style="{ left: loadingGroup === 0 ? '0' : '50%' }"
            >
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <span class="mt-2">AI đang xử lý...</span>
            </div>
            <div class="row align-items-stretch" style="min-height: 400px">
              <div
                v-for="(group, groupIdx) in compareGroups"
                :key="groupIdx"
                class="col-6"
                :class="{ 'group-disabled': loadingGroup === groupIdx }"
                @dragover.prevent
                @drop="onDropToGroup($event, groupIdx)"
              >
                <div
                  class="border rounded p-3 min-vh-50 mb-3 bg-light d-flex flex-column align-items-center justify-content-center flex-grow-1 h-100 position-relative"
                  style="min-height: 300px; height: 100%"
                >
                  <button
                    class="btn btn-primary btn-sm position-absolute"
                    style="bottom: 8px; right: 8px; z-index: 2; border-radius: 50%; width: 40px; height: 40px; display: flex; align-items: center; justify-content: center;"
                    @click="tryOnModel(groupIdx)"
                    :disabled="loadingGroup === groupIdx"
                    title="Thử đồ trên người mẫu"
                  >
                    <i class="bi bi-magic"></i>
                  </button>
                  <!-- Nút lật mặt ở góc phải trên -->
                  <button
                    class="btn btn-outline-secondary btn-sm position-absolute"
                    style="top: 8px; right: 8px; z-index: 2"
                    :disabled="!tryOnResults[groupIdx] || loadingGroup === groupIdx"
                    @click="groupFlipped[groupIdx] = !groupFlipped[groupIdx]"
                  >
                    {{ groupFlipped[groupIdx] ? 'Xem sản phẩm' : 'Xem mẫu thử đồ' }}
                  </button>
                  <div
                    v-if="!groupFlipped[groupIdx]"
                    class="position-relative justify-content-between"
                  >
                    <div v-if="group.products.length === 0">
                      <div class="text-secondary text-center py-5">
                        <i class="bi bi-box-arrow-in-down" style="font-size: 2rem"></i><br />
                        <span>Kéo sản phẩm vào đây để tạo nhóm so sánh mới</span>
                      </div>
                    </div>
                    <template v-else>
                      <div class="row g-2">
                        <div
                          v-for="(item, idx) in group.products"
                          :key="idx"
                          class="draggable-item mb-2"
                          :class="[
                            item.type === 'combo' ? 'col-12' : 'col-6',
                            group.selectedProductIdx === idx ? 'selected-product' : '',
                          ]"
                          @click="selectProduct(groupIdx, idx)"
                        >
                          <div
                            v-if="item.type === 'combo'"
                            class="combo-card row mb-2"
                            :class="group.selectedProductIdx === idx ? 'selected' : ''"
                          >
                            <div
                              class="combo-title col-12 d-flex align-items-center justify-content-between"
                            >
                              <span>{{ item.comboName }}</span>
                              <button
                                class="btn btn-danger btn-sm mt-1"
                                @click.stop="removeFromCombo(groupIdx, idx)"
                              >
                                X
                              </button>
                            </div>
                            <div class="combo-products col-12">
                              <div
                                v-for="(prod, pidx) in item.products"
                                :key="pidx"
                                class="col-6 m-1"
                                :class="item.selectedComboProductIdx === pidx ? 'selected' : ''"
                                @click.stop="selectComboProduct(groupIdx, idx, pidx)"
                              >
                                <div class="">
                                  <div class="product-card mini">
                                    <img
                                      :src="prod.image"
                                      alt=""
                                      style="cursor: pointer"
                                      @click.stop="
                                        openLightboxGroupProduct(groupIdx, idx, pidx, true)
                                      "
                                    />
                                    <div>{{ prod.name }}</div>
                                    <div class="variant">
                                      <select
                                        v-if="prod.variants"
                                        v-model="prod.variantKey"
                                        @change="onChangeVariantCombo(groupIdx, idx, pidx)"
                                        class="form-select form-select-sm"
                                      >
                                        <option
                                          v-for="(v, vIdx) in prod.variants"
                                          :key="vIdx"
                                          :value="vIdx"
                                        >
                                          {{ v.color }} / {{ v.size }}
                                        </option>
                                      </select>
                                      <span v-else
                                        >{{ prod.variant.color }} / {{ prod.variant.size }}</span
                                      >
                                    </div>
                                    <div class="price">
                                      {{ formatCurrency(prod.variant.price) }}
                                    </div>
                                  </div>
                                  <div
                                    v-if="pidx < item.products.length - 1"
                                    class="combo-divider"
                                  ></div>
                                </div>
                              </div>
                            </div>
                          </div>
                          <div
                            v-else
                            class="product-card col-12"
                            :class="group.selectedProductIdx === idx ? 'selected' : ''"
                          >
                            <img
                              :src="item.image"
                              alt=""
                              style="cursor: pointer"
                              @click.stop="openLightboxGroupProduct(groupIdx, idx, null, false)"
                            />
                            <div>{{ item.name }}</div>
                            <div class="variant">
                              <select
                                v-if="item.variants"
                                v-model="item.variantKey"
                                @change="onChangeVariant(groupIdx, idx)"
                                class="form-select form-select-sm"
                              >
                                <option
                                  v-for="(v, vIdx) in item.variants"
                                  :key="vIdx"
                                  :value="vIdx"
                                >
                                  {{ v.color }} / {{ v.size }}
                                </option>
                              </select>
                              <span v-else>{{ item.variant.color }} / {{ item.variant.size }}</span>
                            </div>
                            <div class="price">{{ formatCurrency(item.variant.price) }}</div>
                            <button
                              class="btn btn-danger btn-sm mt-1"
                              @click.stop="removeFromCompare(groupIdx, idx)"
                            >
                              Xóa
                            </button>
                          </div>
                        </div>
                      </div>
                    </template>

                  </div>
                  <!-- Mặt sau: hình ảnh thử đồ nếu có -->
                  <template v-else>
                    <div v-if="tryOnResults[groupIdx]">
                      <div class="text-center">
                        <b>Ảnh người mẫu đã ghép:</b><br />
                        <div class="try-on-image-container">
                          <img
                            :src="tryOnResults[groupIdx].image"
                            style="max-width: 220px; border-radius: 8px; border: 1px solid #ccc"
                          />
                          <button class="download-btn" @click="downloadTryOnResult(groupIdx)" title="Tải ảnh về">
                            <i class="bi bi-download"></i>
                          </button>
                        </div>
                        <div v-if="tryOnResults[groupIdx].score != null" class="mt-2">
                          <b>Điểm thẩm mỹ:</b>
                          <span style="font-size: 1.3rem; color: #e67e22"
                            > {{ tryOnResults[groupIdx].score }}/10</span
                          >
                        </div>
                        <div v-if="tryOnResults[groupIdx].style" class="mt-2">
                          <b>Phong cách:</b>
                          <span> {{ tryOnResults[groupIdx].style }}</span>
                        </div>
                        <div v-if="tryOnResults[groupIdx].gender_suitability" class="mt-2">
                          <b>Giới tính phù hợp:</b>
                          <span> {{ tryOnResults[groupIdx].gender_suitability }}</span>
                        </div>
                        <div v-if="!tryOnResults[groupIdx].score && !tryOnResults[groupIdx].style" class="mt-2 text-muted">
                          <small>AI không thể phân tích ảnh này.</small>
                        </div>
                      </div>
                    </div>
                    <div v-else class="text-center text-secondary py-5">
                      <i class="bi bi-image" style="font-size: 2rem"></i><br />
                      <span>Chưa có hình ảnh thử đồ cho nhóm này.</span>
                    </div>
                  </template>
                </div>
                <div class="">
                  <div class="compare-sum">
                    Giá tổng: <b>{{ formatCurrency(getGroupTotal(group)) }}</b>
                  </div>
                  <div class="compare-info-section">
                    <div v-if="group.selectedProductIdx != null">
                      <template v-if="group.products[group.selectedProductIdx] && group.products[group.selectedProductIdx].type === 'combo' && group.products[group.selectedProductIdx].selectedComboProductIdx != null">
                        <!-- Focus vào sản phẩm trong combo -->
                        <h5 class="info-title">Mô tả:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.products?.[group.products[group.selectedProductIdx]?.selectedComboProductIdx]?.description || 'Không có mô tả' }}</p>
                        
                        <h5 class="info-title">Đánh giá:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.products?.[group.products[group.selectedProductIdx]?.selectedComboProductIdx]?.rating || 'Chưa có đánh giá' }} ★</p>
                        
                        <h5 class="info-title">Thông tin:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.products?.[group.products[group.selectedProductIdx]?.selectedComboProductIdx]?.info || 'Không có thông tin' }}</p>
                      </template>
                      <template v-else>
                        <!-- Focus vào sản phẩm lẻ hoặc combo -->
                        <h5 class="info-title">Mô tả:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.description || 'Không có mô tả' }}</p>
                        
                        <h5 class="info-title">Đánh giá:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.rating || 'Chưa có đánh giá' }} ★</p>
                        
                        <h5 class="info-title">Thông tin:</h5>
                        <p>{{ group.products[group.selectedProductIdx]?.info || 'Không có thông tin' }}</p>
                      </template>
                    </div>
                    <div v-else>
                      <span style="color: #888">Bấm vào sản phẩm để xem chi tiết</span>
                    </div>
                  </div>
                </div>
              </div>
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
  <VueEasyLight
    :visible="isLightboxOpen"
    :imgs="lightboxImages"
    :index="lightboxIndex"
    @hide="closeLightbox"
  />
</template>


<script>
import { formatCurrency } from '@/constants/formatCurrency'
import VueEasyLight from 'vue-easy-lightbox'
import CompareStorageHelper from '@/models/dtos/expansionModels/compareObject'
import Swal from 'sweetalert2'
import LightXService from '@/services/LightXService' // Import LightXService


export default {
  name: 'CompareProduct',
  components: { VueEasyLight },
  data() {
    return {
      showModal: false,
      showSidebarModal: true,
      selectedProducts: [],
      showApiSettings: false,
      loadingGroup: null, // Index of the group currently being processed by AI
      apiKeys: {
        lightxApiKey: localStorage.getItem('lightxApiKey') || '',
      },
      compareGroups: [
        { products: [], selectedProductIdx: null },
        { products: [], selectedProductIdx: null },
      ],
      groupFlipped: [false, false],
      dragItem: null,
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
      tryOnResults: {},
      showModelSelection: false,
      currentTryOnGroupIdx: null,
      // Restore predefined models and selection state
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
      // User-uploaded model state
      userModelFile: null,
      userModelPreviewUrl: '',
    }
  },
  mounted() {
    this.loadSelectedProducts()
  },
  watch: {
    showModal(val) {
      if (val) this.loadSelectedProducts()
    },
  },
  methods: {
    formatCurrency,
    loadSelectedProducts() {
      this.selectedProducts = CompareStorageHelper.getCompareList()
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
          confirmButtonText: 'Đã hiểu'
        });
      }
    },
    selectPredefinedModel(model) {
      this.selectedTryOnModel = model
      // Clear user upload selection
      this.userModelFile = null
      this.userModelPreviewUrl = ''
      document.getElementById('modelUploadInput').value = '' // Reset file input
    },
    handleModelUpload(event) {
      const file = event.target.files[0]
      if (!file) return

      // Validation: Check if it's an image
      if (!file.type.startsWith('image/')) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Vui lòng chỉ chọn file hình ảnh.',
        })
        return
      }

      // Validation: Check file size (5MB limit)
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
      // Clear predefined model selection
      this.selectedTryOnModel = null
    },
    openLightboxGroupProduct(groupIdx, prodIdx, comboIdx, isComboChild) {
      // Lấy danh sách ảnh của group
      const group = this.compareGroups[groupIdx]
      let images = []
      let index = 0
      if (isComboChild) {
        // Combo: gom tất cả ảnh sản phẩm con
        images = group.products[prodIdx].products.map((p) => p.image)
        index = comboIdx
      } else {
        // Sản phẩm lẻ: gom tất cả ảnh sản phẩm lẻ trong group
        images = group.products.filter((p) => p.type !== 'combo').map((p) => p.image)
        // Tìm index của item hiện tại trong mảng sản phẩm lẻ
        const lIndex = group.products
          .filter((p) => p.type !== 'combo')
          .findIndex((p, i) => i === prodIdx)
        index = lIndex >= 0 ? lIndex : 0
      }
      this.lightboxImages = images
      this.lightboxIndex = index
      this.isLightboxOpen = true
    },
    // Chọn sản phẩm trong group (radio style)
    selectProduct(groupIdx, prodIdx) {
      const group = this.compareGroups[groupIdx]
      group.selectedProductIdx = prodIdx
      // Nếu là combo thì reset selectedComboProductIdx
      if (group.products[prodIdx] && group.products[prodIdx].type === 'combo') {
        if (group.products[prodIdx].selectedComboProductIdx == null) {
          group.products[prodIdx].selectedComboProductIdx = null
        }
      }
    },
    // Chọn sản phẩm con trong combo
    selectComboProduct(groupIdx, comboIdx, prodIdx) {
      const group = this.compareGroups[groupIdx]
      group.selectedProductIdx = comboIdx
      group.products[comboIdx].selectedComboProductIdx = prodIdx
    },
    addToCompare(item, groupIdx = 0) {
      if (this.compareGroups[groupIdx].products.length < 4) {
        // Check for category conflict before adding
        if (this.checkCategoryConflict(item, groupIdx)) {
          Swal.fire({
            icon: 'warning',
            title: 'Không thể thêm sản phẩm',
            text: `Không thể thêm "${item.name || item.comboName}" vì nhóm này đã có sản phẩm cùng loại. Vui lòng chọn nhóm khác hoặc xóa sản phẩm cùng loại.`,
          })
          return
        }
        this.compareGroups[groupIdx].products.push(this.cloneProduct(item))
        this.compareGroups[groupIdx].selectedProductIdx = this.compareGroups[groupIdx].products.length - 1;
        
      } else {
          Swal.fire({
            icon: 'warning',
            title: 'Bạn không thể thêm nhiều hơn!',
            text: `Bạn không thể thêm hơn 3 sản phẩm/combo trong group.`,
          })
          return
        }
      this.loadSelectedProducts()
    },
    addComboToCompare(combo, groupIdx = 0) {
      if (this.compareGroups[groupIdx].products.length < 4) {
        // Check for category conflict before adding combo
        if (this.checkCategoryConflict(combo, groupIdx)) {
          Swal.fire({
            icon: 'warning',
            title: 'Không thể thêm combo',
            text: `Không thể thêm "${combo.comboName}" vì nhóm này đã có sản phẩm cùng loại. Vui lòng chọn nhóm khác hoặc xóa sản phẩm cùng loại.`,
          })
          return
        }
        this.compareGroups[groupIdx].products.push(this.cloneProduct(combo))
        this.compareGroups[groupIdx].selectedProductIdx = this.compareGroups[groupIdx].products.length - 1;
        
      } else {
          Swal.fire({
            icon: 'warning',
            title: 'Bạn không thể thêm nhiều hơn!',
            text: `Bạn không thể thêm hơn 3 sản phẩm/combo trong group.`,
          })
          return
        }
      this.loadSelectedProducts()
    },
    cloneProduct(item) {
      // Đảm bảo mỗi item thêm vào group là một object mới, tránh ảnh hưởng lẫn nhau
      return JSON.parse(JSON.stringify(item))
    },
    removeFromCompare(groupIdx, prodIdx) {
      this.compareGroups[groupIdx].products.splice(prodIdx, 1)
      this.loadSelectedProducts()
    },
    // Không cho tách lẻ combo trong group so sánh
    removeFromCombo(groupIdx, comboIdx) {
      const combo = this.compareGroups[groupIdx].products[comboIdx]
      if (combo.type === 'combo') {
        this.compareGroups[groupIdx].products.splice(comboIdx, 1)
      }
      this.loadSelectedProducts()
    },
    getGroupTotal(group) {
      let total = 0
      for (const item of group.products) {
        if (item.type === 'combo') {
          total += item.products.reduce((sum, p) => sum + p.variant.price, 0)
        } else {
          total += item.variant.price
        }
      }
      return total
    },
    onChangeVariant(groupIdx, prodIdx) {
      const item = this.compareGroups[groupIdx].products[prodIdx]
      if (item.variants && item.variantKey != null) {
        item.variant = { ...item.variants[item.variantKey] }
      }
    },
    onChangeVariantCombo(groupIdx, comboIdx, prodIdx) {
      const combo = this.compareGroups[groupIdx].products[comboIdx]
      const prod = combo.products[prodIdx]
      if (prod.variants && prod.variantKey != null) {
        prod.variant = { ...prod.variants[prod.variantKey] }
      }
    },
    onDragStart(e, item) {
      this.dragItem = item
      e.dataTransfer.effectAllowed = 'move'
      e.dataTransfer.setData('text/plain', JSON.stringify(item))
    },
    onDropToGroup(e, groupIdx) {
      e.preventDefault()
      let item
      try {
        item = JSON.parse(e.dataTransfer.getData('text/plain'))
      } catch {
        item = this.dragItem
      }
      if (!item) return
      // Check for category conflict before adding
      if (this.checkCategoryConflict(item, groupIdx)) {
        Swal.fire({
          icon: 'warning',
          title: 'Không thể thêm sản phẩm',
          text: `Không thể thêm "${item.name || item.comboName}" vì nhóm này đã có sản phẩm cùng loại. Vui lòng chọn nhóm khác hoặc xóa sản phẩm cùng loại.`,
        })
        return
      }
      this.compareGroups[groupIdx].products.push(this.cloneProduct(item))
      this.compareGroups[groupIdx].selectedProductIdx = this.compareGroups[groupIdx].products.length - 1;
      
      this.dragItem = null
      this.loadSelectedProducts()
    },
    // onDropToNewGroup: không còn logic tạo group mới
    onDoubleClickSidebar(item) {
      // Khi double click, hỏi người dùng muốn thêm vào group nào (0 hoặc 1)
      Swal.fire({
        title: 'Thêm vào nhóm so sánh',
        text: 'Bạn muốn thêm sản phẩm này vào nhóm so sánh nào?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Nhóm 2',
        cancelButtonText: 'Nhóm 1',
      }).then((result) => {
        const groupIdx = result.isConfirmed ? 1 : 0
        if (item.type === 'combo') {
          this.addComboToCompare(item, groupIdx)
        } else {
          this.addToCompare(item, groupIdx)
        }
      })
    },
    closeLightbox() {
      this.isLightboxOpen = false
    },
    async tryOnModel(groupIdx) {
      const lightxApiKey = localStorage.getItem('lightxApiKey');
      if (!lightxApiKey) {
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

      const savedUrl = localStorage.getItem(`lightx_result_url_${groupIdx}`);
      if (savedUrl) {
        Swal.fire({
          title: 'Phân tích chưa hoàn tất',
          text: 'Đã tìm thấy kết quả thử đồ chưa được phân tích. Bạn có muốn tiếp tục phân tích ảnh này không?',
          icon: 'info',
          showCancelButton: true,
          confirmButtonText: 'Tiếp tục phân tích',
          cancelButtonText: 'Bắt đầu lại',
        }).then((result) => {
          if (result.isConfirmed) {
            this.resumeAnalysis(groupIdx, savedUrl);
          } else {
            localStorage.removeItem(`lightx_result_url_${groupIdx}`);
            this.showModelSelection = true;
            this.currentTryOnGroupIdx = groupIdx;
          }
        });
      } else {
        this.showModelSelection = true;
        this.currentTryOnGroupIdx = groupIdx;
      }
    },

    async resumeAnalysis(groupIdx, imageUrl) {
      this.loadingGroup = groupIdx;
      const group = this.compareGroups[groupIdx];
      const lightxApiKey = localStorage.getItem('lightxApiKey');

      try {
        const { tryOnImageUrl, analysisResult } = await LightXService.performTryOnAndAnalysis(
          lightxApiKey,
          imageUrl, // Use the saved imageUrl as the model image for resuming
          group.products
        );

        this.tryOnResults[groupIdx] = {
          model: { name: 'Người mẫu đã lưu', url: imageUrl },
          image: tryOnImageUrl,
          score: analysisResult?.aesthetic_score, // Use optional chaining
          style: analysisResult?.style,
          gender_suitability: analysisResult?.gender_suitability,
          products: group.products,
          time: new Date().toISOString(),
        };
        this.tryOnResults = { ...this.tryOnResults };
        this.groupFlipped[groupIdx] = true;

        // Notify user if analysis failed but image is available
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
        // This catch block now only handles critical errors from image generation
        console.error('Error during resumed analysis:', error);
        Swal.fire({
          icon: 'error',
          title: 'Tạo ảnh thất bại',
          text: error.message || 'Không thể tạo ảnh thử đồ. Vui lòng thử lại.',
        });
      } finally {
        localStorage.removeItem(`lightx_result_url_${groupIdx}`);
        this.loadingGroup = null;
      }
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

    async confirmModelSelection() {
      if (!this.selectedTryOnModel && !this.userModelFile) {
        Swal.fire({
          icon: 'warning',
          title: 'Chưa chọn mẫu',
          text: 'Vui lòng chọn một mẫu có sẵn hoặc tải lên ảnh của bạn.',
        });
        return;
      }

      const groupIdx = this.currentTryOnGroupIdx;
      this.loadingGroup = groupIdx;
      this.showModelSelection = false;

      const modelImageUrl = this.selectedTryOnModel
        ? this.selectedTryOnModel.url
        : this.userModelPreviewUrl;

      const lightxApiKey = localStorage.getItem('lightxApiKey');

      try {
        const group = this.compareGroups[groupIdx];
        if (!group || !group.products || group.products.length === 0) {
          Swal.fire({ icon: 'warning', title: 'Không có sản phẩm', text: 'Nhóm này chưa có sản phẩm để thử đồ.' });
          return;
        }

        const { tryOnImageUrl, analysisResult } = await LightXService.performTryOnAndAnalysis(
          lightxApiKey,
          modelImageUrl,
          group.products
        );

        this.tryOnResults[groupIdx] = {
          model: this.selectedTryOnModel
            ? { name: this.selectedTryOnModel.name, url: this.selectedTryOnModel.url }
            : { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl },
          image: tryOnImageUrl,
          score: analysisResult?.aesthetic_score, // Use optional chaining
          style: analysisResult?.style,
          gender_suitability: analysisResult?.gender_suitability,
          products: group.products,
          time: new Date().toISOString(),
        };
        this.tryOnResults = { ...this.tryOnResults };
        this.groupFlipped[groupIdx] = true;

        // Notify user if analysis failed but image is available
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
        // This catch block now only handles critical errors from image generation
        console.error('Error during try-on process:', error);
        Swal.fire({
          icon: 'error',
          title: 'Lỗi tạo ảnh',
          text: error.message || 'Có lỗi xảy ra trong quá trình tạo ảnh thử đồ.',
        });
      } finally {
        this.loadingGroup = null;
        this.cancelModelSelection();
      }
    },
    cancelModelSelection() {
      this.showModelSelection = false
      this.currentTryOnGroupIdx = null
      this.userModelFile = null
      this.userModelPreviewUrl = ''
      this.selectedTryOnModel = null
    },
    checkCategoryConflict(item, groupIdx) {
      const group = this.compareGroups[groupIdx];
      if (group.products.length === 0) return false;

      // Get all clothing types ('top', 'bottom') from the existing group products
      const groupClothingTypes = group.products
        .flatMap(p => {
          if (p.type === 'combo' && p.products && p.products.length > 0) {
            return p.products.map(prod => LightXService.getClothingCategory(prod.category));
          }
          return [LightXService.getClothingCategory(p.category)];
        });

      // Get all clothing types from the new item to be added
      let itemClothingTypes = [];
      if (item.type === 'combo' && item.products && item.products.length > 0) {
        itemClothingTypes = item.products.map(prod => LightXService.getClothingCategory(prod.category));
      } else {
        itemClothingTypes = [LightXService.getClothingCategory(item.category)];
      }

      // Check for conflict: if any clothing type from the new item already exists in the group
      // We ignore 'unknown' types in this check.
      return itemClothingTypes.some(
        type => type !== 'unknown' && groupClothingTypes.includes(type)
      );
    },
    async downloadTryOnResult(groupIdx) {
      const result = this.tryOnResults[groupIdx];
      if (!result || !result.image) return;

      try {
        // Fetch the image data
        const response = await fetch(result.image);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const blob = await response.blob();

        // Create a link and trigger the download
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        // Suggest a filename, e.g., tryon_result_1.png
        const extension = blob.type.split('/')[1] || 'jpg';
        a.download = `tryon_result_${groupIdx + 1}.${extension}`;
        document.body.appendChild(a); // Required for Firefox
        a.click();

        // Clean up
        URL.revokeObjectURL(url);
        document.body.removeChild(a);

      } catch (error) {
        console.error('Error downloading the try-on image:', error);
        Swal.fire({
          icon: 'error',
          title: 'Tải ảnh thất bại',
          text: 'Không thể tải về hình ảnh thử đồ. Vui lòng kiểm tra lại kết nối mạng hoặc thử lại sau.',
        });
      }
    },
    removeFromSidebar(item) {
      // Use the provided CompareStorageHelper.removeProductFromCompare method
      if (item.type === 'single') {
        CompareStorageHelper.removeProductFromCompare(
          item.id,
          'single',
          item.variant?.color,
          item.variant?.size,
        )
      } else if (item.type === 'combo') {
        CompareStorageHelper.removeProductFromCompare(item.id, 'combo')
      }
      this.loadSelectedProducts() // Refresh the sidebar list
    },
  },
}
</script>

<style scoped>
.loading-overlay-group {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 50%;
  background: rgba(255, 255, 255, 0.8);
  z-index: 10;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  transition: left 0.3s ease;
}

.group-disabled {
  opacity: 0.5;
  pointer-events: none;
}

.compare-btn-fixed {
  position: fixed;
  left: 50%;
  bottom: 32px;
  transform: translateX(-50%);
  z-index: 1001;
  background: #42a5f5;
  color: #fff;
  font-size: 1.5rem; /* Tăng kích thước icon */
  width: 64px; /* Đặt chiều rộng và chiều cao bằng nhau */
  height: 64px;
  padding: 0; /* Xóa padding */
  border: none;
  border-radius: 50%; /* Bo tròn thành hình tròn */
  box-shadow: 0 2px 12px #0002;
  cursor: pointer;
  transition: background 0.2s;
  display: flex; /* Căn giữa icon */
  align-items: center;
  justify-content: center;
}
.compare-btn-fixed:hover {
  background: #1976d2;
}
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
.compare-modal {
  background: #fff;
  border-radius: 16px;
  width: 95vw; /* Tăng chiều rộng trên màn hình lớn */
  max-width: 1400px; /* Tăng max-width */
  min-height: 600px;
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

.header-left, .header-right {
  flex: 1;
  display: flex;
  align-items: center;
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

.compare-main {
  flex: 2;
  overflow-x: hidden; /* Ẩn thanh cuộn ngang */
  display: flex;
  flex-direction: column;
}

/* Responsive adjustments */
@media (max-width: 992px) {
  .compare-modal {
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
  .compare-sidebar {
    min-width: unset;
    width: 100%;
    max-height: 300px; /* Giới hạn chiều cao sidebar trên mobile */
  }
  .compare-main {
    width: 100%;
  }
  .header-center h2 {
    font-size: 1.5rem;
  }
  .close-btn {
    font-size: 1.8rem;
  }
}

@media (max-width: 768px) {
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

.compare-list {
  display: flex;
  flex-direction: column;
  gap: 32px;
}
.compare-group {
  background: #f5f5f5;
  border-radius: 12px;
  padding: 16px 24px;
  margin-bottom: 8px;
  box-shadow: 0 2px 8px #0001;
  min-height: 120px;
}
.compare-group-products {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 8px;
}
.product-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.05);
  padding: 12px;
  min-width: 140px;
  text-align: center;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between; /* Distribute space evenly */
  height: 100%; /* Ensure cards in a row have equal height */
}
.product-card img {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 6px;
  margin-bottom: 6px;
}
.product-card .variant {
  font-size: 0.95rem;
  color: #666;
}
.product-card .price {
  font-weight: bold;
  color: #1976d2;
  margin-top: 2px;
}
.product-card .remove-btn {
  margin-top: 6px;
  background: #e53935;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 2px 10px;
  font-size: 0.95rem;
  cursor: pointer;
}
.product-card:hover .remove-btn {
  display: block;
}
.product-card.sidebar {
  flex-direction: row;
  align-items: center;
  gap: 12px; /* Increased gap for better spacing */
  min-width: 220px;
  margin-bottom: 8px;
  padding: 8px 10px;
  text-align: left; /* Align text to left for better readability in row layout */
}
.product-card.mini {
  min-width: 90px; /* Slightly reduced min-width */
  padding: 6px 8px;
  font-size: 0.9rem; /* Slightly smaller font size */
  flex-grow: 1; /* Allow mini cards to grow */
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
  height: 100%;
}
.product-card.mini img {
  width: 50px;
  height: 50px;
  margin-bottom: 4px;
}
.combo-card {
  background: #fffbe7;
  border: 1px dashed #fbc02d;
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  align-items: center; /* Center content horizontally */
  justify-content: space-between; /* Distribute space evenly */
  min-width: 180px;
  position: relative;
  height: 100%; /* Ensure cards in a row have equal height */
}
.combo-card.sidebar {
  margin-bottom: 8px;
  min-width: 220px;
  padding: 8px 12px; /* Adjusted padding */
  align-items: flex-start; /* Align content to start */
}
.combo-title {
  font-weight: bold;
  color: #fbc02d;
  margin-bottom: 4px;
  font-size: 1.05rem;
}
.combo-products {
  display: flex;
  flex-wrap: wrap; /* Allow items to wrap to the next line */
  justify-content: center; /* Center items horizontally */
  align-items: flex-start; /* Align items to the top */
  gap: 8px; /* Add some gap between mini product cards */
  width: 100%; /* Ensure it takes full width */
}
.combo-divider {
  width: 100%; /* Ensure divider spans full width */
  height: 1px;
  border-bottom: 1px dashed #fbc02d;
  margin: 8px 0; /* Adjust margin for better spacing */
}
.compare-sum {
  margin-bottom: 8px;
  font-size: 1.1rem;
  color: #1976d2;
}
.compare-info-section {
  background: #fff;
  border-radius: 6px;
  padding: 8px 12px;
  min-height: 40px;
  font-size: 1.05rem;
}

.info-title {
  font-weight: bold;
  color: #1976d2;
  margin-top: 15px;
  margin-bottom: 5px;
  border-bottom: 1px solid #eee;
  padding-bottom: 5px;
}

.info-title:first-of-type {
  margin-top: 0;
}
.compare-sidebar {
  flex: 1;
  min-width: 260px;
  background: #f0f4f8;
  border-radius: 12px;
  padding: 16px 12px;
  box-shadow: 0 1px 6px #0001;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  max-height: 70vh;
  overflow: auto;
}
.sidebar-list {
  margin-top: 8px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.add-btn {
  margin-top: 4px;
  background: #42a5f5;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 2px 10px;
  font-size: 0.95rem;
  cursor: pointer;
}
.add-btn:hover {
  background: #1976d2;
}
.drop-new-group {
  border: 2px dashed #90caf9;
  border-radius: 8px;
  padding: 18px;
  text-align: center;
  color: #1976d2;
  font-size: 1.1rem;
  margin-top: 16px;
  background: #e3f2fd;
  cursor: pointer;
  min-height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.sidebar-draggable {
  cursor: grab;
}

.model-selection-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1003;
  display: flex;
  align-items: center;
  justify-content: center;
}

.model-selection-modal {
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  width: 90%;
  max-width: 500px;
  text-align: center;
  display: flex;
  flex-direction: column;
  gap: 20px; /* Add gap between sections */
}

.model-list {
  display: flex;
  justify-content: center;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: 0; /* Remove bottom margin as gap is handled by parent */
}

.model-item {
  cursor: pointer;
  padding: 10px;
  border: 2px solid #eee;
  border-radius: 8px;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center; /* Center content vertically */
}

.model-item:hover {
  border-color: #42a5f5;
  box-shadow: 0 0 8px rgba(66, 165, 245, 0.3);
}

.model-item.selected {
  border-color: #1976d2;
  background-color: #e3f2fd;
  box-shadow: 0 0 10px rgba(25, 118, 210, 0.5);
}

.user-model-preview {
  transform: scale(1.15);
  border-width: 3px;
  border-color: #1976d2;
  box-shadow: 0 4px 15px rgba(25, 118, 210, 0.4);
}

.model-thumbnail {
  width: 100px;
  height: 150px;
  object-fit: cover;
  border-radius: 4px;
  margin-bottom: 8px;
}

.model-selection-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.model-selection-actions .btn {
  padding: 8px 20px;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
}

.model-selection-actions .btn-primary {
  background-color: #1976d2;
  color: #fff;
  border: none;
}

.model-selection-actions .btn-secondary {
  background-color: #ccc;
  color: #333;
  border: none;
}

.api-settings-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 1003;
  display: flex;
  align-items: center;
  justify-content: center;
}

.api-settings-modal {
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  width: 90%;
  max-width: 500px;
  text-align: center;
}

.api-settings-modal h3 {
  margin-bottom: 20px;
  color: #333;
}

.form-group {
  margin-bottom: 15px;
  text-align: left;
}

.form-control {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
}

.api-settings-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 20px;
}

.api-settings-actions .btn {
  padding: 8px 20px;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
}

.api-settings-actions .btn-primary {
  background-color: #1976d2;
  color: #fff;
  border: none;
}

.api-settings-actions .btn-secondary {
  background-color: #ccc;
  color: #333;
  border: none;
}

.delete-sidebar-btn {
  position: absolute;
  top: 4px;
  right: 4px;
  background: #e53935;
  color: #fff;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 1;
  opacity: 0.8;
  transition: opacity 0.2s;
}

.delete-sidebar-btn:hover {
  opacity: 1;
  background: #c62828;
}

.product-card.selected, .combo-card.selected {
  border: 2px solid #1976d2;
  box-shadow: 0 0 12px rgba(25, 118, 210, 0.5);
  transform: scale(1.03);
}

.combo-card.sidebar .combo-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
  margin-bottom: 4px;
}

.try-on-image-container {
  position: relative;
  display: inline-block; /* To wrap tightly around the image */
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