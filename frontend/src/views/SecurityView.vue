<template>
  <div class="space-y-6">
    <h1 class="text-2xl font-bold tracking-tight text-ink">Seguridad</h1>

    <div v-if="isLoadingUser" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton class="h-48" />
    </div>

    <ErrorState v-else-if="loadError" @retry="loadUser" />

    <BaseCard v-else>
      <template #header>
        <h2 class="text-lg font-semibold text-ink">Verificación en dos pasos</h2>
        <span
          v-if="isEnabled"
          class="inline-flex h-7 items-center gap-1.5 rounded-full bg-success-soft px-2.5 text-xs font-medium text-ink"
        >
          <AppIcon name="check" class="size-4 text-success" />
          Activada
        </span>
        <span
          v-else
          class="inline-flex h-7 items-center gap-1.5 rounded-full bg-surface-sunken px-2.5 text-xs font-medium text-ink-muted"
        >
          <AppIcon name="shield" class="size-4" />
          Desactivada
        </span>
      </template>

      <!-- Estado A: inactiva -->
      <div v-if="stage === 'inactive'" class="space-y-4">
        <p class="text-sm text-ink-muted">
          Añade una capa extra de seguridad. Cuando la actives, al iniciar sesión te pediremos un código
          temporal de tu aplicación autenticadora, además de tu contraseña.
        </p>
        <BaseButton :loading="actionBusy" loading-label="Preparando…" @click="startSetup">
          Activar verificación en dos pasos
        </BaseButton>
      </div>

      <!-- Estado B: alta en curso -->
      <div v-else-if="stage === 'setup'" class="space-y-5">
        <p class="text-sm text-ink">
          Escanea este código QR con tu aplicación autenticadora (Google Authenticator, Authy, 1Password u otra).
        </p>
        <img
          v-if="qrDataUrl"
          :src="qrDataUrl"
          alt="Código QR para configurar la verificación en dos pasos"
          class="mx-auto size-44 rounded-lg border border-line bg-white p-3"
        />
        <div class="space-y-2">
          <p class="text-sm text-ink-muted">¿No puedes escanear el código? Introduce esta clave en tu aplicación:</p>
          <div class="flex items-center gap-2">
            <code class="flex-1 break-all rounded-md bg-surface-sunken px-3 py-2 font-semibold tracking-wider text-ink">
              {{ setup?.secret }}
            </code>
            <BaseButton variant="secondary" size="compact" :icon="copiedSecret ? 'check' : 'copy'" @click="copySecret">
              {{ copiedSecret ? 'Copiado' : 'Copiar' }}
            </BaseButton>
          </div>
        </div>
        <form class="space-y-4" novalidate @submit.prevent="confirmEnable">
          <OtpInput
            v-model="confirmCode"
            label="Código de confirmación"
            help="Introduce el código de 6 dígitos que muestra tu aplicación para confirmar."
            :disabled="actionBusy"
            :error="confirmError"
          />
          <div class="flex items-center justify-end gap-3">
            <BaseButton variant="secondary" type="button" :disabled="actionBusy" @click="cancelSetup">Cancelar</BaseButton>
            <BaseButton type="submit" :loading="actionBusy" loading-label="Activando…">Confirmar y activar</BaseButton>
          </div>
        </form>
      </div>

      <!-- Pantalla de códigos de recuperación -->
      <div v-else-if="stage === 'recovery'" class="space-y-4">
        <div class="flex items-start gap-2 rounded-lg border border-warning/30 bg-warning-soft p-3">
          <AppIcon name="alert-triangle" class="size-5 shrink-0 text-warning" />
          <p class="text-sm text-ink">
            Guarda estos códigos en un lugar seguro. Son la única forma de entrar si pierdes el acceso a tu
            aplicación autenticadora, y no volverás a verlos.
          </p>
        </div>
        <h3 class="text-base font-semibold text-ink">Tus códigos de recuperación</h3>
        <ul class="grid grid-cols-2 gap-2">
          <li
            v-for="code in recoveryCodes"
            :key="code"
            class="rounded-md bg-surface-sunken px-3 py-2 text-center font-semibold tracking-wider text-ink"
          >
            {{ code }}
          </li>
        </ul>
        <div class="flex gap-3">
          <BaseButton variant="secondary" :icon="copiedCodes ? 'check' : 'copy'" @click="copyCodes">
            {{ copiedCodes ? 'Copiado' : 'Copiar' }}
          </BaseButton>
          <BaseButton variant="secondary" icon="download" @click="downloadCodes">Descargar</BaseButton>
        </div>
        <label class="flex items-start gap-2 text-sm text-ink">
          <input v-model="saved" type="checkbox" class="mt-0.5 size-4 rounded border-line accent-primary" />
          <span>Guardé mis códigos de recuperación en un lugar seguro.</span>
        </label>
        <BaseButton :disabled="!saved" class="w-full sm:w-auto" @click="finishRecovery">Finalizar</BaseButton>
      </div>

      <!-- Estado C: activa -->
      <div v-else class="space-y-4">
        <p class="text-sm text-ink-muted">{{ activeSinceText }}</p>
        <BaseButton variant="danger" @click="disableOpen = true">Desactivar</BaseButton>
      </div>
    </BaseCard>

    <Modal :open="disableOpen" title="Desactivar verificación en dos pasos" :close-on-overlay="false" :busy="disableBusy" @close="closeDisable">
      <p class="text-sm text-ink-muted">
        Al desactivarla, tu cuenta quedará protegida solo con la contraseña. Para confirmar, introduce tu
        contraseña y un código.
      </p>
      <p v-if="disableError" class="rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger" role="alert">
        {{ disableError }}
      </p>
      <form class="space-y-4" novalidate @submit.prevent="confirmDisable">
        <BaseInput
          v-model="disablePassword"
          type="password"
          label="Contraseña actual"
          autocomplete="current-password"
          :disabled="disableBusy"
        />
        <OtpInput
          v-model="disableCode"
          variant="recovery"
          label="Código"
          help="Un código de 6 dígitos de tu app o uno de tus códigos de recuperación."
          :disabled="disableBusy"
        />
      </form>
      <template #footer>
        <BaseButton variant="secondary" :disabled="disableBusy" @click="closeDisable">Cancelar</BaseButton>
        <BaseButton variant="danger" :loading="disableBusy" loading-label="Desactivando…" @click="confirmDisable">
          Desactivar
        </BaseButton>
      </template>
    </Modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import QRCode from 'qrcode'

