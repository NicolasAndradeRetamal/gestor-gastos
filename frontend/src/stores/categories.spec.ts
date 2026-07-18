import { createPinia, setActivePinia } from 'pinia'
import { AxiosError, AxiosHeaders } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { categoriesService } from '@/services/categories.service'
import { useCategoriesStore } from '@/stores/categories'
import type { Category } from '@/types'

vi.mock('@/services/categories.service', () => ({
  categoriesService: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    remove: vi.fn(),
  },
}))

const mockedCategoriesService = vi.mocked(categoriesService)

const defaultCategory: Category = { id: 'c1', name: 'Comida', color: '#F97316', isDefault: true }
const customCategory: Category = { id: 'c2', name: 'Suscripciones', color: '#3B82F6', isDefault: false }

describe('categories store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('splits default and custom categories', async () => {
    mockedCategoriesService.list.mockResolvedValue([defaultCategory, customCategory])
    const store = useCategoriesStore()

    await store.fetchAll()

    expect(store.defaultCategories).toEqual([defaultCategory])
    expect(store.customCategories).toEqual([customCategory])
  })

  it('does not refetch once loaded unless forced', async () => {
    mockedCategoriesService.list.mockResolvedValue([defaultCategory])
    const store = useCategoriesStore()

    await store.fetchAll()
    await store.fetchAll()

    expect(mockedCategoriesService.list).toHaveBeenCalledTimes(1)

    await store.fetchAll(true)
    expect(mockedCategoriesService.list).toHaveBeenCalledTimes(2)
  })

  it('rejects with a 409 ProblemDetails when deleting a category with expenses', async () => {
    mockedCategoriesService.list.mockResolvedValue([customCategory])
    const conflict = new AxiosError('Conflict', '409', undefined, undefined, {
      status: 409,
      statusText: 'Conflict',
      headers: new AxiosHeaders(),
      config: { headers: new AxiosHeaders() },
      data: { title: 'Conflict', status: 409 },
    })
    mockedCategoriesService.remove.mockRejectedValue(conflict)
    const store = useCategoriesStore()
    await store.fetchAll()

    await expect(store.remove('c2')).rejects.toMatchObject({ status: 409 })
    expect(store.customCategories).toEqual([customCategory])
  })
})
