<template>
  <BaseCard>
    <div v-if="loading" class="space-y-2" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton v-for="n in 5" :key="n" class="h-16 md:h-12" />
    </div>

    <EmptyState
      v-else-if="items.length === 0"
      title="Sin gastos en este periodo"
      description="Ajusta el rango de fechas o el filtro de categoría, o añade un gasto nuevo."
      action-label="Añadir gasto"
      @action="emit('add')"
    />

    <template v-else>
      <ul class="space-y-2 md:hidden">
        <li v-for="expense in items" :key="expense.id">
          <button
            type="button"
            class="flex w-full items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-4 py-3 text-left"
            @click="emit('edit', expense)"
          >
            <span class="flex items-center gap-3">
              <span
                class="flex size-8 shrink-0 items-center justify-center rounded-full"
                :style="{ backgroundColor: `color-mix(in srgb, ${expense.category.color} 20%, transparent)` }"
                aria-hidden="true"
              >
                <span class="size-2.5 rounded-full" :style="{ backgroundColor: expense.category.color }" />
              </span>
              <span class="flex flex-col">
                <span class="text-sm font-medium text-ink">{{ expense.note || expense.category.name }}</span>
                <span class="text-xs text-ink-muted tabular-nums">{{ formatShortDate(expense.spentAt) }}</span>
              </span>
            </span>
            <span class="flex flex-col items-end">
              <span class="text-base font-bold text-ink tabular-nums">{{ formatAmount(expense.amount) }}</span>
              <span v-if="expense.note" class="text-xs text-ink-muted">{{ expense.category.name }}</span>
            </span>
          </button>
        </li>
      </ul>

      <table class="hidden w-full text-sm md:table">
        <thead>
          <tr class="border-b border-line">
            <th scope="col" class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-ink-muted">Fecha</th>
            <th scope="col" class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-ink-muted">Categoría</th>
            <th scope="col" class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-ink-muted">Nota</th>
            <th scope="col" class="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-ink-muted">Monto</th>
            <th scope="col" class="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-ink-muted">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="expense in items"
            :key="expense.id"
            class="border-b border-line transition-colors last:border-0 hover:bg-surface-sunken"
          >
            <td class="px-4 py-3 tabular-nums text-ink-muted">{{ formatFullDate(expense.spentAt) }}</td>
            <td class="px-4 py-3">
              <CategoryBadge :name="expense.category.name" :color="expense.category.color" size="sm" />
            </td>
            <td class="max-w-xs truncate px-4 py-3" :class="expense.note ? 'text-ink' : 'text-ink-muted'">
              {{ expense.note || '—' }}
            </td>
            <td class="px-4 py-3 text-right font-semibold tabular-nums text-ink">{{ formatAmount(expense.amount) }}</td>
            <td class="px-4 py-3">
              <div class="flex items-center justify-center gap-1">
                <BaseButton
                  variant="ghost"
                  size="compact"
                  icon-only
                  icon="edit"
                  :aria-label="`Editar gasto de ${formatAmount(expense.amount)}`"
                  @click="emit('edit', expense)"
                >
                  Editar
                </BaseButton>
                <BaseButton
                  variant="ghost-danger"
                  size="compact"
                  icon-only
                  icon="trash"
                  :aria-label="`Eliminar gasto de ${formatAmount(expense.amount)}`"
                  @click="emit('delete', expense)"
                >
                  Eliminar
                </BaseButton>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <Pagination
        :page="page"
        :page-size="pageSize"
        :total-items="totalItems"
        :total-pages="totalPages"
        @update:page="emit('update:page', $event)"
      />
    </template>
  </BaseCard>
</template>

<script setup lang="ts">
import BaseButton from '@/components/base/BaseButton.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import CategoryBadge from '@/components/base/CategoryBadge.vue'
import EmptyState from '@/components/base/EmptyState.vue'
import Pagination from '@/components/base/Pagination.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import { formatAmount, formatFullDate, formatShortDate } from '@/composables/useFormat'
import type { Expense } from '@/types'

defineProps<{
  items: Expense[]
  loading: boolean
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}>()

const emit = defineEmits<{
  edit: [expense: Expense]
  delete: [expense: Expense]
  add: []
  'update:page': [page: number]
}>()
</script>
