export interface Category {
  id: string
  name: string
  color: string
  icon?: string | null
  isDefault: boolean
}

export interface CategoryRequest {
  name: string
  color: string
  icon?: string
}
