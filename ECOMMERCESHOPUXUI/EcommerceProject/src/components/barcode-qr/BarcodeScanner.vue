<template>
  <div class="row justify-content-center text-center gap-2">
    <div ref="video" style="width: 100%; max-width: 320px"></div>
    <div v-if="error" class="text-danger">{{ error }}</div>
    <div v-if="scanned">Đã quét: {{ scanned }}</div>
  </div>
</template>

<script>
import Quagga from 'quagga'

export default {
  name: 'BarcodeScanner',
  data() {
    return {
      scanned: '',
      error: '',
    }
  },
  mounted() {
    Quagga.init(
      {
        inputStream: {
          type: 'LiveStream',
          target: this.$refs.video,
          constraints: { facingMode: 'environment' },
        },
        decoder: { readers: ['code_128_reader', 'ean_reader'] },
      },
      (err) => {
        if (err) {
          this.error = 'Không thể khởi tạo camera'
          return
        }
        Quagga.start()
      },
    )
    Quagga.onDetected(this.onDetected)
  },
  beforeUnmount() {
    Quagga.offDetected(this.onDetected)
    Quagga.stop()
  },
  methods: {
    onDetected(result) {
      if (result && result.codeResult && result.codeResult.code) {
        this.scanned = result.codeResult.code
        this.$emit('scanned', this.scanned)
        Quagga.stop()
      }
    },
  },
}
</script>
