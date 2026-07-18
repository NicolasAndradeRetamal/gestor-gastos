import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import BaseInput from '@/components/base/BaseInput.vue'

describe('BaseInput', () => {
  it('associates the visible label with the input', () => {
    render(BaseInput, { props: { modelValue: '', label: 'Correo electrónico' } })

    expect(screen.getByLabelText('Correo electrónico')).toBeInTheDocument()
  })

  it('emits update:modelValue as the user types', async () => {
    const user = userEvent.setup()
    const { emitted } = render(BaseInput, { props: { modelValue: '', label: 'Nombre' } })

    await user.type(screen.getByLabelText('Nombre'), 'Ana')

    expect(emitted('update:modelValue')).toBeTruthy()
  })

  it('shows the error message with role="alert" and marks the field invalid', () => {
    render(BaseInput, { props: { modelValue: '', label: 'Monto', error: 'El monto debe ser mayor que 0.' } })

    const alert = screen.getByRole('alert')
    expect(alert).toHaveTextContent('El monto debe ser mayor que 0.')
    expect(screen.getByLabelText('Monto')).toHaveAttribute('aria-invalid', 'true')
  })
})
