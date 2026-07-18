export type DateRangePreset = 'month' | 'quarter' | 'year' | 'all'

export const DATE_RANGE_PRESETS: { value: DateRangePreset; label: string }[] = [
  { value: 'month', label: 'Este mes' },
  { value: 'quarter', label: 'Últimos 3 meses' },
  { value: 'year', label: 'Este año' },
  { value: 'all', label: 'Todo' },
]

function toIsoDate(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function getPresetRange(preset: DateRangePreset, today = new Date()): { from?: string; to?: string } {
  const to = toIsoDate(today)
  switch (preset) {
    case 'month':
      return { from: toIsoDate(new Date(today.getFullYear(), today.getMonth(), 1)), to }
    case 'quarter':
      return { from: toIsoDate(new Date(today.getFullYear(), today.getMonth() - 2, 1)), to }
    case 'year':
      return { from: toIsoDate(new Date(today.getFullYear(), 0, 1)), to }
    case 'all':
      return {}
  }
}
