<template>
  <Teleport to="body">
    <div class="fixed inset-x-4 top-4 z-50 flex flex-col items-center gap-2 sm:inset-x-auto sm:right-4 sm:items-end">
      <ToastItem
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        :toast="toast"
        @dismiss="toastStore.dismiss(toast.id)"
      />
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import ToastItem from '@/components/base/ToastItem.vue'
import { useToastStore } from '@/stores/toast'

const toastStore = useToastStore()
</script>
