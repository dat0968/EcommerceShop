<template>
  <div class="review-product-combo">
    <!-- Main Comment Input -->
    <div class="comment-input-area">
      <div class="row">
        <h5 class="mb-3">Để lại bình luận</h5>
      </div>
      <div v-if="isAuthenticated">
        <textarea class="form-control mb-2" rows="3" placeholder="Viết bình luận của bạn..." v-model="newCommentContent"
          :disabled="isSubmitting"></textarea>
        <div class="row">
          <button class="btn btn-primary col-3" @click="addComment" :disabled="isSubmitting">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            <span v-if="isSubmitting"> Đang gửi...</span>
            <span v-else>Gửi bình luận</span>
          </button>
        </div>
      </div>
      <div v-else>
        <textarea class="form-control mb-2" rows="3" placeholder="Bạn phải đăng nhập để bình luận" disabled></textarea>
        <p class="mt-2">
          Vui lòng <router-link to="/login">đăng nhập</router-link> để gửi bình luận.
        </p>
      </div>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loading" class="loading-state">
      <div v-for="n in 3" :key="n" class="review-item skeleton">
        <div class="review-header">
          <div class="reviewer-avatar skeleton-box"></div>
          <div class="reviewer-info">
            <div class="skeleton-box skeleton-line-sm"></div>
            <div class="skeleton-box skeleton-line-xs"></div>
          </div>
        </div>
        <div class="review-content">
          <div class="skeleton-box skeleton-line-md"></div>
          <div class="skeleton-box skeleton-line-lg"></div>
        </div>
      </div>
    </div>

    <!-- Comments List -->
    <div v-else>
      <div v-if="paginatedComments.length" class="reviews-list">
        <ul class="list-group list-group-flush">
          <li v-for="comment in paginatedComments" :key="comment.id" class="list-group-item review-item">
            <!-- Main Comment -->
            <div class="review-header">
              <img :src="comment.avatar || '/path/to/default-avatar.png'" alt="avatar" class="reviewer-avatar" />
              <div class="reviewer-info">
                <strong class="reviewer-name">{{ comment.hoTen || 'Ẩn danh' }}</strong>
                <span class="review-date">{{ formatDate(comment.ngayBinhLuan) }}</span>
              </div>
            </div>
            <div class="review-content">
              <p>{{ comment.noiDung }}</p>
            </div>

            <!-- Reply Button -->
            <button v-if="isAuthenticated && replyingToCommentId !== comment.id" class="btn btn-sm btn-outline-primary reply-btn"
              @click="startReply(comment.id)">
              <i class="fa fa-reply"></i> Trả lời
            </button>

            <!-- Reply Input Form -->
            <div v-if="replyingToCommentId === comment.id" class="reply-input-area mt-2">
              <textarea class="form-control" rows="2" placeholder="Viết câu trả lời của bạn..." v-model="replyContent"
                :disabled="isSubmittingReply"></textarea>
              <div class="mt-2">
                <button class="btn btn-sm btn-primary" @click="submitReply(comment.id)" :disabled="isSubmittingReply">
                  <span v-if="isSubmittingReply" class="spinner-border spinner-border-sm" role="status"
                    aria-hidden="true"></span>
                  <span v-if="isSubmittingReply"> Đang gửi...</span>
                  <span v-else>Gửi</span>
                </button>
                <button class="btn btn-sm btn-secondary ms-2" @click="cancelReply"
                  :disabled="isSubmittingReply">Hủy</button>
              </div>
            </div>

            <!-- Replies Section -->
            <div v-if="comment.replies && comment.replies.length > 0" class="replies-section">
              <div v-for="reply in comment.replies" :key="reply.id" class="shop-reply">
                <div class="review-header">
                  <img :src="reply.avatar || '/path/to/default-avatar.png'" alt="avatar" class="reviewer-avatar"
                    style="width: 35px; height: 35px;" />
                  <div class="reviewer-info">
                    <strong class="reviewer-name">{{ reply.hoTen || 'Nhân viên' }}</strong>
                    <span class="review-date">{{ formatDate(reply.ngayBinhLuan) }}</span>
                  </div>
                </div>
                <div class="review-content">
                  <p>{{ reply.noiDung }}</p>
                </div>
              </div>
            </div>
          </li>
        </ul>
        <!-- Pagination -->
        <nav v-if="totalPages > 1" aria-label="Page navigation" class="mt-4 d-flex justify-content-center">
          <ul class="pagination">
            <li class="page-item" :class="{ disabled: currentPage === 1 }">
              <a class="page-link" href="#" @click.prevent="changePage(currentPage - 1)">&laquo;</a>
            </li>
            <li v-for="page in totalPages" :key="page" class="page-item" :class="{ active: currentPage === page }">
              <a class="page-link" href="#" @click.prevent="changePage(page)">{{ page }}</a>
            </li>
            <li class="page-item" :class="{ disabled: currentPage === totalPages }">
              <a class="page-link" href="#" @click.prevent="changePage(currentPage + 1)">&raquo;</a>
            </li>
          </ul>
        </nav>
      </div>
      <!-- Empty State -->
      <div v-else class="empty-state">
        <EmptySuggestBox iconSub="fa fa-comments" contentText="Chưa có bình luận nào"
          suggestContent="Hãy là người đầu tiên để lại bình luận cho sản phẩm này!" />
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, watch, onMounted } from 'vue';
import { commentService } from '@/services/commentService';
import Swal from 'sweetalert2';
import EmptySuggestBox from '@/components/common/EmptySuggestBox.vue';
import Cookies from 'js-cookie';

