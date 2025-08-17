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

    <section class="shop spad">
      <div class="container" style="min-height: 50vh">
        <div class="row">
          <div class="col-lg-12">
            <div class="d-flex align-items-center mb-4 position-relative">
              <ul class="nav nav-tabs flex-grow-1">
                <li class="nav-item">
                  <a class="nav-link" :class="{ active: activeTab === 'notReviewed' }" href="#"
                    @click.prevent="activeTab = 'notReviewed'">Chưa đánh giá
                    <span v-if="notReviewed.length" class="badge bg-primary ms-1">{{
                      notReviewed.length
                      }}</span></a>
                </li>
                <li class="nav-item">
                  <a class="nav-link" :class="{ active: activeTab === 'reviewed' }" href="#"
                    @click.prevent="activeTab = 'reviewed'">Đã đánh giá</a>
                </li>
              </ul>
              <div class="ms-3">
                <button class="btn btn-outline-secondary" @click="reloadReviews" :disabled="isLoading"
                  title="Tải lại danh sách đánh giá">
                  <i class="fa fa-refresh" :class="{ 'fa-spin': isLoading }"></i>
                </button>
              </div>
            </div>

            <div v-if="isLoading" class="text-center my-5">
              <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
              </div>
              <p class="mt-2">Đang tải dữ liệu...</p>
            </div>

            <!-- Tab Chưa đánh giá -->
            <div v-if="!isLoading && activeTab === 'notReviewed'">
              <div v-if="notReviewed.length" class="orders-list">
                <div v-for="orderGroup in notReviewed" :key="orderGroup.maHd" class="order-card">
                  <!-- Order Header -->
                  <div class="order-header">
                    <div class="order-info">
                      <h3 class="order-id">Đơn hàng #{{ orderGroup.maHd }}</h3>
                      <p class="order-date">{{ formatDate(orderGroup.ngayTao) }}</p>
                    </div>
                    <div class="order-status">
                      <span :class="['status-badge', getStatusClass(orderGroup.tinhTrang)]">
                        <i :class="getStatusIcon(orderGroup.tinhTrang)"></i>
                        {{ orderGroup.tinhTrang }}
                      </span>
                    </div>
                  </div>

                  <!-- Order Items Preview -->
                  <div class="order-items">
                    <div v-for="item in orderGroup.items" :key="item.maCthd"
                      class="order-item p-3 border border-rounded mb-2">
                      <div class="item-details row">
                        <div class="item-image col-4">
                          <img :src="item.hinhAnhUrl" :alt="item.tenDoiTuong" />
                        </div>
                        <div class="col-8">

                          <div class="product-review-summary">
                            <h5 class="">
                              <RouterLink :to="item.maSp ? '/product/' + item.maSp : '/combo/' + item.maCombo"
                                class="text-decoration-none">
                                {{ item.tenDoiTuong }}
                              </RouterLink>
                            </h5>
                            <p class="item-variant" v-if="item.kichThuoc || item.mauSac">
                              <span v-if="item.kichThuoc">Size: {{ item.kichThuoc }}</span>
                              <span v-if="item.kichThuoc && item.mauSac"> | </span>
                              <span v-if="item.mauSac">Màu: {{ item.mauSac }}</span>
                            </p>
                            <!-- <p class="item-quantity">x{{ item.soLuong }}</p> -->
                          </div>
                        </div>

                        <div class="review-input-section">
                          <div class="mb-3 mt-3">
                            <label class="form-label fw-bold">Đánh giá của bạn:</label>
                            <StarRating :rating="item._editSoSao" :read-only="false"
                              @update:rating="item._editSoSao = $event" />
                            <textarea v-model.trim="item._editNoiDung" class="form-control" rows="3"
                              placeholder="Sản phẩm dùng có tốt không? Bạn có hài lòng không? Hãy chia sẻ cảm nhận của bạn tại đây nhé."></textarea>
                          </div>

                          <div class="mb-3">
                            <label class="form-label fw-bold">Hình ảnh kèm theo:</label>
                            <input type="file" multiple accept="image/*" class="form-control"
                              :disabled="getImageCount(item) >= maxImages" @change="onImagesChange($event, item)" />
                            <small class="form-text text-muted">Tối đa {{ maxImages }} ảnh, mỗi ảnh không quá
                              5MB.</small>
                            <div v-if="item._previewImgs && item._previewImgs.length" class="d-flex flex-wrap mt-2">
                              <div v-for="(img, idx) in item._previewImgs" :key="idx"
                                class="position-relative me-2 mb-2">
                                <img :src="img" class="img-fluid border rounded"
                                  style="width: 100px; height: 100px; object-fit: cover"
                                  @click="openLightbox(item._previewImgs, idx)" />
                                <button class="btn btn-sm btn-danger position-absolute top-0 end-0"
                                  @click="removePreviewImage(item, idx)">
                                  &times;
                                </button>
                              </div>
                            </div>
                          </div>

                          <button class="btn btn-primary" @click="submitReview(item)" :disabled="item._isSubmitting">
                            <span v-if="item._isSubmitting" class="spinner-border spinner-border-sm" role="status"
                              aria-hidden="true"></span>
                            Gửi đánh giá
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                  <!-- No "more items" indicator for review page as all items are listed for review -->
                </div>
              </div>
              <EmptySuggestBox v-else contentText="Bạn không có sản phẩm nào cần đánh giá. Hãy mua sắm thêm nhé!"
                linkNav="/shop" />
            </div>

            <!-- Tab Đã đánh giá -->
            <div v-if="!isLoading && activeTab === 'reviewed'">
              <div v-if="reviewed.length" class="orders-list">
                <div v-for="orderGroup in reviewed" :key="orderGroup.maHd" class="order-card">
                  <!-- Order Header -->
                  <div class="order-header">
                    <div class="order-info">
                      <h3 class="order-id">Đơn hàng #{{ orderGroup.maHd }}</h3>
                      <p class="order-date">{{ formatDate(orderGroup.ngayTao) }}</p>
                    </div>
                    <div class="order-status">
                      <span :class="['status-badge', getStatusClass(orderGroup.tinhTrang)]">
                        <i :class="getStatusIcon(orderGroup.tinhTrang)"></i>
                        {{ orderGroup.tinhTrang }}
                      </span>
                    </div>
                  </div>

                  <!-- Order Items Preview -->
                  <div class="order-items">
                    <div v-for="item in orderGroup.items" :key="item.id" class="order-item">
                      <div class="item-details p-3 border border-rounded mb-2 row">
                        <div class="item-image col-4">
                          <img :src="item.hinhAnhUrl" :alt="item.tenDoiTuong" class="img-fluid" />
                        </div>
                        <div class="col-8">
                          <div class="product-review-summary">
                            <h5 class="">
                              <RouterLink :to="item.maSp ? '/product/' + item.maSp : '/combo/' + item.maCombo"
                                class="text-decoration-none">
                                {{ item.tenDoiTuong }}
                              </RouterLink>
                            </h5>
                            <p class="item-variant" v-if="item.kichThuoc || item.mauSac">
                              <span v-if="item.kichThuoc">Size: {{ item.kichThuoc }}</span>
                              <span v-if="item.kichThuoc && item.mauSac"> | </span>
                              <span v-if="item.mauSac">Màu: {{ item.mauSac }}</span>
                            </p>
                            <StarRating :rating="item.soSao" />
                          </div>
                        </div>

                        <div class="review-display-section">
                          <p class="mb-2">{{ item.noiDung }}</p>
                          <div v-if="item.hinhAnhs && item.hinhAnhs.length">

                            <b>Hình ảnh đánh giá</b>
                            <br>
                          </div>
                          <div v-if="item.hinhAnhs && item.hinhAnhs.length" class="d-flex flex-wrap my-2">
                            <img v-for="(img, idx) in item.hinhAnhs" :key="idx" :src="img"
                              class="img-fluid me-2 mb-2 border rounded"
                              style="width: 100px; height: 100px; object-fit: cover"
                              @click="openLightbox(item.hinhAnhs, idx)" />
                          </div>
                          <blockquote v-if="item.shopPhanHoi" class="blockquote bg-light p-2 rounded mt-2">
                            <p class="mb-0 small">
                              <strong>Phản hồi từ Shop:</strong> {{ item.shopPhanHoi }}
                            </p>
                          </blockquote>
                          <button v-if="!item._isEditing" class="btn btn-sm btn-outline-primary mt-2"
                            @click="toggleEditMode(item)">
                            <i class="fa fa-edit"></i> Chỉnh sửa
                          </button>

                          <div v-if="item._isEditing" class="mt-3 p-3 border rounded bg-light">
                            <h6 class="mb-3">Chỉnh sửa đánh giá</h6>
                            <div class="mb-3">
                              <label class="form-label fw-bold">Số sao:</label>
                              <StarRating :rating="item._editSoSao" :read-only="false"
                                @update:rating="item._editSoSao = $event" />
                            </div>
                            <div class="mb-3">
                              <label class="form-label fw-bold">Nội dung:</label>
                              <textarea v-model.trim="item._editNoiDung" class="form-control" rows="3"></textarea>
                            </div>
                            <div class="mb-3">
                              <label class="form-label fw-bold">Hình ảnh:</label>
                              <input type="file" multiple accept="image/*" class="form-control"
                                :disabled="getImageCount(item) >= maxImages"
                                @change="onImagesChangeForEdit($event, item)" />
                              <small class="form-text text-muted">Tối đa {{ maxImages }} ảnh, mỗi ảnh không quá
                                5MB.</small>
                              <div v-if="item._previewImgs && item._previewImgs.length" class="d-flex flex-wrap mt-2">
                                <div v-for="(img, idx) in item._previewImgs" :key="idx"
                                  class="position-relative me-2 mb-2">
                                  <img :src="img" class="img-fluid border rounded"
                                    style="width: 100px; height: 100px; object-fit: cover"
                                    @click="openLightbox(item._previewImgs, idx)" />
                                  <button class="btn btn-sm btn-danger position-absolute top-0 end-0"
                                    @click="removePreviewImageForEdit(item, idx)">
                                    &times;
                                  </button>
                                </div>
                              </div>
                            </div>
                            <button class="btn btn-primary me-2" @click="updateReview(item)"
                              :disabled="item._isSubmitting">
                              <span v-if="item._isSubmitting" class="spinner-border spinner-border-sm" role="status"
                                aria-hidden="true"></span>
                              Lưu thay đổi
                            </button>
                            <button class="btn btn-secondary" @click="cancelEdit(item)">Hủy</button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <EmptySuggestBox v-else contentText="Bạn chưa đánh giá sản phẩm nào." linkNav="/shop" />
            </div>
          </div>
        </div>
      </div>
    </section>
    <VueEasyLightbox :visible="isLightboxOpen" :imgs="lightboxImages" :index="lightboxIndex" @hide="closeLightbox" />
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import * as axiosConfig from '@/utils/axiosClient'
import ResponseAPI from '@/models/ResponseAPI'
import pathReplaceImg from '@/utils/processPathImg'
import EmptySuggestBox from '@/components/common/EmptySuggestBox.vue'

