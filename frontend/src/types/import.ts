export interface ImportPreviewRow {
  rowNumber: number
  spentAt: string | null
  categoryName: string | null
  categoryId: string | null
  amount: number | null
  note: string | null
  valid: boolean
  errors: string[]
}

export interface ImportPreview {
  rows: ImportPreviewRow[]
  validCount: number
  invalidCount: number
}

export interface ImportConfirmRow {
  spentAt: string
  categoryId: string
  amount: number
  note: string | null
}

export interface ImportResult {
  imported: number
}
