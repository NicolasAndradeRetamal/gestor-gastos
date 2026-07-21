<template>
  <Modal :open="open" :title="isEditing ? 'Editar plantilla' : 'Nueva plantilla'" :busy="busy" @close="emit('close')">
    <p v-if="bannerMessage" role="alert" class="rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger">
      {{ bannerMessage }}
    </p>
    <form id="recurring-form" class="space-y-4" novalidate @submit.prevent="onSubmit">
      <BaseSelect v-model="categoryId" label="Categoría" required :error="fieldError('categoryId')">
        <option value="" disabled>Selecciona una categoría</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option>
      </BaseSelect>
      <BaseInput
        v-model="amount"
        type="number"
        label="Monto"
        required
        inputmode="decimal"
        min="0"
        step="0.01"
        placeholder="0.00"
        :error="fieldError('amount')"
      />
      <BaseSelect
        v-model="dayOfMonth"
        label="Día del mes"
        required
        help="Si el mes no llega a ese día, se registra el último día del mes."
        :error="fieldError('dayOfMonth')"
      >
        <option v-for="day in 31" :key="day" :value="String(day)">{{ day }}</option>
      </BaseSelect>
      <BaseTextarea v-model="note" label="Nota" :maxlength="500" :error="fieldError('note')" />
    </form>
    <template #footer>
      <BaseButton variant="secondary" :disabled="busy" @click="emit('close')">Cancelar</BaseButton>
      <BaseButton type="submit" form="recurring-form" :loading="busy" loading-label="Guardando…">
        {{ isEditing ? 'Guardar cambios' : 'Guardar plantilla' }}
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
import type { Category, ProblemDetails, RecurringExpense, RecurringExpenseRequest } from '@/types'

const props = defineProps<{
  open: boolean
  template: RecurringExpense | null
  categories: Category[]
  busy: boolean
  error: ProblemDetails | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: RecurringExpenseRequest]
}>()

const categoryId = ref('')
const amount = ref('')
const dayOfMonth = ref('1')
const note = ref('')

const isEditing = computed(() => Boolean(props.template))

watch(
  () => props.open,
  (isOpen) => {
    if (!isOpen) return
    categoryId.value = props.template?.categoryId ?? ''
    amount.value = props.template ? String(props.template.amount) : ''
    dayOfMonth.value = props.template ? String(props.template.dayOfMonth) : '1'
    note.value = props.template?.note ?? ''
  },
)

function fieldError(field: string): string | undefined {
  return props.error?.errors?.[field]?.[0]
}

const bannerMessage = computed(() => {
  const error = props.error
  if (!error || error.errors) return null
  return error.detail ?? 'No se pudo guardar la plantilla.'
})

function onSubmit(): void {
  emit('submit', {
    categoryId: categoryId.value,
    amount: Number(amount.value),
    dayOfMonth: Number(dayOfMonth.value),
    note: note.value.trim() ? note.value.trim() : null,
  })
}
</script>
