<template>
  <li>
    <a href="#" @click.prevent="showModal = true" class="dropdown-item d-flex">
      <div class="position-relative">
        <span class="icon_ribbon_alt me-2"></span>
        <div v-if="maxSpins - spinCount > 0" class="position-absolute top-0 start-50 translate-middle badge rounded-pill bg-danger">{{ maxSpins - spinCount }}</div>
      </div>
      Vòng quay
    </a>
  </li>

  <teleport to="body">
    <!-- Modal for the wheel -->
    <div
      v-if="showModal"
      class="modal fade show d-block"
      tabindex="-1"
      style="background: rgba(0, 0, 0, 0.45)"
      @click.self="closeModal"
    >
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content p-4 position-relative">
          <div class="modal-header text-center border-0 pb-0">
            <h5 class="modal-title w-100 fs-4">Vòng Quay May Mắn
              <!-- Icon to trigger the info modal -->
              <a href="#" @click.prevent="openInfoModal" class="position-relative text-decoration-none ms-3">
                <span class="icon_info_alt"></span>
              </a>
            </h5>
            <button
              type="button"
              class="btn-close position-absolute end-0 top-0 m-3"
              @click="closeModal"
              aria-label="Close"
            ></button>
          </div>
          <div class="modal-body d-flex flex-column align-items-center">
            <!-- Responsive Wheel Container -->
            <div class="wheel-container">
              <svg
                class="wheel-svg"
                viewBox="0 0 100 100"
                :style="{ transform: `rotate(${rotation}deg)` }"
              >
                <g v-for="(item, idx) in prizes" :key="idx">
                  <path
                    :d="describeArc(50, 50, 48, idx * arc, (idx + 1) * arc)"
                    :fill="item.isBlank ? '#BDBDBD' : colors[idx % colors.length]"
                    stroke="#fff"
                    stroke-width="0.5"
                  />
                  <text
                    class="wheel-text"
                    :x="getTextPos(idx).x"
                    :y="getTextPos(idx).y"
                    :transform="getTextTransform(idx)"
                  >
                    {{ item.isBlank ? item.name : item.revealed ? item.name : '?' }}
                  </text>
                </g>
              </svg>
              <div class="wheel-pointer">▼</div>
            </div>

            <!-- Spin Button -->
            <button
              class="btn btn-primary mt-4 px-4 py-2 fs-5"
              :disabled="spinning || spinCount >= maxSpins"
              @click="spin"
            >
              <span v-if="spinning">Đang quay...</span>
              <span v-else>Quay ({{ maxSpins - spinCount }} lượt)</span>
            </button>

            <button
              class="btn btn-info mt-3 px-4 py-2 fs-5 text-white"
              :disabled="checkingCoupon"
              @click="checkSpinCount"
            >
              <span v-if="checkingCoupon">Đang kiểm tra...</span>
              <span v-else>Kiểm tra lượt quay</span>
            </button>

            <!-- Result Display -->
            <div
              v-if="selectedPrize"
              :class="[
                'alert',
                selectedPrize.isBlank ? 'alert-secondary' : 'alert-success',
                'mt-4',
                'text-center',
                'w-100',
              ]"
              role="alert"
            >
              <h5 v-if="!selectedPrize.isBlank" class="mb-2">Chúc mừng bạn đã trúng!</h5>
              <div class="fw-bold fs-5">{{ selectedPrize.name }}</div>
              <div v-if="!selectedPrize.isBlank && selectedPrize.revealed" class="mt-2">
                Mã code:
                <code class="coupon-code" @click="copyCode(selectedPrize.code)">{{
                  selectedPrize.code
                }}</code>
              </div>
              <transition name="fade">
                <div v-if="copied" class="text-success mt-2">Đã sao chép!</div>
              </transition>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Info Modal -->
    <WheelInfoModal v-if="showInfoModal" :show="showInfoModal" :wheelInfo="wheelInfo" @close="showInfoModal = false" />
  </teleport>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest';
import { getFromApi, postToApi, patchToApi } from '@/utils/axiosClient';
import Swal from 'sweetalert2';
import { formatCurrency } from '@/constants/formatCurrency';
import confetti from 'canvas-confetti';
import WheelInfoModal from './WheelInfoModal.vue';

