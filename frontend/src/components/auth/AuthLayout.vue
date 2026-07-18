<template>
  <div class="flex min-h-dvh items-center justify-center bg-surface p-4">
    <div class="w-full max-w-sm rounded-xl border border-line bg-surface-raised p-6 shadow-card sm:p-8">
      <div class="mb-6 flex items-center justify-center gap-2">
        <AppIcon name="wallet" class="size-7 text-primary" />
        <span class="text-xl font-bold text-ink">Gestor de Gastos</span>
      </div>
      <h2 class="mb-4 text-center text-lg font-semibold text-ink">{{ title }}</h2>
      <p
        v-if="errorMessage"
        class="mb-4 rounded-lg border border-danger/30 bg-danger-soft p-3 text-sm text-danger"
        role="alert"
      >
        {{ errorMessage }}
      </p>
      <slot />
      <p class="mt-6 text-center text-sm text-ink-muted">
        <slot name="footer" />
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppIcon from '@/components/base/AppIcon.vue'

defineProps<{
  title: string
  errorMessage?: string | null
}>()
</script>
