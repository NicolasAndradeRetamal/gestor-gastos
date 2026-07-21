import { render, screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'

import OtpInput from '@/components/base/OtpInput.vue'

describe('OtpInput', () => {
  it('renders a single input with numeric hints for the totp variant', () => {
    render(OtpInput, { props: { modelValue: '', label: 'Código de verificación' } })

    const input = screen.getByLabelText('Código de verificación') as HTMLInputElement
    expect(input.inputMode).toBe('numeric')
    expect(input.getAttribute('autocomplete')).toBe('one-time-code')
    expect(input.maxLength).toBe(6)
  })

  it('strips non-digits from the totp variant', async () => {
    const user = userEvent.setup()
    const onUpdate = vi.fn()
    render(OtpInput, { props: { modelValue: '', label: 'Código', 'onUpdate:modelValue': onUpdate } })

    await user.type(screen.getByLabelText('Código'), '1a2')

    expect(onUpdate).toHaveBeenLastCalledWith('12')
  })

  it('emits "complete" when six digits are entered', async () => {
    const user = userEvent.setup()
    const onUpdate = vi.fn()
    const onComplete = vi.fn()
    render(OtpInput, {
      props: { modelValue: '', label: 'Código', 'onUpdate:modelValue': onUpdate, onComplete },
    })

    await user.type(screen.getByLabelText('Código'), '123456')

    expect(onUpdate).toHaveBeenLastCalledWith('123456')
    expect(onComplete).toHaveBeenCalled()
  })

  it('uses text input mode for the recovery variant', () => {
    render(OtpInput, { props: { modelValue: '', label: 'Código', variant: 'recovery' } })

    const input = screen.getByLabelText('Código') as HTMLInputElement
    expect(input.inputMode).toBe('text')
    expect(input.maxLength).toBe(11)
  })

  it('exposes the error message with an alert role', () => {
    render(OtpInput, { props: { modelValue: '', label: 'Código', error: 'Código incorrecto.' } })

    expect(screen.getByRole('alert')).toHaveTextContent('Código incorrecto.')
  })
})
