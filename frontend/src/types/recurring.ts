export interface RecurringExpense {
  id: string
  categoryId: string
  categoryName: string
  color: string
  amount: number
  dayOfMonth: number
  note: string | null
  nextRunOn: string
}

export interface RecurringExpenseRequest {
  categoryId: string
  amount: number
  dayOfMonth: number
  note?: string | null
}
