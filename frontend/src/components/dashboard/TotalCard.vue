<template>
  <div class="rounded-xl border border-primary/20 bg-primary-soft p-4 shadow-card sm:p-6">
    <p class="text-xs font-semibold uppercase tracking-wide text-ink-muted">Total del periodo</p>
    <p class="text-4xl font-extrabold tracking-tight text-ink tabular-nums sm:text-5xl">
      {{ formatAmount(total) }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { formatAmount } from '@/composables/useFormat'

defineProps<{
  total: number
}>()
</script>
