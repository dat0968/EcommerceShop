<template>
  <li>
    <a href="#" @click.prevent="showModal = true" class="d-flex dropdown-item">
      <div class="position-relative">
        <i class="bi bi-gem me-2"></i>
        <div
          v-if="spinsLeft > 0"
          class="position-absolute top-0 start-50 translate-middle badge rounded-pill bg-danger"
        >
          {{ spinsLeft }}
        </div>
      </div>
      Vòng quay
    </a>
  </li>
  <li>
    <a href="#" @click.prevent="openInfoModal" class="d-flex dropdown-item" title="Xem thể lệ">
      <div class="position-relative">
        <span class="icon_book me-2"></span>
      </div>
      Mã giảm giá
    </a>
  </li>
  <div v-bind="$attrs">
    <teleport to="body">
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
              <h5 class="modal-title w-100 fs-4">
                Vòng Quay May Mắn
                <a
                  href="#"
                  @click.prevent="openInfoModal"
                  class="position-relative text-decoration-none ms-3"
                  title="Xem thể lệ"
                >
                  <i class="bi bi-question-circle-fill info-icon-highlight"></i>
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
              <div class="wheel-container">
                <svg
                  class="wheel-svg"
                  viewBox="0 0 100 100"
                  :style="{ transform: `rotate(${rotation}deg)` }"
                >
                  <g v-for="(item, idx) in wheelPrizes" :key="idx">
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
                      {{ item.name }}
                    </text>
                  </g>
                </svg>
                <div class="wheel-pointer">▼</div>
              </div>

              <button
                class="btn btn-primary mt-4 px-4 py-2 fs-5"
                :disabled="isSpinning || spinsLeft <= 0"
                @click="spinWheel"
              >
                <span v-if="isSpinning">Đang quay...</span>
                <span v-else>Quay (còn {{ spinsLeft }} lượt)</span>
              </button>

              <button
                class="btn btn-info mt-3 px-4 py-2 fs-5 text-white"
                :disabled="isSpinning || isLoading"
                @click="initializeWheel"
              >
                <span v-if="isLoading"
                  ><i class="bi bi-arrow-clockwise spin-animation"></i> Đang tải...</span
                >
                <span v-else><i class="bi bi-arrow-clockwise"></i> Kiểm tra lại</span>
              </button>

              <div
                v-if="resultMessage"
                :class="[
                  'alert',
                  resultIsWin ? 'alert-success' : 'alert-secondary',
                  'mt-4',
                  'text-center',
                  'w-100',
                ]"
                role="alert"
              >
                <h5 v-if="resultIsWin" class="mb-2">Chúc mừng bạn đã trúng!</h5>
                <div class="fw-bold fs-5">{{ resultMessage }}</div>
                <div v-if="resultIsWin && couponCode" class="mt-2 coupon-wrapper">
                  Mã code:
                  <code class="coupon-code" @click="copyCode(couponCode)">{{ couponCode }}</code>
                  <transition name="tooltip-fade">
                    <div v-if="copiedCode" class="copy-tooltip">
                      <i class="bi bi-check-circle-fill me-1"></i>Đã sao chép!
                    </div>
                  </transition>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <WheelInfoModal
        v-if="showInfoModal"
        :show="showInfoModal"
        :wheelInfo="wheelInfo"
        @close="showInfoModal = false"
      />
    </teleport>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import { getFromApi, postToApi } from '@/utils/axiosClient'
import confetti from 'canvas-confetti'
import Swal from 'sweetalert2'
import WheelInfoModal from './WheelInfoModal.vue'

