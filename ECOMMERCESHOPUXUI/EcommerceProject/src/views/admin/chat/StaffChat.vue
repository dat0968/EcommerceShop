<script setup>
import { ref, onMounted, onUnmounted, computed, nextTick, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Cookies from 'js-cookie';
import Swal from 'sweetalert2';
import { GetApiUrl } from '@/constants/api'
import {
  rtdb,
  dbRef,
  onValue,
  set,
  push,
  rtdbServerTimestamp,
  off,
  get,
  update
} from '../../../firebase/rtdb-config.js';

import {
  ref as storageRef,
  uploadBytes,
  getDownloadURL
} from 'firebase/storage';
import { storage } from '../../../firebase/rtdb-config.js';

import { fetchWithAuth } from '@/services/authService.js';
import ConnectionStatus from '../../../components/ConnectionStatus.vue';
import { formatCurrency } from '@/constants/formatCurrency';


// ==================== STATES ====================
const getUrlAPI = ref(GetApiUrl())
const accessToken = ref(Cookies.get('accessToken'));
const staffId = ref(null);
const staffName = ref('');
const staffAvatar = ref('');
const staffRole = ref('');
const activeChats = ref([]);
const currentChat = ref(null);
const messages = ref([]);
const newMessage = ref('');
const imageFiles = ref([]);
const imagePreviews = ref([]);
const isLoading = ref(true);
const searchTerm = ref('');
const showEmojiPicker = ref(false);
const isTyping = ref(false);
const typingTimeout = ref(null);
const messagesListener = ref(null);
const chatsListener = ref(null);
const unreadCountTotal = ref(0);
const emojiList = ref(['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘']);
const customerInfo = ref(null);
const chatFilter = ref('all');
const isOnline = ref(navigator.onLine);

// State cho modal chuyển giao
const isTransferModalVisible = ref(false);
const selectedStaff = ref(null);
const availableStaff = ref([]);

// ==================== CONTROL FLAGS ====================
const isAuthenticating = ref(false);
const isLoadingChats = ref(false);
const lastAuthCheck = ref(0);
const AUTH_CHECK_INTERVAL = 5000; // 5 giây
const connectionChangeCount = ref(0);
const lastConnectionChange = ref(0);
const statusListeners = ref(new Map());

// ==================== AUTO UNLOCK STATES ====================
const autoUnlockTimeouts = ref(new Map()); // Map để track timeout cho từng chat
const INACTIVITY_TIMEOUT = 1 * 60 * 1000; // Thời gian khoá
const WARNING_THRESHOLD = 30 * 1000; // Cảnh báo trước 30 giây
const lastStaffActivity = ref(new Map()); // Track last activity của staff
const inactivityWarnings = ref(new Map()); // Track warnings đã hiển thị

const router = useRouter();
const route = useRoute();
const showSidebar = ref(false);
// Thêm vào phần đầu của <script setup>
const notificationsListener = ref(null);
const orderHistories = ref({});

const loadOrderHistories = async (customerIds) => {
  try {
    const response = await fetchWithAuth(`${getUrlAPI.value}/api/Chat/GetOrderHistories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(customerIds)
    });
    if (!response.ok) {
      throw new Error('Không thể tải lịch sử đơn hàng');
    }
    const result = await response.json();
    if (result.success) {
      result.data.forEach(customer => {
        orderHistories.value[customer.customerId] = customer.orders;
      });
    } else {
      throw new Error(result.message || 'Không thể tải lịch sử đơn hàng');
    }
  } catch (error) {
    console.error('Lỗi khi tải lịch sử đơn hàng:', error);
    orderHistories.value = {};
  }
};

// Thêm vào phần STATES
const showOrderDetailModal = ref(false);
const selectedOrder = ref(null);

// Cập nhật hàm viewOrderDetails
const viewOrderDetails = (orderId) => {
  if (!currentChat.value || !orderHistories.value[currentChat.value.customerId]) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không tìm thấy thông tin đơn hàng.',
      confirmButtonText: 'OK'
    });
    return;
  }

  const order = orderHistories.value[currentChat.value.customerId].find(o => o.orderId === orderId);
  if (!order) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: `Không tìm thấy đơn hàng với mã ${orderId}.`,
      confirmButtonText: 'OK'
    });
    return;
  }

  selectedOrder.value = order;
  showOrderDetailModal.value = true;
};
// Thêm hàm xử lý thông báo
const setupNotificationsListener = () => {
  const notificationRef = dbRef(rtdb, `notifications/${staffId.value}`);

  notificationsListener.value = onValue(notificationRef, async (snapshot) => {
    const notificationsData = snapshot.val();
    if (!notificationsData) return;

    for (const [notificationId, notification] of Object.entries(notificationsData)) {
      if (!notification.read) {
        await Swal.fire({
          icon: 'info',
          title: 'Thông báo chuyển giao',
          html: notification.message,
          confirmButtonText: 'Xem cuộc trò chuyện',
          showCancelButton: true,
          cancelButtonText: 'Đóng',
          timer: 5000,
          timerProgressBar: true
        }).then((result) => {
          if (result.isConfirmed && notification.chatId) {
            router.push(`/admin/chat/${notification.chatId}`);
          }
        });

        await set(dbRef(rtdb, `notifications/${staffId.value}/${notificationId}/read`), true);
      }
    }
  }, (error) => {
    console.error('Lỗi khi lắng nghe thông báo:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi thông báo',
      text: 'Không thể tải thông báo. Vui lòng thử lại sau.',
      confirmButtonText: 'OK'
    });
  });
};

// Cập nhật onMounted
onMounted(async () => {
  console.log('🚀 Component mounted');
  sessionStorage.removeItem('authErrorShown');
  sessionStorage.removeItem('chatLoadErrorShown');

  window.addEventListener('online', handleConnectionChange);
  window.addEventListener('offline', handleConnectionChange);

  const isAuthenticated = await checkAuth();
  if (isAuthenticated) {
    console.log('✅ Authentication successful');
    try {
      await cleanupActiveStatus();
      if (!isLoadingChats.value) {
        loadActiveChats();
      }
      updateUserStatus(navigator.onLine);
      loadAvailableStaff();
      setupNotificationsListener();
    } catch (error) {
      console.error('❌ Error during initialization:', error);
    }
  } else {
    console.log('❌ Authentication failed');
  }
  scrollToBottom();
});

onUnmounted(() => {
  console.log('🛑 Component unmounting');
  autoUnlockTimeouts.value.forEach((timeoutId, key) => {
    clearTimeout(timeoutId);
    console.log(`🚫 Cleared timeout for ${key}`);
  });
  autoUnlockTimeouts.value.clear();
  lastStaffActivity.value.clear();
  inactivityWarnings.value.clear();

  window.removeEventListener('online', handleConnectionChange);
  window.removeEventListener('offline', handleConnectionChange);

  if (messagesListener.value && currentChat.value) {
    off(dbRef(rtdb, `messages/${currentChat.value.id}`), 'value', messagesListener.value);
    messagesListener.value = null;
  }

  if (chatsListener.value) {
    off(dbRef(rtdb, 'conversations'), 'value', chatsListener.value);
    chatsListener.value = null;
  }

  if (notificationsListener.value) {
    off(dbRef(rtdb, `notifications/${staffId.value}`), 'value', notificationsListener.value);
    notificationsListener.value = null;
  }

  statusListeners.value.forEach((listener, customerId) => {
    if (listener) {
      off(dbRef(rtdb, `userStatus/${customerId}`), 'value', listener);
    }
  });
  statusListeners.value.clear();

  if (typingTimeout.value) {
    clearTimeout(typingTimeout.value);
    typingTimeout.value = null;
  }

  updateUserStatus(false);
  if (currentChat.value && !currentChat.value.isReadOnly) {
    unlockChat(currentChat.value.id, 'logout');
  }

  sessionStorage.removeItem('authErrorShown');
  sessionStorage.removeItem('chatLoadErrorShown');
  console.log('✅ Cleanup completed');
});

// Cập nhật onUnmounted để hủy listener thông báo
onUnmounted(() => {
  console.log('🛑 Component unmounting');

  // Clear all auto unlock timeouts
  autoUnlockTimeouts.value.forEach((timeoutId, key) => {
    clearTimeout(timeoutId);
    console.log(`🚫 Cleared timeout for ${key}`);
  });
  autoUnlockTimeouts.value.clear();
  lastStaffActivity.value.clear();
  inactivityWarnings.value.clear();

  // Remove event listeners
  window.removeEventListener('online', handleConnectionChange);
  window.removeEventListener('offline', handleConnectionChange);

  // Cleanup Firebase listeners
  if (messagesListener.value && currentChat.value) {
    off(dbRef(rtdb, `messages/${currentChat.value.id}`), 'value', messagesListener.value);
    messagesListener.value = null;
  }

  if (chatsListener.value) {
    off(dbRef(rtdb, 'conversations'), 'value', chatsListener.value);
    chatsListener.value = null;
  }

  if (notificationsListener.value) {
    off(dbRef(rtdb, `notifications/${staffId.value}`), 'value', notificationsListener.value);
    notificationsListener.value = null;
  }

  // Cleanup status listeners
  statusListeners.value.forEach((listener, customerId) => {
    if (listener) {
      off(dbRef(rtdb, `userStatus/${customerId}`), 'value', listener);
    }
  });
  statusListeners.value.clear();

  // Clear timeouts
  if (typingTimeout.value) {
    clearTimeout(typingTimeout.value);
    typingTimeout.value = null;
  }

  // Update status to offline and unlock current chat if any
  updateUserStatus(false);
  if (currentChat.value && !currentChat.value.isReadOnly) {
    unlockChat(currentChat.value.id, 'logout');
  }

  // Clear session flags
  sessionStorage.removeItem('authErrorShown');
  sessionStorage.removeItem('chatLoadErrorShown');

  console.log('✅ Cleanup completed');
});
const toggleSidebar = () => {
  showSidebar.value = !showSidebar.value;
};

// ==================== COMPUTED PROPERTIES ====================
const sortedMessages = computed(() => {
  return [...messages.value].sort((a, b) => a.timestamp - b.timestamp);
});

const filteredChats = computed(() => {
  let filtered = [...activeChats.value];

  if (chatFilter.value === 'active') {
    filtered = filtered.filter(chat => chat.isActive);
  } else if (chatFilter.value === 'unread') {
    filtered = filtered.filter(chat => chat.unreadStaff > 0);
  }

  if (searchTerm.value.trim()) {
    const term = searchTerm.value.toLowerCase().trim();
    filtered = filtered.filter(chat =>
      chat.customerName.toLowerCase().includes(term)
    );
  }

  return filtered;
});

// ==================== UTILITY FUNCTIONS ====================
const debounce = (func, delay) => {
  let timeoutId;
  return (...args) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func.apply(null, args), delay);
  };
};

const getImageUrl = (relativePath) => {
  if (!relativePath) return '/default-avatar.png';
  const fileName = relativePath.split('/').pop();
  if (relativePath.includes('AnhKhachHang') || relativePath.includes('AnhNhanVien')) {
    return `${getUrlAPI.value}/api/Customer/image/${fileName}`;
  }
  return `${getUrlAPI.value}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
};

const formatTime = (timestamp) => {
  if (!timestamp) return '';

  const date = new Date(timestamp);
  const now = new Date();

  if (date.toDateString() === now.toDateString()) {
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  }

  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const scrollToBottom = () => {
  nextTick(() => {
    const chatContainer = document.querySelector('.chat-messages');
    if (chatContainer) {
      chatContainer.scrollTop = chatContainer.scrollHeight;
    }
  });
};

// ==================== AUTO UNLOCK FUNCTIONS ====================

/**
 * Thiết lập timeout tự động mở khóa cho cuộc trò chuyện
 * @param {string} chatId - ID của cuộc trò chuyện
 * @param {number} customTimeout - Thời gian timeout tùy chỉnh (optional)
 */
const setupAutoUnlock = (chatId, customTimeout = INACTIVITY_TIMEOUT) => {
  // Clear timeout cũ nếu có
  clearAutoUnlock(chatId);

  console.log(`⏰ Thiết lập auto-unlock cho chat ${chatId} trong ${customTimeout / 1000}s`);

  const timeoutId = setTimeout(async () => {
    await handleAutoUnlock(chatId);
  }, customTimeout);

  autoUnlockTimeouts.value.set(chatId, timeoutId);
  lastStaffActivity.value.set(chatId, Date.now());

  // Thiết lập warning trước 30 giây
  const warningTimeoutId = setTimeout(() => {
    showInactivityWarning(chatId);
  }, customTimeout - WARNING_THRESHOLD);

  autoUnlockTimeouts.value.set(`${chatId}_warning`, warningTimeoutId);
};

/**
 * Hủy timeout tự động mở khóa
 * @param {string} chatId - ID của cuộc trò chuyện
 */
const clearAutoUnlock = (chatId) => {
  const timeoutId = autoUnlockTimeouts.value.get(chatId);
  const warningTimeoutId = autoUnlockTimeouts.value.get(`${chatId}_warning`);

  if (timeoutId) {
    clearTimeout(timeoutId);
    autoUnlockTimeouts.value.delete(chatId);
  }

  if (warningTimeoutId) {
    clearTimeout(warningTimeoutId);
    autoUnlockTimeouts.value.delete(`${chatId}_warning`);
  }

  inactivityWarnings.value.delete(chatId);
  console.log(`🚫 Đã hủy auto-unlock cho chat ${chatId}`);
};

/**
 * Reset timeout khi có hoạt động của staff
 * @param {string} chatId - ID của cuộc trò chuyện
 */
const resetAutoUnlock = (chatId) => {
  if (!chatId) return;

  lastStaffActivity.value.set(chatId, Date.now());
  setupAutoUnlock(chatId);

  // Clear warning nếu có
  if (inactivityWarnings.value.has(chatId)) {
    inactivityWarnings.value.delete(chatId);
    console.log(`✅ Đã reset warning cho chat ${chatId}`);
  }
};

/**
 * Xử lý tự động mở khóa cuộc trò chuyện
 * @param {string} chatId - ID của cuộc trò chuyện
 */
 const handleAutoUnlock = async (chatId) => {
  try {
    console.log(`🔓 Bắt đầu auto-unlock cho chat ${chatId}`);

    // Kiểm tra xem chat còn bị khóa bởi staff hiện tại không
    const chatRef = dbRef(rtdb, `conversations/${chatId}`);
    const snapshot = await get(chatRef);
    const chatData = snapshot.val();

    if (!chatData) {
      console.log(`❌ Chat ${chatId} không tồn tại`);
      return;
    }

    // Kiểm tra xem có phải staff hiện tại đang khóa không
    if (chatData.lockedByStaffId !== staffId.value) {
      console.log(`⚠️ Chat ${chatId} không được khóa bởi staff hiện tại`);
      return;
    }

    // Kiểm tra trạng thái online của nhân viên khóa chat
    const staffStatusRef = dbRef(rtdb, `userStatus/${chatData.lockedByStaffId}`);
    const staffSnapshot = await get(staffStatusRef);
    const staffStatus = staffSnapshot.val();

    if (!staffStatus || staffStatus.isOnline === false) {
      console.log(`🔓 Nhân viên ${chatData.lockedByStaffName} không online, mở khóa chat ${chatId}`);
      await unlockChat(chatId, 'offline');
      return;
    }

    // Kiểm tra tin nhắn để xác định có cần thông báo hay không
    const messagesRef = dbRef(rtdb, `messages/${chatId}`);
    const messagesSnapshot = await get(messagesRef);
    const messagesData = messagesSnapshot.val();

    let shouldShowNotification = true; // Mặc định hiện thông báo

    if (messagesData) {
      const messages = Object.values(messagesData);
      const lastCustomerMessage = messages
        .filter(msg => msg.loaiNguoiGui === 'customer')
        .sort((a, b) => b.thoiGian - a.thoiGian)[0];

      const lastStaffMessage = messages
        .filter(msg => msg.loaiNguoiGui === 'staff')
        .sort((a, b) => b.thoiGian - a.thoiGian)[0];

      // Nếu có tin nhắn staff sau tin nhắn customer cuối, không unlock
      if (lastStaffMessage && lastCustomerMessage &&
        lastStaffMessage.thoiGian > lastCustomerMessage.thoiGian) {
        console.log(`✅ Staff đã phản hồi, không unlock chat ${chatId}`);
        return;
      }

      // Nếu tin nhắn cuối từ customer cách đây ít hơn thời gian timeout, không unlock
      if (lastCustomerMessage &&
        (Date.now() - lastCustomerMessage.thoiGian) < INACTIVITY_TIMEOUT) {
        console.log(`⏰ Chưa đủ thời gian, không unlock chat ${chatId}`);
        setupAutoUnlock(chatId, INACTIVITY_TIMEOUT - (Date.now() - lastCustomerMessage.thoiGian));
        return;
      }

      // Kiểm tra xem staff có phản hồi sau tin nhắn cuối của customer không
      // Nếu có thì không cần hiện thông báo
      if (lastStaffMessage && lastCustomerMessage) {
        const timeSinceStaffReply = Date.now() - lastStaffMessage.thoiGian;
        const timeSinceCustomerMessage = Date.now() - lastCustomerMessage.thoiGian;
        
        // Nếu staff đã phản hồi trong vòng 5 phút gần đây, không hiện thông báo
        if (timeSinceStaffReply < 5 * 60 * 1000 && lastStaffMessage.thoiGian > lastCustomerMessage.thoiGian) {
          shouldShowNotification = false;
          console.log(`📝 Staff đã phản hồi gần đây, không hiện thông báo unlock`);
        }
      }
    }

    // Thực hiện unlock
    await unlockChat(chatId, 'auto', shouldShowNotification);

  } catch (error) {
    console.error(`❌ Lỗi khi auto-unlock chat ${chatId}:`, error);
  }
};

/**
 * Mở khóa cuộc trò chuyện
 * @param {string} chatId - ID của cuộc trò chuyện
 * @param {string} reason - Lý do unlock ('auto', 'manual', 'transfer', 'offline')
 * @param {boolean} showNotification - Có hiển thị thông báo hay không
 */
 const unlockChat = async (chatId, reason = 'manual', showNotification = true) => {
  try {
    const chatRef = dbRef(rtdb, `conversations/${chatId}`);

    // Ghi log unlock
    const unlockLogRef = dbRef(rtdb, `conversations/${chatId}/unlockHistory`);
    const newUnlockRef = push(unlockLogRef);
    await set(newUnlockRef, {
      staffId: staffId.value,
      staffName: staffName.value,
      reason: reason,
      timestamp: rtdbServerTimestamp(),
      unlockTime: Date.now(),
      notificationShown: showNotification
    });

    // Remove lock
    const updates = {
      lockedByStaffId: null,
      lockedByStaffName: null,
      lockedAt: null,
      ngayCapNhat: rtdbServerTimestamp()
    };

    await update(chatRef, updates);

    // Clear timeout
    clearAutoUnlock(chatId);

    // Chỉ thông báo cho staff nếu cần thiết
    if (reason === 'auto' && showNotification) {
      // Chỉ thông báo nếu đang ở trong chat đó
      if (currentChat.value && currentChat.value.id === chatId) {
        await Swal.fire({
          icon: 'warning',
          title: 'Cuộc trò chuyện đã được mở khóa',
          text: 'Do không có phản hồi trong thời gian quy định, cuộc trò chuyện đã được mở khóa cho staff khác.',
          confirmButtonText: 'Hiểu rồi',
          timer: 5000,
          timerProgressBar: true
        });

        // Chuyển về trang chính
        router.push('/admin/chat');
        currentChat.value = null;
      }
    } else if (reason === 'offline') {
      // Thông báo khi staff offline (luôn hiện)
      if (currentChat.value && currentChat.value.id === chatId) {
        await Swal.fire({
          icon: 'info',
          title: 'Phiên làm việc kết thúc',
          text: 'Cuộc trò chuyện đã được mở khóa do mất kết nối.',
          confirmButtonText: 'OK',
          timer: 3000,
          timerProgressBar: true
        });

        router.push('/admin/chat');
        currentChat.value = null;
      }
    }

    console.log(`🔓 Đã unlock chat ${chatId} - Lý do: ${reason} - Thông báo: ${showNotification}`);

  } catch (error) {
    console.error(`❌ Lỗi khi unlock chat ${chatId}:`, error);
  }
};

/**
 * Hiển thị cảnh báo không hoạt động (CẢI TIẾN)
 * @param {string} chatId - ID của cuộc trò chuyện
 */
 const showInactivityWarning = async (chatId) => {
  if (inactivityWarnings.value.has(chatId)) {
    return; // Đã hiển thị warning rồi
  }

  // Chỉ hiển thị warning nếu đang ở trong chat đó
  if (!currentChat.value || currentChat.value.id !== chatId) {
    return;
  }

  // Kiểm tra xem staff có phản hồi gần đây không
  try {
    const messagesRef = dbRef(rtdb, `messages/${chatId}`);
    const messagesSnapshot = await get(messagesRef);
    const messagesData = messagesSnapshot.val();

    if (messagesData) {
      const messages = Object.values(messagesData);
      const lastStaffMessage = messages
        .filter(msg => msg.loaiNguoiGui === 'staff')
        .sort((a, b) => b.thoiGian - a.thoiGian)[0];

      const lastCustomerMessage = messages
        .filter(msg => msg.loaiNguoiGui === 'customer')
        .sort((a, b) => b.thoiGian - a.thoiGian)[0];

      // Nếu staff đã phản hồi sau tin nhắn cuối của customer, không hiện warning
      if (lastStaffMessage && lastCustomerMessage && 
          lastStaffMessage.thoiGian > lastCustomerMessage.thoiGian) {
        console.log(`📝 Staff đã phản hồi, không hiện warning cho chat ${chatId}`);
        return;
      }
    }
  } catch (error) {
    console.error('Lỗi khi kiểm tra tin nhắn cho warning:', error);
  }

  inactivityWarnings.value.set(chatId, true);

  Swal.fire({
    icon: 'warning',
    title: 'Cảnh báo không hoạt động',
    text: 'Bạn sẽ mất quyền xử lý cuộc trò chuyện này trong 30 giây nếu không phản hồi.',
    confirmButtonText: 'Tôi sẽ phản hồi',
    showCancelButton: true,
    cancelButtonText: 'Mở khóa ngay',
    timer: 15000,
    timerProgressBar: true,
    allowOutsideClick: false,
    allowEscapeKey: false
  }).then((result) => {
    if (result.dismiss === Swal.DismissReason.cancel) {
      // Staff chọn mở khóa ngay
      unlockChat(chatId, 'manual', false); // Không hiện thông báo khi unlock thủ công
    } else if (result.isConfirmed) {
      // Reset timeout để tiếp tục
      resetAutoUnlock(chatId);
    }
    // Nếu timeout hoặc dismiss khác, để auto unlock tự chạy nhưng không hiện thông báo
  });
};

// ==================== AUTH FUNCTIONS ====================
const checkAuth = async () => {
  // Tránh gọi lại nếu đang trong quá trình xác thực
  if (isAuthenticating.value) {
    console.log('🔐 Đang trong quá trình xác thực, bỏ qua...');
    return false;
  }

  // Tránh gọi lại quá nhanh
  const now = Date.now();
  if (now - lastAuthCheck.value < AUTH_CHECK_INTERVAL) {
    console.log('🔐 Xác thực quá nhanh, bỏ qua...');
    return staffId.value ? true : false;
  }

  lastAuthCheck.value = now;
  isAuthenticating.value = true;

  console.log('🔐 Bắt đầu xác thực...');

  if (!accessToken.value) {
    isAuthenticating.value = false;
    router.push('/LoginStaff?redirect=/admin/chat');
    return false;
  }

  try {
    const response = await fetch(`${getUrlAPI.value}/api/Chat/GetStaffInfo`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${accessToken.value}`
      }
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Phiên đăng nhập đã hết hạn');
      } else if (response.status === 403) {
        throw new Error('Bạn không có quyền truy cập');
      } else {
        throw new Error(`Không thể xác thực: ${response.status}`);
      }
    }

    const result = await response.json();
    if (!result.success) {
      throw new Error(result.message || 'Không thể xác thực');
    }

    staffId.value = result.data.id;
    staffName.value = result.data.hoTen;
    staffAvatar.value = result.data.hinh || '/default-avatar.png';
    staffRole.value = result.data.vaiTro || 'Staff';

    isAuthenticating.value = false;
    return true;

  } catch (error) {
    console.error('❌ Lỗi xác thực:', error);
    isAuthenticating.value = false;

    // Chỉ hiện thông báo lỗi một lần
    if (!sessionStorage.getItem('authErrorShown')) {
      sessionStorage.setItem('authErrorShown', 'true');

      await Swal.fire({
        icon: 'error',
        title: 'Lỗi xác thực',
        text: error.message || 'Vui lòng đăng nhập lại để tiếp tục.',
        confirmButtonText: 'OK'
      });

      Cookies.remove('accessToken');
      Cookies.remove('refreshToken');
      router.push('/LoginStaff?redirect=/admin/chat');
    }

    return false;
  }
};

// ==================== CHAT FUNCTIONS ====================
const cleanupActiveStatus = async () => {
  try {
    const conversationsRef = dbRef(rtdb, 'conversations');
    const snapshot = await get(conversationsRef);
    if (!snapshot.exists()) {
      return;
    }
    const conversations = snapshot.val();
    const updates = {};
    let count = 0;

    Object.keys(conversations).forEach(chatId => {
      if (conversations[chatId].trangThai === 'active') {
        updates[`${chatId}/trangThai`] = null;
        count++;
      }
    });

    if (count > 0) {
      await update(conversationsRef, updates);
    }
  } catch (error) {
    console.error('❌ Lỗi khi xóa trạng thái active:', error);
  }
};

const loadActiveChats = () => {
  if (isLoadingChats.value) {
    console.log('📝 Đang load chats, bỏ qua...');
    return;
  }

  isLoadingChats.value = true;

  try {
    const conversationsRef = dbRef(rtdb, 'conversations');

    if (chatsListener.value) {
      off(conversationsRef, 'value', chatsListener.value);
      chatsListener.value = null;
    }

    chatsListener.value = onValue(conversationsRef, (snapshot) => {
      const data = snapshot.val();
      if (!data) {
        activeChats.value = [];
        isLoading.value = false;
        isLoadingChats.value = false;
        return;
      }

      statusListeners.value.forEach((listener, customerId) => {
        if (listener) {
          off(dbRef(rtdb, `userStatus/${customerId}`), 'value', listener);
        }
      });
      statusListeners.value.clear();

      const chats = Object.entries(data).map(([id, chat]) => ({
        id,
        customerName: chat.tenKH,
        customerAvatar: chat.anhDaiDienKH,
        customerId: chat.maKH,
        lastMessage: chat.tinNhanCuoi,
        lastMessageTime: chat.thoiGianTinNhanCuoi,
        unreadStaff: chat.soTinNhanChuaDocStaff || 0,
        unreadCustomer: chat.soTinNhanChuaDoc || 0,
        isActive: false,
        updatedAt: chat.ngayCapNhat || Date.now(),
        createdAt: chat.ngayTao || Date.now(),
        staffId: chat.maNV,
        staffName: chat.tenNV,
        lockedByStaffId: chat.lockedByStaffId,
        lockedByStaffName: chat.lockedByStaffName
      })).sort((a, b) => b.updatedAt - a.updatedAt);

      activeChats.value = chats;

      // Lấy danh sách customerIds để tải lịch sử đơn hàng
      const customerIds = chats.map(chat => chat.customerId).filter(id => id);
      if (customerIds.length > 0) {
        loadOrderHistories(customerIds);
      }

      chats.forEach(chat => {
        if (chat.customerId && !statusListeners.value.has(chat.customerId)) {
          const customerStatusRef = dbRef(rtdb, `userStatus/${chat.customerId}`);
          const listener = onValue(customerStatusRef, (statusSnapshot) => {
            const statusData = statusSnapshot.val();
            const index = activeChats.value.findIndex(c => c.id === chat.id);
            if (index !== -1) {
              activeChats.value[index].isActive = statusData ? statusData.isOnline : false;
            }
          });

          statusListeners.value.set(chat.customerId, listener);
        }
      });

      unreadCountTotal.value = chats.reduce((total, chat) => {
        return total + (chat.unreadStaff || 0);
      }, 0);

      isLoading.value = false;
      isLoadingChats.value = false;

      const chatId = route.params.id;
      if (chatId && !currentChat.value) {
        selectChat(chatId);
      }
    }, (error) => {
      console.error('Lỗi khi tải danh sách chat:', error);
      isLoading.value = false;
      isLoadingChats.value = false;

      if (!sessionStorage.getItem('chatLoadErrorShown')) {
        sessionStorage.setItem('chatLoadErrorShown', 'true');
        Swal.fire({
          icon: 'error',
          title: 'Lỗi tải dữ liệu',
          text: 'Không thể tải danh sách trò chuyện. Vui lòng thử lại sau.',
          confirmButtonText: 'OK'
        });
      }
    });
  } catch (error) {
    console.error('Lỗi khi thiết lập listener:', error);
    isLoading.value = false;
    isLoadingChats.value = false;
  }
};

const debouncedLoadActiveChats = debounce(loadActiveChats, 1000);

const loadCustomerInfo = async (customerId) => {
  try {
    const response = await fetchWithAuth(`${getUrlAPI.value}/api/Chat/GetCustomerById?id=${customerId}`);
    if (!response.ok) {
      throw new Error('Không thể tải thông tin khách hàng');
    }
    const result = await response.json();
    if (result.success) {
      customerInfo.value = result.data;
    } else {
      throw new Error(result.message || 'Không thể tải thông tin khách hàng');
    }
  } catch (error) {
    console.error('Lỗi khi tải thông tin khách hàng:', error);
    customerInfo.value = null;
  }
};

const selectChat = async (chatId) => {
  try {
    // Cleanup old message listener và auto unlock
    if (messagesListener.value && currentChat.value?.id) {
      off(dbRef(rtdb, `messages/${currentChat.value.id}`), 'value', messagesListener.value);
      messagesListener.value = null;
      // Clear auto unlock của chat cũ
      clearAutoUnlock(currentChat.value.id);
    }

    const chatRef = dbRef(rtdb, `conversations/${chatId}`);
    const snapshot = await get(chatRef);
    const chatData = snapshot.val();

    if (!chatData) {
      Swal.fire({
        icon: 'error',
        title: 'Không tìm thấy',
        text: 'Cuộc trò chuyện không tồn tại hoặc đã bị xóa.',
        confirmButtonText: 'OK'
      });
      router.push('/admin/chat');
      return;
    }

    const isLockedByOther = chatData.lockedByStaffId && chatData.lockedByStaffId !== staffId.value;
    if (isLockedByOther) {
      Swal.fire({
        icon: 'warning',
        title: 'Phiên bị khóa',
        text: `Cuộc trò chuyện đang được xử lý bởi ${chatData.lockedByStaffName || 'nhân viên khác'}. Bạn chỉ có thể xem lịch sử tin nhắn.`,
        confirmButtonText: 'OK'
      });
    }

    // Lock chat và thiết lập auto unlock
    if (!isLockedByOther && !chatData.lockedByStaffId) {
      await set(dbRef(rtdb, `conversations/${chatId}/lockedByStaffId`), staffId.value);
      await set(dbRef(rtdb, `conversations/${chatId}/lockedByStaffName`), staffName.value);
      await set(dbRef(rtdb, `conversations/${chatId}/lockedAt`), rtdbServerTimestamp());

      // Thiết lập auto unlock
      setupAutoUnlock(chatId);
    } else if (!isLockedByOther) {
      // Nếu chat đã được lock bởi staff hiện tại, thiết lập lại auto unlock
      setupAutoUnlock(chatId);
    }

    if (!chatData.maNV || chatData.maNV !== staffId.value) {
      await set(dbRef(rtdb, `conversations/${chatId}/maNV`), staffId.value);
      await set(dbRef(rtdb, `conversations/${chatId}/tenNV`), staffName.value);
    }

    currentChat.value = {
      id: chatId,
      customerName: chatData.tenKH,
      customerAvatar: chatData.anhDaiDienKH,
      customerId: chatData.maKH,
      lastMessage: chatData.tinNhanCuoi,
      lastMessageTime: chatData.thoiGianTinNhanCuoi,
      unreadStaff: chatData.soTinNhanChuaDocStaff || 0,
      unreadCustomer: chatData.soTinNhanChuaDoc || 0,
      isActive: false,
      staffId: chatData.maNV,
      staffName: chatData.tenNV,
      updatedAt: chatData.ngayCapNhat,
      createdAt: chatData.ngayTao,
      lockedByStaffId: chatData.lockedByStaffId,
      lockedByStaffName: chatData.lockedByStaffName,
      lockedAt: chatData.lockedAt,
      isReadOnly: isLockedByOther
    };

    router.push(`/admin/chat/${chatId}`);

    if (chatData.maKH) {
      const customerStatusRef = dbRef(rtdb, `userStatus/${chatData.maKH}`);
      onValue(customerStatusRef, (statusSnapshot) => {
        const statusData = statusSnapshot.val();
        if (statusData && currentChat.value) {
          currentChat.value.isActive = statusData.isOnline || false;
        }
      });
      loadCustomerInfo(chatData.maKH);
    }

    await set(dbRef(rtdb, `conversations/${chatId}/soTinNhanChuaDocStaff`), 0);

    const messagesRef = dbRef(rtdb, `messages/${chatId}`);
    messagesListener.value = onValue(messagesRef, (snapshot) => {
      const messagesData = snapshot.val();
      if (!messagesData) {
        messages.value = [];
        return;
      }

      const messagesList = Object.entries(messagesData).map(([id, message]) => ({
        id,
        senderId: message.nguoiGui,
        senderName: message.tenNguoiGui,
        senderRole: message.loaiNguoiGui,
        content: message.noiDung,
        anhUrls: message.anhUrls || (message.anhUrl ? [message.anhUrl] : null),
        timestamp: message.thoiGian,
        isRead: message.daDoc,
        status: message.trangThai,
        type: message.loai
      })).sort((a, b) => a.timestamp - b.timestamp);

      messages.value = messagesList;

      // Check for new customer messages để reset timeout
      const lastActivity = lastStaffActivity.value.get(chatId) || 0;
      const newCustomerMessages = messagesList.filter(msg =>
        msg.senderRole === 'customer' &&
        msg.timestamp > lastActivity
      );

      if (newCustomerMessages.length > 0 && !isLockedByOther) {
        // Có tin nhắn mới từ customer, reset auto unlock
        console.log(`📬 Có tin nhắn mới từ customer trong chat ${chatId}, reset auto unlock`);
        resetAutoUnlock(chatId);
      }

      Object.entries(messagesData).forEach(([id, message]) => {
        if (message.loaiNguoiGui === 'customer' && !message.daDoc) {
          set(dbRef(rtdb, `messages/${chatId}/${id}/daDoc`), true);
          set(dbRef(rtdb, `messages/${chatId}/${id}/thoiGianDoc`), rtdbServerTimestamp());
        }
      });

      scrollToBottom();
    });
  } catch (error) {
    console.error('Lỗi khi chọn chat:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể tải cuộc trò chuyện. Vui lòng thử lại sau.',
      confirmButtonText: 'OK'
    });
  }
};

// ==================== MESSAGE FUNCTIONS ====================
// 1. Điều chỉnh hàm sendMessage để lưu trữ avatar của nhân viên
const sendMessage = async () => {
  if (!currentChat.value) return;

  const chatRef = dbRef(rtdb, `conversations/${currentChat.value.id}`);
  const snapshot = await get(chatRef);
  const chatData = snapshot.val();
  if (chatData.lockedByStaffId && chatData.lockedByStaffId !== staffId.value) {
    Swal.fire({
      icon: 'warning',
      title: 'Phiên bị khóa',
      text: `Cuộc trò chuyện đang được xử lý bởi ${chatData.lockedByStaffName || 'nhân viên khác'}.`,
      confirmButtonText: 'OK'
    });
    return;
  }

  if (!newMessage.value.trim() && imageFiles.value.length === 0) {
    return;
  }

  try {
    // Reset auto unlock khi staff gửi tin nhắn
    console.log(`📤 Staff gửi tin nhắn trong chat ${currentChat.value.id}, reset auto unlock`);
    resetAutoUnlock(currentChat.value.id);

    // Clear warning nếu có vì staff đã phản hồi
    if (inactivityWarnings.value.has(currentChat.value.id)) {
      inactivityWarnings.value.delete(currentChat.value.id);
      console.log(`✅ Đã clear warning cho chat ${currentChat.value.id} vì staff đã phản hồi`);
    }

    let imageUrls = [];
    if (imageFiles.value.length > 0) {
      for (const file of imageFiles.value) {
        const storageReference = storageRef(storage, `chat-images/${Date.now()}_${file.name}`);
        const uploadResult = await uploadBytes(storageReference, file);
        const url = await getDownloadURL(uploadResult.ref);
        imageUrls.push(url);
      }
    }

    const isVideoOnly = imageFiles.value.length > 0 && imageFiles.value.every(file => file.type.startsWith('video/'));
    const messageData = {
      nguoiGui: staffId.value,
      tenNguoiGui: staffName.value,
      anhNguoiGui: staffAvatar.value,
      loaiNguoiGui: 'staff',
      noiDung: newMessage.value.trim(),
      anhUrls: imageUrls.length > 0 ? imageUrls : null,
      thoiGian: rtdbServerTimestamp(),
      daDoc: false,
      trangThai: 'sent',
      loai: isVideoOnly && !newMessage.value.trim() ? 'video' : (imageUrls.length > 0 ? 'image' : 'text')
    };

    const messagesRef = dbRef(rtdb, `messages/${currentChat.value.id}`);
    const newMessageRef = push(messagesRef);
    await set(newMessageRef, messageData);

    const conversationUpdate = {
      maKH: currentChat.value.customerId,
      tenKH: currentChat.value.customerName,
      anhDaiDienKH: currentChat.value.customerAvatar,
      maNV: staffId.value,
      tenNV: staffName.value,
      tinNhanCuoi: imageUrls.length > 0 ? (isVideoOnly ? '🎥 Video' : '📷 Hình ảnh') : newMessage.value.trim(),
      thoiGianTinNhanCuoi: rtdbServerTimestamp(),
      soTinNhanChuaDoc: (currentChat.value.unreadCustomer || 0) + 1,
      soTinNhanChuaDocStaff: 0,
      ngayCapNhat: rtdbServerTimestamp(),
      ngayTao: currentChat.value.createdAt || rtdbServerTimestamp()
    };

    await update(dbRef(rtdb, `conversations/${currentChat.value.id}`), conversationUpdate);

    newMessage.value = '';
    imageFiles.value = [];
    imagePreviews.value = [];
    showEmojiPicker.value = false;

  } catch (error) {
    console.error('Lỗi khi gửi tin nhắn:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: error.message || 'Không thể gửi tin nhắn. Vui lòng thử lại sau.',
      confirmButtonText: 'OK'
    });
  }
};


// ==================== TRANSFER FUNCTIONS ====================
const transferChat = async (newStaffId, newStaffName) => {
  if (!currentChat.value) return;

  try {
    const chatRef = dbRef(rtdb, `conversations/${currentChat.value.id}`);
    const snapshot = await get(chatRef);
    const chatData = snapshot.val();

    if (!chatData) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: 'Cuộc trò chuyện không tồn tại.',
        confirmButtonText: 'OK'
      });
      return;
    }

    clearAutoUnlock(currentChat.value.id);

    const updates = {
      maNV: newStaffId,
      tenNV: newStaffName,
      lockedByStaffId: newStaffId,
      lockedByStaffName: newStaffName,
      ngayCapNhat: rtdbServerTimestamp()
    };

    const transferHistoryRef = dbRef(rtdb, `conversations/${currentChat.value.id}/transferHistory`);
    const newTransferRef = push(transferHistoryRef);
    await set(newTransferRef, {
      fromStaffId: staffId.value,
      fromStaffName: staffName.value,
      toStaffId: newStaffId,
      toStaffName: newStaffName,
      timestamp: rtdbServerTimestamp()
    });

    await update(chatRef, updates);

    // Gửi thông báo chi tiết hơn
    const notificationRef = dbRef(rtdb, `notifications/${newStaffId}`);
    const newNotificationRef = push(notificationRef);
    await set(newNotificationRef, {
      message: `Cuộc trò chuyện với khách hàng ${currentChat.value.customerName} (ID: ${currentChat.value.customerId}) đã được ${staffName.value} chuyển giao lúc ${new Date().toLocaleString('vi-VN')}.`,
      timestamp: rtdbServerTimestamp(),
      chatId: currentChat.value.id,
      read: false
    });

    // Thông báo thành công cho nhân viên hiện tại
    Swal.fire({
      icon: 'success',
      title: 'Chuyển giao thành công',
      html: `Cuộc trò chuyện với <strong>${currentChat.value.customerName}</strong> đã được chuyển cho <strong>${newStaffName}</strong>.<br>Nhân viên nhận chuyển giao sẽ được thông báo.`,
      confirmButtonText: 'OK',
      timer: 3000,
      timerProgressBar: true
    });

    debouncedLoadActiveChats();
    currentChat.value = null;
    router.push('/admin/chat');
  } catch (error) {
    console.error('Lỗi khi chuyển giao chat:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể chuyển giao cuộc trò chuyện. Vui lòng thử lại sau.',
      confirmButtonText: 'OK'
    });
  }
};

const loadAvailableStaff = async () => {
  try {
    const response = await fetchWithAuth(`${getUrlAPI.value}/api/Chat/GetAllStaff`);
    const result = await response.json();
    if (result.success) {
      availableStaff.value = result.data.filter(staff => staff.id !== staffId.value);
    }
  } catch (error) {
    console.error('Lỗi khi tải danh sách nhân viên:', error);
  }
};

const showTransferModal = () => {
  isTransferModalVisible.value = true;
  loadAvailableStaff();
};

const confirmTransfer = () => {
  if (selectedStaff.value) {
    transferChat(selectedStaff.value.id, selectedStaff.value.hoTen);
    isTransferModalVisible.value = false;
    selectedStaff.value = null;
  }
};

// ==================== FILE HANDLING ====================
const handleFileUpload = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  const validFiles = files.filter(file => {
    if (file.size > 20 * 1024 * 1024) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: `File ${file.name} quá lớn. Vui lòng chọn file nhỏ hơn 20MB.`,
        confirmButtonText: 'OK'
      });
      return false;
    }

    const validTypes = ['image/', 'video/mp4', 'video/webm', 'video/ogg'];
    if (!validTypes.some(type => file.type.startsWith(type))) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: `File ${file.name} không phải hình ảnh hoặc video hợp lệ.`,
        confirmButtonText: 'OK'
      });
      return false;
    }

    return true;
  });

  imageFiles.value = [...imageFiles.value, ...validFiles];

  const newPreviews = validFiles.map(file => {
    return new Promise((resolve) => {
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => resolve({ type: 'image', url: e.target.result });
        reader.readAsDataURL(file);
      } else if (file.type.startsWith('video/')) {
        resolve({ type: 'video', url: URL.createObjectURL(file) });
      }
    });
  });

  Promise.all(newPreviews).then(previews => {
    imagePreviews.value = [...imagePreviews.value, ...previews];
  });
};

const removeImage = (index) => {
  imageFiles.value.splice(index, 1);
  imagePreviews.value.splice(index, 1);
};

const addEmoji = (emoji) => {
  newMessage.value += emoji;
  showEmojiPicker.value = false;
};

const handleTyping = debounce(() => {
  if (!currentChat.value) return;

  // Reset auto unlock khi staff đang typing
  console.log(`⌨️ Staff đang typing trong chat ${currentChat.value.id}, reset auto unlock`);
  resetAutoUnlock(currentChat.value.id);

  clearTimeout(typingTimeout.value);

  const typingRef = dbRef(rtdb, `userStatus/${staffId.value}/isTyping`);
  set(typingRef, true);

  typingTimeout.value = setTimeout(() => {
    set(typingRef, false);
  }, 2000);
}, 300);

// ==================== STATUS FUNCTIONS ====================
const updateUserStatus = async (isOnlineStatus) => {
  if (!staffId.value) return;

  if (!navigator.onLine && isOnlineStatus) {
    isOnlineStatus = false;
  }

  try {
    const userStatusRef = dbRef(rtdb, `userStatus/${staffId.value}`);
    await set(userStatusRef, {
      isOnline: isOnlineStatus,
      lastSeen: rtdbServerTimestamp(),
      name: staffName.value,
      avatar: staffAvatar.value,
      type: 'staff'
    });
  } catch (error) {
    console.error('Lỗi khi cập nhật trạng thái:', error);
  }
};

const handleConnectionChange = () => {
  const now = Date.now();

  // Tránh xử lý quá nhanh
  if (now - lastConnectionChange.value < 2000) {
    return;
  }

  lastConnectionChange.value = now;
  const prevOnline = isOnline.value;
  isOnline.value = navigator.onLine;

  if (prevOnline !== isOnline.value) {
    connectionChangeCount.value++;
    updateUserStatus(isOnline.value);

    // Chỉ hiện thông báo nếu thay đổi thực sự và không quá thường xuyên
    if (connectionChangeCount.value <= 3) {
      if (!isOnline.value) {
        Swal.fire({
          icon: 'warning',
          title: 'Mất kết nối',
          text: 'Bạn đang ở chế độ offline. Một số tính năng sẽ không hoạt động.',
          confirmButtonText: 'OK',
          timer: 3000,
          timerProgressBar: true
        });
      } else {
        // Thông báo kết nối lại ngắn gọn hơn
        if (connectionChangeCount.value > 1) {
          Swal.fire({
            icon: 'success',
            title: 'Đã kết nối lại',
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 2000
          });
        }

        // Reload dữ liệu khi có kết nối lại
        if (staffId.value && !isLoadingChats.value) {
          setTimeout(() => {
            debouncedLoadActiveChats();
          }, 1000);
        }
      }
    }
  }
};

const openMedia = (url) => {
  if (typeof window !== 'undefined' && window.open) {
    window.open(url, '_blank');
  } else {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể mở hình ảnh/video.',
      confirmButtonText: 'OK'
    });
  }
};

// ==================== WATCHERS ====================
watch(() => route.params.id, (newChatId, oldChatId) => {
  if (newChatId && newChatId !== oldChatId && (!currentChat.value || currentChat.value.id !== newChatId)) {
    selectChat(newChatId);
  }
});

// Watch for auth token changes
watch(() => accessToken.value, (newToken) => {
  if (!newToken) {
    // Clear all data when token is removed
    staffId.value = null;
    staffName.value = '';
    staffAvatar.value = '';
    staffRole.value = '';
    activeChats.value = [];
    currentChat.value = null;
    messages.value = [];
    customerInfo.value = null;

    // Clear auto unlock timeouts
    autoUnlockTimeouts.value.forEach((timeoutId, key) => {
      clearTimeout(timeoutId);
    });
    autoUnlockTimeouts.value.clear();
    lastStaffActivity.value.clear();
    inactivityWarnings.value.clear();
  }
});

// ==================== LIFECYCLE HOOKS ====================
onMounted(async () => {
  console.log('🚀 Component mounted');

  // Clear các flag error cũ
  sessionStorage.removeItem('authErrorShown');
  sessionStorage.removeItem('chatLoadErrorShown');

  // Add event listeners
  window.addEventListener('online', handleConnectionChange);
  window.addEventListener('offline', handleConnectionChange);

  // Kiểm tra xác thực một lần
  const isAuthenticated = await checkAuth();
  if (isAuthenticated) {
    console.log('✅ Authentication successful');

    try {
      await cleanupActiveStatus();

      // Load chats một lần
      if (!isLoadingChats.value) {
        loadActiveChats();
      }

      updateUserStatus(navigator.onLine);
      loadAvailableStaff();

    } catch (error) {
      console.error('❌ Error during initialization:', error);
    }
  } else {
    console.log('❌ Authentication failed');
  }

  // Scroll to bottom after mount
  scrollToBottom();
});

onUnmounted(() => {
  console.log('🛑 Component unmounting');

  // Clear all auto unlock timeouts
  autoUnlockTimeouts.value.forEach((timeoutId, key) => {
    clearTimeout(timeoutId);
    console.log(`🚫 Cleared timeout for ${key}`);
  });
  autoUnlockTimeouts.value.clear();
  lastStaffActivity.value.clear();
  inactivityWarnings.value.clear();

  // Remove event listeners
  window.removeEventListener('online', handleConnectionChange);
  window.removeEventListener('offline', handleConnectionChange);

  // Cleanup Firebase listeners
  if (messagesListener.value && currentChat.value) {
    off(dbRef(rtdb, `messages/${currentChat.value.id}`), 'value', messagesListener.value);
    messagesListener.value = null;
  }

  if (chatsListener.value) {
    off(dbRef(rtdb, 'conversations'), 'value', chatsListener.value);
    chatsListener.value = null;
  }

  // Cleanup status listeners
  statusListeners.value.forEach((listener, customerId) => {
    if (listener) {
      off(dbRef(rtdb, `userStatus/${customerId}`), 'value', listener);
    }
  });
  statusListeners.value.clear();

  // Clear timeouts
  if (typingTimeout.value) {
    clearTimeout(typingTimeout.value);
    typingTimeout.value = null;
  }

  // Update status to offline and unlock current chat if any
  updateUserStatus(false);
  if (currentChat.value && !currentChat.value.isReadOnly) {
    unlockChat(currentChat.value.id, 'logout');
  }

  // Clear session flags
  sessionStorage.removeItem('authErrorShown');
  sessionStorage.removeItem('chatLoadErrorShown');

  console.log('✅ Cleanup completed');
});

// ==================== ERROR HANDLING ====================
// Global error handler for unhandled promises
window.addEventListener('unhandledrejection', (event) => {
  console.error('Unhandled promise rejection:', event.reason);

  // Prevent default browser behavior
  event.preventDefault();

  // Show user-friendly error message
  if (!sessionStorage.getItem('globalErrorShown')) {
    sessionStorage.setItem('globalErrorShown', 'true');

    Swal.fire({
      icon: 'error',
      title: 'Có lỗi xảy ra',
      text: 'Đã có lỗi không mong muốn. Vui lòng tải lại trang.',
      confirmButtonText: 'Tải lại',
      allowOutsideClick: false
    }).then((result) => {
      if (result.isConfirmed) {
        window.location.reload();
      }
    });

    // Clear flag after 10 seconds
    setTimeout(() => {
      sessionStorage.removeItem('globalErrorShown');
    }, 10000);
  }
});

// ==================== DEVELOPMENT HELPERS ====================
// Development mode logging
if (import.meta.env.DEV) {
  // Add development helpers
  window.chatDebug = {
    staffId,
    activeChats,
    currentChat,
    messages,
    isLoading,
    isLoadingChats,
    isAuthenticating,
    autoUnlockTimeouts,
    lastStaffActivity,
    inactivityWarnings,
    checkAuth,
    loadActiveChats: debouncedLoadActiveChats,
    setupAutoUnlock,
    clearAutoUnlock,
    resetAutoUnlock,
    unlockChat,
    clearErrors: () => {
      sessionStorage.removeItem('authErrorShown');
      sessionStorage.removeItem('chatLoadErrorShown');
      sessionStorage.removeItem('globalErrorShown');
    },
    // Helper để test auto unlock
    forceAutoUnlock: (chatId) => {
      if (chatId) {
        handleAutoUnlock(chatId);
      } else if (currentChat.value) {
        handleAutoUnlock(currentChat.value.id);
      }
    },
    // Helper để test warning
    showWarning: (chatId) => {
      if (chatId) {
        showInactivityWarning(chatId);
      } else if (currentChat.value) {
        showInactivityWarning(currentChat.value.id);
      }
    },
    // Helper để xem trạng thái timeouts
    getTimeouts: () => {
      const result = {};
      autoUnlockTimeouts.value.forEach((timeoutId, key) => {
        result[key] = timeoutId;
      });
      return result;
    },
    // Helper để xem activity
    getActivities: () => {
      const result = {};
      lastStaffActivity.value.forEach((timestamp, chatId) => {
        result[chatId] = new Date(timestamp).toLocaleString();
      });
      return result;
    }
  };

  console.log('🔧 Development mode - window.chatDebug available');
  console.log('🔧 Available debug commands:');
  console.log('  - window.chatDebug.forceAutoUnlock() - Force unlock current chat');
  console.log('  - window.chatDebug.showWarning() - Show inactivity warning');
  console.log('  - window.chatDebug.clearErrors() - Clear all error flags');
  console.log('  - window.chatDebug.getTimeouts() - View active timeouts');
  console.log('  - window.chatDebug.getActivities() - View last activities');
}
</script>

<template>
  <div class="chat-container" style="margin-top: 70px;">
    <ConnectionStatus />

    <!-- Header tổng đài -->
    <div class="chat-header-main">
      <div class="user-status">
        <span :class="{ 'online': isOnline, 'offline': !isOnline }">
          {{ isOnline ? 'Đang kết nối' : 'Mất kết nối' }}
        </span>
      </div>
      <!-- Nút toggle sidebar cho màn hình nhỏ -->
      <button class="btn btn-toggle-sidebar d-md-none" @click="toggleSidebar">
        <i class="bi bi-list"></i>
      </button>
    </div>

    <div v-if="isLoading" class="loading-overlay">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
    </div>

    <div v-else class="chat-body">
      <!-- Danh sách chat -->
      <div class="chat-sidebar" :class="{ 'show': showSidebar }">
        <div class="chat-list-header">
          <div class="d-flex justify-content-between align-items-center">
            <h5 class="mb-0">Cuộc trò chuyện</h5>
            <span class="badge bg-accent rounded-pill ms-2" v-if="unreadCountTotal > 0">
              {{ unreadCountTotal }}
            </span>
          </div>
        </div>

        <div class="chat-search">
          <div class="input-group">
            <span class="input-group-text">
              <i class="bi bi-search"></i>
            </span>
            <input type="text" class="form-control" placeholder="Tìm kiếm..." v-model="searchTerm">
          </div>
        </div>

        <div class="chat-filters">
          <div class="btn-group w-100">
            <button type="button" class="btn btn-sm"
              :class="chatFilter === 'all' ? 'btn-primary' : 'btn-outline-primary'" @click="chatFilter = 'all'">
              Tất cả
            </button>
            <button type="button" class="btn btn-sm"
              :class="chatFilter === 'active' ? 'btn-primary' : 'btn-outline-primary'" @click="chatFilter = 'active'">
              Đang hoạt động
            </button>
            <button type="button" class="btn btn-sm"
              :class="chatFilter === 'unread' ? 'btn-primary' : 'btn-outline-primary'" @click="chatFilter = 'unread'">
              Chưa đọc
            </button>
          </div>
        </div>

        <div class="chat-list">
          <div v-for="chat in filteredChats" :key="chat.id" class="chat-item"
            :class="{ 'active': currentChat && chat.id === currentChat.id }"
            @click="selectChat(chat.id); showSidebar = false">
            <div class="d-flex align-items-center">
              <!-- <div class="position-relative">
                <img :src="getImageUrl(chat.customerAvatar) || '/default-avatar.png'" alt="Avatar" class="chat-avatar">
                <span v-if="chat.isActive" class="online-indicator" title="Khách hàng đang online"></span>
              </div> -->
              <div class="ms-3 flex-grow-1">
                <div class="d-flex justify-content-between align-items-center">
                  <h6 class="mb-0 text-truncate">
                    {{ chat.customerName }}
                    <span v-if="chat.isActive" class="badge bg-accent ms-1">
                      Online
                    </span>
                    <span v-if="chat.lockedByStaffId && chat.lockedByStaffId !== staffId.value"
                      class="badge bg-warning ms-1">
                      {{ chat.lockedByStaffName }}
                    </span>
                  </h6>
                  <small class="text-muted">{{ formatTime(chat.lastMessageTime) }}</small>
                </div>
                <p class="text-muted mb-0 text-truncate">
                  <span v-if="chat.lastMessage && chat.lastMessage.startsWith('📷')">
                    <i class="bi bi-image me-1"></i>
                  </span>
                  <span v-else-if="chat.lastMessage && chat.lastMessage.startsWith('🎥')">
                    <i class="bi bi-film me-1"></i>
                  </span>
                  {{ chat.lastMessage }}
                </p>
                <span v-if="chat.unreadStaff > 0" class="badge bg-primary rounded-pill float-end">
                  {{ chat.unreadStaff }}
                </span>
              </div>
            </div>
          </div>

          <div v-if="filteredChats.length === 0" class="p-3 text-center">
            <p class="text-muted">Chưa có cuộc trò chuyện</p>
            <button class="btn btn-primary btn-sm mt-2" @click="loadActiveChats">
              <i class="bi bi-arrow-clockwise me-1"></i> Tải lại
            </button>
          </div>
        </div>
      </div>

     <!-- Thêm vào cuối <template>, trước thẻ đóng </div> của chat-container -->
<!-- Modal chi tiết hóa đơn -->
<div v-if="showOrderDetailModal" class="modal fade show d-block" tabindex="-1">
  <div class="modal-dialog modal-lg">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Chi tiết đơn hàng #{{ selectedOrder?.orderId }}</h5>
        <button type="button" class="btn-close" @click="showOrderDetailModal = false"></button>
      </div>
      <div class="modal-body">
        <div v-if="selectedOrder">
          <div class="mb-3">
            <p><strong>Ngày đặt hàng: </strong>{{ formatTime(selectedOrder.orderDate) }}</p>
            <p><strong>Trạng thái: </strong>{{ selectedOrder.status }}</p>
            <p><strong>Tổng tiền: </strong>{{ formatCurrency(selectedOrder.totalAmount) }}</p>
          </div>
          <h6>Danh sách sản phẩm</h6>
          <table class="table table-bordered">
            <thead>
              <tr>
                <th>Tên sản phẩm</th>
                <th>Số lượng</th>
                <th>Giá</th>
                <th>Tổng</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in selectedOrder.items" :key="index">
                <td>{{ item.productName }}</td>
                <td>{{ item.quantity }}</td>
                <td>{{ formatCurrency(item.price) }}</td>
                <td>{{ formatCurrency(item.quantity * item.price) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-else class="text-muted">Không có thông tin chi tiết đơn hàng.</p>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" @click="showOrderDetailModal = false">Đóng</button>
      </div>
    </div>
  </div>
</div>

<!-- Sửa lại phần chat-main để tích hợp order-history -->
<div class="chat-main">
  <div v-if="currentChat" class="chat-header">
    <div class="d-flex align-items-center justify-content-between">
      <div class="d-flex align-items-center">
        <img :src="getImageUrl(currentChat.customerAvatar) || '/default-avatar.png'" alt="Avatar" class="chat-avatar">
        <div class="ms-3">
          <h6 class="mb-0">{{ currentChat.customerName }}</h6>
          <small class="text-muted">{{ currentChat.isActive ? 'Đang online' : 'Ngoại tuyến' }}</small>
        </div>
      </div>
      <button v-if="!currentChat.isReadOnly" class="btn btn-sm btn-outline-primary" @click="showTransferModal">Chuyển giao</button>
    </div>
  </div>

  <div v-if="currentChat" class="chat-content d-flex">
    <!-- Khung chat -->
    <div class="chat-messages flex-grow-1 position-relative">
      <div v-for="message in sortedMessages" :key="message.id" class="message mb-3" :class="{ 'message-sent': message.senderRole !== 'customer', 'message-received': message.senderRole === 'customer' }">
        <div class="message-wrapper">
          <img v-if="message.senderRole === 'customer'" :src="getImageUrl(currentChat.customerAvatar) || '/default-avatar.png'" alt="Customer Avatar" class="message-avatar">
          <div class="message-content">
            <div class="message-sender" v-if="message.senderName">
              {{ message.senderName }}
              <span v-if="message.senderRole !== 'customer'" class="staff-label">(Nhân viên)</span>
            </div>
            <div v-if="message.anhUrls && message.anhUrls.length > 0" class="message-image mb-2">
              <div class="image-gallery">
                <template v-for="(url, imgIndex) in message.anhUrls" :key="imgIndex">
                  <img v-if="url && url.match(/\.(jpg|jpeg|png|gif|webp)$/i)" :src="url" alt="Hình ảnh" @click="() => openMedia(url)">
                  <video v-else-if="url && url.match(/\.(mp4|webm|ogg)$/i)" :src="url" controls class="message-video"></video>
                </template>
              </div>
            </div>
            <div v-if="message.content" class="message-text">{{ message.content }}</div>
            <div class="message-time">
              {{ formatTime(message.timestamp) }}
              <span v-if="message.senderRole !== 'customer'" class="ms-1">
                <i class="bi" :class="{ 'bi-check': message.status === 'sent', 'bi-check-all': message.status === 'delivered', 'bi-check-all text-accent': message.isRead }"></i>
              </span>
            </div>
          </div>
        </div>
      </div>
      <div v-if="messages.length === 0" class="text-center my-5">
        <p class="text-muted">Bắt đầu trò chuyện ngay!</p>
      </div>
      <button class="btn btn-primary btn-sm scroll-to-bottom" @click="scrollToBottom"><i class="bi bi-arrow-down"></i></button>
    </div>

    <!-- Lịch sử đơn hàng -->
    <!-- <div class="order-history p-3" style="width: 300px; border-left: 1px solid var(--secondary);">
      <h6>Lịch sử đơn hàng</h6>
      <div v-if="orderHistories[currentChat.customerId]?.length">
        <div v-for="order in orderHistories[currentChat.customerId]" :key="order.orderId" class="mb-3">
          <p><strong>Mã đơn: </strong>{{ order.orderId }}</p>
          <p><strong>Ngày đặt: </strong>{{ formatTime(order.orderDate) }}</p>
          <p><strong>Trạng thái: </strong>{{ order.status }}</p>
          <p><strong>Tổng tiền: </strong>{{ order.totalAmount.toLocaleString('vi-VN') }} VND</p>
          <button class="btn btn-sm btn-outline-primary" @click="viewOrderDetails(order.orderId)">Xem chi tiết</button>
        </div>
      </div>
      <p v-else class="text-muted">Chưa có đơn hàng</p>
    </div> -->
  </div>

  <div v-if="currentChat && !currentChat.isReadOnly" class="chat-input">
    <div v-if="imagePreviews.length > 0" class="image-preview mb-2">
      <div class="image-gallery">
        <div v-for="(preview, index) in imagePreviews" :key="index" class="position-relative d-inline-block">
          <img v-if="preview.type === 'image'" :src="preview.url" alt="Preview" class="preview-image">
          <video v-else-if="preview.type === 'video'" :src="preview.url" controls class="preview-image"></video>
          <button type="button" class="btn-close position-absolute top-0 end-0" @click="removeImage(index)"></button>
        </div>
      </div>
    </div>

    <div class="input-group">
      <button class="btn btn-outline-light" @click="() => showEmojiPicker = !showEmojiPicker">
        <i class="bi bi-emoji-smile"></i>
      </button>
      <label class="btn btn-outline-light" for="file-upload">
        <i class="bi bi-film"></i>
        <input type="file" id="file-upload" class="d-none" accept="image/*,video/*" multiple @change="handleFileUpload">
      </label>
      <input type="text" class="form-control" placeholder="Nhập tin nhắn..." v-model="newMessage" @input="handleTyping" @keypress.enter="sendMessage">
      <button class="btn btn-primary" @click="sendMessage" :disabled="!isOnline">
        <i class="bi bi-send"></i> Gửi
      </button>
    </div>

    <div v-if="showEmojiPicker" class="emoji-picker">
      <div class="emoji-container">
        <span v-for="emoji in emojiList" :key="emoji" class="emoji" @click="addEmoji(emoji)">{{ emoji }}</span>
      </div>
    </div>
  </div>

  <div v-if="currentChat && currentChat.isReadOnly" class="p-3 text-center">
    <p class="text-muted">Chế độ chỉ xem: Cuộc trò chuyện đang được xử lý bởi {{ currentChat.lockedByStaffName }}</p>
  </div>
  <div v-if="!currentChat" class="d-flex flex-column justify-content-center align-items-center h-100">
    <i class="bi bi-chat-dots fs-1 text-muted"></i>
    <h5 class="mt-3">Chọn một cuộc trò chuyện để bắt đầu</h5>
  </div>
</div>
    </div>

    <!-- Modal chuyển giao -->
    <div v-if="isTransferModalVisible" class="modal fade show d-block" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Chuyển giao cuộc trò chuyện</h5>
            <button type="button" class="btn-close" @click="isTransferModalVisible = false"></button>
          </div>
          <div class="modal-body">
            <label for="staffSelect" class="form-label">Chọn nhân viên</label>
            <select id="staffSelect" class="form-select" v-model="selectedStaff">
              <option v-for="staff in availableStaff" :key="staff.id" :value="staff">
                {{ staff.hoTen }}
              </option>
            </select>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="isTransferModalVisible = false">Hủy</button>
            <button type="button" class="btn btn-primary" @click="confirmTransfer">Chuyển giao</button>
          </div>
        </div>
      </div>
    </div>
    
  </div>
</template>

<style scoped>
/* Import Google Fonts */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600&family=Poppins:wght@500;700&display=swap');

/* Định nghĩa màu sắc */
:root {
  --primary: #5CCAE7;
  --secondary: #ABA2B7;
  --accent: #EC4E79;
  --light-bg: #f6f7f9;
  --text-dark: #2d2d2d;
  --text-muted: #61AFFE;
  --accent-light: #ffe6ec;
}

.chat-container {
  height: calc(100vh - 50px);
  background-color: var(--light-bg);
  border-radius: 15px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  font-family: 'Inter', sans-serif;
  margin: 20px;
  display: flex;
  flex-direction: column;
}

.chat-header-main {
  background: linear-gradient(90deg, var(--primary), var(--secondary), var(--accent), var(--secondary));
  color: white;
  padding: 10px 15px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  flex-shrink: 0;
}

.user-status {
  font-size: 0.75rem;
}

.user-status .online {
  color: #28a745;
}

.user-status .offline {
  color: #dc3545;
}

.btn-toggle-sidebar {
  background: transparent;
  border: none;
  color: white;
  font-size: 1.2rem;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.85);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.chat-body {
  flex-grow: 1;
  display: flex;
  overflow: hidden;
}

.chat-sidebar {
  background-color: white;
  border-right: 1px solid var(--secondary);
  flex: 0 0 30%;
  max-width: 350px;
  min-width: 250px;
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: transform 0.3s ease;
}

.chat-sidebar.show {
  transform: translateX(0);
}

.chat-list-header {
  background-color: var(--primary);
  color: white;
  font-family: 'Poppins', sans-serif;
  padding: 12px;
  flex-shrink: 0;
}

.chat-search {
  padding: 12px;
}

.chat-search .input-group {
  background: none;
  box-shadow: none;
  border: 1px solid var(--secondary);
  border-radius: 25px;
}

.chat-search .input-group-text {
  background: transparent;
  border: none;
}

.chat-search .form-control {
  border: none;
  box-shadow: none;
}

.chat-filters {
  padding: 12px;
}

.chat-list {
  flex-grow: 1;
  overflow-y: auto;
  padding-bottom: 10px;
}

.chat-item {
  cursor: pointer;
  transition: all 0.3s ease;
  border-bottom: 1px solid rgba(171, 162, 183, 0.3);
  padding: 12px;
}

.chat-item:hover {
  background-color: rgba(92, 202, 231, 0.1);
}

.chat-item.active {
  background-color: rgba(92, 202, 231, 0.2);
  border-left: 3px solid var(--primary);
}

.chat-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  border: 1px solid var(--secondary);
}

.online-indicator {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 10px;
  height: 10px;
  background-color: var(--accent);
  border-radius: 50%;
  border: 1px solid white;
}

.chat-main {
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  height: 100%;
  position: relative;
}

.chat-header {
  background-color: var(--light-bg);
  border-bottom: 1px solid rgba(171, 162, 183, 0.3);
  padding: 12px;
  flex-shrink: 0;
  z-index: 10;
}

.chat-content {
  display: flex;
  flex-grow: 1;
  overflow: hidden;
}

.chat-messages {
  flex-grow: 1;
  overflow-y: auto;
  background-color: var(--light-bg);
  padding: 20px;
  scroll-behavior: smooth;
  min-height: 0; /* Đảm bảo không bị tràn */
}

.message {
  display: flex;
  margin-bottom: 12px;
}

.message-sent {
  justify-content: flex-end;
}

.message-received {
  justify-content: flex-start;
}

.message-wrapper {
  display: flex;
  align-items: flex-start;
  max-width: 80%;
}

.message-sent .message-wrapper {
  flex-direction: row-reverse;
}

.message-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  margin-right: 8px;
  flex-shrink: 0;
}

.message-sent .message-avatar {
  margin-right: 0;
  margin-left: 8px;
}

.message-content {
  padding: 10px 14px;
  border-radius: 15px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  position: relative;
  max-width: 100%;
}

.message-sent .message-content {
  background-color: #dcf8c6;
  border-top-right-radius: 4px;
}

.message-received .message-content {
  background-color: var(--secondary);
  color: var(--text-dark);
  border-top-left-radius: 4px;
}

.message-sender {
  font-size: 0.75rem;
  font-weight: 600;
  margin-bottom: 4px;
  color: var(--primary);
}

.message-sent .message-sender {
  text-align: right;
  color: #4a9c5d;
}

.message-received .message-sender {
  text-align: left;
  color: var(--primary);
}

.staff-label {
  font-size: 0.7rem;
  font-weight: normal;
  font-style: italic;
  opacity: 0.8;
}

.message-image .image-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.message-image img,
.message-image video {
  max-width: 300px;
  max-height: 200px;
  border-radius: 12px;
  cursor: pointer;
  object-fit: cover;
  border: 1px solid var(--secondary);
}

.message-text {
  word-break: break-word;
  font-size: 0.95rem;
}

.message-time {
  font-size: 0.7rem;
  margin-top: 4px;
  text-align: right;
  display: flex;
  align-items: center;
  justify-content: flex-end;
}

.message-sent .message-time {
  color: var(--accent-light);
}

.message-received .message-time {
  color: var(--text-muted);
}

.text-accent {
  color: var(--primary);
}

.chat-input {
  background-color: var(--light-bg);
  border-top: 1px solid rgba(171, 162, 183, 0.3);
  padding: 12px;
  flex-shrink: 0;
  position: sticky;
  bottom: 0;
  z-index: 10; /* Đảm bảo không bị che */
}

.image-preview .image-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.preview-image {
  max-height: 100px;
  border-radius: 8px;
  border: 1px solid var(--secondary);
}

.input-group {
  background: linear-gradient(90deg, var(--primary), var(--secondary), var(--accent), var(--secondary));
  padding: 1px;
  border-radius: 25px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
}

.input-group > * {
  background-color: white;
}

.form-control {
  border: none;
  font-size: 0.9rem;
  border-radius: 25px 0 0 25px;
  color: var(--text-dark);
  flex-grow: 1;
}

.form-control::placeholder {
  color: var(--secondary);
}

.btn-primary {
  background: transparent;
  border-color: transparent;
  border-radius: 0 25px 25px 0;
  padding: 8px 16px;
  font-weight: 600;
  color: #61AFFE;
  /* background: linear-gradient(90deg, var(--primary), var(--secondary), var(--accent), var(--secondary)); */
  font-size: 0.9rem;
}

.btn-primary:hover {
  background: linear-gradient(90deg, #4ab6d3,)
}

.btn-primary:disabled {
  background: var(--secondary);
  opacity: 0.6;
}

.btn-outline-light {
  border-color: transparent;
  color: var(--accent);
  border-radius: 50%;
  padding: 6px;
}

.btn-outline-light:hover {
  background-color: rgba(236, 78, 121, 0.1);
  color: var(--accent);
}

.badge.bg-primary {
  background-color: var(--accent);
  font-size: 0.7rem;
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.badge.bg-accent {
  background-color: var(--primary);
  color: #61AFFE;
  font-size: 0.7rem;
}

.emoji-picker {
  position: absolute;
  bottom: 70px;
  left: 15px;
  background-color: white;
  border-radius: 10px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
  padding: 10px;
  width: 240px;
  z-index: 100;
}

.emoji-container {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 8px;
}

.emoji {
  font-size: 1.5rem;
  cursor: pointer;
  transition: transform 0.2s;
  text-align: center;
}

.emoji:hover {
  transform: scale(1.2);
}

.scroll-to-bottom {
  position: fixed;
  bottom: 80px;
  right: 20px;
  z-index: 10;
  display: none;
}

.chat-messages:hover .scroll-to-bottom {
  display: block;
}

.order-history {
  background-color: white;
  overflow-y: auto;
  width: 300px;
  flex-shrink: 0;
  max-height: 100%; /* Giới hạn chiều cao để cuộn */
}

/* Modal chi tiết hóa đơn */
.modal-lg {
  max-width: 800px;
}

.modal-content {
  border-radius: 10px;
}

.modal-header {
  background-color: var(--primary);
  color: white;
  border-bottom: none;
}

.modal-body {
  padding: 20px;
}

.table {
  font-size: 0.9rem;
}

.table th, .table td {
  vertical-align: middle;
  text-align: left;
}

.table th {
  background-color: var(--light-bg);
  color: var(--text-dark);
}

.table td {
  color: var(--text-dark);
}

/* Responsive styles */
@media (max-width: 991px) {
  .chat-sidebar {
    flex: 0 0 40%;
    max-width: 300px;
    min-width: 200px;
  }

  .order-history {
    width: 250px; /* Giảm chiều rộng */
  }

  .message-wrapper {
    max-width: 85%;
  }

  .message-image img,
  .message-image video {
    max-width: 250px;
    max-height: 180px;
  }

  .chat-item {
    padding: 10px;
  }

  .message-content {
    padding: 8px 12px;
  }

  .message-text {
    font-size: 0.9rem;
  }

  .message-time {
    font-size: 0.65rem;
  }
}

@media (max-width: 767px) {
  .chat-container {
    margin: 10px;
    height: calc(100vh - 40px);
  }

  .chat-sidebar {
    position: fixed;
    top: 0;
    left: 0;
    width: 80%;
    max-width: 280px;
    height: 100%;
    z-index: 1000;
    transform: translateX(-100%);
    box-shadow: 2px 0 5px rgba(0, 0, 0, 0.2);
  }

  .chat-sidebar.show {
    transform: translateX(0);
  }

  .chat-content {
    flex-direction: column; /* Chuyển sang cột */
  }

  .chat-messages {
    max-height: calc(100vh - 200px); /* Đảm bảo không bị tràn */
    min-height: 200px;
  }

  .order-history {
    width: 100%;
    border-left: none;
    border-top: 1px solid var(--secondary);
    max-height: 250px; /* Giới hạn chiều cao */
    flex-shrink: 0;
  }

  .chat-input {
    padding-bottom: 10px; /* Thêm padding để tránh che khuất */
  }

  .message-wrapper {
    max-width: 90%;
  }

  .message-avatar {
    width: 28px;
    height: 28px;
    margin-right: 6px;
  }

  .message-sent .message-avatar {
    margin-right: 0;
    margin-left: 6px;
  }

  .message-content {
    padding: 6px 10px;
    border-radius: 12px;
  }

  .message-text {
    font-size: 0.85rem;
  }

  .message-time {
    font-size: 0.6rem;
  }

  .message-image img,
  .message-image video {
    max-width: 200px;
    max-height: 150px;
  }

  .input-group {
    border-radius: 20px;
  }

  .form-control {
    font-size: 0.85rem;
    border-radius: 20px 0 0 20px;
  }

  .btn-primary {
    padding: 6px 12px;
    font-size: 0.85rem;
    border-radius: 0 20px 20px 0;
  }

  .btn-outline-light {
    padding: 4px;
  }

  .emoji-picker {
    bottom: 60px;
    width: 200px;
  }

  .emoji {
    font-size: 1.3rem;
  }

  .message-sender {
    font-size: 0.7rem;
  }

  .staff-label {
    font-size: 0.65rem;
  }

  .scroll-to-bottom {
    bottom: 70px;
    right: 10px;
  }
}
</style>