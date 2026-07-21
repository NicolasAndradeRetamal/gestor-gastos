<template>
  <div class="space-y-6">
    <h1 class="text-2xl font-bold tracking-tight text-ink">Importar gastos</h1>

    <template v-if="!preview">
      <BaseCard>
        <label
          class="flex cursor-pointer flex-col items-center gap-3 rounded-lg border border-dashed border-line p-8 text-center transition-colors hover:border-ink-muted"
        >
          <AppIcon name="upload" class="size-8 text-ink-muted" />
          <span class="text-sm font-medium text-ink">Arrastra tu archivo CSV o selecciónalo</span>
          <span class="text-xs text-ink-muted">Formato: fecha, categoria, monto, nota</span>
          <input type="file" accept=".csv,text/csv" class="sr-only" :disabled="busy" @change="onFileSelected" />
        </label>
        <p v-if="loadError" role="alert" class="mt-4 rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
          {{ loadError }}
        </p>
        <div class="mt-4 flex items-center justify-between gap-3">
          <button type="button" class="text-sm font-semibold text-primary" @click="downloadTemplate">
            Descargar plantilla de ejemplo
          </button>
          <span v-if="busy" class="text-sm text-ink-muted" role="status">Procesando…</span>
        </div>
      </BaseCard>
    </template>

    <template v-else>
      <div class="flex flex-wrap items-center gap-3">
        <span class="inline-flex h-7 items-center gap-1.5 rounded-full bg-success-soft px-2.5 text-xs font-medium text-ink">
          <AppIcon name="check" class="size-4 text-success" />
          {{ preview.validCount }} válidas
        </span>
        <span
          v-if="preview.invalidCount > 0"
          class="inline-flex h-7 items-center gap-1.5 rounded-full bg-danger-soft px-2.5 text-xs font-medium text-danger"
        >
          <AppIcon name="alert-circle" class="size-4" />
          {{ preview.invalidCount }} con errores
        </span>
      </div>

      <BaseCard class="overflow-hidden !p-0">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-line text-left text-xs text-ink-muted">
                <th class="p-3 font-medium">#</th>
                <th class="p-3 font-medium">Fecha</th>
                <th class="p-3 font-medium">Categoría</th>
                <th class="p-3 font-medium">Monto</th>
                <th class="p-3 font-medium">Nota</th>
                <th class="p-3 font-medium">Estado</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in preview.rows"
                :key="row.rowNumber"
                class="border-b border-line/60"
                :class="row.valid ? '' : 'bg-danger-soft/50'"
              >
                <td class="p-3 text-ink-muted tabular-nums">{{ row.rowNumber }}</td>
                <td class="p-3 text-ink">{{ row.spentAt ?? '—' }}</td>
                <td class="p-3 text-ink">{{ row.categoryName ?? '—' }}</td>
                <td class="p-3 tabular-nums text-ink">{{ row.amount ?? '—' }}</td>
                <td class="max-w-40 truncate p-3 text-ink-muted">{{ row.note ?? '—' }}</td>
                <td class="p-3">
                  <span v-if="row.valid" class="inline-flex items-center gap-1 text-success">
                    <AppIcon name="check" class="size-4" /> Válida
                  </span>
                  <span v-else class="flex items-start gap-1 text-xs font-medium text-danger">
                    <AppIcon name="alert-circle" class="mt-0.5 size-3.5 shrink-0" />
                    <span>{{ row.errors.join(' ') }}</span>
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </BaseCard>

      <div class="flex items-center justify-end gap-3">
        <BaseButton variant="secondary" :disabled="busy" @click="reset">Cancelar</BaseButton>
        <BaseButton :disabled="preview.validCount === 0" :loading="busy" loading-label="Importando…" @click="confirmImport">
          Importar {{ preview.validCount }} gastos
        </BaseButton>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

import AppIcon from '@/components/base/AppIcon.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import { importExportService } from '@/services/importExport.service'
import { toProblemDetails } from '@/services/http'
import { useToastStore } from '@/stores/toast'
import type { ImportConfirmRow, ImportPreview } from '@/types'

const router = useRouter()
const toast = useToastStore()

const preview = ref<ImportPreview | null>(null)
const busy = ref(false)
const loadError = ref<string | null>(null)

async function onFileSelected(event: Event): Promise<void> {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  busy.value = true
  loadError.value = null
  try {
    preview.value = await importExportService.preview(file)
  } catch (err) {
    loadError.value = toProblemDetails(err).detail ?? 'No se pudo leer el archivo.'
  } finally {
    busy.value = false
  }
}

async function confirmImport(): Promise<void> {
  if (!preview.value) return
  const rows: ImportConfirmRow[] = preview.value.rows
    .filter((row) => row.valid)
    .map((row) => ({
      spentAt: row.spentAt!,
      categoryId: row.categoryId!,
      amount: row.amount!,
      note: row.note,
    }))

  busy.value = true
  try {
    const result = await importExportService.confirm(rows)
    toast.success(`Se importaron ${result.imported} gastos.`)
    router.push({ name: 'expenses' })
  } catch (err) {
    toast.error(toProblemDetails(err).detail ?? 'No se pudieron importar los gastos.')
  } finally {
    busy.value = false
  }
}

function reset(): void {
  preview.value = null
  loadError.value = null
}

function downloadTemplate(): void {
  const sample = 'fecha,categoria,monto,nota\n2026-01-15,Comida,42.50,Almuerzo\n'
  const url = URL.createObjectURL(new Blob([sample], { type: 'text/csv' }))
  const link = document.createElement('a')
  link.href = url
  link.download = 'plantilla-gastos.csv'
  link.click()
  URL.revokeObjectURL(url)
}
</script>
