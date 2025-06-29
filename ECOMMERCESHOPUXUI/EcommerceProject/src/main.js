import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPersist from 'pinia-plugin-persistedstate'
import replaceBrokenImages from '@/utils/autoReplaceImages'
import { initApiBaseUrl } from '@/utils/axiosClient'
// import Toast from 'vue-toastification';
// import 'vue-toastification/dist/index.css'
// Cực kỳ quan trọng: import jQuery và gán vào window
import $ from 'jquery'
import './plugins/owl.js'
window.$ = window.jQuery = jQuery
// Import owl.carousel sau khi gán jQuery
import 'owl.carousel'
import App from './App.vue'
import router from './router'
const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPersist)

app.use(pinia)
app.use(router)
// app.use(Toast, {
//   position: 'top-right',
//   timeout: 5000,
//   closeOnClick: true,
//   pauseOnHover: true,
//   draggable: true,
//   draggablePercent: 0.6,
//   showCloseButtonOnHover: false,
//   hideProgressBar: false,
//   closeButton: 'button',
//   icon: true,
//   rtl: true,
// });
app.mount('#app')

await initApiBaseUrl()
replaceBrokenImages()

