import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import { categoriesService } from '@/services/categories.service'
import { toProblemDetails } from '@/services/http'
import type { Category, CategoryRequest, ProblemDetails } from '@/types'

export const useCategoriesStore = defineStore('categories', () => {
  const items = ref<Category[]>([])
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)
  const loaded = ref(false)

  const defaultCategories = computed(() => items.value.filter((category) => category.isDefault))
  const customCategories = computed(() => items.value.filter((category) => !category.isDefault))

  async function fetchAll(force = false): Promise<void> {
    if (loaded.value && !force) return
    isLoading.value = true
    error.value = null
    try {
      items.value = await categoriesService.list()
      loaded.value = true
    } catch (err) {
      error.value = toProblemDetails(err)
    } finally {
      isLoading.value = false
    }
  }

  async function create(payload: CategoryRequest): Promise<Category | null> {
    try {
      const created = await categoriesService.create(payload)
      items.value = [...items.value, created]
      return created
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function update(id: string, payload: CategoryRequest): Promise<Category | null> {
    try {
      const updated = await categoriesService.update(id, payload)
      items.value = items.value.map((category) => (category.id === id ? updated : category))
      return updated
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  async function remove(id: string): Promise<void> {
    try {
      await categoriesService.remove(id)
      items.value = items.value.filter((category) => category.id !== id)
    } catch (err) {
      throw toProblemDetails(err)
    }
  }

  return {
    items,
    isLoading,
    error,
    defaultCategories,
    customCategories,
    fetchAll,
    create,
    update,
    remove,
  }
})
