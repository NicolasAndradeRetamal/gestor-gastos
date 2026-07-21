<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <h1 class="text-2xl font-bold tracking-tight text-ink">Presupuestos</h1>
      <BaseButton class="w-full md:w-auto" icon="plus" @click="openCreate">Crear presupuesto</BaseButton>
    </div>

    <ErrorState v-if="budgetsStore.error" :message="budgetsStore.error.detail" @retry="load" />

    <div v-else-if="budgetsStore.isLoading && budgetsStore.items.length === 0" class="space-y-3" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton v-for="n in 3" :key="n" class="h-24" />
    </div>

    <EmptyState
      v-else-if="budgetsStore.items.length === 0"
      icon="wallet"
      title="Todavía no tienes presupuestos"
      description="Crea uno para controlar tu gasto mensual por categoría."
      action-label="Crear presupuesto"
      @action="openCreate"
    />

    <div v-else class="grid gap-3 sm:grid-cols-2">
      <BaseCard v-for="budget in budgetsStore.items" :key="budget.id">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <BudgetProgress :budget="budget" />
          </div>
          <div class="flex shrink-0 gap-1">
            <BaseButton variant="ghost-muted" size="compact" icon-only icon="edit" aria-label="Editar presupuesto" @click="openEdit(budget)">
              Editar
            </BaseButton>
            <BaseButton variant="ghost-danger" size="compact" icon-only icon="trash" aria-label="Eliminar presupuesto" @click="askDelete(budget)">
              Eliminar
            </BaseButton>
          </div>
        </div>
      </BaseCard>
    </div>

    <BudgetForm
      :open="formOpen"
      :budget="editing"
      :available-categories="availableCategories"
      :busy="busy"
      :error="formError"
      @close="closeForm"
      @submit="onSubmit"
    />

    <ConfirmDialog
      :open="Boolean(deleting)"
      title="Eliminar presupuesto"
      message="¿Eliminar este presupuesto? El gasto registrado no se ve afectado."
      :busy="busy"
      @close="deleting = null"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BudgetProgress from '@/components/base/BudgetProgress.vue'
import ConfirmDialog from '@/components/base/ConfirmDialog.vue'
import EmptyState from '@/components/base/EmptyState.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import BudgetForm from '@/components/budgets/BudgetForm.vue'
import { useBudgetsStore } from '@/stores/budgets'
import { useCategoriesStore } from '@/stores/categories'
import { useToastStore } from '@/stores/toast'
import type { Budget, ProblemDetails } from '@/types'

const budgetsStore = useBudgetsStore()
const categoriesStore = useCategoriesStore()
const toast = useToastStore()

const formOpen = ref(false)
const editing = ref<Budget | null>(null)
const deleting = ref<Budget | null>(null)
const busy = ref(false)
const formError = ref<ProblemDetails | null>(null)

const availableCategories = computed(() =>
  categoriesStore.items.filter((category) => !budgetsStore.budgetedCategoryIds.has(category.id)),
)

function load(): void {
  budgetsStore.fetchAll(true)
  categoriesStore.fetchAll()
}

onMounted(load)

function openCreate(): void {
  editing.value = null
  formError.value = null
  formOpen.value = true
}

function openEdit(budget: Budget): void {
  editing.value = budget
  formError.value = null
  formOpen.value = true
}

function closeForm(): void {
  if (busy.value) return
  formOpen.value = false
}

async function onSubmit(payload: { categoryId: string; amount: number }): Promise<void> {
  busy.value = true
  formError.value = null
  try {
    if (editing.value) {
      await budgetsStore.update(editing.value.id, { amount: payload.amount })
      toast.success('Presupuesto actualizado.')
    } else {
      await budgetsStore.create(payload)
      toast.success('Presupuesto creado.')
    }
    formOpen.value = false
  } catch (err) {
    formError.value = err as ProblemDetails
  } finally {
    busy.value = false
  }
}

function askDelete(budget: Budget): void {
  deleting.value = budget
}

async function confirmDelete(): Promise<void> {
  if (!deleting.value) return
  busy.value = true
  try {
    await budgetsStore.remove(deleting.value.id)
    toast.success('Presupuesto eliminado.')
    deleting.value = null
  } catch {
    toast.error('No se pudo eliminar el presupuesto.')
  } finally {
    busy.value = false
  }
}
</script>
