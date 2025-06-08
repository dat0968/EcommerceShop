<template>
  <div>
    <!-- Nút mở modal, đặt absolute góc trái -->
    <button
      class="btn btn-primary position-fixed"
      style="bottom: 30px; left: 20px; z-index: 1050"
      @click="showModal = !showModal"
    >
      {{ showModal ? '🔴' : '🟢' }}
    </button>

    <!-- Modal overlay -->
    <div v-if="showModal" class="modal-backdrop fade show" style="z-index: 1040"></div>
    <!-- Modal card -->
    <div
      v-if="showModal"
      class="bg-white border-rounded p-3 position-fixed top-50 start-50 translate-middle"
      style="z-index: 1051; min-width: 700px; max-width: 95vw; max-height: 95vh; overflow: auto"
    >
      <ul class="nav nav-tabs mb-3">
        <li class="nav-item">
          <a
            class="nav-link"
            :class="{ active: activeTab === 'product' }"
            href="#"
            @click.prevent="activeTab = 'product'"
            >Sản phẩm/Combo</a
          >
        </li>
        <li class="nav-item">
          <a
            class="nav-link"
            :class="{ active: activeTab === 'order' }"
            href="#"
            @click.prevent="activeTab = 'order'"
            >Hóa đơn của tôi</a
          >
        </li>
      </ul>

      <div v-if="activeTab === 'product'">
        <div class="row mb-3">
          <div class="col-9 mb-3">
            <input
              type="number"
              v-model="infoTransTab1.objectId"
              min="1"
              name=""
              id=""
              class="form-control"
              placeholder="Nhập mã"
            />
          </div>
          <div class="col-3">
            <label for=""
              ><input type="checkbox" v-model="infoTransTab1.isProduct" name="" id="" />Là sản
              phẩm</label
            >
          </div>
          <button class="col-12 btn btn-info" @click="tranformDataTab1">Nhập</button>
        </div>
        <ReviewProductCombo
          :objectId="infoTransTab1._objectId"
          :isProduct="infoTransTab1._isProduct"
        />
      </div>
      <div v-else>
        <div class="row mb-3">
          <div class="col-12 mb-3">
            <input
              type="number"
              v-model="infoTransTab2.orderId"
              min="1"
              name=""
              id=""
              class="form-control"
              placeholder="Nhập mã"
            />
          </div>
          <button class="col-12 btn btn-info" @click="transformDataTab2">Nhập</button>
        </div>
        <ReviewOrder :orderId="infoTransTab2._orderId" />
      </div>
    </div>
  </div>
</template>

<script>
import ReviewProductCombo from '@/components/reviews/ReviewProductCombo.vue'
import ReviewOrder from '@/components/reviews/ReviewOrder.vue'

export default {
  name: 'TestReaction',
  components: {
    ReviewProductCombo,
    ReviewOrder,
  },
  data() {
    return {
      showModal: false,
      activeTab: 'product',
      infoTransTab1: {
        objectId: null,
        isProduct: true,
        _objectId: null,
        _isProduct: true,
      },
      infoTransTab2: {
        orderId: null,
        _orderId: null,
      },
    }
  },
  methods: {
    tranformDataTab1() {
      this.infoTransTab1._objectId = this.infoTransTab1.objectId
      this.infoTransTab1._isProduct = this.infoTransTab1.isProduct
    },
    transformDataTab2() {
      this.infoTransTab2._orderId = this.infoTransTab2.orderId
      console.log(this.infoTransTab2)
    },
  },
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.3);
}
</style>
