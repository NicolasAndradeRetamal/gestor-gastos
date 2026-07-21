import { createPinia } from 'pinia'
import { createApp } from 'vue'

import App from '@/App.vue'
import '@/lib/chartSetup'
import router from '@/router'
import { setUnauthorizedHandler } from '@/services/http'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'

import '@/assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

const authStore = useAuthStore()
const toastStore = useToastStore()

// Fired only when a session became invalid (a 401 that a token refresh couldn't
// recover): clear the session, tell the user, and send them to the login screen.
setUnauthorizedHandler(() => {
  const wasAuthenticated = authStore.isAuthenticated
  void authStore.logout()
  if (wasAuthenticated) {
    toastStore.warning('Tu sesión expiró. Vuelve a iniciar sesión.')
  }
  void router.push({ name: 'login' })
})
void authStore.fetchCurrentUser()

app.mount('#app')