import Swal from 'sweetalert2'
import VueEasyLightbox from 'vue-easy-lightbox'
import StarRating from '@/components/common/StarRating.vue'

export default {
  name: 'CustomerReview',
  components: { EmptySuggestBox, VueEasyLightbox, StarRating },
  data() {
    return {
      activeTab: 'notReviewed',
      notReviewed: [],
      reviewed: [],
      isLoading: true,
      maxImages: 5,
      maxImageSize: 5 * 1024 * 1024, // 5MB
      isLightboxOpen: false,
      lightboxImages: [],
      lightboxIndex: 0,
    }
  },
  created() {
    this.reloadReviews()
  },
  methods: {
    formatDate(dateStr) {
      if (!dateStr) return ''
      return new Date(dateStr).toLocaleString('vi-VN')
    },
    getStatusClass(status) {
      const statusLower = status.toLowerCase()
      if (statusLower.includes('chờ') || statusLower.includes('xử lý')) return 'status-processing'
      if (statusLower.includes('giao') && !statusLower.includes('đã nhận')) return 'status-shipping'
      if (statusLower.includes('đã nhận') || statusLower.includes('đã thanh toán')) return 'status-delivered'
      if (statusLower.includes('hủy') || statusLower.includes('hoàn trả')) return 'status-cancelled'
      return 'status-default'
    },
    getStatusIcon(status) {
      const statusLower = status.toLowerCase()
      if (statusLower.includes('chờ') || statusLower.includes('xử lý')) return 'fas fa-clock'
      if (statusLower.includes('giao') && !statusLower.includes('đã nhận')) return 'fas fa-truck'
      if (statusLower.includes('đã nhận') || statusLower.includes('đã thanh toán')) return 'fas fa-check-circle'
      if (statusLower.includes('hủy') || statusLower.includes('hoàn trả')) return 'fas fa-times-circle'
      return 'fas fa-box'
    },
    async reloadReviews() {
      this.isLoading = true
      try {
        const res = await axiosConfig.getFromApi('/Review/users', ConfigsRequest.takeAuth())
        if (ResponseAPI.handleNotificationAndIsFailResponse(res)) {
          this.notReviewed = []
          this.reviewed = []
          return
        }
        this.prepareData(res.data)
      } catch (error) {
        console.error('Failed to load reviews:', error)
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Không thể tải danh sách đánh giá. Vui lòng thử lại sau.',
        })
        this.notReviewed = []
        this.reviewed = []
      } finally {
        this.isLoading = false
      }
    },
    prepareData(data) {
      this.notReviewed = (data.notReviewIn7days || []).map((orderGroup) => ({
        ...orderGroup,
        items: orderGroup.items.map((item) => ({
          ...item,
          hinhAnhUrl: pathReplaceImg(undefined, 'HinhAnh/Products', item.tenHinhAnh),
          _editSoSao: 5,
          _editNoiDung: '',
          _selectedFiles: [],
          _previewImgs: [],
          _isSubmitting: false,
        })),
      }));
      this.reviewed = (data.listReviewed || []).map((orderGroup) => ({
        ...orderGroup,
        items: orderGroup.items.map((item) => {
          let hinhAnhs = [];
          if (Array.isArray(item.hinhAnhs)) {
            hinhAnhs = item.hinhAnhs;
          } else if (typeof item.hinhAnhs === 'string') {
            hinhAnhs = item.hinhAnhs.split(',').filter(Boolean);
          }

          return {
            ...item,
            hinhAnhUrl: pathReplaceImg(undefined, 'HinhAnh/Products', item.tenHinhAnh),
            hinhAnhs: hinhAnhs.map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img)),
            _isEditing: false,
            _editSoSao: item.soSao,
            _editNoiDung: item.noiDung,
            _selectedFiles: [],
            _previewImgs: hinhAnhs.map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img)),
            _removedImageUrls: [],
            _isSubmitting: false,
          };
        }),
      }));
    },
    onImagesChange(event, item) {
      const files = Array.from(event.target.files)
      if (!files.length) return

      const nonImageFiles = files.filter((file) => !file.type.startsWith('image/'))
      if (nonImageFiles.length > 0) {
        Swal.fire(
          'Loại tệp không hợp lệ',
          `Các tệp sau không phải là hình ảnh: ${nonImageFiles.map((f) => f.name).join(', ')}`,
          'error',
        )
        event.target.value = ''
        return
      }

      const totalImages = (item._selectedFiles?.length || 0) + files.length
      if (totalImages > this.maxImages) {
        Swal.fire(
          'Số lượng ảnh vượt quá giới hạn',
          `Bạn chỉ có thể tải lên tối đa ${this.maxImages} ảnh.`,
          'warning',
        )
        event.target.value = ''
        return
      }

      const oversizedFiles = files.filter((file) => file.size > this.maxImageSize)
      if (oversizedFiles.length > 0) {
        Swal.fire(
          'Kích thước ảnh quá lớn',
          `Các ảnh sau vượt quá dung lượng ${this.maxImageSize / 1024 / 1024
          }MB: ${oversizedFiles.map((f) => f.name).join(', ')}`,
          'error',
        )
        event.target.value = ''
        return
      }

      item._selectedFiles.push(...files)
      item._previewImgs.push(...files.map((file) => URL.createObjectURL(file)))
      event.target.value = '' // Reset input for next selection
    },
    removePreviewImage(item, index) {
      item._selectedFiles.splice(index, 1)
      const removedUrl = item._previewImgs.splice(index, 1)[0]
      URL.revokeObjectURL(removedUrl)
    },
    async submitReview(item) {
      if (!item._editSoSao) {
        Swal.fire('Chưa chọn sao', 'Vui lòng chọn số sao để đánh giá.', 'warning')
        return
      }
      if (!item._editNoiDung) {
        Swal.fire('Chưa nhập nội dung', 'Vui lòng chia sẻ cảm nhận của bạn.', 'warning')
        return
      }
      item._isSubmitting = true
      try {
        const formData = new FormData()
        formData.append('noiDung', item._editNoiDung)
        formData.append('soSao', item._editSoSao)
        if (item.maSp) formData.append('maSp', item.maSp)
        if (item.maCombo) formData.append('maCombo', item.maCombo)
        formData.append('maCtHd', item.maCthd)
        item._selectedFiles.forEach((file) => {
          formData.append('hinhAnhs', file)
        })

        const res = await axiosConfig.postToApi(
          `/Review?isProduct=${!!item.maSp}`,
          formData,
          ConfigsRequest.takeAuth(),
        )

        if (!res.success) {
          Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: res.message,
          })
          return
        }

        Swal.fire('Thành công!', 'Cảm ơn bạn đã gửi đánh giá.', 'success')
        await this.reloadReviews()
      } catch (error) {
        console.error('Submit review failed:', error)
        Swal.fire('Gửi thất bại', 'Đã có lỗi xảy ra, vui lòng thử lại.', 'error')
      } finally {
        item._isSubmitting = false
      }
    },
    getImageCount(item) {
      return item._selectedFiles?.length || 0
    },
    openLightbox(imgs, index = 0) {
      this.lightboxImages = imgs
      this.lightboxIndex = index
      this.isLightboxOpen = true
    },
    closeLightbox() {
      this.isLightboxOpen = false;
    },
    toggleEditMode(item) {
      item._isEditing = !item._isEditing;
      // Reset edit data if canceling edit
      if (!item._isEditing) {
        item._editSoSao = item.soSao;
        item._editNoiDung = item.noiDung;
        item._selectedFiles = [];
        item._previewImgs = item.hinhAnhs.map((img) => pathReplaceImg(undefined, 'HinhAnh/Reviews', img));
        item._removedImageUrls = [];
      }
    },
    cancelEdit(item) {
      this.toggleEditMode(item);
    },
    onImagesChangeForEdit(event, item) {
      const files = Array.from(event.target.files);
      if (!files.length) return;

      const nonImageFiles = files.filter((file) => !file.type.startsWith('image/'));
      if (nonImageFiles.length > 0) {
        Swal.fire(
          'Loại tệp không hợp lệ',
          `Các tệp sau không phải là hình ảnh: ${nonImageFiles.map((f) => f.name).join(', ')}`,
          'error',
        );
        event.target.value = '';
        return;
      }

      const totalImages = (item._selectedFiles?.length || 0) + item._previewImgs.length + files.length;
      if (totalImages > this.maxImages) {
        Swal.fire(
          'Số lượng ảnh vượt quá giới hạn',
          `Bạn chỉ có thể tải lên tối đa ${this.maxImages} ảnh.`,
          'warning',
        );
        event.target.value = '';
        return;
      }

      const oversizedFiles = files.filter((file) => file.size > this.maxImageSize);
      if (oversizedFiles.length > 0) {
        Swal.fire(
          'Kích thước ảnh quá lớn',
          `Các ảnh sau vượt quá dung lượng ${this.maxImageSize / 1024 / 1024
          }MB: ${oversizedFiles.map((f) => f.name).join(', ')}`,
          'error',
        );
        event.target.value = '';
        return;
      }

      item._selectedFiles.push(...files);
      item._previewImgs.push(...files.map((file) => URL.createObjectURL(file)));
      event.target.value = ''; // Reset input for next selection
    },
    removePreviewImageForEdit(item, index) {
      const removedUrl = item._previewImgs.splice(index, 1)[0];
      // Check if the removed image was an existing one (not a newly selected file)
      if (!removedUrl.startsWith('blob:')) {
        item._removedImageUrls.push(removedUrl);
      } else {
        // If it was a new file, remove it from _selectedFiles as well
        const fileIndex = item._selectedFiles.findIndex(file => URL.createObjectURL(file) === removedUrl);
        if (fileIndex > -1) {
          item._selectedFiles.splice(fileIndex, 1);
        }
        URL.revokeObjectURL(removedUrl);
      }
    },
    async updateReview(item) {
      if (!item._editSoSao) {
        Swal.fire('Chưa chọn sao', 'Vui lòng chọn số sao để đánh giá.', 'warning');
        return;
      }
      if (!item._editNoiDung) {
        Swal.fire('Chưa nhập nội dung', 'Vui lòng chia sẻ cảm nhận của bạn.', 'warning');
        return;
      }
      item._isSubmitting = true;
      try {
        const formData = new FormData();
        formData.append('maDg', item.maDg);
        formData.append('noiDung', item._editNoiDung);
        formData.append('soSao', item._editSoSao);
        if (item.maSp) formData.append('maSp', item.maSp);
        if (item.maCombo) formData.append('maCombo', item.maCombo);
        formData.append('maCtHd', item.maCthd);

        item._selectedFiles.forEach((file) => {
          formData.append('hinhAnhs', file);
        });

        // Append removed image URLs to send to API
        item._removedImageUrls.forEach((url) => {
          formData.append('removedImageUrls', url);
        });

        const res = await axiosConfig.putToApi(
          `/Review?isProduct=${!!item.maSp}`,
          formData,
          ConfigsRequest.takeAuth(),
        );

        if (!res.success) {
          Swal.fire({
            icon: 'error',
            title: 'Lỗi',
            text: res.message,
          });
          return;
        }

        Swal.fire('Thành công!', 'Đánh giá của bạn đã được cập nhật.', 'success');
        await this.reloadReviews();
      } catch (error) {
        console.error('Update review failed:', error);
        Swal.fire('Cập nhật thất bại', 'Đã có lỗi xảy ra, vui lòng thử lại.', 'error');
      } finally {
        item._isSubmitting = false;
        item._isEditing = false; // Exit edit mode
      }
    },
  },
};
</script>

