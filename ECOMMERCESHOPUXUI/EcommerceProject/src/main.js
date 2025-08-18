import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPersist from 'pinia-plugin-persistedstate'
import replaceBrokenImages from '@/utils/autoReplaceImages'
import './plugins/owl.js'
import App from './App.vue'
import router from './router'
const app = createApp(App)

const pinia = createPinia()
pinia.use(piniaPersist)

app.use(pinia)
app.use(router)
app.mount('#app')

async function initializeApp() {
  replaceBrokenImages();
}

initializeApp();

