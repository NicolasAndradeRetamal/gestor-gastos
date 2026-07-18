<template>
  <AuthLayout title="Iniciar sesión" :error-message="errorMessage">
    <form class="space-y-4" novalidate @submit.prevent="onSubmit">
      <BaseInput
        v-model="email"
        type="email"
        label="Correo electrónico"
        required
        autocomplete="email"
        :error="fieldError('email')"
      />
      <BaseInput
        v-model="password"
        type="password"
        label="Contraseña"
        required
        autocomplete="current-password"
        :error="fieldError('password')"
      />
      <BaseButton type="submit" class="w-full" :loading="authStore.isLoading" loading-label="Iniciando sesión…">
        Iniciar sesión
      </BaseButton>
    </form>
    <template #footer>
      ¿No tenés cuenta?
      <RouterLink :to="{ name: 'register' }" class="font-semibold text-primary">Registrate</RouterLink>
    </template>
  </AuthLayout>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import AuthLayout from '@/components/auth/AuthLayout.vue'
import { useAuthStore } from '@/stores/auth'

const email = ref('')
const password = ref('')

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const errorMessage = computed(() => {
  const error = authStore.error
  if (!error || error.errors) return null
  if (error.status === 401) return 'Correo o contraseña incorrectos.'
  return error.detail ?? 'No se pudo iniciar sesión. Inténtalo de nuevo.'
})

function fieldError(field: string): string | undefined {
  return authStore.error?.errors?.[field]?.[0]
}

async function onSubmit(): Promise<void> {
  const ok = await authStore.login({ email: email.value, password: password.value })
  if (ok) {
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    router.push(redirect)
  }
}
</script>
