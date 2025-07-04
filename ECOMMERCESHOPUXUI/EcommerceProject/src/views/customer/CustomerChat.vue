<script setup>
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Cookies from 'js-cookie';
import Swal from 'sweetalert2';

// Import từ rtdb-config
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
} from '../../firebase/rtdb-config';

import { GetApiUrl } from '../../constants/api.js';
import { fetchWithAuth } from '@/services/authService';
import ConnectionStatus from '../../components/ConnectionStatus.vue';

// States
const accessToken = ref(Cookies.get('accessToken'));
const userId = ref(null);
const userName = ref('');
const userAvatar = ref('');
const activeChats = ref([]);
const currentChat = ref(null);
const messages = ref([]);
const newMessage = ref('');
const imageFiles = ref([]); // Thay imageFile bằng mảng để hỗ trợ nhiều ảnh
const imagePreviews = ref([]); // Thay imagePreview bằng mảng để hiển thị nhiều ảnh preview
const isLoading = ref(true);
const showEmojiPicker = ref(false);
const isTyping = ref(false);
const typingTimeout = ref(null);
const messagesListener = ref(null);
const chatsListener = ref(null);
const emojiList = ref(['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘']);
const isOnline = ref(navigator.onLine);
const staffList = ref([]);
const apiUrl = 'https://localhost:7217';
// Notification states
const lastMessageCount = ref(0);
const notificationEnabled = ref(true);
const audioContext = ref(null);

