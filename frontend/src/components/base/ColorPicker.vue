<template>
  <div>
    <span class="mb-1.5 block text-sm font-medium text-ink" id="color-picker-label">{{ label }}</span>
    <div role="radiogroup" aria-labelledby="color-picker-label" class="flex flex-wrap gap-3">
      <button
        v-for="option in CATEGORY_COLORS"
        :key="option"
        type="button"
        role="radio"
        :aria-checked="option === modelValue"
        :aria-label="option"
        class="flex size-8 items-center justify-center rounded-full transition-shadow"
        :class="option === modelValue ? 'ring-2 ring-offset-2 ring-primary' : ''"
        :style="{ backgroundColor: option }"
        @click="emit('update:modelValue', option)"
      >
        <AppIcon v-if="option === modelValue" name="check" class="size-4 text-white" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '@/components/base/AppIcon.vue'
import { CATEGORY_COLORS } from '@/lib/categoryColors'

defineProps<{
  modelValue: string
  label: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>
