<template>
  <div class="row justify-content-center text-center gap-2">
    <video ref="video" style="width: 100%; max-width: 320px" autoplay></video>
    <div v-if="error" class="text-danger">{{ error }}</div>
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
  async beforeUnmount() {
    await this.$nextTick()
    if (this.codeReader) {
      try {
        await this.codeReader.stopContinuousDecode()
      } catch (e) {
        // ? ignore if already stopped
      }
    }
  },
}
</script>
