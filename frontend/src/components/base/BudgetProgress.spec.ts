import { render, screen } from '@testing-library/vue'
import { describe, expect, it } from 'vitest'

import BudgetProgress from '@/components/base/BudgetProgress.vue'
import type { Budget } from '@/types'

function budget(overrides: Partial<Budget> = {}): Budget {
  return {
    id: 'b1',
    categoryId: 'c1',
    categoryName: 'Comida',
    color: '#F97316',
    amount: 100,
    spent: 50,
    percentage: 50,
    status: 'ok',
    ...overrides,
  }
}

describe('BudgetProgress', () => {
  it('shows the used percentage for an ok budget', () => {
    render(BudgetProgress, { props: { budget: budget({ percentage: 50, status: 'ok' }) } })

    expect(screen.getByText('50% usado')).toBeInTheDocument()
    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '50')
  })

  it('warns when approaching the limit', () => {
    render(BudgetProgress, { props: { budget: budget({ percentage: 85, status: 'warning' }) } })

    expect(screen.getByText(/Te estás acercando al límite/)).toBeInTheDocument()
  })

  it('flags an exceeded budget and reports the real percentage', () => {
    render(BudgetProgress, { props: { budget: budget({ percentage: 130, status: 'exceeded' }) } })

    expect(screen.getByText(/Límite superado \(130%\)/)).toBeInTheDocument()
    expect(screen.getByRole('progressbar')).toHaveAttribute('aria-valuenow', '130')
  })
})
