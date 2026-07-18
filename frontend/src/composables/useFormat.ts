const LOCALE = 'es-AR'

const currencyFormatter = new Intl.NumberFormat(LOCALE, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function formatAmount(value: number): string {
  return currencyFormatter.format(value)
}

// `spentAt` comes as a plain `YYYY-MM-DD` date (no time/zone); parse it
// manually to avoid the UTC-midnight shift that `new Date(str)` introduces.
function parseIsoDate(isoDate: string): Date {
  const [year, month, day] = isoDate.split('-').map(Number)
  return new Date(year ?? 1970, (month ?? 1) - 1, day ?? 1)
}

export function formatShortDate(isoDate: string): string {
  return new Intl.DateTimeFormat(LOCALE, { day: '2-digit', month: 'short' }).format(parseIsoDate(isoDate))
}

export function formatFullDate(isoDate: string): string {
  return new Intl.DateTimeFormat(LOCALE, { day: '2-digit', month: '2-digit', year: 'numeric' }).format(
    parseIsoDate(isoDate),
  )
}

export function formatMonthLabel(yearMonth: string): string {
  const [year, month] = yearMonth.split('-').map(Number)
  const date = new Date(year ?? 1970, (month ?? 1) - 1, 1)
  return new Intl.DateTimeFormat(LOCALE, { month: 'short' }).format(date)
}
