import { httpClient } from '@/services/http'
import type { Expense, ExpenseListParams, ExpenseRequest, PagedResult } from '@/types'

export const expensesService = {
  async list(params: ExpenseListParams): Promise<PagedResult<Expense>> {
    const { data } = await httpClient.get<PagedResult<Expense>>('/expenses', { params })
    return data
  },

  async get(id: string): Promise<Expense> {
    const { data } = await httpClient.get<Expense>(`/expenses/${id}`)
    return data
  },

  async create(payload: ExpenseRequest): Promise<Expense> {
    const { data } = await httpClient.post<Expense>('/expenses', payload)
    return data
  },

  async update(id: string, payload: ExpenseRequest): Promise<Expense> {
    const { data } = await httpClient.put<Expense>(`/expenses/${id}`, payload)
    return data
  },

  async remove(id: string): Promise<void> {
    await httpClient.delete(`/expenses/${id}`)
  },
}
