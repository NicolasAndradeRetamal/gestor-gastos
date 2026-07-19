import { mount } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent } from 'vue'

// Stubs matchMedia so each test controls whether the OS reports dark mode,
// and captures the 'change' listener useTheme registers to follow it live.
function stubMatchMedia(matches: boolean): { emitSystemChange: (next: boolean) => void } {
  const listeners: Array<(event: MediaQueryListEvent) => void> = []
  const mediaQueryList = {
    matches,
    media: '(prefers-color-scheme: dark)',
    addEventListener: (_type: string, listener: (event: MediaQueryListEvent) => void) => {
      listeners.push(listener)
    },
    removeEventListener: vi.fn(),
  }
  window.matchMedia = vi.fn().mockReturnValue(mediaQueryList) as unknown as typeof window.matchMedia

  return {
    emitSystemChange: (next: boolean) => {
      listeners.forEach((listener) => listener({ matches: next } as MediaQueryListEvent))
    },
  }
}

// useTheme keeps module-level singleton state; each test re-imports it fresh
// after resetting modules so a previous test's toggle doesn't leak in.
async function mountTheme() {
  const { useTheme } = await import('@/composables/useTheme')
  const TestComponent = defineComponent({
    setup: () => useTheme(),
    template: '<div />',
  })
  return mount(TestComponent)
}

describe('useTheme', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
    delete document.documentElement.dataset.theme
  })

  afterEach(() => {
    delete document.documentElement.dataset.theme
  })

  it('starts from the system preference when there is no stored override', async () => {
    stubMatchMedia(true)

    const wrapper = await mountTheme()

    expect(wrapper.vm.theme).toBe('dark')
    expect(document.documentElement.dataset.theme).toBeUndefined()
  })

  it('starts from the stored value when the user already chose a theme', async () => {
    localStorage.setItem('theme', 'light')
    stubMatchMedia(true)

    const wrapper = await mountTheme()

    expect(wrapper.vm.theme).toBe('light')
  })

  it('toggle flips the theme, sets data-theme and persists the choice', async () => {
    stubMatchMedia(false)

    const wrapper = await mountTheme()
    expect(wrapper.vm.theme).toBe('light')

    wrapper.vm.toggle()
    expect(wrapper.vm.theme).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(localStorage.getItem('theme')).toBe('dark')

    wrapper.vm.toggle()
    expect(wrapper.vm.theme).toBe('light')
    expect(localStorage.getItem('theme')).toBe('light')
  })

  it('keeps following the system preference live until the user overrides it', async () => {
    const { emitSystemChange } = stubMatchMedia(false)

    const wrapper = await mountTheme()
    expect(wrapper.vm.theme).toBe('light')

    emitSystemChange(true)
    expect(wrapper.vm.theme).toBe('dark')
  })

  it('stops following the system preference once the user toggles manually', async () => {
    const { emitSystemChange } = stubMatchMedia(false)

    const wrapper = await mountTheme()
    wrapper.vm.toggle()
    expect(wrapper.vm.theme).toBe('dark')

    // System reports light again, but the manual override must stick.
    emitSystemChange(false)
    expect(wrapper.vm.theme).toBe('dark')
  })
})
