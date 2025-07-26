<template>
  <div>
    <li><a href="#" @click="showModal = true">QR/Barcode</a></li>

    <!-- Modal -->
    <div v-if="showModal" class="modal-backdrop fade show" style="z-index: 1050"></div>
    <div v-if="showModal" class="modal d-block" tabindex="-1" style="z-index: 1060">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Test QR/Barcode</h5>
            <button type="button" class="btn-close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <!-- Tabs QR/Barcode -->
            <ul class="nav nav-tabs mb-3">
              <li class="nav-item">
                <a
                  class="nav-link"
                  :class="{ active: mainTab === 'qr' }"
                  href="#"
                  @click.prevent="mainTab = 'qr'"
                  >QR Code</a
                >
              </li>
              <li class="nav-item">
                <a
                  class="nav-link"
                  :class="{ active: mainTab === 'barcode' }"
                  href="#"
                  @click.prevent="mainTab = 'barcode'"
                  >Barcode</a
                >
              </li>
            </ul>
            <!-- Sub-tabs Generate/Scan -->
            <div v-if="mainTab === 'qr'">
              <ul class="nav nav-tabs mb-2">
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: qrTab === 'generate' }"
                    href="#"
                    @click.prevent="qrTab = 'generate'"
                    >Generate</a
                  >
                </li>
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: qrTab === 'scan' }"
                    href="#"
                    @click.prevent="qrTab = 'scan'"
                    >Scan</a
                  >
                </li>
              </ul>
              <div v-if="qrTab === 'generate'">
                <div class="mb-2">
                  <input
                    v-model="qrValue"
                    class="form-control"
                    placeholder="Nhập dữ liệu QR code..."
                  />
                </div>
                <QrGenerator :value="qrValue" />
              </div>
              <div v-else>
                <QrScanner @scanned="onQrScanned" />
                <div v-if="qrScanned" class="alert alert-success mt-2">
                  Kết quả: {{ qrScanned }}
                </div>
              </div>
            </div>
            <div v-else>
              <ul class="nav nav-tabs mb-2">
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: barcodeTab === 'generate' }"
                    href="#"
                    @click.prevent="barcodeTab = 'generate'"
                    >Generate</a
                  >
                </li>
                <li class="nav-item">
                  <a
                    class="nav-link"
                    :class="{ active: barcodeTab === 'scan' }"
                    href="#"
                    @click.prevent="barcodeTab = 'scan'"
                    >Scan</a
                  >
                </li>
              </ul>
              <div v-if="barcodeTab === 'generate'">
                <div class="mb-2">
                  <input
                    v-model="barcodeValue"
                    class="form-control"
                    placeholder="Nhập dữ liệu barcode..."
                  />
                </div>
                <BarcodeGenerator :value="barcodeValue" />
              </div>
              <div v-else>
                <BarcodeScanner @scanned="onBarcodeScanned" />
                <div v-if="barcodeScanned" class="alert alert-success mt-2">
                  Kết quả: {{ barcodeScanned }}
                </div>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeModal">Đóng</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import QrGenerator from '@/components/barcode-qr/QrGenerator.vue'
import QrScanner from '@/components/barcode-qr/QrScanner.vue'
import BarcodeGenerator from '@/components/barcode-qr/BarcodeGenerator.vue'
import BarcodeScanner from '@/components/barcode-qr/BarcodeScanner.vue'

export default {
  name: 'TestCodeQr',
  components: { QrGenerator, QrScanner, BarcodeGenerator, BarcodeScanner },
  data() {
    return {
      showModal: false,
      mainTab: 'qr',
      qrTab: 'generate',
      barcodeTab: 'generate',
      qrValue: '',
      barcodeValue: '',
      qrScanned: '',
      barcodeScanned: '',
    }
  },
  methods: {
    closeModal() {
      this.showModal = false
      this.qrScanned = ''
      this.barcodeScanned = ''
    },
    onQrScanned(val) {
      this.qrScanned = val
    },
    onBarcodeScanned(val) {
      this.barcodeScanned = val
    },
  },
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: #000;
  opacity: 0.3;
}
.modal {
  background: rgba(0, 0, 0, 0.08);
}
</style>
