import { defineStore } from 'pinia'
import { ref } from 'vue'

import { toProblemDetails } from '@/services/http'
import { expensesService } from '@/services/expenses.service'
import type { Expense, ExpenseListParams, ExpenseRequest, ProblemDetails } from '@/types'

export interface ExpenseFilters {
  from?: string
  to?: string
  categoryId?: string
}

const DEFAULT_PAGE_SIZE = 20

export const useExpensesStore = defineStore('expenses', () => {
  const items = ref<Expense[]>([])
  const page = ref(1)
  const pageSize = ref(DEFAULT_PAGE_SIZE)
  const totalItems = ref(0)
  const totalPages = ref(0)
  const filters = ref<ExpenseFilters>({})
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)

  function setFilters(next: ExpenseFilters): void {
    filters.value = next
    page.value = 1
  }

  function setPage(next: number): void {
    page.value = next
  }

  async function fetchList(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      const params: ExpenseListParams = {
        ...filters.value,
        page: page.value,
        pageSize: pageSize.value,
        sort: '-spentAt',
      }
      const result = await expensesService.list(params)
      items.value = result.items
      page.value = result.page
      pageSize.value = result.pageSize
      totalItems.value = result.totalItems
      totalPages.value = result.totalPages
    } catch (err) {
      error.value = toProblemDetails(err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(payload: ExpenseRequest): Promise<void> {
    try {
      await expensesService.create(payload)
      await fetchList()
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function update(id: string, payload: ExpenseRequest): Promise<void> {
    try {
      await expensesService.update(id, payload)
      await fetchList()
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function remove(id: string): Promise<void> {
    try {
      await expensesService.remove(id)
      await fetchList()
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  return {
    items,
    page,
    pageSize,
    totalItems,
    totalPages,
    filters,
    isLoading,
    error,
    setFilters,
    setPage,
    fetchList,
    create,
    update,
    remove,
  }
})
