import { createApp } from 'vue'
import { createPinia } from 'pinia'

import replaceBrokenImages from '@/utils/autoReplaceImages'
import { initApiBaseUrl } from '@/utils/axiosClient'

import App from './App.vue'
import router from './router'
const app = createApp(App)

await initApiBaseUrl()

app.use(createPinia())
app.use(router)

app.mount('#app')

replaceBrokenImages()
