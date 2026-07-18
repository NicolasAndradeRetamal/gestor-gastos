import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import ExpenseList from '@/components/expenses/ExpenseList.vue'
import type { Expense } from '@/types'

const expense: Expense = {
  id: 'e1',
  amount: 42.5,
  spentAt: '2026-07-15',
  note: 'Almuerzo',
  category: { id: 'c1', name: 'Comida', color: '#F97316' },
}

const baseProps = { items: [] as Expense[], loading: false, page: 1, pageSize: 20, totalItems: 0, totalPages: 0 }

describe('ExpenseList', () => {
  it('shows a loading status while fetching', () => {
    render(ExpenseList, { props: { ...baseProps, loading: true } })

    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('shows the empty state when there are no expenses', () => {
    render(ExpenseList, { props: baseProps })

    expect(screen.getByText('Sin gastos en este periodo')).toBeInTheDocument()
  })

  it('emits "add" from the empty state action', async () => {
    const user = userEvent.setup()
    const { emitted } = render(ExpenseList, { props: baseProps })

    await user.click(screen.getByRole('button', { name: 'Añadir gasto' }))

    expect(emitted('add')).toBeTruthy()
  })

  it('renders expenses and emits edit/delete', async () => {
    const user = userEvent.setup()
    const { emitted } = render(ExpenseList, {
      props: { ...baseProps, items: [expense], totalItems: 1, totalPages: 1 },
    })

    expect(screen.getAllByText('Almuerzo').length).toBeGreaterThan(0)

    await user.click(screen.getByRole('button', { name: /Editar gasto/ }))
    expect(emitted('edit')?.[0]).toEqual([expense])

    await user.click(screen.getByRole('button', { name: /Eliminar gasto/ }))
    expect(emitted('delete')?.[0]).toEqual([expense])
  })
})
