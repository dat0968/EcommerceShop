<template>
  <div>
    <!-- Breadcrumb Begin -->
    <div class="breadcrumb-option">
      <div class="container">
        <div class="row">
          <div class="col-lg-12">
            <div class="breadcrumb__links">
              <a href="/"><i class="fa fa-home"></i> Trang chủ</a>
              <span>Đánh giá</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Breadcrumb End -->

    <!-- Shop Section Begin -->
    <section class="shop spad">
      <div class="container" style="min-height: 50vh">
        <div class="row">
          <div class="d-flex align-items-center mb-3">
            <ul class="nav nav-tabs flex-grow-1">
              <li class="nav-item">
                <a
                  class="nav-link p-1 rounded"
                  :class="{ active: activeTab === 'notReviewed' }"
                  href="#"
                  @click.prevent="activeTab = 'notReviewed'"
                  >Chưa đánh giá</a
                >
              </li>
              <li class="nav-item">
                <a
                  class="nav-link p-1 rounded"
                  :class="{ active: activeTab === 'reviewed' }"
                  href="#"
                  @click.prevent="activeTab = 'reviewed'"
                  >Đã đánh giá</a
                >
              </li>
            </ul>
            <button class="btn btn-outline-primary ms-3" @click="reloadReviews">
              <span class="bi bi-arrow-clockwise"></span> Load lại
            </button>
          </div>

          <div v-if="activeTab === 'notReviewed'">
            <div v-if="notReviewed.length">
              <div v-for="item in notReviewed" :key="item.id" class="border rounded p-3 mb-3">
                <div class="mb-2 p-2 bg-light d-flex align-items-center">
                  <div v-if="item.tenHinhAnh" class="me-3">
                    <img
                      :src="pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh)"
                      alt="Ảnh sản phẩm"
                      class="img-fluid border border-light rounded"
                      @click="
                        openLightbox(
                          [pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh)],
                          0,
                        )
                      "
                      style="width: 7em; height: 5em; object-fit: cover"
                    />
                  </div>
                  <div>
                    <strong>{{ item.maSp ? 'Sản phẩm' : 'Combo' }}:</strong>
                    {{ item.maSp || item.maCombo }}
                    <span v-if="item.tenDoiTuong">| {{ item.tenDoiTuong }}</span>
                    <span v-if="item.kichThuoc">| Size: {{ item.kichThuoc }}</span>
                    <span v-if="item.mauSac">| Màu: {{ item.mauSac }}</span>
                    <span v-if="item.donGia">| Giá: {{ formatCurrency(item.donGia) }}</span>
                    <span v-if="item.soLuongTon !== undefined">| Tồn: {{ item.soLuongTon }}</span>
                  </div>
                </div>
                <div class="mb-2">
                  <label>Số sao:</label>
                  <select
                    v-model.number="item._editSoSao"
                    class="form-control"
                    style="width: 120px; display: inline-block"
                  >
                    <option v-for="n in 5" :key="n" :value="n">{{ n }} Sao</option>
                  </select>
                  <span class="ms-2">
                    <span v-for="n in item._editSoSao" :key="n" style="color: #ffc107">★</span>
                    <span v-for="n in 5 - item._editSoSao" :key="'empty' + n" style="color: #e4e5e9"
                      >★</span
                    >
                  </span>
                </div>
                <div class="mb-2">
                  <label>Nội dung đánh giá:</label>
                  <input
                    v-model="item._editNoiDung"
                    class="form-control"
                    placeholder="Nội dung..."
                  />
                </div>
                <div class="mb-2">
                  <input
                    type="file"
                    multiple
                    accept="image/*"
                    :disabled="getImageCount(item) >= maxImages"
                    @change="onImagesChange($event, item)"
                  />
                  <small class="text-muted ms-2"
                    >{{ getImageCount(item) }}/{{ maxImages }} ảnh</small
                  >
                  <div class="d-flex flex-wrap mt-2">
                    <img
                      v-for="(img, idx) in item._previewImgs || []"
                      :key="idx"
                      :src="img"
                      class="img-fluid me-2 border border-light rounded-5"
                      style="max-width: 10em; height: 10em"
                      @click="openLightbox(item._previewImgs, idx)"
                    />
                  </div>
                </div>
                <button class="btn btn-success btn-sm me-2" @click="submitReview(item)">
                  Lưu đánh giá
                </button>
              </div>
            </div>
            <EmptySuggestBox
              v-else
              contentText="Bạn chưa có sản phẩm cần đánh giá nào. Hãy khám phá thêm sản phẩm!"
              linkNav="/shop"
            />
          </div>

          <div v-else>
            <div v-if="reviewed.length">
              <div v-for="item in reviewed" :key="item.id" class="border rounded p-3 mb-3">
                <div class="mb-2 p-2 bg-light d-flex align-items-center">
                  <div v-if="item.tenHinhAnh" class="me-3">
                    <img
                      :src="pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh)"
                      alt="Ảnh sản phẩm"
                      class="img-fluid border border-light rounded"
                      @click="
                        openLightbox(
                          [pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh)],
                          0,
                        )
                      "
                      style="width: 7em; height: 5em; object-fit: cover"
                    />
                  </div>
                  <div>
                    <strong>{{ item.maSp ? 'Sản phẩm' : 'Combo' }}:</strong>
                    {{ item.maSp || item.maCombo }}
                    <span v-if="item.tenDoiTuong">| {{ item.tenDoiTuong }}</span>
                    <span v-if="item.kichThuoc">| Size: {{ item.kichThuoc }}</span>
                    <span v-if="item.mauSac">| Màu: {{ item.mauSac }}</span>
                    <span v-if="item.donGia">| Giá: {{ formatCurrency(item.donGia) }}</span>
                    <span v-if="item.soLuongTon !== undefined">| Tồn: {{ item.soLuongTon }}</span>
                  </div>
                </div>
                <div class="mb-2">
                  <span>
                    <span v-for="n in item.soSao" :key="n" style="color: #ffc107">★</span>
                    <span v-for="n in 5 - item.soSao" :key="'empty' + n" style="color: #e4e5e9"
                      >★</span
                    >
                  </span>
                  <span class="ms-2 text-muted">{{ item.soSao }} sao</span>
                </div>
                <div class="mb-2"><strong>Nội dung:</strong> {{ item.noiDung }}</div>
                <div v-if="item.hinhAnhs && item.hinhAnhs.length" class="d-flex flex-wrap mb-2">
                  <img
                    v-for="(img, idx) in Array.isArray(item.hinhAnhs)
                      ? item.hinhAnhs
                      : item.hinhAnhs.split(',')"
                    :key="idx"
                    :src="pathReplaceImg(undefined, 'HinhAnh/Reviews', img)"
                    class="img-fluid me-2 border border-light rounded-5"
                    style="max-width: 10em; height: 10em"
                    @click="openLightbox(getReviewImagesFullPath(item), idx)"
                  />
                </div>
                <blockquote
                  v-if="item.shopPhanHoi"
                  class="col-12"
                  style="border-left: 2px solid #ccc; padding-left: 10px; margin: 10px 0"
                >
                  <strong>Phản hồi của shop:</strong>
                  {{ item.shopPhanHoi }}
                </blockquote>
              </div>
            </div>
            <EmptySuggestBox
              v-else
              contentText="Bạn chưa có sản phẩm cần đánh giá nào. Hãy khám phá thêm sản phẩm!"
              linkNav="/shop"
            />
          </div>
        </div>
      </div>
    </section>
    <!-- Shop Section End -->
  </div>
  <VueEasyLight
    :visible="isLightboxOpen"
    :imgs="lightboxImages"
    :index="lightboxIndex"
    @hide="closeLightbox"
  />
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'
import EmptySuggestBox from '@/components/common/EmptySuggestBox.vue'
import { formatCurrency } from '@/constants/formatCurrency'
import Swal from 'sweetalert2'
import VueEasyLight from 'vue-easy-lightbox'

