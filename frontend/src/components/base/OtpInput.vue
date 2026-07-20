<template>
  <div>
    <label :for="id" class="mb-1.5 block text-sm font-medium text-ink">
      {{ label }}
      <span v-if="required" class="text-danger" aria-hidden="true">*</span>
    </label>
    <input
      :id="id"
      ref="inputRef"
      type="text"
      :value="modelValue"
      :inputmode="variant === 'totp' ? 'numeric' : 'text'"
      autocomplete="one-time-code"
      :pattern="variant === 'totp' ? '\\d{6}' : undefined"
      :maxlength="variant === 'totp' ? 6 : 11"
      :placeholder="variant === 'totp' ? '000000' : 'XXXXX-XXXXX'"
      :disabled="disabled"
      :aria-invalid="hasError || undefined"
      :aria-describedby="describedBy"
      class="h-14 w-full rounded-md border bg-surface-raised text-center text-2xl font-semibold tabular-nums text-ink transition-colors hover:border-ink-muted disabled:cursor-not-allowed disabled:bg-surface-sunken disabled:text-ink-muted"
      :class="[
        hasError ? 'border-danger focus-visible:outline-danger' : 'border-line focus-visible:border-primary',
        variant === 'totp' ? 'tracking-[0.5em]' : 'tracking-wider uppercase',
      ]"
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
import { computed, nextTick, onMounted, useId, useTemplateRef, watch } from 'vue'

import AppIcon from '@/components/base/AppIcon.vue'

const props = withDefaults(
  defineProps<{
    modelValue: string
    label: string
    variant?: 'totp' | 'recovery'
    required?: boolean
    disabled?: boolean
    help?: string
    error?: string
    autofocus?: boolean
  }>(),
  { variant: 'totp' },
)

const emit = defineEmits<{
  'update:modelValue': [value: string]
  // Emitted when a TOTP reaches its full 6 digits, enabling optional auto-submit.
  complete: []
}>()

const id = useId()
const helpId = `${id}-help`
const errorId = `${id}-error`
const inputRef = useTemplateRef<HTMLInputElement>('inputRef')

const hasError = computed(() => Boolean(props.error))
const describedBy = computed(() => (hasError.value ? errorId : props.help ? helpId : undefined))

function normalize(raw: string): string {
  return props.variant === 'totp' ? raw.replace(/\D/g, '').slice(0, 6) : raw.toUpperCase().slice(0, 11)
}

function onInput(event: Event): void {
  const value = normalize((event.target as HTMLInputElement).value)
  emit('update:modelValue', value)
  if (props.variant === 'totp' && value.length === 6) emit('complete')
}

onMounted(() => {
  if (props.autofocus) inputRef.value?.focus()
})

// After an error the parent clears the value; refocus so the user can retry.
watch(
  () => props.error,
  async (next) => {
    if (next) {
      await nextTick()
      inputRef.value?.focus()
    }
  },
)
</script>
