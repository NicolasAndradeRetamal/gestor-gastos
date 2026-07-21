import { httpClient } from '@/services/http'
import type { Budget, BudgetCreateRequest, BudgetUpdateRequest } from '@/types'

export const budgetsService = {
  async list(): Promise<Budget[]> {
    const { data } = await httpClient.get<Budget[]>('/budgets')
    return data
  },

  async create(payload: BudgetCreateRequest): Promise<Budget> {
    const { data } = await httpClient.post<Budget>('/budgets', payload)
    return data
  },

  async update(id: string, payload: BudgetUpdateRequest): Promise<Budget> {
    const { data } = await httpClient.put<Budget>(`/budgets/${id}`, payload)
    return data
  },

  async remove(id: string): Promise<void> {
    await httpClient.delete(`/budgets/${id}`)
  },
}
