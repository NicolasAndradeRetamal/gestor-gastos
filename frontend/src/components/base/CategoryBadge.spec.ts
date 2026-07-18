import { render, screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'

import CategoryBadge from '@/components/base/CategoryBadge.vue'

describe('CategoryBadge', () => {
  it('always shows the category name next to the color, never color alone', () => {
    render(CategoryBadge, { props: { name: 'Comida', color: '#F97316' } })

    expect(screen.getByText('Comida')).toBeInTheDocument()
  })

  it('renders the compact "sm" variant with just a dot and the name', () => {
    const { container } = render(CategoryBadge, { props: { name: 'Transporte', color: '#3B82F6', size: 'sm' } })

    expect(screen.getByText('Transporte')).toBeInTheDocument()
    expect(container.querySelector('.rounded-full')).toBeTruthy()
  })
})