export default {
  name: 'CustomerReview',
  data() {
    return {
      activeTab: 'notReviewed',
      notReviewed: [],
      reviewed: [],
      pathReplaceImg,
      maxImages: 5,
      maxImageSize: 5 * 1024 * 1024, // 5MB
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
    }
  },
  components: { EmptySuggestBox, VueEasyLight },
  mounted() {
    this.loadFromCookieOrApi()
  },
  methods: {
    formatCurrency,
    loadFromCookieOrApi() {
      const cookie = document.cookie.split('; ').find((row) => row.startsWith('userReviews='))
      if (cookie) {
        try {
          const data = JSON.parse(decodeURIComponent(cookie.split('=')[1]))
          this.prepareData(data)
        } catch {
          this.reloadReviews()
        }
      } else {
        this.reloadReviews()
      }
    },
    async reloadReviews() {
      try {
        const res = await axiosConfig.getFromApi('/Review/users', ConfigsRequest.takeAuth())
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) {
          this.notReviewed = []
          this.reviewed = []
          return
        }
        document.cookie =
          'userReviews=' + encodeURIComponent(JSON.stringify(res.data)) + '; path=/;'
        this.prepareData(res.data)
      } catch {
        this.notReviewed = []
        this.reviewed = []
      }
    },
    prepareData(data) {
      this.notReviewed = (data.notReviewIn7days || []).map((item) => ({
        ...item,
        _editSoSao: 5,
        _editNoiDung: '',
        _selectedFiles: [],
        _previewImgs: [],
      }))
      this.reviewed = data.listReviewed || []
    },
    onImagesChange(e, item) {
      const files = Array.from(e.target.files)
      // Kiểm tra số lượng ảnh
      if (files.length > this.maxImages) {
        Swal.fire({
          icon: 'warning',
          title: 'Chỉ được chọn tối đa 5 ảnh!',
          timer: 2000,
          showConfirmButton: false,
        })
        e.target.value = ''
        return
      }
      // Kiểm tra dung lượng từng ảnh
      const oversize = files.find((f) => f.size > this.maxImageSize)
      if (oversize) {
        Swal.fire({
          icon: 'error',
          title: 'Ảnh vượt quá 5MB!',
          text: `Ảnh "${oversize.name}" vượt quá dung lượng cho phép.`,
        })
        e.target.value = ''
        return
      }
      // Xóa preview cũ
      if (item._previewImgs) item._previewImgs.forEach((url) => URL.revokeObjectURL(url))
      item._selectedFiles = files
      item._previewImgs = files.map((file) => URL.createObjectURL(file))
    },
    async submitReview(item) {
      try {
        if (!item._editNoiDung || !item._editSoSao) {
          Swal.fire({
            icon: 'warning',
            title: 'Vui lòng nhập nội dung và chọn số sao!',
            timer: 2000,
            showConfirmButton: false,
          })
          return
        }
        const formData = new FormData()
        formData.append('noiDung', item._editNoiDung)
        formData.append('soSao', item._editSoSao)
        if (item.maSp) formData.append('maSp', item.maSp)
        if (item.maCombo) formData.append('maCombo', item.maCombo)
        formData.append('maCtHd', item.maCthd)
        if (item._selectedFiles) {
          item._selectedFiles.forEach((file) => formData.append('hinhAnhs', file))
        }
        const res = await axiosConfig.postToApi(
          `/Review?isProduct=${!!item.maSp}`,
          formData,
          ConfigsRequest.takeAuth(),
        )
        if (ResponseAPI.handleNotificationAndIsFailResponse(res, true)) return
        Swal.fire({
          icon: 'success',
          title: 'Đã gửi đánh giá!',
          timer: 1200,
          showConfirmButton: false,
        })
        this.reloadReviews()
      } catch (e) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: e.message,
        })
      }
    },
    getImageCount(item) {
      return item._selectedFiles ? item._selectedFiles.length : 0
    },
    openLightbox(imgs, idx = 0) {
      this.lightboxImages = imgs
      this.lightboxIndex = idx
      this.isLightboxOpen = true
    },
    closeLightbox() {
      this.isLightboxOpen = false
    },
    getReviewImages(item) {
      // Trả về mảng tên file ảnh (không path)
      if (!item.hinhAnhs) return []
      return Array.isArray(item.hinhAnhs) ? item.hinhAnhs : item.hinhAnhs.split(',')
    },
    getReviewImagesFullPath(item) {
      // Trả về mảng path đầy đủ cho lightbox
      return this.getReviewImages(item).map((img) =>
        this.pathReplaceImg(undefined, 'HinhAnh/Reviews', img),
      )
    },
  },
}
</script>

<style scope>
* {
  color: black;
}
</style>
