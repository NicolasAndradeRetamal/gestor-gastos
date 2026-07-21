import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  type: ToastType
  message: string
  duration: number
}

// Success/info are quick confirmations; warnings/errors need longer to read.
const DURATIONS: Record<ToastType, number> = {
  success: 4500,
  info: 4500,
  warning: 8000,
  error: 8000,
}

const MAX_VISIBLE = 3
let nextId = 0

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([])

  function dismiss(id: number): void {
    toasts.value = toasts.value.filter((toast) => toast.id !== id)
  }

  function push(type: ToastType, message: string): number {
    const id = nextId++
    toasts.value = [...toasts.value, { id, type, message, duration: DURATIONS[type] }].slice(-MAX_VISIBLE)
    return id
  }

  function success(message: string): number {
    return push('success', message)
  }

  function error(message: string): number {
    return push('error', message)
  }

  function warning(message: string): number {
    return push('warning', message)
  }

  function info(message: string): number {
    return push('info', message)
  }

  return { toasts, push, dismiss, success, error, warning, info }
})
