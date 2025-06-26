<template>
  <a href="#" @click.prevent="showModal = !showModal">
    <span class="icon_ribbon_alt"></span>
    <div class="tip">{{ maxSpins - spinCount }}</div>
  </a>
  <div
    v-if="showModal"
    class="modal fade show d-block"
    tabindex="-1"
    style="background: rgba(0, 0, 0, 0.45)"
  >
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content p-4 position-relative">
        <div class="modal-header position-relative text-center">
          Vòng quay may mắn
          <button
            type="button"
            class="btn-close position-absolute end-0 top-0 m-3"
            @click="showModal = false"
          ></button>
        </div>
        <div class="d-flex flex-column align-items-center">
          <div class="position-relative" style="width: 320px; height: 320px">
            <svg
              :width="size"
              :height="size"
              :style="{ transform: `rotate(${rotation}deg)` }"
              style="border-radius: 50%; box-shadow: 0 2px 12px #0002; background: #fff"
            >
              <g v-for="(item, idx) in codes" :key="idx">
                <path
                  :d="describeArc(size / 2, size / 2, size / 2 - 10, idx * arc, (idx + 1) * arc)"
                  :fill="colors[idx % colors.length]"
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
                  {{ item.name }}
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
            Quay ({{ maxSpins - spinCount }} lượt còn lại)
          </button>
          <div v-if="selectedIndex !== null" class="alert alert-info mt-4 text-center w-100">
            <h5 class="mb-2">Kết quả:</h5>
            <div>
              <b>{{ codes[selectedIndex].name }}</b>
            </div>
            <div class="mt-2">
              Mã code:
              <code
                class="bg-light text-primary rounded px-2 py-1 ms-1"
                style="cursor: pointer"
                @click="copyCode(codes[selectedIndex].code)"
                >{{ codes[selectedIndex].code }}</code
              >
            </div>
            <transition name="fade">
              <div v-if="copied" class="text-success mt-2">Đã copy!</div>
            </transition>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ConfigsRequest from '@/models/ConfigsRequest'
import { getFromApi, postToApi, patchToApi } from '@/utils/axiosClient'
import Swal from 'sweetalert2'
function polarToCartesian(cx, cy, r, angle) {
  const a = ((angle - 90) * Math.PI) / 180.0
  return {
    x: cx + r * Math.cos(a),
    y: cy + r * Math.sin(a),
  }
}
function describeArc(cx, cy, r, startAngle, endAngle) {
  const start = polarToCartesian(cx, cy, r, endAngle)
  const end = polarToCartesian(cx, cy, r, startAngle)
  const largeArcFlag = endAngle - startAngle <= 180 ? '0' : '1'
  return [
    'M',
    cx,
    cy,
    'L',
    start.x,
    start.y,
    'A',
    r,
    r,
    0,
    largeArcFlag,
    0,
    end.x,
    end.y,
    'Z',
  ].join(' ')
}

function randomString(length = 10) {
  const chars = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
  let result = ''
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return result
}

function randomCouponList() {
  const coupons = []
  for (let i = 0; i < 10; i++) {
    const isPercent = Math.random() < 0.5
    let value, name
    if (isPercent) {
      value = Math.floor(Math.random() * 46) + 5 // 5% - 50%
      name = `Giảm ${value}%`
    } else {
      value = (Math.floor(Math.random() * 14) + 1) * 50000 // 50k - 700k
      name = `Giảm ${value.toLocaleString()}đ`
    }
    coupons.push({
      name,
      code: randomString(10),
      isPercent,
      value,
    })
  }
  return coupons
}

