<template>
  <span v-if="size === 'sm'" class="inline-flex items-center gap-1.5">
    <span class="size-2.5 shrink-0 rounded-full" :style="{ backgroundColor: color }" aria-hidden="true" />
    <span class="text-ink">{{ name }}</span>
  </span>
  <span
    v-else
    class="inline-flex h-7 items-center gap-1.5 rounded-full pl-1.5 pr-2.5 text-xs font-medium"
    :style="{ backgroundColor: softBackground, color }"
  >
    <span class="size-2 shrink-0 rounded-full" :style="{ backgroundColor: color }" aria-hidden="true" />
    {{ name }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  name: string
  color: string
  size?: 'md' | 'sm'
}>()

const softBackground = computed(() => `color-mix(in srgb, ${props.color} 15%, transparent)`)
const size = computed(() => props.size ?? 'md')
const color = computed(() => props.color)
const name = computed(() => props.name)
</script>
