import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue'

export type Theme = 'light' | 'dark'

const STORAGE_KEY = 'theme'

function readStoredTheme(): Theme | null {
  const stored = localStorage.getItem(STORAGE_KEY)
  return stored === 'light' || stored === 'dark' ? stored : null
}

function systemPrefersDark(): boolean {
  return window.matchMedia('(prefers-color-scheme: dark)').matches
}

function resolveActiveTheme(): Theme {
  return readStoredTheme() ?? (systemPrefersDark() ? 'dark' : 'light')
}

// Module-level singleton so every component using the composable shares the
// same reactive theme and stays in sync (sidebar, mobile menu, ...).
const theme = ref<Theme>(resolveActiveTheme())

function toggle(): void {
  const next: Theme = theme.value === 'dark' ? 'light' : 'dark'
  theme.value = next
  document.documentElement.dataset.theme = next
  localStorage.setItem(STORAGE_KEY, next)
}

export function useTheme(): { theme: Ref<Theme>; toggle: () => void } {
  const media = window.matchMedia('(prefers-color-scheme: dark)')

  // Follows the OS preference live, but only while the user never overrode it manually.
  function onSystemChange(event: MediaQueryListEvent): void {
    if (readStoredTheme() === null) {
      theme.value = event.matches ? 'dark' : 'light'
    }
  }

  onMounted(() => media.addEventListener('change', onSystemChange))
  onBeforeUnmount(() => media.removeEventListener('change', onSystemChange))

  return { theme, toggle }
}
