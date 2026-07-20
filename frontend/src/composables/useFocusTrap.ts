import { ref } from 'vue'

// Remembers the element focused before a modal opens and restores it on close.
export function useFocusTrap() {
  const panelRef = ref<HTMLElement | null>(null)
  let previouslyFocused: HTMLElement | null = null

  function activate(): void {
    previouslyFocused = document.activeElement as HTMLElement | null
  }

  function deactivate(): void {
    previouslyFocused?.focus()
    previouslyFocused = null
  }

  return { panelRef, activate, deactivate }
}

// Keeps Tab/Shift+Tab cycling within a container (modal panel, popover menu).
export function trapTabKey(event: KeyboardEvent, container: HTMLElement | null): void {
  if (!container) return
  const focusable = container.querySelectorAll<HTMLElement>(
    'a[href], button:not([disabled]), textarea, input, select, [tabindex]:not([tabindex="-1"])',
  )
  if (focusable.length === 0) return
  const first = focusable[0]
  const last = focusable[focusable.length - 1]
  if (!first || !last) return
  if (event.shiftKey && document.activeElement === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault()
    first.focus()
  }
}
