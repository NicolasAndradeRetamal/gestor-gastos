<template>
  <button
    :type="props.type ?? 'button'"
    :class="classes"
    :disabled="props.disabled || props.loading"
    :aria-busy="props.loading || undefined"
  >
    <AppIcon v-if="props.icon && !props.loading" :name="props.icon" :class="iconClass" />
    <Spinner v-if="props.loading" :class="iconClass" :label="props.loadingLabel ?? 'Cargando…'" />
    <span :class="{ 'sr-only': props.iconOnly }">
      <slot />
    </span>
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'

import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import Spinner from '@/components/base/Spinner.vue'

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'ghost-danger' | 'ghost-muted'
type Size = 'default' | 'compact'

const props = defineProps<{
  variant?: Variant
  size?: Size
  type?: 'button' | 'submit' | 'reset'
  disabled?: boolean
  loading?: boolean
  loadingLabel?: string
  icon?: IconName
  iconOnly?: boolean
}>()

// No horizontal padding here: it's decided once in `paddingClass` so a button
// never carries two conflicting `px-*` utilities (Tailwind resolves those by
// source order, not class order, which would silently crush icon-only buttons).
const BASE =
  'inline-flex items-center justify-center gap-2 rounded-md text-sm font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50 active:scale-[0.98]'

const SIZE_CLASSES: Record<Size, string> = {
  default: 'h-11',
  compact: 'h-9',
}

const VARIANT_CLASSES: Record<Variant, string> = {
  primary: 'bg-primary text-white hover:bg-primary-strong',
  secondary: 'bg-surface-raised text-ink border border-line hover:bg-surface-sunken',
  danger: 'bg-danger text-white hover:bg-red-700 focus-visible:outline-danger',
  ghost: 'text-primary hover:bg-primary-soft disabled:hover:bg-transparent',
  'ghost-danger':
    'text-danger hover:bg-danger-soft disabled:hover:bg-transparent focus-visible:outline-danger',
  'ghost-muted': 'text-ink-muted hover:bg-surface-sunken disabled:hover:bg-transparent',
}

// Exactly one horizontal-padding utility per button.
const paddingClass = computed(() => {
  if (props.iconOnly) return 'px-0'
  if (props.variant?.startsWith('ghost')) return 'px-2'
  return props.size === 'compact' ? 'px-3' : 'px-4'
})

const classes = computed(() => [
  BASE,
  paddingClass.value,
  props.iconOnly
    ? (props.size === 'compact' ? 'size-8' : 'size-11') + ' shrink-0'
    : SIZE_CLASSES[props.size ?? 'default'],
  VARIANT_CLASSES[props.variant ?? 'primary'],
])

// Icon-only buttons use a bigger icon at the default 44px area (24px) than at
// the 32px compact variant (16px); buttons with a visible label always use 16px.
const iconClass = computed(() => (props.iconOnly && props.size !== 'compact' ? 'size-6' : 'size-4'))
</script>
