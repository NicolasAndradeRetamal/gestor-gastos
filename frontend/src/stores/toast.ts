import { defineStore } from 'pinia'
import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface Toast {
  id: number
  type: ToastType
  message: string
}

const AUTO_DISMISS_MS = 5000
let nextId = 0

export const useToastStore = defineStore('toast', () => {
  const toasts = ref<Toast[]>([])

  function dismiss(id: number): void {
    toasts.value = toasts.value.filter((toast) => toast.id !== id)
  }

  function push(type: ToastType, message: string): number {
    const id = nextId++
    toasts.value = [...toasts.value, { id, type, message }].slice(-3)
    if (type === 'success' || type === 'info') {
      setTimeout(() => dismiss(id), AUTO_DISMISS_MS)
    }
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
