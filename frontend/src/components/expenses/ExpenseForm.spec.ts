import { fireEvent, render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import ExpenseForm from '@/components/expenses/ExpenseForm.vue'
import type { Category, Expense } from '@/types'

const categories: Category[] = [
  { id: 'c1', name: 'Comida', color: '#F97316', isDefault: true },
  { id: 'c2', name: 'Transporte', color: '#3B82F6', isDefault: true },
]

const baseProps = {
  open: true,
  categories,
  expense: null as Expense | null,
  busy: false,
  error: null,
}

describe('ExpenseForm', () => {
  it('renders the create title and empty fields when there is no expense', () => {
    render(ExpenseForm, { props: baseProps })

    expect(screen.getByRole('dialog', { name: 'Añadir gasto' })).toBeInTheDocument()
    expect(screen.getByLabelText(/Monto/)).toHaveValue(null)
  })

  it('prefills the fields when editing an existing expense', () => {
    const expense: Expense = {
      id: 'e1',
      amount: 42.5,
      spentAt: '2026-07-15',
      note: 'Almuerzo',
      category: { id: 'c1', name: 'Comida', color: '#F97316' },
    }

    render(ExpenseForm, { props: { ...baseProps, expense } })

    expect(screen.getByRole('dialog', { name: 'Editar gasto' })).toBeInTheDocument()
    expect(screen.getByLabelText(/Monto/)).toHaveValue(42.5)
    expect(screen.getByLabelText('Nota')).toHaveValue('Almuerzo')
  })

  it('emits submit with the entered payload', async () => {
    const user = userEvent.setup()
    const { emitted } = render(ExpenseForm, { props: baseProps })

    await user.type(screen.getByLabelText(/Monto/), '15.50')
    await fireEvent.update(screen.getByLabelText(/Fecha/), '2026-07-10')
    await user.selectOptions(screen.getByLabelText(/Categoría/), 'c2')
    await user.click(screen.getByRole('button', { name: 'Guardar gasto' }))

    const events = emitted('submit') as unknown[][] | undefined
    expect(events).toBeTruthy()
    expect(events?.[0]?.[0]).toMatchObject({ amount: 15.5, spentAt: '2026-07-10', categoryId: 'c2' })
  })

  it('maps API field errors and a banner message onto the form', () => {
    render(ExpenseForm, {
      props: {
        ...baseProps,
        error: {
          title: 'Validation failed',
          status: 400,
          detail: 'One or more fields are invalid.',
          errors: { amount: ['El monto debe ser mayor que 0.'] },
        },
      },
    })

    expect(screen.getByText('El monto debe ser mayor que 0.')).toBeInTheDocument()
  })
})
