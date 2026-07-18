<template>
  <div class="w-full md:w-auto">
    <label :for="id" class="sr-only">Rango de fechas</label>
    <div class="relative w-full md:w-48">
      <select
        :id="id"
        :value="modelValue"
        class="h-11 w-full appearance-none rounded-md border border-line bg-surface-raised px-3 pr-9 text-sm font-medium text-ink transition-colors hover:border-ink-muted focus-visible:border-primary"
        @change="onChange"
      >
        <option v-for="preset in DATE_RANGE_PRESETS" :key="preset.value" :value="preset.value">
          {{ preset.label }}
        </option>
      </select>
      <AppIcon name="chevron-down" class="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-ink-muted" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'

import AppIcon from '@/components/base/AppIcon.vue'
import { DATE_RANGE_PRESETS, type DateRangePreset } from '@/composables/useDateRangePresets'

defineProps<{
  modelValue: DateRangePreset
}>()

const emit = defineEmits<{
  'update:modelValue': [value: DateRangePreset]
}>()

const id = useId()

function onChange(event: Event): void {
  emit('update:modelValue', (event.target as HTMLSelectElement).value as DateRangePreset)
}
</script>
