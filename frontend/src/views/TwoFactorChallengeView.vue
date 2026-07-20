<template>
  <AuthLayout title="Verificación en dos pasos" :error-message="bannerError">
    <template v-if="expired">
      <BaseButton class="w-full" @click="backToLogin">Volver a iniciar sesión</BaseButton>
    </template>
    <form v-else class="space-y-4" novalidate @submit.prevent="onSubmit">
      <p class="text-sm text-ink-muted">{{ helpText }}</p>
      <OtpInput
        v-model="code"
        :variant="useRecovery ? 'recovery' : 'totp'"
        :label="useRecovery ? 'Código de recuperación' : 'Código de verificación'"
        autofocus
        :disabled="authStore.isLoading"
        :error="fieldError"
      />
      <BaseButton
        type="submit"
        class="w-full"
        :loading="authStore.isLoading"
        loading-label="Verificando…"
      >
        Verificar
      </BaseButton>
    </form>
    <template #footer>
      <button
        v-if="!expired"
        type="button"
        class="font-semibold text-primary"
        @click="toggleMode"
      >
        {{ useRecovery ? 'Usar el código de la aplicación' : '¿Perdiste tu dispositivo? Usa un código de recuperación' }}
      </button>
    </template>
  </AuthLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

import BaseButton from '@/components/base/BaseButton.vue'
import OtpInput from '@/components/base/OtpInput.vue'
import AuthLayout from '@/components/auth/AuthLayout.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const router = useRouter()

const code = ref('')
const useRecovery = ref(false)
const fieldError = ref<string | undefined>()
const expired = ref(false)
const rateLimited = ref(false)

const helpText = computed(() =>
  useRecovery.value
    ? 'Introduce uno de tus códigos de recuperación (formato XXXXX-XXXXX).'
    : 'Introduce el código de 6 dígitos de tu aplicación autenticadora.',
)

const bannerError = computed(() => {
  if (expired.value) return 'Tu sesión de verificación expiró. Inicia sesión de nuevo.'
  if (rateLimited.value) return 'Has hecho demasiados intentos. Espera un momento e inténtalo de nuevo.'
  return null
})

onMounted(() => {
  if (!authStore.hasTwoFactorChallenge) {
    router.replace({ name: 'login' })
  }
})

function toggleMode(): void {
  useRecovery.value = !useRecovery.value
  code.value = ''
  fieldError.value = undefined
}

function backToLogin(): void {
  authStore.cancelTwoFactorChallenge()
  router.replace({ name: 'login' })
}

async function onSubmit(): Promise<void> {
  fieldError.value = undefined
  rateLimited.value = false

  const ok = await authStore.verifyTwoFactor(code.value)
  if (ok) {
    router.replace({ name: 'dashboard' })
    return
  }

  const error = authStore.error
  if (error?.status === 429) {
    rateLimited.value = true
    return
  }

  // The challenge token expired (5 min) rather than a wrong code.
  if (error?.status === 401 && /desaf|expir/i.test(error.detail ?? '')) {
    expired.value = true
    return
  }

  code.value = ''
  fieldError.value = 'Código incorrecto. Vuelve a intentarlo.'
}
</script>
