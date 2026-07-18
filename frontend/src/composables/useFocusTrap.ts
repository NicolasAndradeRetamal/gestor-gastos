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
