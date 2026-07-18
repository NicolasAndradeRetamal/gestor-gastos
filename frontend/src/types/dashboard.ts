export interface CategoryTotal {
  categoryId: string
  categoryName: string
  color: string
  total: number
}

export interface MonthTotal {
  month: string
  total: number
}

export interface DashboardSummary {
  total: number
  byCategory: CategoryTotal[]
  byMonth: MonthTotal[]
}

export interface DashboardParams {
  from?: string
  to?: string
}
