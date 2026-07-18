export interface ExpenseCategory {
  id: string
  name: string
  color: string
}

export interface Expense {
  id: string
  amount: number
  spentAt: string
  note?: string | null
  category: ExpenseCategory
}

export interface ExpenseRequest {
  amount: number
  spentAt: string
  note?: string
  categoryId: string
}

export type ExpenseSort = 'spentAt' | '-spentAt' | 'amount' | '-amount'

export interface ExpenseListParams {
  from?: string
  to?: string
  categoryId?: string
  page?: number
  pageSize?: number
  sort?: ExpenseSort
}