const router = useRouter();
const route = useRoute();
const getApiUrl = GetApiUrl();
const getImageUrl = (relativePath) => {
  if (!relativePath) return 'Không ảnh';
  if (relativePath.includes('AnhKhachHang')) {
    const fileName = relativePath.split('/').pop();
    return `${apiUrl}/api/Customer/image/${fileName}`;
  }
  return `${apiUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
};
// Computed properties
const sortedMessages = computed(() => {
  return [...messages.value].sort((a, b) => a.thoiGian - b.thoiGian);
});

const hasUnreadMessages = computed(() => {
  return activeChats.value.some(chat => chat.soTinNhanChuaDoc > 0);
});

// Thêm hàm để xóa trạng thái 'active' không chính xác
const cleanupActiveStatus = async () => {
  try {
    console.log('🧹 Bắt đầu dọn dẹp trạng thái active không chính xác...');

    const conversationsRef = dbRef(rtdb, 'conversations');
    const snapshot = await get(conversationsRef);

    if (!snapshot.exists()) {
      console.log('❌ Không có dữ liệu conversations');
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
      console.log(`✅ Đã xóa trạng thái 'active' không chính xác trong ${count} cuộc trò chuyện`);
    } else {
      console.log('✅ Không có trạng thái active không chính xác nào cần xóa');
    }
  } catch (error) {
    console.error('❌ Lỗi khi xóa trạng thái active:', error);
  }
};

// Methods
const checkAuth = async () => {
  if (!accessToken.value) {
    router.push('/Login?redirect=/chat');
    return false;
  }

  try {
    const response = await fetchWithAuth(`${getApiUrl}/api/Chat/GetUserInfo`);

    if (!response.ok) {
      console.error('Lỗi API:', await response.text());
      throw new Error('Không thể xác thực');
    }

    const result = await response.json();

    if (!result.success) {
      throw new Error(result.message || 'Không thể xác thực');
    }

    userId.value = result.data.id;
    userName.value = result.data.hoTen;
    userAvatar.value = result.data.hinh || '/default-avatar.png';
    console.log('Xác thực thành công:', result.data);

    updateOnlineStatus(true);

    return true;
  } catch (error) {
    console.error('Lỗi xác thực:', error);

    Swal.fire({
      icon: 'error',
      title: 'Lỗi xác thực',
      text: 'Vui lòng đăng nhập lại để tiếp tục.',
      confirmButtonText: 'OK'
    }).then(() => {
      router.push('/Login?redirect=/chat');
    });

    return false;
  }
};

const updateOnlineStatus = (isOnlineStatus) => {
  if (!userId.value) return;

  if (!navigator.onLine && isOnlineStatus) {
    isOnlineStatus = false;
  }

  console.log(`🔄 Cập nhật trạng thái online: ${isOnlineStatus}`);

  try {
    const userStatusRef = dbRef(rtdb, `userStatus/${userId.value}`);
    set(userStatusRef, {
      isOnline: isOnlineStatus,
      lastSeen: rtdbServerTimestamp(),
      name: userName.value,
      avatar: userAvatar.value,
      type: 'customer'
    });
  } catch (error) {
    console.error('❌ Lỗi khi cập nhật trạng thái:', error);
  }
};

// Tạo âm thanh thông báo
const createNotificationSound = () => {
  try {
    audioContext.value = new (window.AudioContext || window.webkitAudioContext)();

    const playNotificationSound = () => {
      if (!audioContext.value || !notificationEnabled.value) return;

      try {
        const oscillator1 = audioContext.value.createOscillator();
        const oscillator2 = audioContext.value.createOscillator();
        const gainNode = audioContext.value.createGain();

        oscillator1.frequency.setValueAtTime(800, audioContext.value.currentTime);
        oscillator2.frequency.setValueAtTime(600, audioContext.value.currentTime);

        oscillator1.type = 'sine';
        oscillator2.type = 'sine';

        gainNode.gain.setValueAtTime(0, audioContext.value.currentTime);
        gainNode.gain.linearRampToValueAtTime(0.3, audioContext.value.currentTime + 0.1);
        gainNode.gain.linearRampToValueAtTime(0, audioContext.value.currentTime + 0.3);

        oscillator1.connect(gainNode);
        oscillator2.connect(gainNode);
        gainNode.connect(audioContext.value.destination);

        oscillator1.start(audioContext.value.currentTime);
        oscillator1.stop(audioContext.value.currentTime + 0.3);

        setTimeout(() => {
          if (!audioContext.value) return;

          const oscillator3 = audioContext.value.createOscillator();
          const gainNode2 = audioContext.value.createGain();

          oscillator3.frequency.setValueAtTime(800, audioContext.value.currentTime);
          oscillator3.type = 'sine';

          gainNode2.gain.setValueAtTime(0, audioContext.value.currentTime);
          gainNode2.gain.linearRampToValueAtTime(0.3, audioContext.value.currentTime + 0.1);
          gainNode2.gain.linearRampToValueAtTime(0, audioContext.value.currentTime + 0.3);

          oscillator3.connect(gainNode2);
          gainNode2.connect(audioContext.value.destination);

          oscillator3.start(audioContext.value.currentTime);
          oscillator3.stop(audioContext.value.currentTime + 0.3);
        }, 200);

      } catch (error) {
        console.log('Không thể phát âm thanh thông báo:', error);
      }
    };

    return playNotificationSound;
  } catch (error) {
    console.log('Không thể tạo AudioContext:', error);
    return () => { };
  }
};

const toggleNotification = () => {
  notificationEnabled.value = !notificationEnabled.value;

  if (notificationEnabled.value) {
    if (Notification.permission === 'default') {
      Notification.requestPermission();
    }

    Swal.fire({
      icon: 'success',
      title: 'Đã bật thông báo',
      text: 'Bạn sẽ nghe âm thanh khi có tin nhắn mới.',
      timer: 2000,
      showConfirmButton: false
    });
  } else {
    Swal.fire({
      icon: 'info',
      title: 'Đã tắt thông báo',
      text: 'Bạn sẽ không nghe âm thanh thông báo.',
      timer: 2000,
      showConfirmButton: false
    });
  }
};

const loadActiveChats = () => {
  try {
    const chatsRef = dbRef(rtdb, 'conversations');

    if (chatsListener.value) {
      off(chatsRef, 'value', chatsListener.value);
    }

    chatsListener.value = onValue(chatsRef, (snapshot) => {
      const data = snapshot.val();
      if (!data) {
        activeChats.value = [];
        isLoading.value = false;
        return;
      }

      const chats = Object.entries(data)
        .filter(([_, chat]) => chat.maKH === userId.value)
        .map(([id, chat]) => ({
          id,
          ...chat,
          ngayCapNhat: chat.ngayCapNhat || Date.now(),
          staffOnline: false
        }))
        .sort((a, b) => b.ngayCapNhat - a.ngayCapNhat);

      activeChats.value = chats;

      chats.forEach(chat => {
        if (chat.maNV) {
          const staffStatusRef = dbRef(rtdb, `userStatus/${chat.maNV}`);
          onValue(staffStatusRef, (statusSnapshot) => {
            const statusData = statusSnapshot.val();
            const isOnline = statusData && statusData.isOnline === true;

            const index = activeChats.value.findIndex(c => c.id === chat.id);
            if (index !== -1) {
              console.log(`🔄 Cập nhật trạng thái online của nhân viên ${chat.tenNV}:`, isOnline);
              activeChats.value[index].staffOnline = isOnline;
            }
          }, { onlyOnce: false });
        }
      });

      isLoading.value = false;

      console.log('🔍 Tìm thấy chats cho user:', userId.value, activeChats.value);

      const chatId = route.params.id;
      if (chatId && chats.find(chat => chat.id === chatId)) {
        selectChat(chatId);
      } else if (chats.length > 0) {
        selectChat(chats[0].id);
      } else {
        console.log('🆕 Không có chat nào, tạo mới...');
        createNewChat();
      }
    }, (error) => {
      console.error('Lỗi khi tải danh sách chat:', error);
      isLoading.value = false;

      Swal.fire({
        icon: 'error',
        title: 'Lỗi tải dữ liệu',
        text: 'Không thể tải danh sách trò chuyện. Vui lòng thử lại sau.',
        confirmButtonText: 'OK'
      });
    });
  } catch (error) {
    console.error('Lỗi khi thiết lập listener cho danh sách chat:', error);
    isLoading.value = false;
  }
};

const selectChat = (chatId) => {
  try {
    if (messagesListener.value) {
      off(dbRef(rtdb, `messages/${currentChat.value?.id}`), 'value', messagesListener.value);
    }

    router.push(`/chat/${chatId}`);

    const chatRef = dbRef(rtdb, `conversations/${chatId}`);
    onValue(chatRef, (snapshot) => {
      const chatData = snapshot.val();
      if (!chatData) {
        Swal.fire({
          icon: 'error',
          title: 'Không tìm thấy',
          text: 'Cuộc trò chuyện không tồn tại hoặc đã bị xóa.',
          confirmButtonText: 'OK'
        });
        router.push('/chat');
        return;
      }

      currentChat.value = {
        id: chatId,
        ...chatData,
        staffOnline: false
      };

      if (chatData.maNV) {
        const staffStatusRef = dbRef(rtdb, `userStatus/${chatData.maNV}`);
        onValue(staffStatusRef, (staffSnapshot) => {
          const staffData = staffSnapshot.val();
          const isOnline = staffData && staffData.isOnline === true;

          console.log(`🔄 Cập nhật trạng thái online của nhân viên ${chatData.tenNV}:`, isOnline);

          if (currentChat.value && currentChat.value.id === chatId) {
            currentChat.value.staffOnline = isOnline;
          }

          staffList.value = [{
            id: chatData.maNV,
            name: staffData ? staffData.name : (chatData.tenNV || 'Nhân viên'),
            isOnline: isOnline,
            lastSeen: staffData ? staffData.lastSeen : Date.now(),
            avatar: staffData ? staffData.avatar : '/default-avatar.png'
          }];
        });
      } else {
        staffList.value = [];
      }

      set(dbRef(rtdb, `conversations/${chatId}/soTinNhanChuaDoc`), 0);

      const messagesRef = dbRef(rtdb, `messages/${chatId}`);
      messagesListener.value = onValue(messagesRef, (snapshot) => {
        const messagesData = snapshot.val();
        if (!messagesData) {
          messages.value = [];
          lastMessageCount.value = 0;
          return;
        }

        const messagesList = Object.entries(messagesData).map(([id, message]) => ({
          id,
          ...message,
          thoiGian: typeof message.thoiGian === 'number' ? message.thoiGian : Date.now()
        }));

        const currentMessageCount = messagesList.length;

        if (currentMessageCount > lastMessageCount.value && lastMessageCount.value > 0) {
          const latestMessage = messagesList[messagesList.length - 1];
          const isMyMessage = latestMessage.nguoiGui === userId.value;

          if (!isMyMessage && notificationEnabled.value) {
            console.log('🔔 Tin nhắn mới từ:', latestMessage.tenNguoiGui);

            const playSound = createNotificationSound();
            playSound();

            if (Notification.permission === 'granted' && document.hidden) {
              new Notification('Tin nhắn mới', {
                body: latestMessage.noiDung || '📷 Hình ảnh',
                icon: '/favicon.ico',
                tag: 'chat-message'
              });
            }
          }
        }

        lastMessageCount.value = currentMessageCount;
        messages.value = messagesList;

        Object.entries(messagesData).forEach(([id, message]) => {
          if (message.loaiNguoiGui === 'staff' && !message.daDoc) {
            set(dbRef(rtdb, `messages/${chatId}/${id}/daDoc`), true);
            set(dbRef(rtdb, `messages/${chatId}/${id}/thoiGianDoc`), rtdbServerTimestamp());
          }
        });

        nextTick(() => {
          scrollToBottom();
        });
      });
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

const createNewChat = async () => {
  try {
    console.log('🆕 Tạo chat mới cho user:', userId.value);

    const existingChats = activeChats.value.filter(chat => chat.maKH === userId.value);
    if (existingChats.length > 0) {
      console.log('✅ Đã có chat tồn tại, chọn chat đầu tiên');
      selectChat(existingChats[0].id);
      return;
    }

    const conversationsRef = dbRef(rtdb, 'conversations');
    const newChatRef = push(conversationsRef);
    const chatId = newChatRef.key;

    console.log('🆕 Tạo chat mới với ID:', chatId);

    const chatData = {
      maKH: userId.value,
      tenKH: userName.value,
      anhDaiDienKH: userAvatar.value,
      tinNhanCuoi: 'Xin chào, tôi cần hỗ trợ.',
      thoiGianTinNhanCuoi: rtdbServerTimestamp(),
      soTinNhanChuaDoc: 0,
      soTinNhanChuaDocStaff: 1,
      ngayTao: rtdbServerTimestamp(),
      ngayCapNhat: rtdbServerTimestamp()
    };

    await set(newChatRef, chatData);
    console.log('✅ Đã lưu chat data');

    const messagesRef = dbRef(rtdb, `messages/${chatId}`);
    const newMessageRef = push(messagesRef);
    await set(newMessageRef, {
      nguoiGui: userId.value,
      tenNguoiGui: userName.value,
      loaiNguoiGui: 'customer',
      noiDung: 'Xin chào, tôi cần hỗ trợ.',
      thoiGian: rtdbServerTimestamp(),
      daDoc: false,
      trangThai: 'sent',
      loai: 'text'
    });

    console.log('✅ Đã tạo tin nhắn đầu tiên');

    selectChat(chatId);
  } catch (error) {
    console.error('Lỗi khi tạo chat mới:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể tạo cuộc trò chuyện mới. Vui lòng thử lại sau.',
      confirmButtonText: 'OK'
    });
  }
};

const sendMessage = async () => {
  if (!currentChat.value) {
    console.log('❌ Không có currentChat, không thể gửi tin nhắn');
    return;
  }

  if (!newMessage.value.trim() && imageFiles.value.length === 0) {
    console.log('❌ Tin nhắn trống, không gửi');
    return;
  }

  try {
    let imageUrls = [];

    if (imageFiles.value.length > 0) {
      for (const file of imageFiles.value) {
        const formData = new FormData();
        formData.append('file', file);

        const uploadResponse = await fetch(`${getApiUrl}/api/Chat/upload-media`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${accessToken.value}`
          },
          body: formData
        });

        if (!uploadResponse.ok) {
          throw new Error('Không thể upload file');
        }

        const uploadResult = await uploadResponse.json();

        if (uploadResult.success) {
          imageUrls.push(`${getApiUrl}${uploadResult.data.url}`);
        } else {
          throw new Error(uploadResult.message || 'Upload thất bại');
        }
      }
    }

    const isVideoOnly = imageFiles.value.length > 0 && imageFiles.value.every(file => file.type.startsWith('video/'));
    const messageData = {
      nguoiGui: userId.value,
      tenNguoiGui: userName.value,
      loaiNguoiGui: 'customer',
      noiDung: newMessage.value.trim(),
      anhUrls: imageUrls.length > 0 ? imageUrls : null,
      thoiGian: rtdbServerTimestamp(),
      daDoc: false,
      trangThai: 'sent',
      loai: isVideoOnly && !newMessage.value.trim() ? 'video' : (imageUrls.length > 0 ? 'image' : 'text')
    };

    console.log('📤 Gửi tin nhắn:', messageData);

    const messagesRef = dbRef(rtdb, `messages/${currentChat.value.id}`);
    const newMessageRef = push(messagesRef);
    await set(newMessageRef, messageData);

    const updateData = {
      tinNhanCuoi: imageUrls.length > 0 ? (isVideoOnly ? '🎥 Video' : '📷 Hình ảnh') : newMessage.value.trim(),
      thoiGianTinNhanCuoi: rtdbServerTimestamp(),
      soTinNhanChuaDocStaff: (currentChat.value.soTinNhanChuaDocStaff || 0) + 1,
      ngayCapNhat: rtdbServerTimestamp()
    };

    Object.keys(updateData).forEach(key => {
      set(dbRef(rtdb, `conversations/${currentChat.value.id}/${key}`), updateData[key]);
    });

    console.log('✅ Đã cập nhật chat:', currentChat.value.id);

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

