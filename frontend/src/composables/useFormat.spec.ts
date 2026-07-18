import { describe, expect, it } from 'vitest'

import { formatAmount, formatFullDate, formatMonthLabel, formatShortDate } from '@/composables/useFormat'

describe('formatAmount', () => {
  it('formats with two decimal places', () => {
    expect(formatAmount(42.5)).toBe('42,50')
  })

  it('formats large amounts with a thousands separator', () => {
    expect(formatAmount(1234.5)).toBe('1.234,50')
  })
})

describe('formatShortDate', () => {
  it('formats an ISO date as "dd mmm" without a UTC shift', () => {
    // A naive `new Date("2026-07-01")` parse would render as the last day of June in negative UTC offsets.
    // Separator varies by ICU build (space on Linux, hyphen on Windows); the
    // assertion that matters is the day is 01 (no UTC-midnight shift to 30 jun).
    expect(formatShortDate('2026-07-01')).toMatch(/01\Wjul/i)
  })
})

describe('formatFullDate', () => {
  it('formats an ISO date as dd/mm/yyyy', () => {
    expect(formatFullDate('2026-07-15')).toBe('15/07/2026')
  })
})

describe('formatMonthLabel', () => {
  it('formats a YYYY-MM value as an abbreviated month', () => {
    expect(formatMonthLabel('2026-01')).toMatch(/ene/i)
    expect(formatMonthLabel('2026-12')).toMatch(/dic/i)
  })
})
