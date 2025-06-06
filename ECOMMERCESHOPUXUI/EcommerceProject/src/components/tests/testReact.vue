<template>
  <div>
    <!-- Nút mở modal, đặt absolute góc trái -->
    <button
      class="btn btn-primary position-fixed"
      style="bottom: 30px; left: 20px; z-index: 1050"
      @click="showModal = !showModal"
    >
      Mở đánh giá
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

      <!-- Tab 1: Sản phẩm/Combo -->
      <div v-if="activeTab === 'product'">
        <h5>Thông tin sản phẩm/combo & đánh giá</h5>
        <div class="row mb-3">
          <div class="col-md-6">
            <label>Mã sản phẩm:</label>
            <input
              v-model.number="productId"
              type="number"
              class="form-control"
              placeholder="Nhập mã sản phẩm"
            />
          </div>
          <div class="col-md-6">
            <label>Mã combo:</label>
            <input
              v-model.number="comboId"
              type="number"
              class="form-control"
              placeholder="Nhập mã combo"
            />
          </div>
        </div>
        <div class="mb-3">
          <button class="btn btn-info me-2" @click="getProductReviews()">
            Lấy đánh giá sản phẩm
          </button>
          <button class="btn btn-info me-2" @click="getComboReviews()">Lấy đánh giá combo</button>
        </div>
        <div class="mb-3">
          <label>Nội dung đánh giá:</label>
          <input v-model="reviewForm.noiDung" class="form-control" placeholder="Nội dung..." />
          <label class="mt-2">Số sao:</label>
          <select v-model.number="reviewForm.soSao" class="form-control">
            <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
          </select>
        </div>
        <div class="mb-3">
          <button class="btn btn-success me-2" @click="addReview">Thêm đánh giá</button>
          <button class="btn btn-warning me-2" @click="updateReview">Cập nhật đánh giá</button>
          <!-- ! Fix this-->
          <button class="btn btn-danger" @click="deleteReview(1)">Xóa đánh giá</button>
        </div>
        <div class="mb-3">
          <strong>Danh sách đánh giá:</strong>
          <ul class="list-group">
            <li
              v-for="review in result.data"
              :key="review.id"
              class="list-group-item position-relative"
            >
              <strong>Nội dung:</strong> {{ review.noiDung }} <br />
              <strong>Số sao:</strong> {{ review.soSao }} <br />
              <strong>Ngày đánh giá:</strong> {{ new Date(review.ngayDanhGia).toLocaleString() }}
              <br />
              <blockquote
                class="col-12"
                style="border-left: 2px solid #ccc; padding-left: 10px; margin: 10px 0"
              >
                <strong>Phản hồi của shop:</strong>
                {{ review.shopPhanHoi ? review.shopPhanHoi : 'Chưa có phản hồi' }}
              </blockquote>
            </li>
          </ul>
        </div>
      </div>

      <!-- Tab 2: Hóa đơn của tôi -->
      <div v-else>
        <div v-if="!isUserLoggedIn" class="alert alert-warning">
          Vui lòng <RouterLink to="/Login">đăng nhập</RouterLink> để xem hóa đơn và đánh giá.
        </div>
        <div v-else>
          <h5>Thông tin hóa đơn & đánh giá</h5>
          <div class="mb-3">
            <label>Mã hóa đơn:</label>
            <input
              v-model.number="orderId"
              type="number"
              class="form-control"
              placeholder="Nhập mã hóa đơn"
            />
            <button class="btn btn-info mt-2" @click="getOrderDetail">Xem hóa đơn</button>
          </div>
          <div v-if="orderDetail">
            <div class="mb-2">
              <h4>Thông tin hóa đơn:</h4>
              <ul class="list-group">
                <li class="list-group-item"><strong>Mã hóa đơn:</strong> {{ orderDetail.maHd }}</li>
                <li class="list-group-item">
                  <strong>Ngày tạo:</strong> {{ new Date(orderDetail.ngayTao).toLocaleString() }}
                </li>
                <li class="list-group-item"><strong>Khách:</strong> {{ orderDetail.hoTen }}</li>
                <li class="list-group-item">
                  <strong>Địa chỉ:</strong> {{ orderDetail.diaChiNhanHang }}
                </li>
                <li class="list-group-item">
                  <strong>Tình trạng:</strong> {{ orderDetail.tinhTrang }}
                </li>
              </ul>
            </div>
            <div>
              <h5>Sản phẩm trong hóa đơn:</h5>
              <div
                v-for="prod in orderDetail.products"
                :key="prod.id"
                class="border rounded p-2 mb-2"
              >
                <div>Mã sản phẩm: {{ prod.maSp }} | Số lượng: {{ prod.soLuong }}</div>
                <div>Đánh giá: {{ prod.soSao > 0 ? prod.soSao + ' sao' : 'Chưa đánh giá' }}</div>
                <div>Nội dung: {{ prod.noiDung }}</div>
                <div>
                  <label>Nội dung đánh giá:</label>
                  <input
                    v-model="prod._editNoiDung"
                    class="form-control"
                    placeholder="Nội dung..."
                  />
                  <label class="mt-2">Số sao:</label>
                  <select v-model.number="prod._editSoSao" class="form-control">
                    <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
                  </select>
                  <div v-if="!prod.maDanhGia" class="mb-2">
                    <input
                      type="file"
                      multiple
                      accept="image/*"
                      @change="onProductImagesChange($event, prod)"
                    />
                    <div class="d-flex flex-wrap mt-2">
                      <img
                        v-for="(img, idx) in prod._previewImgs || []"
                        :key="idx"
                        :src="img"
                        style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
                      />
                    </div>
                  </div>
                  <div v-else>
                    <div class="d-flex flex-wrap mt-2">
                      <img
                        v-for="(img, idx) in prod.hinhAnhs || []"
                        :key="idx"
                        :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
                        style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
                      />
                    </div>
                  </div>
                  <button
                    class="btn btn-success btn-sm me-2 mt-2"
                    @click="submitOrderProductReview(prod)"
                  >
                    Lưu
                  </button>
                  <button
                    class="btn btn-danger btn-sm mt-2"
                    @click="deleteOrderProductReview(prod)"
                  >
                    Xóa
                  </button>
                </div>
              </div>
              <h5>Combo trong hóa đơn:</h5>
              <div
                v-for="combo in orderDetail.combos"
                :key="combo.maCombo"
                class="border rounded p-2 mb-2"
              >
                <div>Mã combo: {{ combo.maCombo }} | Số lượng: {{ combo.soLuong }}</div>
                <div>Đánh giá: {{ combo.soSao > 0 ? combo.soSao + ' sao' : 'Chưa đánh giá' }}</div>
                <div>Nội dung: {{ combo.noiDung }}</div>
                <div>
                  <label>Nội dung đánh giá:</label>
                  <input
                    v-model="combo._editNoiDung"
                    class="form-control"
                    placeholder="Nội dung..."
                  />
                  <label class="mt-2">Số sao:</label>
                  <select v-model.number="combo._editSoSao" class="form-control">
                    <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
                  </select>
                  <div v-if="!combo.maDanhGia" class="mb-2">
                    <input
                      type="file"
                      multiple
                      accept="image/*"
                      @change="onComboImagesChange($event, combo)"
                    />
                    <div class="d-flex flex-wrap mt-2">
                      <img
                        v-for="(img, idx) in combo._previewImgs || []"
                        :key="idx"
                        :src="img"
                        style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
                      />
                    </div>
                  </div>
                  <div v-else>
                    <div class="d-flex flex-wrap mt-2">
                      <img
                        v-for="(img, idx) in combo.hinhAnhs || []"
                        :key="idx"
                        :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
                        style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
                      />
                    </div>
                  </div>
                  <button
                    class="btn btn-success btn-sm me-2 mt-2"
                    @click="submitOrderComboReview(combo)"
                  >
                    Lưu
                  </button>
                  <button class="btn btn-danger btn-sm mt-2" @click="deleteOrderComboReview(combo)">
                    Xóa
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div v-else-if="orderResult">
            <pre class="bg-light p-2" style="max-height: 200px; overflow: auto">{{
              orderResult
            }}</pre>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import authService from '@/services/authService'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'