export default {
  name: 'WheelRandomCode',
  components: {
    WheelInfoModal,
  },
  data() {
    return {
      showModal: false,
      showInfoModal: false,
      wheelInfo: {},
      prizes: [],
      colors: ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA'],
      rotation: 0,
      spinning: false,
      selectedPrize: null,
      maxSpins: 0,
      spinCount: 0,
      copied: false,
      checkingCoupon: false,
    };
  },
  computed: {
    arc() {
      return this.prizes.length > 0 ? 360 / this.prizes.length : 0;
    },
  },
  mounted() {
    this.initializeWheel();
  },
  methods: {
    // WHEEL DRAWING UTILITIES
    polarToCartesian(cx, cy, r, angle) {
      const a = ((angle - 90) * Math.PI) / 180.0;
      return { x: cx + r * Math.cos(a), y: cy + r * Math.sin(a) };
    },
    describeArc(cx, cy, r, startAngle, endAngle) {
      const start = this.polarToCartesian(cx, cy, r, endAngle);
      const end = this.polarToCartesian(cx, cy, r, startAngle);
      const largeArcFlag = endAngle - startAngle <= 180 ? '0' : '1';
      return `M ${cx} ${cy} L ${start.x} ${start.y} A ${r} ${r} 0 ${largeArcFlag} 0 ${end.x} ${end.y} Z`;
    },
    getTextPos(idx) {
      const angle = (idx + 0.5) * this.arc;
      return this.polarToCartesian(50, 50, 28, angle);
    },
    getTextTransform(idx) {
      const angle = (idx + 0.5) * this.arc;
      const pos = this.getTextPos(idx);
      return `rotate(${angle + 90} ${pos.x} ${pos.y})`;
    },

    // API & DATA LOGIC
    fetchPrizeList() {
      const generatedPrizes = Array(9).fill(null).map(() => ({ name: '?', code: '', isBlank: false, revealed: false }));
      const blankPrize = { name: 'Chúc bạn may mắn lần sau', isBlank: true };
      const allPrizes = [...generatedPrizes, blankPrize];
      for (let i = allPrizes.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [allPrizes[i], allPrizes[j]] = [allPrizes[j], allPrizes[i]];
      }
      this.prizes = allPrizes;
    },
    async fetchWheelInfo() {
      try {
        const res = await getFromApi('/WheelCoupon/private-coupon', ConfigsRequest.takeAuth());
        if (res && res.success) {
          this.wheelInfo = res.data;
        }
      } catch (error) {
        console.error('Failed to fetch wheel info:', error);
        this.wheelInfo = { streak: 0, totalOrderValue: 0, privateCoupons: [] };
      }
    },
    async checkSpinCount() {
      this.checkingCoupon = true;
      try {
        const res = await getFromApi('/WheelCoupon/time-spin-wheel-coupon', ConfigsRequest.takeAuth());
        if (res && res.success) {
          const spins = Number(res.data) || 0;
          this.maxSpins = spins;
          this.spinCount = 0;
          this.selectedPrize = null;
          this.fetchPrizeList();
          Swal.fire({
            title: 'Lượt quay của bạn',
            text: `Bạn hiện có ${spins} lượt quay.`,
            icon: 'info',
            confirmButtonText: 'Đã hiểu',
          });
        } else {
          Swal.fire({ title: 'Lỗi', text: res?.message || 'Không thể kiểm tra lượt quay.', icon: 'error' });
        }
      } catch (error) {
        console.error('Failed to check spin count:', error);
        Swal.fire({ title: 'Lỗi', text: 'Đã xảy ra lỗi khi kiểm tra lượt quay.', icon: 'error' });
      } finally {
        this.checkingCoupon = false;
      }
    },

    // COMPONENT METHODS
    openInfoModal() {
      this.fetchWheelInfo();
      this.showInfoModal = true;
    },
    closeModal() {
      if (!this.spinning) {
        this.showModal = false;
      }
    },
    triggerFireworks() {
      const duration = 5 * 1000;
      const animationEnd = Date.now() + duration;
      const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 1051 };
      function randomInRange(min, max) { return Math.random() * (max - min) + min; }
      const interval = setInterval(() => {
        const timeLeft = animationEnd - Date.now();
        if (timeLeft <= 0) return clearInterval(interval);
        const particleCount = 50 * (timeLeft / duration);
        confetti({ ...defaults, particleCount, origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 } });
        confetti({ ...defaults, particleCount, origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 } });
      }, 250);
    },
    async spin() {
      if (this.spinning || this.spinCount >= this.maxSpins) return;

      this.spinning = true;
      this.selectedPrize = null;
      this.copied = false;

      const blankIndex = this.prizes.findIndex((p) => p.isBlank);
      let targetIndex;
      let couponData = null;
      let spinConsumed = false;

      let running = true;
      let currentRotation = this.rotation;
      let frameId;
      const speed = 15;
      const animateSpin = () => {
        if (!running) return;
        currentRotation += speed;
        this.rotation = currentRotation;
        frameId = requestAnimationFrame(animateSpin);
      }
      animateSpin();

      try {
        const res = await postToApi('/WheelCoupon/spin', ConfigsRequest.takeAuth());
        if (res && res.success) {
          spinConsumed = true;
          if (res.data && res.data.maCode && res.data.maCode !== 'BLANK') {
            couponData = res.data;
            const availableIndexes = this.prizes.map((p, idx) => (!p.isBlank && !p.revealed ? idx : -1)).filter((idx) => idx !== -1);
            targetIndex = availableIndexes[Math.floor(Math.random() * availableIndexes.length)];
          } else {
            targetIndex = blankIndex;
          }
        } else {
          targetIndex = blankIndex;
          Swal.fire({ title: 'Lỗi!', text: res?.message || 'Không thể nhận kết quả.', icon: 'error' });
        }
      } catch (error) {
        console.error('Failed to get coupon from API:', error);
        targetIndex = blankIndex;
        Swal.fire({ title: 'Lỗi!', text: 'Lỗi kết nối máy chủ.', icon: 'error' });
      } finally {
        running = false;
        cancelAnimationFrame(frameId);

        const finalAngle = 360 - (targetIndex * this.arc + this.arc / 2);
        const currentDeg = this.rotation % 360;
        let delta = finalAngle - currentDeg;
        if (delta < 0) delta += 360;
        const smoothRotation = currentRotation + delta + 360 * 2;
        this.rotation = smoothRotation;

        setTimeout(() => {
          if (couponData) {
            const winningPrize = {
              name: couponData.isPercent ? `Giảm ${couponData.phanTramGiam}%` : `Giảm ${formatCurrency(couponData.soTienGiam)}`,
              code: couponData.maCode,
              isPercent: couponData.isPercent,
              revealed: true,
              isBlank: false,
            };
            this.prizes.splice(targetIndex, 1, winningPrize);
            this.selectedPrize = winningPrize;
            this.triggerFireworks();
          } else {
            this.selectedPrize = this.prizes[targetIndex];
          }
          this.spinning = false;
          if (spinConsumed) {
            this.spinCount++;
          }
        }, 2000);
      }
    },
    copyCode(code) {
      if (!code || !navigator.clipboard) return;
      navigator.clipboard.writeText(code).then(() => {
        this.copied = true;
        setTimeout(() => { this.copied = false; }, 1500);
      }).catch((err) => {
        console.error('Failed to copy code:', err);
        Swal.fire({ title: 'Lỗi', text: 'Không thể sao chép mã.', icon: 'error' });
      });
    },

    // LIFECYCLE HOOKS
    async initializeWheel() {
      const today = new Date().toISOString().slice(0, 10);
      const lastLoginUpdate = localStorage.getItem('wheel_last_login_update') || '';

      if (lastLoginUpdate !== today) {
        try {
          const res = await patchToApi('/WheelCoupon/update-last-login-streak', '', ConfigsRequest.takeAuth());
          if (res && res.success) {
            await this.fetchWheelInfo(); 
            this.showInfoModal = true;
          }
          localStorage.setItem('wheel_last_login_update', today);
        } catch (error) {
          console.error('Failed to update login streak:', error);
        }
      }

      await this.checkSpinCount();
      this.fetchPrizeList();

      const wheelSwalDate = localStorage.getItem('wheel_swal_date') || '';
      if (this.maxSpins > 0 && today !== wheelSwalDate) {
        Swal.fire({
          title: 'Vòng Quay May Mắn!',
          text: 'Bạn có lượt quay miễn phí hôm nay, muốn thử vận may không?',
          icon: 'info',
          showCancelButton: true,
          confirmButtonText: 'Quay ngay!',
          cancelButtonText: 'Để sau',
          allowOutsideClick: false,
        }).then((result) => {
          if (result.isConfirmed) {
            this.showModal = true;
          }
          localStorage.setItem('wheel_swal_date', today);
        });
      }
    },
  },
};
</script>

