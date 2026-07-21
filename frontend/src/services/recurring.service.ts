import { httpClient } from '@/services/http'
import type { RecurringExpense, RecurringExpenseRequest } from '@/types'

const BASE = '/recurring-expenses'

export const recurringService = {
  async list(): Promise<RecurringExpense[]> {
    const { data } = await httpClient.get<RecurringExpense[]>(BASE)
    return data
  },

  async create(payload: RecurringExpenseRequest): Promise<RecurringExpense> {
    const { data } = await httpClient.post<RecurringExpense>(BASE, payload)
    return data
  },

  async update(id: string, payload: RecurringExpenseRequest): Promise<RecurringExpense> {
    const { data } = await httpClient.put<RecurringExpense>(`${BASE}/${id}`, payload)
    return data
  },

  async remove(id: string): Promise<void> {
    await httpClient.delete(`${BASE}/${id}`)
  },
}
