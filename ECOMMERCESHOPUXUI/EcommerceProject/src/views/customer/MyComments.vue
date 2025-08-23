<template>
  <div class="container mt-4 vh-100">
    <h2 class="text-center mb-4">Bình luận của tôi</h2>

    <div v-if="loading" class="text-center">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Đang tải...</span>
      </div>
      <p>Đang tải bình luận của bạn...</p>
    </div>

    <div v-else-if="comments.length === 0" class="text-center text-muted">
      <p>Bạn chưa có bình luận nào.</p>
    </div>

    <div v-else>
      <div v-for="comment in comments" :key="comment.id" class="card mb-3">
        <div class="card-body">
          <h5 class="card-title">{{ comment.noiDung }}</h5>
          <h6 class="card-subtitle mb-2 text-muted">
            Bình luận vào: {{ formatDate(comment.ngayBinhLuan) }}
          </h6>
          <p class="card-text">
            Sản phẩm: 
            <router-link v-if="comment.maSP" :to="`/product/${comment.maSP}`">
              {{ comment.maSP }}
            </router-link>
            <router-link v-else-if="comment.maCombo" :to="`/combo/${comment.maCombo}`">
              {{ comment.maCombo }}
            </router-link>
          </p>
          <div v-if="comment.replies && comment.replies.length > 0">
            <h6>Phản hồi:</h6>
            <div v-for="reply in comment.replies" :key="reply.id" class="card bg-light mb-2 ms-4">
              <div class="card-body">
                <p class="card-text">{{ reply.noiDung }}</p>
                <small class="text-muted">{{ formatDate(reply.ngayBinhLuan) }}</small>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { GetApiUrl } from '@/constants/api';
import { validateToken, decodeToken } from '@/utils/auth';
import Cookies from 'js-cookie';

const getUrlAPI = ref(GetApiUrl());
const comments = ref([]);
const loading = ref(true);
const accessToken = ref(Cookies.get('accessToken'));
const refreshToken = ref(Cookies.get('refreshToken'));

const fetchMyComments = async () => {
  try {
    const validatetoken = await validateToken(accessToken.value, refreshToken.value);
    if (!validatetoken.isValid) {
      // Redirect to login or show a message
      return;
    }
    accessToken.value = validatetoken.newAccessToken;
    const decodedToken = decodeToken(accessToken.value);
    const userId = decodedToken.IdUser;

    const response = await fetch(`${getUrlAPI.value}/api/Comment/my-comments`, {
      headers: {
        'Authorization': `Bearer ${accessToken.value}`,
      },
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    if (result.success) {
      comments.value = result.data;
    } else {
      console.error('Failed to fetch comments:', result.message);
    }
  } catch (error) {
    console.error('Error fetching my comments:', error);
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
};

onMounted(() => {
  fetchMyComments();
});
</script>

<style scoped>
.container {
  max-width: 900px;
  margin: 40px auto;
  padding: 20px;
  background-color: #f8f9fa;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

h2 {
  color: #343a40;
  font-weight: 700;
  margin-bottom: 30px;
  position: relative;
  padding-bottom: 10px;
}

h2::after {
  content: '';
  position: absolute;
  left: 50%;
  bottom: 0;
  transform: translateX(-50%);
  width: 60px;
  height: 3px;
  background-color: #007bff;
  border-radius: 2px;
}

.text-center.mb-4 {
  margin-bottom: 30px !important;
}

.spinner-border {
  width: 3rem;
  height: 3rem;
  color: #007bff;
}

.text-center.text-muted {
  padding: 50px 20px;
  background-color: #e9ecef;
  border-radius: 8px;
  font-size: 1.1rem;
  color: #6c757d !important;
}

.card {
  border: none;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.12);
}

.card-body {
  padding: 25px;
}

.card-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #495057;
  margin-bottom: 10px;
}

.card-subtitle {
  font-size: 0.9rem;
  color: #868e96;
  margin-bottom: 15px;
}

.card-text {
  font-size: 1rem;
  color: #343a40;
  line-height: 1.6;
  margin-bottom: 15px;
}

.card-text router-link {
  color: #007bff;
  text-decoration: none;
  font-weight: 500;
}

.card-text router-link:hover {
  text-decoration: underline;
}

.card.bg-light {
  background-color: #f1f3f5 !important;
  border: 1px solid #e9ecef;
  border-radius: 8px;
}

.card.bg-light .card-body {
  padding: 15px;
}

.card.bg-light p {
  font-size: 0.95rem;
  color: #495057;
}

.card.bg-light small {
  font-size: 0.85rem;
  color: #868e96;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .container {
    margin: 20px auto;
    padding: 15px;
  }

  h2 {
    font-size: 1.8rem;
    margin-bottom: 20px;
  }

  .card-body {
    padding: 20px;
  }

  .card-title {
    font-size: 1.1rem;
  }

  .card-text {
    font-size: 0.95rem;
  }
}

@media (max-width: 576px) {
  h2 {
    font-size: 1.5rem;
  }

  .card-body {
    padding: 15px;
  }

  .card-title {
    font-size: 1rem;
  }

  .card-text {
    font-size: 0.9rem;
  }
}
</style>
