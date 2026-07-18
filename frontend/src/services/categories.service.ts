import { httpClient } from '@/services/http'
import type { Category, CategoryRequest } from '@/types'

export const categoriesService = {
  async list(): Promise<Category[]> {
    const { data } = await httpClient.get<Category[]>('/categories')
    return data
  },

  async create(payload: CategoryRequest): Promise<Category> {
    const { data } = await httpClient.post<Category>('/categories', payload)
    return data
  },

  async update(id: string, payload: CategoryRequest): Promise<Category> {
    const { data } = await httpClient.put<Category>(`/categories/${id}`, payload)
    return data
  },

  async remove(id: string): Promise<void> {
    await httpClient.delete(`/categories/${id}`)
  },
}