export default {
  name: 'WheelRandomCode',
  components: {
    WheelInfoModal,
  },
  inheritAttrs: false,
  data() {
    return {
      showModal: false,
      showInfoModal: false,
      wheelInfo: {},
      wheelPrizes: [], // 10 prizes including one blank
      presetToken: null,
      originalPrizes: [], // 9 real prizes from API
      colors: ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA'],
      rotation: 0,
      isSpinning: false,
      spinsLeft: 0,
      // Result state
      resultMessage: null,
      resultIsWin: false,
      couponCode: null,
      copiedCode: null,
      isLoading: false, // Added for refresh button
    }
  },
  computed: {
    arc() {
      return this.wheelPrizes.length > 0 ? 360 / this.wheelPrizes.length : 0
    },
  },
  watch: {
    showModal(newValue) {
      if (newValue) {
        this.initializeWheel()
      }
    },
  },
  mounted() {
    this.promptSpinIfAvailable()
  },
  methods: {
    async promptSpinIfAvailable() {
      try {
        const today = new Date()
        const todayKey = today.toISOString().slice(0, 10) // YYYY-MM-DD
        const lastShown = localStorage.getItem('wheelPromptLastDate')
        if (lastShown === todayKey) return

        const spinsRes = await getFromApi(
          '/WheelCoupon/time-spin-wheel-coupon',
          ConfigsRequest.takeAuth(),
        )
        const spins = spinsRes && spinsRes.success ? Number(spinsRes.data) || 0 : 0
        this.spinsLeft = spins
        if (spins > 0) {
          const result = await Swal.fire({
            title: 'Vòng quay may mắn',
            text: `Bạn đang có ${spins} lượt quay. Bạn muốn quay ngay bây giờ không?`,
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: 'Quay ngay',
            cancelButtonText: 'Để sau',
            reverseButtons: true,
          })
          localStorage.setItem('wheelPromptLastDate', todayKey)
          if (result.isConfirmed) {
            this.showModal = true
          }
        }
      } catch (error) {
        console.error('Failed to check spins for prompt:', error)
      }
    },

    // WHEEL DRAWING UTILITIES
    polarToCartesian(cx, cy, r, angle) {
      const a = ((angle - 90) * Math.PI) / 180.0
      return { x: cx + r * Math.cos(a), y: cy + r * Math.sin(a) }
    },
    describeArc(cx, cy, r, startAngle, endAngle) {
      const start = this.polarToCartesian(cx, cy, r, endAngle)
      const end = this.polarToCartesian(cx, cy, r, startAngle)
      const largeArcFlag = endAngle - startAngle <= 180 ? '0' : '1'
      return `M ${cx} ${cy} L ${start.x} ${start.y} A ${r} ${r} 0 ${largeArcFlag} 0 ${end.x} ${end.y} Z`
    },
    getTextPos(idx) {
      const angle = (idx + 0.5) * this.arc
      return this.polarToCartesian(50, 50, 28, angle)
    },
    getTextTransform(idx) {
      const angle = (idx + 0.5) * this.arc
      const pos = this.getTextPos(idx)
      return `rotate(${angle + 90} ${pos.x} ${pos.y})`
    },

    // API & DATA LOGIC
    async initializeWheel() {
      this.resultMessage = null // Clear previous result
      this.isLoading = true
      try {
        const spinsRes = await getFromApi(
          '/WheelCoupon/time-spin-wheel-coupon',
          ConfigsRequest.takeAuth(),
        )
        this.spinsLeft = spinsRes && spinsRes.success ? Number(spinsRes.data) || 0 : 0

        if (this.spinsLeft > 0) {
          const presetRes = await getFromApi(
            '/WheelCoupon/generate-preset',
            ConfigsRequest.takeAuth(),
          )
          console.log(presetRes)
          if (presetRes && presetRes.success) {
            this.presetToken = presetRes.data.presetToken
            this.originalPrizes = presetRes.data.displayValues.map((name) => ({
              name,
              isBlank: false,
            }))
            this.setupWheelPrizes()
          } else {
            this.wheelPrizes = Array(10).fill({ name: 'Lỗi', isBlank: true })
          }
        } else {
          this.wheelPrizes = Array(10).fill({ name: 'Hết lượt', isBlank: true })
        }
      } catch (error) {
        console.error('Initialization failed:', error)
        this.wheelPrizes = Array(10).fill({ name: 'Lỗi', isBlank: true })
      } finally {
        this.isLoading = false
      }
    },
    setupWheelPrizes() {
      const blankPrize = { name: 'Chúc bạn may mắn lần sau', isBlank: true }
      let prizes = [...this.originalPrizes]
      const blankIndex = Math.floor(Math.random() * (prizes.length + 1))
      prizes.splice(blankIndex, 0, blankPrize)
      this.wheelPrizes = prizes
    },
    async claimPrize(finalIndex) {
      const landedPrize = this.wheelPrizes[finalIndex]
      let indexToSend = -1

      if (!landedPrize.isBlank) {
        indexToSend = this.originalPrizes.findIndex((p) => p.name === landedPrize.name)
      }
      if (indexToSend === -1) {
        indexToSend = 0
      }

      try {
        const res = await postToApi(
          '/WheelCoupon/claim-preset',
          { presetToken: this.presetToken, wonIndex: indexToSend },
          ConfigsRequest.takeAuth(),
        )
        if (res && res.success) {
          if (res.data.isWin === false) {
            this.resultIsWin = false
            this.resultMessage = 'Chúc bạn may mắn lần sau!'
          } else {
            // Handle prize types: coupon or marks
            this.resultIsWin = true
            if (res.data.rewardType === 'marks') {
              this.couponCode = null
              const added = res.data.marksAdded || 0
              const newStreak = res.data.newStreak
              this.resultMessage = `Bạn nhận được +${added} đánh dấu! Streak hiện tại: ${newStreak}`
              if (this.wheelInfo && typeof newStreak === 'number') {
                this.wheelInfo = { ...this.wheelInfo, streak: newStreak }
              }
            } else {
              this.couponCode = res.data.maCode
              this.resultMessage = res.data.isPercent
                ? `Giảm ${res.data.phanTramGiam}%`
                : `Giảm ${new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(res.data.soTienGiam)}`
            }
            this.triggerFireworks()
          }
        } else {
          this.resultIsWin = false
          this.resultMessage = res?.message || 'Có lỗi xảy ra, vui lòng thử lại.'
        }
      } catch (error) {
        this.resultIsWin = false
        this.resultMessage = 'Không thể kết nối đến máy chủ.'
        console.error('Claiming prize failed:', error)
      } finally {
        this.isSpinning = false
        this.spinsLeft-- // Manually decrement spin count
      }
    },

    // COMPONENT METHODS
    async openInfoModal() {
      try {
        const res = await getFromApi('/WheelCoupon/private-coupon', ConfigsRequest.takeAuth())
        this.wheelInfo = res && res.success ? res.data : {}
        this.showInfoModal = true
      } catch (error) {
        console.error('Failed to fetch wheel info:', error)
      }
    },
    closeModal() {
      if (!this.isSpinning) {
        this.showModal = false
      }
    },
    triggerFireworks() {
      const duration = 3 * 1000
      const animationEnd = Date.now() + duration
      const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 1051 }
      function randomInRange(min, max) {
        return Math.random() * (max - min) + min
      }
      const interval = setInterval(() => {
        const timeLeft = animationEnd - Date.now()
        if (timeLeft <= 0) return clearInterval(interval)
        const particleCount = 50 * (timeLeft / duration)
        confetti({
          ...defaults,
          particleCount,
          origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 },
        })
        confetti({
          ...defaults,
          particleCount,
          origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 },
        })
      }, 250)
    },
    spinWheel() {
      if (this.isSpinning || this.spinsLeft <= 0) return

      this.isSpinning = true
      this.resultMessage = null
      this.copiedCode = null

      const targetIndex = Math.floor(Math.random() * this.wheelPrizes.length)
      const finalAngle = 360 - (targetIndex * this.arc + this.arc / 2)
      const currentDeg = this.rotation % 360
      let delta = finalAngle - currentDeg
      if (delta < 0) delta += 360
      const totalRotations = 360 * 5
      this.rotation += totalRotations + delta

      setTimeout(() => {
        this.claimPrize(targetIndex)
      }, 4000)
    },
    copyCode(code) {
      if (!code || !navigator.clipboard) return
      navigator.clipboard
        .writeText(code)
        .then(() => {
          this.copiedCode = code
          setTimeout(() => {
            this.copiedCode = null
          }, 1500)
        })
        .catch((err) => {
          console.error('Failed to copy code:', err)
        })
    },
  },
}
</script>

