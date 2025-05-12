<script setup>
  import Header from '../../components/Header.vue'
  import Footer  from '../../components/Footer.vue'
  import 'bootstrap/dist/css/bootstrap.min.css'
  import 'bootstrap'
  import '@fortawesome/fontawesome-free/css/all.min.css'
  import 'jquery-ui-dist/jquery-ui.css'
  import 'magnific-popup/dist/magnific-popup.css'
  import 'owl.carousel/dist/assets/owl.carousel.min.css'
  import 'slicknav/dist/slicknav.min.css'
  import '../../assets/Customer/css/elegant-icons.css'
  import '../../assets/Customer/css/style.css'
  //import 'jquery'; 
  import '../../assets/Customer/js/jquery-3.3.1.min.js'
  import 'jquery-ui-dist/jquery-ui'
  import 'magnific-popup'
  //import mixitup from 'mixitup'
  import '../../assets/Customer/js/mixitup.min.js'
  import 'jquery-countdown'
  //import 'slicknav'
  import '../../assets/Customer/js/jquery.slicknav'
  import 'owl.carousel'
  import 'jquery.nicescroll'
  import '../../assets/Customer/js/main.js'
</script>
<template>
  <div>
    <Header />
    <router-view />
    <Footer/>
  </div>
</template>


<style>
</style>