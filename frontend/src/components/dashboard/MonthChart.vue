<template>
  <div class="h-64 w-full">
    <Bar :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup lang="ts">
import type { ChartOptions } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'

import { useCssColor } from '@/composables/useCssColor'
import { formatAmount, formatMonthLabel } from '@/composables/useFormat'
import type { MonthTotal } from '@/types'

const props = defineProps<{
  items: MonthTotal[]
}>()

const primaryColor = useCssColor('--color-primary')

const chartData = computed(() => ({
  labels: props.items.map((item) => formatMonthLabel(item.month)),
  datasets: [
    {
      label: 'Total',
      data: props.items.map((item) => item.total),
      backgroundColor: primaryColor.value || '#4f46e5',
      borderRadius: 4,
      maxBarThickness: 40,
    },
  ],
}))

const chartOptions: ChartOptions<'bar'> = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (context) => formatAmount(Number(context.raw)),
      },
    },
  },
  scales: {
    y: { beginAtZero: true },
  },
}
</script>
