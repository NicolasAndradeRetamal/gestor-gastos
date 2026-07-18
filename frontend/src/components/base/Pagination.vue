<template>
  <div class="mt-4 flex items-center justify-between text-sm text-ink-muted">
    <p>Mostrando {{ from }}–{{ to }} de {{ totalItems }}</p>
    <div class="flex items-center gap-2">
      <BaseButton variant="secondary" size="compact" :disabled="page <= 1" @click="emit('update:page', page - 1)">
        Anterior
      </BaseButton>
      <BaseButton
        variant="secondary"
        size="compact"
        :disabled="page >= totalPages"
        @click="emit('update:page', page + 1)"
      >
        Siguiente
      </BaseButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

import BaseButton from '@/components/base/BaseButton.vue'

const props = defineProps<{
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}>()

const emit = defineEmits<{
  'update:page': [page: number]
}>()

const from = computed(() => (props.totalItems === 0 ? 0 : (props.page - 1) * props.pageSize + 1))
const to = computed(() => Math.min(props.page * props.pageSize, props.totalItems))
</script>
