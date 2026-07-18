import { defineStore } from 'pinia'
import { ref } from 'vue'

import { dashboardService } from '@/services/dashboard.service'
import { toProblemDetails } from '@/services/http'
import type { DashboardParams, DashboardSummary, ProblemDetails } from '@/types'

export const useDashboardStore = defineStore('dashboard', () => {
  const summary = ref<DashboardSummary | null>(null)
  const filters = ref<DashboardParams>({})
  const isLoading = ref(false)
  const error = ref<ProblemDetails | null>(null)

  function setFilters(next: DashboardParams): void {
    filters.value = next
  }

  async function fetchSummary(): Promise<void> {
    isLoading.value = true
    error.value = null
    try {
      summary.value = await dashboardService.summary(filters.value)
    } catch (err) {
      error.value = toProblemDetails(err)
    } finally {
      isLoading.value = false
    }
  }

  return { summary, filters, isLoading, error, setFilters, fetchSummary }
})
