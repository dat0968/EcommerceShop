<template>
  <div>
    <h5>Thông tin hóa đơn & đánh giá</h5>
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
          <li class="list-group-item"><strong>Tình trạng:</strong> {{ orderDetail.tinhTrang }}</li>
        </ul>
      </div>
      <div
        v-for="item in orderItems"
        :key="item._type + '-' + item.id"
        class="border rounded p-3 mb-3"
      >
        <div class="d-flex align-items-center mb-2">
          <span
            class="badge"
            :class="item._type === 'product' ? 'bg-primary' : 'bg-success'"
            style="margin-right: 10px"
          >
            {{ item._type === 'product' ? 'Sản phẩm' : 'Combo' }}
          </span>
          <strong class="me-2">
            {{ item._type === 'product' ? 'Mã SP: ' + item.maSp : 'Mã Combo: ' + item.maCombo }}
          </strong>
          <span class="text-muted" style="font-size: 13px"> Số lượng: {{ item.soLuong }} </span>
        </div>

        <!-- Gộp input và hiển thị luôn -->
        <div class="mb-2">
          <label class="fw-bold">Số sao:</label>
          <div>
            <select
              v-model.number="item._editSoSao"
              class="form-control d-inline-block"
              style="width: 120px; display: inline-block"
            >
              <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
            </select>
            <span class="ms-2" style="font-size: xxx-large">
              <span v-for="n in item._editSoSao" :key="n" style="color: #ffc107">★</span>
              <span v-for="n in 5 - item._editSoSao" :key="'empty' + n" style="color: #e4e5e9"
                >★</span
              >
            </span>
          </div>
        </div>
        <div class="mb-2">
          <label class="fw-bold">Nội dung đánh giá:</label>
          <input v-model="item._editNoiDung" class="form-control" placeholder="Nội dung..." />
        </div>
        <div v-if="!item.maDanhGia" class="mb-2">
          <input
            type="file"
            multiple
            accept="image/*"
            @change="
              item._type === 'product'
                ? onProductImagesChange($event, item)
                : onComboImagesChange($event, item)
            "
          />
          <div class="d-flex flex-wrap mt-2">
            <img
              v-for="(img, idx) in item._previewImgs || []"
              :key="idx"
              :src="img"
              style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
            />
          </div>
        </div>
        <div v-else>
          <div class="d-flex flex-wrap mt-2">
            <img
              v-for="(img, idx) in item.hinhAnhs
                ? Array.isArray(item.hinhAnhs)
                  ? item.hinhAnhs
                  : item.hinhAnhs.split(',')
                : []"
              :key="idx"
              :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
              style="max-width: 80px; margin-right: 8px; border: 1px solid #ccc"
            />
          </div>
        </div>
        <blockquote
          v-if="item.shopPhanHoi"
          class="col-12"
          style="border-left: 2px solid #ccc; padding-left: 10px; margin: 10px 0"
        >
          <strong>Phản hồi của shop:</strong>
          {{ item.shopPhanHoi ? item.shopPhanHoi : 'Chưa có phản hồi' }}
        </blockquote>
        <button
          class="btn btn-success btn-sm me-2 mt-2"
          @click="
            item._type === 'product' ? submitOrderProductReview(item) : submitOrderComboReview(item)
          "
        >
          Lưu
        </button>
        <button
          class="btn btn-danger btn-sm mt-2"
          @click="
            item._type === 'product' ? deleteOrderProductReview(item) : deleteOrderComboReview(item)
          "
        >
          Xóa
        </button>
      </div>
    </div>
    <div v-else-if="orderResult">
      <pre class="bg-light p-2" style="max-height: 200px; overflow: auto">{{ orderResult }}</pre>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'

export default {
  name: 'ReviewOrder',
  props: {
    orderId: Number,
  },
  data() {
    return {
      orderIdLocal: this.orderId || null,
      orderDetail: null,
      orderResult: '',
      pathReplaceImg,
    }
  },
  computed: {
    orderItems() {
      if (!this.orderDetail) return []
      const products = (this.orderDetail.products || []).map((p) => ({ ...p, _type: 'product' }))
      const combos = (this.orderDetail.combos || []).map((c) => ({ ...c, _type: 'combo' }))
      return [...products, ...combos]
    },
  },
  methods: {
    isValidId(id) {
      return id !== null && id !== undefined && id !== 0 && !isNaN(id)
    },
    onProductImagesChange(e, prod) {
      if (prod._previewImgs) {
        prod._previewImgs.forEach((url) => URL.revokeObjectURL(url))
      }
      prod._selectedFiles = Array.from(e.target.files)
      prod._previewImgs = prod._selectedFiles.map((file) => URL.createObjectURL(file))
    },
    onComboImagesChange(e, combo) {
      if (combo._previewImgs) {
        combo._previewImgs.forEach((url) => URL.revokeObjectURL(url))
      }
      combo._selectedFiles = Array.from(e.target.files)
      combo._previewImgs = combo._selectedFiles.map((file) => URL.createObjectURL(file))
    },
    async getOrderDetail() {
      this.orderResult = 'Đang tải...'
      this.orderDetail = null
      try {
        const res = await axiosConfig.getFromApi(
          `/Review/orders/${this.orderIdLocal}`,
          ConfigsRequest.takeAuth(),
        )
        if (res.success) {
          this.orderDetail = res.data
          // Reset input cho từng sản phẩm/combo
          this.orderDetail.products.forEach((p) => {
            p._editNoiDung = p.noiDung || ''
            p._editSoSao = p.soSao || 5
            p._selectedFiles = []
            p._previewImgs = []
          })
          this.orderDetail.combos.forEach((c) => {
            c._editNoiDung = c.noiDung || ''
            c._editSoSao = c.soSao || 5
            c._selectedFiles = []
            c._previewImgs = []
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
  watch: {
    orderId(val) {
      this.orderIdLocal = val
      if (this.isValidId(val)) this.getOrderDetail()
    },
  },
  mounted() {
    if (this.isValidId(this.orderIdLocal)) this.getOrderDetail()
  },
}
</script>