const handleFileUpload = (event) => {
  const files = Array.from(event.target.files);
  if (!files.length) return;

  const validFiles = files.filter(file => {
    // Tăng giới hạn lên 20MB
    if (file.size > 20 * 1024 * 1024) {
      Swal.fire({
        icon: 'error',
        title: 'Lỗi',
        text: `File ${file.name} quá lớn. Vui lòng chọn file nhỏ hơn 20MB.`,
        confirmButtonText: 'OK'
      });
      return false;
    }

    // Kiểm tra định dạng file
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

// Cập nhật hàm removeImage để xóa cả video
const removeImage = (index) => {
  imageFiles.value.splice(index, 1);
  imagePreviews.value.splice(index, 1);
};

const addEmoji = (emoji) => {
  newMessage.value += emoji;
  showEmojiPicker.value = false;
};

const handleTyping = () => {
  if (!currentChat.value) return;

  clearTimeout(typingTimeout.value);

  const typingRef = dbRef(rtdb, `userStatus/${userId.value}/isTyping`);
  set(typingRef, true);

  typingTimeout.value = setTimeout(() => {
    set(typingRef, false);
  }, 2000);
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
  const chatContainer = document.querySelector('.chat-messages');
  if (chatContainer) {
    chatContainer.scrollTop = chatContainer.scrollHeight;
  }
};

const handleConnectionChange = () => {
  const prevOnlineState = isOnline.value;
  isOnline.value = navigator.onLine;

  console.log(`🌐 Trạng thái kết nối thay đổi: ${prevOnlineState} -> ${isOnline.value}`);

  if (prevOnlineState !== isOnline.value) {
    updateOnlineStatus(isOnline.value);

    if (!isOnline.value) {
      Swal.fire({
        icon: 'warning',
        title: 'Mất kết nối',
        text: 'Bạn đang ở chế độ offline. Một số tính năng sẽ không hoạt động cho đến khi kết nối được khôi phục.',
        confirmButtonText: 'OK'
      });
    } else {
      if (userId.value) {
        loadActiveChats();
      }
    }
  }
};

// Lifecycle hooks
onMounted(async () => {
  window.addEventListener('online', handleConnectionChange);
  window.addEventListener('offline', handleConnectionChange);

  if (Notification.permission === 'default') {
    await Notification.requestPermission();
  }

  const initAudio = () => {
    createNotificationSound();
    document.removeEventListener('click', initAudio);
    document.removeEventListener('keydown', initAudio);
  };

  document.addEventListener('click', initAudio);
  document.addEventListener('keydown', initAudio);

  const isAuthenticated = await checkAuth();
  if (isAuthenticated) {
    await cleanupActiveStatus();
    updateOnlineStatus(navigator.onLine);
    loadActiveChats();
  }

  // Đảm bảo scroll xuống dưới cùng khi giao diện được tải lần đầu
  nextTick(() => {
    scrollToBottom();
  });
});
const openMedia = (url) => {
  if (typeof window !== 'undefined' && window.open) {
    window.open(url, '_blank');
  } else {
    console.error('Không thể mở cửa sổ mới. Window.open không khả dụng.');
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể mở hình ảnh. Vui lòng thử lại hoặc kiểm tra trình duyệt.',
      confirmButtonText: 'OK'
    });
  }
};
onUnmounted(() => {
  window.removeEventListener('online', handleConnectionChange);
  window.removeEventListener('offline', handleConnectionChange);

  if (messagesListener.value && currentChat.value) {
    off(dbRef(rtdb, `messages/${currentChat.value.id}`), 'value', messagesListener.value);
  }

  if (chatsListener.value) {
    off(dbRef(rtdb, 'conversations'), 'value', chatsListener.value);
  }

  if (audioContext.value) {
    audioContext.value.close();
  }

  if (userId.value) {
    const userStatusRef = dbRef(rtdb, `userStatus/${userId.value}`);
    set(userStatusRef, {
      isOnline: false,
      lastSeen: rtdbServerTimestamp(),
      name: userName.value,
      avatar: userAvatar.value,
      type: 'customer'
    });
  }
});
</script>
<template>
  <div class="chat-container">
    <ConnectionStatus />

    <!-- Header tổng đài -->
    <div class="chat-header-main">
      <div class="user-status">
        <span :class="{ 'online': isOnline, 'offline': !isOnline }">
          {{ isOnline ? 'Đang kết nối' : 'Mất kết nối' }}
        </span>
      </div>
    </div>

    <div v-if="isLoading" class="loading-overlay">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
    </div>

    <div v-else class="chat-body row g-0">
      <!-- Danh sách chat -->
      <div class="col-md-4 col-lg-3 chat-sidebar">
        <div class="chat-list-header p-3">
          <div class="d-flex justify-content-between align-items-center">
            <h5 class="mb-0">Cuộc trò chuyện</h5>
            <span class="badge bg-accent rounded-pill ms-2" v-if="hasUnreadMessages">Mới</span>
            <button class="btn btn-sm btn-outline-light" @click="toggleNotification"
              :title="notificationEnabled ? 'Tắt thông báo' : 'Bật thông báo'">
              <i class="bi"
                :class="notificationEnabled ? 'bi-bell-fill text-success' : 'bi-bell-slash-fill text-muted'"></i>
            </button>
          </div>
        </div>

        <div class="chat-list">
          <div v-for="chat in activeChats" :key="chat.id" class="chat-item p-3"
            :class="{ 'active': currentChat && chat.id === currentChat.id }" @click="selectChat(chat.id)">
            <div class="d-flex align-items-center">
              <div class="position-relative">
                <img :src="getImageUrl(chat.anhDaiDienKH) || '/default-avatar.png'" alt="Avatar" class="chat-avatar">
                <span v-if="chat.staffOnline" class="online-indicator" title="Nhân viên đang online"></span>
              </div>
              <div class="ms-3 flex-grow-1">
                <div class="d-flex justify-content-between align-items-center">
                  <h6 class="mb-0 text-truncate">
                    {{ chat.tenKH }}
                    <span v-if="chat.staffOnline" class="badge bg-accent ms-1">
                      Online
                    </span>
                  </h6>
                  <small class="text-muted">{{ formatTime(chat.thoiGianTinNhanCuoi) }}</small>
                </div>
                <p class="text-muted mb-0 text-truncate">
                  <span v-if="chat.tinNhanCuoi && chat.tinNhanCuoi.startsWith('📷')">
                    <i class="bi bi-image me-1"></i>
                  </span>
                  <span v-else-if="chat.tinNhanCuoi && chat.tinNhanCuoi.startsWith('🎥')">
                    <i class="bi bi-film me-1"></i>
                  </span>
                  {{ chat.tinNhanCuoi }}
                </p>
                <span v-if="chat.soTinNhanChuaDoc > 0" class="badge bg-primary rounded-pill float-end">
                  {{ chat.soTinNhanChuaDoc }}
                </span>
              </div>
            </div>
          </div>

          <div v-if="activeChats.length === 0" class="p-3 text-center">
            <p class="text-muted">Chưa có cuộc trò chuyện</p>
            <button class="btn btn-primary btn-sm mt-2" @click="createNewChat">
              <i class="bi bi-chat-dots me-1"></i> Bắt đầu trò chuyện
            </button>
          </div>
        </div>
      </div>

      <!-- Nội dung chat -->
      <div class="col-md-8 col-lg-9 chat-main">
        <div v-if="currentChat" class="chat-header p-3">
          <div class="d-flex align-items-center">
            <img :src="staffList.length > 0 ? (getImageUrl(staffList[0].avatar) || '/default-avatar.png') : '/staff-avatar.png'"
              alt="Avatar" class="chat-avatar">
            <div class="ms-3">
              <h6 class="mb-0">
                {{ staffList.length > 0 ? staffList[0].name : 'Nhân viên hỗ trợ' }}
              </h6>
              <small class="text-muted">
                {{ staffList.length > 0 && staffList[0].isOnline ? 'Đang online' : 'Ngoại tuyến' }}
              </small>
            </div>
          </div>
        </div>

        <div v-if="currentChat" class="chat-messages p-4">
          <div v-for="(message, index) in sortedMessages" :key="message.id" class="message mb-3" :class="{
            'message-sent': message.loaiNguoiGui === 'customer',
            'message-received': message.loaiNguoiGui === 'staff'
          }">
            <div class="message-wrapper">
              <img v-if="message.loaiNguoiGui === 'staff'"
                :src="staffList.length > 0 ? (getImageUrl(staffList[0].avatar) || '/default-avatar.png') : '/staff-avatar.png'"
                alt="Staff Avatar" class="message-avatar">
              <div class="message-content">
                <div v-if="message.anhUrls && message.anhUrls.length > 0" class="message-image mb-2">
                  <div class="image-gallery">
                    <template v-for="(url, imgIndex) in message.anhUrls" :key="imgIndex">
                      <img v-if="url && url.match(/\.(jpg|jpeg|png|gif|webp)$/i)" :src="url" alt="Hình ảnh"
                        @click="() => openMedia(url)">
                      <video v-else-if="url && url.match(/\.(mp4|webm|ogg)$/i)" :src="url" controls
                        class="message-video"></video>
                    </template>
                  </div>
                </div>
                <div v-if="message.noiDung" class="message-text">{{ message.noiDung }}</div>
                <div class="message-time">
                  {{ formatTime(message.thoiGian) }}
                  <span v-if="message.loaiNguoiGui === 'customer'" class="ms-1">
                    <i class="bi" :class="{
                      'bi-check': message.trangThai === 'sent',
                      'bi-check-all': message.trangThai === 'delivered',
                      'bi-check-all text-accent': message.daDoc
                    }"></i>
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="messages.length === 0" class="text-center my-5">
            <p class="text-muted">Bắt đầu trò chuyện ngay!</p>
          </div>
        </div>

        <div v-if="currentChat" class="chat-input p-3">
          <div v-if="imagePreviews.length > 0" class="image-preview mb-2">
            <div class="image-gallery">
              <div v-for="(preview, index) in imagePreviews" :key="index" class="position-relative d-inline-block">
                <img v-if="preview.type === 'image'" :src="preview.url" alt="Preview" class="preview-image">
                <video v-else-if="preview.type === 'video'" :src="preview.url" controls class="preview-image"></video>
                <button type="button" class="btn-close position-absolute top-0 end-0"
                  @click="removeImage(index)"></button>
              </div>
            </div>
          </div>

          <div class="input-group">
            <button class="btn btn-outline-light" @click="() => showEmojiPicker = !showEmojiPicker">
              <i class="bi bi-emoji-smile"></i>
            </button>

            <label class="btn btn-outline-light" for="file-upload">
              <i class="bi bi-film"></i>
              <input type="file" id="file-upload" class="d-none" accept="image/*,video/*" multiple
                @change="handleFileUpload">
            </label>

            <input type="text" class="form-control" placeholder="Nhập tin nhắn..." v-model="newMessage"
              @input="handleTyping" @keypress.enter="sendMessage">

            <button class="btn btn-primary" @click="sendMessage" :disabled="!isOnline">
              <i class="bi bi-send"></i> Gửi
            </button>
          </div>

          <div v-if="showEmojiPicker" class="emoji-picker">
            <div class="emoji-container">
              <span v-for="emoji in emojiList" :key="emoji" class="emoji" @click="addEmoji(emoji)">
                {{ emoji }}
              </span>
            </div>
          </div>
        </div>

        <div v-if="!currentChat" class="d-flex flex-column justify-content-center align-items-center h-100">
          <i class="bi bi-chat-dots fs-1 text-muted"></i>
          <h5 class="mt-3">Chọn một cuộc trò chuyện để bắt đầu</h5>
          <button v-if="activeChats.length === 0" class="btn btn-primary mt-3" @click="createNewChat">
            Bắt đầu trò chuyện mới
          </button>
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
  /* Xanh lam nhạt */
  --secondary: #ABA2B7;
  /* Xám tím nhạt */
  --accent: #EC4E79;
  /* Hồng đậm */
  --light-bg: #f6f7f9;
  /* Nền sáng, nhạt từ tông xám tím */
  --text-dark: #2d2d2d;
  /* Chữ tối */
  --text-muted: #7a7a7a;
  /* Chữ nhạt */
  --accent-light: #ffe6ec;
  /* Màu nhạt từ #EC4E79 */
}

