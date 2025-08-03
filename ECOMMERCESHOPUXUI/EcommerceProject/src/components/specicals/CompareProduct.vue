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
                      <div class="row">
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

                    <div class="mt-3 text-center">
                      <button
                        class="btn btn-primary"
                        @click="tryOnModel(groupIdx)"
                        :disabled="loadingGroup === groupIdx"
                      >
                        Thử đồ trên người mẫu
                      </button>
                    </div>
                  </div>
                  <!-- Mặt sau: hình ảnh thử đồ nếu có -->
                  <template v-else>
                    <div v-if="tryOnResults[groupIdx]">
                      <div class="text-center">
                        <b>Ảnh người mẫu đã ghép:</b><br />
                        <img
                          :src="tryOnResults[groupIdx].image"
                          style="max-width: 220px; border-radius: 8px; border: 1px solid #ccc"
                        />
                        <div class="mt-2">
                          <b>Điểm thẩm mỹ:</b>
                          <span style="font-size: 1.3rem; color: #e67e22"
                            >{{ tryOnResults[groupIdx].score }}/10</span
                          >
                        </div>
                        <div class="mt-2">
                          <b>Phong cách:</b>
                          <span>{{ tryOnResults[groupIdx].style }}</span>
                        </div>
                        <div class="mt-2">
                          <b>Giới tính phù hợp:</b>
                          <span>{{ tryOnResults[groupIdx].gender_suitability }}</span>
                        </div>
                        <button class="btn btn-success mt-2" @click="downloadTryOnResult(groupIdx)">
                          Tải về kết quả thử đồ
                        </button>
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
                  <div class="compare-tabs">
                    <button
                      v-for="tab in infoTabs"
                      :key="tab"
                      :class="{ active: group.activeTab === tab }"
                      @click="group.activeTab = tab"
                    >
                      {{ tab }}
                    </button>
                  </div>
                  <div class="compare-tab-content">
                    <div v-if="group.selectedProductIdx != null">
                      <div
                        v-if="
                          group.products[group.selectedProductIdx] &&
                          group.products[group.selectedProductIdx].type === 'combo' &&
                          group.products[group.selectedProductIdx].selectedComboProductIdx != null
                        "
                      >
                        <!-- Focus vào sản phẩm trong combo -->
                        <div v-if="group.activeTab === 'Mô tả'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx]?.products?.[
                                group.products[group.selectedProductIdx]?.selectedComboProductIdx
                              ]?.name || ''
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx]?.products?.[
                              group.products[group.selectedProductIdx]?.selectedComboProductIdx
                            ]?.description || ''
                          }}
                        </div>
                        <div v-else-if="group.activeTab === 'Đánh giá'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx]?.products?.[
                                group.products[group.selectedProductIdx]?.selectedComboProductIdx
                              ]?.name || ''
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx]?.products?.[
                              group.products[group.selectedProductIdx]?.selectedComboProductIdx
                            ]?.rating || ''
                          }}
                          ★
                        </div>
                        <div v-else-if="group.activeTab === 'Thông tin'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx]?.products?.[
                                group.products[group.selectedProductIdx]?.selectedComboProductIdx
                              ]?.name || ''
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx]?.products?.[
                              group.products[group.selectedProductIdx]?.selectedComboProductIdx
                            ]?.info || ''
                          }}
                        </div>
                      </div>
                      <div v-else>
                        <!-- Focus vào sản phẩm lẻ hoặc combo -->
                        <div v-if="group.activeTab === 'Mô tả'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName ||
                              ''
                            }}:</b
                          >
                          {{ group.products[group.selectedProductIdx].description }}
                        </div>
                        <div v-else-if="group.activeTab === 'Đánh giá'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName ||
                              ''
                            }}:</b
                          >
                          {{ group.products[group.selectedProductIdx].rating }} ★
                        </div>
                        <div v-else-if="group.activeTab === 'Thông tin'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName ||
                              ''
                            }}:</b
                          >
                          {{ group.products[group.selectedProductIdx].info }}
                        </div>
                      </div>
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

import * as axiosConfig from '@/utils/axiosClient'

/**
 * Converts a dataURL to a Blob object.
 * @param {string} dataurl - The data URL to convert.
 * @returns {Blob|null} - The resulting Blob, or null if input is invalid.
 */
