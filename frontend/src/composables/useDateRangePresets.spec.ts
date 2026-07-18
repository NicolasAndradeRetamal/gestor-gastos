import { describe, expect, it } from 'vitest'

import { getPresetRange } from '@/composables/useDateRangePresets'

describe('getPresetRange', () => {
  const referenceDate = new Date(2026, 6, 17) // 2026-07-17

  it('returns the first day of the current month for "month"', () => {
    expect(getPresetRange('month', referenceDate)).toEqual({ from: '2026-07-01', to: '2026-07-17' })
  })

  it('returns a 3-month window for "quarter"', () => {
    expect(getPresetRange('quarter', referenceDate)).toEqual({ from: '2026-05-01', to: '2026-07-17' })
  })

  it('returns January 1st of the current year for "year"', () => {
    expect(getPresetRange('year', referenceDate)).toEqual({ from: '2026-01-01', to: '2026-07-17' })
  })

  it('returns no bounds for "all"', () => {
    expect(getPresetRange('all', referenceDate)).toEqual({})
  })
})