export default {
  name: 'TestReaction',
  data() {
    return {
      showModal: false,
      activeTab: 'product',
      productId: 0,
      comboId: 0,
      reviewForm: {
        id: null,
        noiDung: '',
        soSao: 5,
        maSp: null,
        maCombo: null,
      },
      result: '',
      isUserLoggedIn: !authService.isExpiredSessionAccess(),
      userId: authService.getUserId(),
      orderId: null,
      orderDetail: null,
      orderResult: '',
      pathReplaceImg,
    }
  },
  computed: {
    orderDetailInfo() {
      if (!this.orderDetail) return ''
      const { maHd, ngayTao, hoTen, diaChiNhanHang, tinhTrang } = this.orderDetail
      return `Mã hóa đơn: ${maHd}\nNgày tạo: ${ngayTao}\nKhách: ${hoTen}\nĐịa chỉ: ${diaChiNhanHang}\nTình trạng: ${tinhTrang}`
    },
  },
  methods: {
    onProductImagesChange(e, prod) {
      prod._selectedFiles = Array.from(e.target.files)
      prod._previewImgs = prod._selectedFiles.map((file) => URL.createObjectURL(file))
    },
    onComboImagesChange(e, combo) {
      combo._selectedFiles = Array.from(e.target.files)
      combo._previewImgs = combo._selectedFiles.map((file) => URL.createObjectURL(file))
    },
    async getProductReviews() {
      this.result = 'Đang tải...'
      try {
        const res = await axiosConfig.getFromApi(
          `/review/products/${this.productId}`,
          ConfigsRequest.getSkipAuthConfig(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã nhận dữ liệu đánh giá!')
        this.result = res
      } catch (e) {
        this.result = e.message
      }
    },
    async getComboReviews() {
      this.result = 'Đang tải...'
      try {
        const res = await axiosConfig.getFromApi(
          `/review/combos/${this.comboId}`,
          ConfigsRequest.getSkipAuthConfig(),
        )
        this.result = res
      } catch (e) {
        this.result = e.message
      }
    },
    async addReview() {
      this.result = 'Đang gửi...'
      try {
        const isProduct = this.productId > 0
        const body = {
          noiDung: this.reviewForm.noiDung,
          soSao: this.reviewForm.soSao,
          maSp: isProduct ? this.productId : null,
          maCombo: isProduct ? this.comboId : null,
        }
        const res = await axiosConfig.postToApi(
          `/Review?isProduct=${isProduct}`,
          body,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã lưu đánh giá!')
        this.result = res
        if (res.data && res.data.id) this.reviewForm.id = res.data.id
      } catch (e) {
        this.result = e.message
      }
    },
    async updateReview() {
      this.result = 'Đang cập nhật...'
      try {
        if (!this.reviewForm.id) {
          this.result = 'Bạn cần nhập id đánh giá để cập nhật!'
          return
        }
        const isProduct = this.productId > 0
        const body = {
          id: this.reviewForm.id,
          noiDung: this.reviewForm.noiDung,
          soSao: this.reviewForm.soSao,
          maSp: isProduct ? this.productId : null,
          maCombo: isProduct ? this.comboId : null,
        }
        const res = await axiosConfig.putToApi(
          `/Review?isProduct=${isProduct}`,
          body,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã cập nhập đánh giá!')
        this.result = res
      } catch (e) {
        this.result = e.message
      }
    },
    async deleteReview(reviewId) {
      this.result = 'Đang xóa...'
      try {
        const res = await axiosConfig.deleteFromApi(
          `/Review/${reviewId}`,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã lưu đánh giá!')
        this.result = res
      } catch (e) {
        this.result = e.message
      }
    },
    // Tab hóa đơn
    async getOrderDetail() {
      this.orderResult = 'Đang tải...'
      this.orderDetail = null
      try {
        const res = await axiosConfig.getFromApi(
          `/Review/orders/${this.orderId}`,
          ConfigsRequest.takeAuth(),
        )
        if (res.success) {
          this.orderDetail = res.data
          // Chuẩn hóa các sản phẩm cho phép chỉnh sửa
          this.orderDetail.products.forEach((p) => {
            p._editNoiDung = p.noiDung
            p._editSoSao = p.soSao
          })
          this.orderDetail.combos.forEach((c) => {
            c._editNoiDung = c.noiDung
            c._editSoSao = c.soSao
          })
        }
        this.orderResult = ''
      } catch (e) {
        this.orderResult = e.message
      }
    },

    async submitOrderProductReview(prod) {
      try {
        let res
        if (prod.maDanhGia) {
          // Đã có đánh giá, chỉ cập nhật
          const body = {
            id: prod.maDanhGia,
            noiDung: prod._editNoiDung,
            soSao: prod._editSoSao,
            maSp: prod.maSp,
            maCtsp: prod.maCtsp,
            maCtHd: prod.id,
          }
          res = await axiosConfig.putToApi(
            `/Review?isProduct=true`,
            body,
            ConfigsRequest.takeAuth(),
          )
        } else {
          // Chưa có đánh giá, gửi kèm ảnh
          const formData = new FormData()
          formData.append('noiDung', prod._editNoiDung)
          formData.append('soSao', prod._editSoSao)
          formData.append('maSp', prod.maSp)
          formData.append('maCtsp', prod.maCtsp)
          formData.append('maCtHd', prod.id)
          if (prod._selectedFiles) {
            prod._selectedFiles.forEach((file) => formData.append('hinhAnhs', file))
          }

          /* ? Maybe create method to check this
          for (let pair of formData.entries()) {
            console.log(pair[0], pair[1])
          } */
          res = await axiosConfig.postToApi(`/Review?isProduct=true`, formData, {
            ...ConfigsRequest.takeAuth(),
          })
        }
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã đánh giá thành công!')
        await this.getOrderDetail()
      } catch (e) {
        alert('Lỗi: ' + e.message)
      }
    },
    async deleteOrderProductReview(prod) {
      try {
        const res = await axiosConfig.deleteFromApi(
          `/Review/${prod.maDanhGia}?isProduct=true`,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) return
        else alert('Đã xóa đánh giá!')
        await this.getOrderDetail()
      } catch (e) {
        alert('Lỗi: ' + e.message)
      }
    },

    async submitOrderComboReview(combo) {
      try {
        let res
        if (combo.maDanhGia) {
          // Đã có đánh giá, chỉ cập nhật
          const body = {
            id: combo.maDanhGia,
            noiDung: combo._editNoiDung,
            soSao: combo._editSoSao,
            maCombo: combo.maCombo,
            maCtHd: combo.id,
          }
          res = await axiosConfig.putToApi(
            `/Review?isProduct=false`,
            body,
            ConfigsRequest.takeAuth(),
          )
        } else {
          // Chưa có đánh giá, gửi kèm ảnh
          const formData = new FormData()
          formData.append('noiDung', combo._editNoiDung)
          formData.append('soSao', combo._editSoSao)
          formData.append('maCombo', combo.maCombo)
          formData.append('maCtHd', combo.id)
          if (combo._selectedFiles) {
            combo._selectedFiles.forEach((file) => formData.append('hinhAnhs', file))
          }
          console.log(formData)
          res = await axiosConfig.postToApi(`/Review?isProduct=false`, formData, {
            ...ConfigsRequest.takeAuth(),
          })
        }
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        else alert('Đã lưu đánh giá!')
        await this.getOrderDetail()
      } catch (e) {
        alert('Lỗi: ' + e.message)
      }
    },
    async deleteOrderComboReview(combo) {
      try {
        const res = await axiosConfig.deleteFromApi(
          `/Review/${combo.maCombo}?isProduct=false`,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) return
        else alert('Đã xóa đánh giá!')
        await this.getOrderDetail()
      } catch (e) {
        alert('Lỗi: ' + e.message)
      }
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