<style scoped>
.nav-tabs .nav-link {
  color: #666;
  border-bottom: 2px solid transparent;
}

.nav-tabs .nav-link.active {
  color: #007bff;
  border-color: #007bff;
  font-weight: bold;
}


/* Orders Content */
.orders-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem 2rem;
}

/* Status Tabs */
.status-tabs {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 0.25rem;
  background: white;
  padding: 0.25rem;
  border-radius: 0.75rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-button {
  padding: 0.75rem 0.5rem;
  border: none;
  background: transparent;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
}

.tab-button:hover {
  background-color: #f1f5f9;
  color: #334155;
}

.tab-button.active {
  background-color: #3b82f6;
  color: white;
}

/* Order Cards */
.orders-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-card {
  background: white;
  border-radius: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  transition: all 0.2s;
}

.order-card:hover {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.25rem 1.25rem 0.75rem;
}

.order-info h3.order-id {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 0.25rem 0;
}

.order-date {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.375rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.025em;
}

.status-processing {
  background-color: #fef3c7;
  color: #d97706;
}

.status-shipping {
  background-color: #dbeafe;
  color: #2563eb;
}

.status-delivered {
  background-color: #dcfce7;
  color: #16a34a;
}

.status-cancelled {
  background-color: #fee2e2;
  color: #dc2626;
}

.status-default {
  background-color: #f1f5f9;
  color: #64748b;
}

/* Order Items */
.order-items {
  padding: 0 1.25rem;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.order-item:last-child {
  border-bottom: none;
}

.item-image {
  width: 3rem;
  height: 3rem;
  flex-shrink: 0;
}

.item-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 0.5rem;
  background-color: #f1f5f9;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-name {
  font-weight: 500;
  color: #1e293b;
  margin: 0 0 0.125rem 0;
  font-size: 0.875rem;
}

.item-variant {
  font-size: 0.75rem;
  color: #64748b;
  margin: 0 0 0.125rem 0;
}

.item-quantity {
  font-size: 0.75rem;
  color: #64748b;
  margin: 0;
}

.item-price {
  font-weight: 600;
  color: #1e293b;
  font-size: 0.875rem;
}

.more-items {
  padding: 0.5rem 0;
  font-size: 0.75rem;
  color: #64748b;
  text-align: center;
  font-style: italic;
}

/* Order Footer */
.order-footer {
  padding: 1rem 1.25rem 1.25rem;
  border-top: 1px solid #f1f5f9;
  background-color: #fafbfc;
}

.order-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.total-label {
  font-weight: 500;
  color: #64748b;
}

.total-amount {
  font-weight: 700;
  color: #3b82f6;
  font-size: 1.125rem;
}

.order-actions {
  display: flex;
  gap: 0.75rem;
}

.action-btn {
  flex: 1;
  padding: 0.625rem 1rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.375rem;
}

.details-btn {
  background-color: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.details-btn:hover {
  background-color: #2563eb;
  border-color: #2563eb;
}

.cancel-btn {
  background-color: transparent;
  color: #dc2626;
  border-color: #dc2626;
}

.cancel-btn:hover {
  background-color: #dc2626;
  color: white;
}

.rate-btn {
  background-color: transparent;
  color: #f59e0b;
  border-color: #f59e0b;
}

.rate-btn:hover {
  background-color: #f59e0b;
  color: white;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 4rem 1rem;
}

.empty-icon {
  font-size: 4rem;
  color: #d1d5db;
  margin-bottom: 1rem;
}

.empty-state h3 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 0.5rem 0;
}

.empty-state p {
  color: #6b7280;
  margin: 0;
}

/* Pagination */
.pagination-nav {
  margin-top: 2rem;
  display: flex;
  justify-content: center;
}

.pagination {
  display: flex;
  list-style: none;
  gap: 0.25rem;
  margin: 0;
  padding: 0;
}

.page-item {
  display: flex;
}

.page-link {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  background-color: white;
  color: #374151;
  text-decoration: none;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
}

.page-link:hover:not(:disabled) {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.page-item.active .page-link {
  background-color: #3b82f6;
  border-color: #3b82f6;
  color: white;
}

.page-item.disabled .page-link {
  color: #9ca3af;
  cursor: not-allowed;
}

/* Modals */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
  overflow-y: auto;
}

.modal-content {
  background: white;
  border-radius: 0.75rem;
  width: 100%;
  max-width: 4xl;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
}

.cancel-modal-content {
  background: white;
  border-radius: 0.75rem;
  width: 100%;
  max-width: 28rem;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 1.5rem 1rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
}

.modal-close {
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 0.375rem;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
}

.modal-close:hover {
  background-color: #f3f4f6;
  color: #374151;
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem 1.5rem;
  border-top: 1px solid #e5e7eb;
}

/* Order Summary Grid */
.order-summary-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  margin-bottom: 2rem;
}

