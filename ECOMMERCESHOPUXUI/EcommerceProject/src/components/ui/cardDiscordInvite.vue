<template>
  <div class="card-discord-invite">
    <div class="card-header">
      <h3>Bạn được mời tham gia vào Server!</h3>
    </div>
    <div class="card-body">
      <p>Nhấn vào để được điều hướng đến lời mời của bạn.</p>
      <a href="https://discord.gg/NHhJtRuB" target="_blank" rel="noopener" class="invite-link"
        >Vào ngay!</a
      >
    </div>
  </div>
</template>

<script>
export default {
  name: 'CardDiscordInvite',
}
</script>

<style scoped>
.card-discord-invite {
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
  text-align: center;
}
.card-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}
.card-body {
  padding: 20px;
  text-align: center;
}
.card-body p {
  margin-bottom: 15px;
  font-size: 14px;
  color: #555;
}
.invite-link {
  display: inline-block;
  padding: 10px 20px;
  background-color: #5865f2;
  color: white;
  text-decoration: none;
  border-radius: 5px;
  font-weight: 500;
  transition: background 0.2s;
}
.invite-link:hover {
  background-color: #404eed;
}
</style>
