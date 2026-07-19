import { onBeforeUnmount, onMounted, ref, type Ref } from 'vue'

export type Theme = 'light' | 'dark'

const STORAGE_KEY = 'theme'

// localStorage can throw (SecurityError) with storage blocked — private mode,
// a sandboxed iframe, browser settings — not just in SSR; never let that throw
// at module scope, or the whole app fails to mount.
function readStoredTheme(): Theme | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    return stored === 'light' || stored === 'dark' ? stored : null
  } catch {
    return null
  }
}

function persistTheme(value: Theme): void {
  try {
    localStorage.setItem(STORAGE_KEY, value)
  } catch {
    // Storage unavailable: the choice just won't survive a reload.
  }
}

function getMediaQueryList(): MediaQueryList | null {
  try {
    return window.matchMedia?.('(prefers-color-scheme: dark)') ?? null
  } catch {
    return null
  }
}

function systemPrefersDark(): boolean {
  return getMediaQueryList()?.matches ?? false
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
  persistTheme(next)
}

export function useTheme(): { theme: Ref<Theme>; toggle: () => void } {
  const media = getMediaQueryList()

  // Follows the OS preference live, but only while the user never overrode it manually.
  function onSystemChange(event: MediaQueryListEvent): void {
    if (readStoredTheme() === null) {
      theme.value = event.matches ? 'dark' : 'light'
    }
  }

  if (media) {
    onMounted(() => media.addEventListener('change', onSystemChange))
    onBeforeUnmount(() => media.removeEventListener('change', onSystemChange))
  }

  return { theme, toggle }
}