.summary-section h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
}

.info-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
}

.info-item .label {
  font-weight: 500;
  color: #6b7280;
  flex-shrink: 0;
}

.info-item .value {
  text-align: right;
  color: #1f2937;
}

.total-row {
  padding-top: 0.75rem;
  border-top: 1px solid #e5e7eb;
  font-weight: 600;
}

.total-price {
  font-size: 1.25rem;
  color: #3b82f6;
}

.status-text.status-delivered {
  color: #16a34a;
}

.status-text.status-processing {
  color: #d97706;
}

.status-text.status-shipping {
  color: #2563eb;
}

.status-text.status-cancelled {
  color: #dc2626;
}

/* Products Section */
.products-section h4,
.combos-section h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 1rem 0;
}

.products-table,
.combos-table {
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  overflow: hidden;
}

.table-header {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  background-color: #f9fafb;
  padding: 0.75rem;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
  border-bottom: 1px solid #e5e7eb;
}

.table-header.combo-header {
  grid-template-columns: 3fr 1fr 1fr 1fr;
}

.table-row {
  display: grid;
  grid-template-columns: 2fr 1fr 1fr 1fr;
  padding: 1rem 0.75rem;
  border-bottom: 1px solid #f3f4f6;
  align-items: center;
}

.table-row.combo-row {
  grid-template-columns: 3fr 1fr 1fr 1fr;
}

