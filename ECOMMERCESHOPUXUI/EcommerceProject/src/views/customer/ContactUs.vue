<template>
  <div>
    <!-- Breadcrumb Begin -->
    <div class="breadcrumb-option">
      <div class="container">
        <div class="row">
          <div class="col-lg-12">
            <div class="breadcrumb__links">
              <a href="/"><i class="fa fa-home"></i> Trang chủ</a>
              <span>Đánh giá</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- Breadcrumb End -->

    <section class="shop spad">
      <div class="container overflow-auto" style="min-height: 50vh">
        <h3 class="mb-3">Liên hệ với cửa hàng</h3>
        <form @submit.prevent="submitContact">
          <div class="mb-2">
            <label class="form-label">Họ tên</label>
            <input v-model="form.name" class="form-control" required />
          </div>
          <div class="mb-2">
            <label class="form-label">Email</label>
            <input v-model="form.email" type="email" class="form-control" required />
          </div>
          <div class="mb-2">
            <label class="form-label">Số điện thoại</label>
            <input v-model="form.phone" class="form-control" required />
          </div>
          <div class="mb-2">
            <label class="form-label">Nội dung</label>
            <textarea v-model="form.message" class="form-control" rows="4" required></textarea>
          </div>
          <button class="btn btn-primary" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm"></span>
            Gửi liên hệ
          </button>
        </form>
      </div>
    </section>
  </div>
</template>

<script>
import Swal from 'sweetalert2'
import { postToApi } from '@/utils/axiosClient'

export default {
  name: 'ContactUs',
  data() {
    return {
      form: {
        name: '',
        email: '',
        phone: '',
        message: '',
      },
      loading: false,
    }
  },
  methods: {
    async submitContact() {
      this.loading = true
      try {
        const res = await postToApi('/Mail/ContactUs', this.form)
        if (res && res.success) {
          Swal.fire({
            icon: 'success',
            title: 'Đã gửi liên hệ!',
            text: 'Cảm ơn bạn đã liên hệ với chúng tôi.',
            timer: 1800,
            showConfirmButton: false,
          })
          this.form = { name: '', email: '', phone: '', message: '' }
        } else {
          Swal.fire({
            icon: 'error',
            title: 'Gửi thất bại',
            text: res?.message || 'Có lỗi xảy ra, vui lòng thử lại.',
          })
        }
      } catch (e) {
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: e.message || 'Có lỗi xảy ra, vui lòng thử lại.',
        })
      } finally {
        this.loading = false
      }
    },
  },
}
</script>
