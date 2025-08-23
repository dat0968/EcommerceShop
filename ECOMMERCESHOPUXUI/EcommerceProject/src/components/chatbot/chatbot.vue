<template>
  <div>
    <!-- Floating Chat Icon -->
    <div class="chatbot-icon" @click="toggleChat">
      💬
    </div>

    <!-- Chatbot Panel -->
    <transition name="slide-up">
      <div v-if="isOpen" class="chatbot-panel">
        <div class="chatbot-header">
          <span>Chatbot hỗ trợ</span>
          <button class="close-btn" @click="toggleChat">×</button>
        </div>

        <div class="chatbot-body" ref="chatBody">
          <div v-for="(msg, i) in messages" :key="i" class="message" :class="msg.sender">
            <div class="bubble" v-html="msg.text"></div>
          </div>
          <div v-if="isLoading" class="loading">Bot đang trả lời...</div>
        </div>

        <form class="chatbot-input" @submit.prevent="sendMessage">
          <input
            v-model="userInput"
            type="text"
            placeholder="Nhập tin nhắn..."
            :disabled="isLoading"
          />
          <button type="submit" :disabled="!userInput.trim() || isLoading">Gửi</button>
        </form>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import axios from 'axios'
import { GetApiUrl } from '@/constants/api'
const getUrlAPI = ref(GetApiUrl())
const isOpen = ref(false)
const userInput = ref('')
const messages = ref([
  { sender: 'bot', text: 'Xin chào! Tôi có thể giúp gì cho bạn?' },
])
const isLoading = ref(false)
const chatBody = ref(null)

const toggleChat = () => {
  isOpen.value = !isOpen.value
  nextTick(() => {
    scrollToBottom()
  })
}

const sendMessage = async () => {
  const input = userInput.value.trim()
  if (!input) return

  messages.value.push({ sender: 'user', text: input })
  userInput.value = ''
  isLoading.value = true
  scrollToBottom()

  try {
    const res = await axios.get(`${getUrlAPI.value}/api/Chatbot?request=${encodeURIComponent(input)}`)
    messages.value.push({ sender: 'bot', text: res.data })
  } catch (e) {
    messages.value.push({ sender: 'bot', text: '❌ Lỗi khi kết nối chatbot.' })
  } finally {
    isLoading.value = false
    scrollToBottom()
  }
}

const scrollToBottom = () => {
  nextTick(() => {
    if (chatBody.value) {
      chatBody.value.scrollTop = chatBody.value.scrollHeight
    }
  })
}
</script>

<style scoped>
.chatbot-icon {
  position: fixed;
  bottom: 20px;
  right: 20px;
  background: #007bff;
  color: white;
  font-size: 24px;
  padding: 12px 16px;
  border-radius: 50%;
  cursor: pointer;
  z-index: 1000;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
}

.chatbot-panel {
  position: fixed;
  bottom: 80px;
  right: 20px;
  width: 320px;
  max-height: 500px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  z-index: 999;
}

.chatbot-header {
  background: #007bff;
  color: white;
  padding: 12px;
  font-weight: bold;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.close-btn {
  background: none;
  border: none;
  color: white;
  font-size: 20px;
  cursor: pointer;
}

.chatbot-body {
  flex-grow: 1;
  overflow-y: auto;
  padding: 12px;
  background: #f9f9f9;
}

.message {
  margin-bottom: 10px;
  display: flex;
}

.message.user {
  justify-content: flex-end;
}

.message.bot {
  justify-content: flex-start;
}

.bubble {
  max-width: 80%;
  padding: 10px;
  border-radius: 18px;
  background: #e0e0e0;
  line-height: 1.4;
}

.message.user .bubble {
  background: #007bff;
  color: white;
  border-bottom-right-radius: 0;
}

.message.bot .bubble {
  background: #f0f0f0;
  color: #333;
  border-bottom-left-radius: 0;
}

.chatbot-input {
  display: flex;
  padding: 8px;
  border-top: 1px solid #ddd;
  background: #fff;
}

.chatbot-input input {
  flex: 1;
  padding: 8px 12px;
  border-radius: 20px;
  border: 1px solid #ccc;
  font-size: 14px;
  outline: none;
}

.chatbot-input button {
  margin-left: 8px;
  padding: 8px 14px;
  border: none;
  background: #007bff;
  color: white;
  border-radius: 20px;
  cursor: pointer;
}

.loading {
  font-size: 13px;
  color: gray;
  text-align: center;
  margin-top: 8px;
}

.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease, opacity 0.3s ease;
}
.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(20px);
  opacity: 0;
}
</style>
