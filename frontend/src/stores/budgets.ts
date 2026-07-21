import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { budgetsService } from '@/services/budgets.service'
import { toProblemDetails } from '@/services/http'
import type { Budget, BudgetCreateRequest, BudgetUpdateRequest, ProblemDetails } from '@/types'

export const useBudgetsStore = defineStore('budgets', () => {
  const items = ref<Budget[]>([])
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)
  const loaded = ref(false)

  const budgetedCategoryIds = computed(() => new Set(items.value.map((budget) => budget.categoryId)))

  async function fetchAll(force = false): Promise<void> {
    if (loaded.value && !force) return
    isLoading.value = true
    error.value = null
    try {
      items.value = await budgetsService.list()
      loaded.value = true
    } catch (err) {
      error.value = toProblemDetails(err)
    } finally {
      isLoading.value = false
    }
  }

  function sortByPercentageDesc(list: Budget[]): Budget[] {
    return [...list].sort((a, b) => b.percentage - a.percentage || a.categoryName.localeCompare(b.categoryName))
  }

  async function create(payload: BudgetCreateRequest): Promise<Budget> {
    try {
      const created = await budgetsService.create(payload)
      items.value = sortByPercentageDesc([...items.value, created])
      return created
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function update(id: string, payload: BudgetUpdateRequest): Promise<Budget> {
    try {
      const updated = await budgetsService.update(id, payload)
      items.value = sortByPercentageDesc(items.value.map((budget) => (budget.id === id ? updated : budget)))
      return updated
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function remove(id: string): Promise<void> {
    try {
      await budgetsService.remove(id)
      items.value = items.value.filter((budget) => budget.id !== id)
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  return {
    items,
    isLoading,
    error,
    loaded,
    budgetedCategoryIds,
    fetchAll,
    create,
    update,
    remove,
  }
})
