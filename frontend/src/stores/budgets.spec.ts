import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { budgetsService } from '@/services/budgets.service'
import { useBudgetsStore } from '@/stores/budgets'
import type { Budget } from '@/types'

vi.mock('@/services/budgets.service', () => ({
  budgetsService: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    remove: vi.fn(),
  },
}))

const mocked = vi.mocked(budgetsService)

function budget(overrides: Partial<Budget> = {}): Budget {
  return {
    id: 'b1',
    categoryId: 'c1',
    categoryName: 'Comida',
    color: '#F97316',
    amount: 100,
    spent: 50,
    percentage: 50,
    status: 'ok',
    ...overrides,
  }
}

describe('budgets store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('loads budgets and exposes the budgeted category ids', async () => {
    mocked.list.mockResolvedValue([budget({ categoryId: 'c1' }), budget({ id: 'b2', categoryId: 'c2' })])
    const store = useBudgetsStore()

    await store.fetchAll()

    expect(store.items).toHaveLength(2)
    expect(store.budgetedCategoryIds.has('c1')).toBe(true)
    expect(store.budgetedCategoryIds.has('c2')).toBe(true)
  })

  it('keeps the list sorted by percentage after creating', async () => {
    mocked.list.mockResolvedValue([budget({ id: 'b1', categoryId: 'c1', percentage: 30 })])
    const store = useBudgetsStore()
    await store.fetchAll()

    mocked.create.mockResolvedValue(budget({ id: 'b2', categoryId: 'c2', percentage: 90, status: 'warning' }))
    await store.create({ categoryId: 'c2', amount: 100 })

    expect(store.items.map((b) => b.id)).toEqual(['b2', 'b1'])
  })

  it('replaces an updated budget in place', async () => {
    mocked.list.mockResolvedValue([budget({ id: 'b1', amount: 100 })])
    const store = useBudgetsStore()
    await store.fetchAll()

    mocked.update.mockResolvedValue(budget({ id: 'b1', amount: 300, percentage: 17 }))
    await store.update('b1', { amount: 300 })

    expect(store.items[0].amount).toBe(300)
  })

  it('removes a budget', async () => {
    mocked.list.mockResolvedValue([budget({ id: 'b1' })])
    const store = useBudgetsStore()
    await store.fetchAll()

    mocked.remove.mockResolvedValue()
    await store.remove('b1')

    expect(store.items).toHaveLength(0)
  })
})
