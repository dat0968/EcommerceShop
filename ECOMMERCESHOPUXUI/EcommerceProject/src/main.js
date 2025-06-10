import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPersist from 'pinia-plugin-persistedstate'
import replaceBrokenImages from '@/utils/autoReplaceImages'
import { initApiBaseUrl } from '@/utils/axiosClient'

import App from './App.vue'
import router from './router'
const app = createApp(App)

await initApiBaseUrl()

const pinia = createPinia()
pinia.use(piniaPersist)

app.use(pinia)
app.use(router)

app.mount('#app')

replaceBrokenImages()
