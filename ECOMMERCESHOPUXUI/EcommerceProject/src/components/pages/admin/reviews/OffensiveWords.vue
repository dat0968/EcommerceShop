<template>
  <div>
    <!-- Nút mở modal -->
    <button class="btn btn-outline-danger mb-2" @click="showModal = true">Quản lý từ cấm</button>

    <!-- Modal quản lý từ cấm -->
    <div v-if="showModal" class="modal-backdrop fade show" style="z-index: 1050"></div>
    <div v-if="showModal" class="modal d-block" tabindex="-1" style="z-index: 1060">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Danh sách từ cấm</h5>
            <button type="button" class="btn-close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <!-- Thanh tìm kiếm -->
            <input
              v-model="search"
              class="form-control mb-3"
              placeholder="Tìm từ cấm..."
              @input="filterWords"
            />

            <!-- Thêm từ mới -->
            <form @submit.prevent="addWords">
              <div class="input-group mb-3">
                <input
                  v-model="newWords"
                  class="form-control"
                  placeholder="Nhập từ cấm, cách nhau bằng dấu ','"
                />
                <button class="btn btn-success" type="submit">Thêm</button>
              </div>
            </form>

            <!-- Danh sách từ cấm dạng badge -->
            <div class="mb-3" style="min-height: 48px">
              <span
                v-for="(word, idx) in filteredWords"
                :key="word"
                class="badge bg-danger me-2 mb-2"
                style="font-size: 1em"
              >
                {{ word }}
                <span class="ms-1" style="cursor: pointer" title="Xóa" @click="removeWord(word)"
                  >&times;</span
                >
              </span>
            </div>

            <!-- Nút xóa tất cả từ đang hiển thị -->
            <button
              class="btn btn-outline-danger btn-sm mb-2"
              @click="removeAllFiltered"
              :disabled="!filteredWords.length"
            >
              Xóa tất cả từ đang hiển thị
            </button>

            <!-- Backup file -->
            <div class="mt-3">
              <button class="btn btn-outline-secondary btn-sm" @click="downloadBackup">
                Tải file backup
              </button>
              <input
                type="file"
                accept=".json"
                class="form-control d-inline-block w-auto ms-2"
                @change="importBackup"
                style="display: inline-block; width: auto"
              />
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeModal">Đóng</button>
            <button class="btn btn-primary" @click="saveToFile">Lưu thay đổi</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// Đường dẫn file từ cấm (có thể cần điều chỉnh khi build)
import defaultWords from '@/assets/default/texts/vn_offensive_words.json'

export default {
  name: 'OffensiveWords',
  data() {
    return {
      showModal: false,
      words: [],
      search: '',
      filteredWords: [],
      newWords: '',
      filePath: 'src/assets/default/texts/vn_offensive_words.json', // chỉ dùng cho backup
    }
  },
  created() {
    // Khởi tạo từ cấm từ file json
    this.words = Array.isArray(defaultWords) ? [...defaultWords] : []
    this.filteredWords = [...this.words]
  },
  methods: {
    closeModal() {
      this.showModal = false
      this.search = ''
      this.newWords = ''
      this.filterWords()
    },
    filterWords() {
      const s = this.search.trim().toLowerCase()
      this.filteredWords = s
        ? this.words.filter((w) => w.toLowerCase().includes(s))
        : [...this.words]
    },
    addWords() {
      if (!this.newWords.trim()) return
      const arr = this.newWords
        .split(',')
        .map((w) => w.trim())
        .filter((w) => w.length > 0)
      let added = 0
      arr.forEach((w) => {
        if (!this.words.includes(w)) {
          this.words.push(w)
          added++
        }
      })
      this.newWords = ''
      this.filterWords()
      if (added) this.$nextTick(() => alert(`Đã thêm ${added} từ mới!`))
    },
    removeWord(word) {
      this.words = this.words.filter((w) => w !== word)
      this.filterWords()
    },
    removeAllFiltered() {
      if (!confirm('Bạn chắc chắn muốn xóa tất cả các từ đang hiển thị?')) return
      this.words = this.words.filter((w) => !this.filteredWords.includes(w))
      this.filterWords()
    },
    saveToFile() {
      // Tải file json mới về máy (không ghi đè file gốc trên server)
      const blob = new Blob([JSON.stringify(this.words, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'vn_offensive_words.json'
      a.click()
      URL.revokeObjectURL(url)
      alert('Đã tải file từ cấm mới về máy. Để cập nhật lên server, hãy upload file này.')
    },
    downloadBackup() {
      // Tải file backup hiện tại
      const blob = new Blob([JSON.stringify(this.words, null, 2)], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'vn_offensive_words_backup.json'
      a.click()
      URL.revokeObjectURL(url)
    },
    importBackup(e) {
      const file = e.target.files[0]
      if (!file) return
      const reader = new FileReader()
      reader.onload = (evt) => {
        try {
          const arr = JSON.parse(evt.target.result)
          if (Array.isArray(arr)) {
            this.words = arr
            this.filterWords()
            alert('Đã nhập file backup thành công!')
          } else {
            alert('File không hợp lệ!')
          }
        } catch {
          alert('File không hợp lệ!')
        }
      }
      reader.readAsText(file)
    },
  },
}
</script>

<style scoped>
.badge {
  font-size: 1em;
  padding: 0.6em 1em;
  display: inline-flex;
  align-items: center;
}
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
