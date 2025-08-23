<template>
  <teleport to="body">
    <div v-if="show" class="modal fade show d-block" tabindex="-1" style="background: rgba(0, 0, 0, 0.5)"
      @click.self="close">
      <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Thông Tin Vòng Quay</h5>
            <button type="button" class="btn-close" @click="close"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <!-- Main Content -->
              <div class="col-md-7">
                <h5 class="mb-3 text-center">Tổng quan</h5>
                <!-- Streak and Spin Stats -->
                <div class="row text-center mb-4">
                  <div class="col-6">
                    <div class="card h-100">
                      <div class="card-body">
                        <h6 class="card-title">Lần quay trúng</h6>
                        <p class="display-4 fw-bold text-success">{{ wheelInfo.wonSpins || 0 }}</p>
                        <p class="card-text">lần</p>
                      </div>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="card h-100">
                      <div class="card-body">
                        <h6 class="card-title">Lần quay hụt</h6>
                        <p class="display-4 fw-bold text-primary">{{ wheelInfo.blankSpins || 0 }}</p>
                        <p class="card-text">lân</p>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- How to get more spins -->
                <div class="how-to-get-spins mt-4">
                  <h5 class="mb-3">Cách Kiếm Thêm Lượt Quay</h5>
                  <!-- Spending Card -->
                  <div class="card mb-3">
                    <div class="card-body d-flex align-items-center">
                      <div class="icon-container text-success me-3">
                        <i class="fas fa-shopping-cart fa-2x"></i>
                      </div>
                      <div class="flex-grow-1">
                        <h6 class="mb-1">Mua sắm tích lũy</h6>
                        <p class="mb-1 text-muted small">Nhận <strong>1</strong> lượt quay cho mỗi <strong>{{
                            formatCurrency(2000000) }}</strong> chi tiêu.</p>
                        <div class="progress" style="height: 10px;">
                          <div class="progress-bar bg-success" role="progressbar"
                            :style="{ width: spendingProgress + '%' }" :aria-valuenow="spendingProgress"
                            aria-valuemin="0" aria-valuemax="100"></div>
                        </div>
                        <small class="text-muted">{{ formatCurrency((wheelInfo.totalOrderValue || 0) % 2000000) }} / {{
                          formatCurrency(2000000) }}</small>
                        <hr>
                        <strong>Tổng chi tiêu: </strong><small class="text-muted">{{
                          formatCurrency(wheelInfo.totalOrderValue) }}</small>
                      </div>
                    </div>
                  </div>
                  <!-- Login Streak Card -->
                  <div class="card">
                    <div class="card-body d-flex align-items-center">
                      <div class="icon-container text-primary me-3">
                        <i class="fas fa-calendar-check fa-2x"></i>
                      </div>
                      <div class="flex-grow-1">
                        <h6 class="mb-1">Đăng nhập mỗi ngày</h6>
                        <p class="mb-1 text-muted small">Nhận 1 lượt quay thưởng khi đạt mốc 10 lần đánh dấu.</p>
                        <div class="streak-marks d-flex gap-2 mt-2">
                          <span v-for="n in 10" :key="'streak-mark-' + n"
                            :class="['streak-mark', { active: n <= streakCycleCount }]"></span>
                        </div>
                        <small class="text-muted">Chuỗi đánh dấu: {{ streakCycleCount }} / 10</small>
                        <div class="mt-3">
                          <button class="btn btn-outline-primary btn-sm" :disabled="isMarking" @click="markStreak">
                            <span v-if="isMarking">Đang đánh dấu...</span>
                            <span v-else>Đánh dấu hôm nay</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Coupon Sidebar -->
              <div class="col-md-5 coupon-sidebar">
                <h5 class="mb-3 text-center">Mã Giảm Giá Của Bạn</h5>
                <div v-if="unusedCoupons.length > 0 || usedCoupons.length > 0" class="coupon-list">
                  <ul class="list-group">
                    <!-- Unused Coupons -->
                    <li v-for="coupon in unusedCoupons" :key="coupon.maCode" class="list-group-item">
                      <div>
                        <span class="coupon-wrapper">
                          <strong class="coupon-code" @click="copyCode(coupon.maCode)">{{ coupon.maCode }}</strong>
                          <transition name="tooltip-fade">
                            <div v-if="copiedCode === coupon.maCode" class="copy-tooltip">
                              <i class="bi bi-check-circle-fill me-1"></i>Đã sao chép!
                            </div>
                          </transition>
                        </span>
                        <strong class="badge bg-success rounded-pill float-end px-2 py-1 fs-p">
                          {{ getCouponValue(coupon) }}
                        </strong>
                      </div>
                      <div class="d-flex justify-content-between align-items-center">
                        <small class="text-danger fst-italic">
                          Hạn dùng: 
                          {{ formatDate(coupon.ngayKetThuc)}}
                        </small>
                        <small class="text-muted icon-info" :title="'Mô tả: \n' + coupon.moTa"></small>
                      </div>
                    </li>
                  </ul>

                  <!-- Separator -->
                  <div v-if="usedCoupons.length > 0 && unusedCoupons.length > 0" class="d-flex align-items-center my-3">
                    <hr class="flex-grow-1">
                    <span class="mx-2 text-muted small">Đã dùng</span>
                    <hr class="flex-grow-1">
                  </div>

                  <!-- Used Coupons -->
                  <ul class="list-group" v-if="usedCoupons.length > 0">
                    <li v-for="coupon in visibleUsedCoupons" :key="coupon.maCode" class="list-group-item coupon-used">
                      <div>
                        <strong class="coupon-code disabled-text">{{ coupon.maCode }}</strong>
                        <span class="badge bg-secondary rounded-pill float-end">{{ getCouponValue(coupon) }}</span>
                      </div>

                      <div class="d-flex justify-content-between align-items-center">
                        <small class="text-danger fst-italic">
                          Hạn dùng: 
                          {{ formatDate(coupon.ngayKetThuc)}}
                        </small>
                        <small class="text-muted icon-info" :title="'Mô tả: \n' + coupon.moTa"></small>
                      </div>
                      <span class="badge bg-secondary mt-2">Đã sử dụng</span>
                    </li>
                  </ul>
                  <div v-if="usedCoupons.length > 3" class="text-center mt-2">
                    <button @click="showAllUsed = !showAllUsed" class="btn btn-link btn-sm">
                      {{ showAllUsed ? 'Ẩn bớt' : 'Xem thêm' }}
                    </button>
                  </div>

                </div>
                <div v-else class="alert alert-secondary text-center mt-3">
                  Bạn chưa có mã giảm giá cá nhân nào.
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script>
import { formatCurrency } from '@/constants/formatCurrency';
import Swal from 'sweetalert2';
import ConfigsRequest from '@/models/ConfigsRequest';
import { patchToApi } from '@/utils/axiosClient';