.chat-container {
  /* width: 1430px; */
  height: calc(100vh - 120px);
  background-color: var(--light-bg);
  border-radius: 15px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  font-family: 'Inter', sans-serif;
  margin: 50px;
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
  height: 100%;
  display: flex;
  flex-direction: column;
}

.chat-list-header {
  background-color: var(--primary);
  color: white;
  font-family: 'Poppins', sans-serif;
  padding: 8px 12px;
  flex-shrink: 0;
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
  padding: 8px 12px;
}

.chat-item:hover {
  background-color: rgba(92, 202, 231, 0.1);
}

.chat-item.active {
  background-color: rgba(92, 202, 231, 0.2);
  border-left: 3px solid var(--primary);
}

.chat-avatar {
  width: 36px;
  height: 36px;
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
  display: flex;
  flex-direction: column;
  height: 100%;
}

.chat-header {
  background-color: var(--light-bg);
  border-bottom: 1px solid rgba(171, 162, 183, 0.3);
  padding: 8px 12px;
  flex-shrink: 0;
}

.chat-messages {
  flex-grow: 1;
  overflow-y: auto;
  background-color: var(--light-bg);
  padding: 15px;
  scroll-behavior: smooth;
  /* Thêm hiệu ứng cuộn mượt */
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
  max-width: 70%;
}

