<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { onSnapshot, collection, getFirestore } from 'firebase/firestore';

const isOnline = ref(navigator.onLine);
const isFirestoreConnected = ref(true);
const db = getFirestore();
let unsubscribeConnection = null;

// Cập nhật trạng thái online/offline của browser
const updateOnlineStatus = () => {
  isOnline.value = navigator.onLine;
};

// Lắng nghe trạng thái kết nối của browser
onMounted(() => {
  window.addEventListener('online', updateOnlineStatus);
  window.addEventListener('offline', updateOnlineStatus);
  
  // Lắng nghe trạng thái kết nối đến Firestore
  try {
    // Sử dụng một collection trống để kiểm tra kết nối
    unsubscribeConnection = onSnapshot(
      collection(db, '_connection_test_'), 
      () => {
        isFirestoreConnected.value = true;
      },
      (error) => {
        console.error('Firestore connection error:', error);
        isFirestoreConnected.value = false;
      }
    );
  } catch (error) {
    console.error('Failed to set up Firestore connection listener:', error);
  }
});

onUnmounted(() => {
  window.removeEventListener('online', updateOnlineStatus);
  window.removeEventListener('offline', updateOnlineStatus);
  
  if (unsubscribeConnection) {
    unsubscribeConnection();
  }
});
</script>

<template>
  <div class="connection-status" :class="{ 'offline': !isOnline || !isFirestoreConnected }" >
    <div v-if="isOnline && isFirestoreConnected" class="status-indicator online">
      <i class="bi bi-wifi"></i>
      <span class="status-text">Đã kết nối</span>
    </div>
    <div v-else class="status-indicator offline">
      <i class="bi bi-wifi-off"></i>
      <span class="status-text">Mất kết nối</span>
    </div>
  </div>
</template>

<style scoped>
.connection-status {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 1000;
  padding: 8px 12px;
  border-radius: 20px;
  background-color: rgba(255, 255, 255, 0.9);
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.connection-status.offline {
  background-color: #f8d7da;
  animation: pulse 2s infinite;
}

.status-indicator {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
}

.status-indicator.online {
  color: #198754;
}

.status-indicator.offline {
  color: #dc3545;
}

.status-indicator i {
  margin-right: 5px;
}

@keyframes pulse {
  0% {
    opacity: 1;
  }
  50% {
    opacity: 0.7;
  }
  100% {
    opacity: 1;
  }
}

@media (max-width: 768px) {
  .connection-status {
    bottom: 10px;
    right: 10px;
    padding: 5px 10px;
  }
  
  .status-text {
    display: none;
  }
  
  .status-indicator i {
    margin-right: 0;
    font-size: 1rem;
  }
}
</style>