export default {
  name: 'WheelRandomCode',
  data() {
    return {
      showModal: false,
      codes: [],
      colors: ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA'],
      size: 320,
      rotation: 0,
      spinning: false,
      selectedIndex: null,
      arc: 36, // 360/10
      maxSpins: 0,
      spinCount: 0,
      copied: false,
      lastSpinDate: '',
    }
  },
  created() {
    this.initWheel()
  },
  methods: {
    async initWheel() {
      // Lấy ngày hiện tại
      const today = new Date().toISOString().slice(0, 10)
      this.lastSpinDate = localStorage.getItem('wheel_last_spin_date') || ''
      // Nếu khác ngày thì request cập nhật streak và số lượt quay
      if (this.lastSpinDate !== today) {
        try {
          await patchToApi(
            '/WheelCoupon/update-last-login-streak',
            '',
            ConfigsRequest.getSkipAuthConfig(),
          )
        } catch (e) {
          console.log('Request Error: ' + e)
        }
        localStorage.setItem('wheel_last_spin_date', today)
      }
      // Lấy số lượt quay thực tế từ API
      try {
        const res = await getFromApi('/WheelCoupon/time-spin-wheel-coupon')
        if (res && res.success && typeof res.data === 'boolean') {
          this.maxSpins = res.data ? 1 : 0 // Nếu true thì còn lượt quay, false thì hết
        } else if (res && res.success && typeof res.data === 'number') {
          this.maxSpins = res.data
        } else {
          this.maxSpins = 0 // fallback
        }
      } catch (e) {
        this.maxSpins = 0
        console.log('Request Error: ' + e)
      }
      // Sinh danh sách coupon mẫu
      this.codes = randomCouponList()
      this.arc = 360 / this.codes.length

      // Hiển thị Swal lần đầu trong ngày nếu còn lượt quay
      const wheelSwalDate = localStorage.getItem('wheel_swal_date') || ''
      if (this.maxSpins > 0 && today !== wheelSwalDate) {
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
            this.showModal = true
          }
          // Lưu lại để không hỏi lại trong ngày
          localStorage.setItem('wheel_swal_date', today)
        })
      }
    },
    describeArc(cx, cy, r, startAngle, endAngle) {
      return describeArc(cx, cy, r, startAngle, endAngle)
    },
    getTextPos(idx) {
      // Đặt text gần tâm hơn để không bị tràn ra ngoài
      const angle = (idx + 0.5) * this.arc
      const r = this.size / 2 - 70 // Giảm bán kính để chữ nằm trong wheel
      const cx = this.size / 2
      const cy = this.size / 2
      const pos = polarToCartesian(cx, cy, r, angle)
      return pos
    },
    getTextTransform(idx) {
      const angle = (idx + 0.5) * this.arc
      // Xoay chữ về hướng tâm vòng quay
      return `rotate(${angle - 90} ${this.getTextPos(idx).x} ${this.getTextPos(idx).y})`
    },
    async spin() {
      if (this.spinning || this.spinCount >= this.maxSpins) return
      this.spinning = true
      this.selectedIndex = null
      this.copied = false
      const minRounds = 3
      const randomIdx = Math.floor(Math.random() * this.codes.length)
      const finalDeg = 360 * minRounds + (360 - randomIdx * this.arc - this.arc / 2)
      const duration = 3500
      const start = this.rotation % 360
      const change = finalDeg - start
      const startTime = performance.now()
      const animate = (now) => {
        const elapsed = now - startTime
        if (elapsed < duration) {
          const ease = 1 - Math.pow(1 - elapsed / duration, 3)
          this.rotation = start + change * ease
          requestAnimationFrame(animate)
        } else {
          this.rotation = start + change
          this.spinning = false
          this.selectedIndex = randomIdx
          this.spinCount++
          this.handleSpinResult()
        }
      }
      requestAnimationFrame(animate)
    },
    async handleSpinResult() {
      // Khi quay xong, tạo coupon ngẫu nhiên tương ứng và gửi lên API
      const coupon = this.codes[this.selectedIndex]
      try {
        await postToApi('/WheelCoupon/private-coupon', null, {
          params: {
            couponCode: coupon.code,
            decreasePrice: coupon.value,
            isPercent: coupon.isPercent,
          },
        })
      } catch (e) {
        console.log('Request Error: ' + e)
      }
    },
    copyCode(code) {
      if (!code) return
      if (navigator && navigator.clipboard) {
        navigator.clipboard.writeText(code).then(() => {
          this.copied = true
          setTimeout(() => {
            this.copied = false
          }, 1200)
        })
      } else {
        const textarea = document.createElement('textarea')
        textarea.value = code
        document.body.appendChild(textarea)
        textarea.select()
        document.execCommand('copy')
        document.body.removeChild(textarea)
        this.copied = true
        setTimeout(() => {
          this.copied = false
        }, 1200)
      }
    },
  },
  watch: {
    codes: {
      handler() {
        this.arc = 360 / this.codes.length
      },
      immediate: true,
    },
  },
}
</script>

<!-- Không cần style custom, chỉ giữ lại style SVG nếu cần -->
