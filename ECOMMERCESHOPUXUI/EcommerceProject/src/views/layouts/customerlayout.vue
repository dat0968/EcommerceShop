<script setup>
import HeaderComponent from '../../components/Header.vue'
import FooterComponent from '../../components/Footer.vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap'
import '@fortawesome/fontawesome-free/css/all.min.css'
import 'jquery-ui-dist/jquery-ui.css'
import 'magnific-popup/dist/magnific-popup.css'
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
import chatbot from '@/components/chatbot/chatbot.vue'
import CompareProduct from '@/components/specicals/CompareProduct.vue'
// import 'owl.carousel/dist/assets/owl.carousel.min.css'

import { useModalVisibility } from '@/composables/useModalVisibility'
const { isModalVisible } = useModalVisibility({
  exclude: ["Profile", "Login"]
})
</script>
<template>
  <div style="background-color:#fffcfa">
    <HeaderComponent />
    <router-view :key="$route.fullPath" />
    <chatbot></chatbot>
    <compare-product v-if="isModalVisible"/>
    <FooterComponent />
  </div>
</template>

<style></style>