function dataURLtoBlob(dataurl) {
  if (!dataurl || typeof dataurl !== 'string') {
    console.error('dataURLtoBlob: Invalid dataurl input', dataurl)
    return null
  }
  // Check if it's not a data URL, but a regular URL or something else
  if (!dataurl.startsWith('data:')) {
    console.error('dataURLtoBlob: Input is not a data URL', dataurl)
    return null
  }
  const arr = dataurl.split(',')
  if (arr.length < 2) {
    console.error('dataURLtoBlob: Malformed dataurl, missing comma', dataurl)
    return null
  }
  const mimeMatch = arr[0].match(/:(.*?);/)
  if (!mimeMatch || !mimeMatch[1]) {
    console.error('dataURLtoBlob: Could not extract mime type', arr[0])
    return null
  }
  const mime = mimeMatch[1]
  let bstr
  try {
    bstr = atob(arr[1])
  } catch (e) {
    console.error('dataURLtoBlob: Failed to decode base64', e, arr[1])
    return null
  }
  const n = bstr.length
  const u8arr = new Uint8Array(n)
  for (let i = 0; i < n; i++) {
    u8arr[i] = bstr.charCodeAt(i)
  }
  return new Blob([u8arr], { type: mime })
}

export default {
  name: 'CompareProduct',
  components: { VueEasyLight },
  data() {
    return {
      showModal: false,
      showSidebarModal: true,
      infoTabs: ['Mô tả', 'Đánh giá', 'Thông tin'],
      selectedProducts: [],
      showApiSettings: false,
      loadingGroup: null, // Index of the group currently being processed by AI
      apiKeys: {
        cloudinaryApiKey: localStorage.getItem('cloudinaryApiKey') || '',
        cloudinaryApiSecret: localStorage.getItem('cloudinaryApiSecret') || '',
        cloudinaryCloudName: localStorage.getItem('cloudinaryCloudName') || '',
        cloudinaryUploadPreset: localStorage.getItem('cloudinaryUploadPreset') || 'unsigned_upload',
        lightxApiKey: localStorage.getItem('lightxApiKey') || '',
        geminiApiKey: localStorage.getItem('geminiApiKey') || '', 
      },
      compareGroups: [
        { products: [], activeTab: 'Mô tả', selectedProductIdx: null },
        { products: [], activeTab: 'Mô tả', selectedProductIdx: null },
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
      if (this.compareGroups[groupIdx].products.length < 10) {
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
      }
      this.loadSelectedProducts()
    },
    addComboToCompare(combo, groupIdx = 0) {
      if (this.compareGroups[groupIdx].products.length < 10) {
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

      const savedUrl = localStorage.getItem(`lightx_result_url_${groupIdx}`)
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
            this.resumeAnalysis(groupIdx, savedUrl)
          } else {
            localStorage.removeItem(`lightx_result_url_${groupIdx}`)
            this.showModelSelection = true
            this.currentTryOnGroupIdx = groupIdx
          }
        })
      } else {
        this.showModelSelection = true
        this.currentTryOnGroupIdx = groupIdx
      }
    },

    async resumeAnalysis(groupIdx, imageUrl) {
      this.loadingGroup = groupIdx
      const group = this.compareGroups[groupIdx]
      const modelInfo = { name: 'Người mẫu đã lưu', url: '' } // Model info is lost, but not critical

      try {
        const geminiResponse = await axiosConfig.postToApi('/TryOn/AnalyzeImage', {
          resultImageUrl: imageUrl,
          productsData: group.products,
        })

        if (!geminiResponse.success) {
          const errorData =  geminiResponse
          throw new Error(`Gemini API request failed: ${errorData.error?.message || geminiResponse.statusText}`)
        }

        const result =  geminiResponse

        this.tryOnResults[groupIdx] = {
          model: modelInfo,
          image: imageUrl, // Use the saved image URL
          score: result.data.aesthetic_score,
          style: result.data.style,
          gender_suitability: result.data.gender_suitability,
          products: group.products,
          time: new Date().toISOString(),
        }
        this.tryOnResults = { ...this.tryOnResults }
        this.groupFlipped[groupIdx] = true

        // Clean up on success
        localStorage.removeItem(`lightx_result_url_${groupIdx}`)
      } catch (error) {
        console.error('Error during resumed analysis:', error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi phân tích',
          text: `Không thể hoàn tất phân tích. Vui lòng thử lại. Lỗi: ${error.message}`,
        })
      } finally {
        this.loadingGroup = null
      }
    },

    cancelApiSettings() {
      this.showApiSettings = false
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

    async confirmModelSelection() {
      if (!this.selectedTryOnModel && !this.userModelFile) {
        Swal.fire({
          icon: 'warning',
          title: 'Chưa chọn mẫu',
          text: 'Vui lòng chọn một mẫu có sẵn hoặc tải lên ảnh của bạn.',
        })
        return
      }

      const groupIdx = this.currentTryOnGroupIdx
      this.loadingGroup = groupIdx
      this.showModelSelection = false

      const modelInfo = this.selectedTryOnModel
        ? { name: this.selectedTryOnModel.name, url: this.selectedTryOnModel.url }
        : { name: 'Người mẫu tải lên', url: this.userModelPreviewUrl }

      const lightXResultUrlKey = `lightx_result_url_${groupIdx}`

      try {
        const group = this.compareGroups[groupIdx]
        if (!group || !group.products || group.products.length === 0) {
          Swal.fire({ icon: 'warning', title: 'Không có sản phẩm', text: 'Nhóm này chưa có sản phẩm để thử đồ.' })
          return
        }

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

        const productPublicUrls = [];
        for (const item of group.products) {
          let imgUrl = item.image || (item.products && item.products[0]?.image);
          if (imgUrl) {
            if (imgUrl.includes('localhost')) {
              const response = await axiosConfig.postToApi('/TryOn/UploadFromUrl', { imageUrl: imgUrl });
              if (!response.success) throw new Error(`Không thể tải ảnh sản phẩm "${item.name || ''}" lên máy chủ.`);
              productPublicUrls.push(response.data.imageUrl);
            } else {
              productPublicUrls.push(imgUrl);
            }
          }
        }

        if (productPublicUrls.length === 0) {
          Swal.fire({ icon: 'error', title: 'Lỗi', text: 'Không có hình ảnh sản phẩm hợp lệ để xử lý.' });
          return;
        }

        // Step 2: Load all images as data URLs to be sent to LightX
        const modelDataUrl = await this.loadImageAsDataUrl(modelImageUrl);
        const productDataUrls = [];
        for(const url of productPublicUrls) {
            productDataUrls.push(await this.loadImageAsDataUrl(url));
        }

        // Step 3: Process with LightX using the reliable blob upload method
        const tryOnImageResultUrl = await this.processWithLightX(modelDataUrl, productDataUrls);
        localStorage.setItem(lightXResultUrlKey, tryOnImageResultUrl);

        // Step 4: Send to Gemini for analysis
        const geminiResponse = await axiosConfig.postToApi('/TryOn/AnalyzeImage', {
          resultImageUrl: tryOnImageResultUrl,
          productsData: group.products,
        })

        if (!geminiResponse.success) {
          const errorData = geminiResponse
          throw new Error(`Gemini API request failed: ${errorData.error?.message || geminiResponse.data.message}`)
        }

        const result = geminiResponse

        this.tryOnResults[groupIdx] = {
          model: modelInfo,
          image: tryOnImageResultUrl,
          score: result.data.aesthetic_score,
          style: result.data.style,
          gender_suitability: result.data.gender_suitability,
          products: group.products,
          time: new Date().toISOString(),
        }
        this.tryOnResults = { ...this.tryOnResults }
        this.groupFlipped[groupIdx] = true

        localStorage.removeItem(lightXResultUrlKey)

      } catch (error) {
        console.error('Error during try-on process:', error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi xử lý',
          text: error.message || 'Có lỗi xảy ra trong quá trình xử lý. Kết quả trung gian đã được lưu, bạn có thể thử lại.',
        })
      } finally {
        this.loadingGroup = null
        this.cancelModelSelection()
      }
    },
    cancelModelSelection() {
      this.showModelSelection = false
      this.currentTryOnGroupIdx = null
      this.userModelFile = null
      this.userModelPreviewUrl = ''
      this.selectedTryOnModel = null
    },
    async processWithLightX(modelDataUrl, productDataUrls) {
      try {
        const apiKey = this.apiKeys.lightxApiKey;

        // Step 1 & 2: Upload model image
        const modelBlob = dataURLtoBlob(modelDataUrl);
        if (!modelBlob) {
          throw new Error('Không thể chuyển đổi ảnh người mẫu sang định dạng có thể xử lý. Ảnh có thể bị hỏng.');
        }
        const modelUploadData = await this.getLightXUploadUrl(apiKey, modelBlob.size);
        await this.uploadToLightX(modelUploadData.uploadImage, modelBlob);
        const modelImageUrl = modelUploadData.imageUrl;

        // Step 1 & 2: Upload product image (we use the first one for the style)
        const productBlob = dataURLtoBlob(productDataUrls[0]);
        if (!productBlob) {
          throw new Error('Không thể chuyển đổi ảnh sản phẩm sang định dạng có thể xử lý. Ảnh có thể bị hỏng.');
        }
        const productUploadData = await this.getLightXUploadUrl(apiKey, productBlob.size);
        await this.uploadToLightX(productUploadData.uploadImage, productBlob);
        const styleImageUrl = productUploadData.imageUrl;

        // Step 3: Start the job
        const orderId = await this.startLightXJob(apiKey, modelImageUrl, styleImageUrl);

        // Step 4: Poll for the result
        const resultUrl = await this.pollLightXJob(apiKey, orderId);
        return resultUrl;

      } catch (error) {
        console.error('Error processing with LightX API:', error);
        let userMessage = error.message; // Default message
        if (error.message && error.message.includes('5044')) {
          userMessage =
            'Không thể xử lý ảnh bằng AI. Điều này có thể do ảnh người mẫu hoặc ảnh sản phẩm không phù hợp (ví dụ: độ phân giải thấp, khuôn mặt không rõ ràng, hoặc định dạng không được hỗ trợ). Vui lòng thử sử dụng một ảnh khác.'
        } else if (error.message && error.message.includes('timed out')) {
          userMessage = 'Quá trình xử lý mất quá nhiều thời gian. Vui lòng thử lại sau.'
        }

        Swal.fire({
          icon: 'error',
          title: 'Lỗi xử lý ảnh từ LightX',
          text: userMessage,
        });
        throw error; // Re-throw to be caught by the calling function
      }
    },
    async getLightXUploadUrl(apiKey, size) {
      const response = await fetch('https://api.lightxeditor.com/external/api/v2/uploadImageUrl', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-api-key': apiKey,
        },
        body: JSON.stringify({
          uploadType: 'imageUrl',
          size: size,
          contentType: 'image/jpeg',
        }),
      })
      const data = await response.json()
      if (data.statusCode !== 2000) {
        console.error('LightX getUploadUrl failed. Full response:', data)
        throw new Error('Failed to get LightX upload URL: ' + data.message)
      }
      return data.body
    },

    async uploadToLightX(uploadUrl, blob) {
      const response = await fetch(uploadUrl, {
        method: 'PUT',
        headers: { 'Content-Type': 'image/jpeg' },
        body: blob,
      })
      if (!response.ok) {
        const errorText = await response.text()
        console.error('LightX image upload failed. Full response:', errorText)
        throw new Error('Failed to upload image to LightX.')
      }
    },

    async startLightXJob(apiKey, imageUrl, styleImageUrl) {
      const response = await fetch('https://api.lightxeditor.com/external/api/v2/aivirtualtryon', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-api-key': apiKey,
        },
        body: JSON.stringify({ imageUrl, styleImageUrl }),
      })
      const data = await response.json()
      if (data.statusCode !== 2000) {
        console.error('LightX startJob failed. Full response:', data)
        throw new Error('Failed to start LightX job: ' + data.message)
      }
      return data.body.orderId
    },

    async pollLightXJob(apiKey, orderId) {
      const maxRetries = 10 // Tăng số lần thử lại
      const delay = 5000 // Tăng thời gian chờ lên 5 giây

      for (let i = 0; i < maxRetries; i++) {
        await new Promise((resolve) => setTimeout(resolve, delay))
        const response = await fetch('https://api.lightxeditor.com/external/api/v2/order-status', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'x-api-key': apiKey,
          },
          body: JSON.stringify({ orderId }),
        })
        const data = await response.json()

        // **FIX: Thêm kiểm tra chặt chẽ cho phản hồi từ API**
        if (data.statusCode !== 2000 || !data.body) {
          console.error('LightX pollJob failed or returned unexpected data. Full response:', data)
          throw new Error(`LightX job status check failed: ${data.message || 'No response body'}`)
        }

        if (data.body.status === 'active') {
          return data.body.output
        }
        if (data.body.status === 'failed') {
          console.error('LightX job failed. Full response:', data)
          throw new Error('LightX job failed.')
        }
        // Nếu trạng thái là 'pending' hoặc khác, vòng lặp sẽ tiếp tục
      }
      throw new Error('LightX job timed out after several retries.')
    },
    async loadImageAsDataUrl(url) {
      const img = await this.loadImage(url);
      const canvas = document.createElement('canvas');
      canvas.width = img.naturalWidth;
      canvas.height = img.naturalHeight;
      const ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0);
      return canvas.toDataURL('image/jpeg');
    },
    loadImage(url) {
      return new Promise((resolve, reject) => {
        const img = new window.Image()
        img.crossOrigin = 'Anonymous' // Request CORS
        img.onload = () => resolve(img)
        img.onerror = (e) => {
          console.error('Error loading image:', url, e)
          reject(new Error(`Failed to load image from ${url}. Check URL and CORS settings.`))
        }
        img.src = url
      })
    },

    // --- End of Re-added Helper Functions ---

    checkCategoryConflict(item, groupIdx) {
      const group = this.compareGroups[groupIdx]
      if (group.products.length === 0) return false
      const groupCategories = group.products
        .map((p) => {
          if (p.type === 'combo' && p.products && p.products.length > 0) {
            return p.products.map((prod) => prod.category || 'unknown')
          }
          return p.category || 'unknown'
        })
        .flat()
      let itemCategories = []
      if (item.type === 'combo' && item.products && item.products.length > 0) {
        itemCategories = item.products.map((prod) => prod.category || 'unknown')
            } else {
        itemCategories = [item.category || 'unknown']
      }

      // Check if any category of the item matches any category in the group
      return itemCategories.some((cat) => groupCategories.includes(cat) && cat !== 'unknown')
    },
    downloadTryOnResult(groupIdx) {
      const result = this.tryOnResults[groupIdx]
      if (!result) return
      const data = {
        ...result,
        image: undefined,
      }
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `tryon_result_${groupIdx + 1}.json`
      a.click()
      URL.revokeObjectURL(url)
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
  width: 90vw;
  max-width: 1200px;
  min-height: 600px;
  max-height: 90vh;
  box-shadow: 0 4px 32px #0003;
  display: flex;
  flex-direction: column;
  position: relative;
}
.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 32px 8px 32px;
  border-bottom: 1px solid #eee;
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

