<template>
  <div>
    <!-- Nút so sánh sản phẩm cố định giữa dưới -->
    <button class="compare-btn-fixed" @click="showModal = true">So sánh sản phẩm</button>

    <!-- Modal so sánh sản phẩm -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="compare-modal">
        <div class="modal-header">
          <button class="col-1 btn" @click="showSidebarModal = !showSidebarModal">
            {{ showSidebarModal ? '>' : '=' }}
          </button>
          <h2 class="text-center col-10">So sánh sản phẩm</h2>
          <button class="close-btn col-1" @click="showModal = false">×</button>
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
                      to="/"
                      style="color: #1976d2; text-decoration: underline; font-weight: 500"
                      >Khám phá sản phẩm ngay!</router-link
                    >
                  </div>
                </div>
              </template>
              <template v-else>
                <div
                  v-for="(item, idx) in selectedProducts"
                  :key="idx"
                  :draggable="true"
                  @dragstart="onDragStart($event, item)"
                  class="sidebar-draggable"
                  @dblclick="onDoubleClickSidebar(item)"
                >
                  <div v-if="item.type === 'combo'" class="combo-card sidebar">
                    <div class="combo-title">{{ item.comboName }}</div>
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
                    <button class="add-btn" @click="addComboToCompare(item)">
                      Thêm vào so sánh
                    </button>
                  </div>
                  <div v-else class="product-card sidebar">
                    <div class="row p-2">
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
                      <button class="add-btn col-12" @click="addToCompare(item)">
                        Thêm vào so sánh
                      </button>
                    </div>
                  </div>
                </div>
              </template>
            </div>
          </div>
          <!-- Vùng so sánh chính -->
          <div class="compare-main">
            <div class="row align-items-stretch" style="min-height: 400px">
              <div
                v-for="(group, groupIdx) in compareGroups"
                :key="groupIdx"
                class="col-6"
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
                    :disabled="!tryOnResults[groupIdx]"
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
                      <div class="w-100 row">
                        <div
                          v-for="(item, idx) in group.products"
                          :key="idx"
                          class="draggable-item mb-2"
                          :class="[
                            item.type === 'combo' ? 'col-12' : 'col-6',
                            group.selectedProductIdx === idx ? 'selected' : '',
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
                                class="col-6 mb-2"
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
                      <button class="btn btn-primary" @click="tryOnModel(groupIdx)">
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
                              group.products[group.selectedProductIdx].products[
                                group.products[group.selectedProductIdx].selectedComboProductIdx
                              ].name
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx].products[
                              group.products[group.selectedProductIdx].selectedComboProductIdx
                            ].description
                          }}
                        </div>
                        <div v-else-if="group.activeTab === 'Đánh giá'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].products[
                                group.products[group.selectedProductIdx].selectedComboProductIdx
                              ].name
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx].products[
                              group.products[group.selectedProductIdx].selectedComboProductIdx
                            ].rating
                          }}
                          ★
                        </div>
                        <div v-else-if="group.activeTab === 'Thông tin'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].products[
                                group.products[group.selectedProductIdx].selectedComboProductIdx
                              ].name
                            }}:</b
                          >
                          {{
                            group.products[group.selectedProductIdx].products[
                              group.products[group.selectedProductIdx].selectedComboProductIdx
                            ].info
                          }}
                        </div>
                      </div>
                      <div v-else>
                        <!-- Focus vào sản phẩm lẻ hoặc combo -->
                        <div v-if="group.activeTab === 'Mô tả'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName
                            }}:</b
                          >
                          {{ group.products[group.selectedProductIdx].description }}
                        </div>
                        <div v-else-if="group.activeTab === 'Đánh giá'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName
                            }}:</b
                          >
                          {{ group.products[group.selectedProductIdx].rating }} ★
                        </div>
                        <div v-else-if="group.activeTab === 'Thông tin'">
                          <b
                            >{{
                              group.products[group.selectedProductIdx].name ||
                              group.products[group.selectedProductIdx].comboName
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
              <!-- Khu vực drop để tạo group mới -->
              <!-- <div class="drop-new-group" @dragover.prevent @drop="onDropToNewGroup($event)">
                <span>Kéo sản phẩm vào đây để tạo nhóm so sánh mới</span>
              </div> -->
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

function dataURLtoBlob(dataurl) {
  var arr = dataurl.split(','),
    mime = arr[0].match(/:(.*?);/)[1],
    bstr = atob(arr[1]),
    n = bstr.length,
    u8arr = new Uint8Array(n)
  while (n--) {
    u8arr[n] = bstr.charCodeAt(n)
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
      compareGroups: [
        { products: [], activeTab: 'Mô tả', selectedProductIdx: null },
        { products: [], activeTab: 'Mô tả', selectedProductIdx: null },
      ],
      groupFlipped: [false, false], // Trạng thái lật mặt của từng group
      dragItem: null,
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
      tryOnResults: {},
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
        this.compareGroups[groupIdx].products.push(this.cloneProduct(item))
      }
      this.loadSelectedProducts()
    },
    addComboToCompare(combo, groupIdx = 0) {
      if (this.compareGroups[groupIdx].products.length < 10) {
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
      // Cho phép thêm bất kỳ sản phẩm hoặc combo vào group
      this.compareGroups[groupIdx].products.push(this.cloneProduct(item))
      this.dragItem = null
      this.loadSelectedProducts()
    },
    // onDropToNewGroup: không còn logic tạo group mới
    onDoubleClickSidebar(item) {
      // Khi double click, hỏi người dùng muốn thêm vào group nào (0 hoặc 1)
      const groupIdx = window.confirm('Thêm vào nhóm so sánh 2? (OK: nhóm 2, Cancel: nhóm 1)')
        ? 1
        : 0
      this.addToCompare(item, groupIdx)
    },
    closeLightbox() {
      this.isLightboxOpen = false
    },
    async tryOnModel(groupIdx) {
      try {
        // 1. Chọn mẫu người mẫu (ở đây demo lấy sẵn 2 mẫu, bạn có thể mở rộng)
        const models = [
          {
            name: 'Nữ đứng',
            url: 'https://images.pexels.com/photos/532220/pexels-photo-532220.jpeg?w=400',
            gender: 'female',
            pose: 'standing',
          },
          {
            name: 'Nam ngồi',
            url: 'https://images.pexels.com/photos/614810/pexels-photo-614810.jpeg?w=400',
            gender: 'male',
            pose: 'sitting',
          },
        ]
        const selectedModel = await this.selectModel(models)
        if (!selectedModel) {
          alert('Bạn chưa chọn mẫu người mẫu.')
          return
        }
        // 2. Ghép ảnh sản phẩm lên người mẫu (canvas)
        const group = this.compareGroups[groupIdx]
        if (!group || !group.products || group.products.length === 0) {
          alert('Nhóm này chưa có sản phẩm để thử đồ.')
          return
        }
        const canvas = document.createElement('canvas')
        const ctx = canvas.getContext('2d')
        canvas.width = 400
        canvas.height = 600
        // Vẽ người mẫu
        let modelImg
        try {
          modelImg = await this.loadImage(selectedModel.url)
        } catch (e) {
          alert('Không thể tải ảnh người mẫu.')
          return
        }
        ctx.drawImage(modelImg, 0, 0, 400, 600)
        // Vẽ từng sản phẩm lên (demo: xếp chồng lên nhau, thực tế cần mapping vị trí theo danh mục)
        let y = 200
        for (const item of group.products) {
          let imgUrl = item.image || (item.products && item.products[0]?.image)
          if (!imgUrl) continue
          let prodImg
          try {
            prodImg = await this.loadImage(imgUrl)
          } catch (e) {
            alert('Không thể tải ảnh sản phẩm: ' + (item.name || ''))
            continue
          }
          ctx.drawImage(prodImg, 100, y, 200, 120) // demo vị trí
          y += 120
        }
        // 3. Lấy base64
        const base64 = canvas.toDataURL('image/jpeg')
        // 4. Gửi lên DeepAI lấy điểm thẩm mỹ
        let score = 0
        try {
          score = await this.getAestheticScore(base64)
        } catch (e) {
          alert('Lỗi khi gửi ảnh lên AI chấm điểm.')
          score = 0
        }
        // 5. Lưu kết quả
        this.tryOnResults[groupIdx] = {
          model: selectedModel,
          image: base64,
          score,
          products: group.products,
          time: new Date().toISOString(),
        }
        this.tryOnResults = { ...this.tryOnResults }
      } catch (err) {
        alert('Đã xảy ra lỗi không xác định khi thử đồ trên người mẫu.')
      }
    },
    async selectModel(models) {
      // Đơn giản: prompt chọn mẫu (có thể làm UI modal đẹp hơn)
      let msg = 'Chọn mẫu người mẫu:\\n'
      models.forEach((m, i) => (msg += `${i + 1}. ${m.name}\\n`))
      const idx = parseInt(window.prompt(msg, '1'), 10) - 1
      if (isNaN(idx) || idx < 0 || idx >= models.length) return null
      return models[idx]
    },
    loadImage(url) {
      return new Promise((resolve, reject) => {
        const img = new window.Image()
        img.crossOrigin = 'Anonymous'
        img.onload = () => resolve(img)
        img.onerror = reject
        img.src = url
      })
    },

    async getAestheticScore(canvas) {
      const apiKey = 'YOUR_DEEPAI_API_KEY' // Thay bằng API Key thật
      try {
        let blob
        if (canvas.toBlob) {
          blob = await new Promise((resolve) => canvas.toBlob(resolve, 'image/jpeg'))
        } else {
          // Fallback: convert base64 to blob
          const dataUrl = canvas.toDataURL('image/jpeg')
          blob = dataURLtoBlob(dataUrl)
        }
        const formData = new FormData()
        formData.append('image', blob, 'tryon.jpg')
        const resp = await fetch('https://api.deepai.org/api/aesthetic-score', {
          method: 'POST',
          headers: {
            'Api-Key': apiKey,
            // KHÔNG đặt Content-Type, để browser tự set multipart/form-data
          },
          body: formData,
        })
        if (!resp.ok) {
          const text = await resp.text()
          alert('Lỗi AI: ' + resp.status + ' - ' + text)
          return 0
        }
        const data = await resp.json()
        return data.output || 0
      } catch (e) {
        alert('Lỗi khi gửi ảnh lên AI: ' + e.message)
        return 0
      }
    },
    downloadTryOnResult(groupIdx) {
      const result = this.tryOnResults[groupIdx]
      if (!result) return
      const data = {
        ...result,
        image: undefined, // Không nhúng base64 vào JSON
      }
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `tryon_result_${groupIdx + 1}.json`
      a.click()
      URL.revokeObjectURL(url)
    },
  },
}
</script>

<style scoped>
.compare-btn-fixed {
  position: fixed;
  left: 50%;
  bottom: 32px;
  transform: translateX(-50%);
  z-index: 1001;
  background: #42a5f5;
  color: #fff;
  font-size: 1.3rem;
  padding: 16px 48px;
  border: none;
  border-radius: 32px;
  box-shadow: 0 2px 12px #0002;
  cursor: pointer;
  transition: background 0.2s;
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
</style>
