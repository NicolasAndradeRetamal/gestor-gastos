import { render, screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'

import BaseButton from '@/components/base/BaseButton.vue'

describe('BaseButton', () => {
  it('uses a 44px tap target with a 24px icon for icon-only buttons by default', () => {
    render(BaseButton, { props: { iconOnly: true, icon: 'close' }, attrs: { 'aria-label': 'Cerrar' } })

    const button = screen.getByRole('button', { name: 'Cerrar' })
    expect(button.className).toContain('size-11')
    expect(button.querySelector('svg')?.getAttribute('class')).toContain('size-6')
    // Guard the padding conflict that once crushed the icon: a fixed-size
    // icon-only button must carry px-0 and never a wider px-* utility.
    expect(button.className).toContain('px-0')
    expect(button.className).not.toContain('px-4')
  })

  it('uses a 32px tap target with a 16px icon for the compact icon-only variant', () => {
    render(BaseButton, {
      props: { iconOnly: true, icon: 'edit', size: 'compact' },
      attrs: { 'aria-label': 'Editar' },
    })

    const button = screen.getByRole('button', { name: 'Editar' })
    expect(button.className).toContain('size-8')
    expect(button.querySelector('svg')?.getAttribute('class')).toContain('size-4')
    expect(button.className).toContain('px-0')
    expect(button.className).not.toContain('px-4')
  })

  it('keeps a 16px icon next to visible text regardless of size', () => {
    render(BaseButton, { props: { icon: 'plus' }, slots: { default: 'Añadir gasto' } })

    const button = screen.getByRole('button', { name: 'Añadir gasto' })
    expect(button.querySelector('svg')?.getAttribute('class')).toContain('size-4')
  })
})
