import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import '@/lib/chartSetup'
import router from '@/router'
import { setUnauthorizedHandler } from '@/services/http'
import { useAuthStore } from '@/stores/auth'

import '@/assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

const authStore = useAuthStore()
setUnauthorizedHandler(() => {
  authStore.logout()
  router.push({ name: 'login' })
})
void authStore.fetchCurrentUser()

app.mount('#app')
