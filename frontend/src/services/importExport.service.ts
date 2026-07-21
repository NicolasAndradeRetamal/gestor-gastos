import { httpClient } from '@/services/http'
import type { ImportConfirmRow, ImportPreview, ImportResult } from '@/types'

export interface ExportFilters {
  from?: string
  to?: string
  categoryId?: string
}

export const importExportService = {
  async preview(file: File): Promise<ImportPreview> {
    const form = new FormData()
    form.append('file', file)
    const { data } = await httpClient.post<ImportPreview>('/expenses/import/preview', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data
  },

  async confirm(rows: ImportConfirmRow[]): Promise<ImportResult> {
    const { data } = await httpClient.post<ImportResult>('/expenses/import', { rows })
    return data
  },

  async exportCsv(filters: ExportFilters): Promise<Blob> {
    const { data } = await httpClient.get('/expenses/export', {
      params: filters,
      responseType: 'blob',
    })
    return data as Blob
  },
}