export default {
  name: 'WheelInfoModal',
  props: {
    show: { type: Boolean, required: true },
    wheelInfo: { type: Object, default: () => ({}) }
  },
  emits: ['close', 'streak-updated'],
  data() {
    return {
      copiedCode: null,
      showAllUsed: false,
      isMarking: false,
      localStreak: null,
    };
  },
  computed: {
    spendingProgress() {
      if (!this.wheelInfo || !this.wheelInfo.totalOrderValue) return 0;
      const progress = (this.wheelInfo.totalOrderValue % 2000000) / 2000000 * 100;
      return Math.floor(progress);
    },
    streakCycleCount() {
      const base = (this.localStreak !== null && this.localStreak !== undefined)
        ? this.localStreak
        : (this.wheelInfo && this.wheelInfo.streak ? this.wheelInfo.streak : 0);
      if (!base) return 0;
      const rem = base % 10;
      return (base > 0 && rem === 0) ? 10 : rem;
    },
    usedCoupons() {
      if (!this.wheelInfo || !this.wheelInfo.privateCoupons) return [];
      return this.wheelInfo.privateCoupons.filter(c => c.isUsed);
    },
    unusedCoupons() {
      if (!this.wheelInfo || !this.wheelInfo.privateCoupons) return [];
      return this.wheelInfo.privateCoupons.filter(c => !c.isUsed);
    },
    visibleUsedCoupons() {
      if (this.showAllUsed) {
        return this.usedCoupons;
      }
      return this.usedCoupons.slice(0, 3);
    }
  },
  methods: {
    formatCurrency,
    close() {
      this.$emit('close');
    },
    getCouponValue(coupon) {
      if (coupon.phanTramGiam) {
        return `Giảm ${coupon.phanTramGiam}%`;
      }
      if (coupon.soTienGiam) {
        return `Giảm ${this.formatCurrency(coupon.soTienGiam)}`;
      }
      return 'Coupon có giá trị';
    },
    copyCode(code) {
      if (!code || !navigator.clipboard) return;
      navigator.clipboard.writeText(code).then(() => {
        this.copiedCode = code;
        setTimeout(() => { this.copiedCode = null; }, 1500);
      }).catch(err => {
        console.error('Failed to copy code:', err);
        Swal.fire({ title: 'Lỗi', text: 'Không thể sao chép mã.', icon: 'error' });
      });
    },
    async markStreak() {
      if (this.isMarking) return;
      this.isMarking = true;
      try {
        const prevStreak = (this.localStreak !== null && this.localStreak !== undefined)
          ? this.localStreak
          : (this.wheelInfo && typeof this.wheelInfo.streak === 'number' ? this.wheelInfo.streak : 0);
        const res = await patchToApi('/WheelCoupon/update-last-login-streak', {}, ConfigsRequest.takeAuth());
        if (res && res.success) {
          const newStreak = res.data?.streak ?? null;
          if (typeof newStreak === 'number') {
            this.localStreak = newStreak; // update UI immediately
            this.$emit('streak-updated', newStreak); // notify parent if needed
          }
          const message = (prevStreak === 0 && typeof newStreak === 'number' && newStreak >= 5)
            ? 'Chúc mừng! Bạn được thưởng 5 mốc điểm danh cho lần đánh dấu đầu tiên.'
            : 'Đã điểm danh hôm nay!';
          Swal.fire({ title: 'Thành công', text: message, icon: 'success', timer: 1800, showConfirmButton: false });
        } else {
          Swal.fire({ title: 'Thông tin', text: res?.message || 'Không thể điểm danh.', icon: 'info' });
        }
      } catch (error) {
        console.error('Mark streak failed:', error);
        Swal.fire({ title: 'Lỗi', text: 'Không thể kết nối đến máy chủ.', icon: 'error' });
      } finally {
        this.isMarking = false;
      }
    },
    formatDate(dateString) {
      if (!dateString) return 'Không thời hạn';
      const date = new Date(dateString);
      if (date.getFullYear() > 9000) return 'Không thời hạn'; // Check for 'MaxValue'
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    }
  }
};
</script>

