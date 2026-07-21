export type BudgetStatus = 'ok' | 'warning' | 'exceeded'

export interface Budget {
  id: string
  categoryId: string
  categoryName: string
  color: string
  amount: number
  spent: number
  percentage: number
  status: BudgetStatus
}

export interface BudgetCreateRequest {
  categoryId: string
  amount: number
}

export interface BudgetUpdateRequest {
  amount: number
}