.message-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  margin-right: 8px;
  flex-shrink: 0;
}

.message-content {
  padding: 8px 12px;
  border-radius: 15px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
  position: relative;
  max-width: 100%;
  /* font-weight: bold; */
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

.message-image .image-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.message-image img {
  max-width: 140px;
  border-radius: 8px;
  cursor: pointer;
}

.message-text {
  word-break: break-word;
  font-size: 0.95rem;
}

.message-time {
  font-size: 0.65rem;
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
  padding: 10px 15px;
  flex-shrink: 0;
}

.image-preview .image-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
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

.input-group>* {
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
  background-color: transparent;
  border-color: transparent;
  border-radius: 0 25px 25px 0;
  padding: 6px 15px;
  font-weight: 600;
  color: #61AFFE;
  background: linear-gradient(90deg, var(--primary), var(--secondary), var(--accent), var(--secondary));
  font-size: 0.9rem;
}

.btn-primary:hover {
  background: linear-gradient(90deg, #4ab6d3, #9a91a6, #d43e68, #9a91a6);
}

.btn-primary:disabled {
  background: var(--secondary);
  opacity: 0.6;
}

.btn-outline-light {
  border-color: transparent;
  color: var(--accent);
  border-radius: 50%;
  padding: 4px;
}

.btn-outline-light:hover {
  background-color: rgba(236, 78, 121, 0.1);
  color: var(--accent);
}

.badge.bg-primary {
  background-color: var(--accent);
  font-size: 0.65rem;
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.badge.bg-accent {
  background-color: var(--primary);
  color: #61AFFE;
  font-size: 0.65rem;
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
  gap: 6px;
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

@media (max-width: 991px) {
  .chat-sidebar {
    display: none;
  }

  .chat-main {
    width: 100%;
  }

  .chat-container {
    margin: 10px;
    height: calc(100vh - 40px);
  }

  .chat-messages {
    height: 50vh;
  }

  .message-wrapper {
    max-width: 85%;
  }

  .emoji-picker {
    width: 200px;
  }

  .chat-item {
    padding: 6px 10px;
  }

  .message-content {
    padding: 6px 10px;
  }

  .message-text {
    font-size: 0.85rem;
  }

  .message-time {
    font-size: 0.6rem;
  }
}

@media (max-width: 576px) {
  .chat-messages {
    padding: 10px;
  }

  .message-avatar {
    width: 24px;
    height: 24px;
    margin-right: 6px;
  }

  .message-content {
    padding: 5px 8px;
    border-radius: 12px;
  }

  .message-text {
    font-size: 0.8rem;
  }

  .message-time {
    font-size: 0.55rem;
  }

  .input-group {
    border-radius: 20px;
  }

  .form-control {
    font-size: 0.8rem;
    border-radius: 20px 0 0 20px;
  }

  .btn-primary {
    padding: 5px 12px;
    font-size: 0.8rem;
    border-radius: 0 20px 20px 0;
  }

  .btn-outline-light {
    padding: 3px;
  }

  .emoji-picker {
    bottom: 60px;
    width: 180px;
  }

  .emoji {
    font-size: 1.2rem;
  }
}

.message-image .image-gallery,
.image-preview .image-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.message-image img,
.message-image video,
.preview-image,
.preview-image video {
  max-width: 300px;
  max-height: 200px;
  border-radius: 12px;
  cursor: pointer;
  object-fit: cover;
  border: 1px solid var(--secondary);
}

.message-wrapper {
  display: flex;
  align-items: flex-start;
  max-width: 80%;
}

.chat-messages {
  flex-grow: 1;
  overflow-y: auto;
  background-color: var(--light-bg);
  padding: 20px;
  scroll-behavior: smooth;
}

@media (max-width: 991px) {
  .message-wrapper {
    max-width: 90%;
  }

  .message-image img,
  .message-image video,
  .preview-image,
  .preview-image video {
    max-width: 250px;
    max-height: 180px;
  }
}

@media (max-width: 576px) {
  .message-wrapper {
    max-width: 95%;
  }

  .message-image img,
  .message-image video,
  .preview-image,
  .preview-image video {
    max-width: 200px;
    max-height: 150px;
  }
}
</style>