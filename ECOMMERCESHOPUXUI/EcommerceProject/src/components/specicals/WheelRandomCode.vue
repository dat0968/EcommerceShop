<template>
  <!-- Icon to trigger the modal -->
  <a href="#" @click.prevent="showModal = true" class="position-relative text-decoration-none">
    <span class="icon_ribbon_alt"></span>
    <div v-if="maxSpins > 0" class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">{{ maxSpins - spinCount }}</div>
  </a>

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
          <h5 class="modal-title w-100 fs-4">Vòng Quay May Mắn</h5>
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
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import ConfigsRequest from '@/models/ConfigsRequest'
import { getFromApi, postToApi, patchToApi } from '@/utils/axiosClient'
import Swal from 'sweetalert2'
import { formatCurrency } from '@/constants/formatCurrency'
import confetti from 'canvas-confetti'

// --- Component State ---
const showModal = ref(false)
const prizes = ref([])
const colors = ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA']
const rotation = ref(0)
const spinning = ref(false)
const selectedPrize = ref(null)
const maxSpins = ref(0)
const spinCount = ref(0)
const copied = ref(false)
const checkingCoupon = ref(false) // New state for loading button

// --- Computed Properties ---
const arc = computed(() => (prizes.value.length > 0 ? 360 / prizes.value.length : 0))

// --- Wheel Drawing Utilities (using viewBox coordinates) ---
const polarToCartesian = (cx, cy, r, angle) => {
  const a = ((angle - 90) * Math.PI) / 180.0
  return { x: cx + r * Math.cos(a), y: cy + r * Math.sin(a) }
}

const describeArc = (cx, cy, r, startAngle, endAngle) => {
  const start = polarToCartesian(cx, cy, r, endAngle)
  const end = polarToCartesian(cx, cy, r, startAngle)
  const largeArcFlag = endAngle - startAngle <= 180 ? '0' : '1'
  // Path: Move to center, Line to arc start, Arc to arc end, Close path
  return `M ${cx} ${cy} L ${start.x} ${start.y} A ${r} ${r} 0 ${largeArcFlag} 0 ${end.x} ${end.y} Z`
}

const getTextPos = (idx) => {
  // Đặt text gần tâm hơn để không tràn viền (radius = 28)
  const angle = (idx + 0.5) * arc.value
  return polarToCartesian(50, 50, 28, angle)
}

const getTextTransform = (idx) => {
  const angle = (idx + 0.5) * arc.value
  const pos = getTextPos(idx)
  // Rotate text to be upright relative to the wheel's edge
  return `rotate(${angle + 90} ${pos.x} ${pos.y})`
}

// --- API & Data Logic ---
const fetchPrizeList = () => {
  // 9 slots are hidden ("?"), 1 slot is blank
  const generatedPrizes = Array(9)
    .fill(null)
    .map(() => ({
      name: '?',
      code: '',
      isBlank: false,
      revealed: false,
    }))
  // Only 1 blank slot
  const blankPrize = { name: 'Chúc bạn may mắn lần sau', isBlank: true }
  const allPrizes = [...generatedPrizes, blankPrize]
  // Shuffle all 10 prizes for a random layout
  for (let i = allPrizes.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[allPrizes[i], allPrizes[j]] = [allPrizes[j], allPrizes[i]]
  }
  prizes.value = allPrizes
}

const initializeWheel = async () => {
  const today = new Date().toISOString().slice(0, 10)
  const lastSpinDate = localStorage.getItem('wheel_last_spin_date') || ''

  if (lastSpinDate !== today) {
    try {
      await patchToApi(
        '/WheelCoupon/update-last-login-streak',
        '',
        ConfigsRequest.getSkipAuthConfig(),
      )
      localStorage.setItem('wheel_last_spin_date', today)
    } catch (error) {
      console.error('Failed to update login streak:', error)
    }
  }

  try {
    const res = await getFromApi('/WheelCoupon/time-spin-wheel-coupon')
    maxSpins.value = res?.success ? Number(res.data) || 0 : 0
  } catch (error) {
    maxSpins.value = 0
    console.error('Failed to get spin count:', error)
  }

  fetchPrizeList()

  const wheelSwalDate = localStorage.getItem('wheel_swal_date') || ''
  if (maxSpins.value > 0 && today !== wheelSwalDate) {
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
        showModal.value = true
      }
      localStorage.setItem('wheel_swal_date', today)
    })
  }
}

onMounted(initializeWheel)

// --- Component Methods ---
const closeModal = () => {
  if (!spinning.value) {
    showModal.value = false
  }
}

