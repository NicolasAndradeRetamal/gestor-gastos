<template>
  <div>
    <label :for="id" class="mb-1.5 block text-sm font-medium text-ink">
      {{ label }}
      <span v-if="required" class="text-danger" aria-hidden="true">*</span>
    </label>
    <input
      :id="id"
      v-bind="$attrs"
      :type="type ?? 'text'"
      :value="modelValue"
      :required="required"
      :disabled="disabled"
      :placeholder="placeholder"
      :min="min"
      :max="max"
      :step="step"
      :inputmode="inputmode"
      :aria-invalid="hasError || undefined"
      :aria-describedby="describedBy"
      class="h-11 w-full rounded-md border bg-surface-raised px-3 text-sm text-ink placeholder:text-ink-muted transition-colors hover:border-ink-muted disabled:cursor-not-allowed disabled:bg-surface-sunken disabled:text-ink-muted"
      :class="hasError ? 'border-danger focus-visible:outline-danger' : 'border-line focus-visible:border-primary'"
      @input="onInput"
    />
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
  modelValue: string | number
  label: string
  type?: string
  required?: boolean
  disabled?: boolean
  placeholder?: string
  help?: string
  error?: string
  min?: string | number
  max?: string | number
  step?: string | number
  inputmode?: 'text' | 'decimal' | 'numeric'
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const id = useId()
const helpId = `${id}-help`
const errorId = `${id}-error`

const hasError = computed(() => Boolean(props.error))
const describedBy = computed(() => (hasError.value ? errorId : props.help ? helpId : undefined))

function onInput(event: Event): void {
  emit('update:modelValue', (event.target as HTMLInputElement).value)
}
</script>
