import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import ExpenseToolsMenu from '@/components/expenses/ExpenseToolsMenu.vue'

describe('ExpenseToolsMenu', () => {
  it('stays closed until the trigger is clicked', () => {
    render(ExpenseToolsMenu)
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })

  it('emits the chosen action and closes', async () => {
    const user = userEvent.setup()
    const { emitted } = render(ExpenseToolsMenu)

    await user.click(screen.getByRole('button', { name: 'Más' }))
    await user.click(screen.getByRole('menuitem', { name: 'Exportar CSV' }))

    expect(emitted().select).toEqual([['export']])
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })

  it('offers all three tools', async () => {
    const user = userEvent.setup()
    render(ExpenseToolsMenu)

    await user.click(screen.getByRole('button', { name: 'Más' }))

    expect(screen.getByRole('menuitem', { name: 'Gastos recurrentes' })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: 'Importar CSV' })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: 'Exportar CSV' })).toBeInTheDocument()
  })
})
