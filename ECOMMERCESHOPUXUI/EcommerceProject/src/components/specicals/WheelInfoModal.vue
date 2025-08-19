
<template>
  <teleport to="body">
    <div v-if="show" class="modal fade show d-block" tabindex="-1" style="background: rgba(0, 0, 0, 0.5)" @click.self="close">
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
                        <p class="mb-1 text-muted small">Nhận <strong>1</strong> lượt quay cho mỗi <strong>{{ formatCurrency(2000000) }}</strong> chi tiêu.</p>
                        <div class="progress" style="height: 10px;">
                          <div class="progress-bar bg-success" role="progressbar" :style="{ width: spendingProgress + '%' }" :aria-valuenow="spendingProgress" aria-valuemin="0" aria-valuemax="100"></div>
                        </div>
                        <small class="text-muted">{{ formatCurrency((wheelInfo.totalOrderValue || 0) % 2000000) }} / {{ formatCurrency(2000000) }}</small>
                        <hr>
                        <strong>Tổng chi tiêu: </strong><small class="text-muted">{{ formatCurrency(wheelInfo.totalOrderValue) }}</small>
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
                        <p class="mb-1 text-muted small">Nhận <strong>1</strong> lượt quay thưởng khi đạt mốc <strong>7</strong> ngày.</p>
                        <div class="progress" style="height: 10px;">
                          <div class="progress-bar bg-primary" role="progressbar" :style="{ width: streakProgress + '%' }" :aria-valuenow="streakProgress" aria-valuemin="0" aria-valuemax="100"></div>
                        </div>
                        <small class="text-muted">{{ (wheelInfo.streak > 0 && wheelInfo.streak % 7 === 0) ? 7 : (wheelInfo.streak || 0) % 7 }} / 7 ngày</small>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Coupon Sidebar -->
              <div class="col-md-5 coupon-sidebar">
                <h5 class="mb-3 text-center">Mã Giảm Giá Của Bạn</h5>
                <div v-if="wheelInfo.privateCoupons && wheelInfo.privateCoupons.length > 0" class="coupon-list">
                  <ul class="list-group">
                    <li v-for="coupon in wheelInfo.privateCoupons" :key="coupon.maCode" class="list-group-item" :class="{'coupon-used': coupon.isUsed}">
                      <div>
                        <strong class="coupon-code" :class="{'disabled-text': coupon.isUsed}" @click="coupon.isUsed ? null : copyCode(coupon.maCode)">{{ coupon.maCode }}</strong>
                        <span class="badge bg-info rounded-pill float-end">{{ getCouponValue(coupon) }}</span>
                      </div>
                      <small class="d-block text-muted mt-1">{{ coupon.moTa }}</small>
                      <small class="d-block text-danger fst-italic">Hạn dùng: {{ formatDate(coupon.ngayKetThuc) }}</small>
                      <span v-if="coupon.isUsed" class="badge bg-secondary mt-2">Đã sử dụng</span>
                    </li>
                  </ul>
                  <transition name="fade">
                    <div v-if="copied" class="text-success mt-2 text-center fw-bold">Đã sao chép!</div>
                  </transition>
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

export default {
  name: 'WheelInfoModal',
  props: {
    show: { type: Boolean, required: true },
    wheelInfo: { type: Object, default: () => ({}) }
  },
  emits: ['close'],
  data() {
    return {
      copied: false,
    };
  },
  computed: {
    spendingProgress() {
      if (!this.wheelInfo || !this.wheelInfo.totalOrderValue) return 0;
      const progress = (this.wheelInfo.totalOrderValue % 2000000) / 2000000 * 100;
      return Math.floor(progress);
    },
    streakProgress() {
      if (!this.wheelInfo || !this.wheelInfo.streak) return 0;
      const progress = (this.wheelInfo.streak % 7) / 7 * 100;
       if (this.wheelInfo.streak > 0 && this.wheelInfo.streak % 7 === 0) {
        return 100;
      }
      return Math.floor(progress);
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
        this.copied = true;
        setTimeout(() => { this.copied = false; }, 1500);
      }).catch(err => {
        console.error('Failed to copy code:', err);
        Swal.fire({ title: 'Lỗi', text: 'Không thể sao chép mã.', icon: 'error' });
      });
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
  max-height: 40rem; /* Adjust height as needed */
  overflow-y: auto;
  padding-right: 10px; /* For scrollbar spacing */
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

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.coupon-used {
  background-color: #f8f9fa; /* Light gray background */
  opacity: 0.6; /* Slightly faded */
  pointer-events: none; /* Disable clicks */
}

.disabled-text {
  color: #6c757d !important; /* Gray out the text */
  cursor: not-allowed; /* Indicate it's not clickable */
}
</style>
