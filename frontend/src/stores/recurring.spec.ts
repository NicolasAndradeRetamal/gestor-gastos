import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { recurringService } from '@/services/recurring.service'
import { useRecurringStore } from '@/stores/recurring'
import type { RecurringExpense } from '@/types'

vi.mock('@/services/recurring.service', () => ({
  recurringService: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    remove: vi.fn(),
  },
}))

const mocked = vi.mocked(recurringService)

function template(overrides: Partial<RecurringExpense> = {}): RecurringExpense {
  return {
    id: 'r1',
    categoryId: 'c1',
    categoryName: 'Suscripciones',
    color: '#A855F7',
    amount: 9.99,
    dayOfMonth: 5,
    note: 'Streaming',
    nextRunOn: '2026-08-05',
    ...overrides,
  }
}

describe('recurring store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('loads templates', async () => {
    mocked.list.mockResolvedValue([template()])
    const store = useRecurringStore()

    await store.fetchAll()

    expect(store.items).toHaveLength(1)
  })

  it('keeps the list sorted by next run date after creating', async () => {
    mocked.list.mockResolvedValue([template({ id: 'r1', nextRunOn: '2026-08-20' })])
    const store = useRecurringStore()
    await store.fetchAll()

    mocked.create.mockResolvedValue(template({ id: 'r2', nextRunOn: '2026-08-03' }))
    await store.create({ categoryId: 'c2', amount: 5, dayOfMonth: 3 })

    expect(store.items.map((t) => t.id)).toEqual(['r2', 'r1'])
  })

  it('removes a template', async () => {
    mocked.list.mockResolvedValue([template({ id: 'r1' })])
    const store = useRecurringStore()
    await store.fetchAll()

    mocked.remove.mockResolvedValue()
    await store.remove('r1')

    expect(store.items).toHaveLength(0)
  })
})