const triggerFireworks = () => {
  const duration = 5 * 1000 // 5 seconds
  const animationEnd = Date.now() + duration
  const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 1051 }

  function randomInRange(min, max) {
    return Math.random() * (max - min) + min
  }

  const interval = setInterval(function () {
    const timeLeft = animationEnd - Date.now()

    if (timeLeft <= 0) {
      return clearInterval(interval)
    }

    const particleCount = 50 * (timeLeft / duration)
    // since particles fall down, start a bit higher than random
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
}

const spin = async () => {
  if (spinning.value || spinCount.value >= maxSpins.value) return

  spinning.value = true
  selectedPrize.value = null
  copied.value = false

  // Determine if it's a blank spin (10% chance)
  const blankIndex = prizes.value.findIndex((p) => p.isBlank)
  const willBeBlank = Math.random() < 0.9 // ? 90% chance of landing on blank

  let targetIndex
  let couponData = null
  let spinConsumed = false // New flag to track if spin was consumed

  // Start continuous spin animation
  let running = true
  let currentRotation = rotation.value
  let frameId
  const speed = 15 // degrees per frame
  function animateSpin() {
    if (!running) return
    currentRotation += speed
    rotation.value = currentRotation
    frameId = requestAnimationFrame(animateSpin)
  }
  animateSpin()

  try {
    if (!willBeBlank) {
      // If not blank, call API to get a coupon
      const res = await postToApi('/WheelCoupon/private-coupon')
      if (res && res.success && res.data) {
        couponData = res.data
        // Find a random unrevealed non-blank slot to stop at
        const availableIndexes = prizes.value
          .map((p, idx) => (!p.isBlank && !p.revealed ? idx : -1))
          .filter((idx) => idx !== -1)
        targetIndex = availableIndexes[Math.floor(Math.random() * availableIndexes.length)]
        spinConsumed = true // Coupon successfully received, spin consumed
      } else {
        // If API call succeeds but returns no data or success: false
        targetIndex = blankIndex // Default to blank prize
        Swal.fire({
          title: 'Lỗi!',
          text: res?.message || 'Không thể nhận kết quả từ máy chủ. Vui lòng thử lại.',
          icon: 'error',
        })
        // spinConsumed remains false, as the spin was not successfully completed due to API response
      }
    } else {
      // If it's a blank spin, target the blank slot and call API to create blank coupon
      targetIndex = blankIndex
      const blankCouponRes = await postToApi('/WheelCoupon/blank-coupon')
      if (blankCouponRes && blankCouponRes.success) {
        spinConsumed = true // Blank coupon successfully created, spin consumed
      } else {
        console.error('Failed to create blank coupon:', blankCouponRes?.message)
        Swal.fire({
          title: 'Lỗi!',
          text: blankCouponRes?.message || 'Không thể ghi nhận lượt quay trống. Vui lòng thử lại.',
          icon: 'error',
        })
      }
    }
  } catch (error) {
    console.error('Failed to get coupon from API:', error)
    // If API call fails, default to blank
    targetIndex = blankIndex
    Swal.fire({
      title: 'Lỗi!',
      text: 'Đã xảy ra lỗi khi nhận kết quả từ máy chủ. Vui lòng thử lại.',
      icon: 'error',
    })
    // spinConsumed remains false, as the spin was not successfully completed due to network error
  } finally {
    running = false
    cancelAnimationFrame(frameId)

    // Calculate final rotation to stop at targetIndex
    const finalAngle = 360 - (targetIndex * arc.value + arc.value / 2)
    const currentDeg = rotation.value % 360
    let delta = finalAngle - currentDeg
    if (delta < 0) delta += 360
    const smoothRotation = currentRotation + delta + 360 * 2 // 2 extra rounds for effect
    rotation.value = smoothRotation

    setTimeout(() => {
      if (couponData) {
        const winningPrize = {
          name: couponData.isPercent
            ? `Giảm ${couponData.phanTramGiam}%`
            : `Giảm ${formatCurrency(couponData.soTienGiam)}`,
          code: couponData.maCode,
          isPercent: couponData.isPercent,
          revealed: true,
          isBlank: false,
        }
        prizes.value.splice(targetIndex, 1, winningPrize)
        selectedPrize.value = winningPrize
        triggerFireworks() // Trigger fireworks on win
      } else {
        selectedPrize.value = prizes.value[targetIndex] // This will be the blank prize or error blank
      }
      spinning.value = false
      if (spinConsumed) {
        // Only increment if the spin was successfully consumed
        spinCount.value++
      }
    }, 2000) // Adjust timeout as needed for animation
  }
}

const copyCode = (code) => {
  if (!code || !navigator.clipboard) return
  navigator.clipboard
    .writeText(code)
    .then(() => {
      copied.value = true
      setTimeout(() => {
        copied.value = false
      }, 1500)
    })
    .catch((err) => {
      console.error('Failed to copy code:', err)
      Swal.fire({ title: 'Lỗi', text: 'Không thể sao chép mã.', icon: 'error' })
    })
}

const checkSpinCount = async () => {
  checkingCoupon.value = true
  try {
    const res = await getFromApi('/WheelCoupon/time-spin-wheel-coupon')
    if (res && res.success) {
      const spins = Number(res.data) || 0
      maxSpins.value = spins // Update maxSpins with the fetched value
      spinCount.value = 0 // Reset spin count as we are getting the fresh data
      selectedPrize.value = null // Clear previous prize
      fetchPrizeList() // Regenerate the prize list
      Swal.fire({
        title: 'Lượt quay của bạn',
        text: `Bạn hiện có ${spins} lượt quay.`,
        icon: 'info',
        confirmButtonText: 'Đã hiểu',
      })
    } else {
      Swal.fire({
        title: 'Lỗi',
        text: res?.message || 'Không thể kiểm tra lượt quay. Vui lòng thử lại.',
        icon: 'error',
      })
    }
  } catch (error) {
    console.error('Failed to check spin count:', error)
    Swal.fire({
      title: 'Lỗi',
      text: 'Đã xảy ra lỗi khi kiểm tra lượt quay. Vui lòng thử lại.',
      icon: 'error',
    })
  } finally {
    checkingCoupon.value = false
  }
}
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
