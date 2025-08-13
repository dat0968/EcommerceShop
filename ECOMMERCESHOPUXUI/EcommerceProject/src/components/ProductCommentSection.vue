<template>
  <div class="product-comment-section">
    <h4>Bình luận</h4>
    <div v-if="isLoading" class="text-center my-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    <div v-else>
      <div class="comment-form mb-4">
        <textarea v-model="newCommentText" class="form-control" rows="3" placeholder="Viết bình luận của bạn..."></textarea>
        <button @click="addComment" class="btn btn-primary mt-2">Gửi bình luận</button>
      </div>
      <div class="comment-list">
        <div v-for="comment in comments" :key="comment.Id" class="comment-item">
          <div class="comment-author">
            <img :src="comment.Avatar || '/img/default-avatar.png'" alt="Avatar" class="avatar">
            <strong>{{ comment.HoTen }}</strong>
          </div>
          <div class="comment-content">
            <p>{{ comment.NoiDung }}</p>
            <div class="comment-actions">
              <small>{{ formatDate(comment.NgayBinhLuan) }}</small>
              <a href="#" @click.prevent="startReply(comment)">Trả lời</a>
              <a href="#" v-if="canEditOrDelete(comment)" @click.prevent="toggleEditMode(comment)">Sửa</a>
              <a href="#" v-if="canEditOrDelete(comment)" @click.prevent="deleteComment(comment.Id)">Xóa</a>
            </div>
            <div v-if="comment.isReplying" class="reply-form mt-2">
              <textarea v-model="comment.replyText" class="form-control" rows="2" placeholder="Viết câu trả lời..."></textarea>
              <button @click="submitReply(comment)" class="btn btn-sm btn-primary mt-1">Gửi</button>
              <button @click="cancelReply(comment)" class="btn btn-sm btn-secondary mt-1">Hủy</button>
            </div>
            <div v-if="comment.isEditing" class="edit-form mt-2">
              <textarea v-model="comment.editText" class="form-control" rows="2"></textarea>
              <button @click="updateComment(comment)" class="btn btn-sm btn-primary mt-1">Lưu</button>
              <button @click="cancelEdit(comment)" class="btn btn-sm btn-secondary mt-1">Hủy</button>
            </div>
            <div v-if="comment.Replies && comment.Replies.length > 0" class="replies">
              <div v-for="reply in comment.Replies" :key="reply.Id" class="comment-item reply-item">
                 <div class="comment-author">
                    <img :src="reply.Avatar || '/img/default-avatar.png'" alt="Avatar" class="avatar">
                    <strong>{{ reply.HoTen }}</strong>
                  </div>
                  <div class="comment-content">
                    <p>{{ reply.NoiDung }}</p>
                    <div class="comment-actions">
                      <small>{{ formatDate(reply.NgayBinhLuan) }}</small>
                      <a href="#" v-if="canEditOrDelete(reply)" @click.prevent="toggleEditMode(reply)">Sửa</a>
                      <a href="#" v-if="canEditOrDelete(reply)" @click.prevent="deleteComment(reply.Id)">Xóa</a>
                    </div>
                    <div v-if="reply.isEditing" class="edit-form mt-2">
                      <textarea v-model="reply.editText" class="form-control" rows="2"></textarea>
                      <button @click="updateComment(reply)" class="btn btn-sm btn-primary mt-1">Lưu</button>
                      <button @click="cancelEdit(reply)" class="btn btn-sm btn-secondary mt-1">Hủy</button>
                    </div>
                  </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { commentService } from '@/services/commentService';
import { useAuthStore } from '@/stores/auth';
import Swal from 'sweetalert2';

