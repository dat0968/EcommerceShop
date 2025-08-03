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
                  <a
                    class="nav-link"
                    :class="{ active: activeTab === 'notReviewed' }"
                    href="#"
                    @click.prevent="activeTab = 'notReviewed'"
                    >Chưa đánh giá
                    <span v-if="notReviewed.length" class="badge bg-primary ms-1">{{
                      notReviewed.length
                    }}</span></a
                  >
                </li>
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: activeTab === 'reviewed' }"
                    href="#"
                    @click.prevent="activeTab = 'reviewed'"
                    >Đã đánh giá</a
                  >
                </li>
              </ul>
              <div class="ms-3">
                <button
                  class="btn btn-outline-secondary"
                  @click="reloadReviews"
                  :disabled="isLoading"
                  title="Tải lại danh sách đánh giá"
                >
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
              <div v-if="notReviewed.length">
                <div v-for="orderGroup in notReviewed" :key="orderGroup.maHd" class="mb-5">
                  <h5 class="mb-3 bg-info text-white p-2 rounded">
                    Đơn hàng #{{ orderGroup.maHd }} - Ngày đặt:
                    {{ new Date(orderGroup.ngayTao).toLocaleDateString('vi-VN') }} - Trạng thái:
                    {{ orderGroup.tinhTrang }}
                  </h5>
                  <div
                    v-for="item in orderGroup.items"
                    :key="item.maCthd"
                    class="border rounded p-3 mb-4 shadow-sm review-item"
                  >
                    <div class="row">
                      <div class="col-md-2">
                        <img
                          :src="item.hinhAnhUrl"
                          alt="Ảnh sản phẩm"
                          class="img-fluid rounded border"
                          style="width: 100%; height: 150px; object-fit: cover; cursor: pointer"
                          @click="openLightbox([item.hinhAnhUrl], 0)"
                        />
                      </div>
                      <div class="col-md-10">
                        <h5 class="mb-1">
                          <RouterLink :to="item.maSp ? '/product/' + item.maSp : '/combo/' + item.maCombo">
                            {{ item.tenDoiTuong }}
                          </RouterLink>
                        </h5>
                        <p class="text-muted small mb-2">
                          <strong>{{ item.maSp ? 'Sản phẩm' : 'Combo' }}:</strong>
                          {{ item.maSp || item.maCombo }}
                          <span v-if="item.kichThuoc">| Size: {{ item.kichThuoc }}</span>
                          <span v-if="item.mauSac">| Màu: {{ item.mauSac }}</span>
                        </p>

                        <div class="mb-3">
                          <label class="form-label fw-bold">Đánh giá của bạn:</label>
                          <StarRating :rating="item._editSoSao" :read-only="false" @update:rating="item._editSoSao = $event" />
                          <textarea
                            v-model.trim="item._editNoiDung"
                            class="form-control"
                            rows="3"
                            placeholder="Sản phẩm dùng có tốt không? Bạn có hài lòng không? Hãy chia sẻ cảm nhận của bạn tại đây nhé."
                          ></textarea>
                        </div>

                        <div class="mb-3">
                          <label class="form-label fw-bold">Hình ảnh kèm theo:</label>
                          <input
                            type="file"
                            multiple
                            accept="image/*"
                            class="form-control"
                            :disabled="getImageCount(item) >= maxImages"
                            @change="onImagesChange($event, item)"
                          />
                          <small class="form-text text-muted"
                            >Tối đa {{ maxImages }} ảnh, mỗi ảnh không quá 5MB.</small
                          >
                          <div
                            v-if="item._previewImgs && item._previewImgs.length"
                            class="d-flex flex-wrap mt-2"
                          >
                            <div
                              v-for="(img, idx) in item._previewImgs"
                              :key="idx"
                              class="position-relative me-2 mb-2"
                            >
                              <img
                                :src="img"
                                class="img-fluid border rounded"
                                style="width: 100px; height: 100px; object-fit: cover"
                                @click="openLightbox(item._previewImgs, idx)"
                              />
                              <button
                                class="btn btn-sm btn-danger position-absolute top-0 end-0"
                                @click="removePreviewImage(item, idx)"
                              >
                                &times;
                              </button>
                            </div>
                          </div>
                        </div>

                        <button
                          class="btn btn-primary"
                          @click="submitReview(item)"
                          :disabled="item._isSubmitting"
                        >
                          <span
                            v-if="item._isSubmitting"
                            class="spinner-border spinner-border-sm"
                            role="status"
                            aria-hidden="true"
                          ></span>
                          Gửi đánh giá
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <EmptySuggestBox
                v-else
                contentText="Bạn không có sản phẩm nào cần đánh giá. Hãy mua sắm thêm nhé!"
                linkNav="/shop"
              />
            </div>

            <!-- Tab Đã đánh giá -->
            <div v-if="!isLoading && activeTab === 'reviewed'">
              <div v-if="reviewed.length">
                <div v-for="orderGroup in reviewed" :key="orderGroup.maHd" class="mb-5">
                  <h5 class="mb-3 bg-info text-white p-2 rounded">
                    Đơn hàng #{{ orderGroup.maHd }} - Ngày đặt:
                    {{ new Date(orderGroup.ngayTao).toLocaleDateString('vi-VN') }} - Trạng thái:
                    {{ orderGroup.tinhTrang }}
                  </h5>
                  <div
                    v-for="item in orderGroup.items"
                    :key="item.id"
                    class="border rounded p-3 mb-4 shadow-sm review-item"
                  >
                    <div class="d-flex align-items-start">
                      <img
                        :src="item.hinhAnhUrl"
                        alt="Ảnh sản phẩm"
                        class="img-fluid rounded border me-3"
                        style="width: 80px; height: 80px; object-fit: cover; cursor: pointer"
                        @click="openLightbox([item.hinhAnhUrl], 0)"
                      />
                      <div class="flex-grow-1">
                        <h6 class="mb-1">
                          <RouterLink :to="item.maSp ? '/product/' + item.maSp : '/combo/' + item.maCombo">
                            {{ item.tenDoiTuong }}
                          </RouterLink>
                        </h6>
                        <StarRating :rating="item.soSao" />
                        <p class="mb-2">{{ item.noiDung }}</p>
                        <div
                          v-if="item.hinhAnhs && item.hinhAnhs.length"
                          class="d-flex flex-wrap my-2"
                        >
                          <img
                            v-for="(img, idx) in item.hinhAnhs"
                            :key="idx"
                            :src="img"
                            class="img-fluid me-2 mb-2 border rounded"
                            style="width: 100px; height: 100px; object-fit: cover; cursor: pointer"
                            @click="openLightbox(item.hinhAnhs, idx)"
                          />
                        </div>
                        <blockquote
                          v-if="item.shopPhanHoi"
                          class="blockquote bg-light p-2 rounded mt-2"
                        >
                          <p class="mb-0 small">
                            <strong>Phản hồi từ Shop:</strong> {{ item.shopPhanHoi }}
                          </p>
                        </blockquote>
                        <button
                          v-if="!item._isEditing"
                          class="btn btn-sm btn-outline-primary mt-2"
                          @click="toggleEditMode(item)"
                        >
                          <i class="fa fa-edit"></i> Chỉnh sửa
                        </button>

                        <div v-if="item._isEditing" class="mt-3 p-3 border rounded bg-light">
                          <h6 class="mb-3">Chỉnh sửa đánh giá</h6>
                          <div class="mb-3">
                            <label class="form-label fw-bold">Số sao:</label>
                            <StarRating :rating="item._editSoSao" :read-only="false" @update:rating="item._editSoSao = $event" />
                          </div>
                          <div class="mb-3">
                            <label class="form-label fw-bold">Nội dung:</label>
                            <textarea
                              v-model.trim="item._editNoiDung"
                              class="form-control"
                              rows="3"
                            ></textarea>
                          </div>
                          <div class="mb-3">
                            <label class="form-label fw-bold">Hình ảnh:</label>
                            <input
                              type="file"
                              multiple
                              accept="image/*"
                              class="form-control"
                              :disabled="getImageCount(item) >= maxImages"
                              @change="onImagesChangeForEdit($event, item)"
                            />
                            <small class="form-text text-muted"
                              >Tối đa {{ maxImages }} ảnh, mỗi ảnh không quá 5MB.</small
                            >
                            <div
                              v-if="item._previewImgs && item._previewImgs.length"
                              class="d-flex flex-wrap mt-2"
                            >
                              <div
                                v-for="(img, idx) in item._previewImgs"
                                :key="idx"
                                class="position-relative me-2 mb-2"
                              >
                                <img
                                  :src="img"
                                  class="img-fluid border rounded"
                                  style="width: 100px; height: 100px; object-fit: cover"
                                  @click="openLightbox(item._previewImgs, idx)"
                                />
                                <button
                                  class="btn btn-sm btn-danger position-absolute top-0 end-0"
                                  @click="removePreviewImageForEdit(item, idx)"
                                >
                                  &times;
                                </button>
                              </div>
                            </div>
                          </div>
                          <button
                            class="btn btn-primary me-2"
                            @click="updateReview(item)"
                            :disabled="item._isSubmitting"
                          >
                            <span
                              v-if="item._isSubmitting"
                              class="spinner-border spinner-border-sm"
                              role="status"
                              aria-hidden="true"
                            ></span>
                            Lưu thay đổi
                          </button>
                          <button class="btn btn-secondary" @click="cancelEdit(item)">Hủy</button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <EmptySuggestBox
                v-else
                contentText="Bạn chưa đánh giá sản phẩm nào."
                linkNav="/shop"
              />
            </div>
          </div>
        </div>
      </div>
    </section>
    <VueEasyLightbox
      :visible="isLightboxOpen"
      :imgs="lightboxImages"
      :index="lightboxIndex"
      @hide="closeLightbox"
    />
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
          hinhAnhUrl: pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh),
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
            hinhAnhUrl: pathReplaceImg(undefined, 'HinhAnh/SanPham', item.tenHinhAnh),
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
          `Các ảnh sau vượt quá dung lượng ${
            this.maxImageSize / 1024 / 1024
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
          `Các ảnh sau vượt quá dung lượng ${
            this.maxImageSize / 1024 / 1024
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
.review-item {
  transition: box-shadow 0.3s ease-in-out;
}
.review-item:hover {
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15) !important;
}
.star-rating .star {
  font-size: 1.75rem;
  color: #e4e5e9;
  cursor: pointer;
  transition: color 0.2s;
}
.star-rating .star.filled,
.star-rating .star:hover,
.star-rating .star:hover ~ .star {
  color: #ffc107;
}
.star-rating:hover .star {
  color: #ffc107;
}
.star-rating .star:hover ~ .star {
  color: #e4e5e9;
}
.btn-danger {
  line-height: 1;
  padding: 0.2rem 0.4rem;
  font-size: 0.8rem;
}
.blockquote {
  border-left: 4px solid #eee;
}
</style>
