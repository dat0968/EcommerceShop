<template>
  <div class="card-map">
    <div class="card-header">
      <h3>{{ title }}</h3>
    </div>
    <div class="card-body">
      <iframe
        :src="mapSrc"
        width="100%"
        height="250"
        style="border: 0"
        allowfullscreen=""
        loading="lazy"
        referrerpolicy="no-referrer-when-downgrade"
      ></iframe>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CardMap',
  props: {
    title: {
      type: String,
      default: 'Bản đồ',
    },
    mapSrc: {
      type: String,
      default:
        'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3892.0437716591928!2d108.07307871059164!3d12.710566120495768!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3171f7b6e379b675%3A0x72662967145555c0!2zVHLGsOG7nW5nIENhbyDEkeG6s25nIEZQVCBQb2x5dGVjaG5pYw!5e0!3m2!1svi!2s!4v1748163761852!5m2!1svi!2s',
    },
  },
}
</script>

<style scoped>
.card-map {
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  max-width: 400px;
  margin: 0 auto;
}
.card-header {
  padding: 16px;
  border-bottom: 1px solid #f0f0f0;
  background: #fafafa;
}
.card-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}
.card-body {
  padding: 0;
}
</style>
