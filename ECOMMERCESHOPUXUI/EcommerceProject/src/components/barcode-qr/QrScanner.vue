<template>
  <div>
    <video ref="video" style="width: 100%; max-width: 320px" autoplay></video>
    <div v-if="error" class="text-danger">{{ error }}</div>
    <div v-if="scanned">Đã quét: {{ scanned }}</div>
  </div>
</template>

<script>
import { BrowserQRCodeReader } from '@zxing/browser'

export default {
  name: 'QrScanner',
  data() {
    return {
      scanned: '',
      error: '',
      codeReader: null,
    }
  },
  mounted() {
    this.codeReader = new BrowserQRCodeReader()
    this.codeReader.decodeFromVideoDevice(null, this.$refs.video, (result, err) => {
      if (result) {
        this.scanned = result.text
        this.$emit('scanned', this.scanned)
        this.codeReader.reset()
      }
      if (err && err.name !== 'NotFoundException') {
        this.error = 'Không thể nhận diện QR code'
      }
    })
  },
  beforeUnmount() {
    if (this.codeReader) {
      this.codeReader.reset()
    }
  },
}
</script>
