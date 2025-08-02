<template>
  <div
    v-if="show"
    class="modal fade show d-block"
    tabindex="-1"
    style="background: rgba(0, 0, 0, 0.45)"
    @click.self="closeModal"
  >
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content p-4 position-relative">
        <div class="modal-header text-center border-0 pb-0">
          <h5 class="modal-title w-100 fs-4">Lịch Chuỗi Đăng Nhập</h5>
          <button
            type="button"
            class="btn-close position-absolute end-0 top-0 m-3"
            @click="closeModal"
            aria-label="Close"
          ></button>
        </div>
        <div class="modal-body d-flex flex-column align-items-center">
          <p class="fs-5">Chuỗi đăng nhập hiện tại của bạn: <span class="fw-bold text-primary">{{ streakData.streak }} ngày</span></p>
          <div class="calendar-grid">
            <div
              v-for="(day, index) in calendarDays"
              :key="index"
              :class="['calendar-day', { 'is-active': day.isActive, 'is-today': day.isToday }]"
            >
              {{ day.date.getDate() }}
            </div>
          </div>
          <p v-if="streakData.isNewStreak" class="mt-3 text-success fw-bold">Bạn đã bắt đầu một chuỗi đăng nhập mới!</p>
          <p v-else-if="streakData.streak > 0" class="mt-3 text-info fw-bold">Tiếp tục duy trì chuỗi đăng nhập để nhận thêm lượt quay!</p>
          <p v-else class="mt-3 text-danger fw-bold">Chuỗi đăng nhập của bạn đã bị ngắt. Đã bắt đầu lại!</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { watch, computed } from 'vue'

const props = defineProps({
  show: Boolean,
  streakData: {
    type: Object,
    default: () => ({ streak: 0, lastLogin: null, isNewStreak: false }),
  },
})

const emit = defineEmits(['close'])

const closeModal = () => {
  emit('close')
}

const calendarDays = computed(() => {
  const days = []
  const today = new Date()
  today.setHours(0, 0, 0, 0)

  const lastLoginDate = props.streakData?.lastLogin ? new Date(props.streakData.lastLogin) : null
  // Ensure lastLoginDate is a valid Date object
  const isValidLastLoginDate = lastLoginDate && !isNaN(lastLoginDate.getTime());
  if (isValidLastLoginDate) lastLoginDate.setHours(0, 0, 0, 0)

  // Generate days for the past week + today
  for (let i = 6; i >= 0; i--) {
    const date = new Date(today)
    date.setDate(today.getDate() - i)
    const isActive = isValidLastLoginDate && date <= lastLoginDate && date > new Date(lastLoginDate.getTime() - props.streakData.streak * 24 * 60 * 60 * 1000)
    const isToday = date.getTime() === today.getTime()
    days.push({ date, isActive, isToday })
  }
  return days
})

watch(() => props.show, (newVal) => {
  if (newVal) {
    // Optional: Trigger some animation here when modal opens
    console.log('Streak modal opened with data:', props.streakData)
  }
})
</script>

<style scoped>
.modal-dialog {
  max-width: 500px;
}
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 10px;
  width: 100%;
  max-width: 400px;
  margin-top: 20px;
}
.calendar-day {
  width: 40px;
  height: 40px;
  display: flex;
  justify-content: center;
  align-items: center;
  border: 1px solid #eee;
  border-radius: 5px;
  font-weight: bold;
  color: #555;
  background-color: #f9f9f9;
}
.calendar-day.is-active {
  background-color: #d4edda; /* Light green for active streak days */
  border-color: #28a745;
  color: #28a745;
}
.calendar-day.is-today {
  background-color: #007bff;
  color: white;
  border-color: #007bff;
}
</style>
