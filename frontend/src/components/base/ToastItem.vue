<template>
  <div
    :role="toast.type === 'error' || toast.type === 'warning' ? 'alert' : 'status'"
    class="flex w-full items-start gap-3 rounded-lg border p-4 shadow-raised sm:w-96"
    :class="COLOR_CLASSES[toast.type]"
  >
    <AppIcon :name="ICONS[toast.type]" class="size-5 shrink-0" :class="ICON_COLOR_CLASSES[toast.type]" />
    <p class="flex-1 text-sm text-ink">{{ toast.message }}</p>
    <BaseButton variant="ghost" icon-only icon="close" aria-label="Cerrar notificación" @click="emit('dismiss')">
      Cerrar
    </BaseButton>
  </div>
</template>

<script setup lang="ts">
import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import type { Toast, ToastType } from '@/stores/toast'

defineProps<{
  toast: Toast
}>()

const emit = defineEmits<{
  dismiss: []
}>()

const COLOR_CLASSES: Record<ToastType, string> = {
  success: 'bg-success-soft border-success/30',
  error: 'bg-danger-soft border-danger/30',
  warning: 'bg-warning-soft border-warning/30',
  info: 'bg-info-soft border-info/30',
}

const ICON_COLOR_CLASSES: Record<ToastType, string> = {
  success: 'text-success',
  error: 'text-danger',
  warning: 'text-warning',
  info: 'text-info',
}

const ICONS: Record<ToastType, IconName> = {
  success: 'check-circle',
  error: 'alert-circle',
  warning: 'alert-triangle',
  info: 'info-circle',
}
</script>