.table-row:last-child {
  border-bottom: none;
}

.col-product,
.col-combo {
  font-size: 0.875rem;
}

.product-info strong,
.combo-info strong {
  color: #1f2937;
  display: block;
  margin-bottom: 0.25rem;
}

.product-variant {
  font-size: 0.75rem;
  color: #6b7280;
}

.combo-items {
  margin-top: 0.5rem;
}

.combo-item {
  font-size: 0.75rem;
  color: #6b7280;
  margin-bottom: 0.25rem;
}

.combo-quantity {
  color: #9ca3af;
}

.col-price {
  text-align: center;
}

.current-price {
  font-weight: 600;
  color: #1f2937;
  display: block;
}

.original-price {
  font-size: 0.75rem;
  color: #ef4444;
  text-decoration: line-through;
}

.col-quantity,
.col-total {
  text-align: center;
  font-weight: 500;
  color: #1f2937;
}

/* Cancel Modal Specific */
.cancel-description {
  color: #6b7280;
  margin-bottom: 1.5rem;
  line-height: 1.5;
}

.reason-options {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.reason-option {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  padding: 0.75rem;
  border-radius: 0.5rem;
  transition: background-color 0.2s;
}

.reason-option:hover {
  background-color: #f9fafb;
}

.reason-option input[type="radio"] {
  margin: 0;
  accent-color: #3b82f6;
}

.reason-text {
  color: #374151;
  font-size: 0.875rem;
  line-height: 1.5;
}

.custom-reason {
  margin-top: 1rem;
}

.custom-reason-input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  resize: vertical;
  transition: border-color 0.2s;
}

