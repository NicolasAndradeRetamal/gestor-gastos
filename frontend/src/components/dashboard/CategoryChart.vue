<template>
  <div class="flex flex-col items-center gap-6 md:flex-row">
    <div class="h-64 w-full shrink-0 md:w-64">
      <Doughnut :data="chartData" :options="chartOptions" />
    </div>
    <ul class="w-full flex-1 space-y-2">
      <li v-for="item in items" :key="item.categoryId" class="flex items-center justify-between gap-3">
        <CategoryBadge :name="item.categoryName" :color="item.color" />
        <span class="text-sm font-semibold text-ink tabular-nums">{{ formatAmount(item.total) }}</span>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import type { ChartOptions } from 'chart.js'
import { computed } from 'vue'
import { Doughnut } from 'vue-chartjs'

import CategoryBadge from '@/components/base/CategoryBadge.vue'
import { formatAmount } from '@/composables/useFormat'
import type { CategoryTotal } from '@/types'

const props = defineProps<{
  items: CategoryTotal[]
}>()

const chartData = computed(() => ({
  labels: props.items.map((item) => item.categoryName),
  datasets: [
    {
      data: props.items.map((item) => item.total),
      backgroundColor: props.items.map((item) => item.color),
      borderWidth: 0,
    },
  ],
}))

const chartOptions: ChartOptions<'doughnut'> = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (context) => `${context.label}: ${formatAmount(Number(context.raw))}`,
      },
    },
  },
}
</script>
