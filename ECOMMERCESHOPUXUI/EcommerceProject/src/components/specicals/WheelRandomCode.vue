<template>
  <a href="#" @click.prevent="showModal = true">
    <span class="icon_ribbon_alt"></span>
    <div v-if="maxSpins > 0" class="tip">{{ maxSpins - spinCount }}</div>
  </a>

  <div
    v-if="showModal"
    class="modal fade show d-block"
    tabindex="-1"
    style="background: rgba(0, 0, 0, 0.45)"
    @click.self="showModal = false"
  >
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content p-4 position-relative">
        <div class="modal-header text-center">
          <h5 class="modal-title w-100">Vòng quay may mắn</h5>
          <button
            type="button"
            class="btn-close position-absolute end-0 top-0 m-3"
            @click="showModal = false"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body d-flex flex-column align-items-center">
          <div class="position-relative" :style="{ width: `${size}px`, height: `${size}px` }">
            <svg
              :width="size"
              :height="size"
              :style="{ transform: `rotate(${rotation}deg)`, transition: 'transform 4s ease-out' }"
              style="border-radius: 50%; box-shadow: 0 2px 12px #0002; background: #fff"
            >
              <g v-for="(item, idx) in prizes" :key="idx">
                <path
                  :d="describeArc(size / 2, size / 2, size / 2 - 10, idx * arc, (idx + 1) * arc)"
                  :fill="item.isBlank ? '#BDBDBD' : colors[idx % colors.length]"
                  stroke="#fff"
                  stroke-width="2"
                />
                <text
                  :x="getTextPos(idx).x"
                  :y="getTextPos(idx).y"
                  text-anchor="middle"
                  alignment-baseline="middle"
                  font-size="14"
                  font-weight="bold"
                  fill="#222"
                  :transform="getTextTransform(idx)"
                  style="pointer-events: none; user-select: none"
                >
                  {{ item.isBlank ? item.name : item.revealed ? item.name : '?' }}
                </text>
              </g>
            </svg>
            <div
              class="position-absolute top-0 start-50 translate-middle-x"
              style="font-size: 2rem; color: #e53935; font-weight: bold; z-index: 2"
            >
              ▼
            </div>
          </div>
          <button
            class="btn btn-primary mt-4 px-4"
            :disabled="spinning || spinCount >= maxSpins"
            @click="spin"
          >
            <span v-if="spinning">Đang quay...</span>
            <span v-else>Quay ({{ maxSpins - spinCount }} lượt còn lại)</span>
          </button>
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
            <div>
              <b>{{ selectedPrize.name }}</b>
            </div>
            <div v-if="!selectedPrize.isBlank && selectedPrize.revealed" class="mt-2">
              Mã code:
              <code
                class="bg-light text-primary rounded px-2 py-1 ms-1"
                style="cursor: pointer"
                @click="copyCode(selectedPrize.code)"
                >{{ selectedPrize.code }}</code
              >
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

// --- Component State ---
const showModal = ref(false)
const prizes = ref([])
const colors = ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA']
const size = 320
const rotation = ref(0)
const spinning = ref(false)
const selectedPrize = ref(null)
const maxSpins = ref(0)
const spinCount = ref(0)
const copied = ref(false)

// --- Computed Properties ---
const arc = computed(() => (prizes.value.length > 0 ? 360 / prizes.value.length : 0))

// --- Wheel Drawing Utilities ---
const polarToCartesian = (cx, cy, r, angle) => {
  const a = ((angle - 90) * Math.PI) / 180.0
  return { x: cx + r * Math.cos(a), y: cy + r * Math.sin(a) }
}

const describeArc = (cx, cy, r, startAngle, endAngle) => {
  const start = polarToCartesian(cx, cy, r, endAngle)
  const end = polarToCartesian(cx, cy, r, startAngle)
  const largeArcFlag = endAngle - startAngle <= 180 ? '0' : '1'
  return `M ${cx} ${cy} L ${start.x} ${start.y} A ${r} ${r} 0 ${largeArcFlag} 0 ${end.x} ${end.y} Z`
}

const getTextPos = (idx) => {
  const angle = (idx + 0.5) * arc.value
  const r = size / 2 - 70 // Place text closer to the center
  return polarToCartesian(size / 2, size / 2, r, angle)
}

const getTextTransform = (idx) => {
  const angle = (idx + 0.5) * arc.value
  const pos = getTextPos(idx)
  return `rotate(${angle - 90} ${pos.x} ${pos.y})` // Orient text towards the center
}

