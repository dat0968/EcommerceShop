<template>
  <div class="row justify-content-center text-center gap-2">
    <svg ref="barcode"></svg>
    <button @click="downloadBarcode" v-if="isValid" class="btn btn-outline-success">Tải ảnh</button>
    <div v-if="!isValid" class="text-danger">Dữ liệu không hợp lệ để tạo barcode!</div>
  </div>
</template>

<script>
import JsBarcode from 'jsbarcode'

export default {
  name: 'BarcodeGenerator',
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
        this.renderBarcode(val)
      },
    },
  },
  methods: {
    async renderBarcode(val) {
      await this.$nextTick()
      if (!val || val.length < 3 || val.length > 32) {
        this.isValid = false
        this.$refs.barcode.innerHTML = ''
        return
      }
      this.isValid = true
      JsBarcode(this.$refs.barcode, val, { format: 'CODE128', displayValue: true })
    },
    downloadBarcode() {
      const svg = this.$refs.barcode
      const serializer = new XMLSerializer()
      const svgStr = serializer.serializeToString(svg)
      const blob = new Blob([svgStr], { type: 'image/svg+xml' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'barcode.svg'
      a.click()
      URL.revokeObjectURL(url)
    },
  },
}
</script>
<!-- npm install @zxing/browser | qrcode | quagga | jsbarcode -->

<style scoped>
svg {
  width: 100%;
  height: 320px;
}
</style>
