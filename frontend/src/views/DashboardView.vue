<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <h1 class="text-2xl font-bold tracking-tight text-ink">Resumen</h1>
      <DateRangeSelector v-model="preset" />
    </div>

    <ErrorState v-if="dashboardStore.error" :message="dashboardStore.error.detail" @retry="load" />

    <div v-else-if="dashboardStore.isLoading && !dashboardStore.summary" class="space-y-6" role="status">
      <span class="sr-only">Cargando…</span>
      <Skeleton class="h-32" />
      <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Skeleton class="h-64" />
        <Skeleton class="h-64" />
      </div>
    </div>

    <EmptyState
      v-else-if="isEmpty"
      title="Todavía no hay datos para mostrar"
      description="Registra tu primer gasto para ver el resumen."
      action-label="Añadir gasto"
      @action="router.push({ name: 'expenses' })"
    />

    <template v-else-if="dashboardStore.summary">
      <TotalCard :total="dashboardStore.summary.total" />

      <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <BaseCard>
          <template #header>
            <h2 class="text-lg font-semibold text-ink">Por categoría</h2>
          </template>
          <CategoryChart v-if="dashboardStore.summary.byCategory.length > 0" :items="dashboardStore.summary.byCategory" />
          <p v-else class="py-10 text-center text-sm text-ink-muted">Sin gastos en este periodo.</p>
        </BaseCard>

        <BaseCard>
          <template #header>
            <h2 class="text-lg font-semibold text-ink">Por mes</h2>
          </template>
          <MonthChart v-if="dashboardStore.summary.byMonth.length > 0" :items="dashboardStore.summary.byMonth" />
          <p v-else class="py-10 text-center text-sm text-ink-muted">Sin gastos en este periodo.</p>
        </BaseCard>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'

import BaseCard from '@/components/base/BaseCard.vue'
import EmptyState from '@/components/base/EmptyState.vue'
import ErrorState from '@/components/base/ErrorState.vue'
import Skeleton from '@/components/base/Skeleton.vue'
import CategoryChart from '@/components/dashboard/CategoryChart.vue'
import DateRangeSelector from '@/components/dashboard/DateRangeSelector.vue'
import MonthChart from '@/components/dashboard/MonthChart.vue'
import TotalCard from '@/components/dashboard/TotalCard.vue'
import { getPresetRange, type DateRangePreset } from '@/composables/useDateRangePresets'
import { useDashboardStore } from '@/stores/dashboard'

const router = useRouter()
const dashboardStore = useDashboardStore()
const preset = ref<DateRangePreset>('all')

const isEmpty = computed(
  () => preset.value === 'all' && dashboardStore.summary?.total === 0,
)

function load(): void {
  dashboardStore.setFilters(getPresetRange(preset.value))
  dashboardStore.fetchSummary()
}

watch(preset, load)
onMounted(load)
</script>
