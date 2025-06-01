<!--
Cách dùng 1: Overlay che phủ một phần (mặc định)
<div class="position-relavtive">
    <Overlay :is-visible="true" overlay-content="Không có dữ liệu để hiển thị." />
</div>

Cách dùng 2: Overlay che phủ toàn trang
<Overlay :is-visible="true" :is-cover-page="true" overlay-content="Đang tải dữ liệu..." />
-->
<template>
  <div :class="['overlay', overlayClass]" v-if="isVisible" style="z-index: 1">
    <div class="content" v-if="!imageSrc && !overlayContent">
      <p>Không có nội dung hiển thị.</p>
    </div>
    <div class="content" v-if="overlayContent">
      <h5>{{ overlayContent }}</h5>
    </div>
    <img v-if="imageSrc" :src="imageSrc" alt="Overlay image" class="overlay-image" />
  </div>
</template>

<script>
export default {
  name: 'OverlayComponent',
  props: {
    isVisible: {
      type: Boolean,
      default: false,
    },
    imageSrc: {
      type: String,
      default: '',
    },
    overlayContent: {
      type: String,
      default: '',
    },
    isCoverPage: {
      type: Boolean,
      default: false, // Mặc định là không bao phủ toàn trang
    },
  },
  computed: {
    overlayClass() {
      return this.isCoverPage ? 'overlay-full' : 'overlay-partial'
    },
  },
}
</script>

<style scoped>
.overlay {
  position: absolute;
  background: radial-gradient(circle, rgba(0, 0, 0, 0.1), rgba(0, 0, 0, 0)); /*Làm mờ viền ngoài*/
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}
.overlay-full {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}
.overlay-partial {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}
.overlay-image {
  max-width: 90%;
  max-height: 90%;
}
.content {
  color: white;
  text-align: center;
}
</style>