// --- API & Data Logic ---
const fetchPrizeList = () => {
  // Prize templates chỉ để xác định số lượng và loại, không hiển thị giá trị cụ thể
  const prizeTemplates = [{}, {}, {}, {}, {}, {}, {}, {}, {}, {}]

  // Ban đầu các ô trúng thưởng chỉ hiển thị "?"
  const generatedPrizes = prizeTemplates.map((prize) => ({
    ...prize,
    name: '?',
    code: '',
    moTa: '',
    phanTramGiam: null,
    soTienGiam: null,
    isBlank: false,
    revealed: false, // Đã nhận thông tin từ API hay chưa
  }))

  // Add blank prizes to achieve ~75% chance of losing
  const blankPrize = { name: 'Chúc bạn may mắn lần sau', isBlank: true }
  const blankPrizes = Array(generatedPrizes.length * 3).fill(blankPrize)

  // Shuffle the prizes for a random layout on the wheel
  const allPrizes = [...generatedPrizes, ...blankPrizes]
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
    if (res?.success) {
      maxSpins.value = Number(res.data) || 0
    } else {
      maxSpins.value = 0
    }
  } catch (error) {
    maxSpins.value = 0
    console.error('Failed to get spin count:', error)
  }

  fetchPrizeList()

  const wheelSwalDate = localStorage.getItem('wheel_swal_date') || ''
  if (maxSpins.value > 0 && today !== wheelSwalDate) {
    Swal.fire({
      title: 'Vòng quay may mắn!',
      text: 'Bạn có lượt quay miễn phí hôm nay, muốn thử vận may ngay không?',
      icon: 'info',
      showCancelButton: true,
      confirmButtonText: 'Quay ngay',
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
const spin = async () => {
  if (spinning.value || spinCount.value >= maxSpins.value) return

  spinning.value = true
  selectedPrize.value = null
  copied.value = false

  const winChance = Math.random()
  let prizeIndex

  if (winChance < 0.75) {
    // --- LOSE (75% chance) ---
    const blankIndices = prizes.value.map((p, i) => (p.isBlank ? i : -1)).filter((i) => i !== -1)
    prizeIndex = blankIndices[Math.floor(Math.random() * blankIndices.length)]
  } else {
    // --- WIN (25% chance) ---
    const winningIndices = prizes.value.map((p, i) => (!p.isBlank ? i : -1)).filter((i) => i !== -1)
    prizeIndex = winningIndices[Math.floor(Math.random() * winningIndices.length)]
  }

  const minRounds = 5
  const finalAngle = 360 * minRounds + (360 - prizeIndex * arc.value - arc.value / 2)

  rotation.value += finalAngle

  setTimeout(async () => {
    const finalRotation = rotation.value % 360
    rotation.value = finalRotation
    spinning.value = false
    spinCount.value++
    selectedPrize.value = prizes.value[prizeIndex]
    // Nếu là ô trúng thưởng thì gọi API lấy coupon và cập nhật lại prize
    if (!selectedPrize.value.isBlank && !selectedPrize.value.revealed) {
      try {
        const res = await postToApi('/WheelCoupon/private-coupon')
        if (res && res.success && res.data) {
          // Cập nhật lại prize vừa quay với thông tin từ API
          const coupon = res.data
          const updatedPrize = {
            ...selectedPrize.value,
            name: coupon.isPercent
              ? `Giảm ${coupon.phanTramGiam}%`
              : `Giảm ${formatCurrency(coupon.soTienGiam)}`,
            code: coupon.maCode,
            moTa: coupon.moTa,
            phanTramGiam: coupon.phanTramGiam,
            soTienGiam: coupon.soTienGiam,
            isPercent: coupon.isPercent,
            revealed: true,
          }
          // Cập nhật vào prizes để lần sau hiển thị đúng
          prizes.value.splice(prizeIndex, 1, updatedPrize)
          selectedPrize.value = updatedPrize
        } else {
          // Nếu lỗi thì vẫn để prize là ?
          selectedPrize.value = { ...selectedPrize.value, name: '?', code: '', revealed: false }
        }
      } catch (error) {
        selectedPrize.value = { ...selectedPrize.value, name: '?', code: '', revealed: false }
        Swal.fire({
          title: 'Lỗi!',
          text: 'Không thể tạo mã giảm giá. Vui lòng thử lại.',
          icon: 'error',
        })
      }
    }
  }, 4000)
}

// Đã xử lý logic tạo coupon trong spin, không cần hàm này nữa

const copyCode = (code) => {
  if (!code) return
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
      // Fallback for older browsers
      const textarea = document.createElement('textarea')
      textarea.value = code
      textarea.style.position = 'fixed'
      document.body.appendChild(textarea)
      textarea.select()
      try {
        document.execCommand('copy')
        copied.value = true
        setTimeout(() => {
          copied.value = false
        }, 1500)
      } catch (fallbackErr) {
        console.error('Fallback copy failed:', fallbackErr)
      }
      document.body.removeChild(textarea)
    })
}
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
