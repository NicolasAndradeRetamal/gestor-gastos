<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <h1 class="text-2xl font-bold tracking-tight text-ink">Gastos recurrentes</h1>
      <BaseButton class="w-full md:w-auto" icon="plus" @click="openCreate">Nueva plantilla</BaseButton>
    </div>

    <p class="text-sm text-ink-muted">
      Estas plantillas se registran como un gasto automáticamente en su día del mes.
    </p>

    <ErrorState v-if="recurringStore.error" :message="recurringStore.error.detail" @retry="load" />

    <div v-else-if="recurringStore.isLoading && recurringStore.items.length === 0" class="grid gap-3 sm:grid-cols-2" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton v-for="n in 3" :key="n" class="h-28" />
    </div>

    <EmptyState
      v-else-if="recurringStore.items.length === 0"
      icon="expenses"
      title="Todavía no tienes gastos recurrentes"
      description="Crea una plantilla para tus gastos fijos y suscripciones."
      action-label="Nueva plantilla"
      @action="openCreate"
    />

    <div v-else class="grid gap-3 sm:grid-cols-2">
      <BaseCard v-for="template in recurringStore.items" :key="template.id">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1 space-y-1">
            <div class="flex items-center justify-between gap-2">
              <CategoryBadge :name="template.categoryName" :color="template.color" size="sm" />
              <span class="text-sm font-semibold tabular-nums text-ink">{{ formatAmount(template.amount) }}</span>
            </div>
            <p v-if="template.note" class="truncate text-sm text-ink">{{ template.note }}</p>
            <p class="text-xs text-ink-muted">
              Día {{ template.dayOfMonth }} de cada mes · Próximo: {{ formatShortDate(template.nextRunOn) }}
            </p>
          </div>
          <div class="flex shrink-0 gap-1">
            <BaseButton variant="ghost-muted" size="compact" icon-only icon="edit" aria-label="Editar plantilla" @click="openEdit(template)">
              Editar
            </BaseButton>
            <BaseButton variant="ghost-danger" size="compact" icon-only icon="trash" aria-label="Eliminar plantilla" @click="deleting = template">
              Eliminar
            </BaseButton>
          </div>
        </div>
      </BaseCard>
    </div>

    <RecurringExpenseForm
      :open="formOpen"
      :template="editing"
      :categories="categoriesStore.items"
      :busy="busy"
      :error="formError"
      @close="closeForm"
      @submit="onSubmit"
    />

    <ConfirmDialog
      :open="Boolean(deleting)"
      title="Eliminar plantilla"
      message="¿Eliminar esta plantilla? Los gastos que ya generó no se ven afectados."
      :busy="busy"
      @close="deleting = null"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import CategoryBadge from '@/components/base/CategoryBadge.vue'
import ConfirmDialog from '@/components/base/ConfirmDialog.vue'
import EmptyState from '@/components/base/EmptyState.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import RecurringExpenseForm from '@/components/recurring/RecurringExpenseForm.vue'
import { formatAmount, formatShortDate } from '@/composables/useFormat'
import { useCategoriesStore } from '@/stores/categories'
import { useRecurringStore } from '@/stores/recurring'
import { useToastStore } from '@/stores/toast'
import type { ProblemDetails, RecurringExpense, RecurringExpenseRequest } from '@/types'

const recurringStore = useRecurringStore()
const categoriesStore = useCategoriesStore()
const toast = useToastStore()

const formOpen = ref(false)
const editing = ref<RecurringExpense | null>(null)
const deleting = ref<RecurringExpense | null>(null)
const busy = ref(false)
const formError = ref<ProblemDetails | null>(null)

function load(): void {
  recurringStore.fetchAll()
  categoriesStore.fetchAll()
}

onMounted(load)

function openCreate(): void {
  editing.value = null
  formError.value = null
  formOpen.value = true
}

function openEdit(template: RecurringExpense): void {
  editing.value = template
  formError.value = null
  formOpen.value = true
}

function closeForm(): void {
  if (busy.value) return
  formOpen.value = false
}

async function onSubmit(payload: RecurringExpenseRequest): Promise<void> {
  busy.value = true
  formError.value = null
  try {
    if (editing.value) {
      await recurringStore.update(editing.value.id, payload)
      toast.success('Plantilla actualizada.')
    } else {
      await recurringStore.create(payload)
      toast.success('Plantilla creada.')
    }
    formOpen.value = false
  } catch (err) {
    formError.value = err as ProblemDetails
  } finally {
    busy.value = false
  }
}

async function confirmDelete(): Promise<void> {
  if (!deleting.value) return
  busy.value = true
  try {
    await recurringStore.remove(deleting.value.id)
    toast.success('Plantilla eliminada.')
    deleting.value = null
  } catch {
    toast.error('No se pudo eliminar la plantilla.')
  } finally {
    busy.value = false
  }
}
</script>
