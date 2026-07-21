<template>
  <div>
    <div class="flex items-center justify-between gap-2">
      <CategoryBadge :name="budget.categoryName" :color="budget.color" size="sm" />
      <span class="text-sm tabular-nums">
        <span class="text-ink">{{ formatAmount(budget.spent) }}</span>
        <span class="text-ink-muted"> / {{ formatAmount(budget.amount) }}</span>
      </span>
    </div>

    <div
      class="mt-2 h-2.5 w-full overflow-hidden rounded-full"
      :class="budget.status === 'exceeded' ? 'bg-danger-soft' : 'bg-surface-sunken'"
      role="progressbar"
      :aria-valuenow="budget.percentage"
      aria-valuemin="0"
      aria-valuemax="100"
      :aria-label="`Presupuesto de ${budget.categoryName}`"
    >
      <div class="h-full rounded-full transition-[width] duration-300" :class="barColor" :style="{ width: barWidth }" />
    </div>

    <p class="mt-1 flex items-center gap-1 text-xs" :class="footerClass">
      <AppIcon v-if="footerIcon" :name="footerIcon" class="size-3.5" />
      {{ footerText }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

import AppIcon, { type IconName } from '@/components/base/AppIcon.vue'
import CategoryBadge from '@/components/base/CategoryBadge.vue'
import { formatAmount } from '@/composables/useFormat'
import type { Budget } from '@/types'

const props = defineProps<{
  budget: Budget
}>()

const barWidth = computed(() => `${Math.min(props.budget.percentage, 100)}%`)

const barColor = computed(() => {
  switch (props.budget.status) {
    case 'exceeded':
      return 'bg-danger'
    case 'warning':
      return 'bg-warning'
    default:
      return 'bg-success'
  }
})

const footerIcon = computed<IconName | null>(() => {
  switch (props.budget.status) {
    case 'exceeded':
      return 'alert-circle'
    case 'warning':
      return 'alert-triangle'
    default:
      return null
  }
})

const footerClass = computed(() => {
  switch (props.budget.status) {
    case 'exceeded':
      return 'text-danger font-medium'
    case 'warning':
      return 'text-warning font-medium'
    default:
      return 'text-ink-muted'
  }
})

const footerText = computed(() => {
  switch (props.budget.status) {
    case 'exceeded':
      return `Límite superado (${props.budget.percentage}%)`
    case 'warning':
      return `Te estás acercando al límite (${props.budget.percentage}%)`
    default:
      return `${props.budget.percentage}% usado`
  }
})
</script>