<style scoped>
.info-icon-highlight {
  font-size: 1.4rem; /* Làm cho biểu tượng lớn hơn một chút */
  color: #17a2b8; /* Màu xanh thông tin, dễ nhận biết */
  transition:
    transform 0.2s ease-in-out,
    color 0.2s;
  vertical-align: middle; /* Căn chỉnh tốt hơn với văn bản */
}

.info-icon-highlight:hover {
  transform: scale(1.15); /* Hiệu ứng phóng to khi di chuột qua */
  color: #138496; /* Đổi màu đậm hơn một chút */
}
@keyframes spin-animation {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
.spin-animation {
  display: inline-block;
  animation: spin-animation 1s linear infinite;
}
.wheel-container {
  position: relative;
  width: clamp(28rem, 70vw, 38rem);
  height: clamp(28rem, 70vw, 38rem);
  margin-bottom: 1rem;
}
.wheel-svg {
  border-radius: 50%;
  box-shadow: 0 0.25rem 1rem rgba(0, 0, 0, 0.15);
  background: #fff;
  transition: transform 4s cubic-bezier(0.25, 0.1, 0.25, 1);
}
.wheel-pointer {
  position: absolute;
  top: -0.5rem;
  left: 50%;
  transform: translateX(-50%);
  font-size: 2.5rem;
  color: #e53935;
  font-weight: bold;
  z-index: 2;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}
.wheel-text {
  text-anchor: middle;
  alignment-baseline: middle;
  font-size: 2.7px;
  font-weight: 600;
  fill: #212529;
  pointer-events: none;
  user-select: none;
}
.coupon-code {
  background-color: #e9ecef;
  color: #d63384;
  border-radius: 0.25rem;
  padding: 0.2rem 0.4rem;
  margin-left: 0.25rem;
  cursor: pointer;
  font-weight: bold;
}
.coupon-wrapper {
  position: relative;
  display: inline-block;
}

.copy-tooltip {
  position: absolute;
  bottom: 125%; /* Position it above the element */
  left: 50%;
  transform: translateX(-50%);
  background-color: #333; /* Dark background like many tooltips */
  color: white;
  padding: 5px 10px;
  border-radius: 4px;
  font-size: 12px;
  white-space: nowrap;
  z-index: 10;
  opacity: 0.9;
}

/* The little arrow */
.copy-tooltip::after {
  content: '';
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
