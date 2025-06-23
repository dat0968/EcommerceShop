<template>
  <div class="wheel-container">
    <div class="wheel-wrapper">
      <svg
        :width="size"
        :height="size"
        :style="{ transform: `rotate(${rotation}deg)` }"
        class="wheel-svg"
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
            class="wheel-label"
            :transform="getTextTransform(idx)"
          >
            {{ item.name }}
          </text>
        </g>
      </svg>
      <div class="wheel-pointer">▼</div>
    </div>
    <button class="spin-btn" :disabled="spinning || spinCount >= maxSpins" @click="spin">
      Quay ({{ maxSpins - spinCount }} lượt còn lại)
    </button>
    <div v-if="selectedIndex !== null" class="result-box">
      <h3>Kết quả:</h3>
      <div>
        <b>{{ codes[selectedIndex].name }}</b>
      </div>
      <div class="code-copy-box">
        Mã code:
        <code class="copyable" @click="copyCode(codes[selectedIndex].code)">{{
          codes[selectedIndex].code
        }}</code>
      </div>
      <transition name="fade">
        <div v-if="copied" class="copied-msg">Đã copy!</div>
      </transition>
    </div>
  </div>
</template>

<script>
// Hàm vẽ cung tròn SVG
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

export default {
  name: 'WheelRandomCode',
  data() {
    return {
      codes: [
        { name: 'Code 1', code: 'ABC123' },
        { name: 'Code 2', code: 'XYZ789' },
        { name: 'Code 3', code: 'LMN456' },
        { name: 'Code 4', code: 'QWE321' },
        { name: 'Code 5', code: 'RTY654' },
      ],
      colors: ['#FFB300', '#FF7043', '#66BB6A', '#42A5F5', '#AB47BC', '#EC407A', '#26C6DA'],
      size: 320,
      rotation: 0,
      spinning: false,
      selectedIndex: null,
      arc: 360 / 5, // Số phần bằng số code
      maxSpins: 3,
      spinCount: 0,
      copied: false,
    }
  },
  methods: {
    describeArc(cx, cy, r, startAngle, endAngle) {
      return describeArc(cx, cy, r, startAngle, endAngle)
    },
    getTextPos(idx) {
      // Đặt text ở giữa mỗi mảng, căn đều theo cung tròn
      const angle = (idx + 0.5) * this.arc
      const r = this.size / 2 - 40 // Đưa text gần mép hơn
      const cx = this.size / 2
      const cy = this.size / 2
      const pos = polarToCartesian(cx, cy, r, angle)
      return pos
    },
    getTextTransform(idx) {
      // Xoay text để luôn hướng ra ngoài, căn đều với mảng
      const angle = (idx + 0.5) * this.arc
      const cx = this.size / 2
      const cy = this.size / 2
      return `rotate(${angle} ${this.getTextPos(idx).x} ${this.getTextPos(idx).y})`
    },
    spin() {
      if (this.spinning || this.spinCount >= this.maxSpins) return
      this.spinning = true
      this.selectedIndex = null
      this.copied = false
      // Quay ít nhất 3 vòng, rồi dừng ở 1 phần ngẫu nhiên
      const minRounds = 3
      const randomIdx = Math.floor(Math.random() * this.codes.length)
      const finalDeg = 360 * minRounds + (360 - randomIdx * this.arc - this.arc / 2)
      const duration = 3500
      // Animate
      const start = this.rotation % 360
      const change = finalDeg - start
      const startTime = performance.now()
      const animate = (now) => {
        const elapsed = now - startTime
        if (elapsed < duration) {
          const ease = 1 - Math.pow(1 - elapsed / duration, 3) // easeOut
          this.rotation = start + change * ease
          requestAnimationFrame(animate)
        } else {
          this.rotation = start + change
          this.spinning = false
          this.selectedIndex = randomIdx
          this.spinCount++
        }
      }
      requestAnimationFrame(animate)
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
        // Fallback cho trình duyệt cũ
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

<style scoped>
.wheel-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-top: 24px;
}
.wheel-wrapper {
  position: relative;
  width: 320px;
  height: 320px;
}
.wheel-svg {
  border-radius: 50%;
  box-shadow: 0 2px 12px #0002;
  background: #fff;
}
.wheel-pointer {
  position: absolute;
  left: 50%;
  top: -24px;
  transform: translateX(-50%);
  font-size: 2rem;
  color: #e53935;
  font-weight: bold;
  z-index: 2;
}
.spin-btn {
  margin-top: 24px;
  padding: 10px 32px;
  font-size: 1.2rem;
  background: #42a5f5;
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}
.spin-btn:disabled {
  background: #bdbdbd;
  cursor: not-allowed;
}
.result-box {
  margin-top: 24px;
  padding: 16px 32px;
  background: #f5f5f5;
  border-radius: 8px;
  text-align: center;
  box-shadow: 0 2px 8px #0001;
  position: relative;
}
.code-copy-box {
  margin-top: 8px;
  font-size: 1.1rem;
}
.copyable {
  background: #e3f2fd;
  color: #1976d2;
  border-radius: 4px;
  padding: 2px 8px;
  margin-left: 4px;
  cursor: pointer;
  transition: background 0.2s;
}
.copyable:hover {
  background: #bbdefb;
}
.copied-msg {
  position: absolute;
  right: 16px;
  top: 8px;
  background: #66bb6a;
  color: #fff;
  padding: 2px 12px;
  border-radius: 12px;
  font-size: 0.95rem;
  animation: fadeout 1.2s linear;
}
@keyframes fadeout {
  0% {
    opacity: 1;
  }
  80% {
    opacity: 1;
  }
  100% {
    opacity: 0;
  }
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter,
.fade-leave-to {
  opacity: 0;
}
.wheel-label {
  font-size: 1rem;
  fill: #222;
  font-weight: bold;
  pointer-events: none;
  user-select: none;
}
</style>
