<template>
  <svg
    :class="props.class"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    stroke-width="2"
    stroke-linecap="round"
    stroke-linejoin="round"
    aria-hidden="true"
  >
    <path v-for="(d, index) in paths" :key="index" :d="d" />
    <circle v-if="circleRadius" cx="12" cy="12" :r="circleRadius" />
  </svg>
</template>

<script setup lang="ts">
import { computed } from 'vue'

export type IconName =
  | 'wallet'
  | 'dashboard'
  | 'expenses'
  | 'categories'
  | 'logout'
  | 'close'
  | 'edit'
  | 'trash'
  | 'chevron-down'
  | 'alert-circle'
  | 'check-circle'
  | 'info-circle'
  | 'alert-triangle'
  | 'plus'
  | 'search-off'
  | 'check'
  | 'sun'
  | 'moon'

const props = defineProps<{
  name: IconName
  class?: string
}>()

const PATHS: Record<IconName, string[]> = {
  wallet: [
    'M3 7a2 2 0 0 1 2-2h11a2 2 0 0 1 2 2v1h-2a3 3 0 0 0 0 6h2v1a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7Z',
    'M16 11h3a1 1 0 0 1 1 1v0a1 1 0 0 1-1 1h-3a1 1 0 0 1 0-2Z',
  ],
  dashboard: ['M4 4h6v6H4V4Z', 'M14 4h6v6h-6V4Z', 'M4 14h6v6H4v-6Z', 'M14 14h6v6h-6v-6Z'],
  expenses: [
    'M6 3h9l3 3v15a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1Z',
    'M9 9h6',
    'M9 13h6',
    'M9 17h3',
  ],
  categories: [
    'm12 3 8 8-8 8-8-8 6-8h2Z',
    'M8.5 8.5h.01',
  ],
  logout: ['M15 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h9', 'M10 12h11', 'm17 8 4 4-4 4'],
  close: ['M6 6l12 12', 'M18 6 6 18'],
  edit: ['M4 20h4l11-11-4-4L4 16v4Z', 'M13.5 6.5l4 4'],
  trash: ['M5 7h14', 'M10 11v6', 'M14 11v6', 'M6 7l1 13a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1l1-13', 'M9 7V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v3'],
  'chevron-down': ['m6 9 6 6 6-6'],
  'alert-circle': ['M12 8v5', 'M12 16h.01'],
  'check-circle': ['m8.5 12 2.5 2.5 5-5'],
  'info-circle': ['M12 11v5', 'M12 8h.01'],
  'alert-triangle': [
    'm12 4 9 16H3L12 4Z',
    'M12 10v4',
    'M12 17h.01',
  ],
  plus: ['M12 5v14', 'M5 12h14'],
  'search-off': ['M10 4a6 6 0 1 0 0 12 6 6 0 0 0 0-12Z', 'm21 21-5.2-5.2'],
  check: ['m5 12 5 5 9-9'],
  sun: [
    'M12 1v2',
    'M12 21v2',
    'M4.22 4.22l1.42 1.42',
    'M18.36 18.36l1.42 1.42',
    'M1 12h2',
    'M21 12h2',
    'M4.22 19.78l1.42-1.42',
    'M18.36 5.64l1.42-1.42',
  ],
  moon: ['M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79Z'],
}

const CIRCLE_RADIUS: Partial<Record<IconName, number>> = {
  'alert-circle': 9,
  'info-circle': 9,
  'check-circle': 9,
  sun: 5,
}

const paths = computed(() => PATHS[props.name])
const circleRadius = computed(() => CIRCLE_RADIUS[props.name])
</script>
