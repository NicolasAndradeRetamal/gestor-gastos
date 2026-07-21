import { defineStore } from 'pinia'
import { ref } from 'vue'

import { recurringService } from '@/services/recurring.service'
import { toProblemDetails } from '@/services/http'
import type { ProblemDetails, RecurringExpense, RecurringExpenseRequest } from '@/types'

export const useRecurringStore = defineStore('recurring', () => {
  const items = ref<RecurringExpense[]>([])
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)

  function sortByNextRun(list: RecurringExpense[]): RecurringExpense[] {
    return [...list].sort((a, b) => a.nextRunOn.localeCompare(b.nextRunOn) || a.categoryName.localeCompare(b.categoryName))
  }

  async function fetchAll(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      items.value = await recurringService.list()
    } catch (err) {
      error.value = toProblemDetails(err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(payload: RecurringExpenseRequest): Promise<RecurringExpense> {
    try {
      const created = await recurringService.create(payload)
      items.value = sortByNextRun([...items.value, created])
      return created
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function update(id: string, payload: RecurringExpenseRequest): Promise<RecurringExpense> {
    try {
      const updated = await recurringService.update(id, payload)
      items.value = sortByNextRun(items.value.map((item) => (item.id === id ? updated : item)))
      return updated
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function remove(id: string): Promise<void> {
    try {
      await recurringService.remove(id)
      items.value = items.value.filter((item) => item.id !== id)
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  return { items, isLoading, error, fetchAll, create, update, remove }
})
