import { httpClient } from '@/services/http'
import type { DashboardParams, DashboardSummary } from '@/types'

export const dashboardService = {
  async summary(params: DashboardParams): Promise<DashboardSummary> {
    const { data } = await httpClient.get<DashboardSummary>('/dashboard/summary', { params })
    return data
  },
}
