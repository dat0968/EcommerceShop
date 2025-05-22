<template>
  <div class="invite-card">
    <h2>Join Our Discord Server!</h2>
    <p>Click the button below to join our vibrant and active community.</p>
    <a href="https://discord.gg/NHhJtRuB" target="_blank">Join Now!</a>
  </div>
</template>

<script>
export default {
  name: 'CardDiscordInvite',
  components: {},
  props: {},
  data() {
    return {}
  },
  computed: {},
  watch: {},
  mounted() {},
  methods: {},
}
</script>

<style scoped>
/* Add your CSS styles here */
.invite-card {
  /* Style the card container */
  width: 100%;
  padding: 20px;
  background-color: #f0f0f0;
  border-radius: 10px;
  text-align: center;
}
.invite-card h2 {
  /* Style the card title */
  margin-bottom: 10px;
  color: #333;
}
.invite-card p {
  /* Style the card description */
  margin-bottom: 15px;
  font-size: 14px;
  color: #555;
}
.invite-card a {
  /* Style the invite link button */
  display: inline-block;
  padding: 10px 20px;
  background-color: #007bff;
  color: white;
  text-decoration: none;
  border-radius: 5px;
}
.invite-card a:hover {
  /* Add a hover effect */
  background-color: #0056b3;
}
</style>
