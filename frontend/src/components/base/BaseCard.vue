<template>
  <div class="rounded-xl border border-line bg-surface-raised p-4 shadow-card sm:p-6" :class="props.class">
    <div v-if="$slots.header" class="mb-4 flex items-center justify-between gap-2">
      <slot name="header" />
    </div>
    <slot />
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  class?: string
}>()
</script>
