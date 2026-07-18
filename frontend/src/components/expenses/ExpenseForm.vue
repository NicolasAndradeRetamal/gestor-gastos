<template>
  <Modal :open="open" :title="isEditing ? 'Editar gasto' : 'Añadir gasto'" :busy="busy" @close="emit('close')">
    <p v-if="bannerMessage" role="alert" class="rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
      {{ bannerMessage }}
    </p>
    <form id="expense-form" class="space-y-4" novalidate @submit.prevent="onSubmit">
      <BaseInput
        v-model="amount"
        type="number"
        label="Monto"
        required
        placeholder="0.00"
        inputmode="decimal"
        min="0.01"
        step="0.01"
        :error="fieldError('amount')"
      />
      <BaseInput
        v-model="spentAt"
        type="date"
        label="Fecha"
        required
        :max="todayIso"
        :error="fieldError('spentAt')"
      />
      <BaseSelect v-model="categoryId" label="Categoría" required :error="fieldError('categoryId')">
        <option value="" disabled>Selecciona una categoría</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </BaseSelect>
      <BaseTextarea v-model="note" label="Nota" placeholder="Opcional" :maxlength="500" :error="fieldError('note')" />
    </form>
    <template #footer>
      <BaseButton variant="secondary" :disabled="busy" @click="emit('close')">Cancelar</BaseButton>
      <BaseButton type="submit" form="expense-form" :loading="busy" loading-label="Guardando…">
        {{ isEditing ? 'Guardar cambios' : 'Guardar gasto' }}
      </BaseButton>
    </template>
  </Modal>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseTextarea from '@/components/base/BaseTextarea.vue'
import Modal from '@/components/base/Modal.vue'
import type { Category, Expense, ExpenseRequest, ProblemDetails } from '@/types'

const props = defineProps<{
  open: boolean
  categories: Category[]
  expense: Expense | null
  busy: boolean
  error: ProblemDetails | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: ExpenseRequest]
}>()

const amount = ref('')
const spentAt = ref('')
const categoryId = ref('')
const note = ref('')

const isEditing = computed(() => Boolean(props.expense))
const todayIso = new Date().toISOString().slice(0, 10)

function resetForm(): void {
  amount.value = props.expense ? String(props.expense.amount) : ''
  spentAt.value = props.expense?.spentAt ?? todayIso
  categoryId.value = props.expense?.category.id ?? ''
  note.value = props.expense?.note ?? ''
}

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) resetForm()
  },
  { immediate: true },
)

function fieldError(field: string): string | undefined {
  return props.error?.errors?.[field]?.[0]
}

const bannerMessage = computed(() => {
  const error = props.error
  if (!error || error.errors) return null
  return error.detail ?? 'No se pudo guardar el gasto.'
})

function onSubmit(): void {
  emit('submit', {
    amount: Number(amount.value),
    spentAt: spentAt.value,
    categoryId: categoryId.value,
    note: note.value || undefined,
  })
}
</script>