import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import Modal from '@/components/base/Modal.vue'
import OtpInput from '@/components/base/OtpInput.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import { authService } from '@/services/auth.service'
import { toProblemDetails } from '@/services/http'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import type { TwoFactorSetupResponse } from '@/types'

const authStore = useAuthStore()
const toast = useToastStore()

const isLoadingUser = ref(false)
const loadError = ref(false)

const setup = ref<TwoFactorSetupResponse | null>(null)
const qrDataUrl = ref('')
const confirmCode = ref('')
const confirmError = ref<string | undefined>()
const recoveryCodes = ref<string[] | null>(null)
const saved = ref(false)
const actionBusy = ref(false)
const copiedSecret = ref(false)
const copiedCodes = ref(false)

const disableOpen = ref(false)
const disableBusy = ref(false)
const disablePassword = ref('')
const disableCode = ref('')
const disableError = ref<string | undefined>()

const isEnabled = computed(() => Boolean(authStore.user?.twoFactorEnabled))

const stage = computed(() => {
  if (recoveryCodes.value) return 'recovery'
  if (setup.value) return 'setup'
  return isEnabled.value ? 'active' : 'inactive'
})

const activeSinceText = computed(() => {
  const enabledAt = authStore.user?.twoFactorEnabledAt
  if (!enabledAt) return 'La verificación en dos pasos está activa.'
  const date = new Intl.DateTimeFormat('es-ES', { day: 'numeric', month: 'short', year: 'numeric' }).format(
    new Date(enabledAt),
  )
  return `La verificación en dos pasos está activa desde el ${date}.`
})

async function loadUser(): Promise<void> {
  if (authStore.user) return
  isLoadingUser.value = true
  loadError.value = false
  try {
    await authStore.fetchCurrentUser()
    if (!authStore.user) loadError.value = true
  } finally {
    isLoadingUser.value = false
  }
}

onMounted(loadUser)

async function startSetup(): Promise<void> {
  actionBusy.value = true
  try {
    const response = await authService.setupTwoFactor()
    setup.value = response
    qrDataUrl.value = await QRCode.toDataURL(response.otpauthUri, { margin: 0, width: 176 })
  } catch (err) {
    toast.error(toProblemDetails(err).detail ?? 'No se pudo iniciar la configuración.')
  } finally {
    actionBusy.value = false
  }
}

function cancelSetup(): void {
  setup.value = null
  qrDataUrl.value = ''
  confirmCode.value = ''
  confirmError.value = undefined
}

async function confirmEnable(): Promise<void> {
  confirmError.value = undefined
  actionBusy.value = true
  try {
    const response = await authService.enableTwoFactor(confirmCode.value)
    recoveryCodes.value = response.recoveryCodes
    if (authStore.user) {
      authStore.setUser({ ...authStore.user, twoFactorEnabled: true, twoFactorEnabledAt: new Date().toISOString() })
    }
  } catch (err) {
    confirmCode.value = ''
    confirmError.value = 'El código no es correcto. Comprueba que la hora de tu dispositivo esté sincronizada e inténtalo de nuevo.'
    void err
  } finally {
    actionBusy.value = false
  }
}

function finishRecovery(): void {
  recoveryCodes.value = null
  setup.value = null
  qrDataUrl.value = ''
  confirmCode.value = ''
  saved.value = false
  toast.success('Verificación en dos pasos activada.')
}

async function copySecret(): Promise<void> {
  if (!setup.value) return
  await navigator.clipboard.writeText(setup.value.secret)
  flash(copiedSecret)
}

async function copyCodes(): Promise<void> {
  if (!recoveryCodes.value) return
  await navigator.clipboard.writeText(recoveryCodes.value.join('\n'))
  flash(copiedCodes)
}

function downloadCodes(): void {
  if (!recoveryCodes.value) return
  const blob = new Blob([recoveryCodes.value.join('\n') + '\n'], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = 'codigos-recuperacion-gestor-gastos.txt'
  link.click()
  URL.revokeObjectURL(url)
}

function flash(flag: { value: boolean }): void {
  flag.value = true
  setTimeout(() => {
    flag.value = false
  }, 2000)
}

function closeDisable(): void {
  if (disableBusy.value) return
  disableOpen.value = false
  disablePassword.value = ''
  disableCode.value = ''
  disableError.value = undefined
}

async function confirmDisable(): Promise<void> {
  disableError.value = undefined
  disableBusy.value = true
  try {
    await authService.disableTwoFactor(disablePassword.value, disableCode.value)
    if (authStore.user) {
      authStore.setUser({ ...authStore.user, twoFactorEnabled: false, twoFactorEnabledAt: null })
    }
    disableBusy.value = false
    disableOpen.value = false
    disablePassword.value = ''
    disableCode.value = ''
    toast.success('Verificación en dos pasos desactivada.')
  } catch {
    disableError.value = 'La contraseña o el código no son correctos.'
    disableBusy.value = false
  }
}
</script>