export default {
  name: 'ProductCommentSection',
  props: {
    productId: {
      type: Number,
      required: true,
    },
  },
  data() {
    return {
      comments: [],
      newCommentText: '',
      isLoading: true,
      authStore: useAuthStore(),
    };
  },
  created() {
    this.fetchComments();
  },
  methods: {
    formatDate(dateStr) {
      if (!dateStr) return '';
      return new Date(dateStr).toLocaleString('vi-VN');
    },
    async fetchComments() {
      this.isLoading = true;
      try {
        const response = await commentService.getCommentsByProductId(this.productId);
        if (response.success) {
          this.comments = response.data.map(comment => ({ ...comment, isReplying: false, replyText: '', isEditing: false, editText: comment.NoiDung }));
        } else {
          Swal.fire('Lỗi', response.message, 'error');
        }
      } catch (error) {
        Swal.fire('Lỗi', 'Không thể tải bình luận.', 'error');
      } finally {
        this.isLoading = false;
      }
    },
    canEditOrDelete(comment) {
        return this.authStore.user?.id === comment.MaKh || this.authStore.user?.role === 'Admin';
    },
    async addComment() {
        if(!this.authStore.isLoggedIn) {
            Swal.fire('Lỗi', 'Bạn cần đăng nhập để bình luận.', 'error');
            return;
        }
        if (!this.newCommentText.trim()) return;

        const commentData = {
            MaSP: this.productId,
            NoiDung: this.newCommentText,
            ParentId: null,
            MaKh: this.authStore.user.id
        };

        const response = await commentService.addComment(commentData);
        if(response.success) {
            this.newCommentText = '';
            this.fetchComments();
        } else {
            Swal.fire('Lỗi', response.message, 'error');
        }
    },
    startReply(comment) {
        comment.isReplying = true;
    },
    cancelReply(comment) {
        comment.isReplying = false;
        comment.replyText = '';
    },
    async submitReply(comment) {
        if(!this.authStore.isLoggedIn) {
            Swal.fire('Lỗi', 'Bạn cần đăng nhập để bình luận.', 'error');
            return;
        }
        if (!comment.replyText.trim()) return;

        const replyData = {
            MaSP: this.productId,
            NoiDung: comment.replyText,
            ParentId: comment.Id,
            MaKh: this.authStore.user.id
        };

        const response = await commentService.addComment(replyData);
        if(response.success) {
            this.fetchComments();
        } else {
            Swal.fire('Lỗi', response.message, 'error');
        }
    },
    toggleEditMode(comment) {
        comment.isEditing = !comment.isEditing;
        if(comment.isEditing) {
            comment.editText = comment.NoiDung;
        }
    },
    cancelEdit(comment) {
        comment.isEditing = false;
    },
    async updateComment(comment) {
        if (!comment.editText.trim()) return;

        const commentData = {
            NoiDung: comment.editText,
            MaSP: comment.MaSP,
            ParentId: comment.ParentId,
            MaKh: comment.MaKh
        };

        const response = await commentService.updateComment(comment.Id, commentData);
        if(response.success) {
            this.fetchComments();
        } else {
            Swal.fire('Lỗi', response.message, 'error');
        }
    },
    async deleteComment(commentId) {
        Swal.fire({
            title: 'Bạn chắc chứ?',
            text: "Bạn sẽ không thể hoàn tác hành động này!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Vâng, xóa nó đi!',
            cancelButtonText: 'Hủy'
        }).then(async (result) => {
            if (result.isConfirmed) {
                const response = await commentService.deleteComment(commentId);
                if(response.success) {
                    Swal.fire(
                        'Đã xóa!',
                        'Bình luận của bạn đã được xóa.',
                        'success'
                    )
                    this.fetchComments();
                } else {
                    Swal.fire('Lỗi', response.message, 'error');
                }
            }
        })
    }
  },
};
</script>

<style scoped>
.product-comment-section {
  margin-top: 30px;
}
.comment-list {
  margin-top: 20px;
}
.comment-item {
  display: flex;
  margin-bottom: 20px;
}
.comment-author {
  margin-right: 15px;
}
.avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
}
.comment-content {
  flex-grow: 1;
}
.comment-actions {
  font-size: 0.8em;
  color: #888;
}
.comment-actions a {
  margin-left: 10px;
  color: #007bff;
  text-decoration: none;
}
.replies {
  margin-top: 15px;
  padding-left: 65px;
}
.reply-item {
    margin-bottom: 15px;
}
</style>