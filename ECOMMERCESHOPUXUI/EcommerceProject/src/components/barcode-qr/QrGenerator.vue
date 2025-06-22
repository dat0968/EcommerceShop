<template>
  <div class="row justify-content-center text-center gap-2">
    <canvas ref="qrcanvas"></canvas>
    <button @click="downloadQr" v-if="isValid" class="btn btn-outline-success">Tải ảnh</button>
    <div v-if="!isValid" class="text-danger">Dữ liệu không hợp lệ để tạo QR!</div>
  </div>
</template>

<script>
import QRCode from 'qrcode'

export default {
  name: 'QrGenerator',
  props: {
    value: { type: String, required: true },
  },
  data() {
    return { isValid: true }
  },
  watch: {
    value: {
      immediate: true,
      handler(val) {
        this.renderQr(val)
      },
    },
  },
  methods: {
    async renderQr(val) {
      await this.$nextTick()
      if (!val || val.length < 1 || val.length > 256) {
        this.isValid = false
        const ctx = this.$refs.qrcanvas.getContext('2d')
        ctx && ctx.clearRect(0, 0, 256, 256)
        return
      }
      this.isValid = true
      QRCode.toCanvas(this.$refs.qrcanvas, val, { width: 200 }, (err) => {
        if (err) this.isValid = false
      })
    },
    downloadQr() {
      const canvas = this.$refs.qrcanvas
      const url = canvas.toDataURL('image/png')
      const a = document.createElement('a')
      a.href = url
      a.download = 'qrcode.png'
      a.click()
    },
  },
}
</script>

<style scoped>
canvas {
  width: 100%;
  height: 320px;
}
</style>