export default {
  name: 'CommentSection',
  components: { EmptySuggestBox },
  props: {
    objectId: {
      type: [String, Number],
      required: true,
    },
    objectType: {
      type: String,
      required: true,
      validator: (value) => ['product', 'combo'].includes(value),
    },
  },
  setup(props) {
    const comments = ref([]);
    const loading = ref(false);
    const isSubmitting = ref(false);
    const isSubmittingReply = ref(false);
    const newCommentContent = ref('');

    const replyingToCommentId = ref(null);
    const replyContent = ref('');

    const currentPage = ref(1);
    const itemsPerPage = ref(5);

    const isAuthenticated = computed(() => !!Cookies.get('accessToken'));

    // --- Lifecycle Hooks ---
    onMounted(() => {
      if (props.objectId && props.objectType) {
        fetchComments();
      }
    });

    // --- Data Fetching ---
    const fetchComments = async () => {
      if (!props.objectId || !props.objectType) return;
      loading.value = true;
      try {
        const response = await commentService.getCommentsByObjectIdAndType(props.objectId, props.objectType);
        if (response.success) {
          const sortComments = (commentList) => {
            commentList.sort((a, b) => new Date(b.ngayBinhLuan) - new Date(a.ngayBinhLuan));
            commentList.forEach(comment => {
              if (comment.replies && comment.replies.length > 0) {
                sortComments(comment.replies);
              }
            });
          };
          sortComments(response.data);
          comments.value = response.data;
        } else {
          Swal.fire('Lỗi', 'Không thể tải được danh sách bình luận.', 'error');
        }
      } catch (error) {
        Swal.fire('Lỗi', 'Đã xảy ra lỗi khi tải bình luận.', 'error');
      } finally {
        loading.value = false;
      }
    };

    // --- Actions ---
    const addComment = async () => {
      if (!newCommentContent.value.trim()) {
        Swal.fire({ icon: 'warning', title: 'Vui lòng nhập nội dung bình luận', toast: true, position: 'top-end', showConfirmButton: false, timer: 3000 });
        return;
      }
      isSubmitting.value = true;

      const commentData = {
        noiDung: newCommentContent.value.trim(),
        parentId: null,
        [props.objectType === 'product' ? 'maSP' : 'maCombo']: props.objectId,
      };

      try {
        const response = await commentService.addComment(commentData);
        if (!response.success) {
          const errorData = await response.json();
          throw new Error(errorData.message || 'Lỗi không xác định từ máy chủ.');
        }
        if (response.success) {
          newCommentContent.value = '';
          Swal.fire({ icon: 'success', title: 'Bình luận đã được gửi', toast: true, position: 'top-end', showConfirmButton: false, timer: 3000 });
          fetchComments(); // Refresh comments after adding
        } else {
          Swal.fire('Lỗi', response.message || 'Không thể gửi bình luận.', 'error');
        }
      } catch (error) {
        Swal.fire('Lỗi', error.message || 'Đã xảy ra lỗi khi gửi bình luận.', 'error');
      } finally {
        isSubmitting.value = false;
      }
    };

    const submitReply = async (parentId) => {
      if (!replyContent.value.trim()) {
        Swal.fire({ icon: 'warning', title: 'Vui lòng nhập nội dung trả lời', toast: true, position: 'top-end', showConfirmButton: false, timer: 3000 });
        return;
      }
      isSubmittingReply.value = true;

      const replyData = {
        noiDung: replyContent.value.trim(),
        parentId: parentId,
        [props.objectType === 'product' ? 'maSP' : 'maCombo']: props.objectId,
      };

      try {
        const response = await commentService.addComment(replyData);
        if (response.success) {
          cancelReply();
          Swal.fire({ icon: 'success', title: 'Đã gửi câu trả lời', toast: true, position: 'top-end', showConfirmButton: false, timer: 3000 });
          fetchComments(); // Refresh comments after adding reply
        } else {
          Swal.fire('Lỗi', response.message || 'Không thể gửi câu trả lời.', 'error');
        }
      } catch (error) {
        Swal.fire('Lỗi', 'Đã xảy ra lỗi khi gửi câu trả lời.', 'error');
      } finally {
        isSubmittingReply.value = false;
      }
    };

    // --- Helper Functions for State Management ---

    const startReply = (commentId) => {
      replyingToCommentId.value = commentId;
      replyContent.value = '';
    };

    const cancelReply = () => {
      replyingToCommentId.value = null;
      replyContent.value = '';
    };

    const totalPages = computed(() => Math.ceil(comments.value.length / itemsPerPage.value));
    const paginatedComments = computed(() => {
      const start = (currentPage.value - 1) * itemsPerPage.value;
      return comments.value.slice(start, start + itemsPerPage.value);
    });
    const changePage = (page) => {
      if (page >= 1 && page <= totalPages.value) currentPage.value = page;
    };
    const formatDate = (dateString) => {
      if (!dateString) return '';
      return new Date(dateString).toLocaleDateString('vi-VN', { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    };

    watch(() => [props.objectId, props.objectType], ([newObjectId, newObjectType], oldValues) => {
      const [oldObjectId, oldObjectType] = oldValues || [undefined, undefined];
      if (newObjectId && newObjectType) {
        fetchComments();
      }
    }, { immediate: true });

    return {
      comments, loading, isSubmitting, isSubmittingReply, newCommentContent,
      replyingToCommentId, replyContent, paginatedComments, currentPage, totalPages,
      addComment, startReply, cancelReply, submitReply, formatDate, changePage,
      isAuthenticated,
    };
  },
};
</script>

<style scoped>
/* ... (previous styles) ... */
.review-product-combo {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  padding: 1rem;
  background-color: #f9f9f9;
}

.comment-input-area {
  background-color: #fff;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  margin-bottom: 1.5rem;
}

.reply-input-area {
  background-color: #f8f9fa;
  padding: 1rem;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: #6c757d;
}

.reviews-list {
  margin-top: 1rem;
}

.review-item {
  background-color: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  margin-bottom: 1rem;
  padding: 1.5rem;
  transition: box-shadow 0.3s ease;
}

.review-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.review-header {
  display: flex;
  align-items: center;
  margin-bottom: 0.75rem;
}

.reviewer-avatar {
  width: 45px;
  height: 45px;
  border-radius: 50%;
  object-fit: cover;
  margin-right: 12px;
  border: 2px solid #eee;
}

.reviewer-info {
  display: flex;
  flex-direction: column;
}

.reviewer-name {
  font-weight: 600;
  color: #212529;
}

.review-date {
  font-size: 0.85em;
  color: #6c757d;
}

.review-content {
  margin-bottom: 0.5rem;
  color: #343a40;
  line-height: 1.6;
}

.replies-section {
  margin-top: 1rem;
  padding-left: 1rem;
  border-left: 3px solid #f0f0f0;
}

.shop-reply {
  background-color: #f8f9fa;
  border-left: 4px solid #0d6efd;
  padding: 1rem;
  margin-top: 1rem;
  border-radius: 0 8px 8px 0;
}

.shop-reply p {
  margin-bottom: 0;
}

.pagination .page-link {
  color: #0d6efd;
}

.pagination .page-item.active .page-link {
  background-color: #0d6efd;
  border-color: #0d6efd;
  color: #fff;
}

.pagination .page-item.disabled .page-link {
  color: #6c757d;
}

/* Skeleton Loader Styles */
.skeleton {
  opacity: 0.7;
  animation: skeleton-loading 1s linear infinite alternate;
}

.skeleton-box {
  background-color: #e2e8f0;
  border-radius: 4px;
}

.skeleton-line-sm {
  width: 100px;
  height: 1em;
}

.skeleton-line-xs {
  width: 80px;
  height: 0.8em;
  margin-top: 4px;
}

.skeleton-line-md {
  width: 90%;
  height: 1em;
  margin-bottom: 0.5em;
}

.skeleton-line-lg {
  width: 70%;
  height: 1em;
}

.reviewer-avatar.skeleton-box {
  width: 45px;
  height: 45px;
  border-radius: 50%;
}

@keyframes skeleton-loading {
  0% {
    background-color: hsl(210, 20%, 85%);
  }

  100% {
    background-color: hsl(210, 20%, 95%);
  }
}
</style>