import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

// ThemeToggle relies on useTheme's module-level singleton, so each test
// re-imports the component fresh after resetting modules and storage.
async function renderToggle(props?: { variant?: 'icon' | 'menu-item' }) {
  const { default: ThemeToggle } = await import('@/components/base/ThemeToggle.vue')
  return render(ThemeToggle, { props })
}

describe('ThemeToggle', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
    delete document.documentElement.dataset.theme
  })

  it('shows the sun icon and offers to switch to dark mode when light is active', async () => {
    await renderToggle()

    expect(screen.getByRole('button', { name: 'Cambiar a modo oscuro' })).toBeInTheDocument()
  })

  it('starts in dark mode when the user already chose it, offering to switch to light', async () => {
    localStorage.setItem('theme', 'dark')

    await renderToggle()

    expect(screen.getByRole('button', { name: 'Cambiar a modo claro' })).toBeInTheDocument()
  })

  it('toggles the theme on click, updates its own label and persists the choice', async () => {
    const user = userEvent.setup()
    await renderToggle()

    await user.click(screen.getByRole('button', { name: 'Cambiar a modo oscuro' }))

    expect(screen.getByRole('button', { name: 'Cambiar a modo claro' })).toBeInTheDocument()
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(localStorage.getItem('theme')).toBe('dark')
  })

  it('renders as a labelled row for the mobile user menu', async () => {
    await renderToggle({ variant: 'menu-item' })

    expect(screen.getByRole('menuitem', { name: 'Modo oscuro' })).toBeInTheDocument()
  })
})
