<template>
  <Modal :open="open" :title="isEditing ? 'Editar presupuesto' : 'Crear presupuesto'" :busy="busy" @close="emit('close')">
    <p v-if="bannerMessage" role="alert" class="rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
      {{ bannerMessage }}
    </p>
    <form id="budget-form" class="space-y-4" novalidate @submit.prevent="onSubmit">
      <div v-if="isEditing">
        <span class="mb-1.5 block text-sm font-medium text-ink">Categoría</span>
        <div class="flex h-11 items-center rounded-md border border-line bg-surface-sunken px-3">
          <CategoryBadge :name="budget!.categoryName" :color="budget!.color" size="sm" />
        </div>
      </div>
      <BaseSelect
        v-else
        v-model="categoryId"
        label="Categoría"
        required
        :error="fieldError('categoryId')"
      >
        <option value="" disabled>Selecciona una categoría</option>
        <option v-for="category in availableCategories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </BaseSelect>
      <BaseInput
        v-model="amount"
        type="number"
        label="Límite mensual"
        required
        inputmode="decimal"
        min="0"
        step="0.01"
        placeholder="0.00"
        help="El importe máximo que quieres gastar en esta categoría cada mes."
        :error="fieldError('amount')"
      />
    </form>
    <template #footer>
      <BaseButton variant="secondary" :disabled="busy" @click="emit('close')">Cancelar</BaseButton>
      <BaseButton type="submit" form="budget-form" :loading="busy" loading-label="Guardando…">
        {{ isEditing ? 'Guardar cambios' : 'Guardar presupuesto' }}
      </BaseButton>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import CategoryBadge from '@/components/base/CategoryBadge.vue'
import Modal from '@/components/base/Modal.vue'
import type { Budget, Category, ProblemDetails } from '@/types'

const props = defineProps<{
  open: boolean
  budget: Budget | null
  availableCategories: Category[]
  busy: boolean
  error: ProblemDetails | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: { categoryId: string; amount: number }]
}>()

const categoryId = ref('')
const amount = ref('')

const isEditing = computed(() => Boolean(props.budget))

watch(
  () => props.open,
  (isOpen) => {
    if (!isOpen) return
    categoryId.value = props.budget?.categoryId ?? ''
    amount.value = props.budget ? String(props.budget.amount) : ''
  },
)

function fieldError(field: string): string | undefined {
  if (field === 'categoryId' && props.error?.status === 409) {
    return 'Ya tienes un presupuesto para esta categoría.'
  }
  return props.error?.errors?.[field]?.[0]
}

const bannerMessage = computed(() => {
  const error = props.error
  if (!error || error.errors || error.status === 409) return null
  return error.detail ?? 'No se pudo guardar el presupuesto.'
})

function onSubmit(): void {
  emit('submit', { categoryId: categoryId.value, amount: Number(amount.value) })
}
</script>
