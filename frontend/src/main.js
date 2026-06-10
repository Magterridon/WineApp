import { createApp } from 'vue'
import { createPinia } from 'pinia'
import 'bootstrap/dist/css/bootstrap.min.css'
import App from './App.vue'
import router from './router/index'
import { initMockData } from './services/mockData'

initMockData()

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
