<template>
  <div>
    <label :for="id" class="mb-1.5 block text-sm font-medium text-ink">
      {{ label }}
      <span v-if="required" class="text-danger" aria-hidden="true">*</span>
    </label>
    <div class="relative">
      <select
        :id="id"
        v-bind="$attrs"
        :value="modelValue"
        :required="required"
        :disabled="disabled"
        :aria-invalid="hasError || undefined"
        :aria-describedby="describedBy"
        class="h-11 w-full appearance-none rounded-md border bg-surface-raised px-3 pr-9 text-sm text-ink transition-colors hover:border-ink-muted disabled:cursor-not-allowed disabled:bg-surface-sunken disabled:text-ink-muted"
        :class="hasError ? 'border-danger focus-visible:outline-danger' : 'border-line focus-visible:border-primary'"
        @change="onChange"
      >
        <slot />
      </select>
      <AppIcon name="chevron-down" class="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-ink-muted" />
    </div>
    <p v-if="help && !hasError" :id="helpId" class="mt-1.5 text-xs text-ink-muted">{{ help }}</p>
    <p v-if="hasError" :id="errorId" role="alert" class="mt-1.5 flex items-center gap-1 text-xs font-medium text-danger">
      <AppIcon name="alert-circle" class="size-3.5" />
      {{ error }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed, useId } from 'vue'

import AppIcon from '@/components/base/AppIcon.vue'

defineOptions({ inheritAttrs: false })

const props = defineProps<{
  modelValue: string
  label: string
  required?: boolean
  disabled?: boolean
  help?: string
  error?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const id = useId()
const helpId = `${id}-help`
const errorId = `${id}-error`

const hasError = computed(() => Boolean(props.error))
const describedBy = computed(() => (hasError.value ? errorId : props.help ? helpId : undefined))

function onChange(event: Event): void {
  emit('update:modelValue', (event.target as HTMLSelectElement).value)
}
</script>