.custom-reason-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Buttons */
.btn {
  padding: 0.625rem 1.25rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-secondary {
  background-color: #f3f4f6;
  color: #374151;
  border-color: #d1d5db;
}

.btn-secondary:hover {
  background-color: #e5e7eb;
  border-color: #9ca3af;
}

.btn-danger {
  background-color: #dc2626;
  color: white;
  border-color: #dc2626;
}

.btn-danger:hover {
  background-color: #b91c1c;
  border-color: #b91c1c;
}

/* Responsive Design */
@media (max-width: 768px) {
  .orders-title {
    font-size: 1.25rem;
  }

  .status-tabs {
    grid-template-columns: repeat(5, 1fr);
  }

  .tab-button {
    padding: 0.5rem 0.25rem;
    font-size: 0.75rem;
  }

  .order-header {
    padding: 1rem 1rem 0.5rem;
  }

  .order-items {
    padding: 0 1rem;
  }

  .order-footer {
    padding: 0.75rem 1rem 1rem;
  }

  .order-actions {
    flex-direction: column;
    gap: 0.5rem;
  }

  .item-image {
    width: 2.5rem;
    height: 2.5rem;
  }

  .order-summary-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }

  .table-header,
  .table-row {
    grid-template-columns: 2fr 1fr 1fr;
    font-size: 0.75rem;
  }

  .col-total {
    display: none;
  }

  .modal-content {
    margin: 1rem;
    max-height: calc(100vh - 2rem);
  }
}

@media (max-width: 640px) {
  .header-content {
    padding: 0 0.75rem;
  }

  .search-filter-section,
  .orders-content {
    padding-left: 0.75rem;
    padding-right: 0.75rem;
  }

  .item-name {
    font-size: 0.75rem;
  }

  .item-variant,
  .item-quantity {
    font-size: 0.625rem;
  }

  .item-price {
    font-size: 0.75rem;
  }
}
</style>
