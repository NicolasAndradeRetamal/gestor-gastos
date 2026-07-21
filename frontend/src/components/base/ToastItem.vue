<template>
  <div
    :role="toast.type === 'error' || toast.type === 'warning' ? 'alert' : 'status'"
    class="pointer-events-auto relative flex w-full items-start gap-3 overflow-hidden rounded-lg border p-4 shadow-raised sm:w-96"
    :class="COLOR_CLASSES[toast.type]"
    @mouseenter="pause"
    @mouseleave="resume"
    @focusin="pause"
    @focusout="resume"
  >
    <AppIcon :name="ICONS[toast.type]" class="mt-0.5 size-5 shrink-0" :class="ICON_COLOR_CLASSES[toast.type]" aria-hidden="true" />
    <p class="flex-1 text-sm leading-snug text-ink">{{ toast.message }}</p>
    <button
      type="button"
      class="-mr-1 -mt-1 inline-flex size-8 shrink-0 items-center justify-center rounded-md text-ink-muted transition-colors duration-150 hover:bg-ink/10 hover:text-ink"
      aria-label="Cerrar notificación"
      @click="emit('dismiss')"
    >
      <AppIcon name="close" class="size-4" />
    </button>
    <div
      class="absolute bottom-0 left-0 h-0.5 motion-reduce:hidden"
      :class="BAR_COLOR_CLASSES[toast.type]"
      :style="{ width: `${barWidth}%` }"
      aria-hidden="true"
    />
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'

import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import type { Toast, ToastType } from '@/stores/toast'

const props = defineProps<{
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

const BAR_COLOR_CLASSES: Record<ToastType, string> = {
  success: 'bg-success/40',
  error: 'bg-danger/40',
  warning: 'bg-warning/40',
  info: 'bg-info/40',
}

const ICONS: Record<ToastType, IconName> = {
  success: 'check-circle',
  error: 'alert-circle',
  warning: 'alert-triangle',
  info: 'info-circle',
}

// Countdown that pauses on hover/focus; the progress bar shows the time left.
const barWidth = ref(100)
let deadline = 0
let remaining = props.toast.duration
let rafId: number | undefined

function tick(): void {
  const left = deadline - Date.now()
  barWidth.value = Math.max(0, (left / props.toast.duration) * 100)
  if (left <= 0) {
    emit('dismiss')
    return
  }
  rafId = requestAnimationFrame(tick)
}

function start(): void {
  deadline = Date.now() + remaining
  rafId = requestAnimationFrame(tick)
}

function pause(): void {
  if (rafId === undefined) return
  cancelAnimationFrame(rafId)
  rafId = undefined
  remaining = Math.max(0, deadline - Date.now())
}

function resume(): void {
  if (rafId === undefined && remaining > 0) start()
}

onMounted(start)
onBeforeUnmount(() => {
  if (rafId !== undefined) cancelAnimationFrame(rafId)
})
</script>
