<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <h1 class="text-2xl font-bold tracking-tight text-ink">Gastos</h1>
      <BaseButton class="w-full md:w-auto" icon="plus" @click="openCreateModal">Añadir gasto</BaseButton>
    </div>

    <ExpenseFilters v-model="filterValue" :categories="categoriesStore.items" />

    <ErrorState v-if="expensesStore.error" :message="expensesStore.error.detail" @retry="expensesStore.fetchList" />

    <ExpenseList
      v-else
      :items="expensesStore.items"
      :loading="expensesStore.isLoading"
      :page="expensesStore.page"
      :page-size="expensesStore.pageSize"
      :total-items="expensesStore.totalItems"
      :total-pages="expensesStore.totalPages"
      @edit="openEditModal"
      @delete="openDeleteDialog"
      @add="openCreateModal"
      @update:page="onPageChange"
    />

    <ExpenseForm
      :open="isFormOpen"
      :categories="categoriesStore.items"
      :expense="editingExpense"
      :busy="isSubmitting"
      :error="formError"
      @close="closeFormModal"
      @submit="onSubmitExpense"
    />

    <ConfirmDialog
      :open="isDeleteDialogOpen"
      title="Eliminar gasto"
      message="¿Eliminar este gasto? Esta acción no se puede deshacer."
      :busy="isDeleting"
      @close="isDeleteDialogOpen = false"
      @confirm="onConfirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import BaseButton from '@/components/base/BaseButton.vue'
import ConfirmDialog from '@/components/base/ConfirmDialog.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import ExpenseFilters, { type ExpenseFilterValue } from '@/components/expenses/ExpenseFilters.vue'
import ExpenseForm from '@/components/expenses/ExpenseForm.vue'
import ExpenseList from '@/components/expenses/ExpenseList.vue'
import { useCategoriesStore } from '@/stores/categories'
import { useExpensesStore } from '@/stores/expenses'
import { useToastStore } from '@/stores/toast'
import type { Expense, ExpenseRequest, ProblemDetails } from '@/types'

const route = useRoute()
const router = useRouter()
const expensesStore = useExpensesStore()
const categoriesStore = useCategoriesStore()
const toastStore = useToastStore()

const queryString = (value: unknown): string => (typeof value === 'string' ? value : '')

const filterValue = ref<ExpenseFilterValue>({
  from: queryString(route.query.from),
  to: queryString(route.query.to),
  categoryId: queryString(route.query.categoryId),
})

const isFormOpen = ref(false)
const editingExpense = ref<Expense | null>(null)
const isSubmitting = ref(false)
const formError = ref<ProblemDetails | null>(null)

const isDeleteDialogOpen = ref(false)
const isDeleting = ref(false)
const expenseToDelete = ref<Expense | null>(null)

function syncRouteAndFetch(): void {
  expensesStore.setFilters({
    from: filterValue.value.from || undefined,
    to: filterValue.value.to || undefined,
    categoryId: filterValue.value.categoryId || undefined,
  })
  router.replace({
    query: {
      ...(filterValue.value.from ? { from: filterValue.value.from } : {}),
      ...(filterValue.value.to ? { to: filterValue.value.to } : {}),
      ...(filterValue.value.categoryId ? { categoryId: filterValue.value.categoryId } : {}),
    },
  })
  expensesStore.fetchList()
}

watch(filterValue, syncRouteAndFetch, { deep: true })

function onPageChange(page: number): void {
  expensesStore.setPage(page)
  expensesStore.fetchList()
}

function openCreateModal(): void {
  editingExpense.value = null
  formError.value = null
  isFormOpen.value = true
}

function openEditModal(expense: Expense): void {
  editingExpense.value = expense
  formError.value = null
  isFormOpen.value = true
}

function closeFormModal(): void {
  isFormOpen.value = false
}

async function onSubmitExpense(payload: ExpenseRequest): Promise<void> {
  isSubmitting.value = true
  formError.value = null
  try {
    if (editingExpense.value) {
      await expensesStore.update(editingExpense.value.id, payload)
      toastStore.success('Gasto actualizado.')
    } else {
      await expensesStore.create(payload)
      toastStore.success('Gasto guardado.')
    }
    isFormOpen.value = false
  } catch (err) {
    formError.value = err as ProblemDetails
    toastStore.error('No se pudo guardar el gasto.')
  } finally {
    isSubmitting.value = false
  }
}

function openDeleteDialog(expense: Expense): void {
  expenseToDelete.value = expense
  isDeleteDialogOpen.value = true
}

async function onConfirmDelete(): Promise<void> {
  if (!expenseToDelete.value) return
  isDeleting.value = true
  try {
    await expensesStore.remove(expenseToDelete.value.id)
    toastStore.success('Gasto eliminado.')
    isDeleteDialogOpen.value = false
  } catch (err) {
    const problem = err as ProblemDetails
    toastStore.error(problem.detail ?? 'No se pudo eliminar el gasto.')
  } finally {
    isDeleting.value = false
  }
}

onMounted(() => {
  categoriesStore.fetchAll()
  syncRouteAndFetch()
})
</script>