<style scoped>
.coupon-sidebar {
  border-left: 1px solid #dee2e6;
  padding-left: 1.5rem;
}

.coupon-list {
  max-height: 40rem;
  /* Adjust height as needed */
  overflow-y: auto;
  padding-right: 10px;
  /* For scrollbar spacing */
}

.icon-container {
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.coupon-code {
  cursor: pointer;
  font-weight: bold;
  color: #d63384;
}

.copy-tooltip {
  position: absolute;
  bottom: 100%; /* Position it above the element */
  left: 20%;
  transform: translateX(-50%);
  background-color: #333; /* Dark background like many tooltips */
  color: white;
  padding: 5px 10px;
  border-radius: 4px;
  font-size: 12px;
  white-space: nowrap;
  z-index: 20;
  opacity: 0.9;
}

/* The little arrow */
.copy-tooltip::after {
  content: "";
  position: absolute;
  top: 100%;
  left: 50%;
  margin-left: -5px;
  border-width: 5px;
  border-style: solid;
  border-color: #333 transparent transparent transparent;
}

/* The transition will be simpler, just fade */
.tooltip-fade-enter-active,
.tooltip-fade-leave-active {
  transition: opacity 0.3s ease;
}

.tooltip-fade-enter-from,
.tooltip-fade-leave-to {
  opacity: 0;
}

.coupon-used {
  background-color: #f8f9fa;
  /* Light gray background */
  opacity: 0.6;
  /* Slightly faded */
  pointer-events: none;
  /* Disable clicks */
}

.disabled-text {
  color: #6c757d !important;
  /* Gray out the text */
  cursor: not-allowed;
  /* Indicate it's not clickable */
}

.streak-marks {
  display: flex;
  align-items: center;
}

.streak-mark {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background-color: #e0e0e0;
  border: 1px solid #bdbdbd;
  display: inline-block;
}

.streak-mark.active {
  background-color: #0d6efd;
  border-color: #0d6efd;
  box-shadow: 0 0 0 2px rgba(13, 110, 253, 0.2);
}
</style>