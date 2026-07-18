<template>
  <div class="animate-pulse rounded-md bg-surface-sunken motion-reduce:animate-none" :class="props.class" />
</template>

<script setup lang="ts">
const props = defineProps<{
  class?: string
}>()
</script>
