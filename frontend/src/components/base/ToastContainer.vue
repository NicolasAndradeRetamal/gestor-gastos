<template>
  <Teleport to="body">
    <TransitionGroup
      tag="div"
      role="region"
      aria-label="Notificaciones"
      class="pointer-events-none fixed inset-x-4 top-4 z-50 flex flex-col items-center gap-2 sm:inset-x-auto sm:right-4 sm:items-end"
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0 -translate-y-2 sm:translate-y-0 sm:translate-x-4 motion-reduce:transform-none"
      leave-active-class="transition duration-150 ease-in"
      leave-to-class="opacity-0 -translate-y-2 sm:translate-y-0 sm:translate-x-4 motion-reduce:transform-none"
      move-class="transition duration-200 ease-out"
    >
      <ToastItem
        v-for="toast in ordered"
        :key="toast.id"
        :toast="toast"
        @dismiss="toastStore.dismiss(toast.id)"
      />
    </TransitionGroup>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from 'vue'

import ToastItem from '@/components/base/ToastItem.vue'
import { useToastStore } from '@/stores/toast'

const toastStore = useToastStore()

// Newest on top.
const ordered = computed(() => [...toastStore.toasts].reverse())
</script>
