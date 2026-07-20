import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

// ThemeToggle relies on useTheme's module-level singleton, so each test
// re-imports the component fresh after resetting modules and storage.
async function renderToggle() {
  const { default: ThemeToggle } = await import('@/components/base/ThemeToggle.vue')
  return render(ThemeToggle)
}

describe('ThemeToggle', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
    delete document.documentElement.dataset.theme
  })

  it('renders as a labelled menu row and offers to switch to dark mode when light is active', async () => {
    await renderToggle()

    expect(screen.getByRole('menuitem', { name: 'Modo oscuro' })).toBeInTheDocument()
  })

  it('starts in dark mode when the user already chose it, offering to switch to light', async () => {
    localStorage.setItem('theme', 'dark')

    await renderToggle()

    expect(screen.getByRole('menuitem', { name: 'Modo claro' })).toBeInTheDocument()
  })

  it('toggles the theme on click, updates its own label and persists the choice', async () => {
    const user = userEvent.setup()
    await renderToggle()

    await user.click(screen.getByRole('menuitem', { name: 'Modo oscuro' }))

    expect(screen.getByRole('menuitem', { name: 'Modo claro' })).toBeInTheDocument()
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(localStorage.getItem('theme')).toBe('dark')
  })
})
