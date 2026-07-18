import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { expensesService } from '@/services/expenses.service'
import { useExpensesStore } from '@/stores/expenses'
import type { Expense, PagedResult } from '@/types'

vi.mock('@/services/expenses.service', () => ({
  expensesService: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    remove: vi.fn(),
  },
}))

const mockedExpensesService = vi.mocked(expensesService)

const expense: Expense = {
  id: 'e1',
  amount: 42.5,
  spentAt: '2026-07-15',
  note: 'Almuerzo',
  category: { id: 'c1', name: 'Comida', color: '#F97316' },
}

function pagedResult(items: Expense[]): PagedResult<Expense> {
  return { items, page: 1, pageSize: 20, totalItems: items.length, totalPages: 1 }
}

describe('expenses store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('loads a page of expenses', async () => {
    mockedExpensesService.list.mockResolvedValue(pagedResult([expense]))
    const store = useExpensesStore()

    await store.fetchList()

    expect(store.items).toEqual([expense])
    expect(store.totalItems).toBe(1)
    expect(store.isLoading).toBe(false)
    expect(store.error).toBeNull()
  })

  it('resets to page 1 when filters change', () => {
    const store = useExpensesStore()
    store.setPage(3)

    store.setFilters({ categoryId: 'c1' })

    expect(store.page).toBe(1)
    expect(store.filters).toEqual({ categoryId: 'c1' })
  })

  it('exposes a ProblemDetails error when the list request fails', async () => {
    mockedExpensesService.list.mockRejectedValue(new Error('network down'))
    const store = useExpensesStore()

    await store.fetchList()

    expect(store.error).not.toBeNull()
    expect(store.items).toEqual([])
  })

  it('re-fetches the list after creating an expense', async () => {
    mockedExpensesService.create.mockResolvedValue(expense)
    mockedExpensesService.list.mockResolvedValue(pagedResult([expense]))
    const store = useExpensesStore()

    await store.create({ amount: 42.5, spentAt: '2026-07-15', categoryId: 'c1' })

    expect(mockedExpensesService.list).toHaveBeenCalledTimes(1)
    expect(store.items).toEqual([expense])
  })
})
