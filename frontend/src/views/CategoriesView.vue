<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <h1 class="text-2xl font-bold tracking-tight text-ink">Categorías</h1>
      <BaseButton class="w-full md:w-auto" icon="plus" @click="openCreateModal">Crear categoría</BaseButton>
    </div>

    <ErrorState v-if="categoriesStore.error" :message="categoriesStore.error.detail" @retry="() => categoriesStore.fetchAll(true)" />

    <div v-else-if="categoriesStore.isLoading && categoriesStore.items.length === 0" class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton v-for="n in 6" :key="n" class="h-14" />
    </div>

    <template v-else>
      <section class="space-y-3">
        <h2 class="text-xs font-semibold uppercase tracking-wide text-ink-muted">Predefinidas</h2>
        <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <div
            v-for="category in categoriesStore.defaultCategories"
            :key="category.id"
            class="flex items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-4 py-3"
          >
            <CategoryBadge :name="category.name" :color="category.color" />
            <span class="text-xs text-ink-muted">Predefinida</span>
          </div>
        </div>
      </section>

      <section class="space-y-3">
        <h2 class="text-xs font-semibold uppercase tracking-wide text-ink-muted">Tus categorías</h2>
        <EmptyState
          v-if="categoriesStore.customCategories.length === 0"
          title="Aún no creaste categorías propias"
          description="Usa las predefinidas o crea una para tus gastos habituales."
          action-label="Crear categoría"
          @action="openCreateModal"
        />
        <div v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <div
            v-for="category in categoriesStore.customCategories"
            :key="category.id"
            class="flex items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-4 py-3"
          >
            <CategoryBadge :name="category.name" :color="category.color" />
            <div class="flex items-center gap-1">
              <BaseButton
                variant="ghost"
                size="compact"
                icon-only
                icon="edit"
                :aria-label="`Editar categoría ${category.name}`"
                @click="openEditModal(category)"
              >
                Editar
              </BaseButton>
              <BaseButton
                variant="ghost-danger"
                size="compact"
                icon-only
                icon="trash"
                :aria-label="`Eliminar categoría ${category.name}`"
                @click="openDeleteDialog(category)"
              >
                Eliminar
              </BaseButton>
            </div>
          </div>
        </div>
      </section>
    </template>

    <CategoryForm
      :open="isFormOpen"
      :category="editingCategory"
      :busy="isSubmitting"
      :error="formError"
      @close="closeFormModal"
      @submit="onSubmitCategory"
    />

    <ConfirmDialog
      :open="isDeleteDialogOpen"
      title="Eliminar categoría"
      message="¿Eliminar esta categoría? Esta acción no se puede deshacer."
      :busy="isDeleting"
      :blocked="isBlocked"
      blocked-message="Esta categoría tiene gastos asociados. Reasigna o elimina esos gastos antes de borrarla."
      @close="closeDeleteDialog"
      @confirm="onConfirmDelete"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import CategoryBadge from '@/components/base/CategoryBadge.vue'
import ConfirmDialog from '@/components/base/ConfirmDialog.vue'
import EmptyState from '@/components/base/EmptyState.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import CategoryForm from '@/components/categories/CategoryForm.vue'
import { useCategoriesStore } from '@/stores/categories'
import { useToastStore } from '@/stores/toast'
import type { Category, CategoryRequest, ProblemDetails } from '@/types'

const categoriesStore = useCategoriesStore()
const toastStore = useToastStore()

const isFormOpen = ref(false)
const editingCategory = ref<Category | null>(null)
const isSubmitting = ref(false)
const formError = ref<ProblemDetails | null>(null)

const isDeleteDialogOpen = ref(false)
const isDeleting = ref(false)
const isBlocked = ref(false)
const categoryToDelete = ref<Category | null>(null)

function openCreateModal(): void {
  editingCategory.value = null
  formError.value = null
  isFormOpen.value = true
}

function openEditModal(category: Category): void {
  editingCategory.value = category
  formError.value = null
  isFormOpen.value = true
}

function closeFormModal(): void {
  isFormOpen.value = false
}

async function onSubmitCategory(payload: CategoryRequest): Promise<void> {
  isSubmitting.value = true
  formError.value = null
  try {
    if (editingCategory.value) {
      await categoriesStore.update(editingCategory.value.id, payload)
      toastStore.success('Categoría actualizada.')
    } else {
      await categoriesStore.create(payload)
      toastStore.success('Categoría creada.')
    }
    isFormOpen.value = false
  } catch (err) {
    formError.value = err as ProblemDetails
    toastStore.error('No se pudo guardar la categoría.')
  } finally {
    isSubmitting.value = false
  }
}

function openDeleteDialog(category: Category): void {
  categoryToDelete.value = category
  isBlocked.value = false
  isDeleteDialogOpen.value = true
}

function closeDeleteDialog(): void {
  isDeleteDialogOpen.value = false
  isBlocked.value = false
}

async function onConfirmDelete(): Promise<void> {
  if (!categoryToDelete.value) return
  isDeleting.value = true
  try {
    await categoriesStore.remove(categoryToDelete.value.id)
    toastStore.success('Categoría eliminada.')
    isDeleteDialogOpen.value = false
  } catch (err) {
    const problem = err as ProblemDetails
    if (problem.status === 409) {
      isBlocked.value = true
    } else {
      toastStore.error(problem.detail ?? 'No se pudo eliminar la categoría.')
    }
  } finally {
    isDeleting.value = false
  }
}

onMounted(() => categoriesStore.fetchAll())
</script>
