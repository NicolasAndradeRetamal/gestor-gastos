<template>
  <span class="inline-flex items-center gap-2" role="status">
    <svg
      class="animate-spin"
      :class="props.class ?? 'size-4'"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2.5"
    >
      <circle class="opacity-25" cx="12" cy="12" r="10" />
      <path class="opacity-100" d="M22 12a10 10 0 0 0-10-10" />
    </svg>
    <span class="sr-only">{{ props.label ?? 'Cargando…' }}</span>
  </span>
</template>

<script setup lang="ts">
const props = defineProps<{
  class?: string
  label?: string
}>()
</script>
