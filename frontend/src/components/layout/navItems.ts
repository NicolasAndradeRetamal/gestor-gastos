import type { IconName } from '@/components/base/AppIcon.vue'

export interface NavItem {
  name: string
  label: string
  icon: IconName
}

export const NAV_ITEMS: NavItem[] = [
  { name: 'dashboard', label: 'Resumen', icon: 'dashboard' },
  { name: 'expenses', label: 'Gastos', icon: 'expenses' },
  { name: 'budgets', label: 'Presupuestos', icon: 'wallet' },
  { name: 'categories', label: 'Categorías', icon: 'categories' },
]