.header-right {
  justify-content: flex-end;
}
.close-btn {
  font-size: 2rem;
  background: none;
  border: none;
  color: #888;
  cursor: pointer;
}
.modal-body {
  display: flex;
  flex: 1;
  padding: 16px 32px;
  gap: 32px;
  min-height: 0;
  max-height: 70vh;
}
.overflow-auto {
  overflow: auto;
}
.compare-main {
  flex: 2;
  overflow-x: auto;
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
  box-shadow: 0 1px 4px #0001;
  padding: 12px 16px;
  min-width: 140px;
  text-align: center;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
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
.product-card.sidebar {
  flex-direction: row;
  align-items: center;
  gap: 8px;
  min-width: 220px;
  margin-bottom: 8px;
  padding: 8px 10px;
}
.product-card.mini {
  min-width: 100px;
  padding: 6px 8px;
  font-size: 0.95rem;
}
.combo-card {
  background: #fffbe7;
  border: 1px dashed #fbc02d;
  border-radius: 8px;
  padding: 8px 12px;
  margin-right: 8px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  min-width: 180px;
  position: relative;
}
.combo-card.sidebar {
  margin-bottom: 8px;
  min-width: 220px;
}
.combo-title {
  font-weight: bold;
  color: #fbc02d;
  margin-bottom: 4px;
  font-size: 1.05rem;
}
.combo-products {
  display: flex;
  align-items: center;
  gap: 0;
}
.combo-divider {
  width: 18px;
  height: 2px;
  border-bottom: 2px dashed #fbc02d;
  margin: 0 4px;
}
.compare-sum {
  margin-bottom: 8px;
  font-size: 1.1rem;
  color: #1976d2;
}
.compare-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}
.compare-tabs button {
  background: #e3f2fd;
  border: none;
  border-radius: 6px;
  padding: 4px 16px;
  font-size: 1rem;
  cursor: pointer;
  color: #1976d2;
  transition: background 0.2s;
}
.compare-tabs button.active {
  background: #1976d2;
  color: #fff;
}
.compare-tab-content {
  background: #fff;
  border-radius: 6px;
  padding: 8px 12px;
  min-height: 40px;
  font-size: 1.05rem;
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
}

.model-selection-modal h3 {
  margin-bottom: 20px;
  color: #333;
}

.model-list {
  display: flex;
  justify-content: center;
  gap: 20px;
  flex-wrap: wrap;
  margin-bottom: 20px;
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

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
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
</style>
