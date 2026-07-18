<template>
  <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end">
    <BaseInput v-model="from" type="date" label="Desde" :max="to || undefined" @update:model-value="emitChange" />
    <BaseInput v-model="to" type="date" label="Hasta" :min="from || undefined" @update:model-value="emitChange" />
    <div class="sm:w-56">
      <BaseSelect v-model="categoryId" label="Categoría" @update:model-value="emitChange">
        <option value="">Todas</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </BaseSelect>
    </div>
    <BaseButton v-if="hasActiveFilters" variant="ghost" class="sm:mb-0.5" @click="clear">
      Limpiar filtros
    </BaseButton>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import type { Category } from '@/types'

export interface ExpenseFilterValue {
  from: string
  to: string
  categoryId: string
}

const props = defineProps<{
  modelValue: ExpenseFilterValue
  categories: Category[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: ExpenseFilterValue]
}>()

const from = ref(props.modelValue.from)
const to = ref(props.modelValue.to)
const categoryId = ref(props.modelValue.categoryId)

watch(
  () => props.modelValue,
  (value) => {
    from.value = value.from
    to.value = value.to
    categoryId.value = value.categoryId
  },
)

const hasActiveFilters = computed(() => Boolean(from.value || to.value || categoryId.value))

function emitChange(): void {
  emit('update:modelValue', { from: from.value, to: to.value, categoryId: categoryId.value })
}

function clear(): void {
  from.value = ''
  to.value = ''
  categoryId.value = ''
  emitChange()
}
</script>
