<template>
  <div>
    <!-- Nút so sánh sản phẩm cố định giữa dưới -->
    <button class="compare-btn-fixed" @click="showModal = true">So sánh sản phẩm</button>

    <!-- Modal so sánh sản phẩm -->
    <div v-if="showModal" class="modal-overlay" @click.self="showModal = false">
      <div class="compare-modal">
        <div class="modal-header">
          <h2>So sánh sản phẩm</h2>
          <button class="close-btn" @click="showModal = false">×</button>
        </div>
        <div class="modal-body overflow-auto">
          <!-- Vùng so sánh chính -->
          <div class="compare-main">
            <div class="compare-list">
              <div
                v-for="(group, groupIdx) in compareGroups"
                :key="groupIdx"
                class="compare-group"
                @dragover.prevent
                @drop="onDropToGroup($event, groupIdx)"
              >
                <div class="compare-group-products">
                  <div v-for="(item, idx) in group.products" :key="idx" class="draggable-item">
                    <div v-if="item.type === 'combo'" class="combo-card">
                      <div class="combo-title">{{ item.comboName }}</div>
                      <div class="combo-products">
                        <div v-for="(prod, pidx) in item.products" :key="pidx">
                          <div class="product-card mini">
                            <img :src="prod.image" alt="" />
                            <div>{{ prod.name }}</div>
                            <div class="variant">
                              {{ prod.variant.color }} / {{ prod.variant.size }}
                            </div>
                            <div class="price">{{ formatPrice(prod.variant.price) }}</div>
                            <!-- Không cho tách lẻ combo trong group so sánh -->
                            <button class="remove-btn" @click="removeFromCombo(groupIdx, idx)">
                              Xóa
                            </button>
                          </div>
                          <div v-if="pidx < item.products.length - 1" class="combo-divider"></div>
                        </div>
                      </div>
                    </div>
                    <div v-else class="product-card">
                      <img :src="item.image" alt="" />
                      <div>{{ item.name }}</div>
                      <div class="variant">{{ item.variant.color }} / {{ item.variant.size }}</div>
                      <div class="price">{{ formatPrice(item.variant.price) }}</div>
                      <button class="remove-btn" @click="removeFromCompare(groupIdx, idx)">
                        Xóa
                      </button>
                    </div>
                  </div>
                </div>
                <div class="compare-sum">
                  Giá tổng: <b>{{ formatPrice(getGroupTotal(group)) }}</b>
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
                  <div v-if="group.activeTab === 'Mô tả'">
                    <div v-for="item in group.products" :key="item.id">
                      <b>{{ item.name || item.comboName }}:</b> {{ item.description }}
                    </div>
                  </div>
                  <div v-else-if="group.activeTab === 'Đánh giá'">
                    <div v-for="item in group.products" :key="item.id">
                      <b>{{ item.name }}:</b> {{ item.rating }} ★
                    </div>
                  </div>
                  <div v-else-if="group.activeTab === 'Thông tin'">
                    <div v-for="item in group.products" :key="item.id">
                      <b>{{ item.name }}:</b> {{ item.info }}
                    </div>
                  </div>
                </div>
              </div>
              <!-- Khu vực drop để tạo group mới -->
              <div class="drop-new-group" @dragover.prevent @drop="onDropToNewGroup($event)">
                <span>Kéo sản phẩm vào đây để tạo nhóm so sánh mới</span>
              </div>
            </div>
          </div>
          <!-- Cột phải: danh sách sản phẩm đã chọn để so sánh -->
          <div class="compare-sidebar">
            <h3>Danh sách đã chọn</h3>
            <div class="sidebar-list">
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
                        <div class="price">{{ formatPrice(prod.variant.price) }}</div>
                      </div>
                      <div v-if="pidx < item.products.length - 1" class="combo-divider"></div>
                    </div>
                  </div>
                  <button class="add-btn" @click="addComboToCompare(item)">Thêm vào so sánh</button>
                </div>
                <div v-else class="product-card sidebar">
                  <div class="row p-2">
                    <div class="col-4 d-flex justify-content-center align-items-center border-end">
                      <img :src="item.image" alt="" class="img-fluid" />
                    </div>
                    <div class="col-8 d-flex flex-column align-items-start grap-2">
                      <div>{{ item.name }}</div>
                      <div class="variant">
                        Loại: {{ item.variant.color }} / {{ item.variant.size }}
                      </div>
                      <div class="price">Giá: {{ formatPrice(item.variant.price) }}</div>
                    </div>
                    <button class="add-btn col-12" @click="addToCompare(item)">
                      Thêm vào so sánh
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CompareProduct',
  data() {
    return {
      showModal: false,
      infoTabs: ['Mô tả', 'Đánh giá', 'Thông tin'],
      selectedProducts: [
        {
          id: 1,
          name: 'Áo dài nữ',
          type: 'single',
          image: 'https://i.imgur.com/1.jpg',
          category: 'Áo dài',
          variant: { color: 'Đỏ', size: 'M', price: 350000 },
          description: 'Áo dài truyền thống nữ, chất liệu lụa.',
          rating: 4.7,
          info: 'Chất liệu: Lụa, Xuất xứ: Việt Nam',
        },
        {
          id: 2,
          name: 'Quần đùi nam',
          type: 'single',
          image: 'https://i.imgur.com/2.jpg',
          category: 'Quần đùi',
          variant: { color: 'Xanh', size: 'L', price: 120000 },
          description: 'Quần đùi nam thể thao, thoáng mát.',
          rating: 4.2,
          info: 'Chất liệu: Cotton, Xuất xứ: Việt Nam',
        },
        {
          id: 3,
          name: 'Áo lót nữ',
          type: 'single',
          image: 'https://i.imgur.com/3.jpg',
          category: 'Áo lót',
          variant: { color: 'Be', size: 'S', price: 90000 },
          description: 'Áo lót nữ nâng ngực, thoải mái.',
          rating: 4.5,
          info: 'Chất liệu: Thun lạnh, Xuất xứ: Việt Nam',
        },
        {
          id: 4,
          name: 'Áo lót nam',
          type: 'single',
          image: 'https://i.imgur.com/3.jpg',
          category: 'Áo lót',
          variant: { color: 'Be', size: 'S', price: 190000 },
          description: 'Áo lót nam nâng ngực, thoải mái.',
          rating: 4.5,
          info: 'Chất liệu: Thun lạnh, Xuất xứ: Việt Nam',
        },
        {
          id: 100,
          type: 'combo',
          comboName: 'Combo Hè Năng Động',
          products: [
            {
              id: 4,
              name: 'Áo sơ mi nam',
              image: 'https://i.imgur.com/4.jpg',
              category: 'Áo sơ mi',
              variant: { color: 'Trắng', size: 'M', price: 220000 },
              description: 'Áo sơ mi nam công sở.',
              rating: 4.3,
              info: 'Chất liệu: Cotton, Xuất xứ: Việt Nam',
            },
            {
              id: 5,
              name: 'Quần đùi nam',
              image: 'https://i.imgur.com/2.jpg',
              category: 'Quần đùi',
              variant: { color: 'Xanh', size: 'L', price: 120000 },
              description: 'Quần đùi nam thể thao, thoáng mát.',
              rating: 4.2,
              info: 'Chất liệu: Cotton, Xuất xứ: Việt Nam',
            },
          ],
          description: 'Nhóm hàng tốt.',
          rating: 3.7,
          info: 'Combo được ưa chuộng nhiều nhất',
        },
      ],
      compareGroups: [],
      dragItem: null,
    }
  },
  methods: {
    formatPrice(val) {
      return val.toLocaleString('vi-VN') + '₫'
    },
    addToCompare(item) {
      // Cho phép thêm sản phẩm lẻ vào group đã có combo và ngược lại
      const group = this.compareGroups.find((g) => g.products.length > 0)
      if (group) {
        group.products.push(item)
      } else {
        this.compareGroups.push({ products: [item], activeTab: 'Mô tả' })
      }
    },
    addComboToCompare(combo) {
      // Cho phép thêm combo vào group đã có sản phẩm lẻ
      const group = this.compareGroups.find((g) => g.products.length > 0)
      if (group) {
        group.products.push(combo)
      } else {
        this.compareGroups.push({ products: [combo], activeTab: 'Mô tả' })
      }
    },
    removeFromCompare(groupIdx, prodIdx) {
      this.compareGroups[groupIdx].products.splice(prodIdx, 1)
      if (this.compareGroups[groupIdx].products.length === 0) {
        this.compareGroups.splice(groupIdx, 1)
      }
    },
    // Không cho tách l�� combo trong group so sánh
    removeFromCombo(groupIdx, comboIdx) {
      const combo = this.compareGroups[groupIdx].products[comboIdx]
      if (combo.type === 'combo') {
        this.compareGroups[groupIdx].products.splice(comboIdx, 1)
      }
      if (this.compareGroups[groupIdx].products.length === 0) {
        this.compareGroups.splice(groupIdx, 1)
      }
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
      this.compareGroups[groupIdx].products.push(item)
      this.dragItem = null
    },
    onDropToNewGroup(e) {
      e.preventDefault()
      let item
      try {
        item = JSON.parse(e.dataTransfer.getData('text/plain'))
      } catch {
        item = this.dragItem
      }
      if (!item) return
      this.compareGroups.push({ products: [item], activeTab: 'Mô tả' })
      this.dragItem = null
    },
    onDoubleClickSidebar(item) {
      // Double click: cho phép chọn group bất kỳ để thêm vào, không giới hạn
      if (this.compareGroups.length === 0) {
        this.compareGroups.push({ products: [item], activeTab: 'Mô tả' })
      } else {
        // Thêm vào group đầu tiên (hoặc có thể mở modal chọn group nếu muốn)
        this.compareGroups[0].products.push(item)
      }
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
