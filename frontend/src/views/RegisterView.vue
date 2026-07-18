<template>
  <AuthLayout title="Crear cuenta" :error-message="errorMessage">
    <form class="space-y-4" novalidate @submit.prevent="onSubmit">
      <BaseInput
        v-model="displayName"
        label="Nombre"
        required
        autocomplete="name"
        :error="fieldError('displayName')"
      />
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
        autocomplete="new-password"
        help="Mínimo 8 caracteres"
        :error="fieldError('password')"
      />
      <BaseButton type="submit" class="w-full" :loading="authStore.isLoading" loading-label="Creando cuenta…">
        Crear cuenta
      </BaseButton>
    </form>
    <template #footer>
      ¿Ya tienes cuenta?
      <RouterLink :to="{ name: 'login' }" class="font-semibold text-primary">Inicia sesión</RouterLink>
    </template>
  </AuthLayout>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

import AuthLayout from '@/components/auth/AuthLayout.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import { useAuthStore } from '@/stores/auth'

const displayName = ref('')
const email = ref('')
const password = ref('')

const authStore = useAuthStore()
const router = useRouter()

const errorMessage = computed(() => {
  const error = authStore.error
  if (!error || error.errors) return null
  if (error.status === 409) return 'Ya existe una cuenta con este correo.'
  return error.detail ?? 'No se pudo crear la cuenta. Inténtalo de nuevo.'
})

function fieldError(field: string): string | undefined {
  return authStore.error?.errors?.[field]?.[0]
}

async function onSubmit(): Promise<void> {
  const ok = await authStore.register({
    displayName: displayName.value,
    email: email.value,
    password: password.value,
  })
  if (ok) {
    router.push('/')
  }
}
</script>