<style scoped>
/* Responsive Wheel Container */
.wheel-container {
  position: relative;
  /* Phóng to hơn: tăng min/max và preferred size */
  width: clamp(28rem, 70vw, 38rem);
  height: clamp(28rem, 70vw, 38rem);
  margin-bottom: 1rem;
}

/* SVG Wheel Styling */
.wheel-svg {
  border-radius: 50%;
  box-shadow: 0 0.25rem 1rem rgba(0, 0, 0, 0.15);
  background: #fff;
  transition: transform 4s cubic-bezier(0.25, 0.1, 0.25, 1); /* Smoother ease-out */
}

/* Pointer arrow */
.wheel-pointer {
  position: absolute;
  top: -0.5rem; /* Position slightly above the wheel */
  left: 50%;
  transform: translateX(-50%);
  font-size: 2.5rem;
  color: #e53935;
  font-weight: bold;
  z-index: 2;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

/* Text inside the wheel */
.wheel-text {
  text-anchor: middle;
  alignment-baseline: middle;
  font-size: 2.7px; /* Giảm kích thước để chữ nằm trọn trong ô */
  font-weight: 600;
  fill: #212529;
  pointer-events: none;
  user-select: none;
}

/* Coupon Code Styling */
.coupon-code {
  background-color: #e9ecef;
  color: #d63384;
  border-radius: 0.25rem;
  padding: 0.2rem 0.4rem;
  margin-left: 0.25rem;
  cursor: pointer;
  font-weight: bold;
}

/* Fade transition for "Copied!" message */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
/* Phóng to modal để bao phủ wheel */
:deep(.modal-dialog) {
  max-width: 900px;
  width: 95vw;
}
:deep(.modal-content) {
  min-width: 0;
  width: 100%;
  max-width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}
</style>